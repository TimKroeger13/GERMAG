namespace GERMAG.Server.DataPulling;

public interface IFetchAllData
{
    void FetchAllData();
}

public class FetchAllData_ : IFetchAllData
{
    private readonly IFetchAllData _fetchAllData;

    public FetchAllData_(IFetchAllData fetchAllData)
    {
        _fetchAllData = fetchAllData;
    }

    public void FetchAllData()
    {
        Console.WriteLine("Greeting form Fetchdata");
        _fetchAllData.FetchAllData();
    }
}
