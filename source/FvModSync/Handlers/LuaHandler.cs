namespace FVModSync.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class LuaHandler
    {
        private static readonly List<string> luaIncludes = new List<string>();

        public static void BackupAndCopy(string gameFilePath, string exportedFilePath)
        {
            if (File.Exists(gameFilePath))
            {
                string backupFilePath = gameFilePath + ".backup";

                File.Delete(backupFilePath);
                File.Copy(gameFilePath, backupFilePath);

                Console.WriteLine("File exists: {0} -- create backup on disk", gameFilePath);

                // copy existing game file to internal list
                LuaHandler.CopyFileToIncludeList(gameFilePath);
                Console.WriteLine("Copy file content to list: {0} ", gameFilePath);
            }
            else // copy from exported file (if it exists)
            {
                if (!File.Exists(exportedFilePath))
                {
                    throw new FileNotFoundException("Exported file not found. Try deleting the FVModSync_exportedFiles folder and running the program again", exportedFilePath);
                }

                LuaHandler.CopyFileToIncludeList(exportedFilePath);
                Console.WriteLine("Copy file content to list: {0} ", exportedFilePath);
            }
        }

        public static void CopyFileToIncludeList(string sourceFile)
        {
            using (Stream stream = File.Open(sourceFile, FileMode.Open))
            {
                StreamReader reader = new StreamReader(stream);

                string content = reader.ReadToEnd();
                string[] contentLines = content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string contentLine in contentLines)
                {
                    if (!luaIncludes.Contains(contentLine))
                    {
                        luaIncludes.Add(contentLine);
                    }
                }
            }
        }

        public static void CreateIncludeFileFromList(string gameFilePath)
        {
            if (luaIncludes.Any())
            {
                string targetDir = @".." + Path.GetDirectoryName(gameFilePath);
                Directory.CreateDirectory(targetDir);

                using (Stream gameFile = File.Open(@".." + gameFilePath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(gameFile))
                    {
                        string[] contentLines = luaIncludes.ToArray();

                        foreach (string contentLine in contentLines)
                        {
                            writer.WriteLine(contentLine);
                        }
                    }
                }
                Console.WriteLine(@"Write list to game files: {0}", gameFilePath);
            }
        }
    }
}