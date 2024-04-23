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

        var researchData = context.Researches.ToList();



        LandParcel landParcelElement = new LandParcel
        {
            Geometry = researchData[0].Geom
        };

        Restricion RestrictionFile = await restrictionFromLandParcel.CalculateRestrictions(landParcelElement);


        //var GeometryJson = geoJsonWriter.Write(RestrictionFile.Geometry_Usable_geoJson);

        File.WriteAllText(path, RestrictionFile.Geometry_Usable_geoJson);



        context.Database.SetCommandTimeout(null);
        return "Test from Server";
    }
}
