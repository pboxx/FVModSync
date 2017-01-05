namespace FVModSync.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using FVModSync.Configuration;

    public class ListHandler
    {
        private static readonly Dictionary<string, List<string>> lists = new Dictionary<string, List<string>>();

        public static void InitList(string internalName)
        {
            string exportedFilePath = Config.ExportFolderName + internalName;
            string gameFilePath = Config.GameFilePrefix + internalName;

            if (File.Exists(gameFilePath))
            {
                Console.WriteLine("Init list {0} from game files ...", internalName);
                ListHandler.AddToList(internalName, gameFilePath);
            }
            else
            {
                if (!File.Exists(exportedFilePath))
                {
                    throw new FileNotFoundException("Exported file {0} not found. Try deleting the FVModSync_exportedFiles folder and running the program again", exportedFilePath);
                }
                Console.WriteLine("Init list {0} from exported files ...", internalName);
                ListHandler.AddToList(internalName, exportedFilePath);
            }
        }

        public static void AddToList(string internalName, string sourceFile)
        {   
            Console.WriteLine("Add to list {0}: file content from {1} ...", internalName, sourceFile);

            if (!lists.ContainsKey(internalName)) 
            {
                lists.Add(internalName, new List<string>());
                InitList(internalName); 
            }

            List<string> list = lists[internalName];

            using (Stream stream = File.Open(sourceFile, FileMode.Open))
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
        }

        public static void CreateFileFromList(string internalName)
        {
            if (lists.ContainsKey(internalName))
            {
                List<string> list = lists[internalName]; 
                
                if (list.Any()) // dont write empty lists
                {
                    string targetDir = Config.GameFilePrefix + Path.GetDirectoryName(internalName);
                    Directory.CreateDirectory(targetDir);

                    using (Stream gameFile = File.Open(Config.GameFilePrefix + internalName, FileMode.Create))
                    {
                        using (StreamWriter writer = new StreamWriter(gameFile))
                        {
                            string[] contentLines = list.ToArray();

                            foreach (string contentLine in contentLines)
                            {
                                writer.WriteLine(contentLine);
                            }
                        }
                    }
                    Console.WriteLine("Write list to game files: {0}", internalName);
                }
            }
            else 
            {
                Console.WriteLine("No list with name: {0}", internalName);
            }
        }
    }
}