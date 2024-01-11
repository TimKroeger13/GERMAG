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

namespace GERMAG.Server.DataPulling
{
    public interface IDatabaseUpdater
    {
        void UpdateDatabase(Root json, int ForeignKey);
    }

    public partial class DatabaseUpdater(DataContext context) : IDatabaseUpdater
    {
        public void UpdateDatabase(Root json, int ForeignKey)
        {
            var espgStringRaw = json.crs!.properties!.name;
            var espgString = epsgRegex().Replace(espgStringRaw!, "");
            var espgNumber = Int32.Parse(espgString);

            using var transaction = context.Database.BeginTransaction();

            var entriesToRemove = context.GeoData.Where(g => g.ParameterKey == ForeignKey);
            context.GeoData.RemoveRange(entriesToRemove);
            context.SaveChanges();
            // F_FEATURE - Implment reseeding so th id dosen't grow infintly

            context.GeothermalParameter.First(gp => gp.Id == ForeignKey).Srid = espgNumber;

                    foreach (var feature in json?.features ?? throw new Exception("DatabaseUpdater: feature not found!"))
                {
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
                        ParameterKey = ForeignKey
                    };
                    context.GeoData.Add(newGeoDatum);
                }
            }

            context.SaveChanges();
            transaction.Commit();
        }

        [GeneratedRegex("EPSG:")]
        private static partial Regex epsgRegex();
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
