namespace FVModSync.Handlers
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
            GenericFileHandler.Init(AddFileContentsToList, internalName);
        }

        public static List<string> GetList(string internalName)
        {
            if (!lists.ContainsKey(internalName))
            {
                lists.Add(internalName, new List<string>());
                InitList(internalName);
            }
            List<string> list = lists[internalName];
            return list;
        }

        public static void AddEntryToList(string internalName, string entry)
        {
            List<string> list = GetList(internalName);

            if (!list.Contains(entry))
            {
                list.Add(entry);

                if (ExternalConfig.ConsoleVerbosity != "quiet")
                {
                    Console.WriteLine("Add entry to {0}: {1}", internalName, entry);
                } 
            }
        }

        public static void AddFileContentsToList(string sourceFilePath, string internalName)
        {
            List<string> list = GetList(internalName);

            using (Stream stream = File.Open(sourceFilePath, FileMode.Open))
            {
                StreamReader reader = new StreamReader(stream);

                string content = reader.ReadToEnd();
                string[] contentLines = content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                //// 0.9.6109 hotfix -- http://steamcommunity.com/groups/ForestVillageModding/discussions/0/135511027313884138/#c135511455867199810
                //
                //string expInitFile = ExternalConfig.ExportFolderName + @"\" + InternalConfig.InternalLuaInitPath;
                //string gameInitFile = ExternalConfig.GameFilePrefix + @"\" + InternalConfig.InternalLuaInitPath;
                //
                //if (sourceFilePath == expInitFile || sourceFilePath == gameInitFile)
                //{
                //    for (int line = 0; line < contentLines.Length; line++)
                //    {
                //        if (contentLines[line] == "for modName in iter(MOD_NAMES:split(\"|\")) do")
                //        {
                //            line = line + 2; // skip rest
                //        }
                //        else
                //        {
                //            list.Add(contentLines[line]);
                //        }
                //    }
                //}
                //else
                //{
                //    foreach (string contentLine in contentLines)
                //    {
                //        if (!list.Contains(contentLine))
                //        {
                //            list.Add(contentLine);
                //            if (ExternalConfig.ConsoleVerbosity != "quiet")
                //            {
                //                Console.WriteLine("Add to {0}: {1}", internalName, sourceFilePath);
                //            }
                //        }
                //    }
                //}

                foreach (string contentLine in contentLines)
                {
                    if (!list.Contains(contentLine))
                    {
                        list.Add(contentLine);
                        if (ExternalConfig.ConsoleVerbosity != "quiet")
                        {
                            Console.WriteLine("Add to {0}: {1}", internalName, sourceFilePath);
                        }
                    }
                }
            }
        }

        public static void CreateFilesFromLists()
        {
            foreach (KeyValuePair<string, List<string>> list in lists)
            {
                var internalName = list.Key;
                var listContent = list.Value;

                if (listContent.Any()) // dont write empty lists
                {
                    string gameFilePath = ExternalConfig.GameFilesPrefix + @"\" + internalName;
                    GenericFileHandler.BackupIfExists(gameFilePath);

                    string targetDir = ExternalConfig.GameFilesPrefix + @"\" + Path.GetDirectoryName(internalName);
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
                    Console.WriteLine("Write to game files: {0}", internalName);
                }
            }
        }
    }
}