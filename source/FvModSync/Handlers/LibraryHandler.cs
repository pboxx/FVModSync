namespace FVModSync.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using FVModSync.Extensions;

    public class LibraryHandler
    {
        private static readonly Dictionary<string, Dictionary<string, string>> libraryOfEverything = new Dictionary<string, Dictionary<string, string>>();
        private static readonly Dictionary<string, Dictionary<string, bool>> libraryOfModdedBits = new Dictionary<string, Dictionary<string, bool>>();

        public static void BackupAndCopy(string relevantPath, string exportFolder)
        {
            string gameFilePath = @".." + relevantPath;
            string exportedFilePath = exportFolder + relevantPath;

            if (File.Exists(gameFilePath))
            {
                string backupFilePath = gameFilePath + ".backup";

                File.Delete(backupFilePath);
                File.Copy(gameFilePath, backupFilePath);

                Console.WriteLine("File exists: {0} -- create backup on disk", gameFilePath);

                // copy existing game file to internal dictionary
                CopyFileToDict(gameFilePath, relevantPath);
            }
            else
            {
                // copy from exported files
                if (!File.Exists(exportedFilePath))
                {
                    throw new FileNotFoundException("Exported CSV file not found. Try deleting the FVModSync_exportedFiles folder and running the program again.", exportedFilePath);
                }

                CopyFileToDict(exportedFilePath, relevantPath);
            }
        }

        public static void CopyFileToDict(string absolutePath, string relevantPath)
        {

            if (!File.Exists(absolutePath))
            {
                throw new FileNotFoundException("File not found", absolutePath);
            }

            using (Stream externalFile = File.Open(absolutePath, FileMode.Open))
            {
                StreamReader reader = new StreamReader(externalFile);
                string header = reader.ReadLine();
                string content = reader.ReadToEnd();

                libraryOfEverything.Add(relevantPath, new Dictionary<string, string>());
                libraryOfModdedBits.Add(relevantPath, new Dictionary<string, bool>());

                libraryOfEverything[relevantPath].Add("fvs_header", header);

                string[] contentLines = content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string contentLine in contentLines)
                {
                    string key = GetKey(relevantPath, contentLine);

                    libraryOfEverything[relevantPath].Add(key, contentLine);
                    libraryOfModdedBits[relevantPath].Add(key, false);
                }
            }
        }

        public static void CopyModdedFileToDict(string csvModdedFilePath)
        {
	        string relevantPath = csvModdedFilePath.GetRelevantPath();

            using (Stream csvStream = File.Open(csvModdedFilePath, FileMode.Open))
            {
                StreamReader reader = new StreamReader(csvStream);
                reader.ReadLine(); // skip the header

                string content = reader.ReadToEnd();
                string[] contentLines = content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string contentLine in contentLines)
                {
	                string cleanLine = contentLine.RemoveTabs();
                    string key = GetKey(relevantPath, cleanLine);

					if (IsDirty(relevantPath, key))
					{
						Console.WriteLine();
						Console.WriteLine("CONFLICT: Record \"{0}\" from {1} already exists", key, csvModdedFilePath);
						Console.WriteLine();
					}

	                Dictionary<string, string> library = libraryOfEverything.GetOrAdd(relevantPath, () => new Dictionary<string, string>());

                    //won't work because records can be "1,2"
                    //if (cleanLine.RecordLength() == library.First().Value.RecordLength())
                    //{
                    //    library.SetValue(key, cleanLine);
                    //    Console.WriteLine("Copy CSV record to dictionary: \"{0}\" from {1}", key, csvModdedFilePath);
                    //}
                    //else
                    //{
                    //    Console.WriteLine();
                    //    Console.WriteLine("WARNING: Record \"{0}\" from {1} has a different length -- {3} -- than the original record -- {2} -- and was not copied.", key, csvModdedFilePath, library.First().Value.RecordLength(), cleanLine.RecordLength());
                    //    Console.WriteLine();
                    //    Console.WriteLine("cleanLine: {0}", cleanLine);
                    //    Console.WriteLine("orig_head: {0}", library.First().Value);
                    //}

                    library.SetValue(key, cleanLine);
					LibraryHandler.SetDirty(relevantPath, key);
                }
            }
        } 

	    public static void CreateGameFileFromLibrary(string relevantPath)
        {
            if (RecordExists(relevantPath))
            {
                string targetDir = @"..\" + Path.GetDirectoryName(relevantPath);
                Directory.CreateDirectory(targetDir);

                using (Stream gameFile = File.Open(".." + relevantPath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(gameFile))
                    {
                        string[] contentLines = libraryOfEverything[relevantPath].Values.ToArray();

                        foreach(string contentLine in contentLines)
                        {
                            writer.WriteLine(contentLine);
                        }
                    }
                }
                Console.WriteLine("Write dictionary to game files: {0}", relevantPath);
            }
        }

	    public static bool RecordExists(string relevantPath)
	    {
		    return libraryOfEverything.ContainsKey(relevantPath);
	    }

        private static string GetKey(string relevantPath, string inputLine)
        {
            string key;

            // TODO handle other stuff with multiple identical keys (like LOD.csv etc; removed from config for now)

            if (relevantPath.EndsWith("dress.csv"))
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

        private static bool IsDirty(string relevantPath, string key)
        {
            return RecordExists(relevantPath)
                   && libraryOfModdedBits[relevantPath].ContainsKey(key)
                   && libraryOfModdedBits[relevantPath][key];
        }

        private static void SetDirty(string relevantPath, string key)
        {
            Dictionary<string, bool> moddedRecords = libraryOfModdedBits.GetOrAdd(relevantPath, () => new Dictionary<string, bool>());
            moddedRecords.SetValue(key, true);
        }
    }
}