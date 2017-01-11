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
                throw new FileNotFoundException("FVModSync.cfg not found");
            }

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExternalConfig));
            using (XmlReader xmlReader = XmlReader.Create(ConfigFileName))
            {
                ExternalConfig configuration = (ExternalConfig)xmlSerializer.Deserialize(xmlReader);
                return configuration.FileLocations;
            }
        }
    }
}