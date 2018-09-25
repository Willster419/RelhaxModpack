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
        /// Default Winforms style
        /// </summary>
        Default = 0,
        /// <summary>
        /// OMC style
        /// </summary>
        Legacy = 1,
        /// <summary>
        /// Default WPF V2 style
        /// </summary>
        DefaultV2 = 2
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
        private static readonly string[] PropertiesToExclude = new string[]
        {
            //put blacklist fields here
        };
        #region saveable modpack settings
        /// <summary>
        /// The custom path to the ModInfo.xml file if loading in test mode
        /// </summary>
        public static string CustomModInfoPath = "";
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
        public static bool SaveLastConfig = false;
        /// <summary>
        /// toggle if the application should save user cache save data like session stats, or auto equip configs
        /// </summary>
        public static bool SaveUserData = false;
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
        public static bool DownloadInstantExtraction = false;
        /// <summary>
        /// toggle the ability to have multiple extractions happening at the same time
        /// </summary>
        public static bool MulticoreExtraction = false;
        /// <summary>
        /// toggle export mode
        /// </summary>
        public static bool ExportMode = false;
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
        /// toggle for if the Preview window should be shown in fullscreen mode
        /// </summary>
        public static bool PreviewFullscreen = false;
        /// <summary>
        /// the height, in pixels, of the Preview window
        /// </summary>
        public static int PreviewHeight = 550;
        /// <summary>
        /// the width, in pixels, of the Preview window
        /// </summary>
        public static int PreviewWidth = 450;
        /// <summary>
        /// The Gif to use when loading previews
        /// </summary>
        public static LoadingGifs GIF = LoadingGifs.Standard;
        /// <summary>
        /// The Uninstall mode to use when uninstalling or installing with the clean install option
        /// </summary>
        public static UninstallModes UninstallMode = UninstallModes.Default;
        /// <summary>
        /// The selection view to use
        /// </summary>
        public static SelectionView ModSelectionView = SelectionView.Default;
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
        #endregion
        /// <summary>
        /// Initializes the Settings (should only be done on application start) and determinds which version of Settings loader method to use
        /// </summary>
        /// <returns></returns>
        public static bool LoadSettings()
        {
            Logging.WriteToLog("Started loading of ModpackSettings");
            if(!File.Exists(Settings.ModpackSettingsFileName))
            {
                Logging.WriteToLog("Modpack settings not found, default assignmetns used");
                Settings.FirstLoad = true;
                Translations.SetLanguageOnFirstLoad();
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(Settings.ModpackSettingsFileName);
                }
                catch
                {
                    Logging.WriteToLog(string.Format("Failed to load {0}, using defaults",Settings.ModpackSettingsFileName),Logfiles.Application,LogLevel.Error);
                    Translations.SetLanguageOnFirstLoad();
                    return false;
                }
                //using child of child rather than xpath gets around the fact that the root element name has changed
                XmlNodeList settings = doc.ChildNodes[0].ChildNodes;
                //https://docs.microsoft.com/en-us/dotnet/api/system.reflection.fieldinfo.getvalue?view=netframework-4.7.2#System_Reflection_FieldInfo_GetValue_System_Object_

                FieldInfo[] fields = typeof(ModpackSettings).GetFields();
                for(int i = 0; i < settings.Count; i++)
                {
                    XmlNode setting = settings[i];
                    switch(setting.Name)
                    {
                        //put legacy names here to direct name change
                        //TODO: super extraction and instant extraction
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
                            SaveLastConfig = bool.Parse(setting.InnerText);
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
                        case "loadingGif":
                            GIF = (LoadingGifs)int.Parse(setting.InnerText);
                            break;
                        case "language":
                            Language = (Languages)int.Parse(setting.InnerText);
                            break;
                        case "SelectionView":
                            ModSelectionView = (SelectionView)int.Parse(setting.InnerText);
                            if (ModSelectionView == SelectionView.Default)
                                ModSelectionView = SelectionView.DefaultV2;
                            break;
                        case "BetaApplication":
                            ApplicationDistroVersion = ApplicationVersions.Beta;
                            break;
                        case "BetaDatabase":
                            DatabaseDistroVersion = DatabaseVersions.Beta;
                            break;
                        //or when a setting name is changed (should never happen)
                        default:
                            //https://stackoverflow.com/questions/4651285/checking-if-a-list-of-objects-contains-a-property-with-a-specific-value
                            //check if field/setting with name will match with the xml name (should always be one in theory)
                            FieldInfo[] matches = fields.Where(f => f.Name.Equals(settings[i].Name)).ToArray();
                            Logging.WriteToLog("" + matches.Count() + " matches for xml setting name " + settings[i].Name,Logfiles.Application,LogLevel.Debug);
                            if (matches.Count() == 1)
                            {
                                FieldInfo settingField = matches[0];
                                //get the type of that field so we know how to parse for each value type
                                //string type = settingField.GetValue(typeof(ModpackSettings)).GetType().ToString();
                                //settingFIeld.setvalue(typeof(ModpackSettings),parse setting innertext)
                                Type type2 = settingField.GetValue(typeof(ModpackSettings)).GetType();
                                //https://stackoverflow.com/questions/5482844/how-to-compare-types
                                if (type2 == typeof(bool))
                                {
                                    settingField.SetValue(typeof(ModpackSettings), bool.Parse(setting.InnerText));
                                }
                                else if (type2 == typeof(int))
                                {
                                    settingField.SetValue(typeof(ModpackSettings), int.Parse(setting.InnerText));
                                }
                                else if ((type2 == typeof(decimal)) || (type2 == typeof(float)) || (type2 == typeof(double)))
                                {
                                    settingField.SetValue(typeof(ModpackSettings), float.Parse(setting.InnerText));
                                }
                                else if (type2 == typeof(LoadingGifs))
                                {
                                    //https://docs.microsoft.com/en-us/dotnet/api/system.enum.parse?view=netframework-4.7.2
                                    settingField.SetValue(typeof(ModpackSettings), Enum.Parse(typeof(LoadingGifs), setting.InnerText));
                                }
                                else if (type2 == typeof(UninstallModes))
                                {
                                    settingField.SetValue(typeof(ModpackSettings), Enum.Parse(typeof(UninstallModes), setting.InnerText));
                                }
                                else if (type2 == typeof(SelectionView))
                                {
                                    settingField.SetValue(typeof(ModpackSettings), Enum.Parse(typeof(SelectionView), setting.InnerText));
                                }
                                else if (type2 == typeof(Languages))
                                {
                                    settingField.SetValue(typeof(ModpackSettings), Enum.Parse(typeof(Languages), setting.InnerText));
                                }
                                else if (type2 == typeof(DatabaseVersions))
                                {
                                    settingField.SetValue(typeof(ModpackSettings), Enum.Parse(typeof(DatabaseVersions), setting.InnerText));
                                }
                                else if (type2 == typeof(ApplicationVersions))
                                {
                                    settingField.SetValue(typeof(ModpackSettings), Enum.Parse(typeof(ApplicationVersions), setting.InnerText));
                                }
                                else
                                {
                                    Logging.WriteToLog("unknown type: " + type2, Logfiles.Application, LogLevel.Warning);
                                }
                            }
                            break;
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// Saves all fields (settings) to an xml file
        /// </summary>
        /// <returns></returns>
        public static bool SaveSettings()
        {
            XmlDocument doc = new XmlDocument();
            //create element called ModpackSettings
            XmlElement settingsHolder = doc.CreateElement(typeof(ModpackSettings).Name);
            doc.AppendChild(settingsHolder);
            if (File.Exists(Settings.ModpackSettingsFileName))
                File.Delete(Settings.ModpackSettingsFileName);
            //if it can delete, then it can save later
            FieldInfo[] fields = typeof(ModpackSettings).GetFields();
            foreach (FieldInfo field in fields)
            {
                XmlElement element = doc.CreateElement(field.Name);
                element.InnerText = field.GetValue(typeof(ModpackSettings)).ToString();
                settingsHolder.AppendChild(element);
            }
            doc.Save(Settings.ModpackSettingsFileName);
            return true;
        }
    }
}
