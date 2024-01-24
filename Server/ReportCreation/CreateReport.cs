using GERMAG.DataModel.Database;
using GERMAG.Server.DataPulling.JsonDeserialize;
using GERMAG.Shared;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System.Text.Json;

namespace GERMAG.Server.ReportCreation;

public interface ICreateReportAsync
{
    Task<IEnumerable<Report>> CreateGeothermalReportAsync();
}

public class CreateReport(DataContext context, IParameterDeserialator parameterDeserialator) : ICreateReportAsync
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

        var jsonData_Root = parameterDeserialator.DeserializeParameters(result[0].Parameter ?? "");

        var b = 3;

    }



}
