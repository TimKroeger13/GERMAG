﻿using GERMAG.DataModel.Database;
using GERMAG.Server.DataPulling.JsonDeserialize;
using GERMAG.Shared;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GERMAG.Server.DataPulling
{
    public interface IDatabaseUpdater
    {
        void UpdateDatabase(Root json, int ForeignKey);
    }

    public partial class DatabaseUpdater(DataContext context) : IDatabaseUpdater
    {
        public void UpdateDatabase(Root json, int foreignKey)
        {
            using var transaction = context.Database.BeginTransaction();

            var espgStringRaw = json.Crs?.Properties?.Name ?? throw new Exception("SRID not found");
            var espgStringRegex = ESPGRegexNumber().Match(ESPGRegexShrink().Replace(espgStringRaw ?? "0", ":"));
            var espgString = espgStringRegex.Value;

            var espgNumber = int.Parse(espgString);

            context.GeoData.Where(g => g.ParameterKey == foreignKey).ExecuteDelete();
            context.SaveChanges();

            context.GeothermalParameter.First(gp => gp.Id == foreignKey).Srid = espgNumber;
            var i = 0;
            var totalLength = json?.Features?.Count ?? 0;

            //find GeometryType

            Geometry_Type CurrentGeometryType = Geometry_Type.empty;

            var geometryTypeNameAsString = json?.Features?[0]?.Geometry?.Type;

            if (geometryTypeNameAsString == "Polygon" || geometryTypeNameAsString == "LineString" || geometryTypeNameAsString == "MultiLineString" || geometryTypeNameAsString == "Point" || geometryTypeNameAsString == "MultiPolygon")
            {
                if (geometryTypeNameAsString == "Polygon" || geometryTypeNameAsString == "MultiPolygon")
                {
                    CurrentGeometryType = Geometry_Type.polygon;
                }
                if (geometryTypeNameAsString == "LineString" || geometryTypeNameAsString == "MultiLineString")
                {
                    CurrentGeometryType = Geometry_Type.polyline;
                }
                if (geometryTypeNameAsString == "Point")
                {
                    CurrentGeometryType = Geometry_Type.point;
                }
                context.GeothermalParameter.First(gp => gp.Id == foreignKey).Geometry_Type = CurrentGeometryType;
            }
            else
            {
                throw new Exception("No Geometry Type in the webrequest found thats matches a known Type");
            }

            switch (CurrentGeometryType)
            {
                case Geometry_Type.polygon:
                    Console.WriteLine("Transfering data to database");
                    foreach (var feature in json?.Features ?? throw new Exception("DatabaseUpdater: feature not found!"))
                    {
                        i++;
                        var coordinates = feature?.Geometry?.Coordinates;

                        if (coordinates != null)
                        {
                            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: espgNumber);
                            var exteriorLinearRing = geometryFactory.CreateLinearRing(coordinates[0].Select(coord => new Coordinate(coord[0], coord[1])).ToArray());
                            var polygon = geometryFactory.CreatePolygon(exteriorLinearRing);

                            if (coordinates.Count > 1)
                            {
                                var holes = new List<LinearRing>();

                                for (int k = 1; k < coordinates.Count; k++)
                                {
                                    var holeLinearRing = geometryFactory.CreateLinearRing(coordinates[k].Select(coord => new Coordinate(coord[0], coord[1])).ToArray());
                                    holes.Add(holeLinearRing);
                                }
                                polygon = geometryFactory.CreatePolygon(exteriorLinearRing, holes.ToArray());
                            }
                            var newGeoDatum = new DataModel.Database.GeoDatum
                            {
                                Id = 0,
                                Geom = polygon,
                                ParameterKey = foreignKey,
                                Parameter = JsonSerializer.Serialize(feature?.Properties)
                            };
                            context.GeoData.Add(newGeoDatum);
                        }
                        if (i % 1000 == 0)
                        {
                            Console.WriteLine(Math.Round((Convert.ToDouble(i) / totalLength) * 100, 0) + "%");
                        }
                    }
                    context.GeothermalParameter.First(gp => gp.Id == foreignKey).LastUpdate = DateTime.Now;
                    context.SaveChanges();
                    break;

                case Geometry_Type.polyline:
                    Console.WriteLine("Transfering data to database");
                    foreach (var feature in json?.Features ?? throw new Exception("DatabaseUpdater: feature not found!"))
                    {
                        i++;
                        var coordinates = feature?.Geometry?.Coordinates;

                        if (coordinates != null)
                        {
                            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: espgNumber);
                            var polyline = geometryFactory.CreateLineString(coordinates[0].Select(coord => new Coordinate(coord[0], coord[1])).ToArray());

                            var newGeoDatum = new DataModel.Database.GeoDatum
                            {
                                Id = 0,
                                Geom = polyline,
                                ParameterKey = foreignKey,
                                Parameter = JsonSerializer.Serialize(feature?.Properties)
                            };
                            context.GeoData.Add(newGeoDatum);
                        }
                        if (i % 1000 == 0)
                        {
                            Console.WriteLine(Math.Round((Convert.ToDouble(i) / totalLength) * 100, 0) + "%");
                        }
                    }
                    context.GeothermalParameter.First(gp => gp.Id == foreignKey).LastUpdate = DateTime.Now;
                    context.SaveChanges();
                    break;

                case Geometry_Type.point:
                    Console.WriteLine("Transfering data to database");
                    foreach (var feature in json?.Features ?? throw new Exception("DatabaseUpdater: feature not found!"))
                    {
                        i++;
                        var coordinates = feature?.Geometry?.Coordinates;

                        if (coordinates != null)
                        {
                            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: espgNumber);
                            var point = geometryFactory.CreatePoint(new Coordinate(coordinates[0][0][0], coordinates[0][0][1]));

                            var newGeoDatum = new DataModel.Database.GeoDatum
                            {
                                Id = 0,
                                Geom = point,
                                ParameterKey = foreignKey,
                                Parameter = JsonSerializer.Serialize(feature?.Properties)
                            };
                            context.GeoData.Add(newGeoDatum);
                        }
                        if (i % 1000 == 0)
                        {
                            Console.WriteLine(Math.Round((Convert.ToDouble(i) / totalLength) * 100, 0) + "%");
                        }
                    }
                    context.GeothermalParameter.First(gp => gp.Id == foreignKey).LastUpdate = DateTime.Now;
                    context.SaveChanges();
                    break;

                case Geometry_Type.raster:
                    throw new Exception("Raster Data is currently not supportet");
                default:
                    throw new Exception("Given Geometry_Type is NOT supportet!");
            }

            transaction.Commit();
            Console.WriteLine("Database Updated!");
        }

        [GeneratedRegex("::")]
        private static partial Regex ESPGRegexShrink();

        [GeneratedRegex("(\\d+)")]
        private static partial Regex ESPGRegexNumber();
    }
}

//using var transaction = context.Database.BeginTransaction();
//transaction.Commit();

//var x = context.GeothermalParameter.First(gp => gp.Id == 1);

//var testlist = context.GeothermalParameter.Where(gp => gp.Id == 1); <- Get list of all Paremeters

// <- Converting Chars to list to later execute them
//var test = context.GeothermalParameter.ToList().Where(gp => gp.Id == 1);
//var a = test.First().Srid;

//x.Id = 0;
//context.GeothermalParameter.Add(x);

//var firstDbEntry = context.GeoData.First(p => p.Id == 1);
//var geo1 = firstDbEntry.Geom;
//var geo2 = firstDbEntry.Geom;
//var result = geo1.Intersects(geo2);