using GERMAG.DataModel.Database;
using GERMAG.Server.ReportCreation;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Union;
using System.Linq;
using NetTopologySuite.IO;
using Microsoft.EntityFrameworkCore;

namespace GERMAG.Server.Research;

public interface ICalcualteAllParameterForArea
{
    Task<string> calucalteAllParameters();
}

public class CalcualteAllParameterForArea(DataContext context, IParameterDeserialator parameterDeserialator) : ICalcualteAllParameterForArea
{
    public async Task<String> calucalteAllParameters()
    {

        context.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

        var researchData = context.Researches.ToList();

        context.Database.SetCommandTimeout(null);

        var b = 3;


        return "Test from Server";
    }
}
