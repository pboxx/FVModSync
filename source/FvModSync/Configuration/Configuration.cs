namespace FVModSync.Configuration
{
    using System.Collections.Generic;

    public class InternalConfig
    {
        public const string AppVersion = "0.2.2beta";
        public const string VersionBlurb = "FVModSync" + AppVersion + "\nMod installer for Life is Feudal: Forest Village\n(c) pbox 2016\nPublished under the GPLv3 https://www.gnu.org/licenses/gpl-3.0.txt \n\n";
        public static readonly string[] PakNames = { "cfg", @"scripts\core" };
        public const string InternalLuaInitPath = @"scripts\core\init.lua";
        public const string InternalLuaConfigPath =  @"scripts\core\config.lua";
        public const string IgnoreCsvFieldString = @"fvsm:ignore";
        public static readonly string[] modDefaultArrays = {    @"scripts\mods\_defaults\live\Worker_boozeOptions.lua", 
                                                                @"scripts\mods\_defaults\live\Worker_dressOptions.lua",
                                                                @"scripts\mods\_defaults\live\Worker_toolOptions.lua",
                                                                @"scripts\mods\_defaults\buildings\SawmillSimple_craftRecipes.lua",
                                                           };
    }

    public sealed class ExternalConfig
    {
        private static string currentGameVersion = "0.0.6042";
        private static string currentGameFilePrefix = "..";
        private static string currentGameFileBackupSuffix = ".backup";
        private static string currentExportFolderName = "FVModSync_exportedFiles";
        private static string currentModsSubfolderName = "mods";
        private static string currentModDefaultsSubfolderName = "_defaults";
        private static string currentConsoleVerbosity = "normal";

        private static List<string> currentFileLocations = new List<string> 
        {   
            @"\cfg\abilitiesButtons.csv",
            @"\cfg\actions.csv",
            @"\cfg\buttons.csv",
            @"\cfg\dress.csv",
            @"\cfg\firstPersonCameraSettings.csv",
            @"\cfg\housesVisualResources.csv",
            @"\cfg\land.csv",
            @"\cfg\limits.csv",
            @"\cfg\Localization.csv",
            @"\cfg\nomads.csv",
            @"\cfg\professionKit.csv",
            @"\cfg\professionWithoutLaborerTasks.csv",
            @"\cfg\resourcesLevelGeneration.csv",
            @"\cfg\resourcesReplacement.csv",
            @"\cfg\soundtracks.csv",
            @"\cfg\tools.csv",
            @"\cfg\undergrowth.csv",
            @"\cfg\normal\abilities.csv",
            @"\cfg\normal\animals.csv",
            @"\cfg\normal\buffs.csv",
            @"\cfg\normal\craft.csv",
            @"\cfg\normal\discovery.csv",
            @"\cfg\normal\disease.csv",
            @"\cfg\normal\foodDeclining.csv",
            @"\cfg\normal\game.csv",
            @"\cfg\normal\houses.csv",
            @"\cfg\normal\plants.csv",
            @"\cfg\normal\resources.csv",
            @"\cfg\normal\resParams.csv",
            @"\cfg\normal\tornado.csv",
        };

        public static string GameVersion = currentGameVersion;
        public static string GameFilePrefix = currentGameFilePrefix;
        public static string GameFileBackupSuffix = currentGameFileBackupSuffix;
        public static string ExportFolderName = currentExportFolderName;
        public static string ModsSubfolderName = currentModsSubfolderName;
        public static string ModDefaultsSubfolderName = currentModDefaultsSubfolderName;
        public static string ConsoleVerbosity = currentConsoleVerbosity;
        public static List<string> FileLocations = currentFileLocations;
    }
}