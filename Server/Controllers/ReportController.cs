using Microsoft.AspNetCore.Mvc;
using GERMAG.Shared;
using Microsoft.AspNetCore.Cors;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;
using GERMAG.Server.ReportCreation;
using GERMAG.Server.GeometryCalculations;
using NetTopologySuite.IO;
using GERMAG.Shared.PointProperties;

namespace GERMAG.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController(ICreateReportAsync createReport, IReceiveLandParcel receiveLandParcel, IRestrictionFromLandParcel restrictionFromLandParcel, IGeoThermalProbesCalcualtion geoThermalProbesCalcualtion, IGetProbeSpecificData getProbeSpecificData, IGetPolylineData getPolylineData) : ControllerBase
{
    [HttpGet("reportdata")]
    [EnableCors(CorsPolicies.GetAllowed)]
    public async Task<IEnumerable<Report>> GetReport(List<double> Xcor, List<double> Ycor, int Srid)
    {
        LandParcel landParcelElement = await receiveLandParcel.GetLandParcel(Xcor, Ycor, Srid);

        if (landParcelElement.Error == true)
        {
            return new[] { new Report
        {
            Error = "Land Parcel not Found in the search area"
        }};
        }

        Restricion RestrictionFile = await restrictionFromLandParcel.CalculateRestrictions(landParcelElement);

        IEnumerable<Report> polygonBasedReport = await createReport.CreateGeothermalReportAsync(landParcelElement, RestrictionFile);

        List<Report> reportList = polygonBasedReport.ToList();
        reportList[0].Geometry = landParcelElement.GeometryJson;

        return reportList;
    }

    [HttpGet("fullreport")]
    [EnableCors(CorsPolicies.GetAllowed)]
    public async Task<IEnumerable<Report>> GetFullReport(double Xcor, double Ycor, int Srid, bool probeRes)
    {
        LandParcel landParcelElement = await receiveLandParcel.GetLandParcel(Xcor, Ycor, Srid);

        if (landParcelElement.Error == true)
        {
            return new[] { new Report
        {
            Error = "Land Parcel not Found in the search area"
        }};
        }

        Restricion RestrictionFile = await restrictionFromLandParcel.CalculateRestrictions(landParcelElement);

        IEnumerable<Report> polygonBasedReport = await createReport.CreateGeothermalReportAsync(landParcelElement, RestrictionFile);

        var FinalReport = polygonBasedReport.ToList();
        FinalReport[0].Geometry_Usable = RestrictionFile.Geometry_Usable_geoJson;
        FinalReport[0].Geometry_Restiction = RestrictionFile.Geometry_Restiction_geoJson;
        FinalReport[0].Usable_Area = RestrictionFile.Usable_Area;
        FinalReport[0].Restiction_Area = RestrictionFile.Restiction_Area;

        List<ProbePoint?> FullPointProbe;

        try
        {
            FullPointProbe = await geoThermalProbesCalcualtion.CalculateGeoThermalProbes(RestrictionFile);
        }
        catch (Exception e)
        {
            FinalReport[0].Error = e.Message;
            return FinalReport;
        }

        if (probeRes) //Probe Resulution
        {
            FullPointProbe = await getProbeSpecificData.GetPointProbeData(landParcelElement, FullPointProbe);
        }

        //LineInformation - ExpectedGroundWaterHeight
        var ZeHGW = await getPolylineData.GetNearestPolylineData(landParcelElement);

        FinalReport[0].ZeHGW = ZeHGW;

        List<ProbePoint?> TruncatedPointProbe = new();

        foreach (var probePoint in FullPointProbe)
        {
            if (probePoint != null)
            {
                TruncatedPointProbe.Add(new ProbePoint
                {
                    GeometryJson = probePoint.GeometryJson,
                    Properties = probePoint.Properties
                });
            }
            else { TruncatedPointProbe.Add(null); }
        }

        FinalReport[0].ProbePoint = TruncatedPointProbe;

        return FinalReport;
    }
}