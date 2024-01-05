using GERMAG.DataModel.Database;
using System.Text.Json;

namespace GERMAG.Server.DataPulling;

public interface IDataFetcher
{
    Task FetchAllData();
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

public class DataFetcher(IDatabaseUpdater databaseUpdater, DataContext context, HttpClient client) : IDataFetcher
{
    public async Task FetchAllData()
    {
        var url = "https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot2400_100?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot2400_100&outputFormat=application/json";

        using var transaction = context.Database.BeginTransaction();
        string SeriallizedInputJson = await client.GetStringAsync(url);
        var test = context.GeothermalParameter.OrderBy(p => p.Id).Last();
        test.Id = 0;
        test.Srid = Random.Shared.Next(1, int.MaxValue);
        context.GeothermalParameter.Add(test);
        context.SaveChanges();
        var hash = HashString(SeriallizedInputJson);

        context.GeothermalParameter.First(a => a.Id == 1).Srid = 12345;
        context.SaveChanges();

        //Check if data is up to date
        //Generate and Check for hash

        Root? jsonData_Root = JsonSerializer.Deserialize<Root>(SeriallizedInputJson) ?? throw new Exception("No wfs found (root)");

        databaseUpdater.updateDatabase(jsonData_Root);
        transaction.Commit();
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