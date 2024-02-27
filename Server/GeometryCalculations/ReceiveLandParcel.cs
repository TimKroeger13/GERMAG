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

            var transformedPoint = context.GeoData  //Database
                .FromSql($"SELECT ST_Transform(ST_SetSRID(ST_MakePoint({originalPoint.X}, {originalPoint.Y}), {Srid}), 25833) AS geom")
                .Select(gd => gd.Geom)
                .FirstOrDefault();

            var landParcelId = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.land_parcels).Id;

            var landparcelIntersection = context.GeoData.Where(gd => gd.ParameterKey == landParcelId && gd.Geom!.Intersects(transformedPoint)).Select(gd => new { gd.Geom, gd.Id }).ToList();


            //var transformedPointGeoJson = context.GeoData.FromSql($"ST_AsGeoJSON(ST_Transform(ST_SetSRID(ST_GeomFromGeoJSON('{{\"type\":\"Point\",\"coordinates\":[392411.44600000046,5821172.262]}}'), 25833), 3857))").First();

           


            //var transformedPointGeoJson3 = context.GeoData.FromSqlInterpolated($@"SELECT ST_AsGeoJSON( ST_Transform( ST_SetSRID( ST_GeomFromGeoJSON('{{""type"":""Point"",""coordinates"":[392411.44600000046,5821172.262]}}'), 25833 ), 3857 ) ) AS geom").Select(gd => gd.Geom).FirstOrDefault();


            var returnValue = new LandParcel
            {
                ParameterKey = landparcelIntersection[0].Id,
                Geometry = landparcelIntersection[0].Geom,
                GeometryJson = geoJsonWriter.Write(landparcelIntersection[0].Geom),
            };

            var GeoJsonString =  "{\"type\":\"Point\",\"coordinates\":[392411.44600000046,5821172.262]}";


            var queryString = "SELECT ST_AsGeoJSON(ST_Transform(ST_SetSRID(ST_GeomFromGeoJSON(@geoJson), 25833), 3857)) AS transformed_geojson";
            var geoJsonString = "{\"type\":\"Point\",\"coordinates\":[392411.44600000046,5821172.262]}";

            var transformedGeoJson = context.GeoData
                .FromSqlRaw(queryString, new Npgsql.NpgsqlParameter("geoJson", geoJsonString))
                .AsEnumerable()
                .FirstOrDefault();


            var transformedPointGeoJson3 = context.GeoData
                .FromSql($"SELECT ST_AsGeoJSON(ST_Transform(ST_SetSRID(ST_GeomFromGeoJSON('{GeoJsonString}'), 25833), 3857)) AS geom")
                .Select(gd => gd.Geom)
                .FirstOrDefault();



            return returnValue;
        });
    }
}