/*
Mod installer for Life is Feudal: Forest Village
Published under the GNU General Public License https://www.gnu.org/licenses/gpl-3.0.txt
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace FVModSync
{

    public class Program
    {

        // TODO verbose switch

        public const string ExportFolderName = "FVModSync_exportedCSV";
        public const string ModsSubfolderName = "mods";

        public static void CopyFileFromModDir(string modFile, string relevantPath)
        {
            string targetFile = @"..\" + relevantPath;

            // how do we deal with existing files here? backup?

            if (File.Exists(targetFile))
            {
                File.Delete(targetFile);
                File.Copy(modFile, targetFile);

                Console.WriteLine("{0} -- Copying file to {1} (overwrite)", modFile, targetFile);
            }
            else
            {
                string targetDir = Path.GetDirectoryName(targetFile);
                Directory.CreateDirectory(targetDir);
                File.Copy(modFile, targetFile);

                Console.WriteLine("{0} -- Copying file to {1} (new)", modFile, targetFile);
            }
        }

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

                Console.WriteLine("Exporting cfg.pak from game files to {0}", ExportFolderName);
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

                    Console.WriteLine("{0} -- File exists, creating backup on disk", gameCsvFilePath);

                    // copy existing game file to internal dictionary
                    DictHandler.CopyFileToDict(gameCsvFilePath, csvIntPath);
                }
                else
                {
                    // copy from exported files
                    DictHandler.CopyFileToDict(exportedCsvFilePath, csvIntPath);
                }
            }


            Console.WriteLine("Searching mods folder: {0} ", ModsSubfolderName);

            string[] modFiles = Directory.GetFiles(ModsSubfolderName, "*", SearchOption.AllDirectories);

            foreach (string modFile in modFiles)
            {
                string[] relevantPathParts = modFile.Split('\\');
                string relevantPath = String.Join("\\", relevantPathParts.Skip(2).ToArray());

                if (modFile.EndsWith(".csv"))
                {
                    if (DictHandler.DictExists(relevantPath)) 
                    {
                        // TODO set dirty bit (so we know we actually need to copy this over)
                        DictHandler.CopyModdedFileToDict(modFile);

                        Console.WriteLine("{0} -- Copying CSV content to dictionary", modFile);
                    }
                    else // this is a custom cvs
                    {
                        Program.CopyFileFromModDir(modFile, relevantPath);
                    }
                }
                else  
                {
                    Program.CopyFileFromModDir(modFile, relevantPath);
                }
            }

            foreach (string csvIntPath in csvPaths)
            {
                // TODO only for modded files
                DictHandler.CreateGameFileFromDict(csvIntPath);
                Console.WriteLine("{0} -- Writing dictionary to game files", csvIntPath);
            }
            Console.WriteLine(" ");
            Console.WriteLine("Everything seems to be fine. Press Enter to close");
            Console.ReadLine();
        }
    }
}
