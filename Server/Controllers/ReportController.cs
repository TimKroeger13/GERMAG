using Microsoft.AspNetCore.Mvc;
using GERMAG.Shared;
using Microsoft.AspNetCore.Cors;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;
using GERMAG.Server.ReportCreation;
using GERMAG.Server.GeometryCalculations;

namespace GERMAG.Server.Controllers;

[ApiController]
[Route("api/[controller]")]

public class ReportController(ICreateReportAsync createReport, IReceiveLandParcel receiveLandParcel) : ControllerBase
{
    [HttpGet("reportdata")]
    [EnableCors(CorsPolicies.GetAllowed)]
    public async Task<IEnumerable<Report>> GetReport(double Xcor, double Ycor, int Srid)
    {
        var a = await receiveLandParcel.GetLandParcel(Xcor, Ycor, Srid);
        return await createReport.CreateGeothermalReportAsync(Xcor, Ycor, Srid);
    }
}