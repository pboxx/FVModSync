namespace FVModSync.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using FVModSync.Extensions;
    using FVModSync.Configuration;

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
                Console.WriteLine("Init table {0} from game files ...", internalName);
                CopyFileToNewDict(gameFilePath, internalName);
            }
            else
            {
                if (!File.Exists(exportedFilePath))
                {
                    throw new FileNotFoundException("Exported CSV file not found. Try deleting the FVModSync_exportedFiles folder and running the program again.", exportedFilePath);
                }
                Console.WriteLine("Init table {0} from exported files ...", internalName);
                CopyFileToNewDict(exportedFilePath, internalName);
            }
        }

        public static void CopyFileToNewDict(string sourceFilePath, string internalName)
        {
            using (Stream externalFile = File.Open(sourceFilePath, FileMode.Open))
            {
                StreamReader reader = new StreamReader(externalFile);
                string header = reader.ReadLine();
                string content = reader.ReadToEnd();

                libraryOfEverything.Add(internalName, new Dictionary<string, string>());
                libraryOfModdedBits.Add(internalName, new Dictionary<string, bool>());

                libraryOfEverything[internalName].Add("fvs_header", header);

                string[] contentLines = content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string contentLine in contentLines)
                {
                    string key = GetKey(internalName, contentLine);

                    libraryOfEverything[internalName].Add(key, contentLine);
                    libraryOfModdedBits[internalName].Add(key, false);
                }
            }
            Console.WriteLine("CopyFileToNewDict {0}: content from {1}", internalName, sourceFilePath);
        }


        public static void CopyModdedFileToDict(string csvModdedFilePath)
        {
	        string internalName = csvModdedFilePath.GetInternalName();

            if (!TableExists(internalName))
            {
                InitTable(internalName);
            }

            using (Stream csvStream = File.Open(csvModdedFilePath, FileMode.Open))
            {
                StreamReader reader = new StreamReader(csvStream);
                reader.ReadLine(); // skip the header

                string content = reader.ReadToEnd();
                string[] contentLines = content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string contentLine in contentLines)
                {
	                string cleanLine = contentLine.RemoveTabs();
                    string key = GetKey(internalName, cleanLine);

					if (IsDirty(internalName, key))
					{
						Console.WriteLine();
						Console.WriteLine("CONFLICT: Record \"{0}\" from {1} already exists", key, csvModdedFilePath);
						Console.WriteLine();
					}

	                Dictionary<string, string> library = libraryOfEverything.GetOrAdd(internalName, () => new Dictionary<string, string>());

                    library.SetValue(key, cleanLine);
					LibraryHandler.SetDirty(internalName, key);
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