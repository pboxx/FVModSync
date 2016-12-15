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

        private const string ExportFolderName = "FVModSync_exportedCSV";
        private const string ModsSubfolderName = "mods";

        public static void Main(string[] args)
        {
            // unpack cfg.pak in case user doesn't have it unpacked already
            // TODO find less pedestrian way (than just "if dir exists") to determine whether those exports are actually valid

            if (!Directory.Exists(ExportFolderName))
            {
                // TODO error catching if something goes wrong here
                // oldversion: Process quickbms = Process.Start(@"quickbms\quickbms.exe", @"-F *.csv -o -q -Y -Q quickbms\life_is_feudal.bms ..\cfg.pak " + ExportFolderName);

                Process quickbms = new Process();
                quickbms.StartInfo.FileName = @"quickbms\quickbms.exe";
                quickbms.StartInfo.Arguments = @"-F *.csv -o -q -Y -Q quickbms\life_is_feudal.bms ..\cfg.pak " + ExportFolderName;
                quickbms.StartInfo.UseShellExecute = false;
                quickbms.StartInfo.RedirectStandardInput = true;
                quickbms.Start();
                quickbms.StandardInput.WriteLine("\n");
                quickbms.WaitForExit(1000);

                Console.WriteLine("Export cfg.pak from game files to {0}", ExportFolderName);
            }

            // create internal dictionaries analog to file list in config
            string[] csvPaths = ConfigReader.LoadCsvPaths();

            foreach (string csvIntPath in csvPaths)
            {
                string gameCsvFilePath = @".." + csvIntPath;
                string exportedCsvFilePath = ExportFolderName + csvIntPath;

                if (File.Exists(gameCsvFilePath))
                {
                    string backupFilePath = gameCsvFilePath + ".backup";

                    File.Delete(backupFilePath);
                    File.Copy(gameCsvFilePath, backupFilePath);

                    Console.WriteLine("File exists: {0} -- create backup on disk", gameCsvFilePath);

                    // copy existing game file to internal dictionary
                    DictHandler.CopyFileToDict(gameCsvFilePath, csvIntPath);
                }
                else
                {
                    // copy from exported files
                    DictHandler.CopyFileToDict(exportedCsvFilePath, csvIntPath);
                }
                DictHandler.InitAsClean(csvIntPath);
            }


            Console.WriteLine("Searching mods folder: {0} ", ModsSubfolderName);

            string[] modFiles = Directory.GetFiles(ModsSubfolderName, "*", SearchOption.AllDirectories);
            Array.Sort(modFiles, StringComparer.InvariantCulture);
 
            Console.WriteLine("Modded files found:");
            foreach (string modFile in modFiles)
            {
                Console.WriteLine(modFile);
            } 

            foreach (string modFile in modFiles)
            {
                string[] relevantPathParts = modFile.Split('\\');
                string relevantPath = string.Join("\\", relevantPathParts.Skip(2).ToArray());
                string targetFile = @"..\" + relevantPath;
                string cvsIntPath = @"\" + relevantPath;

                if (modFile.EndsWith(".csv", StringComparison.Ordinal))
                {
                    if (DictHandler.DictExists(relevantPath))
                    {
                        DictHandler.CopyModdedFileToDict(modFile);
                        Console.WriteLine("Copy CSV content to dictionary: {0}", modFile);

                        DictHandler.SetDirty(cvsIntPath);
                        Console.WriteLine("Set dirty bit: {0}", cvsIntPath);
                    }
                    else // this is a custom cvs
                    {
                        Program.CopyFileFromModDir(modFile, targetFile);
                    }
                }
                else // this is some other file
                {
                    Program.CopyFileFromModDir(modFile, targetFile);
                }
            }

            foreach (string csvIntPath in csvPaths)
            {
                DictHandler.CreateGameFileFromDict(csvIntPath);
            }
            Console.WriteLine(" ");
            Console.WriteLine("Everything seems to be fine. Press Enter to close");
            Console.ReadLine();
        }

        private static void CopyFileFromModDir(string modFile, string targetFile)
        {
            // how do we deal with existing files here? backup?

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