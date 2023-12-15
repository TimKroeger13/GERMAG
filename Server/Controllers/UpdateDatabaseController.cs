using Microsoft.AspNetCore.Mvc;
using GERMAG.Shared;
using GERMAG.DataModel.Database;
using GERMAG.DataModel;
namespace GERMAG.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UpdateDatabaseController : Controller
{
    [HttpGet("greet")]
    public string greetFunction()
    {
        return "Test string from controller";
    }
}
