/*
Mod installer for Life is Feudal: Forest Village
Published under the GNU General Public License https://www.gnu.org/licenses/gpl-3.0.txt
*/

namespace FVModSync
{
    using FVModSync.Configuration;
    using FVModSync.Extensions;
    using FVModSync.Handlers;
    using FVModSync.Services;
    using System;
    using System.Linq;

    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(Config.Version);

            try
            {
                string[] pakNames = { "cfg", "scripts" };
                string[] csvRecognisedPaths = ConfigReader.LoadCsvPaths();
                string[] modFiles = GenericFileHandler.SearchModFiles();

                if (QuickBmsUnpacker.Unpack(pakNames))
                {
                    foreach (string modFile in modFiles)
                    {
                        string internalName = modFile.GetInternalName();

                        if (modFile.EndsWith(".csv", StringComparison.Ordinal))
                        {
                            // is this a game file we handle
                            if (csvRecognisedPaths.Contains(internalName))
                            {
                                CsvHandler.ParseCsvToTable(modFile, internalName);
                            }
                            else // this is a custom csv
                            {
                                GenericFileHandler.CopyFileFromModDir(modFile);
                            }
                        }
                        else if (internalName == Config.InternalLuaIncludePath)
                        {
                            ListHandler.AddToList(modFile, internalName);
                        }
                        else if (internalName == Config.InternalLuaConfigPath)
                        {
                            AssignmentListHandler.AddToAssignmentList(modFile, internalName);
                        }
                        else if (Config.modDefaults.Contains(internalName))
                        {
                            ArrayHandler.AddToArray(modFile, internalName);
                        }
                        else if (modFile.EndsWith(".scheme", StringComparison.Ordinal))
                        {
                            if (QuickBmsUnpacker.Unpack(new string[] { "gui" }))
                            {
                                SchemeHandler.AddToScheme(modFile, internalName);
                            }
                        }
                        else if (!modFile.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) && !modFile.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) // this is some other file
                        {
                            GenericFileHandler.CopyFileFromModDir(modFile);
                        }
                    }
                    Console.WriteLine();
                    CsvHandler.CreateGameFilesFromTables();
                    ListHandler.CreateFilesFromLists();
                    SchemeHandler.CreateFilesFromSchemes();
                    AssignmentListHandler.CreateFilesFromLists();
                    ArrayHandler.CreateFilesFromLists();

                    Console.WriteLine();
                    Console.WriteLine("Everything seems to be fine. Press Enter to close");
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("ERROR:");
                Console.Error.WriteLine("{0}: {1}\n{2}", e.GetType().Name, e.Message, e.StackTrace);
            }
            Console.ReadLine();
        }
    }
}