namespace FVModSync.Configuration
{
    using System.Xml.Schema;
    using System.Xml.Serialization;

    public class Config
    {
        public const string Version = "FVModSync v0.2beta\nMod installer for Life is Feudal: Forest Village\n(c) pbox 2016\nPublished under the GPLv3 https://www.gnu.org/licenses/gpl-3.0.txt \n";
        public const string GameFilePrefix = @"..";
        public const string GameFileBackupSuffix = ".backup";
        public const string ExportFolderName = "FVModSync_exportedFiles";
        public const string ModsSubfolderName = "mods";
        public const string InternalLuaIncludePath = @"\scripts\include.lua";
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