using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace RelhaxModpack
{
    //all of the settings for the modpack. kept in a static class
    //so all the forms have access to a single version of the settings
    public static class Settings
    {
        public static bool backupModFolder { get; set; }
        public static bool cleanInstallation { get; set; }
        public static bool forceManuel { get; set; }
        public static bool largeFont { get; set; }
        public static bool comicSans { get; set; }
        public static bool firstLoad { get; set; }
        public static bool saveLastConfig { get; set; }
        public static bool saveUserData { get; set; }
        public static bool darkUI { get; set; }
        public static bool cleanUninstall { get; set; }
        public static int modSelectionHeight { get; set; }
        public static int modSelectionWidth { get; set; }
        public static int loadingGif { get; set; }
        public static float fontSize { get; set; }
        public static string fontName { get; set; }
        public static string settingsXmlFile = Application.StartupPath + "\\RelHaxSettings.xml";
        public enum LoadingGifs { standard = 0, thirdGuards = 1 };
        public static LoadingGifs gif;
        public const float normalSizeFont = 8.25F;
        public const float largeSizeFont = 10.0F;
        public const string defaultFontType = "Microsoft Sance Serif";
        public const string comicSansFontType = "Comic Sans MS";
        private static int tempLoadedLanguage = -1;
        public static bool ModSelectionFullscreen = false;
        public static int previewX = 0;
        public static int previewY = 0;
        //enumeration for the type of mod selection list view
        public enum SelectionView { defaultt = 0, legacy = 1 };
        public static SelectionView sView = SelectionView.defaultt;
        public static int tempLoadedView = 0;
        //loads settings from xml file
        public static void loadSettings()
        {
            Settings.firstLoad = false;
            Settings.appendToLog("Loading application settings");
            if (!File.Exists(settingsXmlFile))
            {
                Settings.appendToLog("WARNING:Settings xml not found, loading defaults");
                //could also use this to determine if first load or not
                Settings.comicSans = false;
                Settings.largeFont = false;
                Settings.backupModFolder = false;
                Settings.cleanInstallation = true;
                Settings.loadingGif = (int)LoadingGifs.standard;
                Settings.forceManuel = false;
                Settings.gif = Settings.LoadingGifs.standard;
                Settings.firstLoad = true;
                Settings.saveLastConfig = false;
                Settings.saveUserData = false;
                Settings.cleanUninstall = false;
                Settings.tempLoadedLanguage = 0;
                Settings.modSelectionHeight = 250;
                Settings.modSelectionWidth = 520;
                ModSelectionFullscreen = false;
                previewX = 0;
                previewY = 0;
                Settings.sView = SelectionView.defaultt;
                Settings.applyInternalSettings();
            }
            else
            {
                Settings.appendToLog("Loading xml file");
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
                        case "largeFont":
                            Settings.largeFont = bool.Parse(n.InnerText);
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
                    }
                }
            }
            Settings.applyInternalSettings();
            Settings.appendToLog("Settings loaded sucessfully");
        }
        //apply internal settings (font name, size, loading gif)
        //based on the boolean settings from above
        public static void applyInternalSettings()
        {
            if (Settings.largeFont)
            {
                Settings.fontSize = Settings.largeSizeFont;
            }
            else
            {
                Settings.fontSize = Settings.normalSizeFont;
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
        }
        //saves settings to xml file
        public static void saveSettings()
        {
            Settings.appendToLog("Saving application settings");
            if (File.Exists(settingsXmlFile)) File.Delete(settingsXmlFile);
            XmlDocument doc = new XmlDocument();
            XmlElement settingsHolder = doc.CreateElement("settings");
            doc.AppendChild(settingsHolder);
            XmlElement xcomicSans = doc.CreateElement("comicSans");
            xcomicSans.InnerText = "" + comicSans;
            settingsHolder.AppendChild(xcomicSans);
            XmlElement xlargeFont = doc.CreateElement("largeFont");
            xlargeFont.InnerText = "" + largeFont;
            settingsHolder.AppendChild(xlargeFont);
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
            Settings.appendToLog("Settings saved sucessfully");
        }
        //logs string info to the log output
        public static void appendToLog(string info)
        {
            //the method should automaticly make the file if it's not there
            File.AppendAllText(Application.StartupPath + "\\RelHaxLog.txt", info + "\n");
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
        public static Font getFont(string fontName, float fontSize)
        {
            return new System.Drawing.Font(fontName, fontSize, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        }
        /*
         * DEACTIVATED to determine where the antivirus issue is
        //extracts embeded rescource onto disk
        public static void extractEmbeddedResource(string outputDir, string resourceLocation, List<string> files)
        {
            foreach (string file in files)
            {
                using (System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceLocation + @"." + file))
                {
                    using (System.IO.FileStream fileStream = new System.IO.FileStream(System.IO.Path.Combine(outputDir, file), System.IO.FileMode.Create))
                    {
                        for (int i = 0; i < stream.Length; i++)
                        {
                            fileStream.WriteByte((byte)stream.ReadByte());
                        }
                        fileStream.Close();
                    }
                }
            }
        }*/
        //returns the md5 hash of the file based on the input file string location
        public static string GetMd5Hash(string inputFile)
        {
            //first, return if the file does not exist
            if (!File.Exists(inputFile))
                return "-1";
            MD5 md5Hash = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            var stream = File.OpenRead(inputFile);
            byte[] data = md5Hash.ComputeHash(stream);
            stream.Close();
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();
            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // Return the hexadecimal string.
            return sBuilder.ToString();
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
                    //c.ForeColor = textColor;
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
        public static bool parseBool(string input, bool defaultValue)
        {
            bool returnVal;
            try
            {
                returnVal = bool.Parse(input);
            }
            catch (System.FormatException)
            {
                returnVal = defaultValue;
            }
            return returnVal;
        }
        public static int parseInt(string input, int defaultValue)
        {
            int returnVal;
            try
            {
                returnVal = int.Parse(input);
            }
            catch (System.FormatException)
            {
                returnVal = defaultValue;
            }
            return returnVal;
        }
        public static float parseFloat(string input, float defaultValue)
        {
            float returnVal;
            try
            {
                returnVal = float.Parse(input);
            }
            catch (System.FormatException)
            {
                returnVal = defaultValue;
            }
            return returnVal;
        }
    }
}
