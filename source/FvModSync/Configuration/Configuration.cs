namespace FVModSync.Configuration
{
    using System.Xml.Schema;
    using System.Xml.Serialization;

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