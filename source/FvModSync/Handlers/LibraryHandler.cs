namespace FVModSync.Handlers
{
    using FVModSync.Configuration;
    using FVModSync.Extensions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class LibraryHandler
    {
        private static readonly Dictionary<string, Dictionary<string, string>> libraryOfEverything = new Dictionary<string, Dictionary<string, string>>();
        private static readonly Dictionary<string, Dictionary<string, bool>> libraryOfModdedBits = new Dictionary<string, Dictionary<string, bool>>();

        public static void InitTable(string internalName)
        {
            string exportedFilePath = Config.ExportFolderName + internalName;
            string gameFilePath = Config.GameFilePrefix + internalName;

            if (File.Exists(gameFilePath))
            {
                Console.WriteLine();
                Console.WriteLine("Init {0} from game files ...", internalName);
                CopyFileToDict(gameFilePath, internalName);
            }
            else
            {
                if (!File.Exists(exportedFilePath))
                {
                    throw new FileNotFoundException("Exported CSV file not found. Try deleting the FVModSync_exportedFiles folder and running the program again.", exportedFilePath);
                }
                Console.WriteLine();
                Console.WriteLine("Init {0} from exported files ...", internalName);
                CopyFileToDict(exportedFilePath, internalName);
            }
        }

        public static void CopyFileToDict(string sourceFilePath, string internalName)
        {
            if (!TableExists(internalName))
            {
                libraryOfEverything.Add(internalName, new Dictionary<string, string>());
                libraryOfModdedBits.Add(internalName, new Dictionary<string, bool>());
                InitTable(internalName);
            }
            Console.WriteLine("Add to {0}: content from {1} ...", internalName, sourceFilePath);

            using (Stream csvStream = File.Open(sourceFilePath, FileMode.Open))
            {
                StreamReader reader = new StreamReader(csvStream);
                string header = reader.ReadLine();
                string content = reader.ReadToEnd();
                
                if (!libraryOfEverything[internalName].TryGetValue("fvs_header", out header))
                {
                    libraryOfEverything[internalName].Add("fvs_header", header);
                }

                string[] contentLines = content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string contentLine in contentLines)
                {
	                string cleanLine = contentLine.RemoveTabs();
                    string key = GetKey(internalName, cleanLine);

					if (IsDirty(internalName, key))
					{
						Console.WriteLine();
						Console.WriteLine("CONFLICT: Record \"{0}\" already exists", key);
					}

	                Dictionary<string, string> record = libraryOfEverything.GetOrAdd(internalName, () => new Dictionary<string, string>());

                    record.SetValue(key, cleanLine);

                    if (sourceFilePath.StartsWith(Config.ModsSubfolderName))
                    {
                        LibraryHandler.SetDirty(internalName, key);
                    }
                }
            }
        }



        public static void CreateGameFilesFromLibrary()
        {
            string[] internalNames = libraryOfEverything.Keys.ToArray();

            foreach (string internalName in internalNames) 
            {
                string gameFilePath = Config.GameFilePrefix + internalName;
                GenericFileHandler.BackupIfExists(gameFilePath);

                string targetDir = Config.GameFilePrefix + @"\" + Path.GetDirectoryName(internalName);
                Directory.CreateDirectory(targetDir);

                using (Stream gameFile = File.Open(Config.GameFilePrefix + internalName, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(gameFile))
                    {
                        string[] contentLines = libraryOfEverything[internalName].Values.ToArray();

                        foreach (string contentLine in contentLines)
                        {
                            writer.WriteLine(contentLine);
                        }
                    }
                }
                Console.WriteLine("Write dictionary to game files: {0}", internalName);
            }
        }

        private static bool TableExists(string internalName)
	    {
		    return libraryOfEverything.ContainsKey(internalName);
	    }

        private static string GetKey(string internalName, string inputLine)
        {
            string key;

            // TODO handle other stuff with multiple identical keys (like LOD.csv etc; removed from config for now)

            if (internalName.EndsWith("dress.csv"))
            {
                string[] keycombo = inputLine.Split(',').Take(2).ToArray();
                key = String.Join("+", keycombo);
            }
            else
            {
                key = inputLine.Split(',').First();
            }
            return key;
        }

        private static bool IsDirty(string internalName, string key)
        {
            return TableExists(internalName)
                   && libraryOfModdedBits[internalName].ContainsKey(key)
                   && libraryOfModdedBits[internalName][key];
        }

        private static void SetDirty(string internalName, string key)
        {
            Dictionary<string, bool> moddedRecords = libraryOfModdedBits.GetOrAdd(internalName, () => new Dictionary<string, bool>());
            moddedRecords.SetValue(key, true);
        }
    }
}