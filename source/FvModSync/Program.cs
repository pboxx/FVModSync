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
                string[] csvRecognisedPaths = ConfigReader.LoadCsvPaths();
                string[] modFiles = GenericFileHandler.SearchModFiles();

                QuickBmsUnpacker.Unpack(pakNames);

                foreach (string modFile in modFiles)
                {
                    string internalName = modFile.GetInternalName();
                    string gameFilePath = Config.GameFilePrefix + internalName;

                    if (modFile.EndsWith(".csv", StringComparison.Ordinal))
                    {
                        // is this a game file we handle
                        if (csvRecognisedPaths.Contains(internalName))
                        {
                            LibraryHandler.CopyFileToDict(modFile, internalName);
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
                    else if (!modFile.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) && !modFile.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) // this is some other file
                    {
                        GenericFileHandler.CopyFileFromModDir(modFile);
                    }
                }

                Console.WriteLine();

                //foreach (string relevantPath in csvRecognisedPaths)
                //{
                //    LibraryHandler.CreateGameFileFromLibrary(relevantPath);
                //}

                LibraryHandler.CreateGameFilesFromLibrary();

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