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
using System.Text.Json;
using GERMAG.Shared.PointProperties;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace GERMAG.Server.Research;

public interface ICalcualteAllParameterForArea
{
    Task<string> calucalteAllParameters();
}

public class CalcualteAllParameterForArea(DataContext context, IFindLocalDirectoryPath findLocalDirectoryPath, IGeoThermalProbesCalcualtion geoThermalProbesCalcualtion, IGetProbeSpecificData getProbeSpecificData) : ICalcualteAllParameterForArea
{
    public async Task<String> calucalteAllParameters()
    {
        //Calcualte RestrictionArea

        Console.WriteLine("Starting scientific calcualtion");

        var inputPath = findLocalDirectoryPath.getLocalPath("CalculationResults", "berlinRestrictionFlaechen.geojson");

        NetTopologySuite.Features.FeatureCollection featureCollection;

        if (File.Exists(inputPath))
        {
            string jsonString = File.ReadAllText(inputPath);

            var reader = new GeoJsonReader();
            featureCollection = reader.Read<NetTopologySuite.Features.FeatureCollection>(jsonString);

            foreach (var elementGeometry in featureCollection)
            {
                elementGeometry.Geometry.SRID = 25833;
            }
        }
        else
        {
            //get geodata
            context.Database.SetCommandTimeout(TimeSpan.FromMinutes(60));

            var selectedData = context.AxSelecteds.ToList();
            var ax_tree = context.AxTrees.ToList();
            var ax_buildings = context.AxBuildings.ToList();

            GeometryFactory geometryFactory = new GeometryFactory();

            List<NetTopologySuite.Geometries.Geometry?> bufferedTrees = ax_tree.Select(tree => tree.Geom?.Buffer(OfficalParameters.TreeDistance)).ToList();

            List<Polygon?> landParcelPolygon = selectedData.Select(selected => (Polygon?)selected.Geom).ToList();

            List<NetTopologySuite.Geometries.Geometry?> landParcelPolygonGeometry = selectedData.Select(selected => selected.Geom).ToList();

            List<LineString?> landParcelLineString = landParcelPolygon.Select(land => land?.ExteriorRing).ToList();

            List<NetTopologySuite.Geometries.Geometry?> bufferedLandParcel = landParcelLineString.Select(landlinestring => landlinestring?.Buffer(OfficalParameters.LandParcelDistance)).ToList();

            List<NetTopologySuite.Geometries.Geometry?> bufferedBuldings = ax_buildings.Select(build => build.Geom?.Buffer(OfficalParameters.BuildingDistance)).ToList();

            List<NetTopologySuite.Geometries.Geometry?> diffTree = calcualteDifference(landParcelPolygonGeometry, bufferedTrees);

            List<NetTopologySuite.Geometries.Geometry?> diffTreeParcel = calcualteDifference(diffTree, bufferedLandParcel);

            List<NetTopologySuite.Geometries.Geometry?> diffTreeParcelBuilding = calcualteDifference(diffTreeParcel, bufferedBuldings);


            featureCollection = new NetTopologySuite.Features.FeatureCollection();

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

            var path = findLocalDirectoryPath.getLocalPath("CalculationResults", "berlinRestrictionFlaechen.geojson");

            var serializer = GeoJsonSerializer.Create();
            using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, featureCollection);
                var geoJsonString = stringWriter.ToString();

                File.WriteAllText(path, geoJsonString);
            }
            context.Database.SetCommandTimeout(null);
        }

        //Calcualte points in featureCollection


        var landParcelID2 = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.land_parcels).Id;

        var landParcelElement2 = new LandParcel
        {
            ParameterKey = landParcelID2,
        };




        List<NetTopologySuite.Geometries.Geometry?> ProbeGeometricPosistions;

        DateTime start = DateTime.Now;
        {
            ProbeGeometricPosistions = await CalcualteProbePositionAsync(featureCollection, landParcelElement2);
        }
        TimeSpan timeItTook = DateTime.Now - start;
        Console.WriteLine(timeItTook);

        var landParcelID = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.land_parcels).Id;

        var landParcelElement = new LandParcel
        {
            ParameterKey = landParcelID,
        };

        List<ProbePoint?> probePointsList = ProbeGeometricPosistions.Select(geometry => new ProbePoint
        {
            Geometry = geometry,
        }).ToList()!;

        var y = await getProbeSpecificData.GetPointProbeData(landParcelElement, probePointsList);


        var u = 0;

