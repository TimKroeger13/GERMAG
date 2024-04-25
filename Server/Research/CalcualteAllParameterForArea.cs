using GERMAG.DataModel.Database;
using GERMAG.Server.ReportCreation;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Union;
using System.Linq;
using NetTopologySuite.IO;
using Microsoft.EntityFrameworkCore;
using GERMAG.Shared;
using GERMAG.Server.GeometryCalculations;
using System.Net;
using System.Text.RegularExpressions;
using GERMAG.Server.DataPulling;
using System.Collections.Generic;
using GeoAPI.Geometries;
using Microsoft.AspNetCore.Http.Features;
using NetTopologySuite.Features;

namespace GERMAG.Server.Research;

public interface ICalcualteAllParameterForArea
{
    Task<string> calucalteAllParameters();
}

public class CalcualteAllParameterForArea(DataContext context, IRestrictionFromLandParcel restrictionFromLandParcel, IFindLocalDirectoryPath findLocalDirectoryPath) : ICalcualteAllParameterForArea
{
    public async Task<String> calucalteAllParameters()
    {
        //get geodata
        context.Database.SetCommandTimeout(TimeSpan.FromMinutes(60));


        var selectedData = context.AxSelecteds.ToList();
        var ax_tree = context.AxTrees.ToList();
        var ax_buildings = context.AxBuildings.ToList();

        GeometryFactory geometryFactory = new GeometryFactory();

        List<NetTopologySuite.Geometries.Geometry?> bufferedTrees = ax_tree.Select(tree => tree.Geom?.Buffer(OfficalParameters.TreeDistance)).ToList();

        List<Polygon?> landParcelPolygon = selectedData.Select(selected => (Polygon?)selected.Geom).ToList();

        List<NetTopologySuite.Geometries.Geometry?> landParcelPolygonGeometry= selectedData.Select(selected => selected.Geom).ToList();

        List<LineString?> landParcelLineString = landParcelPolygon.Select(land => land?.ExteriorRing).ToList();

        List<NetTopologySuite.Geometries.Geometry?> bufferedLandParcel = landParcelLineString.Select(landlinestring => landlinestring?.Buffer(OfficalParameters.LandParcelDistance)).ToList();

        List<NetTopologySuite.Geometries.Geometry?> bufferedBuldings = ax_buildings.Select(build => build.Geom?.Buffer(OfficalParameters.BuildingDistance)).ToList();

        List<NetTopologySuite.Geometries.Geometry?> diffTree = calcualteDifference(landParcelPolygonGeometry, bufferedTrees);

        List<NetTopologySuite.Geometries.Geometry?> diffTreeParcel = calcualteDifference(diffTree, bufferedLandParcel);

        List<NetTopologySuite.Geometries.Geometry?> diffTreeParcelBuilding = calcualteDifference(diffTreeParcel, bufferedBuldings);


        

        var path = findLocalDirectoryPath.getLocalPath("CalculationResults", "berlinRestrictionFlaechen.geojson");



        var featureCollection = new NetTopologySuite.Features.FeatureCollection();

        var k = 0;
        foreach (var geometry in diffTreeParcelBuilding)
        {
            k++;
            Console.WriteLine(k + " / " + (diffTreeParcelBuilding.Count() - 1));
            if (geometry != null && (geometry is Polygon || geometry is MultiPolygon))
            {
                var properties = new AttributesTable();
                properties.Add("SRID", 28533);

                var feature = new NetTopologySuite.Features.Feature(geometry, properties);
                featureCollection.Add(feature);
            }
        }

        var serializer = GeoJsonSerializer.Create();
        using (var stringWriter = new StringWriter())
        {
            serializer.Serialize(stringWriter, featureCollection);
            var geoJsonString = stringWriter.ToString();

            File.WriteAllText(path, geoJsonString);
        }



 /*
                var geometryList = new List<NetTopologySuite.Geometries.Geometry?>();

                var k = 0;
                foreach (var geometry in diffTreeParcelBuilding)
                {
                    k++;
                    Console.WriteLine(k + " / " + (diffTreeParcelBuilding.Count() - 1));
                    if (geometry != null && (geometry is Polygon || geometry is MultiPolygon))
                    {
                        geometryList.Add(geometry as NetTopologySuite.Geometries.Geometry);
                    }
                }



                var geometryCollection = new GeometryCollection(geometryList.ToArray());

                geometryCollection.SRID = 28533;

                var serializer = GeoJsonSerializer.Create();
                using (var stringWriter = new StringWriter())
                {
                    serializer.Serialize(stringWriter, geometryCollection);
                    var geoJsonString = stringWriter.ToString();
                    File.WriteAllText(path, geoJsonString);
                }*/




        

        Console.WriteLine("All restricions for Berlin calculated!");



        context.Database.SetCommandTimeout(null);
        return "Test from Server";
    }

    private List<NetTopologySuite.Geometries.Geometry?> calcualteDifference(List<NetTopologySuite.Geometries.Geometry?> sourcePolygon, List<NetTopologySuite.Geometries.Geometry?> differenceList)
    {
        GeometryFactory geometryFactory = new GeometryFactory();

        for (var i = 0; i < sourcePolygon.Count; i++)
        {
            Console.WriteLine(i + " / " + (sourcePolygon.Count() - 1));

            var single = sourcePolygon[i];

            if (single == null) {
                Console.WriteLine("Empty Element");
                continue; }

            List<NetTopologySuite.Geometries.Geometry?> intersectingGeometry = differenceList.Where(e => e!.Intersects(single)).ToList();

            if (intersectingGeometry.Count == 0) { continue; }

            NetTopologySuite.Geometries.Geometry intersectingGeometryUnion = geometryFactory.BuildGeometry(intersectingGeometry).Union();

            sourcePolygon[i] = single.Difference(intersectingGeometryUnion);

        }

        return sourcePolygon;

    }



}
