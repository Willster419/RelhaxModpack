using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;

namespace RelhaxModpack
{
    //all of the settings for the modpack. kept in a static class
    //so all the forms have access to a single version of the settings
    public static class Settings
    {
        ////general settings
        //toggle if the program should notify the user if the database version is the same as the last installed version
        public static bool NotifyIfSameDatabase { get; set; }
        //toggle if the program will backup the current mod installation
        public static bool BackupModFolder { get; set; }
        //toggle if the program will clean the mods and res_mods folders before installation
        public static bool CleanInstallation { get; set; }
        public static bool ForceManuel { get; set; }
        public static bool ExpandAllLegacy { get; set; }
        public static bool ComicSans { get; set; }
        public static bool FirstLoad { get; set; }
        public static bool SaveLastConfig { get; set; }
        public static bool SaveUserData { get; set; }
        public static bool DarkUI { get; set; }
        public static bool DisableBorders { get; set; }
        //toggle if the installation complete window will be shown
        public static bool ShowInstallCompleteWindow { get; set; }
        //toggle if the program will delete the WoT appdata cache
        public static bool ClearCache { get; set; }
        public static bool DeleteLogs { get; set; }
        public static bool DisableColorChange { get; set; }
        //toggle if the program will create desktop shortcuts
        public static bool CreateShortcuts { get; set; }
        //toggle instant extraction
        public static bool InstantExtraction { get; set; }
        public static int ModSelectionHeight { get; set; }
        public static int ModSelectionWidth { get; set; }
        public static int LoadingGif { get; set; }
        public static string FontName { get; set; }
        public static float ScaleSize { get; set; }
        public static string SettingsXmlFile = Path.Combine(Application.StartupPath, "RelHaxSettings.xml");
        public static string ManagerInfoDatFile = Path.Combine(Application.StartupPath, "RelHaxTemp", "managerInfo.dat");
        public static string ModInfoDatFile = Path.Combine(Application.StartupPath, "RelHaxTemp", "modInfo.dat");
        //informations for macroList
        public static string TanksLocation = "";
        public static string TanksVersion = "";
        public static string TanksOnlineFolderVersion = "";
        public static string RelhaxTempFolder = Path.Combine(Application.StartupPath, "RelHaxTemp");
        //needed to create to first line to installedRelhaxFiles.log
        public static string DatabaseVersion = "";
        //
        public static string ConfigFileVersion = "2.0";     // for later imports of this files, we need a better identification
        public enum LoadingGifs { Standard = 0, ThirdGuards = 1 };
        public static LoadingGifs GIF;
        //font settings
        public const float FontSize100 = 8.25F;//1.0 font scaling
        public const float FontSize125 = 10.25F;//1.25 font scaling
        public const float FontSize175 = 14.25F;//1.75 font scaling
        public const float FontSize225 = 18.5F;//2.25 font scaling
        public const float FontSize275 = 22.5F;//2.75 font scaling
        public const float Scale100 = 1.0f;//1.0 font scaling
        public const float Scale125 = 1.25f;//1.25 font scaing
        public const float Scale175 = 1.75f;//1.75 font scaling
        public const float Scale225 = 2.25f;//2.25 font scaling
        public const float Scale275 = 2.75f;//2.75 font scaling
        public const string DefaultFontType = "Microsoft Sance Serif";
        public const string ComicSansFontType = "Comic Sans MS";
        private static int TempLoadedLanguage = -1;
        public static bool ModSelectionFullscreen = false;
        public static int PreviewX = 0;
        public static int PreviewY = 0;
        public static string CustomModInfoPath = "";
        //enumeration for the type of mod selection list view
        public enum SelectionView { Default = 0, Legacy = 1 };
        public static SelectionView SView = SelectionView.Default;
        public static int TempLoadedView = 0;
        public enum FontSize
        {
            Font100 = 0,
            Font125 = 1,
            Font175 = 2,
            DPI100 = 3,
            DPI125 = 4,
            DPI175 = 5,
            Font225 = 6,
            Font275 = 7,
            DPI225 = 8,
            DPI275 = 9,
            DPIAUTO = 10
        };
        public static FontSize FontSizeforum = FontSize.Font100;
        public static int TempFontSizeForum = 0;//default to font scale, regular
        public static AutoScaleMode AppScalingMode = AutoScaleMode.Font;
        public static Font AppFont = new System.Drawing.Font(DefaultFontType, FontSize100);
        //loads settings from xml file
        public static void LoadSettings()
        {
            //Settings declared here are set for what their default values should be, then later modified in the settings xml file
            //i.e. when new features are added
            Settings.InstantExtraction = false;
            Settings.FirstLoad = false;
            Settings.CreateShortcuts = true;
            Logging.Manager("Loading application settings");
            if (!File.Exists(SettingsXmlFile))
            {
                Logging.Manager("WARNING:Settings xml not found, loading defaults");
                //could also use this to determine if first load or not
                //default is to turn all features off
                Settings.ComicSans = false;
                Settings.BackupModFolder = false;
                Settings.CleanInstallation = true;
                Settings.LoadingGif = (int)LoadingGifs.Standard;
                Settings.ForceManuel = false;
                Settings.GIF = Settings.LoadingGifs.Standard;
                Settings.FirstLoad = true;
                Settings.SaveLastConfig = false;
                Settings.SaveUserData = false;
                Settings.ClearCache = false;
                Settings.DisableBorders = false;
                Settings.NotifyIfSameDatabase = false;
                Settings.CreateShortcuts = false;
                Settings.InstantExtraction = false;
                Logging.Manager("Language: " + CultureInfo.CurrentCulture.DisplayName);
                string lang = CultureInfo.InstalledUICulture.Name.Split('-')[0];
                if (lang.ToLower().Equals("de"))
                {
                    Settings.TempLoadedLanguage = 1;
                }
                else if (lang.ToLower().Equals("pl"))
                {
                    Settings.TempLoadedLanguage = 2;
                }
                else if (lang.ToLower().Equals("fr"))
                {
                    Settings.TempLoadedLanguage = 3;
                }
                else
                {
                    Settings.TempLoadedLanguage = 0;
                }
                Settings.ModSelectionHeight = 480;
                Settings.ModSelectionWidth = 800;
                Settings.FontSizeforum = Settings.FontSize.Font100;
                Settings.ExpandAllLegacy = false;
                Settings.ModSelectionFullscreen = false;
                Settings.DisableColorChange = false;
                Settings.DeleteLogs = false;
                Settings.PreviewX = 0;
                Settings.PreviewY = 0;
                Settings.CustomModInfoPath = "";
                Settings.TempFontSizeForum = 0;
                Settings.FontSizeforum = FontSize.Font100;
                Settings.SView = SelectionView.Default;
                Settings.ShowInstallCompleteWindow = false;
                Settings.applyInternalSettings();
            }
            else
            {
                Logging.Manager("Loading xml file");
                XmlDocument doc = new XmlDocument();
                doc.Load(SettingsXmlFile);
                XmlNodeList settingsList = doc.ChildNodes[0].ChildNodes;
                foreach (XmlNode n in settingsList)
                {
                    switch (n.Name)
                    {
                        case "comicSans":
                            Settings.ComicSans = bool.Parse(n.InnerText);
                            break;
                        case "backupModFolder":
                            Settings.BackupModFolder = bool.Parse(n.InnerText);
                            break;
                        case "cleanInstallation":
                            Settings.CleanInstallation = bool.Parse(n.InnerText);
                            break;
                        case "loadingGif":
                            Settings.LoadingGif = int.Parse(n.InnerText);
                            break;
                        case "forceManuel":
                            Settings.ForceManuel = bool.Parse(n.InnerText);
                            break;
                        case "modSelectionHeight":
                            Settings.ModSelectionHeight = int.Parse(n.InnerText);
                            break;
                        case "modSelectionWidth":
                            Settings.ModSelectionWidth = int.Parse(n.InnerText);
                            break;
                        case "saveLastConfig":
                            Settings.SaveLastConfig = bool.Parse(n.InnerText);
                            break;
                        case "saveUserData":
                            Settings.SaveUserData = bool.Parse(n.InnerText);
                            break;
                        case "clearCache":
                            Settings.ClearCache = bool.Parse(n.InnerText);
                            break;
                        case "darkUI":
                            Settings.DarkUI = bool.Parse(n.InnerText);
                            break;
                        case "expandAllLegacy":
                            Settings.ExpandAllLegacy = bool.Parse(n.InnerText);
                            break;
                        case "disableBorders":
                            Settings.DisableBorders = bool.Parse(n.InnerText);
                            break;
                        case "disableColorChange":
                            Settings.DisableColorChange = bool.Parse(n.InnerText);
                            break;
                        case "deleteLogs":
                            Settings.DeleteLogs = bool.Parse(n.InnerText);
                            break;
                        case "language":
                            Settings.TempLoadedLanguage = int.Parse(n.InnerText);
                            break;
                        case "ModSelectionFullscreen":
                            Settings.ModSelectionFullscreen = bool.Parse(n.InnerText);
                            break;
                        case "NotifyIfSameDatabase":
                            Settings.NotifyIfSameDatabase = bool.Parse(n.InnerText);
                            break;
                        case "ShowInstallCompleteWindow":
                            Settings.ShowInstallCompleteWindow = bool.Parse(n.InnerText);
                            break;
                        case "previewX":
                            Settings.PreviewX = int.Parse(n.InnerText);
                            break;
                        case "previewY":
                            Settings.PreviewY = int.Parse(n.InnerText);
                            break;
                        case "customModInfoPath":
                            Settings.CustomModInfoPath = n.InnerText;
                            break; 
                        case "SelectionView":
                            Settings.TempLoadedView = int.Parse(n.InnerText);
                            break;
                        case "FontSizeForum":
                            Settings.TempFontSizeForum = int.Parse(n.InnerText);
                            break;
                        case "CreateShortcuts":
                            Settings.CreateShortcuts = bool.Parse(n.InnerText);
                            break;
                        case "InstantExtraction":
                            Settings.InstantExtraction = bool.Parse(n.InnerText);
                            break;
                    }
                }
            }
            Settings.applyInternalSettings();
            Logging.Manager("Settings loaded sucessfully");
        }
        //apply internal settings (font name, size, loading gif)
        //based on the boolean settings from above
        public static void applyInternalSettings()
        {
            switch (Settings.TempFontSizeForum)
            {
                default:
                    Settings.FontSizeforum = FontSize.Font100;
                    break;
                case 0:
                    Settings.FontSizeforum = FontSize.Font100;
                    break;
                case 1:
                    Settings.FontSizeforum = FontSize.Font125;
                    break;
                case 2:
                    Settings.FontSizeforum = FontSize.Font175;
                    break;
                case 3:
                    Settings.FontSizeforum = FontSize.DPI100;
                    break;
                case 4:
                    Settings.FontSizeforum = FontSize.DPI125;
                    break;
                case 5:
                    Settings.FontSizeforum = FontSize.DPI175;
                    break;
                case 6:
                    Settings.FontSizeforum = FontSize.Font225;
                    break;
                case 7:
                    Settings.FontSizeforum = FontSize.Font275;
                    break;
                case 8:
                    Settings.FontSizeforum = FontSize.DPI225;
                    break;
                case 9:
                    Settings.FontSizeforum = FontSize.DPI275;
                    break;
                case 10:
                    Settings.FontSizeforum = FontSize.DPIAUTO;
                    break;
            }
            if (Settings.ComicSans)
            {
                Settings.FontName = Settings.ComicSansFontType;
            }
            else
            {
                Settings.FontName = Settings.DefaultFontType;
            }
            switch (Settings.LoadingGif)
            {
                case 0:
                    Settings.GIF = Settings.LoadingGifs.Standard;
                    break;
                case 1:
                    Settings.GIF = Settings.LoadingGifs.ThirdGuards;
                    break;
            }
            switch (Settings.TempLoadedLanguage)
            {
                case 0:
                    //english
                    Translations.language = Translations.Languages.English;
                    break;
                case 1:
                    //german
                    Translations.language = Translations.Languages.German;
                    break;
                case 2:
                    //polish
                    Translations.language = Translations.Languages.Polish;
                    break;
                case 3:
                    //french
                    Translations.language = Translations.Languages.French;
                    break;
            }
            //apply the internal setting of the view selection
            switch (Settings.TempLoadedView)
            {
                case 0:
                    //default (relhax)
                    Settings.SView = SelectionView.Default;
                    break;
                case 1:
                    //legacy (OMC)
                    Settings.SView = SelectionView.Legacy;
                    break;
            }
            //apply scaling settings
            Settings.ApplyScalingProperties();
        }
        //saves settings to xml file
        public static void saveSettings()
        {
            Logging.Manager("Saving application settings");
            if (File.Exists(SettingsXmlFile)) File.Delete(SettingsXmlFile);
            XmlDocument doc = new XmlDocument();
            XmlElement settingsHolder = doc.CreateElement("settings");
            doc.AppendChild(settingsHolder);
            XmlElement xcomicSans = doc.CreateElement("comicSans");
            xcomicSans.InnerText = "" + ComicSans;
            settingsHolder.AppendChild(xcomicSans);
            XmlElement xfontSizeForum = doc.CreateElement("FontSizeForum");
            xfontSizeForum.InnerText = "" + (int)FontSizeforum;
            settingsHolder.AppendChild(xfontSizeForum);
            XmlElement xbackupModFolder = doc.CreateElement("backupModFolder");
            xbackupModFolder.InnerText = "" + BackupModFolder;
            settingsHolder.AppendChild(xbackupModFolder);
            XmlElement xcleanInstallation = doc.CreateElement("cleanInstallation");
            xcleanInstallation.InnerText = "" + CleanInstallation;
            settingsHolder.AppendChild(xcleanInstallation);
            XmlElement xsaveLastConfig = doc.CreateElement("saveLastConfig");
            xsaveLastConfig.InnerText = "" + Settings.SaveLastConfig;
            settingsHolder.AppendChild(xsaveLastConfig);
            XmlElement xsaveUserData = doc.CreateElement("saveUserData");
            xsaveUserData.InnerText = "" + Settings.SaveUserData;
            settingsHolder.AppendChild(xsaveUserData);
            XmlElement xclearCache = doc.CreateElement("clearCache");
            xclearCache.InnerText = "" + Settings.ClearCache;
            settingsHolder.AppendChild(xclearCache);
            XmlElement xdarkUI = doc.CreateElement("darkUI");
            xdarkUI.InnerText = "" + Settings.DarkUI;
            settingsHolder.AppendChild(xdarkUI);
            XmlElement xexpandAllLegacy = doc.CreateElement("expandAllLegacy");
            xexpandAllLegacy.InnerText = "" + Settings.ExpandAllLegacy;
            settingsHolder.AppendChild(xexpandAllLegacy);
            XmlElement xdisableBorders = doc.CreateElement("disableBorders");
            xdisableBorders.InnerText = "" + Settings.DisableBorders;
            settingsHolder.AppendChild(xdisableBorders);
            XmlElement xdisableColorChange = doc.CreateElement("disableColorChange");
            xdisableColorChange.InnerText = "" + Settings.DisableColorChange;
            settingsHolder.AppendChild(xdisableColorChange);
            XmlElement xdeleteLogs = doc.CreateElement("deleteLogs");
            xdeleteLogs.InnerText = "" + Settings.DeleteLogs;
            settingsHolder.AppendChild(xdeleteLogs);
            XmlElement xNotifyIfSameDatabase = doc.CreateElement("NotifyIfSameDatabase");
            xNotifyIfSameDatabase.InnerText = "" + Settings.NotifyIfSameDatabase;
            settingsHolder.AppendChild(xNotifyIfSameDatabase);
            XmlElement xShowInstallCompleteWindow = doc.CreateElement("ShowInstallCompleteWindow");
            xShowInstallCompleteWindow.InnerText = "" + Settings.ShowInstallCompleteWindow;
            settingsHolder.AppendChild(xShowInstallCompleteWindow);
            XmlElement xlanguage = doc.CreateElement("language");
            xlanguage.InnerText = "" + (int)Translations.language;
            settingsHolder.AppendChild(xlanguage);
            XmlElement xSelectionView = doc.CreateElement("SelectionView");
            xSelectionView.InnerText = "" + (int)Settings.SView;
            settingsHolder.AppendChild(xSelectionView);
            switch (Settings.GIF)
            {
                case (Settings.LoadingGifs.Standard):
                    {
                        Settings.LoadingGif = 0;
                        break;
                    }
                case (Settings.LoadingGifs.ThirdGuards):
                    {
                        Settings.LoadingGif = 1;
                        break;
                    }
            }
            XmlElement xloadingGif = doc.CreateElement("loadingGif");
            xloadingGif.InnerText = "" + LoadingGif;
            settingsHolder.AppendChild(xloadingGif);
            XmlElement xforceManuel = doc.CreateElement("forceManuel");
            xforceManuel.InnerText = "" + ForceManuel;
            settingsHolder.AppendChild(xforceManuel);
            XmlElement xmodSelectionHeight = doc.CreateElement("modSelectionHeight");
            xmodSelectionHeight.InnerText = "" + ModSelectionHeight;
            settingsHolder.AppendChild(xmodSelectionHeight);
            XmlElement xmodSelectionWidth = doc.CreateElement("modSelectionWidth");
            xmodSelectionWidth.InnerText = "" + ModSelectionWidth;
            settingsHolder.AppendChild(xmodSelectionWidth);
            XmlElement xModSelectionFullscreen = doc.CreateElement("ModSelectionFullscreen");
            xModSelectionFullscreen.InnerText = "" + ModSelectionFullscreen;
            settingsHolder.AppendChild(xModSelectionFullscreen);
            XmlElement xCreateShortcuts = doc.CreateElement("CreateShortcuts");
            xCreateShortcuts.InnerText = "" + CreateShortcuts;
            settingsHolder.AppendChild(xCreateShortcuts);
            XmlElement xInstantExtraction = doc.CreateElement("InstantExtraction");
            xInstantExtraction.InnerText = "" + InstantExtraction;
            settingsHolder.AppendChild(xInstantExtraction);
            XmlElement xpreviewX = doc.CreateElement("previewX");
            if (PreviewX < 0) { PreviewX = 0; };
            xpreviewX.InnerText = "" + PreviewX;
            settingsHolder.AppendChild(xpreviewX);
            XmlElement xpreviewY = doc.CreateElement("previewY");
            if (PreviewY < 0) { PreviewY = 0; };
            xpreviewY.InnerText = "" + PreviewY;
            settingsHolder.AppendChild(xpreviewY);
            XmlElement customModInfoPath = doc.CreateElement("customModInfoPath");
            if (Settings.CustomModInfoPath != "") { customModInfoPath.InnerText = Settings.CustomModInfoPath; }
            settingsHolder.AppendChild(customModInfoPath); 

            doc.Save(SettingsXmlFile);
            Logging.Manager("Settings saved sucessfully");
        }
        //returns the loading image for the picture viewer, based on
        //which loading image the user specified
        public static Image getLoadingImage()
        {
            switch (Settings.GIF)
            {
                case (LoadingGifs.Standard):
                    {
                        return RelhaxModpack.Properties.Resources.loading;
                    }
                case (LoadingGifs.ThirdGuards):
                    {
                        return RelhaxModpack.Properties.Resources.loading_3rdguards;
                    }
            }
            return null;
        }
        public static void ApplyScalingProperties()
        {
            if (Settings.ComicSans)
            {
                Settings.FontName = Settings.ComicSansFontType;
            }
            else
            {
                Settings.FontName = Settings.DefaultFontType;
            }
            switch (FontSizeforum)
            {
                default:
                    //set the autoscale mode
                    AppScalingMode = AutoScaleMode.Font;
                    //set the scale amount
                    ScaleSize = Scale100;
                    //set the font
                    AppFont = new Font(DefaultFontType, FontSize100);
                    break;
                case FontSize.Font100:
                    ScaleSize = Scale100;
                    AppScalingMode = AutoScaleMode.Font;
                    AppFont = new Font(FontName, FontSize100);
                    break;
                case FontSize.Font125:
                    ScaleSize = Scale125;
                    AppScalingMode = AutoScaleMode.Font;
                    AppFont = new Font(FontName, FontSize125);
                    break;
                case FontSize.Font175:
                    ScaleSize = Scale175;
                    AppScalingMode = AutoScaleMode.Font;
                    AppFont = new Font(FontName, FontSize175);
                    break;
                case FontSize.Font225:
                    ScaleSize = Scale225;
                    AppScalingMode = AutoScaleMode.Font;
                    AppFont = new Font(FontName, FontSize225);
                    break;
                case FontSize.Font275:
                    ScaleSize = Scale275;
                    AppScalingMode = AutoScaleMode.Font;
                    AppFont = new Font(FontName, FontSize275);
                    break;
                case FontSize.DPI100:
                    ScaleSize = Scale100;
                    AppScalingMode = AutoScaleMode.Dpi;
                    AppFont = new Font(FontName, FontSize100);
                    break;
                case FontSize.DPI125:
                    ScaleSize = Scale125;
                    AppScalingMode = AutoScaleMode.Dpi;
                    AppFont = new Font(FontName, FontSize125);
                    break;
                case FontSize.DPI175:
                    ScaleSize = Scale175;
                    AppScalingMode = AutoScaleMode.Dpi;
                    AppFont = new Font(FontName, FontSize175);
                    break;
                case FontSize.DPI225:
                    ScaleSize = Scale225;
                    AppScalingMode = AutoScaleMode.Dpi;
                    AppFont = new Font(FontName, FontSize225);
                    break;
                case FontSize.DPI275:
                    ScaleSize = Scale275;
                    AppScalingMode = AutoScaleMode.Dpi;
                    AppFont = new Font(FontName, FontSize275);
                    break;
                case FontSize.DPIAUTO:
                    ScaleSize = Utils.GetScalingFactor();
                    AppScalingMode = AutoScaleMode.Dpi;
                    float nweFontSize = FontSize100 * ScaleSize;
                    float roundedanswer = (float)Math.Round(nweFontSize * 4, MidpointRounding.ToEven) / 4;
                    AppFont = new Font(FontName, roundedanswer);
                    break;
            }
        }
        //sets a form to have a dark UI
        public static void setUIColor(System.Windows.Forms.Form window)
        {
            Color backColor;
            Color textColor;
            if (Settings.DarkUI)
            {
                backColor = SystemColors.ControlDarkDark;
                textColor = Color.White;
            }
            else
            {
                backColor = SystemColors.Control;
                textColor = SystemColors.ControlText;
            }
            window.BackColor = backColor;
            foreach (Control c in window.Controls)
            {
                if (c is CheckBox || c is RadioButton || c is Label)
                {
                    c.ForeColor = textColor;
                }
                else if (c is Panel || c is GroupBox)
                {
                    c.BackColor = backColor;
                    c.ForeColor = textColor;
                    foreach (Control subC in c.Controls)
                    {
                        if (subC is CheckBox || subC is RadioButton || subC is Label)
                        {
                            subC.ForeColor = textColor;
                        }
                        else if (subC is Panel || subC is GroupBox)
                        {
                            subC.BackColor = backColor;
                            subC.ForeColor = textColor;
                            foreach (Control subC2 in subC.Controls)
                            {
                                if (subC2 is CheckBox || subC2 is RadioButton || subC2 is Label)
                                {
                                    subC2.ForeColor = textColor;
                                }
                                else if (subC2 is Panel || subC2 is GroupBox)
                                {
                                    subC2.BackColor = backColor;
                                    subC2.ForeColor = textColor;
                                    foreach (Control subC3 in subC2.Controls)
                                    {
                                        if (subC3 is CheckBox || subC3 is RadioButton || subC3 is Label)
                                        {
                                            subC3.ForeColor = textColor;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (c is PictureBox)
                {
                    c.BackColor = backColor;
                }
                else if (c is TabControl)
                {
                    c.BackColor = backColor;
                    foreach (TabPage t in c.Controls)
                    {
                        t.BackColor = backColor;
                        foreach (Control subC in t.Controls)
                        {
                            foreach (Control subC2 in subC.Controls)
                            {
                                if (subC2 is CheckBox)
                                {
                                    subC2.ForeColor = textColor;
                                }
                            }
                        }
                    }
                }
            }
        }
        public static Color getTextColor()
        {
            if (Settings.DarkUI)
                return Color.White;

            else
                return SystemColors.ControlText;
        }
        public static Color getBackColor()
        {
            if (Settings.DarkUI)
                return SystemColors.ControlDark;

            else
                return SystemColors.Control;
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("shell32.dll")]
        public static extern UInt32 SHAppBarMessage(UInt32 dwMessage, ref APPBARDATA pData);

        public enum AppBarMessages
        {
            New = 0x00,
            Remove = 0x01,
            QueryPos = 0x02,
            SetPos = 0x03,
            GetState = 0x04,
            GetTaskBarPos = 0x05,
            Activate = 0x06,
            GetAutoHideBar = 0x07,
            SetAutoHideBar = 0x08,
            WindowPosChanged = 0x09,
            SetState = 0x0a
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            public UInt32 cbSize;
            public IntPtr hWnd;
            public UInt32 uCallbackMessage;
            public UInt32 uEdge;
            public Rectangle rc;
            public Int32 lParam;
        }

        public enum AppBarStates
        {
            AutoHide = 0x01,
            AlwaysOnTop = 0x02
        }

        /// <summary>
        /// Set the Taskbar State option
        /// </summary>
        /// <param name="option">AppBarState to activate</param>
        public static void SetTaskbarState(AppBarStates option)
        {
            APPBARDATA msgData = new APPBARDATA();
            msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
            msgData.hWnd = FindWindow("System_TrayWnd", null);
            msgData.lParam = (Int32)(option);
            SHAppBarMessage((UInt32)AppBarMessages.SetState, ref msgData);
        }

        /// <summary>
        /// Gets the current Taskbar state
        /// </summary>
        /// <returns>current Taskbar state</returns>
        public static AppBarStates GetTaskbarState()
        {
            APPBARDATA msgData = new APPBARDATA();
            msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
            msgData.hWnd = FindWindow("System_TrayWnd", null);
            return (AppBarStates)SHAppBarMessage((UInt32)AppBarMessages.GetState, ref msgData);
        }
    }
}
