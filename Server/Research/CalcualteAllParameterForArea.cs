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

        List<LineString?> landParcelLineString = landParcelPolygon.Select(land => land?.ExteriorRing).ToList();

        List<NetTopologySuite.Geometries.Geometry?> bufferedLandParcel = landParcelLineString.Select(landlinestring => landlinestring?.Buffer(OfficalParameters.LandParcelDistance)).ToList();

        List<NetTopologySuite.Geometries.Geometry?> bufferedBuldings = ax_buildings.Select(build => build.Geom?.Buffer(OfficalParameters.BuildingDistance)).ToList();

        var i = 0;


        foreach (var single in landParcelPolygon)
        {
            List<NetTopologySuite.Geometries.Geometry?> intersectingTrees = bufferedTrees.Where(bt => bt!.Intersects(single)).ToList();

            single.Difference(intersectingTrees.Select(x => x).ToList());


                        i++;
            Console.WriteLine(i + " / " + bufferedTrees.Count());
        }

        var b = 3;



        //List<NetTopologySuite.Geometries.Geometry?>


        //var geoJsonWriter = new GeoJsonWriter();
        //var path = findLocalDirectoryPath.getLocalPath("CalculationResults", "berlinWohnflaeche.geojson");
        //File.WriteAllText(path, geoJsonWriter.Write(UsableArea));


        /*NetTopologySuite.Geometries.Geometry? bufferedTrees = ax_tree!.Buffer(OfficalParameters.TreeDistance);

        Polygon? landParcelPolygon = (Polygon?)researchData!.Geom;
        LineString? landParcelLineString = landParcelPolygon?.ExteriorRing;

        NetTopologySuite.Geometries.Geometry? bufferedLandParcel = landParcelLineString?.Buffer(OfficalParameters.LandParcelDistance);

        NetTopologySuite.Geometries.Geometry? bufferedBuldings = ax_buildings?.Geom!.Buffer(OfficalParameters.BuildingDistance);



        NetTopologySuite.Geometries.Geometry? UsableArea = landParcelPolygon?.Difference(bufferedLandParcel).Difference(bufferedBuldings).Difference(bufferedTrees);
        UsableArea = UsableArea?.Union();

        NetTopologySuite.Geometries.Geometry? RestictionArea = bufferedLandParcel?.Union(bufferedBuldings).Union(bufferedTrees);
        RestictionArea = landParcelPolygon?.Intersection(RestictionArea);
        RestictionArea = RestictionArea?.Union();

        var geoJsonWriter = new GeoJsonWriter();

        var returnValue = new Restricion
        {
            Geometry_Usable = UsableArea,
            Geometry_Restiction = RestictionArea,
            Geometry_Usable_geoJson = geoJsonWriter.Write(UsableArea),
            Geometry_Restiction_geoJson = geoJsonWriter.Write(RestictionArea),
            Usable_Area = UsableArea?.Area ?? 0,
            Restiction_Area = RestictionArea?.Area ?? 0,
        };

        var path = findLocalDirectoryPath.getLocalPath("CalculationResults", "berlinWohnfläche.geojson");
        File.WriteAllText(path, returnValue.Geometry_Usable_geoJson);*/



        context.Database.SetCommandTimeout(null);
        return "Test from Server";
    }
}
