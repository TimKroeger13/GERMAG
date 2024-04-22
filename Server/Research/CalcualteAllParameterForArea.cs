using GERMAG.DataModel.Database;
using GERMAG.Server.ReportCreation;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Union;
using System.Linq;
using NetTopologySuite.IO;

namespace GERMAG.Server.Research;

public interface ICalcualteAllParameterForArea
{
    Task<string> calucalteAllParameters();
}

public class CalcualteAllParameterForArea(DataContext context, IParameterDeserialator parameterDeserialator) : ICalcualteAllParameterForArea
{
    public async Task<String> calucalteAllParameters()
    {


        var usableAreaID = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.area_usage).Id;

        var AllUsableArea = context.GeoData.Where(gd => gd.ParameterKey == usableAreaID).ToList();

        var deserializedArea = AllUsableArea.Select(gd => new
        {
            geom = gd.Geom,
            paramer = parameterDeserialator.DeserializeParameters(gd.Parameter ?? "")
        }).ToList();

        var selectedUsableArea = deserializedArea.Where(de => de.paramer.Bezeich == "AX_Wohnbauflaeche")
            .Select(de => new
            {
                geom = de.geom
            }).ToList();

        NetTopologySuite.Geometries.Geometry unionGeometry = selectedUsableArea[0].geom!;

        var i = 1;
        foreach (var geometry in selectedUsableArea.Skip(1))
        {
            i++;
            Console.WriteLine(i + " | " + selectedUsableArea.Count);
            unionGeometry = unionGeometry!.Union(geometry.geom);
        }



        var geoJsonWriter = new GeoJsonWriter();

        string geoJson = geoJsonWriter.Write(unionGeometry);

        string downloadsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";

        string filePath = Path.Combine(downloadsDirectory, "Slectedusablearea.geojson");

        File.WriteAllText(filePath, geoJson);


        var b = 4;
        /*
        var x = selectedUsableArea
        .Skip(1)
        .Aggregate(selectedUsableArea[0].geom, (currentUnion, geometry) => currentUnion!.Union(geometry.geom));

        */


        return "Test from Server";
    }
}
