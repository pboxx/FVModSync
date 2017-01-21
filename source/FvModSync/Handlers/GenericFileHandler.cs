namespace FVModSync.Handlers
{
    using FVModSync.Configuration;
    using FVModSync.Extensions;
    using System;
    using System.IO;

    public class GenericFileHandler
    {
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

        public static void Init(Action<string, string> AddToTarget, string internalName)
        {
            string exportedFilePath = ExternalConfig.ExportFolderName + @"\" + internalName;
            string gameFilePath = ExternalConfig.GameFilePrefix + @"\" + internalName;

            if (File.Exists(gameFilePath))
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
            string targetFilePath = ExternalConfig.GameFilePrefix + @"\" + modFilePath.GetInternalName();

            DoCopy(modFilePath, targetFilePath, false);
        }

        public static void CopyScriptFromModDir(string modFilePath)
        {
            string internalName = modFilePath.GetInternalScriptName();
            string targetModDirName = modFilePath.GetModDirName();

            string targetFilePath = ExternalConfig.GameFilePrefix + @"\scripts\mods\" + targetModDirName + @"\" + internalName;

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
                ListHandler.AddEntryToList(InternalConfig.InternalLuaInitPath, requireMe);
            }
        }
    }
}
