using Microsoft.AspNetCore.Mvc;
using GERMAG.Shared;
using Microsoft.AspNetCore.Cors;
using static System.Net.Mime.MediaTypeNames;

namespace GERMAG.Server.Controllers;

[ApiController]
[Route("api/[controller]")]

public class ReportController(ILogger<ReportController> logger) : ControllerBase
{
    private readonly ILogger<ReportController> _logger = logger;

    [HttpGet("reportdata")]
    [EnableCors(CorsPolicies.GetAllowed)]
    public IEnumerable<Report> Get()
    {
        return new[] { new Report
        {
            Test = "testStringReturn"
        }};

    }
}