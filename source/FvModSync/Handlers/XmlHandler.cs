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
        private static readonly Dictionary<string, XDocument> xmlDocs = new Dictionary<string, XDocument>();

        public static void InitXml(string internalName)
        {
            GenericFileHandler.Init(AddToXml, internalName);
        }

        public static void AddToXml(string sourceFilePath, string internalName)
        {
            if (ExternalConfig.ConsoleVerbosity != "quiet")
            {
                Console.WriteLine();
                Console.WriteLine("Parse {0} ...", sourceFilePath);
            }

            if (!xmlDocs.ContainsKey(internalName))
            {
                xmlDocs.Add(internalName, new XDocument());

                if (ExternalConfig.schemeFiles.Contains(internalName))
                {
                    InitXml(internalName);
                }
            }

            XDocument xmlDoc = xmlDocs[internalName];
            XDocument xmlExternalFile = XDocument.Load(sourceFilePath);

            var externalFileRoot = xmlExternalFile.Root;
            var externalFileElements = externalFileRoot.Descendants();

            if (xmlDoc.Root == null)
            {
                xmlDoc.Add(externalFileRoot);
                xmlDoc.Root.RemoveNodes();
            }

            var xmlDocElements = xmlDoc.Root.Descendants();

            bool nodeExists = false;

            foreach (XNode enode in externalFileElements) 
            {
                foreach (XNode inode in xmlDocElements) 
                {
                    if (XNode.DeepEquals(enode, inode)) 
                    {
                        nodeExists = true;
                    }
                }
                if (!nodeExists)
                {
                    xmlDoc.Root.Add(enode);
                }
            }


            if (ExternalConfig.ConsoleVerbosity != "quiet")
            {
                Console.WriteLine("Add to {0}: {1}", internalName, sourceFilePath);
            } 
        }

        public static void CreateFilesFromXml()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("	");

            foreach (KeyValuePair<string, XDocument> xmlscheme in xmlDocs)
            {
                var internalName = xmlscheme.Key;
                var xmlFile = xmlscheme.Value;

                string gameFilePath = ExternalConfig.GameFilesPrefix + @"\" + internalName;
                GenericFileHandler.BackupIfExists(gameFilePath);

                string targetDir = ExternalConfig.GameFilesPrefix + @"\" + Path.GetDirectoryName(internalName);
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