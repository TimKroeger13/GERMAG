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

            var x = context.GeothermalParameter.First(gp => gp.Id == 1);

            context.SaveChanges();

        }

        [GeneratedRegex("EPSG:")]
        private static partial Regex epsgRegex();
    }
}
