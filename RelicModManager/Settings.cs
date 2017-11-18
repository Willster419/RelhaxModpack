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
        //toggle super extraction
        public static bool SuperExtraction { get; set; }
        public static int ModSelectionHeight { get; set; }
        public static int ModSelectionWidth { get; set; }
        public static string FontName { get; set; }
        public static float ScaleSize { get; set; }
        //file locations
        public static string SettingsXmlFile = Path.Combine(Application.StartupPath, "RelHaxxml");
        public static string RelhaxTempFolder = Path.Combine(Application.StartupPath, "RelHaxTemp");
        public static string RelhaxDownloadsFolder = Path.Combine(Application.StartupPath, "RelHaxDownloads");
        public static string MD5HashDatabaseXmlFile = Path.Combine(RelhaxDownloadsFolder, "MD5HashDatabase.xml");
        public static string OnlineDatabaseXmlFile = Path.Combine(RelhaxDownloadsFolder, "onlineDatabase.xml");
        public static string ManagerInfoDatFile = Path.Combine(RelhaxTempFolder, "managerInfo.dat");
        public static string ModInfoDatFile = Path.Combine(RelhaxTempFolder, "modInfo.dat");
        //informations for macroList
        public static string TanksLocation = "";
        public static string TanksVersion = "";
        public static string TanksOnlineFolderVersion = "";
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
        public const string DefaultFontType = "Microsoft Sans Serif";
        public const string ComicSansFontType = "Comic Sans MS";
        public static bool ModSelectionFullscreen = false;
        public static int PreviewX = 0;
        public static int PreviewY = 0;
        public static string CustomModInfoPath = "";
        //enumeration for the type of uninstall mode
        public enum UninstallModes
        {
            Smart = 0,
            Clean = 1
        }
        public static UninstallModes UninstallMode = UninstallModes.Smart;
        //enumeration for the type of mod selection list view
        public enum SelectionView { Default = 0, Legacy = 1 };
        public static SelectionView SView = SelectionView.Default;
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
        public static AutoScaleMode AppScalingMode = AutoScaleMode.Font;
        public static Font AppFont = new Font(DefaultFontType, FontSize100);
        //loads settings from xml file
        public static void LoadSettings()
        {
            //Settings declared here are set for what their default values should be, then later modified in the settings xml file
            //i.e. when new features are added
            InstantExtraction = false;
            FirstLoad = false;
            CreateShortcuts = true;
            CleanInstallation = true;
            SuperExtraction = false;
            Logging.Manager("Loading application settings");
            if (!File.Exists(SettingsXmlFile))
            {
                Logging.Manager("WARNING:Settings xml not found, loading defaults");
                //could also use this to determine if first load or not
                //default is to turn all features off
                ComicSans = false;
                BackupModFolder = false;
                CleanInstallation = true;
                ForceManuel = false;
                GIF = LoadingGifs.Standard;
                FirstLoad = true;
                SaveLastConfig = false;
                SaveUserData = false;
                ClearCache = false;
                DisableBorders = false;
                NotifyIfSameDatabase = false;
                CreateShortcuts = false;
                InstantExtraction = false;
                Logging.Manager("Language: " + CultureInfo.CurrentCulture.DisplayName);
                string lang = CultureInfo.InstalledUICulture.Name.Split('-')[0].ToLower();
                switch (lang)
                {
                    case "de":
                        Translations.language = Translations.Languages.German;
                        break;
                    case "pl":
                        Translations.language = Translations.Languages.Polish;
                        break;
                    case "fr":
                        Translations.language = Translations.Languages.French;
                        break;
                    default:
                        Translations.language = Translations.Languages.English;
                        break;
                }
                ModSelectionHeight = 480;
                ModSelectionWidth = 800;
                FontSizeforum = FontSize.Font100;
                UninstallMode = UninstallModes.Smart;
                ExpandAllLegacy = false;
                ModSelectionFullscreen = false;
                DisableColorChange = false;
                DeleteLogs = false;
                PreviewX = 0;
                PreviewY = 0;
                CustomModInfoPath = "";
                FontSizeforum = FontSize.Font100;
                SView = SelectionView.Default;
                ShowInstallCompleteWindow = false;
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
                            ComicSans = bool.Parse(n.InnerText);
                            break;
                        case "backupModFolder":
                            BackupModFolder = bool.Parse(n.InnerText);
                            break;
                        case "cleanInstallation":
                            CleanInstallation = bool.Parse(n.InnerText);
                            break;
                        case "loadingGif":
                            GIF = (LoadingGifs)int.Parse(n.InnerText);
                            break;
                        case "forceManuel":
                            ForceManuel = bool.Parse(n.InnerText);
                            break;
                        case "modSelectionHeight":
                            ModSelectionHeight = int.Parse(n.InnerText);
                            break;
                        case "modSelectionWidth":
                            ModSelectionWidth = int.Parse(n.InnerText);
                            break;
                        case "saveLastConfig":
                            SaveLastConfig = bool.Parse(n.InnerText);
                            break;
                        case "saveUserData":
                            SaveUserData = bool.Parse(n.InnerText);
                            break;
                        case "clearCache":
                            ClearCache = bool.Parse(n.InnerText);
                            break;
                        case "darkUI":
                            DarkUI = bool.Parse(n.InnerText);
                            break;
                        case "expandAllLegacy":
                            ExpandAllLegacy = bool.Parse(n.InnerText);
                            break;
                        case "disableBorders":
                            DisableBorders = bool.Parse(n.InnerText);
                            break;
                        case "disableColorChange":
                            DisableColorChange = bool.Parse(n.InnerText);
                            break;
                        case "deleteLogs":
                            DeleteLogs = bool.Parse(n.InnerText);
                            break;
                        case "language":
                            Translations.language = (Translations.Languages)int.Parse(n.InnerText);
                            break;
                        case "ModSelectionFullscreen":
                            ModSelectionFullscreen = bool.Parse(n.InnerText);
                            break;
                        case "NotifyIfSameDatabase":
                            NotifyIfSameDatabase = bool.Parse(n.InnerText);
                            break;
                        case "ShowInstallCompleteWindow":
                            ShowInstallCompleteWindow = bool.Parse(n.InnerText);
                            break;
                        case "previewX":
                            PreviewX = int.Parse(n.InnerText);
                            break;
                        case "previewY":
                            PreviewY = int.Parse(n.InnerText);
                            break;
                        case "customModInfoPath":
                            CustomModInfoPath = n.InnerText;
                            break; 
                        case "SelectionView":
                            SView = (SelectionView)int.Parse(n.InnerText);
                            break;
                        case "FontSizeForum":
                            FontSizeforum = (FontSize)int.Parse(n.InnerText);
                            break;
                        case "CreateShortcuts":
                            CreateShortcuts = bool.Parse(n.InnerText);
                            break;
                        case "InstantExtraction":
                            InstantExtraction = bool.Parse(n.InnerText);
                            break;
                        case "UninstallMode":
                            UninstallMode = (UninstallModes)int.Parse(n.InnerText);
                            break;
                        case "SuperExtraction":
                            SuperExtraction = bool.Parse(n.InnerText);
                            break;
                    }
                }
            }
            ApplInternalProperties();
            Logging.Manager("Settings loaded sucessfully");
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
            xsaveLastConfig.InnerText = "" + SaveLastConfig;
            settingsHolder.AppendChild(xsaveLastConfig);
            XmlElement xsaveUserData = doc.CreateElement("saveUserData");
            xsaveUserData.InnerText = "" + SaveUserData;
            settingsHolder.AppendChild(xsaveUserData);
            XmlElement xclearCache = doc.CreateElement("clearCache");
            xclearCache.InnerText = "" + ClearCache;
            settingsHolder.AppendChild(xclearCache);
            XmlElement xdarkUI = doc.CreateElement("darkUI");
            xdarkUI.InnerText = "" + DarkUI;
            settingsHolder.AppendChild(xdarkUI);
            XmlElement xexpandAllLegacy = doc.CreateElement("expandAllLegacy");
            xexpandAllLegacy.InnerText = "" + ExpandAllLegacy;
            settingsHolder.AppendChild(xexpandAllLegacy);
            XmlElement xdisableBorders = doc.CreateElement("disableBorders");
            xdisableBorders.InnerText = "" + DisableBorders;
            settingsHolder.AppendChild(xdisableBorders);
            XmlElement xdisableColorChange = doc.CreateElement("disableColorChange");
            xdisableColorChange.InnerText = "" + DisableColorChange;
            settingsHolder.AppendChild(xdisableColorChange);
            XmlElement xdeleteLogs = doc.CreateElement("deleteLogs");
            xdeleteLogs.InnerText = "" + DeleteLogs;
            settingsHolder.AppendChild(xdeleteLogs);
            XmlElement xNotifyIfSameDatabase = doc.CreateElement("NotifyIfSameDatabase");
            xNotifyIfSameDatabase.InnerText = "" + NotifyIfSameDatabase;
            settingsHolder.AppendChild(xNotifyIfSameDatabase);
            XmlElement xShowInstallCompleteWindow = doc.CreateElement("ShowInstallCompleteWindow");
            xShowInstallCompleteWindow.InnerText = "" + ShowInstallCompleteWindow;
            settingsHolder.AppendChild(xShowInstallCompleteWindow);
            XmlElement xlanguage = doc.CreateElement("language");
            xlanguage.InnerText = "" + (int)Translations.language;
            settingsHolder.AppendChild(xlanguage);
            XmlElement xSelectionView = doc.CreateElement("SelectionView");
            xSelectionView.InnerText = "" + (int)SView;
            settingsHolder.AppendChild(xSelectionView);
            XmlElement xloadingGif = doc.CreateElement("loadingGif");
            xloadingGif.InnerText = "" + (int)GIF;
            settingsHolder.AppendChild(xloadingGif);
            XmlElement xUninstallMode = doc.CreateElement("UninstallMode");
            xUninstallMode.InnerText = "" + (int)UninstallMode;
            settingsHolder.AppendChild(xUninstallMode);
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
            XmlElement xSuperExtraction = doc.CreateElement("SuperExtraction");
            xSuperExtraction.InnerText = "" + SuperExtraction;
            settingsHolder.AppendChild(xSuperExtraction);
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
            if (CustomModInfoPath != "") { customModInfoPath.InnerText = CustomModInfoPath; }
            settingsHolder.AppendChild(customModInfoPath); 

            doc.Save(SettingsXmlFile);
            Logging.Manager("Settings saved sucessfully");
        }
        //returns the loading image for the picture viewer, based on
        //which loading image the user specified
        public static Image getLoadingImage()
        {
            switch (GIF)
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
        public static void ApplInternalProperties()
        {
            if (ComicSans)
            {
                FontName = ComicSansFontType;
            }
            else
            {
                FontName = DefaultFontType;
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
        public static void setUIColor(Form window)
        {
            Color backColor;
            Color textColor;
            if (DarkUI)
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
            if (DarkUI)
                return Color.White;

            else
                return SystemColors.ControlText;
        }
        public static Color getBackColor()
        {
            if (DarkUI)
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
