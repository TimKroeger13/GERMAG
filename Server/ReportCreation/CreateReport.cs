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

public class CreateReport(IFindAllParameterForCoordinate findAllParameterForCoordinate, IParameterDeserialator parameterDeserialator, ICreateReportStructure createReportStructure) : ICreateReportAsync
{
    public async Task<IEnumerable<Report>> CreateGeothermalReportAsync()
    {
        const double Xcor = 392692.7;
        const double Ycor = 5824271.2;
        const int Srid = 25833;

        var ParameterList = await Task.Run(() => findAllParameterForCoordinate.FindCoordianteParameters(Xcor, Ycor, Srid));

        var jsonData_Parameter = await Task.Run(() => ParameterList.Select(p => parameterDeserialator.DeserializeParameters(p.Parameter ?? ""))
                                          .ToList());

        var mergedList = ParameterList.Zip(jsonData_Parameter, (original, jsonData) =>
        new CoordinateParameters
        {
            Type = original.Type,
            ParameterKey = original.ParameterKey,
            Parameter = original.Parameter,
            JsonDataParameter = jsonData
        })
        .ToList();

        var CompleteReport = await Task.Run(() => createReportStructure.CreateReport(mergedList, Xcor, Ycor, Srid));

        return CompleteReport;

    }
}
