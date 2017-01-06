namespace FVModSync.Configuration
{
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    public class ConfigReader
    {
        private const string ConfigFileName = "FVModSync.cfg";

        public static string[] LoadCsvPaths()
        {
            if (!File.Exists(ConfigFileName))
            {
                throw new FileNotFoundException("Configuration file not found", ConfigFileName);
            }

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Configuration));
            using (XmlReader xmlReader = XmlReader.Create(ConfigFileName))
            {
                Configuration configuration = (Configuration)xmlSerializer.Deserialize(xmlReader);
                return configuration.FileLocations;
            }
        }
    }
}