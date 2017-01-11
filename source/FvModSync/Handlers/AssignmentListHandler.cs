namespace FVModSync.Handlers
{
    using FVModSync.Configuration;
    using FVModSync.Extensions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class AssignmentListHandler
    {
        private static readonly Dictionary<string, Dictionary<string, string>> assignmentLists = new Dictionary<string, Dictionary<string, string>>();

        public static void InitList(string internalName)
        {
            string exportedFilePath = Config.ExportFolderName + internalName;
            string gameFilePath = Config.GameFilePrefix + internalName;

            if (File.Exists(gameFilePath))
            {
                Console.WriteLine();
                Console.WriteLine("Init list from game files: {0} ...", internalName);
                AddToList(gameFilePath, internalName);
            }
            else
            {
                if (!File.Exists(exportedFilePath))
                {
                    throw new FileNotFoundException("Exported file {0} not found. Try deleting the FVModSync_exportedFiles folder and running the program again", exportedFilePath);
                }
                Console.WriteLine();
                Console.WriteLine("Init assignment list from exported files: {0} ...", internalName);
                AddToList(exportedFilePath, internalName);
            }
        }

        public static void AddToList(string sourceFilePath, string internalName)
        {
            if (!assignmentLists.ContainsKey(internalName))
            {
                assignmentLists.Add(internalName, new Dictionary<string, string>());
                InitList(internalName);
            }

            Dictionary<string, string> assignmentList = assignmentLists[internalName];

            using (Stream stream = File.Open(sourceFilePath, FileMode.Open))
            {
                StreamReader reader = new StreamReader(stream);

                string content = reader.ReadToEnd();
                string[] contentLines = content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string contentLine in contentLines)
                {
                    if (!contentLine.Trim().Equals("}"))
                    {
                        var parts = contentLine.Split('=');
                        var key = parts[0].Trim();
                        if (parts.Length > 1 && key != "config")
                        {
                            var val = parts[1].Trim();
                            assignmentList.SetValue(key, val);
                        }
                    }
                }
            }
            Console.WriteLine("Add to assignment list {0}: {1}", internalName, sourceFilePath);
        }

        public static void CreateFilesFromLists()
        {
            foreach (KeyValuePair<string, Dictionary<string, string>> assignmentList in assignmentLists)
            {
                var internalName = assignmentList.Key;
                var listEntries = assignmentList.Value;

                if (listEntries.Any()) // dont write empty lists
                {
                    string gameFilePath = Config.GameFilePrefix + internalName;
                    GenericFileHandler.BackupIfExists(gameFilePath);

                    string targetDir = Config.GameFilePrefix + Path.GetDirectoryName(internalName);
                    Directory.CreateDirectory(targetDir);

                    using (Stream gameFileStream = File.Open(gameFilePath, FileMode.Create))
                    {
                        using (StreamWriter writer = new StreamWriter(gameFileStream))
                        {
                            writer.WriteLine("config =");
                            writer.WriteLine("{");
                            foreach (KeyValuePair<string, string> listEntry in listEntries)
                            {
                                writer.WriteLine("    " + listEntry.Key + " = " + listEntry.Value);
                            }
                            writer.WriteLine("}");
                        }
                    }
                    Console.WriteLine("Write assignment list to game files: {0}", internalName);
                }
            }
        }
    }
}