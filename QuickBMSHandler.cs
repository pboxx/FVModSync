using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace FVModSync
{
    public class QuickBMSHandler
    {
        public static void Unpack(string PakName, string ExportFolderName)
        {
            // unpack cfg.pak in case user doesn't have it unpacked already
            // TODO find less pedestrian way (than just "if dir exists") to determine whether those exports are actually valid

            // TODO error catching if something goes wrong here (like cry if pak not found)
            // oldversion: Process quickbms = Process.Start(@"quickbms\quickbms.exe", @"-F *.csv -o -q -Y -Q quickbms\life_is_feudal.bms ..\cfg.pak " + ExportFolder);

            Process quickbms = new Process();
            quickbms.StartInfo.FileName = @"quickbms\quickbms.exe";
            quickbms.StartInfo.Arguments = @"-o -q -Y -Q quickbms\life_is_feudal.bms " + PakName + " " + ExportFolderName;
            quickbms.StartInfo.UseShellExecute = false;
            quickbms.StartInfo.RedirectStandardInput = true;
            quickbms.Start();
            quickbms.StandardInput.WriteLine("\n");
            quickbms.WaitForExit(1000);

            Console.WriteLine("Export {0} from game files to {1}", PakName, ExportFolderName);
                      
        }
    }
}
