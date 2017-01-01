﻿/*
Mod installer for Life is Feudal: Forest Village
Published under the GNU General Public License https://www.gnu.org/licenses/gpl-3.0.txt
*/

namespace FVModSync
{
	using System;
	using System.IO;
	using System.Linq;

	using FVModSync.Configuration;
	using FVModSync.Handlers;
	using FVModSync.Services;
    using System.Diagnostics;

	/// <summary>
    /// The main program.
    /// </summary>
    public static class Program
    {
        // TODO verbose switch

        private const string Version = "FVModSync v0.2beta\nMod installer for Life is Feudal: Forest Village\n(c) pbox 2016\nPublished under the GPLv3 https://www.gnu.org/licenses/gpl-3.0.txt \n";
        private const string ExportFolder = "FVModSync_exportedFiles";
        private const string ModsSubfolder = "mods";
        private const string LuaIncludeFilePath = @"\scripts\include.lua";

        /// <summary>
        /// The main entry point.
        /// </summary>
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine(Version);
                QuickBmsUnpacker.Unpack(@"..\cfg.pak", ExportFolder);
                QuickBmsUnpacker.Unpack(@"..\scripts.pak", ExportFolder);

                if (!Directory.Exists(ModsSubfolder))
                {
                    throw new DirectoryNotFoundException("Mods subfolder not found. Try putting your mods in a subfolder named \"mods\" and running the program again");
                }

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
                    string relevantPath = @"\" + string.Join("\\", relevantPathParts.Skip(2).ToArray());
                    string targetFile = @".." + relevantPath;

                    if (modFile.EndsWith(".csv", StringComparison.Ordinal))
                    {
                        // is this a game file we handle
                        if (csvRecognisedPaths.Contains(relevantPath))
                        {
                            if (!LibraryHandler.RecordExists(relevantPath))
                            {
                                // create internal dictionary
                                LibraryHandler.BackupAndCopy(relevantPath, ExportFolder);
                            }
                            LibraryHandler.CopyModdedFileToDict(modFile);

                            Console.WriteLine("Copy CSV content to dictionary: {0}", modFile);
                        }
                        else // this is a custom csv
                        {
                            Program.CopyFileFromModDir(modFile, targetFile);
                        }
                    }
                    else if (relevantPath == LuaIncludeFilePath)
                    {
                        LuaHandler.BackupAndCopy(targetFile, ExportFolder + relevantPath);
                        LuaHandler.CopyFileToIncludeList(modFile);
                    }
                    else if (!modFile.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) // this is some other file
                    {
                        Program.CopyFileFromModDir(modFile, targetFile);
                    }
                }

                Console.WriteLine();

                foreach (string relevantPath in csvRecognisedPaths)
                {
                    LibraryHandler.CreateGameFileFromLibrary(relevantPath);
                }

                LuaHandler.CreateIncludeFileFromList(LuaIncludeFilePath);

                Console.WriteLine(" ");
                Console.WriteLine("Everything seems to be fine. Press Enter to close");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error:");
                Console.Error.WriteLine("{0}: {1}\n{2}", e.GetType().Name, e.Message, e.StackTrace);
            }
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