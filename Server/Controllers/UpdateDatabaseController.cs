using GERMAG.DataModel.Database;
using GERMAG.Server.DataPulling;
using Microsoft.AspNetCore.Mvc;

namespace GERMAG.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UpdateDatabaseController(IDataFetcher fetchData, DataContext context) : Controller
{
    [HttpGet("greet")]
    public string greetFunction()
    {
        return "Database Update command was run successfully! But the function is not fully impelmented yet.";
    }

    [HttpGet("checkForUpdates")]
    public async Task UpdateDatabase()
    {
        var test = context.GeothermalParameter.First();
        await fetchData.FetchAllData();
    }
}