namespace FVModSync.Configuration
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    public class ConfigReader
    {
        private const string ConfigFileName = "FVModSync_Configuration.xml";

        public static void InitConfig()
        {
            if (!File.Exists(ConfigFileName))
            {
                Console.WriteLine(ConfigFileName + " not found; using default values");
            }
            else
            {
                XDocument xconfig = XDocument.Load(ConfigFileName);

                ExternalConfig.GameVersion = xconfig.Root.Element("gameVersion").Value;
                ExternalConfig.GameFilesPrefix = xconfig.Root.Element("gameFilePrefix").Value;
                ExternalConfig.GameFileBackupSuffix = xconfig.Root.Element("gameFileBackupSuffix").Value;
                ExternalConfig.ExportFolderName = xconfig.Root.Element("exportFolderName").Value;
                ExternalConfig.ModsSubfolderName = xconfig.Root.Element("modsSubfolderName").Value;
                ExternalConfig.ModDefaultsSubfolderName = xconfig.Root.Element("modDefaultsFolderName").Value;
                ExternalConfig.ConsoleVerbosity = xconfig.Root.Element("consoleVerbosity").Value;
                ExternalConfig.csvFiles = xconfig.Root.Element("csvFiles").Descendants().Select(e => e.Value).ToList();
                ExternalConfig.schemeFiles = xconfig.Root.Element("schemeFiles").Descendants().Select(e => e.Value).ToList();
            }
        }
    }
}