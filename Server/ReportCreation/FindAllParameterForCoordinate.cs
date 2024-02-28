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
    List<GeometryElementParameter> FindCoordianteParameters(LandParcel landParcelElement);
}

public class FindAllParameterForCoordinate(DataContext context) : IFindAllParameterForCoordinate
{
    public List<GeometryElementParameter> FindCoordianteParameters(LandParcel landParcelElement)
    {
        /*        var geoJsonWriter = new GeoJsonWriter();

                var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: Srid);
                var originalPoint = geometryFactory.CreatePoint(new Coordinate(Xcor, Ycor));

                var transformedPoint = context.GeoData  //Database
                    .FromSql($"SELECT ST_Transform(ST_SetSRID(ST_MakePoint({originalPoint.X}, {originalPoint.Y}), {Srid}), 25833) AS geom")
                    .Select(gd => gd.Geom)
                    .FirstOrDefault();

                var landParcelId = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.land_parcels).Id;

                var landparcelIntersection = context.GeoData.Where(gd => gd.ParameterKey == landParcelId && gd.Geom!.Intersects(transformedPoint)).Select(gd => new { gd.Geom, gd.Id }).ToList();

                var returnValue = new LandParcel
                {
                    ParameterKey = landparcelIntersection[0].Id,
                    Geometry = landparcelIntersection[0].Geom,
                    GeometryJson = geoJsonWriter.Write(landparcelIntersection[0].Geom),
                };*/

        var IntersectingGeometry = context.GeoData
            .Where(gd => gd.ParameterKey != landParcelElement.ParameterKey && gd.Geom!.Intersects(landParcelElement.Geometry))
            .Select(gd => new
            {
                gd.ParameterKey,
                gd.Parameter
                //Geometry = geoJsonWriter.Write(gd.Geom)
            });

        //var landPacelGeometry = context.GeoData.Where(gd => landparcelIntersection.Any(lp => lp.Id == gd.Id)).Select(gd => new { gd.ParameterKey, gd.Parameter}); //Geometry = geoJsonWriter.Write(gd.Geom)

        /*var landParcelResult = landPacelGeometry
            .Join(
                context.GeothermalParameter,
                ig => ig.ParameterKey,
                gp => gp.Id,
                (ig, gp) => new GeometryElementParameter
                {
                    Type = gp.Type,
                    ParameterKey = ig.ParameterKey,
                    Parameter = ig.Parameter
                    //Geometry = gp.Type == TypeOfData.land_parcels ? ig.Geometry : null
                })
            .ToList();*/

        var landParcelResult = new GeometryElementParameter
        {
            Type = TypeOfData.land_parcels,
            ParameterKey = landParcelElement.ParameterKey,
            Parameter = landParcelElement.Parameter
        };

        var intersectingResult = IntersectingGeometry
            .Join(
                context.GeothermalParameter,
                ig => ig.ParameterKey,
                gp => gp.Id,
                (ig, gp) => new GeometryElementParameter
                {
                    Type = gp.Type,
                    ParameterKey = ig.ParameterKey,
                    Parameter = ig.Parameter
                    //Geometry = gp.Type == TypeOfData.land_parcels ? ig.Geometry : null
                })
            .ToList();

        var result = intersectingResult.Append(landParcelResult).ToList();

        //var result = intersectingResult.ToList();

        return result;
    }
}