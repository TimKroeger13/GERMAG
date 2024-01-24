using GERMAG.DataModel.Database;
using GERMAG.Shared;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace GERMAG.Server.ReportCreation;

public interface ICreateReportAsync
{
    Task<IEnumerable<Report>> CreateGeothermalReportAsync();
}

public class CreateReport(DataContext context) : ICreateReportAsync
{
    public async Task<IEnumerable<Report>> CreateGeothermalReportAsync()
    {
        FindIntersections();

        return new[] { new Report
    {
        Test = "Hier könnten ihre geothermischen Daten stehen!"
    }};
    }


    private void FindIntersections()
    {
        var Xcor = 392692.7;
        var Ycor = 5824271.2;
        var Srid = 25833;

        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: Srid);
        var point = geometryFactory.CreatePoint(new Coordinate(Xcor, Ycor));

        var landparcelIntersection = context.GeoData.Where(gd => gd.ParameterKey == 1 && gd.Geom!.Intersects(point)).Select(gd => gd.Geom);

        var IntersectingGeometry = context.GeoData.Where(gd => gd.ParameterKey != 1 && landparcelIntersection.Any(lp => gd.Geom!.Intersects(lp))).Select(gd => new { gd.ParameterKey, gd.Parameter });

        var result = IntersectingGeometry.Join(
         context.GeothermalParameter,
         ig => ig.ParameterKey,
         gp => gp.Id,
         (ig, gp) => new
         {
             gp.Type,
             ig.ParameterKey,
             ig.Parameter,
         }).ToList();

    }



}
