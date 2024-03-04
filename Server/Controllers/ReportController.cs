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
public class ReportController(ICreateReportAsync createReport, IReceiveLandParcel receiveLandParcel, IRestrictionFromLandParcel restrictionFromLandParcel) : ControllerBase
{
    [HttpGet("reportdata")]
    [EnableCors(CorsPolicies.GetAllowed)]
    public async Task<IEnumerable<Report>> GetReport(double Xcor, double Ycor, int Srid)
    {
        LandParcel landParcelElement = await receiveLandParcel.GetLandParcel(Xcor, Ycor, Srid);

        if (landParcelElement.Error == true)
        {
            return new[] { new Report
        {
            Error = "Land Parcel not Found in the search area"
        }};
        }

        IEnumerable<Report> polygonBasedReport = await createReport.CreateGeothermalReportAsync(landParcelElement);

        List<Report> reportList = polygonBasedReport.ToList();
        reportList[0].Geometry = landParcelElement.GeometryJson;

        return reportList;
    }


    [HttpGet("fullreport")]
    [EnableCors(CorsPolicies.GetAllowed)]
    public async Task<String> GetFullReport(double Xcor, double Ycor, int Srid)
    {
        LandParcel landParcelElement = await receiveLandParcel.GetLandParcel(Xcor, Ycor, Srid);

        return await restrictionFromLandParcel.CalculateRestrictions(landParcelElement);


        //return "Second API works";
    }

}