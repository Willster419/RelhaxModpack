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
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using System.Xml.Linq;
using RelhaxModpack.Common;

namespace RelhaxModpack.Settings
{
    /// <summary>
    /// Provides access to all settings used in the modpack.
    /// </summary>
    public class ModpackSettings : ISettingsFile
    {
        /// <summary>
        /// The name of the settings file on disk
        /// </summary>
        public const string SettingsFilename = "RelhaxSettingsV2.xml";

        /// <summary>
        /// The name of the Relhax V1 settings file on disk
        /// </summary>
        public const string OldSettingsFilename = "RelHaxSettings.xml";

        /// <summary>
        /// The name of the xml file on disk
        /// </summary>
        public string Filename { get { return SettingsFilename; } }

        /// <summary>
        /// A list of properties and fields to exclude from saving/loading to and from xml
        /// </summary>
        public string[] MembersToExclude { get { return new string[] { nameof(MembersToExclude), nameof(Filename), nameof(SettingsFilename), nameof(OldSettingsFilename) }; } }

        #region Save-able modpack settings
        /// <summary>
        /// The custom path to the ModInfo.xml file if loading in test mode
        /// </summary>
        public string CustomModInfoPath = string.Empty;

        /// <summary>
        /// The name of the git branch to use when downloading the database. Uses the RelhaxModpackDatabase repository
        /// </summary>
        /// <remarks>URL for the repository: https://github.com/Willster419/RelhaxModpackDatabase </remarks>
        public string BetaDatabaseSelectedBranch = "master";

        /// <summary>
        /// Flag to track when/if the user has MSVCP2013 installed to load the atlas image processing libraries
        /// </summary>
        public bool AtlasLibrariesCanBeLoaded = false;

        /// <summary>
        /// Show a warning (once) if the user launched the application right from the downloads folder
        /// </summary>
        public bool InformIfApplicationInDownloadsFolder = true;

        /// <summary>
        /// Toggle if the program should notify the user if the database version is the same as the last installed version
        /// </summary>
        public bool NotifyIfSameDatabase = false;

        /// <summary>
        /// Toggle if the program will backup the current mod installation
        /// </summary>
        public bool BackupModFolder = false;

        /// <summary>
        /// Toggle if the program will clean the mods and res_mods folders before installation
        /// </summary>
        public bool CleanInstallation = true;

        /// <summary>
        /// Toggle if the program should force the user to manually point to the WoT location
        /// </summary>
        public bool ForceManuel = false;

        /// <summary>
        /// Toggle if the application should automatically save the last selected config to also be automatically loaded upon selection load
        /// </summary>
        public bool SaveLastSelection = true;

        /// <summary>
        /// Toggle if the application should save user cache save data like session stats, or auto equip configs
        /// </summary>
        public bool SaveUserData = false;

        /// <summary>
        /// Toggle if the application should save disabled mods in the user's selection file. If selected, when a mod is enabled again, it will be automatically selected
        /// </summary>
        public bool SaveDisabledMods = true;

        /// <summary>
        /// A one time run bool to display a message about how to use user mods
        /// </summary>
        public bool DisplayUserModsWarning = true;

        /// <summary>
        /// A one time run bool to display a message about how the user can right click a selection to get a preview window
        /// </summary>
        public bool DisplaySelectionPreviewMessage = true;

        /// <summary>
        /// Toggle if the application should use debug level logging or just info
        /// </summary>
        public bool VerboseLogging = false;

        /// <summary>
        /// Toggle if the application can gather statistic data about it's usage
        /// </summary>
        /// <remarks>The data that the modpack gathers is only the list of packages you selected for installation. This helps us determine which mods are popular, and for how best to
        /// prioritize which mods to work on first in the event the support team is overloaded</remarks>
        public bool AllowStatisticDataGather = true;

        /// <summary>
        /// Toggle for each view if the borders around the child selection options should show
        /// </summary>
        public bool EnableBordersLegacyView = false;

        /// <summary>
        /// Toggle for each view if the borders around the child selection options should show
        /// </summary>
        public bool EnableBordersDefaultV2View = false;

        /// <summary>
        /// Toggle for each view if the color change should occur when a child selection happens
        /// </summary>
        public bool EnableColorChangeLegacyView = false;

        /// <summary>
        /// Toggle for each view if the color change should occur when a child selection happens
        /// </summary>
        public bool EnableColorChangeDefaultV2View = false;

        /// <summary>
        /// Toggle if the installation complete window will be shown
        /// </summary>
        public bool ShowInstallCompleteWindow = false;

        /// <summary>
        /// Toggle if the program will delete the WoT appdata cache
        /// </summary>
        public bool ClearCache = false;

