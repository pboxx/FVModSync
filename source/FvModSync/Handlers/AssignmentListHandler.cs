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
            GenericFileHandler.Init(AddToAssignmentList, internalName);
        }

        public static void AddToAssignmentList(string sourceFilePath, string internalName)
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

                if (listEntries.Any())
                {
                    string gameFilePath = ExternalConfig.GameFilePrefix + internalName;
                    GenericFileHandler.BackupIfExists(gameFilePath);

                    string targetDir = ExternalConfig.GameFilePrefix + Path.GetDirectoryName(internalName);
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