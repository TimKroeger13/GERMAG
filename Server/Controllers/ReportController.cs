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
    public async Task<IEnumerable<Report>> GetReport(double Xcor, double Ycor, int Srid)
    {
        return await createReport.CreateGeothermalReportAsync(Xcor, Ycor, Srid);
    }
}

public class ReportPDFController(ICreateReportAsync createReport) : ControllerBase
{
    [HttpGet("reportPDF")]
    [EnableCors(CorsPolicies.GetAllowed)]
    public async Task<IEnumerable<Report>> GetReport(double Xcor, double Ycor, int Srid)
    {
        return await createReport.CreateGeothermalReportAsync(Xcor, Ycor, Srid);

    }
}