/*
d

            List<ProbePoint?> DataFilledProbePoints = await getProbeSpecificData.GetPointProbeData(landParcelElement, FullPointProbe);

            MaxDepthList.AddRange(DataFilledProbePoints.Select(dfpp => dfpp?.Properties?.MaxDepth).ToList());
            GeoPotenDepthList.AddRange(DataFilledProbePoints.Select(dfpp => dfpp?.Properties?.GeoPotenDepth).ToList());
            GeoPotenList.AddRange(DataFilledProbePoints.Select(dfpp => dfpp?.Properties?.GeoPoten).ToList());

        }













        var savePath = findLocalDirectoryPath.getLocalPath("CalculationResults", "MaxDepth.geojson");
        File.WriteAllText(savePath, JsonConvert.SerializeObject(MaxDepthList));

        savePath = findLocalDirectoryPath.getLocalPath("CalculationResults", "GeoPotenDepth.geojson");
        File.WriteAllText(savePath, JsonConvert.SerializeObject(GeoPotenDepthList));

        savePath = findLocalDirectoryPath.getLocalPath("CalculationResults", "GeoPoten.geojson");
        File.WriteAllText(savePath, JsonConvert.SerializeObject(GeoPotenList));*/

        Console.WriteLine("All restricions for Berlin calculated!");

        return "Test from Server";
    }

/*    private async Task<List<ProbePoint?>> CalcualteProbeValuesAsync(List<NetTopologySuite.Geometries.Geometry> geometryList)
    {
        var landParcelID = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.land_parcels).Id;

        var landParcelElement = new LandParcel
        {
            ParameterKey = landParcelID,
        };

        List<Task<List<ProbePoint?>>> tasks = new List<Task<List<ProbePoint?>>>();

        var batchsize = 100;

        var itterations = Math.Ceiling((double)geometryList.Count() / batchsize);

        for(int i = 0; i< itterations; i++)
        {
            Console.WriteLine("Fetchign Probevalues " + i + " / " + (itterations-1));

            var geometryBatch = geometryList.Skip(i * batchsize).Take(batchsize);

             List<ProbePoint?> probePointsList = geometryBatch.Select(geometry => new ProbePoint
            {
                Geometry = geometry,
            }).ToList()!;

            tasks.Add(getProbeSpecificData.GetPointProbeData(landParcelElement, probePointsList));
        }

        List<ProbePoint?>[] results = await Task.WhenAll(tasks);

        var a = new List<ProbePoint?>();

        return a;

    }*/

    private async Task<List<NetTopologySuite.Geometries.Geometry?>> CalcualteProbePositionAsync(NetTopologySuite.Features.FeatureCollection featureCollection, LandParcel landParcelElement)
    {
        List<Task<List<ProbePoint?>>> tasks = new List<Task<List<ProbePoint?>>>();

        var u = 0;

        foreach (var FeatureElement in featureCollection)
        {
            u++;
            Console.WriteLine(u);
            if (u >= 4)
            {
                break;
            }

            var featureGeometry = (NetTopologySuite.Geometries.Geometry?)FeatureElement.Geometry;

            var featureResrictionArea = new Restricion
            {
                Geometry_Usable = featureGeometry
            };

            tasks.Add(geoThermalProbesCalcualtion.CalculateGeoThermalProbes(featureResrictionArea));
        }

        List<ProbePoint?>[] results = await Task.WhenAll(tasks);

        List<NetTopologySuite.Geometries.Geometry?> outputList = new List<NetTopologySuite.Geometries.Geometry?>();

        foreach (var result in results)
        {
            outputList.AddRange(result.Select(r => r.Geometry));
        }

        return outputList;

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






/*List<ProbePoint?> FullPointProbe;

List<NetTopologySuite.Geometries.Polygon?> listofAllRestrictionPolygons = new List<NetTopologySuite.Geometries.Polygon?>();

        foreach (var FeatureElement in featureCollection)
        {
            var featureGeometry = FeatureElement.Geometry;
            
            if (featureGeometry is Polygon)
            {
                listofAllRestrictionPolygons?.Add((Polygon?) featureGeometry);
            }
            else if (featureGeometry is MultiPolygon)
{
    MultiPolygon? unionPolygon = (MultiPolygon?)featureGeometry;

    foreach (var singlepolygon in unionPolygon!)
    {
        listofAllRestrictionPolygons?.Add((Polygon?)singlepolygon);
    }
}
        }

        var b = 3;
*/
