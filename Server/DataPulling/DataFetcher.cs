using System.Xml.Linq;
using System;
using GERMAG.DataModel.Database;
using System.Text.Json;
using GERMAG.Shared;
using System.Net;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace GERMAG.Server.DataPulling;

public interface IDataFetcher
{
    void fetchAllData();
}

public class Crs
{
    public string? type { get; set; }
    public Properties? properties { get; set; }
}

public class Feature
{
    public string? type { get; set; }
    public string? id { get; set; }
    public Geometry? geometry { get; set; }
    public Properties? properties { get; set; }
}

public class Geometry
{
    public string? type { get; set; }
    public List<List<List<double>>>? coordinates { get; set; }
}

public class Properties
{
    public string? gridcode { get; set; }
    public string? la_100txt { get; set; }
    public string? name { get; set; }
}

public class Root
{
    public string? type { get; set; }
    public List<double>? bbox { get; set; }
    public int totalFeatures { get; set; }
    public List<Feature>? features { get; set; }
    public Crs? crs { get; set; }
}

public class DataFetcher(IDatabaseUpdater databaseUpdater, DataContext context) : IDataFetcher
{
    private readonly IDatabaseUpdater _databaseUpdater = databaseUpdater;

    public async void fetchAllData()
    {
        // <- Add reference Database connection here
        // <- Feed all get request in noted in the database by a loop
        // for (int i = 0; i < x; i++) {}

        // Example URL
        var url = "https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot2400_100?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot2400_100&outputFormat=application/json";

        HttpClient client = new();
        string SeriallizedInputJson = await client.GetStringAsync(url);


        //context.GeothermalParameter.First().Srid = 9999;

        var hash = HashString(SeriallizedInputJson);


        //Check if data is up to date
        //Generate and Check for hash
        string name1 = "Theodore Onyejiaku";

        Root? jsonData_Root = JsonSerializer.Deserialize<Root>(SeriallizedInputJson);

        _databaseUpdater.updateDatabase(jsonData_Root);

    }

    private int HashString(string text)
    {
        unchecked
        {
            int hash = 23;
            foreach (char c in text)
            {
                hash = hash * 31 + c;
            }
            return hash;
        }
    }
}
