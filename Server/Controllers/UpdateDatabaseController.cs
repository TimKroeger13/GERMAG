using Microsoft.AspNetCore.Mvc;
using GERMAG.Shared;
using GERMAG.DataModel.Database;
using GERMAG.DataModel;
using GERMAG.Server.DataPulling;
namespace GERMAG.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UpdateDatabaseController(IDataFetcher fetchData) : Controller
{
    [HttpGet("greet")]
    public string GreetFunction()
    {
        return "Database Update command was run successfully! But the function is not fully impelmented yet.";
    }

    [HttpGet("checkForUpdates")]
    public async Task UpdateDatabase()
    {
        await fetchData.FetchAllData();
    }
}
