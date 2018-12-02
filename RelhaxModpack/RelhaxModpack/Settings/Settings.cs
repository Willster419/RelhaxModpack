using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack
{
    /// <summary>
    /// Handles all generic constants and statics used throughout the application
    /// </summary>
    public static class Settings
    {
        #region Constants
        /// <summary>
        /// The Startup root path of the application. Does not include the application name
        /// </summary>
        public static readonly string ApplicationStartupPath = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// The absolute name of the application settings file
        /// </summary>
        public const string ModpackSettingsFileName = "RelhaxSettings.xml";

        public const string OldModpackSettingsFilename = "RelHaxSettings.xml";

        public const string UISettingsFileName = "UISettings.xml";

        public const string ThirdPartySettingsFileName = "ThirdPartySettings.xml";

        //the config file version for saving the user's selection prefrences
        public const string ConfigFileVersion = "2.0";
        public const string ApplicationUpdateURL = "http://wotmods.relhaxmodpack.com/RelhaxModpack/RelhaxModpack.zip";
        public const string ApplicationBetaUpdateURL = "http://wotmods.relhaxmodpack.com/RelhaxModpack/RelhaxModpackBeta.zip";
        public const string ApplicationUpdateFileName = "RelhaxModpack_update.zip";
        public const string RelicBatchUpdateScript = "RelicCopyUpdate.bat";
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

        public static readonly string[] FoldersToCheck = new string[]
        {
            RelhaxDownloadsFolder,
            RelhaxModBackupFolder,
            RelhaxLibrariesFolder,
            RelhaxUserConfigsFolder,
            RelhaxUserModsFolder,
            RelhaxTempFolder
        };

        public static readonly string MD5HashDatabaseXmlFile = Path.Combine(RelhaxDownloadsFolder, "MD5HashDatabase.xml");

        public static readonly string ManagerInfoDatFile = Path.Combine(RelhaxTempFolder, "managerInfo.dat");

        public static readonly int NumLogicalProcesors = Environment.ProcessorCount;

        public const string PatchFolderName = "_patch";

        public const string ShortcutFolderName = "_shortcuts";

        public const string XmlUnpackFolderName = "TODO";

        public const string AtlasCreationFoldername = "TODO";
        #endregion

        #region Statics
        //file and folder macro locations
        public static string AppDataFolder = "";
        public static string WoTDirectory = "";
        //version informations
        public static string WoTClientVersion = "";
        public static string WoTModpackOnlineFolderVersion = "";
        public static string DatabaseVersion = "";
        public static bool FirstLoad = false;
        public static bool FirstLoadToV2 = false;
        #endregion

        #region Settings that can be over-ridden by loading thirdPartySettings
        //TODO: make some of these properties so that the get can return eithor the modpack standalone or third party version
        public static string DefaultStartAddress = @"http://wotmods.relhaxmodpack.com/WoT/{onlineFolder}/";
        public static string DefaultEndAddress = @"";
        #endregion
    }
}
