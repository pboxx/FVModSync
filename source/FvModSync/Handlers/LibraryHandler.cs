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

        private static void SetDirty(string relevantPath, string key)
        {
            Dictionary<string, bool> moddedRecords = libraryOfModdedBits.GetOrAdd(relevantPath, () => new Dictionary<string, bool>());
            moddedRecords.SetValue(key, true);
        }

        public static void CopyFileToDict(string absolutePath, string relevantPath)
        {
            // TODO throw some error if externalFile not found
            // TODO handle stuff with multiple identical keys (like cfg/dress.csv, LOD.csv; removed from config for now)

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
                    string key = contentLine.Split(',').First();
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
                    string key = cleanLine.FirstEntryFromRecord();

					if (IsDirty(relevantPath, key))
					{
						Console.WriteLine();
						Console.WriteLine("CONFLICT: Entry \"{0}\" from {1} already exists", key, csvModdedFilePath);
						Console.WriteLine();
					}

	                Dictionary<string, string> library = libraryOfEverything.GetOrAdd(relevantPath, () => new Dictionary<string, string>());
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

	    private static bool IsDirty(string relevantPath, string key)
	    {
		    return RecordExists(relevantPath)
				   && libraryOfModdedBits[relevantPath].ContainsKey(key)
				   && libraryOfModdedBits[relevantPath][key];
	    }

	    public static bool RecordExists(string relevantPath)
	    {
		    return libraryOfEverything.ContainsKey(relevantPath);
	    }
    }
}