using System.Xml.Linq;
using System;
using GERMAG.DataModel.Database;
using System.Text.Json;
using GERMAG.Shared;
using System.Net;
using Microsoft.EntityFrameworkCore.Storage.Json;
using GERMAG.Client.Services;
using GERMAG.DataModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using GERMAG.Server.DataPulling.JsonDeserialize;

namespace GERMAG.Server.DataPulling;

public interface IDataFetcher
{
    Task FetchAllData();
}
//#pragma warning disable IDE1006 // Naming Styles
/*
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
*/
//#pragma warning restore IDE1006 // Naming Styles

public class DataFetcher(DataContext context, IDatabaseUpdater databaseUpdater, HttpClient client, IJsonDeserializeSwitch jsonDeserializeSwitch) : IDataFetcher
{
    public async Task FetchAllData()
    {
        var allGeothermalParameters = context.GeothermalParameter.ToList();

        for (int i = 0; i < allGeothermalParameters.Count; i++)
        {
            var getrequest = allGeothermalParameters[i].Getrequest;

            string SeriallizedInputJson = await client.GetStringAsync(getrequest);

            //Check if Data is up to date

            var hash = HashString(SeriallizedInputJson);

            context.GeothermalParameter.First(gp => gp.Id == allGeothermalParameters[i].Id).LastPing = DateTime.Now;
            context.SaveChanges();

            //update Data when not up to date

            var jsonData_Root = jsonDeserializeSwitch.ChooseDeserializationJson(SeriallizedInputJson, allGeothermalParameters[i].Type);
            //Root? jsonData_Root = JsonSerializer.Deserialize<Root>(SeriallizedInputJson) ?? throw new Exception("No wfs found (root)");

            databaseUpdater.UpdateDatabase(jsonData_Root, allGeothermalParameters[i].Id);
        }
    }

    private static int HashString(string text)
    {
        unchecked
        {
            int hash = 23;
            foreach (char c in text)
            {
                hash = (hash * 31) + c;
            }
            return hash;
        }
    }
}
