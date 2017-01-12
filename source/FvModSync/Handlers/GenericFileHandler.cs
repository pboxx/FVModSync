namespace FVModSync.Handlers
{
    using FVModSync.Configuration;
    using FVModSync.Extensions;
    using FVModSync.Handlers;
    using System;
    using System.IO;

    public class GenericFileHandler
    {
        public static void BackupIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                string backupFilePath = filePath + ".backup";
                File.Delete(backupFilePath);
                File.Copy(filePath, backupFilePath);
            }
        }

        public static string[] SearchModFiles()
        {
            if (Directory.Exists(Config.ModsSubfolderName))
            {
                string[] modFiles = Directory.GetFiles(Config.ModsSubfolderName, "*", SearchOption.AllDirectories);

                Array.Sort(modFiles, StringComparer.InvariantCulture);

                Console.WriteLine("Modded files found:");
                foreach (string modFile in modFiles)  { Console.WriteLine(modFile); }
                Console.WriteLine();

                return modFiles;
            }
            else
            {
                throw new DirectoryNotFoundException("Mods subfolder not found. Try putting your mods in a subfolder named \"mods\" and running the program again");
            }
        }

        public static void InitFromModDefault(Action<string, string> AddToTarget, string internalName)
        {
            string modDefaultFilePath = Config.ModDefaultsFolderName + internalName;
                
            if (!File.Exists(modDefaultFilePath)) 
            {
                throw new FileNotFoundException("Mod default {0} not found.", modDefaultFilePath);
            }
            Console.WriteLine();
            Console.WriteLine("Init from mod default: {0} ...", internalName);
            AddToTarget(modDefaultFilePath, internalName);
        }

        public static void Init(Action<string, string> AddToTarget, string internalName)
        {
            string exportedFilePath = Config.ExportFolderName + internalName;
            string gameFilePath = Config.GameFilePrefix + internalName;

            if (File.Exists(gameFilePath))
            {
                Console.WriteLine();
                Console.WriteLine("Init from game files: {0} ...", internalName);
                AddToTarget(gameFilePath, internalName);
            }
            else
            {
                if (!File.Exists(exportedFilePath))
                {
                    throw new FileNotFoundException("Exported file {0} not found. Try deleting the FVModSync_exportedFiles folder and running the program again", exportedFilePath);
                }
                Console.WriteLine();
                Console.WriteLine("Init from exported files: {0} ...", internalName);
                AddToTarget(exportedFilePath, internalName);
            }
        }

        public static void CopyFileFromModDir(string modFile)
        {
            string targetFile = Config.GameFilePrefix + modFile.GetInternalName();

            if (File.Exists(targetFile))
            {
                FileInfo modFileInfo = new FileInfo(modFile);
                FileInfo targetFileInfo = new FileInfo(targetFile);

                if (modFileInfo.LastWriteTime > targetFileInfo.LastWriteTime)
                {
                    File.Delete(targetFile);
                    File.Copy(modFile, targetFile);
                    Console.WriteLine();
                    Console.WriteLine("Copy file {0} to {1} (overwrite)", modFile, targetFile);
                }
            }
            else
            {
                string targetDir = Path.GetDirectoryName(targetFile);

                Directory.CreateDirectory(targetDir);
                File.Copy(modFile, targetFile);
                Console.WriteLine();
                Console.WriteLine("Copy file {0} to {1} (new)", modFile, targetFile);
            }
        }
    }
}
