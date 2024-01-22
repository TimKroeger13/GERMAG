﻿using System.Xml.Linq;
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
using System.Text.RegularExpressions;
using NetTopologySuite.Geometries;

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
            Console.WriteLine("Pining data: " + allGeothermalParameters[i].Type + " | " + allGeothermalParameters[i].Area);
            var Getrequest = allGeothermalParameters[i].Getrequest;

            string SeriallizedInputJson = await client.GetStringAsync(Getrequest);

            //Check if Data is up to date

            var DataHash = allGeothermalParameters[i].Hash ?? -1;
            var hash = HashString(SeriallizedInputJson);

            context.GeothermalParameter.First(gp => gp.Id == allGeothermalParameters[i].Id).LastPing = DateTime.Now;
            context.SaveChanges();

            if (DataHash != hash)
            {
                //Differentiate between different coordiante formats

                bool isSingleCoordinateFormat = Regex.IsMatch(SeriallizedInputJson, "coordinate[s]?\\\":\\[\\d");
                bool IsLongCoordianteFormat = Regex.IsMatch(SeriallizedInputJson, "coordinate[s]?\\\":\\[\\[\\[\\[");
                bool IsShortCoordianteFormat = Regex.IsMatch(SeriallizedInputJson, "coordinate[s]?\\\":\\[\\[\\d");

                var Format = JsonFormat.normal;

                if (isSingleCoordinateFormat)
                {
                    SeriallizedInputJson = Regex.Replace(SeriallizedInputJson, "\"coordinates\"", "\"coordinatesSingle\"");
                    SeriallizedInputJson = Regex.Replace(SeriallizedInputJson, "\"coordinate\"", "\"coordinatesSingle\"");
                    Format = JsonFormat.single_coordiantes;
                }
                if (IsLongCoordianteFormat)
                {
                    SeriallizedInputJson = Regex.Replace(SeriallizedInputJson, "\"coordinates\"", "\"coordinatesLong\"");
                    SeriallizedInputJson = Regex.Replace(SeriallizedInputJson, "\"coordinate\"", "\"coordinatesLong\"");
                    Format = JsonFormat.long_coordiantes;
                }
                if (IsShortCoordianteFormat)
                {
                    SeriallizedInputJson = Regex.Replace(SeriallizedInputJson, "\"coordinates\"", "\"coordinatesShort\"");
                    SeriallizedInputJson = Regex.Replace(SeriallizedInputJson, "\"coordinate\"", "\"coordinatesShort\"");
                    Format = JsonFormat.short_coordiantes;
                }

                //update Data when not up to date

                Console.WriteLine("DeserializationJson: " + allGeothermalParameters[i].Type + " | " + allGeothermalParameters[i].Area);
                var jsonData_Root = jsonDeserializeSwitch.ChooseDeserializationJson(SeriallizedInputJson, allGeothermalParameters[i].Type, Format);

                Console.WriteLine("Insert Data into Database: " + allGeothermalParameters[i].Type + " | " + allGeothermalParameters[i].Area);
                databaseUpdater.UpdateDatabase(jsonData_Root, allGeothermalParameters[i].Id);

                context.GeothermalParameter.First(gp => gp.Id == allGeothermalParameters[i].Id).Hash = hash;
                context.SaveChanges();
            }
            else
            {
                if (i == allGeothermalParameters.Count-1)
                {
                    Console.WriteLine("Database Updated!");
                }
            }
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
