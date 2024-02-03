using NetTopologySuite.Geometries;
using NetTopologySuite;
using GERMAG.DataModel.Database;
using GERMAG.Shared;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries.Utilities;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;
using ProjNet.IO.CoordinateSystems;
using NetTopologySuite.CoordinateSystems.Transformations;
using GeoAPI;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using NetTopologySuite.IO;

namespace GERMAG.Server.ReportCreation;

public interface IFindAllParameterForCoordinate
{
    List<CoordinateParameters> FindCoordianteParameters(double Xcor, double Ycor, int Srid);
}

public class FindAllParameterForCoordinate(DataContext context) : IFindAllParameterForCoordinate
{
    public List<CoordinateParameters> FindCoordianteParameters(double Xcor, double Ycor, int Srid)
    {

        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: Srid);
        var originalPoint = geometryFactory.CreatePoint(new Coordinate(Xcor, Ycor));

        var landParcelId = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.land_parcels).Id;

        var landparcelIntersection = context.GeoData.Where(gd => gd.ParameterKey == landParcelId && gd.Geom!.Intersects(originalPoint)).Select(gd => new { gd.Geom, gd.Id});

        //var IntersectingGeometry = context.GeoData.Where(gd => gd.ParameterKey != landParcelId && landparcelIntersection.Any(lp => gd.Geom!.Intersects(lp.Geom))).Select(gd => new { gd.ParameterKey, gd.Parameter , gd.Geom});

        var geoJsonWriter = new GeoJsonWriter();

        var IntersectingGeometry = context.GeoData
            .Where(gd => gd.ParameterKey != landParcelId && landparcelIntersection.Any(lp => gd.Geom!.Intersects(lp.Geom)))
            .Select(gd => new
            {
                gd.ParameterKey,
                gd.Parameter,
                Geometry = geoJsonWriter.Write(gd.Geom)
            });


        var landPacelGeometry = context.GeoData.Where(gd => landparcelIntersection.Any(lp => lp.Id == gd.Id)).Select(gd => new { gd.ParameterKey, gd.Parameter, Geometry = geoJsonWriter.Write(gd.Geom)});

        //var combinedGeometry = landPacelGeometry.Concat(IntersectingGeometry);


        var landParcelResult = landPacelGeometry
    .Join(
        context.GeothermalParameter,
        ig => ig.ParameterKey,
        gp => gp.Id,
        (ig, gp) => new CoordinateParameters
        {
            Type = gp.Type,
            ParameterKey = ig.ParameterKey,
            Parameter = ig.Parameter,
            Geometry = gp.Type == TypeOfData.land_parcels ? ig.Geometry : null
        })
    .ToList();  // Materialize the result here

        var intersectingResult = IntersectingGeometry
            .Join(
                context.GeothermalParameter,
                ig => ig.ParameterKey,
                gp => gp.Id,
                (ig, gp) => new CoordinateParameters
                {
                    Type = gp.Type,
                    ParameterKey = ig.ParameterKey,
                    Parameter = ig.Parameter,
                    Geometry = gp.Type == TypeOfData.land_parcels ? ig.Geometry : null
                })
            .ToList();  // Materialize the result here

        var result = landParcelResult.Concat(intersectingResult).ToList();

        return result;
    }
}
