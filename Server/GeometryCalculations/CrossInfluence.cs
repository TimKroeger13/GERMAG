using GERMAG.Shared.PointProperties;

namespace GERMAG.Server.GeometryCalculations;

public interface ICrossInfluence
{
    Task<double> GetCrossInfluence(List<ProbePoint?> PointProbe);
}

public class CrossInfluence : ICrossInfluence
{
    public async Task<double> GetCrossInfluence(List<ProbePoint?> PointProbe)
    {
        return await Task.Run(() =>
        {
            int n = PointProbe.Count;
            double CrossFactor;

            var RawSum = PointProbe.Select(x => x?.Properties?.RawExtractionKW).Sum() ?? 0;


            if (n ==1)
            {
                CrossFactor = 1.1;
            }
            else if (n < 70)
            {
                CrossFactor = (159.863 - 12.5247 * Math.Log(100.761 * n - 141.675)) / 100;
            }
            else
            {
                CrossFactor = 0.49;
            }

            var AdeptedSum = Math.Round((double)RawSum * CrossFactor, 2);

            return AdeptedSum;
        });
    }
}
