namespace FVModSync.Handlers
{
    using FVModSync.Configuration;
    using FVModSync.Extensions;
    using Microsoft.VisualBasic.FileIO;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class CsvHandler
    {
        private static readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> libraryOfEntries = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        private static readonly Dictionary<string, Dictionary<string, Dictionary<string, bool>>> libraryOfModdedEntries = new Dictionary<string, Dictionary<string, Dictionary<string, bool>>>();
        private static readonly List<string> csvRecognisedPaths = ExternalConfig.csvFiles;

        public static void InitTable(string internalName)
        {
            GenericFileHandler.Init(ParseCsvToTable, internalName);
        }

        public static void ParseCsvToTable(string sourceFilePath, string internalName) 
        {
            if (!libraryOfEntries.ContainsKey(internalName))
            {
                libraryOfEntries.Add(internalName, new Dictionary<string, Dictionary<string, string>>());
                libraryOfModdedEntries.Add(internalName, new Dictionary<string, Dictionary<string, bool>>());

                if (csvRecognisedPaths.Contains(internalName))
                {
                    InitTable(internalName);
                } 
            }

            bool isMod = false;

            if (sourceFilePath.StartsWith(ExternalConfig.ModsSubfolderName))
            {
                isMod = true;
            }

            if (ExternalConfig.ConsoleVerbosity != "quiet")
            {
                Console.WriteLine("Parse to {0}: {1} ...", internalName, sourceFilePath);
            }

            using (Stream externalFile = File.Open(sourceFilePath, FileMode.Open))
            {
                using (TextFieldParser parser = new TextFieldParser(externalFile))
                {
                    parser.SetDelimiters(",");
                    parser.HasFieldsEnclosedInQuotes = true;
                    parser.TrimWhiteSpace = true;

                    string[] header = parser.ReadFields();

                    if (SetOrAddHeader(sourceFilePath, internalName, "fvms_header", header, isMod))
                    {
                        while (!parser.EndOfData)
                        {
                            try
                            {
                                string[] record = parser.ReadFields();
                                string recordName = CreateKey(internalName, record);

                                if (record.Length != header.Length)
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("ERROR: \"{0}\" in file {1} -- Record length ({2}) does not match header length ({3}); record ignored", recordName, sourceFilePath, record.Length, header.Length);
                                }

                                else
                                {
                                    List<string> DirtyWords = new List<string>();

                                    for (int i = 0; i < record.Length; i++)
                                    {
                                        string fieldValue = record[i];
                                        string fieldName = header[i];

                                        if (fieldValue != InternalConfig.IgnoreCsvFieldString)
                                        {
                                            if (IsDirty(internalName, recordName, fieldName))
                                            {
                                                DirtyWords.Add(fieldName);
                                            }

                                            SetOrAddField(internalName, recordName, fieldName, fieldValue);

                                            if (isMod)
                                            {
                                                SetDirty(internalName, recordName, fieldName, fieldValue);
                                            }
                                        }
                                    }
                                    if (DirtyWords.Count > 0)
                                    {
                                        string DirtyNames = String.Join(", ", DirtyWords.ToArray());
                                        Console.WriteLine();
                                        Console.WriteLine("CONFLICTS: Field(s) {0} in record \"{1}\" already exist -- File: {2}", String.Join(", ", DirtyWords.ToArray()), recordName, sourceFilePath);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.Error.WriteLine("ERROR:");
                                Console.Error.WriteLine("{0}: {1}\n{2}", e.GetType().Name, e.Message, e.StackTrace);
                            }
                        }
                    }
                }
            }
        }

        public static bool SetOrAddHeader(string sourceFilePath, string internalName, string recordName, string[] header, bool isMod)
        {
            for (int i = 0; i < header.Length; i++)
            {
                string fieldName = header[i];

                var table = libraryOfEntries[internalName];

                if (!table.ContainsKey(recordName))
                {
                    table.Add(recordName, new Dictionary<string, string>());
                }

                var record = libraryOfEntries[internalName][recordName];

                if (record.ContainsKey(fieldName))
                {
                    record[fieldName] = fieldName;
                }

                else
                {
                    if (isMod)
                    {
                        Console.WriteLine();
                        Console.WriteLine("ERROR -- File {0}: Attempt to add unknown header field \"{1}\"; file ignored", sourceFilePath, fieldName);
                        Console.WriteLine();
                        return false;
                    }
                    else
                    {
                        record.Add(fieldName, fieldName);
                    }
                }
            }
            return true;
        }

        public static void SetOrAddField(string internalName, string recordName, string fieldName, string fieldValue)
        {
            var table = libraryOfEntries[internalName];

            if (!table.ContainsKey(recordName))
            {
                table.Add(recordName, new Dictionary<string, string>());
            }

            var record = libraryOfEntries[internalName][recordName];

            if (record.ContainsKey(fieldName))
            {
                record[fieldName] = fieldValue;

                if (ExternalConfig.ConsoleVerbosity == "debug")
                {
                    Console.WriteLine("Update field {0}: {1}", fieldName, fieldValue);
                }
            }
            else
            {
                record.Add(fieldName, fieldValue);

                if (ExternalConfig.ConsoleVerbosity == "debug")
                {
                    Console.WriteLine("Add new field {0}: {1}", fieldName, fieldValue);
                }
            }
        }

        public static void CreateGameFilesFromTables()
        {
            foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, string>>> table in libraryOfEntries)
            {
                var internalName = table.Key;
                var tableContent = table.Value;
                var recordNames = tableContent.Keys;
                var records = tableContent.Values;

                string gameFilePath = ExternalConfig.GameFilePrefix + @"\" + internalName;
                GenericFileHandler.BackupIfExists(gameFilePath);

                string targetDir = ExternalConfig.GameFilePrefix + @"\" + Path.GetDirectoryName(internalName);
                Directory.CreateDirectory(targetDir);

                using (Stream gameFile = File.Open(gameFilePath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(gameFile))
                    {
                        foreach (Dictionary<string, string> record in records)
                        {
                            var entryValues = record.Values.ToArray();
                            string line;

                            List<string> delimitedEntriesWrapped = new List<string>();

                            foreach (string entryValue in entryValues)
                            {
                                delimitedEntriesWrapped.Add(entryValue.Replace(@"""", @"""""").Replace("\t", "").WrapDelimited());
                            }
                            line = String.Join(",", delimitedEntriesWrapped.ToArray());

                            writer.WriteLine(line);
                        }
                    }
                }
                Console.WriteLine("Write table to game files: {0}", internalName);
            }
        }

        private static void SetDirty(string internalName, string recordName, string fieldName, string fieldValue)
        {
            var table = libraryOfModdedEntries[internalName];

            if (!table.ContainsKey(recordName))
            {
                table.Add(recordName, new Dictionary<string, bool>());
            }

            var record = libraryOfModdedEntries[internalName][recordName];

            if (record.ContainsKey(fieldName))
            {
                record[fieldName] = true;
            }
            else
            {
                record.Add(fieldName, true);
            }
        }

        private static bool IsDirty(string internalName, string recordName, string fieldName)
        {
            return (libraryOfModdedEntries.ContainsKey(internalName)
                   && libraryOfModdedEntries[internalName].ContainsKey(recordName)
                   && libraryOfModdedEntries[internalName][recordName].ContainsKey(fieldName)
                   && libraryOfModdedEntries[internalName][recordName][fieldName]);
        }

        private static string CreateKey(string internalName, string[] records)
        {
            string key;

            // TODO handle other stuff with multiple identical keys (like LOD.csv etc; removed from config for now)

            if (internalName.EndsWith("dress.csv"))
            {
                string[] keycombo = records.Take(2).ToArray();
                key = String.Join(" ", keycombo);
            }
            else
            {
                key = records.First();
            }
            return key;
        }
    }
}