        /// <summary>
        /// Toggle if the program will delete xvm, pmod, WoT logs
        /// </summary>
        public bool DeleteLogs = false;

        /// <summary>
        /// Toggle if the program will create desktop shortcuts
        /// </summary>
        public bool CreateShortcuts = false;

        /// <summary>
        /// Toggle the ability for the modpack to extract a package as soon as it is downloaded
        /// </summary>
        public bool InstallWhileDownloading = false;

        /// <summary>
        /// Toggle the ability to have multiple extractions happening at the same time
        /// </summary>
        public bool MulticoreExtraction = false;

        /// <summary>
        /// Toggle export mode
        /// </summary>
        public bool ExportMode = false;

        /// <summary>
        /// Force all packages to be enabled in the selection list
        /// </summary>
        public bool ForceEnabled = false;

        /// <summary>
        /// Force all packages to be visible in the selection list
        /// </summary>
        public bool ForceVisible = false;

        /// <summary>
        /// When selected, the installation engine will disable event triggers.
        /// </summary>
        /// <remarks>The triggers, when enabled, enable the application to start some install tasks when all required packages are downloaded</remarks>
        public bool DisableTriggers = true;

        /// <summary>
        /// Toggle one click install mode
        /// </summary>
        /// <remarks>Allows the user to install a selection when clicking the install button</remarks>
        public bool OneClickInstall = false;

        /// <summary>
        /// Toggle auto install mode
        /// </summary>
        /// <remarks>Allows the user to have the application automatically check for and install a selection file when the database updates</remarks>
        public bool AutoInstall = false;

        /// <summary>
        /// Toggle the advanced installation progress window
        /// </summary>
        public bool AdvancedInstalProgress = false;

        /// <summary>
        /// True - After an installation the application will ask to delete old download cache files
        /// False - After an installation the application will always delete old download cache files
        /// </summary>
        public bool DeleteCacheFiles = false;

        /// <summary>
        /// Toggle for if the Preview window should be shown in fullscreen mode
        /// </summary>
        public bool PreviewFullscreen = false;

        /// <summary>
        /// Toggle for if the ModSelectionView window should be shown in fullscreen mode
        /// </summary>
        public bool ModSelectionFullscreen = false;

        /// <summary>
        /// Toggle for minimizing the application to the system tray (After the application is done installing mods I presume?)
        /// </summary>
        public bool MinimizeToSystemTray = false;

        /// <summary>
        /// Toggle for if the selection list options in Legacy view should be collapsed by default on start
        /// </summary>
        public bool ShowOptionsCollapsedLegacy = false;

        /// <summary>
        /// Toggle if during auto or one click load if the application should inform if any selection errors during selection file load
        /// </summary>
        public bool AutoOneclickShowWarningOnSelectionsFail = false;

        /// <summary>
        /// Toggle if the application should apply a custom font to the MainWindow and all windows of RelhaxWindow with 
        /// </summary>
        public bool EnableCustomFont = false;

        /// <summary>
        /// Flag to enable a check if the user is running the minimum required .NET Framework version for this application
        /// </summary>
        public bool ValidFrameworkVersion = false;

        /// <summary>
        /// Toggle to enable minimalist mode during install. It won't install additional recommended files, only minimum files to allow the packages to work.
        /// </summary>
        /// <seealso cref="Database.DatabasePackage.MinimalistModeExclude"/>
        public bool MinimalistMode = false;

        /// <summary>
        /// The time, in a specified unit, to check for anew data base version
        /// </summary>
        public int AutoInstallFrequencyInterval = 10;

        /// <summary>
        /// The time unit to use for the interval (mins, hours, etc)
        /// </summary>
        public int AutoInstallFrequencyTimeUnit = 0;

        /// <summary>
        /// The height, in pixels, of the ModSelectionView window
        /// </summary>
        public int ModSelectionHeight = 480;

        /// <summary>
        /// The width, in pixels, of the ModSelectionView window
        /// </summary>
        public int ModSelectionWidth = 800;

        /// <summary>
        /// The x-coordinate location, in pixels, of the Preview window
        /// </summary>
        public int PreviewX = 0;

        /// <summary>
        /// The y-coordinate location, in pixels, of the Preview window
        /// </summary>
        public int PreviewY = 0;

        /// <summary>
        /// The height, in pixels, of the Preview window
        /// </summary>
        public int PreviewHeight = 550;

        /// <summary>
        /// The width, in pixels, of the Preview window
        /// </summary>
        public int PreviewWidth = 450;

        /// <summary>
        /// The factor to scale the window size and components by. From 1 to 3 in increments of 0.25
        /// </summary>
        public double DisplayScale = 1.0f;

