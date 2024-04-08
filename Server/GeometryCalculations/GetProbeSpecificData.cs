using GERMAG.DataModel.Database;
using GERMAG.Shared;
using GERMAG.Shared.PointProperties;

namespace GERMAG.Server.GeometryCalculations;

public interface IGetProbeSpecificData
{
    Task<List<ProbePoint?>> GetPointProbeData(LandParcel landParcelElement, List<ProbePoint?> probePoints);
}

public class GetProbeSpecificData(DataContext context) : IGetProbeSpecificData
{
    public async Task<List<ProbePoint?>> GetPointProbeData(LandParcel landParcelElement, List<ProbePoint?> probePoints)
    {
        foreach (var SingleProbePoint in probePoints)
        {
            if (SingleProbePoint == null || SingleProbePoint.Geometry == null)
            {
                continue;
            }

            SingleProbePoint.Geometry.SRID = 25833;

            var IntersectingGeometry = context.GeoData
                .Where(gd => gd.ParameterKey != landParcelElement.ParameterKey && gd.Geom!.Intersects(SingleProbePoint.Geometry))
                .Select(gd => new
                {
                    gd.ParameterKey,
                    gd.Parameter
                });

            var intersectingResult = IntersectingGeometry
                .Join(
                context.GeothermalParameter,
                ig => ig.ParameterKey,
                gp => gp.Id,
                (ig, gp) => new GeometryElementParameter
                {
                    Type = gp.Type,
                    ParameterKey = ig.ParameterKey,
                    Parameter = ig.Parameter
                }).ToList();

            int? MaxDepth = intersectingResult.FirstOrDefault(element => element.Type == TypeOfData.depth_restrictions)?.ParameterKey;
            if (MaxDepth == null)
            {
                MaxDepth = 100; //Berlin spesific
            }


            var GeoPoten2400 = intersectingResult.FirstOrDefault(element => element.Type == TypeOfData.geo_poten_100m_with_2400ha);


            var b = 3;
        }

        var a = probePoints.ToList();

        return probePoints;
    }
}