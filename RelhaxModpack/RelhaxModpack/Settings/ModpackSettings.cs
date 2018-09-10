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
    public enum LoadingGifs
    {
        Standard = 0,
        ThirdGuards = 1
    };
    //enumeration for the type of uninstall mode
    public enum UninstallModes
    {
        Default = 0,
        Quick = 1
    }
    /// <summary>
    /// Provides access to all settings used in the modpack.
    /// </summary>
    public class ModpackSettings
    {
        public ModpackSettings(string filePath) : base()
        {
            SettingsFilePath = filePath;
        }
        /// <summary>
        /// The absolute path of the application settings file
        /// </summary>
        public string SettingsFilePath { get; private set; }
        /// <summary>
        /// Get the name of the settings file
        /// </summary>
        public string SettingsFileName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(SettingsFilePath))
                    return Path.GetFileName(SettingsFilePath);
                else
                    return string.Empty;
            }
        }
        //list of properties to exclude from the properties enumeration
        private string[] PropertiesToExclude = new string[]
        {

        };
        #region saveable modpack settings
        /// <summary>
        /// toggle if the program should notify the user if the database version is the same as the last installed version
        /// </summary>
        public bool NotifyIfSameDatabase = false;
        /// <summary>
        /// toggle if the program will backup the current mod installation
        /// </summary>
        public bool BackupModFolder = false;
        /// <summary>
        /// toggle if the program will clean the mods and res_mods folders before installation
        /// </summary>
        public bool CleanInstallation = true;
        /// <summary>
        /// toggle if the program should force the user to manually point to the WoT location
        /// </summary>
        public bool ForceManuel = false;
        /// <summary>
        /// toggle if the application should automatically save the last selected config to also be automatically loaded upon selection load
        /// </summary>
        public bool SaveLastConfig = false;
        /// <summary>
        /// toggle if the application should save user cache save data like session stats, or auto equip configs
        /// </summary>
        public bool SaveUserData = false;
        //toggle for each view if the borders around the child selection options should show
        public bool EnableBordersDefaultView = false;
        public bool EnableBordersLegacyView = false;
        public bool EnableBordersDefaultV2View = false;
        //toggle for each view if the color change should occur when a child selection happends
        public bool EnableColorChangeDefaultView = false;
        public bool EnableColorChangeLegacyView = false;
        public bool EnableColorChangeDefaultV2View = false;
        //toggle if the installation complete window will be shown
        public bool ShowInstallCompleteWindow = false;
        //toggle if the program will delete the WoT appdata cache
        public bool ClearCache = false;
        public bool DeleteLogs = false;
        //toggle if the program will create desktop shortcuts
        public bool CreateShortcuts = false;
        //toggle instant extraction
        public bool InstantExtraction = false;
        //toggle super extraction
        public bool SuperExtraction = false;
        //turn on export mode
        public bool ExportMode = false;
        /// <summary>
        /// the height, in pixels, of the ModSelectionView window
        /// </summary>
        public int ModSelectionHeight = 480;
        /// <summary>
        /// the width, in pixels, of the ModSelectionView window
        /// </summary>
        public int ModSelectionWidth = 800;
        /// <summary>
        /// toggle for if the ModSelectionView window should be shown in fullscreen mode
        /// </summary>
        public bool ModSelectionFullscreen = false;
        /// <summary>
        /// the x-coordinate location, in pixels, of the Preview window
        /// </summary>
        public int PreviewX = 0;
        /// <summary>
        /// the y-coordinate location, in pixels, of the Preview window
        /// </summary>
        public int PreviewY = 0;
        /// <summary>
        /// toggle for if the Preview window should be shown in fullscreen mode
        /// </summary>
        public bool PreviewFullscreen = false;
        /// <summary>
        /// the height, in pixels, of the Preview window
        /// </summary>
        public int PreviewHeight = 550;
        /// <summary>
        /// the width, in pixels, of the Preview window
        /// </summary>
        public int PreviewWidth = 450;
        public LoadingGifs GIF = LoadingGifs.Standard;
        public UninstallModes UninstallMode = UninstallModes.Default;
        public SelectionView SView = SelectionView.Default;
        #endregion
        /// <summary>
        /// Initializes the Settings (should only be done on application start) and determinds which version of Settings loader method to use
        /// </summary>
        /// <returns></returns>
        public bool LoadSettings()
        {
            Logging.WriteToLog("Started loading of ModpackSettings");
            if(!File.Exists(SettingsFilePath))
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
                    doc.Load(SettingsFilePath);
                }
                catch
                {
                    Logging.WriteToLog(string.Format("Failed to load {0}, using defaults",SettingsFilePath),Logfiles.Application,LogLevel.Error);
                    Translations.SetLanguageOnFirstLoad();
                    return false;
                }
                XmlNodeList settings = doc.ChildNodes[0].ChildNodes;
                FieldInfo[] fields = typeof(ModpackSettings).GetFields();
                for(int i = 0; i < settings.Count; i++)
                {
                    XmlNode setting = settings[i];
                    switch(setting.Name)
                    {
                        //put legacy names here to direct name change
                        default:
                            //check if field/setting with name will match with the xml name (should always be one in theory)
                            FieldInfo[] matches = fields.Where(f => f.Name.Equals(settings[i].Name)).ToArray();
                            Logging.WriteToLog("" + matches.Count() + " matches for xml setting name " + settings[i].Name,Logfiles.Application,LogLevel.Debug);
                            if (matches.Count() == 1)
                            {
                                FieldInfo settingField = matches[0];
                                //get the type of that field so we know how to parse
                                string type = settingField.GetValue(this).GetType().ToString();
                                Type type2 = settingField.GetValue(this).GetType();
                                switch(Type.GetTypeCode(type2))
                                {
                                    case TypeCode.Boolean:
                                        settingField.SetValue(this, bool.Parse(setting.InnerText));
                                        break;
                                    default:
                                        Logging.WriteToLog("unknown type: " + type2, Logfiles.Application, LogLevel.Debug);
                                        break;
                                }
                                //setting.SetValue(this,typeof())
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
        public bool SaveSettings()
        {
            XmlDocument doc = new XmlDocument();
            //create element called ModpackSettings
            XmlElement settingsHolder = doc.CreateElement(this.GetType().Name);
            doc.AppendChild(settingsHolder);
            if (File.Exists(SettingsFilePath))
                File.Delete(SettingsFilePath);
            //if it can delete, then it can save later
            FieldInfo[] fields = typeof(ModpackSettings).GetFields();
            foreach (FieldInfo field in fields)
            {
                XmlElement element = doc.CreateElement(field.Name);
                element.InnerText = field.GetValue(this).ToString();
                settingsHolder.AppendChild(element);
            }
            doc.Save(SettingsFilePath);
            return true;
        }
    }
}
