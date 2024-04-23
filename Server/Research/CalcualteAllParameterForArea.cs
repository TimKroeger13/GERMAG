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
        //get geodata
        context.Database.SetCommandTimeout(TimeSpan.FromMinutes(60));


        var researchData = context.Researches.FirstOrDefault();
        var ax_tree = context.AxTrees.FirstOrDefault();
        var ax_buildings = context.AxBuildings.FirstOrDefault();


        NetTopologySuite.Geometries.Geometry? bufferedTrees = ax_tree!.Geom!.Buffer(OfficalParameters.TreeDistance);

        Polygon? landParcelPolygon = (Polygon?)researchData!.Geom;
        LineString? landParcelLineString = landParcelPolygon?.ExteriorRing;

        NetTopologySuite.Geometries.Geometry? bufferedLandParcel = landParcelLineString?.Buffer(OfficalParameters.LandParcelDistance);

        NetTopologySuite.Geometries.Geometry? bufferedBuldings = ax_buildings?.Geom!.Buffer(OfficalParameters.BuildingDistance);



        NetTopologySuite.Geometries.Geometry? UsableArea = landParcelPolygon?.Difference(bufferedLandParcel).Difference(bufferedBuldings).Difference(bufferedTrees);
        UsableArea = UsableArea?.Union();

        NetTopologySuite.Geometries.Geometry? RestictionArea = bufferedLandParcel?.Union(bufferedBuldings).Union(bufferedTrees);
        RestictionArea = landParcelPolygon?.Intersection(RestictionArea);
        RestictionArea = RestictionArea?.Union();

        var geoJsonWriter = new GeoJsonWriter();

        var returnValue = new Restricion
        {
            Geometry_Usable = UsableArea,
            Geometry_Restiction = RestictionArea,
            Geometry_Usable_geoJson = geoJsonWriter.Write(UsableArea),
            Geometry_Restiction_geoJson = geoJsonWriter.Write(RestictionArea),
            Usable_Area = UsableArea?.Area ?? 0,
            Restiction_Area = RestictionArea?.Area ?? 0,
        };

        var path = findLocalDirectoryPath.getLocalPath("CalculationResults", "berlinWohnfläche.geojson");
        File.WriteAllText(path, returnValue.Geometry_Usable_geoJson);



        context.Database.SetCommandTimeout(null);
        return "Test from Server";
    }
}
