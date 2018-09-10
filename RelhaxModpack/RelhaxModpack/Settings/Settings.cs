using System.IO;


namespace RelhaxModpack
{
    #region Settings Enumerations
    /// <summary>
    /// Database distribution levels
    /// </summary>
    public enum DatabaseVersions
    {
        /// <summary>
        /// The stable public database
        /// </summary>
        Stable,
        /// <summary>
        /// The unstable public beta database
        /// </summary>
        Beta,
        /// <summary>
        /// The unstable private testing database.
        /// </summary>
        Test
    }
    #endregion
    //TODO: documentation
    public static class Settings
    {
        #region Settings constants
        /// <summary>
        /// The Startup root path of the application. Does not include the application name
        /// </summary>
        public static readonly string ApplicationStartupPath = System.AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// The absolute name of the application settings file
        /// </summary>
        public const string ModpackSettingsFileName = "RelhaxSettings.xml";

        public const string OldModpackSettingsFilename = "RelHaxSettings.xml";

        public const string UISettingsFileName = "UISettings.xml";

        public const string ThirdPartySettingsFileName = "ThirdPartySettings.xml";

        //the config file version for saving the user's selection prefrences
        public const string ConfigFileVersion = "2.0";
        /// <summary>
        /// The current distribution version of the application
        /// Alhpa should NEVER be built for public distribution unless direct testing!
        /// </summary>
        public const ApplicationVersions ApplicationVersion = ApplicationVersions.Alpha;

        public static readonly string RelhaxDownloadsFolder = Path.Combine(ApplicationStartupPath, "RelhaxDownloads");

        public static readonly string RelhaxModBackupFolder = Path.Combine(ApplicationStartupPath, "RelhaxModBackup");

        public static readonly string RelhaxLibrariesFolder = Path.Combine(ApplicationStartupPath, "RelhaxLibraries");

        public static readonly string RelhaxUserConfigsFolder = Path.Combine(ApplicationStartupPath, "RelhaxUserConfigs");

        public static readonly string RelhaxUserModsFolder = Path.Combine(ApplicationStartupPath, "RelhaxUserMods");

        public static readonly string RelhaxTempFolder = Path.Combine(ApplicationStartupPath, "RelhaxTemp");

        public static readonly string MD5HashDatabaseXmlFile = Path.Combine(RelhaxDownloadsFolder, "MD5HashDatabase.xml");
        #endregion

        #region Settings statics
        //application command line level settings
        //also serves as place to put default values
        public static bool FirstLoad = false;
        public static bool FirstLoadToV2 = false;
        public static DatabaseVersions DatabaseDistroVersion = RelhaxModpack.DatabaseVersions.Stable;
        public static bool SkipUpdate = false;
        public static bool SilentStart = false;
        public static bool ForceVisible = false;
        public static bool ForceEnabled = false;
        public static bool PatchCheck = false;
        public static bool DatabaseUpdate = false;
        public static bool DatabaseEdit = false;
        //use the filename as check for auto install
        public static string AutoInstallFileName = string.Empty;
        //use key filename as check for update key mode
        public static string UpdateKeyFileName = string.Empty;
        public static string EditorAutoLoadFileName = string.Empty;
        //TODO: make some of these properties so that the get can return eithor the modpack standalone or third party version
        public static string DefaultStartAddress = @"http://wotmods.relhaxmodpack.com/WoT/{onlineFolder}/";
        public static string DefaultEndAddress = @"";
        //file and folder macro locations
        public static string AppDataFolder = "";
        public static string TanksLocation = "";
        //version informations
        public static string TanksVersion = "";
        public static string TanksOnlineFolderVersion = "";
        //needed to create to first line to installedRelhaxFiles.log
        public static string DatabaseVersion = "";
        public static string CustomModInfoPath = "";
        #endregion
        public static void ParseCommandLineConflicts()
        {
            //check for conflicting command line arguements
        }
        #region Settings refrences and init

        public static ModpackSettings ModpackSettings;

        public static UISettings UISettings;

        public static ThirdPartySettings ThirdPartySettings;

        public static bool InitSettings()
        {
            //legacy compatibility: move the old settings name to the new settings name
            if (File.Exists(Path.Combine(ApplicationStartupPath, OldModpackSettingsFilename)))
                File.Move(Path.Combine(ApplicationStartupPath, OldModpackSettingsFilename), Path.Combine(ApplicationStartupPath, ModpackSettingsFileName));
            if (ModpackSettings != null)
                throw new BadMemeException("What the hell you doing fool?");
            ModpackSettings = new ModpackSettings(Path.Combine(ApplicationStartupPath, ModpackSettingsFileName));
            ModpackSettings.LoadSettings();
            return true;
        }
        #endregion
    }
}
