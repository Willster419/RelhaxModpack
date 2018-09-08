using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Settings
{
    #region Settings Enumerations

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
        public const string ModpackSettingsFileName = "RelHaxSettings.xml";

        public const string UISettingsFileName = "UISettings.xml";

        public const string ThirdPartySettingsFileName = "ThirdPartySettings.xml";

        //the config file version for saving the user's selection prefrences
        public const string ConfigFileVersion = "2.0";

        public static readonly string RelhaxDownloadsFolder = Path.Combine(ApplicationStartupPath, "RelHaxDownloads");

        public static readonly string RelhaxModBackupFolder = Path.Combine(ApplicationStartupPath, "RelHaxModBackup");

        public static readonly string RelhaxLibrariesFolder = Path.Combine(ApplicationStartupPath, "RelHaxLibraries");

        public static readonly string RelhaxTempFolder = Path.Combine(ApplicationStartupPath, "RelHaxTemp");

        public static readonly string MD5HashDatabaseXmlFile = Path.Combine(RelhaxDownloadsFolder, "MD5HashDatabase.xml");

        public static readonly string OnlineDatabaseXmlFile = Path.Combine(RelhaxDownloadsFolder, "onlineDatabase.xml");
        #endregion

        #region Settings statics
        //TODO: make these properties so that the get can return eithor the modpack standalone or third party version
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

        #region Settings refrences and init

        public static ModpackSettings ModpackSettings;

        public static UISettings UISettings;

        public static ThirdPartySettings ThirdPartySettings;

        public static bool InitSettings()
        {
            return true;
        }
        #endregion
    }
}
