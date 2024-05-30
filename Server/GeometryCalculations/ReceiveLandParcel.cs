using GERMAG.DataModel.Database;
using GERMAG.Shared;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;
using Npgsql;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;

namespace GERMAG.Server.GeometryCalculations;

public interface IReceiveLandParcel
{
    Task<LandParcel> GetLandParcel(List<double> Xcor, List<double> Ycor, int Srid);
}

public class ReceiveLandParcel(DataContext context) : IReceiveLandParcel
{
    public async Task<LandParcel> GetLandParcel(List<double> Xcor, List<double> Ycor, int Srid)
    {
        return await Task.Run(() =>
        {
            var geoJsonWriter = new GeoJsonWriter();

            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: Srid);
            var originalPoint = geometryFactory.CreatePoint(new Coordinate(Xcor[0], Ycor[0]));

            var transformedPoint = context.GeoData  //Database
                .FromSql($"SELECT ST_Transform(ST_SetSRID(ST_MakePoint({originalPoint.X}, {originalPoint.Y}), {Srid}), 25833) AS geom")
                .Select(gd => gd.Geom)
                .FirstOrDefault();

            var landParcelID = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.land_parcels).Id;

            var landparcelIntersection = context.GeoData.Where(gd => gd.ParameterKey == landParcelID && gd.Geom!.Intersects(transformedPoint)).Select(gd => new { gd.Geom, gd.Id, gd.ParameterKey, gd.Parameter }).Take(1).ToList();

            if (landparcelIntersection.Count == 0)
            {
                return new LandParcel
                {
                    Error = true,
                };
            }

            var returnValue = new LandParcel
            {
                GeoDataID = landparcelIntersection[0].Id,
                ParameterKey = landParcelID,
                Parameter = landparcelIntersection[0].Parameter,
                Geometry = landparcelIntersection[0].Geom,
                GeometryJson = geoJsonWriter.Write(landparcelIntersection[0].Geom),
            };

            return returnValue;
        });
    }
}