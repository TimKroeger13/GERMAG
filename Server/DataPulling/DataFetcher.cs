using GERMAG.DataModel.Database;
using GERMAG.Server.DataPulling.JsonDeserialize;
using System.Text.RegularExpressions;

namespace GERMAG.Server.DataPulling;

public interface IDataFetcher
{
    Task FetchAllData();
}

public class DataFetcher(DataContext context, IDatabaseUpdater databaseUpdater, HttpClient client, IJsonDeserialize jsonDeserialize) : IDataFetcher
{
    public async Task FetchAllData()
    {
        var allGeothermalParameters = context.GeothermalParameter.OrderBy(gp => gp.Id).ToList();

        for (int i = 0; i < allGeothermalParameters.Count; i++)
        {
            Console.WriteLine("Pining data: " + allGeothermalParameters[i].Type + " | " + allGeothermalParameters[i].Area);
            var getrequest = allGeothermalParameters[i].Getrequest;

            client.Timeout = TimeSpan.FromMinutes(10);
            string seriallizedInputJson = await client.GetStringAsync(getrequest);

            //Check if Data is up to date

            var dataHash = allGeothermalParameters[i].Hash ?? -1;
            var hash = HashString(seriallizedInputJson);

            context.GeothermalParameter.First(gp => gp.Id == allGeothermalParameters[i].Id).LastPing = DateTime.Now;
            context.SaveChanges();

            if (dataHash != hash)
            {
                //Differentiate between different coordiante formats

                bool isSingleCoordinateFormat = Regex.IsMatch(seriallizedInputJson, "coordinate[s]?\\\":\\[\\d");
                bool IsLongCoordianteFormat = Regex.IsMatch(seriallizedInputJson, "coordinate[s]?\\\":\\[\\[\\[\\[");
                bool IsShortCoordianteFormat = Regex.IsMatch(seriallizedInputJson, "coordinate[s]?\\\":\\[\\[\\d");

                var Format = JsonFormat.normal;

                if (isSingleCoordinateFormat)
                {
                    seriallizedInputJson = Regex.Replace(seriallizedInputJson, "\"coordinates\"", "\"coordinatesSingle\"");
                    seriallizedInputJson = Regex.Replace(seriallizedInputJson, "\"coordinate\"", "\"coordinatesSingle\"");
                    Format = JsonFormat.single_coordiantes;
                }
                if (IsLongCoordianteFormat)
                {
                    seriallizedInputJson = Regex.Replace(seriallizedInputJson, "\"coordinates\"", "\"coordinatesLong\"");
                    seriallizedInputJson = Regex.Replace(seriallizedInputJson, "\"coordinate\"", "\"coordinatesLong\"");
                    Format = JsonFormat.long_coordiantes;
                }
                if (IsShortCoordianteFormat)
                {
                    seriallizedInputJson = Regex.Replace(seriallizedInputJson, "\"coordinates\"", "\"coordinatesShort\"");
                    seriallizedInputJson = Regex.Replace(seriallizedInputJson, "\"coordinate\"", "\"coordinatesShort\"");
                    Format = JsonFormat.short_coordiantes;
                }

                //update Data when not up to date

                Console.WriteLine("DeserializationJson: " + allGeothermalParameters[i].Type + " | " + allGeothermalParameters[i].Area);
                var jsonData_Root = jsonDeserialize.DeserializeJson(seriallizedInputJson, allGeothermalParameters[i].Type, Format);

                Console.WriteLine("Insert Data into Database: " + allGeothermalParameters[i].Type + " | " + allGeothermalParameters[i].Area);
                databaseUpdater.UpdateDatabase(jsonData_Root, allGeothermalParameters[i].Id);

                context.GeothermalParameter.First(gp => gp.Id == allGeothermalParameters[i].Id).Hash = hash;
                context.SaveChanges();
            }
            else
            {
                if (i == allGeothermalParameters.Count - 1)
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