﻿namespace FVModSync.Handlers
{
    using FVModSync.Configuration;
    using FVModSync.Extensions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class GenericFileHandler
    {
        private static readonly Dictionary<string, List<string>> storedFiles = new Dictionary<string, List<string>>();

        public static void BackupIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                string backupFilePath = filePath + ExternalConfig.GameFileBackupSuffix;
                File.Delete(backupFilePath);
                File.Copy(filePath, backupFilePath);
            }
        }

        public static string[] SearchModFiles()
        {
            // TODO put mod defaults at top of list no matter what their folder is named
            if (Directory.Exists(ExternalConfig.ModsSubfolderName))
            {
                string[] modFiles = Directory.GetFiles(ExternalConfig.ModsSubfolderName, "*", SearchOption.AllDirectories);

                Array.Sort(modFiles, StringComparer.InvariantCulture);

                if (ExternalConfig.ConsoleVerbosity != "quiet")
                {
                    Console.WriteLine("Modded files found:");
                    foreach (string modFile in modFiles) { Console.WriteLine(modFile); }
                    Console.WriteLine();
                }
                return modFiles;
            }
            else
            {
                throw new DirectoryNotFoundException("Mods subfolder not found. Try putting your mods in a subfolder named \"mods\" and running the program again");
            }
        }

        public static void InitStoredFile(string internalName)
        {
            Init(AppendContentsToStoredFile, internalName);
        }

        public static List<string> GetFile(string internalName)
        {
            if (!storedFiles.ContainsKey(internalName))
            {
                storedFiles.Add(internalName, new List<string>());
                InitStoredFile(internalName);
            }
            List<string> storedFile = storedFiles[internalName];
            return storedFile;
        }

        public static void AppendContentsToStoredFile(string sourceFilePath, string internalName)
        {
            List<string> storedFile = GetFile(internalName);

            using (Stream stream = File.Open(sourceFilePath, FileMode.Open))
            {
                StreamReader reader = new StreamReader(stream);

                string content = reader.ReadToEnd();
                string[] contentLines = content.Split(new[] { "\r\n" }, StringSplitOptions.None);

                foreach (string contentLine in contentLines)
                {
                    storedFile.Add(contentLine);
                }

                if (ExternalConfig.ConsoleVerbosity != "quiet")
                {
                    Console.WriteLine("Append file to {0}: {1}", internalName, sourceFilePath);
                }
            }
        }

        public static void AddLineToFile(string internalName, string entry)
        {
            List<string> storedFile = GetFile(internalName);

            if (!storedFile.Contains(entry))
            {
                storedFile.Add(entry);

                if (ExternalConfig.ConsoleVerbosity != "quiet")
                {
                    Console.WriteLine("Add entry to {0}: {1}", internalName, entry);
                }
            }
        }

        public static void WriteStoredFiles()
        {
            foreach (KeyValuePair<string, List<string>> storedFile in storedFiles)
            {
                var internalName = storedFile.Key;
                var fileContent = storedFile.Value;

                if (fileContent.Any()) // dont write empty arrays
                {
                    string gameFilePath = ExternalConfig.GameFilesPrefix + @"\" + internalName;
                    BackupIfExists(gameFilePath);

                    string targetDir = ExternalConfig.GameFilesPrefix + @"\" + Path.GetDirectoryName(internalName);
                    Directory.CreateDirectory(targetDir);

                    File.WriteAllLines(gameFilePath, fileContent.ToArray());

                    Console.WriteLine("Write to game files: {0}", internalName);
                }
            }
        }
    
        public static void Init(Action<string, string> AddToTarget, string internalName)
        {
            string exportedFilePath = ExternalConfig.ExportFolderName + @"\" + internalName;
            string gameFilePath = ExternalConfig.GameFilesPrefix + @"\" + internalName;

            if (File.Exists(gameFilePath) && internalName != InternalConfig.InternalLuaInitPath)
            {
                if (ExternalConfig.ConsoleVerbosity != "quiet")
                {
                    Console.WriteLine();
                    Console.WriteLine("Init from game files: {0} ...", internalName);
                }
                AddToTarget(gameFilePath, internalName);
            }
            else
            {
                if (!File.Exists(exportedFilePath))
                {
                    throw new FileNotFoundException(string.Format("Exported file {0} not found. Make sure the mod you're installing is compatible with the game version you have. If it is, try deleting the {1} folder and running the program again", exportedFilePath, ExternalConfig.ExportFolderName));
                }
                if (ExternalConfig.ConsoleVerbosity != "quiet")
                {
                    Console.WriteLine();
                    Console.WriteLine("Init from exported files: {0} ...", internalName);
                }
                AddToTarget(exportedFilePath, internalName);
            }
        }

        public static void CopyFileFromModDir(string modFilePath)
        {
            string targetFilePath = ExternalConfig.GameFilesPrefix + @"\" + modFilePath.GetInternalName();

            DoCopy(modFilePath, targetFilePath, false);
        }

        public static void CopyScriptFromModDir(string modFilePath)
        {
            string internalName = modFilePath.GetScriptName();
            string targetModDirName = modFilePath.GetModDirName();

            string targetFilePath = ExternalConfig.GameFilesPrefix + @"\" + InternalConfig.GameFilesModDir + @"\" + targetModDirName + @"\" + internalName;

            DoCopy(modFilePath, targetFilePath, true);
        }

        private static void DoCopy(string sourceFilePath, string targetFilePath, bool AddReqToInit)
        {
            if (File.Exists(targetFilePath))
            {
                FileInfo modFileInfo = new FileInfo(sourceFilePath);
                FileInfo targetFileInfo = new FileInfo(targetFilePath);

                if (modFileInfo.LastWriteTime > targetFileInfo.LastWriteTime)
                {
                    File.Delete(targetFilePath);
                    File.Copy(sourceFilePath, targetFilePath);

                    if (ExternalConfig.ConsoleVerbosity != "quiet")
                    {
                        Console.WriteLine();
                        Console.WriteLine("Copy file {0} to {1} (overwrite)", sourceFilePath, targetFilePath);
                    }
                }
                else
                {
                    if (ExternalConfig.ConsoleVerbosity == "verbose")
                    {
                        Console.WriteLine();
                        Console.WriteLine("File {0} has the same timestamp as target {1}; file ignored", sourceFilePath, targetFilePath);
                    }
                }
            }
            else
            {
                string targetDir = Path.GetDirectoryName(targetFilePath);

                Directory.CreateDirectory(targetDir);
                File.Copy(sourceFilePath, targetFilePath);

                if (ExternalConfig.ConsoleVerbosity != "quiet")
                {
                    Console.WriteLine();
                    Console.WriteLine("Copy file {0} to {1} (new)", sourceFilePath, targetFilePath);
                }
            }

            if (AddReqToInit)
            {
                string targetModDirName = sourceFilePath.GetModDirName();
                string requireMe = @"requireMod('" + targetModDirName + "')";
                AddLineToFile(InternalConfig.InternalLuaInitPath, requireMe);
            }
        }
    }
}
