using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace RelhaxModpack
{
    //all of the settings for the modpack. kept in a static class
    //so all the forms have access to a single version of the settings
    #region cool enums
    //enumeration for the font size for the application
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
    #endregion
    public static class Settings
    {
        //#region general settings
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
        /// toggle if comic sans font should be the default font. true=comic sans, false=microsoft sans serif (default in most applications)
        /// </summary>
        public static bool ComicSans = false;
        /// <summary>
        /// flag for if it is the first time loading the application (determined if RelHaxSettings.xml exists or not)
        /// </summary>
        public static bool FirstLoad = false;
        /// <summary>
        /// toggle if the application should automatically save the last selected config to also be automatically loaded upon selection load
        /// </summary>
        public static bool SaveLastConfig = false;
        /// <summary>
        /// toggle if the application should save user cache save data like session stats, or auto equip configs
        /// </summary>
        public static bool SaveUserData = false;
        /// <summary>
        /// toggle a dark UI for using the modpack at night.
        /// TODO: after wpf move, change this to allow any coloring settings
        /// </summary>
        public static bool DarkUI = false;
        //toggle for each view if the borders around the child selection options should show
        public static bool EnableBordersDefaultView = false;
        public static bool EnableBordersLegacyView = false;
        public static bool EnableBordersDefaultV2View = false;
        //toggle for each view if the color change should occur when a child selection happends
        public static bool EnableColorChangeDefaultView = false;
        public static bool EnableColorChangeLegacyView = false;
        public static bool EnableColorChangeDefaultV2View = false;
        //toggle if the installation complete window will be shown
        public static bool ShowInstallCompleteWindow = false;
        //toggle if the program will delete the WoT appdata cache
        public static bool ClearCache = false;
        public static bool DeleteLogs = false;
        //toggle if the program will create desktop shortcuts
        public static bool CreateShortcuts = false;
        //toggle instant extraction
        public static bool InstantExtraction = false;
        //toggle super extraction
        public static bool SuperExtraction = false;
        public static string FontName;
        public static float ScaleSize;
        //turn on export mode
        public static bool ExportMode = false;
        /// <summary>
        /// toggle for if the user wants to use the beta database
        /// </summary>
        public static bool BetaDatabase = false;
        /// <summary>
        /// toggle for if the user wants to use the beta application (note it won't happen until application restart)
        /// </summary>
        public static bool BetaApplication = false;
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
        /// toggle for if the application should use the alternate updating method. Should be friendlier with antivirus applications.
        /// </summary>
        public static bool UseAlternateUpdateMethod = false;
        public static LoadingGifs GIF = LoadingGifs.Standard;
        public static UninstallModes UninstallMode = UninstallModes.Default;
        public static SelectionView SView = SelectionView.Default;
        public static FontSize FontSizeforum = FontSize.Font100;
        public static AutoScaleMode AppScalingMode = AutoScaleMode.Font;
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
        //file and folder locations
        public readonly static string SettingsXmlFile = Path.Combine(Application.StartupPath, "RelHaxSettings.xml");
        public static string RelhaxDownloadsFolder = Path.Combine(Application.StartupPath, "RelHaxDownloads");
        public static string RelHaxModBackupFolder = Path.Combine(Application.StartupPath, "RelHaxModBackup");
        public static string RelHaxLibrariesFolder = Path.Combine(Application.StartupPath, "RelHaxLibraries"); 
        public static string MD5HashDatabaseXmlFile = Path.Combine(RelhaxDownloadsFolder, "MD5HashDatabase.xml");
        public static string OnlineDatabaseXmlFile = Path.Combine(RelhaxDownloadsFolder, "onlineDatabase.xml");
        public static string RelhaxTempFolder = Path.Combine(Application.StartupPath, "RelHaxTemp");
        // this element creates the randowm session tag (to use with backup of user data and other temporary files and folders in the temp folder)
        private static string sT { get { return Path.GetRandomFileName(); } }
        // this element stored the random created Tag during the complete runtime of the exe
        public static string SessionTag = sT;
        // public static string ManagerInfoDatFile = Path.Combine(RelhaxTempFolder, Path.GetRandomFileName() + "_managerInfo.dat");
        public static string ManagerInfoDatFile = Path.Combine(RelhaxTempFolder, SessionTag + "_managerInfo.dat");
        public static string ModInfoDatFile = Path.Combine(RelhaxTempFolder, "modInfo.dat");
        public const string DefaultStartAddress = @"http://wotmods.relhaxmodpack.com/WoT/{onlineFolder}/";
        public const string DefaultEndAddress = @"";
        //file and folder macro locations
        public static string AppDataFolder = "";
        public static string TanksLocation = "";
        //version informations
        public static string TanksVersion = "";
        public static string TanksOnlineFolderVersion = "";
        //needed to create to first line to installedRelhaxFiles.log
        public static string DatabaseVersion = "";
        //the config file version for saving the user's selection prefrences
        public static string ConfigFileVersion = "2.0";     // for later imports of this files, we need a better identification
        public static string CustomModInfoPath = "";
        public static Font AppFont = new Font(DefaultFontType, FontSize100);
        //loads settings from xml file
        public static void LoadSettings()
        {
            //Settings declared here are set for what their default values should be, then later modified in the settings xml file
            //i.e. when new features are added
            FirstLoad = false;
            Logging.Manager("Loading application settings");
            if (!File.Exists(SettingsXmlFile))
            {
                Logging.Manager("WARNING:Settings xml not found, defaults used");
                //also specify that this is the first load
                FirstLoad = true;
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
                        case "EnableBordersDefaultView":
                            EnableBordersDefaultView = bool.Parse(n.InnerText);
                            break;
                        case "EnableBordersLegacyView":
                            EnableBordersLegacyView = bool.Parse(n.InnerText);
                            break;
                        case "EnableBordersDefaultV2View":
                            EnableBordersDefaultV2View = bool.Parse(n.InnerText);
                            break;
                        case "EnableChildColorChangeDefaultView":
                            EnableColorChangeDefaultView = bool.Parse(n.InnerText);
                            break;
                        case "EnableChildColorChangeLegacyView":
                            EnableColorChangeLegacyView = bool.Parse(n.InnerText);
                            break;
                        case "EnableChildColorChangeDefaultV2View":
                            EnableColorChangeDefaultV2View = bool.Parse(n.InnerText);
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
                        case "PreviewFullscreen":
                            PreviewFullscreen = bool.Parse(n.InnerText);
                            break;
                        case "PreviewWidth":
                            PreviewWidth = int.Parse(n.InnerText);
                            break;
                        case "PreviewHeight":
                            PreviewHeight = int.Parse(n.InnerText);
                            break;
                        case "ExportMode":
                            ExportMode = bool.Parse(n.InnerText);
                            break;
                        case "UseAlternateUpdateMethod":
                            UseAlternateUpdateMethod = bool.Parse(n.InnerText);
                            break;
                        case "BetaApplication":
                            if(!Program.betaAppSetFromCommandLine)
                                BetaApplication = bool.Parse(n.InnerText);
                            break;
                        case "BetaDatabase":
                            if(!Program.betaDBSetFromCommandLine)
                                BetaDatabase = bool.Parse(n.InnerText);
                            break;
                    }
                }
            }
            ApplyInternalProperties();
            Logging.Manager("Settings loaded successfully");
        }
        //saves settings to xml file
        public static void SaveSettings()
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
            //modselectionlistUI options
            XmlElement xEnableBordersDefaultView = doc.CreateElement("EnableBordersDefaultView");
            xEnableBordersDefaultView.InnerText = "" + EnableBordersDefaultView;
            settingsHolder.AppendChild(xEnableBordersDefaultView);
            XmlElement xEnableBordersDefaultV2View = doc.CreateElement("EnableBordersDefaultV2View");
            xEnableBordersDefaultV2View.InnerText = "" + EnableBordersDefaultV2View;
            settingsHolder.AppendChild(xEnableBordersDefaultV2View);
            XmlElement xEnableBordersLegacyView = doc.CreateElement("EnableBordersLegacyView");
            xEnableBordersLegacyView.InnerText = "" + EnableBordersLegacyView;
            settingsHolder.AppendChild(xEnableBordersLegacyView);
            XmlElement xEnableChildColorChangeDefaultView = doc.CreateElement("EnableChildColorChangeDefaultView");
            xEnableChildColorChangeDefaultView.InnerText = "" + EnableColorChangeDefaultView;
            settingsHolder.AppendChild(xEnableChildColorChangeDefaultView);
            XmlElement xEnableChildColorChangeDefaultV2View = doc.CreateElement("EnableChildColorChangeDefaultV2View");
            xEnableChildColorChangeDefaultV2View.InnerText = "" + EnableColorChangeDefaultV2View;
            settingsHolder.AppendChild(xEnableChildColorChangeDefaultV2View);
            XmlElement xEnableChildColorChangeLegacyView = doc.CreateElement("EnableChildColorChangeLegacyView");
            xEnableChildColorChangeLegacyView.InnerText = "" + EnableColorChangeLegacyView;
            settingsHolder.AppendChild(xEnableChildColorChangeLegacyView);
            //installer settings
            XmlElement xdeleteLogs = doc.CreateElement("deleteLogs");
            xdeleteLogs.InnerText = "" + DeleteLogs;
            settingsHolder.AppendChild(xdeleteLogs);
            XmlElement xNotifyIfSameDatabase = doc.CreateElement("NotifyIfSameDatabase");
            xNotifyIfSameDatabase.InnerText = "" + NotifyIfSameDatabase;
            settingsHolder.AppendChild(xNotifyIfSameDatabase);
            XmlElement xShowInstallCompleteWindow = doc.CreateElement("ShowInstallCompleteWindow");
            xShowInstallCompleteWindow.InnerText = "" + ShowInstallCompleteWindow;
            settingsHolder.AppendChild(xShowInstallCompleteWindow);
            XmlElement xExportMode = doc.CreateElement("ExportMode");
            xExportMode.InnerText = "" + ExportMode;
            settingsHolder.AppendChild(xExportMode);
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
            XmlElement xPreviewFullscreen = doc.CreateElement("PreviewFullscreen");
            xPreviewFullscreen.InnerText = "" + PreviewFullscreen;
            settingsHolder.AppendChild(xPreviewFullscreen);
            XmlElement xPreviewWidth = doc.CreateElement("PreviewWidth");
            xPreviewWidth.InnerText = "" + PreviewWidth;
            settingsHolder.AppendChild(xPreviewWidth);
            XmlElement xPreviewHeight = doc.CreateElement("PreviewHeight");
            xPreviewHeight.InnerText = "" + PreviewHeight;
            settingsHolder.AppendChild(xPreviewHeight);
            XmlElement xUseAlternateUpdateMethod = doc.CreateElement("UseAlternateUpdateMethod");
            xUseAlternateUpdateMethod.InnerText = "" + UseAlternateUpdateMethod;
            settingsHolder.AppendChild(xUseAlternateUpdateMethod);
            XmlElement xBetaApplication = doc.CreateElement("BetaApplication");
            xBetaApplication.InnerText = "" + BetaApplication;
            settingsHolder.AppendChild(xBetaApplication);
            XmlElement xBetaDatabase = doc.CreateElement("BetaDatabase");
            xBetaDatabase.InnerText = "" + BetaDatabase;
            settingsHolder.AppendChild(xBetaDatabase);

            doc.Save(SettingsXmlFile);
            Logging.Manager("Settings saved successfully");
        }
        //returns the loading image for the picture viewer, based on
        //which loading image the user specified
        public static Image GetLoadingImage()
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
        public static void ApplyInternalProperties()
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
        public static void SetUIColorsWinForms(Form form)
        {
            #region current color apply settings
            /*  Form back = requested color
            *   
            *   panel back = transparent
            *   groupbox back = transparent
            *   tablelayoutpanel back = transparent
            *   
            *   checkbox back = transparent
            *   radiobutton back = transparent
            *   label back = transparent
            *   tabcontrol back = transparent
            *   picturebox back = transparent
            *   tabpage back = transparent
            *   elementhost back = transparent
            *   
            *   button back = requested color
            *   textbox back = requested color
            *   richtextbox back = requested color
            *   
            *   all text is forcolor->textColor
            *   **KEEP THESE SETTINGS CONSISTANT!!**
            */
            #endregion
            Color backColor = (DarkUI) ? SystemColors.ControlDark : SystemColors.Control;
            Color textColor = (DarkUI) ? Color.White : SystemColors.ControlText;
            form.BackColor = backColor;
            SetUIColorsWinForms(form.Controls, backColor, textColor);
        }
        public static void SetUIColorsWinForms(Control.ControlCollection controls, Color backColor, Color textColor)
        {
            foreach (Control c in controls)
            {
                if (c is TableLayoutPanel || c is Panel)
                {
                    Panel p = (Panel)c;
                    p.ForeColor = textColor;
                    p.BackColor = Color.Transparent;
                    if (p.Controls.Count > 0)
                        SetUIColorsWinForms(p.Controls, backColor, textColor);
                }
                else if (c is GroupBox gb)
                {
                    gb.ForeColor = textColor;
                    gb.BackColor = Color.Transparent;
                    if(gb.Controls.Count > 0)
                    {
                        SetUIColorsWinForms(gb.Controls, backColor, textColor);
                    }
                }
                else if (c is CheckBox || c is RadioButton || c is Label || c is PictureBox)
                {
                    c.ForeColor = textColor;
                    c.BackColor = Color.Transparent;
                }
                else if (c is Button || c is TextBox || c is RichTextBox || c is ComboBox)
                {
                    c.ForeColor = textColor;
                    c.BackColor = backColor;
                }
            }
        }
        public static Color GetBackColorWinForms()
        {
            return (DarkUI) ? SystemColors.ControlDark : SystemColors.Control;
        }
        public static Color GetTextColorWinForms()
        {
            return (DarkUI) ? Color.White : SystemColors.ControlText;
        }
        public static System.Windows.Media.Brush GetBackColorWPF()
        {
            return (DarkUI) ? System.Windows.Media.Brushes.Gray : System.Windows.Media.Brushes.White;
        }
        public static System.Windows.Media.Brush GetTextColorWPF()
        {
            return (DarkUI) ? System.Windows.Media.Brushes.White : System.Windows.Media.Brushes.Black;
        }
    }
}
