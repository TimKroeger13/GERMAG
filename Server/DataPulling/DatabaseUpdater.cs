using GERMAG.DataModel.Database;
using NetTopologySuite.Geometries;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using NetTopologySuite;
using NetTopologySuite.Algorithm;
using GERMAG.DataModel;
using NetTopologySuite.Operation.Overlay;

namespace GERMAG.Server.DataPulling
{
    public interface IDatabaseUpdater
    {
        void UpdateDatabase(Root json);
    }

    public partial class DatabaseUpdater(DataContext context) : IDatabaseUpdater
    {
        public void UpdateDatabase(Root json)
        {
            var espgStringRaw = json.crs!.properties!.name;
            var espgString = epsgRegex().Replace(espgStringRaw!, "");
            var espgNumber = Int32.Parse(espgString);

            context.GeothermalParameter.First(gp => gp.Id == 1).Srid = espgNumber;

            for (var i = 0; i < 38; i++)
            {
                //foreach (var feature in json?.features ?? throw new Exception("DatabaseUpdater: feature not found!"))
                //{
                i = 36;

                var coordinates = json.features?[i].geometry?.coordinates;
                //var coordiantes = feature?.geometry?.coordinates;

                if (coordinates != null)
                {
                    var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: espgNumber);
                    var linearRing = geometryFactory.CreateLinearRing(coordinates.SelectMany(c => c).Select(coord => new Coordinate(coord[0], coord[1])).ToArray());
                    var polygon = geometryFactory.CreatePolygon(linearRing);

                    var newGeoDatum = new DataModel.Database.GeoDatum
                    {
                        Id = 0,
                        Geom = polygon,
                        ParameterKey = 1
                    };
                    context.GeoData.Add(newGeoDatum);
                }
            }







            /*
            var coordinates = json?.features?.FirstOrDefault()?.geometry?.coordinates;

            if (coordinates != null)
            {
                var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: espgNumber);
                var linearRing = geometryFactory.CreateLinearRing(coordinates.SelectMany(c => c).Select(coord => new Coordinate(coord[0], coord[1])).ToArray());
                var polygon = geometryFactory.CreatePolygon(linearRing);
                context.GeoData.First(gp => gp.Id == 2).Geom = polygon;
            }
            */













            context.SaveChanges();


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
