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

public class DataFetcher(DataContext context, IDatabaseUpdater databaseUpdater, HttpClient client, IJsonDeserialize jsonDeserializeSwitch) : IDataFetcher
{
    public async Task FetchAllData()
    {
        var allGeothermalParameters = context.GeothermalParameter.OrderBy(gp => gp.Id).ToList();

        for (int i = 0; i < allGeothermalParameters.Count; i++)
        {
            //i = 10;

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
