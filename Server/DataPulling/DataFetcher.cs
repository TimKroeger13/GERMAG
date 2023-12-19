using System.Xml.Linq;
using System;
using GERMAG.DataModel.Database;

namespace GERMAG.Server.DataPulling;

public interface IDataFetcher
{
    void fetchAllData();
}

public class DataFetcher(IDatabaseUpdater databaseUpdater, DataContext context) : IDataFetcher
{
    private readonly IDatabaseUpdater _databaseUpdater = databaseUpdater;

    public void fetchAllData()
    {
        // <- Add reference Database connection here
        // <- Feed all get request in noted in the database by a loop
        // for (int i = 0; i < x; i++) {}

        // Example URL
        var url = "https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot2400_100?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot2400_100";
        

        try
        {
            XDocument xml = XDocument.Load(url);

            // <- Save downloaded Data in a Database here


            // For testing, before a DB is connected: Saving of the GML 
            var savePath = Directory.GetCurrentDirectory() + "..\\..\\Resources\\Test.gml";
            Directory.CreateDirectory(Path.GetDirectoryName(savePath) ?? "");

            if (savePath != null)
            {
                xml.Save(savePath);
            }
            else
            {
                // Do Something
            }


           
        }
        catch (Exception)
        {
        }

        _databaseUpdater.updateDatabase();

    }
}
