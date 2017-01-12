namespace FVModSync.Handlers
{
    using FVModSync.Configuration;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;

    public class XmlHandler
    {
        private static readonly Dictionary<string, XDocument> schemes = new Dictionary<string, XDocument>();

        public static void InitScheme(string internalName)
        {
            GenericFileHandler.Init(AddToScheme, internalName);
        }

        public static void AddToScheme(string sourceFilePath, string internalName)
        {

            if (!schemes.ContainsKey(internalName))
            {
                schemes.Add(internalName, new XDocument());
                InitScheme(internalName);
            }

            XDocument scheme = schemes[internalName];
            XDocument xmlFile = XDocument.Load(sourceFilePath);

            var fileRoot = xmlFile.Root;
            var fileElements = fileRoot.Descendants();

            if (scheme.Root == null)
            {
                scheme.Add(fileRoot);
                scheme.Root.RemoveNodes();
            }

            scheme.Root.Add(fileElements);

            Console.WriteLine("Add to scheme {0}: {1}", internalName, sourceFilePath);
        }

        public static void CreateFilesFromXml()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("	");

            foreach (KeyValuePair<string, XDocument> xmlscheme in schemes)
            {
                var internalName = xmlscheme.Key;
                var xmlFile = xmlscheme.Value;

                string gameFilePath = ExternalConfig.GameFilePrefix + internalName;
                GenericFileHandler.BackupIfExists(gameFilePath);

                string targetDir = ExternalConfig.GameFilePrefix + Path.GetDirectoryName(internalName);
                Directory.CreateDirectory(targetDir);

                using (XmlWriter writer = XmlWriter.Create(gameFilePath, settings))
                {
                    xmlFile.Save(writer);
                }
                Console.WriteLine("Write XML to game files: {0}", internalName);             
            }
        }
    }
}