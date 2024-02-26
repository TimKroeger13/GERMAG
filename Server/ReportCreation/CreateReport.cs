using GERMAG.DataModel.Database;
using GERMAG.Server.DataPulling.JsonDeserialize;
using GERMAG.Shared;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System.Text.Json;

namespace GERMAG.Server.ReportCreation;

public interface ICreateReportAsync
{
    Task<IEnumerable<Report>> CreateGeothermalReportAsync(double Xcor, double Ycor, int Srid);
}

public class CreateReport(IFindAllParameterForCoordinate findAllParameterForCoordinate, IParameterDeserialator parameterDeserialator, ICreateReportStructure createReportStructure) : ICreateReportAsync
{
    public async Task<IEnumerable<Report>> CreateGeothermalReportAsync(double Xcor, double Ycor, int Srid)
    {
        var ParameterList = await Task.Run(() => findAllParameterForCoordinate.FindCoordianteParameters(Xcor, Ycor, Srid));

        var jsonData_Parameter = await Task.Run(() => ParameterList.Select(p => parameterDeserialator.DeserializeParameters(p.Parameter ?? ""))
                                          .ToList());

        var mergedList = ParameterList.Zip(jsonData_Parameter, (original, jsonData) =>
        new GeometryElementParameter
        {
            Type = original.Type,
            ParameterKey = original.ParameterKey,
            Parameter = original.Parameter,
            //Geometry = original.Geometry
            JsonDataParameter = jsonData
        })
        .ToList();

        var CompleteReport = await Task.Run(() => createReportStructure.CreateReport(mergedList, Xcor, Ycor, Srid));


        return CompleteReport;

    }
}
