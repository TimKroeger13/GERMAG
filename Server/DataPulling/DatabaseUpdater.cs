using GERMAG.DataModel.Database;
using NetTopologySuite.Geometries;
using System.Xml.Linq;

namespace GERMAG.Server.DataPulling
{
    public interface IDatabaseUpdater
    {
        void updateDatabase(XDocument xml, string savePath);
    }

    public class DatabaseUpdater(DataContext context) : IDatabaseUpdater 
    {
        public void updateDatabase(XDocument xml, String savePath)
        {

            xml.Save(savePath);
            var xmlString = xml.ToString();
            var envelope = Envelope.Parse(xmlString);
            context.GeothermalParameter.First().Geometry = Geometry.DefaultFactory.ToGeometry(envelope);
            context.SaveChanges();

        }
    }
}
