using GERMAG.DataModel.Database;
using NetTopologySuite.Geometries;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace GERMAG.Server.DataPulling
{
    public interface IDatabaseUpdater
    {
        void updateDatabase(Root json);
    }

    public class DatabaseUpdater(DataContext context) : IDatabaseUpdater
    {
        public void updateDatabase(Root json)
        {
            var espgStringRaw = json.crs.properties.name;
            var espgString = Regex.Replace(espgStringRaw, "EPSG:", "");
            var espgNumber = Int32.Parse(espgString);


            var x = context.GeothermalParameter.First().Srid;

            context.GeothermalParameter.First().Srid = 9999;
            context.SaveChanges();

            var b = 3;

            //var xmlString = xml.ToString();
            //var envelope = Envelope.Parse(xmlString);
            //context.GeothermalParameter.First().Geom = Geometry.DefaultFactory.ToGeometry(envelope);
            //context.SaveChanges();
        }
    }
}
