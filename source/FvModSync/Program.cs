﻿/*
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
            Console.WriteLine(InternalConfig.VersionBlurb);
            ConfigReader.InitConfig();
            try
            {
                string[] pakNames = InternalConfig.PakNames;
                string[] modFiles = GenericFileHandler.SearchModFiles();
                List<string> csvRecognisedPaths = ExternalConfig.csvFiles;

                if (QuickBmsUnpacker.Unpack(pakNames))
                {
                    foreach (string modFilePath in modFiles)
                    {
                        string internalName = modFilePath.GetInternalName();

                        if (modFilePath.EndsWith(".csv", StringComparison.Ordinal))
                        {
                            // is this a game file we handle
                            if (csvRecognisedPaths.Contains(internalName))
                            {
                                CsvHandler.ParseCsvToTable(modFilePath, internalName);
                            }
                            else // this is a custom csv
                            {
                                GenericFileHandler.CopyFileFromModDir(modFilePath);
                            }
                        }
                        else if (internalName == InternalConfig.InternalLuaInitPath)
                        {
                            GenericFileHandler.AppendContentsToStoredFile(modFilePath, internalName);
                        }
                        else if (internalName == InternalConfig.InternalLuaConfigPath)
                        {
                            AssignmentListHandler.AddToAssignmentList(modFilePath, internalName);
                        }
                        else if (modFilePath.Contains(InternalConfig.ModDefaultListsDir))
                        {
                            ArrayHandler.AddToArray(modFilePath);
                        }
                        else if (modFilePath.EndsWith(".lua"))
                        {
                            GenericFileHandler.CopyScriptFromModDir(modFilePath);
                        }
                        else if (modFilePath.EndsWith(".scheme", StringComparison.Ordinal) || modFilePath.EndsWith(".imageset", StringComparison.Ordinal))
                        {
                            if (QuickBmsUnpacker.Unpack(new string[] { "gui" }))
                            {
                                // adding to GameLook1.imageset is actually obsolete as of 0.9.6042 (but still possible)
                                XmlHandler.AddToXml(modFilePath, internalName);
                            }
                        }
                        else if (!modFilePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) && !modFilePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) // this is some other file
                        {
                            GenericFileHandler.CopyFileFromModDir(modFilePath);
                        }
                    }
                    Console.WriteLine();
                    CsvHandler.CreateGameFilesFromTables();
                    ArrayHandler.CreateFilesFromArrays();
                    XmlHandler.CreateFilesFromXml();
                    ListHandler.CreateFilesFromLists();
                    AssignmentListHandler.CreateFilesFromLists();
                    GenericFileHandler.WriteStoredFiles();

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