/*
Mod installer for Life is Feudal: Forest Village
Published under the GNU General Public License https://www.gnu.org/licenses/gpl-3.0.txt
*/

using System;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace FVModSync
{
    public class Program
    {
        // TODO verbose switch

        private const string Version = "FVModSync v0.1.4beta\nMod installer for Life is Feudal: Forest Village\n(c) pbox 2016\nPublished under the GPLv3 https://www.gnu.org/licenses/gpl-3.0.txt";
        private const string ExportFolder = "FVModSync_exportedFiles";
        private const string ModsSubfolder = "mods";
        private const string LuaIncludeFilePath = @"\scripts\include.lua";

        public static void Main(string[] args)
        {
            Console.WriteLine(Version);
            Console.WriteLine();

            QuickBMSHandler.Unpack(@"..\cfg.pak", ExportFolder);
            QuickBMSHandler.Unpack(@"..\scripts.pak", ExportFolder);

            Console.WriteLine("Searching mods folder: {0} ", ModsSubfolder);

            string[] csvRecognisedPaths = ConfigReader.LoadCsvPaths();
            string[] modFiles = Directory.GetFiles(ModsSubfolder, "*", SearchOption.AllDirectories);

            Array.Sort(modFiles, StringComparer.InvariantCulture);
            Console.WriteLine("Modded files found:");

            foreach (string modFile in modFiles)
            {
                Console.WriteLine(modFile);
            }
            Console.WriteLine();

            foreach (string modFile in modFiles)
            {
                string[] relevantPathParts = modFile.Split('\\');
                string relevantPath = string.Join("\\", relevantPathParts.Skip(2).ToArray());
                string targetFile = @"..\" + relevantPath;
                string intPath = @"\" + relevantPath;


                if (modFile.EndsWith(".csv", StringComparison.Ordinal))
                {
                    // is this a game file we handle
                    if (csvRecognisedPaths.Contains(intPath) )
                    {
                        if (!DictHandler.DictExists(intPath))
                        {
                            // create internal dictionary
                            DictHandler.BackupAndCopy(intPath, ExportFolder);
                        }
                        DictHandler.CopyModdedFileToDict(modFile);

                        Console.WriteLine("Copy CSV content to dictionary: {0}", modFile);
                    }
                    else // this is a custom csv
                    {
                        Program.CopyFileFromModDir(modFile, targetFile);
                    }
                }
                else if (intPath == LuaIncludeFilePath)
                {
                    LuaHandler.BackupAndCopy(targetFile, ExportFolder + intPath);
                    LuaHandler.CopyFileToIncludeList(modFile);
                }
                else if (!modFile.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) // this is some other file
                {
                    Program.CopyFileFromModDir(modFile, targetFile);
                }
            }

            Console.WriteLine();

            foreach (string csvIntPath in csvRecognisedPaths)
            {
                DictHandler.CreateGameFileFromDict(csvIntPath);
            }

            LuaHandler.CreateIncludeFileFromList(LuaIncludeFilePath);

            Console.WriteLine(" ");
            Console.WriteLine("Everything seems to be fine. Press Enter to close");
            Console.ReadLine();
        }

        private static void CopyFileFromModDir(string modFile, string targetFile)
        {
            // TODO decide how we deal with existing files here .. backup them all?

            if (File.Exists(targetFile))
            {
                File.Delete(targetFile);
                File.Copy(modFile, targetFile);
                Console.WriteLine("Copy file {0} to {1} (overwrite)", modFile, targetFile);
            }
            else
            {
                string targetDir = Path.GetDirectoryName(targetFile);
                Directory.CreateDirectory(targetDir);
                File.Copy(modFile, targetFile);
                Console.WriteLine("Copy file {0} to {1} (new)", modFile, targetFile);
            }
        }
    }
}