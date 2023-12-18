using System.Xml.Linq;

namespace GERMAG.Server.DataPulling
{
    public interface IDatabaseUpdater
    {
        bool updateDatabase(string url, String type);
    }

    public class DatabaseUpdater : IDatabaseUpdater
    {
        public bool updateDatabase(string url, String type)
        {
            try
            {
                var xml = XDocument.Load(url);

                // <- Save downloaded Data in a Database here


                // For testing, before a DB is connected: Saving of the GML 
                var savePath = Directory.GetCurrentDirectory() + "..\\..\\Resources\\Test.gml";
                Directory.CreateDirectory(Path.GetDirectoryName(savePath) ?? "");

                if (savePath != null)
                {
                    xml.Save(savePath);
                }
                else
                {
                    // Do Something
                }


                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
