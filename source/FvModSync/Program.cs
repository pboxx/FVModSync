/*
Mod installer for Life is Feudal: Forest Village
Published under the GNU General Public License https://www.gnu.org/licenses/gpl-3.0.txt
*/

namespace FVModSync
{
	using System;
	using System.IO;
	using System.Linq;

	using FVModSync.Configuration;
    using FVModSync.Extensions;
	using FVModSync.Handlers;
	using FVModSync.Services;
    using System.Diagnostics;

	/// <summary>
    /// The main program.
    /// </summary>
    public static class Program
    {
        // TODO verbose switch

        /// <summary>
        /// The main entry point.
        /// </summary>
        public static void Main(string[] args)
        {
            Console.WriteLine(Config.Version);

            try
            {
                string[] pakNames = { "cfg", "scripts" };
                QuickBmsUnpacker.Unpack(pakNames);

                string[] csvRecognisedPaths = ConfigReader.LoadCsvPaths();
                string[] modFiles = GenericFileHandler.SearchModFiles();

                foreach (string modFile in modFiles)
                {
                    string internalName = modFile.GetInternalName();
                    string gameFilePath = Config.GameFilePrefix + internalName;

                    if (modFile.EndsWith(".csv", StringComparison.Ordinal))
                    {
                        // is this a game file we handle
                        if (csvRecognisedPaths.Contains(internalName))
                        {
                            if (!LibraryHandler.RecordExists(internalName))
                            {
                                // create internal dictionary
                                LibraryHandler.BackupAndCopy(internalName);
                            }
                            LibraryHandler.CopyModdedFileToDict(modFile);

                            Console.WriteLine("Copy CSV content to dictionary: {0}", modFile);
                        }
                        else // this is a custom csv
                        {
                            GenericFileHandler.CopyFileFromModDir(modFile);
                        }
                    }
                    else if (internalName == Config.InternalLuaIncludePath)
                    {
                        GenericFileHandler.BackupIfExists(gameFilePath);
                        ListHandler.AddToList(internalName, modFile);
                    }
                    else if (!modFile.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) // this is some other file
                    {
                        GenericFileHandler.CopyFileFromModDir(modFile);
                    }
                }

                Console.WriteLine();

                foreach (string relevantPath in csvRecognisedPaths)
                {
                    LibraryHandler.CreateGameFileFromLibrary(relevantPath);
                }

                ListHandler.CreateFileFromList(Config.InternalLuaIncludePath);

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
    }
}