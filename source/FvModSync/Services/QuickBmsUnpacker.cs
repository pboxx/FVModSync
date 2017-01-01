namespace FVModSync.Services
{
    using System;
    using System.Diagnostics;

    public class QuickBmsUnpacker
    {
        public static void Unpack(string pakName, string exportFolderName)
        {
            // unpack .pak in case user doesn't have it unpacked already
            // TODO find less pedestrian way (than just "if dir exists") to determine whether those exports are actually valid
            // TODO error catching if something goes wrong here (like cry if pak not found, quickbms missing, life_is_feudal.bms missing etc)

            Console.WriteLine("Exporting {0} from game files to {1} ... ", pakName, exportFolderName);

            Process quickbms = new Process();
            quickbms.StartInfo.FileName = @"quickbms\quickbms.exe";
            quickbms.StartInfo.Arguments = @"-o -q -Y -Q quickbms\life_is_feudal.bms " + pakName + " " + exportFolderName;
            quickbms.StartInfo.UseShellExecute = false;
            quickbms.StartInfo.RedirectStandardInput = true;
            quickbms.Start();
            quickbms.StandardInput.WriteLine("\n");
            quickbms.WaitForExit(1000);

            if (quickbms.ExitCode != 0)
            {
                throw new InvalidOperationException(string.Format("quickbms exited with code {0} -- please check that it is set up correctly.", quickbms.ExitCode));
            }
        }
    }
}
