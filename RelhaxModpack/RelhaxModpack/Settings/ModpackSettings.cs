using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Windows;
using System.Reflection;
using System.Globalization;

namespace RelhaxModpack
{
    /// <summary>
    /// The type of selection view for how to display the selection tree
    /// </summary>
    public enum SelectionView
    {
        /// <summary>
        /// OMC style
        /// </summary>
        Legacy = 1,
        /// <summary>
        /// Default WPF V2 style
        /// </summary>
        DefaultV2 = 0
    };
    /// <summary>
    /// The different loading Gifs for when loading the preview
    /// </summary>
    public enum LoadingGifs
    {
        /// <summary>
        /// The standard loading Gif
        /// </summary>
        Standard = 0,
        /// <summary>
        /// Thirdguards head loading Gif
        /// </summary>
        ThirdGuards = 1
    };
    /// <summary>
    /// The types on uninstall mods the application supports
    /// </summary>
    public enum UninstallModes
    {
        /// <summary>
        /// Default uninstall method to uninstall all modifiecation done by the application
        /// </summary>
        Default = 0,
        /// <summary>
        /// Quick uninstall method to remove 
        /// </summary>
        Quick = 1
    }
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
    /// <summary>
    /// Provides access to all settings used in the modpack.
    /// </summary>
    public static class ModpackSettings
    {
        //list of properties to exclude from the properties enumeration
        public static readonly string[] PropertiesToExclude = new string[]
        {
            //put blacklist fields here
            nameof(PropertiesToExclude)
        };
        #region saveable modpack settings
        /// <summary>
        /// The custom path to the ModInfo.xml file if loading in test mode
        /// </summary>
        public static string CustomModInfoPath = "";

        public static string BetaDatabaseSelectedBranch = "master";
        /// <summary>
        /// toggle if the program should notify the user if the database version is the same as the last installed version
        /// </summary>
        public static bool NotifyIfSameDatabase = false;
        /// <summary>
        /// toggle if the program will backup the current mod installation
        /// </summary>
        public static bool BackupModFolder = false;
        /// <summary>
        /// toggle if the program will clean the mods and res_mods folders before installation
        /// </summary>
        public static bool CleanInstallation = true;
        /// <summary>
        /// toggle if the program should force the user to manually point to the WoT location
        /// </summary>
        public static bool ForceManuel = false;
        /// <summary>
        /// toggle if the application should automatically save the last selected config to also be automatically loaded upon selection load
        /// </summary>
        public static bool SaveLastSelection = false;
        /// <summary>
        /// toggle if the application should save user cache save data like session stats, or auto equip configs
        /// </summary>
        public static bool SaveUserData = false;

        public static bool SaveDisabledMods = true;

        public static bool DisplayUserModsWarning = true;

        public static bool VerboseLogging = true;

        public static bool AllowStatisticDataGather = true;
        /// <summary>
        /// toggle for each view if the borders around the child selection options should show
        /// </summary>
        public static bool EnableBordersLegacyView = false;
        /// <summary>
        /// toggle for each view if the borders around the child selection options should show
        /// </summary>
        public static bool EnableBordersDefaultV2View = false;
        /// <summary>
        /// toggle for each view if the color change should occur when a child selection happends
        /// </summary>
        public static bool EnableColorChangeLegacyView = false;
        /// <summary>
        /// toggle for each view if the color change should occur when a child selection happends
        /// </summary>
        public static bool EnableColorChangeDefaultV2View = false;
        /// <summary>
        /// toggle if the installation complete window will be shown
        /// </summary>
        public static bool ShowInstallCompleteWindow = false;
        /// <summary>
        /// toggle if the program will delete the WoT appdata cache
        /// </summary>
        public static bool ClearCache = false;
        /// <summary>
        /// toggle if the program will delete xvm, pmod, WoT logs
        /// </summary>
        public static bool DeleteLogs = false;
        /// <summary>
        /// toggle if the program will create desktop shortcuts
        /// </summary>
        public static bool CreateShortcuts = false;
        /// <summary>
        /// toggle the ability for the modpack to extract a package as soon as it is downloaded
        /// </summary>
        public static bool InstallWhileDownloading = false;
        /// <summary>
        /// toggle the ability to have multiple extractions happening at the same time
        /// </summary>
        public static bool MulticoreExtraction = false;
        /// <summary>
        /// toggle export mode
        /// </summary>
        public static bool ExportMode = false;

        public static bool ForceEnabled = false;

        public static bool ForceVisible = false;

        public static bool DisableTriggers = true;

        public static bool OneClickInstall = false;