        /// <summary>
        /// The download mirror to use when running the download manager
        /// </summary>
        /// <remarks>0 based index for easy usage into the ApplicationConstants.DownloadMirrors field</remarks>
        /// <seealso cref="ApplicationConstants.DownloadMirrors"/>
        public int DownloadMirror = 0;

        /// <summary>
        /// The path to the auto or one click selection file
        /// </summary>
        /// <remarks>Used for both auto and one click installation modes</remarks>
        public string AutoOneclickSelectionFilePath = string.Empty;

        /// <summary>
        /// The name of the font to use for the MainWindow and all windows that are of RelhaxWindow type
        /// </summary>
        /// <remarks>If the font name is not found in the system folder, this is reset to empty</remarks>
        public string CustomFontName = string.Empty;

        /// <summary>
        /// The Uninstall mode to use when uninstalling or installing with the clean install option
        /// </summary>
        public UninstallModes UninstallMode = UninstallModes.Default;

        /// <summary>
        /// The selection view to use
        /// </summary>
        public SelectionView ModSelectionView = SelectionView.Legacy;

        /// <summary>
        /// The selected language
        /// </summary>
        public Languages Language = Languages.English;

        /// <summary>
        /// The distribution version of the database to use when selecting mods
        /// </summary>
        public DatabaseVersions DatabaseDistroVersion = DatabaseVersions.Stable;

        /// <summary>
        /// The application distribution version to use. When selected, it won't take affect until application restart
        /// </summary>
        public ApplicationVersions ApplicationDistroVersion = ApplicationVersions.Stable;

        /// <summary>
        /// The theme to apply to the application
        /// </summary>
        public UIThemes ApplicationTheme = UIThemes.Default;
        #endregion

        #region Legacy compatibility
        /// <summary>
        /// Check for old poorly named settings that don't directly match setting property names
        /// </summary>
        /// <param name="settings">The node that holds all the settings nodes</param>
        public void ApplyLegacySettings(List<XElement> settings)
        {
            for (int i = 0; i < settings.Count; i++)
            {
                XElement setting = settings[i];
                switch (setting.Name.LocalName)
                {
                    //put legacy names here to direct name change
                    case "backupModFolder":
                        BackupModFolder = bool.Parse(setting.Value);
                        break;
                    case "cleanInstallation":
                        CleanInstallation = bool.Parse(setting.Value);
                        break;
                    case "forceManuel":
                        ForceManuel = bool.Parse(setting.Value);
                        break;
                    case "saveLastConfig":
                        SaveLastSelection = bool.Parse(setting.Value);
                        break;
                    case "saveUserData":
                        SaveUserData = bool.Parse(setting.Value);
                        break;
                    case "EnableChildColorChangeLegacyView":
                        EnableColorChangeLegacyView = bool.Parse(setting.Value);
                        break;
                    case "EnableChildColorChangeDefaultV2View":
                        EnableColorChangeDefaultV2View = bool.Parse(setting.Value);
                        break;
                    case "clearCache":
                        ClearCache = bool.Parse(setting.Value);
                        break;
                    case "deleteLogs":
                        DeleteLogs = bool.Parse(setting.Value);
                        break;
                    case "modSelectionHeight":
                        ModSelectionHeight = int.Parse(setting.Value);
                        break;
                    case "modSelectionWidth":
                        ModSelectionWidth = int.Parse(setting.Value);
                        break;
                    case "previewX":
                        PreviewX = int.Parse(setting.Value);
                        break;
                    case "previewY":
                        PreviewY = int.Parse(setting.Value);
                        break;
                    case "language":
                        Language = (Languages)int.Parse(setting.Value);
                        break;
                    case "SelectionView":
                        ModSelectionView = (SelectionView)int.Parse(setting.Value);
                        break;
                    case "BetaApplication":
                        if(CommonUtils.ParseBool(setting.Value, false))
                        {
                            ApplicationDistroVersion = ApplicationVersions.Beta;
                        }
                        else
                        {
                            ApplicationDistroVersion = ApplicationVersions.Stable;
                        }
                        break;
                    case "BetaDatabase":
                        if (CommonUtils.ParseBool(setting.Value, false))
                        {
                            DatabaseDistroVersion = DatabaseVersions.Beta;
                        }
                        else
                        {
                            DatabaseDistroVersion = DatabaseVersions.Stable;
                        }
                        break;
                    case "SuperExtraction":
                        MulticoreExtraction = CommonUtils.ParseBool(setting.Value, false);
                        break;
                    case "InstantExtraction":
                        InstallWhileDownloading = CommonUtils.ParseBool(setting.Value, false);
                        break;
                }
            }
        }
        #endregion
    }
}
