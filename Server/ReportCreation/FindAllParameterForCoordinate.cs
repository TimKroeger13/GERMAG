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

        var transformedPoint = context.GeoData
            .FromSql($"SELECT ST_Transform(ST_SetSRID(ST_MakePoint({originalPoint.X}, {originalPoint.Y}), {Srid}), 25833) AS geom")
            .Select(gd => gd.Geom)
            .FirstOrDefault();

        var landParcelId = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.land_parcels).Id;

        var landparcelIntersection = context.GeoData.Where(gd => gd.ParameterKey == landParcelId && gd.Geom!.Intersects(transformedPoint)).Select(gd => new { gd.Geom, gd.Id});

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
            .ToList();

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
            .ToList();

        var result = landParcelResult.Concat(intersectingResult).ToList();

        return result;
    }
}
