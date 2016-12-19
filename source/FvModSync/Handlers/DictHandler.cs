namespace FVModSync.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using FVModSync.Extensions;

    public class DictHandler
    {
        private static readonly Dictionary<string, Dictionary<string, string>> libraryOfEverything = new Dictionary<string, Dictionary<string, string>>();
        private static readonly Dictionary<string, Dictionary<string, bool>> libraryOfModdedBits = new Dictionary<string, Dictionary<string, bool>>();

        public static void BackupAndCopy(string csvIntPath, string exportFolder)
        {
            string gameCsvFilePath = @".." + csvIntPath;
            string exportedCsvFilePath = exportFolder + csvIntPath;

            if (File.Exists(gameCsvFilePath))
            {
                string backupFilePath = gameCsvFilePath + ".backup";

                File.Delete(backupFilePath);
                File.Copy(gameCsvFilePath, backupFilePath);

                Console.WriteLine("File exists: {0} -- create backup on disk", gameCsvFilePath);

                // copy existing game file to internal dictionary
                CopyFileToDict(gameCsvFilePath, csvIntPath);
            }
            else
            {
                // copy from exported files
                if (!File.Exists(exportedCsvFilePath))
                {
                    throw new FileNotFoundException("Exported CSV file not found. Try deleting the FVModSync_exportedFiles folder and running the program again.", exportedCsvFilePath);
                }

                CopyFileToDict(exportedCsvFilePath, csvIntPath);
            }
        }

        private static void SetDirty(string csvIntPath, string key)
        {
            Dictionary<string, bool> moddedRecords = libraryOfModdedBits.GetOrAdd(csvIntPath, () => new Dictionary<string, bool>());
            moddedRecords.SetValue(key, true);
        }

        public static void CopyFileToDict(string csvAbsPath, string csvIntPath)
        {
            // TODO throw some error if csvExtFile not found
            // TODO handle stuff with multiple identical keys (like cfg/dress.csv, LOD.csv; removed from config for now)

            if (!File.Exists(csvAbsPath))
            {
                throw new FileNotFoundException("File not found", csvAbsPath);
            }

            using (Stream csvExtFile = File.Open(csvAbsPath, FileMode.Open))
            {
                StreamReader reader = new StreamReader(csvExtFile);
                string header = reader.ReadLine();
                string content = reader.ReadToEnd();

                libraryOfEverything.Add(csvIntPath, new Dictionary<string, string>());
                libraryOfModdedBits.Add(csvIntPath, new Dictionary<string, bool>());

                libraryOfEverything[csvIntPath].Add("fvs_header", header);

                string[] contentLines = content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string contentLine in contentLines)
                {
                    string key = contentLine.Split(',').First();
                    libraryOfEverything[csvIntPath].Add(key, contentLine);
                    libraryOfModdedBits[csvIntPath].Add(key, false);
                }
            }
        }

        public static void CopyModdedFileToDict(string csvModdedFilePath)
        {
	        string csvIntPath = csvModdedFilePath.GetRelevantPath();

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

					if (IsDirty(csvIntPath, key))
					{
						Console.WriteLine();
						Console.WriteLine("CONFLICT: Entry \"{0}\" from {1} already exists", key, csvModdedFilePath);
						Console.WriteLine();
					}

	                Dictionary<string, string> library = libraryOfEverything.GetOrAdd(csvIntPath, () => new Dictionary<string, string>());
					library.SetValue(key, cleanLine);
					DictHandler.SetDirty(csvIntPath, key);
                }
            }
        }

	    public static void CreateGameFileFromLibrary(string csvIntPath)
        {
            if (RecordExists(csvIntPath))
            {
                string targetDir = @"..\" + Path.GetDirectoryName(csvIntPath);
                Directory.CreateDirectory(targetDir);

                using (Stream gameFile = File.Open(".." + csvIntPath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(gameFile))
                    {
                        string[] contentLines = libraryOfEverything[csvIntPath].Values.ToArray();

                        foreach(string contentLine in contentLines)
                        {
                            writer.WriteLine(contentLine);
                        }
                    }
                }
                Console.WriteLine("Write dictionary to game files: {0}", csvIntPath);
            }
        }

	    private static bool IsDirty(string csvIntPath, string key)
	    {
		    return RecordExists(csvIntPath)
				   && libraryOfModdedBits[csvIntPath].ContainsKey(key)
				   && libraryOfModdedBits[csvIntPath][key];
	    }

	    public static bool RecordExists(string csvIntPath)
	    {
		    return libraryOfEverything.ContainsKey(csvIntPath);
	    }
    }
}