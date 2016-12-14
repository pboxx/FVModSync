using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace FVModSync
{
    public class ConfigReader
    {
        public static string[] LoadCsvPaths()
        {
            string xmlFilePath = "FVModSync.cfg";
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Configuration));
            using (XmlReader xmlReader = XmlReader.Create(xmlFilePath))
            {
                Configuration configuration = (Configuration)xmlSerializer.Deserialize(xmlReader);
                
                // TODO[pb] validate game version 
                
                return configuration.FileLocations;
            }
        }

        [XmlRoot(ElementName = "configuration")]
        public sealed class Configuration
        {
            [XmlElement(ElementName = "gameVersion", Form = XmlSchemaForm.Unqualified)]
            public string GameVersion { get; set; }

            [XmlArray(ElementName = "fileLocations", Form = XmlSchemaForm.Unqualified)]
            [XmlArrayItem("fileLocation", typeof(string))]
            public string[] FileLocations { get; set; }
        }
    }
}
