/*
Mod installer for Life is Feudal: Forest Village (c) pbox 2016
Published under the GNU General Public License https://www.gnu.org/licenses/gpl-3.0.txt
*/

namespace FVModSync
{
    using FVModSync.Configuration;
    using FVModSync.Extensions;
    using FVModSync.Handlers;
    using FVModSync.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Program
    {

        public static void Main(string[] args)
        {
            // TODO figure out actual game version + reexport if necessary
            ConfigReader.InitConfig();
            Console.WriteLine(InternalConfig.VersionBlurb);

            try
            {
                string[] pakNames = { "cfg", "scripts" };
                string[] modFiles = GenericFileHandler.SearchModFiles();
                List<string> csvRecognisedPaths = ExternalConfig.FileLocations;

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
                        else if (internalName == InternalConfig.InternalLuaIncludePath)
                        {
                            ListHandler.AddToList(modFile, internalName);
                        }
                        else if (internalName == InternalConfig.InternalLuaConfigPath)
                        {
                            AssignmentListHandler.AddToAssignmentList(modFile, internalName);
                        }
                        else if (InternalConfig.modDefaults.Contains(internalName))
                        {
                            ArrayHandler.AddToArray(modFile, internalName);
                        }
                        else if (modFile.EndsWith(".scheme", StringComparison.Ordinal) || modFile.EndsWith(".imageset", StringComparison.Ordinal))
                        {
                            if (QuickBmsUnpacker.Unpack(new string[] { "gui" }))
                            {
                                XmlHandler.AddToScheme(modFile, internalName);
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
                    XmlHandler.CreateFilesFromXml();
                    AssignmentListHandler.CreateFilesFromLists();
                    ArrayHandler.CreateFilesFromArrays();

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