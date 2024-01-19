using GERMAG.DataModel.Database;
using NetTopologySuite.Geometries;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using NetTopologySuite;
using NetTopologySuite.Algorithm;
using GERMAG.DataModel;
using NetTopologySuite.Operation.Overlay;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Runtime.ExceptionServices;
using GERMAG.Server.DataPulling.JsonDeserialize;
using System.Linq;
using System;
using NetTopologySuite.IO;

namespace GERMAG.Server.DataPulling
{
    public interface IDatabaseUpdater
    {
        void UpdateDatabase(Root json, int ForeignKey, Geometry_Type Geometry_Type);
    }

    public partial class DatabaseUpdater(DataContext context) : IDatabaseUpdater
    {
        public void UpdateDatabase(Root json, int ForeignKey, Geometry_Type CurrentGeometryType)
        {
            using var transaction = context.Database.BeginTransaction();

            var espgStringRaw = json.crs!.properties!.name;
            var espgStringRegex = ESPGRegexNumber().Match(ESPGRegexShrink().Replace(espgStringRaw ?? "0", ":"));
            var espgString = espgStringRegex.Value;

            var espgNumber = Int32.Parse(espgString);

            var entriesToRemove = context.GeoData.Where(g => g.ParameterKey == ForeignKey);
            context.GeoData.RemoveRange(entriesToRemove);
            context.SaveChanges();
            // F_FEATURE - Implment reseeding so th id dosen't grow infintly

            context.GeothermalParameter.First(gp => gp.Id == ForeignKey).Srid = espgNumber;
            var i = 0;
            var totalLength = 0;

            switch (CurrentGeometryType)
            {
                case Geometry_Type.polygon:
                    Console.WriteLine("Transfering data to database");
                    foreach (var feature in json?.features ?? throw new Exception("DatabaseUpdater: feature not found!"))
                    {
                        i++;
                        var coordinates = feature?.geometry?.coordinates;

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
                                ParameterKey = ForeignKey,
                                Parameter = JsonSerializer.Serialize(feature?.properties)
                            };
                            context.GeoData.Add(newGeoDatum);
                        }
                        if (i % 1000 == 0)
                        {
                            Console.WriteLine(Math.Round((Convert.ToDouble(i) / totalLength) * 100, 0) + "%");
                        }
                    }
                    context.GeothermalParameter.First(gp => gp.Id == ForeignKey).LastUpdate = DateTime.Now;
                    context.SaveChanges();
                    break;
                case Geometry_Type.polyline:
                    Console.WriteLine("Transfering data to database");
                    foreach (var feature in json?.features ?? throw new Exception("DatabaseUpdater: feature not found!"))
                    {
                        i++;
                        var coordinates = feature?.geometry?.coordinates;

                        if (coordinates != null)
                        {
                            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: espgNumber);
                            var polyline = geometryFactory.CreateLineString(coordinates[0].Select(coord => new Coordinate(coord[0], coord[1])).ToArray());

                            var newGeoDatum = new DataModel.Database.GeoDatum
                            {
                                Id = 0,
                                Geom = polyline,
                                ParameterKey = ForeignKey,
                                Parameter = JsonSerializer.Serialize(feature?.properties)
                            };
                            context.GeoData.Add(newGeoDatum);
                        }
                        if (i % 1000 == 0)
                        {
                            Console.WriteLine(Math.Round((Convert.ToDouble(i) / totalLength) * 100, 0) + "%");
                        }
                    }
                    context.GeothermalParameter.First(gp => gp.Id == ForeignKey).LastUpdate = DateTime.Now;
                    context.SaveChanges();
                    break;
                case Geometry_Type.raster:
                    throw new Exception("Raster Data is currently not supportet");
                default:
                    throw new Exception ("Given Geometry_Type is NOT supportet!");
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
