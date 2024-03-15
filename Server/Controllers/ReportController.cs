using Microsoft.AspNetCore.Mvc;
using GERMAG.Shared;
using Microsoft.AspNetCore.Cors;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;
using GERMAG.Server.ReportCreation;
using GERMAG.Server.GeometryCalculations;
using NetTopologySuite.IO;

namespace GERMAG.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController(ICreateReportAsync createReport, IReceiveLandParcel receiveLandParcel, IRestrictionFromLandParcel restrictionFromLandParcel, IGeoThermalProbesCalcualtion geoThermalProbesCalcualtion) : ControllerBase
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
    public async Task<IEnumerable<Report>> GetFullReport(double Xcor, double Ycor, int Srid)
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

        Restricion RestrictionFile = await restrictionFromLandParcel.CalculateRestrictions(landParcelElement);

        var FinalReport = polygonBasedReport.ToList();
        FinalReport[0].Geometry_Usable = RestrictionFile.Geometry_Usable_geoJson;
        FinalReport[0].Geometry_Restiction = RestrictionFile.Geometry_Restiction_geoJson;
        FinalReport[0].Usable_Area = RestrictionFile.Usable_Area;
        FinalReport[0].Restiction_Area = RestrictionFile.Restiction_Area;


        var geoJsonWriter = new GeoJsonWriter();

        var test = await geoThermalProbesCalcualtion.CalculateGeoThermalProbes(RestrictionFile);

        FinalReport[0].ProbePoint = test.Item1;
        FinalReport[0].Geometry_LeftOverArea = geoJsonWriter.Write(test.Item2);

        return FinalReport;
    }
}