﻿namespace FVModSync.Handlers
{
    using FVModSync.Configuration;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class ListHandler
    {
        private static readonly Dictionary<string, List<string>> lists = new Dictionary<string, List<string>>();

        public static void InitList(string internalName)
        {
            GenericFileHandler.Init(AddToList, internalName);
        }

        public static void AddToList(string sourceFilePath, string internalName)
        {   
            if (!lists.ContainsKey(internalName)) 
            {
                lists.Add(internalName, new List<string>());
                InitList(internalName); 
            }

            List<string> list = lists[internalName];

            using (Stream stream = File.Open(sourceFilePath, FileMode.Open))
            {
                StreamReader reader = new StreamReader(stream);

                string content = reader.ReadToEnd();
                string[] contentLines = content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string contentLine in contentLines)
                {
                    if (!list.Contains(contentLine))
                    {
                        list.Add(contentLine);
                    }
                }
            }
            Console.WriteLine("Add to list {0}: {1}", internalName, sourceFilePath);
        }

        public static void CreateFilesFromLists()
        {
            foreach (KeyValuePair<string, List<string>> list in lists)
            {
                var internalName = list.Key;
                var listContent = list.Value;

                if (listContent.Any()) // dont write empty arrays
                {
                    string gameFilePath = ExternalConfig.GameFilePrefix + internalName;
                    GenericFileHandler.BackupIfExists(gameFilePath);

                    string targetDir = ExternalConfig.GameFilePrefix + Path.GetDirectoryName(internalName);
                    Directory.CreateDirectory(targetDir);

                    using (Stream gameFileStream = File.Open(gameFilePath, FileMode.Create))
                    {
                        using (StreamWriter writer = new StreamWriter(gameFileStream))
                        {
                            string[] contentLines = listContent.ToArray();

                            foreach (string contentLine in contentLines)
                            {
                                writer.WriteLine(contentLine);
                            }
                        }
                    }
                    Console.WriteLine("Write list to game files: {0}", internalName);
                }
            }
        }
    }
}