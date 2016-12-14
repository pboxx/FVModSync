using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FVModSync
{
    public class DictHandler
    {
        public static Dictionary<string, Dictionary<string, string>> libraryOfEverything = new Dictionary<string, Dictionary<string, string>>();

        public static void CopyFileToDict(string csvAbsPath, string csvIntPath)
        {
            // TODO throw some error if file not found
            // TODO exception for stuff with multiple identical keys (like cfg/dress.csv, LOD.csv; removed from config for now)

            using (Stream csvExtFile = File.Open(csvAbsPath, FileMode.Open))
            {
                StreamReader reader = new StreamReader(csvExtFile);
                string header = reader.ReadLine();
                string content = reader.ReadToEnd();

                libraryOfEverything.Add(csvIntPath, new Dictionary<string, string>());
                libraryOfEverything[csvIntPath].Add ("fvs_header", header);

                string[] contentLines = content.Split(new[] { "\r\n" }, StringSplitOptions.None);

                foreach (string contentLine in contentLines)
                {
                    string key = contentLine.Split(',').First();

                    libraryOfEverything[csvIntPath].Add(key, contentLine);
                }
            }
            Console.WriteLine("{0} -- Copying to dictionary", csvAbsPath);
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
                string[] contentLines = content.Split(new[] { "\r\n" }, StringSplitOptions.None);

                foreach (string contentLine in contentLines)
                {
                    // use first field as dict key 

                    string key = contentLine.Split(',').First();

                    if (libraryOfEverything[csvIntPath].ContainsKey(key))
                    {
                        libraryOfEverything[csvIntPath][key] = contentLine;
                    }
                    else
                    {
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
            else
            {
                return false;
            }
        }

        public static void CreateGameFileFromDict(string csvIntPath)
        {
            string targetDir = @"..\" + Path.GetDirectoryName(csvIntPath);
            Directory.CreateDirectory(targetDir);

            using (Stream gameFile = File.Open(".." + csvIntPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(gameFile))
                { 
                    string[] contentLines = libraryOfEverything[csvIntPath].Values.ToArray();

                    for (int i = 0; i < (contentLines.Length -1); i++)
                    {
                        writer.WriteLine(contentLines[i]);
                    }

                    // no CRLF after last line -- will crash game ver 0.9.6005 with cfg/Localization.csv and cfg/normal/plants.csv
                    writer.Write(contentLines.Last());
                }
            }
        }
    }
}
