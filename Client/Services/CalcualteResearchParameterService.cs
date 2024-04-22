using GERMAG.Shared.Core;
using GERMAG.Client.Core;
using GERMAG.Client.Pages;

namespace GERMAG.Client.Services;

public interface ICalcualteResearchParameterService
{
    Task<string> calcualteResearch();
}

public class CalcualteResearchParameterService(IRestInteropFactory factory) : ICalcualteResearchParameterService
{
    private readonly IRestInterop _restInterop = factory.CreateRestInterop("api/Research");
    public async Task<string> calcualteResearch()
    {
        var a = await _restInterop.GetAsync("research") ?? "Server not reached";
        return await _restInterop.GetAsync("research") ?? "Server not reached";
    }
}
