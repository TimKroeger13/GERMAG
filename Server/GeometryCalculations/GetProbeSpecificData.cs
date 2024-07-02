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

public class GetProbeSpecificData(DataContext context, IGetProbeSepcificDataSingleProbe getProbeSepcificDataSingleProbe) : IGetProbeSpecificData
{
    public async Task<List<ProbePoint?>> GetPointProbeData(LandParcel landParcelElement, List<ProbePoint?> probePoints)
    {
        List<Task<ProbePoint?>> tasks = new();

        for (int i = 0; i < probePoints.Count; i++) //probePoints.Count    (int i = 0; i < probePoints.Count; i++)
        {
            Console.WriteLine("Probe: " + i + " / " + (probePoints.Count-1));

            tasks.Add(getProbeSepcificDataSingleProbe.GetSingleProbeData(landParcelElement, probePoints[i], context));
        }

        ProbePoint?[] results = await Task.WhenAll(tasks);

        List<ProbePoint?> probePointList = new();

        probePointList.AddRange(results);

        return probePointList;
    }
}