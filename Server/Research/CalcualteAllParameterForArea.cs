namespace GERMAG.Server.Research;

public interface ICalcualteAllParameterForArea
{
    Task<string> calucalteAllParameters();
}

public class CalcualteAllParameterForArea : ICalcualteAllParameterForArea
{
    public async Task<String> calucalteAllParameters()
    {
        return "Test from Server";
    }
}
