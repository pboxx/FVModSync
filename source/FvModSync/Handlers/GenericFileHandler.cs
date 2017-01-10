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
