using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Runtime.InteropServices;
using System;

namespace RelhaxModpack
{
    //all of the settings for the modpack. kept in a static class
    //so all the forms have access to a single version of the settings
    public static class Settings
    {
        public static bool backupModFolder { get; set; }
        public static bool cleanInstallation { get; set; }
        public static bool forceManuel { get; set; }
        public static bool expandAllLegacy { get; set; }
        public static bool comicSans { get; set; }
        public static bool firstLoad { get; set; }
        public static bool saveLastConfig { get; set; }
        public static bool saveUserData { get; set; }
        public static bool darkUI { get; set; }
        public static bool cleanUninstall { get; set; }
        public static bool disableBorders { get; set; }
        public static int modSelectionHeight { get; set; }
        public static int modSelectionWidth { get; set; }
        public static int loadingGif { get; set; }
        public static float fontSize { get; set; }
        public static string fontName { get; set; }
        public static string settingsXmlFile = Application.StartupPath + "\\RelHaxSettings.xml";
        public enum LoadingGifs { standard = 0, thirdGuards = 1 };
        public static LoadingGifs gif;
        public const float normalSizeFont = 8.25F;
        public const float largeSizeFont = 10.5F;
        public const float UHDSizeFont = 13.5F;
        public const string defaultFontType = "Microsoft Sance Serif";
        public const string comicSansFontType = "Comic Sans MS";
        private static int tempLoadedLanguage = -1;
        private static int tempFontSize = -1;
        public static bool ModSelectionFullscreen = false;
        public static int previewX = 0;
        public static int previewY = 0;
        //enumeration for the type of mod selection list view
        public enum SelectionView { defaultt = 0, legacy = 1 };
        public static SelectionView sView = SelectionView.defaultt;
        public enum FontSize { regular = 0, large = 1, UHD = 2 };
        public static FontSize fontSizeforum = FontSize.regular;
        public static int tempLoadedView = 0;
        public enum ScaleMode { font = 0, dpi = 1 };
        public static ScaleMode scalingMode = ScaleMode.font;
        private static int tempLoadedScaleMode = 0;
        //loads settings from xml file
        public static void loadSettings()
        {
            Settings.firstLoad = false;
            Utils.appendToLog("Loading application settings");
            if (!File.Exists(settingsXmlFile))
            {
                Utils.appendToLog("WARNING:Settings xml not found, loading defaults");
                //could also use this to determine if first load or not
                Settings.comicSans = false;
                Settings.backupModFolder = false;
                Settings.cleanInstallation = true;
                Settings.loadingGif = (int)LoadingGifs.standard;
                Settings.forceManuel = false;
                Settings.gif = Settings.LoadingGifs.standard;
                Settings.firstLoad = true;
                Settings.saveLastConfig = false;
                Settings.saveUserData = false;
                Settings.cleanUninstall = false;
                Settings.disableBorders = false;
                Settings.tempLoadedLanguage = 0;
                Settings.modSelectionHeight = 480;
                Settings.modSelectionWidth = 800;
                Settings.fontSizeforum = Settings.FontSize.regular;
                Settings.tempFontSize = (int)FontSize.regular;
                Settings.expandAllLegacy = false;
                ModSelectionFullscreen = false;
                previewX = 0;
                previewY = 0;
                Settings.sView = SelectionView.defaultt;
                Settings.tempLoadedScaleMode = 0;
                Settings.scalingMode = ScaleMode.font;
                Settings.applyInternalSettings();
            }
            else
            {
                Utils.appendToLog("Loading xml file");
                XmlDocument doc = new XmlDocument();
                doc.Load(settingsXmlFile);
                XmlNodeList settingsList = doc.ChildNodes[0].ChildNodes;
                foreach (XmlNode n in settingsList)
                {
                    switch (n.Name)
                    {
                        case "comicSans":
                            Settings.comicSans = bool.Parse(n.InnerText);
                            break;
                        case "fontSize":
                            Settings.tempFontSize = int.Parse(n.InnerText);
                            break;
                        case "backupModFolder":
                            Settings.backupModFolder = bool.Parse(n.InnerText);
                            break;
                        case "cleanInstallation":
                            Settings.cleanInstallation = bool.Parse(n.InnerText);
                            break;
                        case "loadingGif":
                            Settings.loadingGif = int.Parse(n.InnerText);
                            break;
                        case "forceManuel":
                            Settings.forceManuel = bool.Parse(n.InnerText);
                            break;
                        case "modSelectionHeight":
                            Settings.modSelectionHeight = int.Parse(n.InnerText);
                            break;
                        case "modSelectionWidth":
                            Settings.modSelectionWidth = int.Parse(n.InnerText);
                            break;
                        case "saveLastConfig":
                            Settings.saveLastConfig = bool.Parse(n.InnerText);
                            break;
                        case "saveUserData":
                            Settings.saveUserData = bool.Parse(n.InnerText);
                            break;
                        case "cleanUninstall":
                            Settings.cleanUninstall = bool.Parse(n.InnerText);
                            break;
                        case "darkUI":
                            Settings.darkUI = bool.Parse(n.InnerText);
                            break;
                        case "expandAllLegacy":
                            Settings.expandAllLegacy = bool.Parse(n.InnerText);
                            break;
                        case "disableBorders":
                            Settings.disableBorders = bool.Parse(n.InnerText);
                            break;
                        case "language":
                            Settings.tempLoadedLanguage = int.Parse(n.InnerText);
                            break;
                        case "ModSelectionFullscreen":
                            Settings.ModSelectionFullscreen = bool.Parse(n.InnerText);
                            break;
                        case "previewX":
                            Settings.previewX = int.Parse(n.InnerText);
                            break;
                        case "previewY":
                            Settings.previewY = int.Parse(n.InnerText);
                            break;
                        case "SelectionView":
                            Settings.tempLoadedView = int.Parse(n.InnerText);
                            break;
                        case "scalingMode":
                            Settings.tempLoadedScaleMode = int.Parse(n.InnerText);
                            break;
                    }
                }
            }
            Settings.applyInternalSettings();
            Utils.appendToLog("Settings loaded sucessfully");
        }
        //apply internal settings (font name, size, loading gif)
        //based on the boolean settings from above
        public static void applyInternalSettings()
        {
            switch (Settings.tempFontSize)
            {
                case 0:
                    Settings.fontSizeforum = FontSize.regular;
                    Settings.fontSize = Settings.normalSizeFont;
                    break;
                case 1:
                    Settings.fontSizeforum = FontSize.large;
                    Settings.fontSize = Settings.largeSizeFont;
                    break;
                case 2:
                    Settings.fontSizeforum = FontSize.UHD;
                    Settings.fontSize = Settings.UHDSizeFont;
                    break;
            }
            if (Settings.comicSans)
            {
                Settings.fontName = Settings.comicSansFontType;
            }
            else
            {
                Settings.fontName = Settings.defaultFontType;
            }
            switch (Settings.loadingGif)
            {
                case 0:
                    Settings.gif = Settings.LoadingGifs.standard;
                    break;
                case 1:
                    Settings.gif = Settings.LoadingGifs.thirdGuards;
                    break;
            }
            switch (Settings.tempLoadedLanguage)
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
            }
            //apply the internal setting of the view selection
            switch (Settings.tempLoadedView)
            {
                case 0:
                    //default (relhax)
                    Settings.sView = SelectionView.defaultt;
                    break;
                case 1:
                    //legacy (OMC)
                    Settings.sView = SelectionView.legacy;
                    break;
            }
            switch (Settings.tempLoadedScaleMode)
            {
                case (int)ScaleMode.dpi:
                    Settings.scalingMode = ScaleMode.dpi;
                    break;
                case (int)ScaleMode.font:
                    Settings.scalingMode = ScaleMode.font;
                    break;
            }
        }
        //saves settings to xml file
        public static void saveSettings()
        {
            Utils.appendToLog("Saving application settings");
            if (File.Exists(settingsXmlFile)) File.Delete(settingsXmlFile);
            XmlDocument doc = new XmlDocument();
            XmlElement settingsHolder = doc.CreateElement("settings");
            doc.AppendChild(settingsHolder);
            XmlElement xcomicSans = doc.CreateElement("comicSans");
            xcomicSans.InnerText = "" + comicSans;
            settingsHolder.AppendChild(xcomicSans);
            XmlElement xlargeFont = doc.CreateElement("fontSize");
            xlargeFont.InnerText = "" + (int)fontSizeforum;
            settingsHolder.AppendChild(xlargeFont);
            XmlElement xscalingMode = doc.CreateElement("scalingMode");
            xscalingMode.InnerText = "" + (int)scalingMode;
            settingsHolder.AppendChild(xscalingMode);
            XmlElement xbackupModFolder = doc.CreateElement("backupModFolder");
            xbackupModFolder.InnerText = "" + backupModFolder;
            settingsHolder.AppendChild(xbackupModFolder);
            XmlElement xcleanInstallation = doc.CreateElement("cleanInstallation");
            xcleanInstallation.InnerText = "" + cleanInstallation;
            settingsHolder.AppendChild(xcleanInstallation);
            XmlElement xsaveLastConfig = doc.CreateElement("saveLastConfig");
            xsaveLastConfig.InnerText = "" + Settings.saveLastConfig;
            settingsHolder.AppendChild(xsaveLastConfig);
            XmlElement xsaveUserData = doc.CreateElement("saveUserData");
            xsaveUserData.InnerText = "" + Settings.saveUserData;
            settingsHolder.AppendChild(xsaveUserData);
            XmlElement xcleanUninstall = doc.CreateElement("cleanUninstall");
            xcleanUninstall.InnerText = "" + Settings.cleanUninstall;
            settingsHolder.AppendChild(xcleanUninstall);
            XmlElement xdarkUI = doc.CreateElement("darkUI");
            xdarkUI.InnerText = "" + Settings.darkUI;
            settingsHolder.AppendChild(xdarkUI);
            XmlElement xexpandAllLegacy = doc.CreateElement("expandAllLegacy");
            xexpandAllLegacy.InnerText = "" + Settings.expandAllLegacy;
            settingsHolder.AppendChild(xexpandAllLegacy);
            XmlElement xdisableBorders = doc.CreateElement("disableBorders");
            xdisableBorders.InnerText = "" + Settings.disableBorders;
            settingsHolder.AppendChild(xdisableBorders);
            XmlElement xlanguage = doc.CreateElement("language");
            xlanguage.InnerText = "" + (int)Translations.language;
            settingsHolder.AppendChild(xlanguage);
            XmlElement xSelectionView = doc.CreateElement("SelectionView");
            xSelectionView.InnerText = "" + (int)Settings.sView;
            settingsHolder.AppendChild(xSelectionView);
            switch (Settings.gif)
            {
                case (Settings.LoadingGifs.standard):
                    {
                        Settings.loadingGif = 0;
                        break;
                    }
                case (Settings.LoadingGifs.thirdGuards):
                    {
                        Settings.loadingGif = 1;
                        break;
                    }
            }
            XmlElement xloadingGif = doc.CreateElement("loadingGif");
            xloadingGif.InnerText = "" + loadingGif;
            settingsHolder.AppendChild(xloadingGif);
            XmlElement xforceManuel = doc.CreateElement("forceManuel");
            xforceManuel.InnerText = "" + forceManuel;
            settingsHolder.AppendChild(xforceManuel);
            XmlElement xmodSelectionHeight = doc.CreateElement("modSelectionHeight");
            xmodSelectionHeight.InnerText = "" + modSelectionHeight;
            settingsHolder.AppendChild(xmodSelectionHeight);
            XmlElement xmodSelectionWidth = doc.CreateElement("modSelectionWidth");
            xmodSelectionWidth.InnerText = "" + modSelectionWidth;
            settingsHolder.AppendChild(xmodSelectionWidth);
            XmlElement xModSelectionFullscreen = doc.CreateElement("ModSelectionFullscreen");
            xModSelectionFullscreen.InnerText = "" + ModSelectionFullscreen;
            settingsHolder.AppendChild(xModSelectionFullscreen);
            XmlElement xpreviewX = doc.CreateElement("previewX");
            xpreviewX.InnerText = "" + previewX;
            settingsHolder.AppendChild(xpreviewX);
            XmlElement xpreviewY = doc.CreateElement("previewY");
            xpreviewY.InnerText = "" + previewY;
            settingsHolder.AppendChild(xpreviewY);

            doc.Save(settingsXmlFile);
            Utils.appendToLog("Settings saved sucessfully");
        }
        public static Image getLoadingImage(LoadingGifs gif)
        {
            switch (gif)
            {
                case (LoadingGifs.standard):
                    {
                        return RelhaxModpack.Properties.Resources.loading;
                    }
                case (LoadingGifs.thirdGuards):
                    {
                        return RelhaxModpack.Properties.Resources.loading_3rdguards;
                    }
            }
            return null;
        }
        //returns the loading image for the picture viewer, based on
        //which loading image the user specified
        public static Image getLoadingImage()
        {
            switch (Settings.gif)
            {
                case (LoadingGifs.standard):
                    {
                        return RelhaxModpack.Properties.Resources.loading;
                    }
                case (LoadingGifs.thirdGuards):
                    {
                        return RelhaxModpack.Properties.Resources.loading_3rdguards;
                    }
            }
            return null;
        }
        //returns a new font for the window
        public static Font getFont()
        {
            if (Settings.comicSans)
            {
                Settings.fontName = Settings.comicSansFontType;
            }
            else
            {
                Settings.fontName = Settings.defaultFontType;
            }
            switch (Settings.fontSizeforum)
            {
                case FontSize.regular:
                    Settings.fontSize = Settings.normalSizeFont;
                    break;
                case FontSize.large:
                    Settings.fontSize = Settings.largeSizeFont;
                    break;
                case FontSize.UHD:
                    Settings.fontSize = Settings.UHDSizeFont;
                    break;
            }
            return new System.Drawing.Font(fontName, fontSize);
        }
        public static AutoScaleMode getAutoScaleMode()
        {
            switch (Settings.scalingMode)
            {
                case Settings.ScaleMode.font:
                    return AutoScaleMode.Font;
                    
                case Settings.ScaleMode.dpi:
                    return AutoScaleMode.Dpi;
                    
            }
            return AutoScaleMode.Font;
        }
        //sets a form to have a dark UI
        public static void setUIColor(System.Windows.Forms.Form window)
        {
            Color backColor;
            Color textColor;
            if (Settings.darkUI)
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
            if (Settings.darkUI)
                return Color.White;

            else
                return SystemColors.ControlText;
        }
        public static Color getBackColor()
        {
            if (Settings.darkUI)
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
