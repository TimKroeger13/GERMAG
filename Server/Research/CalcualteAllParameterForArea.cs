using GERMAG.DataModel.Database;
using GERMAG.Server.ReportCreation;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Union;
using System.Linq;
using NetTopologySuite.IO;
using Microsoft.EntityFrameworkCore;
using GERMAG.Shared;
using GERMAG.Server.GeometryCalculations;
using System.Net;
using System.Text.RegularExpressions;
using GERMAG.Server.DataPulling;

namespace GERMAG.Server.Research;

public interface ICalcualteAllParameterForArea
{
    Task<string> calucalteAllParameters();
}

public class CalcualteAllParameterForArea(DataContext context, IRestrictionFromLandParcel restrictionFromLandParcel, IFindLocalDirectoryPath findLocalDirectoryPath) : ICalcualteAllParameterForArea
{
    public async Task<String> calucalteAllParameters()
    {
        var path = findLocalDirectoryPath.getLocalPath("CalculationResults", "berlinWohnfläche.geojson");

        //var geoJsonWriter = new GeoJsonWriter();

        //get geodata
        context.Database.SetCommandTimeout(TimeSpan.FromMinutes(60));

        var researchData = context.Researches.FirstOrDefault();
        var ax_tree = context.AxTrees.FirstOrDefault();
        var ax_buildings = context.AxBuildings.FirstOrDefault();


        var buildingID = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.building_surfaces).Id;
        var TreeID = context.GeothermalParameter.Where(gp => gp.Type == TypeOfData.tree_points).Select(gp => gp.Id).ToList();


        //var GeometryJson = geoJsonWriter.Write(RestrictionFile.Geometry_Usable_geoJson);

        //File.WriteAllText(path, RestrictionFile.Geometry_Usable_geoJson);



        context.Database.SetCommandTimeout(null);
        return "Test from Server";
    }
}

//Scaffold-DbContext "Host=10.140.17.197:5433;Database=gasag;Username=postgres;Password=gasag" Npgsql.EntityFrameworkCore.PostgreSQL -NoOnConfiguring -OutputDir../Shared/DataModel/Database -Force -Context DataContext -Namespace GERMAG.DataModel.Database -StartupProject GERMAG.Server -Project GERMAG.Shared
