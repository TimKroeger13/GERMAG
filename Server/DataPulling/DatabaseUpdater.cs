using GERMAG.DataModel.Database;
using NetTopologySuite.Geometries;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Text.RegularExpressions;

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


            //using var transaction = context.Database.BeginTransaction();


            var x = context.GeothermalParameter.First(gp => gp.Id == 1);
            //var testlist = context.GeothermalParameter.Where(gp => gp.Id == 1); <- Get list of all Paremeters

            // <- Converting Chars to list to later execute them
            //var test = context.GeothermalParameter.ToList().Where(gp => gp.Id == 1);
            //var a = test.First().Srid;

            //x.Id = 0;
            //context.GeothermalParameter.Add(x);

            context.GeothermalParameter.First(gp => gp.Id == 1).Srid = espgNumber;

            //context.GeothermalParameter.First().Geom = NetTopologySuite.Geometries.Geometry
                
            context.SaveChanges();
            //transaction.Commit();

        }

        [GeneratedRegex("EPSG:")]
        private static partial Regex epsgRegex();
    }
}
