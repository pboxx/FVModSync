using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace FVModSync
{
    public class DictHandler
    {
        private static readonly Dictionary<string, Dictionary<string, string>> libraryOfEverything = new Dictionary<string, Dictionary<string, string>>();
        private static readonly Dictionary<string, bool> libraryOfModdedBits = new Dictionary<string, bool>();

        public static void InitAsClean(string csvIntPath)
        {
            libraryOfModdedBits.Add(csvIntPath, false);
        }

        public static void SetDirty(string csvIntPath)
        {
            libraryOfModdedBits[csvIntPath] = true;            
        }

        public static void CopyFileToDict(string csvAbsPath, string csvIntPath)
        {
            // TODO throw some error if csvExtFile not found
            // TODO no need to read files if not modded
            // TODO handle stuff with multiple identical keys (like cfg/dress.csv, LOD.csv; removed from config for now)

            using (Stream csvExtFile = File.Open(csvAbsPath, FileMode.Open))
            {
                StreamReader reader = new StreamReader(csvExtFile);
                string header = reader.ReadLine();
                string content = reader.ReadToEnd();

                libraryOfEverything.Add(csvIntPath, new Dictionary<string, string>());
                libraryOfEverything[csvIntPath].Add("fvs_header", header);

                string[] contentLines = content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string contentLine in contentLines)
                {
                    string key = contentLine.Split(',').First();
                    libraryOfEverything[csvIntPath].Add(key, contentLine);
                }
            }
            // Console.WriteLine("Copy to dictionary: {0}", csvAbsPath);
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
                    string key = contentLine.Split(',').First();

                    if (libraryOfEverything[csvIntPath].ContainsKey(key))
                    {
                        // overwrite existing line in CSV
                        libraryOfEverything[csvIntPath][key] = contentLine;
                    }
                    else
                    {
                        // add new line to CSV
                        libraryOfEverything[csvIntPath].Add(key, contentLine);
                    }
                }
            }
        }

        public static bool DictExists(string csvIntPath)
        {
            if (libraryOfEverything.ContainsKey('\\' + csvIntPath))
            {
                return true;
            }
            return false;
        }

        public static void CreateGameFileFromDict(string csvIntPath)
        {
            if (libraryOfModdedBits[csvIntPath] == true) 
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