        public static bool AutoInstall = false;
        /// <summary>
        /// Toggle the advanced installation progress window
        /// </summary>
        public static bool AdvancedInstalProgress = false;
        /// <summary>
        /// True - After an installation the application will ask to delete old download cache files
        /// False - After an installation the application will always delete old download cache files
        /// </summary>
        public static bool DeleteCacheFiles = false;
        /// <summary>
        /// toggle for if the Preview window should be shown in fullscreen mode
        /// </summary>
        public static bool PreviewFullscreen = false;
        /// <summary>
        /// the height, in pixels, of the ModSelectionView window
        /// </summary>
        public static int ModSelectionHeight = 480;
        /// <summary>
        /// the width, in pixels, of the ModSelectionView window
        /// </summary>
        public static int ModSelectionWidth = 800;
        /// <summary>
        /// toggle for if the ModSelectionView window should be shown in fullscreen mode
        /// </summary>
        public static bool ModSelectionFullscreen = false;
        /// <summary>
        /// the x-coordinate location, in pixels, of the Preview window
        /// </summary>
        public static int PreviewX = 0;
        /// <summary>
        /// the y-coordinate location, in pixels, of the Preview window
        /// </summary>
        public static int PreviewY = 0;
        /// <summary>
        /// the height, in pixels, of the Preview window
        /// </summary>
        public static int PreviewHeight = 550;
        /// <summary>
        /// the width, in pixels, of the Preview window
        /// </summary>
        public static int PreviewWidth = 450;

        public static string AutoOneclickSelectionFilePath = string.Empty;

        /// <summary>
        /// The Uninstall mode to use when uninstalling or installing with the clean install option
        /// </summary>
        public static UninstallModes UninstallMode = UninstallModes.Default;
        /// <summary>
        /// The selection view to use
        /// </summary>
        public static SelectionView ModSelectionView = SelectionView.DefaultV2;
        /// <summary>
        /// The selected language
        /// </summary>
        public static Languages Language = Languages.English;
        /// <summary>
        /// The distribution version of the database to use when selecting mods
        /// </summary>
        public static DatabaseVersions DatabaseDistroVersion = DatabaseVersions.Stable;
        /// <summary>
        /// The application distribution version to use. When selected, it won't take affect until application restart
        /// </summary>
        public static ApplicationVersions ApplicationDistroVersion = ApplicationVersions.Stable;
        /// <summary>
        /// toggle for minimizing the application to the system tray (After the application is done installing mods I presume?)
        /// </summary>
        public static bool MinimizeToSystemTray = false;
        #endregion

        #region legacy compatability
        public static void ApplyOldSettings(XmlNodeList settings)
        {
            for (int i = 0; i < settings.Count; i++)
            {
                XmlNode setting = settings[i];
                switch (setting.Name)
                {
                    //put legacy names here to direct name change
                    //TODO: super extraction and instant extraction?
                    case "backupModFolder":
                        BackupModFolder = bool.Parse(setting.InnerText);
                        break;
                    case "cleanInstallation":
                        CleanInstallation = bool.Parse(setting.InnerText);
                        break;
                    case "forceManuel":
                        ForceManuel = bool.Parse(setting.InnerText);
                        break;
                    case "saveLastConfig":
                        SaveLastSelection = bool.Parse(setting.InnerText);
                        break;
                    case "saveUserData":
                        SaveUserData = bool.Parse(setting.InnerText);
                        break;
                    case "EnableChildColorChangeLegacyView":
                        EnableColorChangeLegacyView = bool.Parse(setting.InnerText);
                        break;
                    case "EnableChildColorChangeDefaultV2View":
                        EnableColorChangeDefaultV2View = bool.Parse(setting.InnerText);
                        break;
                    case "clearCache":
                        ClearCache = bool.Parse(setting.InnerText);
                        break;
                    case "deleteLogs":
                        DeleteLogs = bool.Parse(setting.InnerText);
                        break;
                    case "modSelectionHeight":
                        ModSelectionHeight = int.Parse(setting.InnerText);
                        break;
                    case "modSelectionWidth":
                        ModSelectionWidth = int.Parse(setting.InnerText);
                        break;
                    case "previewX":
                        PreviewX = int.Parse(setting.InnerText);
                        break;
                    case "previewY":
                        PreviewY = int.Parse(setting.InnerText);
                        break;
                    case "language":
                        Language = (Languages)int.Parse(setting.InnerText);
                        break;
                    case "SelectionView":
                        ModSelectionView = (SelectionView)int.Parse(setting.InnerText);
                        break;
                    case "BetaApplication":
                        ApplicationDistroVersion = ApplicationVersions.Beta;
                        break;
                    case "BetaDatabase":
                        DatabaseDistroVersion = DatabaseVersions.Beta;
                        break;
                }
            }
        }
        #endregion
    }
}
