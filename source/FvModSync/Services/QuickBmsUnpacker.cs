namespace FVModSync.Services
{
    using FVModSync.Configuration;
    using System;
    using System.Diagnostics;
    using System.IO;

    public class QuickBmsUnpacker
    {
        public static bool Unpack(string[] pakNames)
        {
            string scriptPath = "";

            if (!File.Exists(@"quickbms\life_is_feudal.bms"))
            {
                if (File.Exists(@"quickbms\life_is_feudal.bms.txt"))
                {
                    scriptPath = @"quickbms\life_is_feudal.bms.txt";
                }
                else
                {
                    throw new FileNotFoundException(string.Format("quickbms script (life_is_feudal.bms) not found -- please put it in the quickbms folder."));
                }
            }
            else
            {
                scriptPath = @"quickbms\life_is_feudal.bms";
            }    
            
            foreach (string pakName in pakNames)
            {
                // TODO find less pedestrian way (than just "if dir exists") to determine whether exports are valid

                string pakPath = Config.GameFilePrefix + @"\" + pakName + ".pak";
                string pakExportDir = Config.ExportFolderName + @"\" + pakName;

                if (!Directory.Exists(pakExportDir)) 
                {
                    Console.WriteLine("Exporting {0} from game files to {1} ... ", pakPath, Config.ExportFolderName);

                    Process quickbms = new Process();
                    quickbms.StartInfo.FileName = @"quickbms\quickbms.exe";
                    quickbms.StartInfo.Arguments = @"-o -q -Y -Q " + scriptPath + " " + pakPath + " " + Config.ExportFolderName;
                    quickbms.StartInfo.UseShellExecute = false;
                    quickbms.StartInfo.RedirectStandardInput = true;
                    quickbms.Start();
                    quickbms.StandardInput.WriteLine("\n");
                    quickbms.WaitForExit();

                    if (quickbms.ExitCode != 0)
                    {
                        throw new InvalidOperationException(string.Format("quickbms exited with code {0} -- please check that it is set up correctly.", quickbms.ExitCode));
                    }            
                }
                else
                {
                    Console.WriteLine("Export directory {0} exists ", pakExportDir);
                }
            }
            Console.WriteLine();
            return true;
        }
    }
}
