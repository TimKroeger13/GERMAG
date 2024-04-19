using GERMAG.DataModel.Database;
using GERMAG.Server.ReportCreation;
using GERMAG.Shared;
using GERMAG.Shared.PointProperties;
using Newtonsoft.Json.Linq;

namespace GERMAG.Server.GeometryCalculations;

public interface IGetProbeSpecificData
{
    Task<List<ProbePoint?>> GetPointProbeData(LandParcel landParcelElement, List<ProbePoint?> probePoints);
}

public class GetProbeSpecificData(IGetProbeSepcificDataSingleProbe getProbeSepcificDataSingleProbe) : IGetProbeSpecificData
{
    public async Task<List<ProbePoint?>> GetPointProbeData(LandParcel landParcelElement, List<ProbePoint?> probePoints)
    {
        for (int i = 0; i < probePoints.Count; i++)
        {
            probePoints[i] = await getProbeSepcificDataSingleProbe.GetSingleProbeData(landParcelElement,probePoints[i]);
        }

        return probePoints;
    }





}