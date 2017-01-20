namespace FVModSync.Handlers
{
    using FVModSync.Configuration;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class ArrayHandler
    {
        private static readonly Dictionary<string, List<string>> arrays = new Dictionary<string, List<string>>();

        public static void AddToArray(string sourceFilePath, string internalName)
        {
            if (InternalConfig.modDefaultArrays.Contains(internalName))
            {
                string modDefaultFilePath = ExternalConfig.ModsSubfolderName + @"\" + ExternalConfig.ModDefaultsSubfolderName + internalName;

                if (!File.Exists(modDefaultFilePath))
                {
                    Console.WriteLine();
                    Console.WriteLine("ERROR: Mod default {0} not found. File ignored: {1}. Proceed with caution!", modDefaultFilePath, sourceFilePath);
                    Console.WriteLine();
                }
                else
                {
                    //if (sourceFilePath != modDefaultFilePath)
                    //{
                    //    DoAdd(modDefaultFilePath, internalName);
                    //    Console.WriteLine("Init from default: {0} ...", modDefaultFilePath);

                    //}
                    DoAdd(sourceFilePath, internalName);
                }
            }
            else
            {
                DoAdd(sourceFilePath, internalName);
            }
        }

        private static void DoAdd(string sourceFilePath, string internalName)
        {
            if (!arrays.ContainsKey(internalName))
            {
                arrays.Add(internalName, new List<string>());
            }

            List<string> array = arrays[internalName];

            using (Stream stream = File.Open(sourceFilePath, FileMode.Open))
            {
                StreamReader reader = new StreamReader(stream);

                string content = reader.ReadToEnd();
                string[] contentLines = content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string contentLine in contentLines)
                {
                    if (!array.Contains(contentLine) && contentLine.Trim() != "}")
                    {
                        array.Add(contentLine);
                    }
                }
            }
            if (ExternalConfig.ConsoleVerbosity != "quiet")
            {
                Console.WriteLine("Add to array {0}: {1}", internalName, sourceFilePath);
            }
        }

        public static void CreateFilesFromArrays()
        {
            foreach (KeyValuePair<string, List<string>> array in arrays)
            {
                var internalName = array.Key;
                var arrayEntries = array.Value;

                if (arrayEntries.Any()) // dont write empty arrays
                {
                    string gameFilePath = ExternalConfig.GameFilePrefix + @"\" + internalName;
                    GenericFileHandler.BackupIfExists(gameFilePath);

                    string targetDir = ExternalConfig.GameFilePrefix + @"\" + Path.GetDirectoryName(internalName);
                    Directory.CreateDirectory(targetDir);

                    using (Stream gameFileStream = File.Open(gameFilePath, FileMode.Create))
                    {
                        using (StreamWriter writer = new StreamWriter(gameFileStream))
                        {
                            string[] contentLines = arrayEntries.ToArray();

                            foreach (string contentLine in contentLines)
                            {
                                writer.WriteLine(contentLine);
                            }
                            writer.WriteLine("}");
                        }
                    }
                    Console.WriteLine("Write array to game files: {0}", internalName);
                }
            }
        }
    }
}