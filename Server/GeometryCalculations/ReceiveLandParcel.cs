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

namespace GERMAG.Server.GeometryCalculations;

public interface IReceiveLandParcel
{
    Task<LandParcel> GetLandParcel(double Xcor, double Ycor, int Srid);
}

public class ReceiveLandParcel(DataContext context) : IReceiveLandParcel
{
    public async Task<LandParcel> GetLandParcel(double Xcor, double Ycor, int Srid)
    {
        return await Task.Run(() =>
        {
            var geoJsonWriter = new GeoJsonWriter();

            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: Srid);
            var originalPoint = geometryFactory.CreatePoint(new Coordinate(Xcor, Ycor));

            var transformedPoint = context.GeoData
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
            };

            return returnValue;
        });
    }
}