using Microsoft.AspNetCore.Mvc;
using GERMAG.Shared;
using Microsoft.AspNetCore.Cors;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;
using GERMAG.Server.ReportCreation;

namespace GERMAG.Server.Controllers;

[ApiController]
[Route("api/[controller]")]

public class ReportController(ICreateReportAsync createReport) : ControllerBase
{
    [HttpGet("reportdata")]
    [EnableCors(CorsPolicies.GetAllowed)]
    public async Task<IEnumerable<Report>> GetReport()
    {
        return await createReport.CreateGeothermalReportAsync();
    }
}

/*public IEnumerable<Report> Get()
{
    return new[] { new Report
    {
        Test = "Hier könnten ihre geothermischen Daten stehen!"
    }};

}*/