using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace FVModSync
{
    public class DictHandler
    {
        private static readonly Dictionary<string, Dictionary<string, string>> libraryOfEverything = new Dictionary<string, Dictionary<string, string>>();
        private static readonly Dictionary<string, Dictionary<string, bool>> libraryOfModdedBits = new Dictionary<string, Dictionary<string, bool>>();

        public static void SetDirty(string csvIntPath, string key)
        {
            Dictionary<string, bool> moddedRecords;
            if (!libraryOfModdedBits.TryGetValue(csvIntPath, out moddedRecords))
            {
                moddedRecords = new Dictionary<string, bool>();
                libraryOfModdedBits.Add(csvIntPath, moddedRecords);
            }

            if (moddedRecords.ContainsKey(key))
            {
                moddedRecords[key] = true;
            }
            else
            {
                moddedRecords.Add(key, true);
            }
        }

        public static void BackupAndCopy(string csvIntPath, string ExportFolder)
        {
            string gameCsvFilePath = @".." + csvIntPath;
            string exportedCsvFilePath = ExportFolder + csvIntPath;

            if (File.Exists(gameCsvFilePath))
            {
                string backupFilePath = gameCsvFilePath + ".backup";

                File.Delete(backupFilePath);
                File.Copy(gameCsvFilePath, backupFilePath);

                Console.WriteLine("File exists: {0} -- create backup on disk", gameCsvFilePath);

                // copy existing game file to internal dictionary
                DictHandler.CopyFileToDict(gameCsvFilePath, csvIntPath);
            }
            else
            {
                // copy from exported files

                if (!File.Exists(exportedCsvFilePath))
                {
                    throw new FileNotFoundException("Exported CSV file not found. Try deleting the FVModSync_exportedFiles folder and running the program again.", exportedCsvFilePath);
                }
                            
                DictHandler.CopyFileToDict(exportedCsvFilePath, csvIntPath);
            }
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
            int relevantPathStart = csvModdedFilePath.IndexOf('\\', csvModdedFilePath.IndexOf('\\') + 1); // this is obscene
            string csvIntPath = csvModdedFilePath.Substring(relevantPathStart);

            using (Stream csvStream = File.Open(csvModdedFilePath, FileMode.Open))
            {
                StreamReader reader = new StreamReader(csvStream);
                reader.ReadLine(); // skip the header

                string content = reader.ReadToEnd();
                string[] contentLines = content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string contentLine in contentLines)
                {
                    // use first field as dict key
                    string cleanLine = Regex.Replace(contentLine, @"\t", "");
                    string key = cleanLine.Split(',').First();

                    if (libraryOfEverything[csvIntPath].ContainsKey(key))
                    {
                        // overwrite existing line in CSV
                        libraryOfEverything[csvIntPath][key] = cleanLine;

                        if (libraryOfModdedBits.ContainsKey(csvIntPath) && libraryOfModdedBits[csvIntPath].ContainsKey(key))
                            if (libraryOfModdedBits[csvIntPath][key] == true)
                            {
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("CONFLICT: Entry \"{0}\" from {1} already exists", key, csvModdedFilePath);
                                    Console.WriteLine();
                                }
                            }
                        DictHandler.SetDirty(csvIntPath, key);
                    }
                    else
                    {
                        // add new line to CSV
                        libraryOfEverything[csvIntPath].Add(key, cleanLine);
                    }
                }
            }
        }
      
        public static bool DictExists(string csvIntPath)
        {
            if (libraryOfEverything.ContainsKey(csvIntPath))
            {
                return true;
            }
            return false;
        }
        

        public static void CreateGameFileFromDict(string csvIntPath)
        {
            if (libraryOfModdedBits.ContainsKey(csvIntPath)) 
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
    }
}