namespace GERMAG.Server.DataPulling;

public interface IDataFetcher
{
    void fetchAllData();
}

public class DataFetcher(IDatabaseUpdater databaseUpdater) : IDataFetcher
{
    private readonly IDatabaseUpdater _databaseUpdater = databaseUpdater;

    public void fetchAllData()
    {
        // <- Add reference Database connection here
        // <- Feed all get request in noted in the database by a loop
        // for (int i = 0; i < x; i++) {}

        // Example URL
        var currentURL = "https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot2400_100?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot2400_100";

        var connectionFeedback = _databaseUpdater.updateDatabase(currentURL, "Entzugsleistung_100M_2400HP");

        Console.WriteLine(connectionFeedback);


    }
}
