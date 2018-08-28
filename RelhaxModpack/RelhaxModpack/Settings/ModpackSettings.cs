using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace RelhaxModpack.Settings
{
    #region Settings Enumerations

    #endregion
    /// <summary>
    /// Provides access to all settings used in the modpack. Common modpack settings are set with key/value pairs.
    /// </summary>
    public static class ModpackSettings
    {
        #region Common/Internal Modpack Settings
        /// <summary>
        /// The Startup root path of the application. Does not include the application name
        /// </summary>
        public static readonly string ApplicationStartupPath = System.AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// The absolute name of the application settings file
        /// </summary>
        public const string SettingsFileName = "RelHaxSettings.xml";
        /// <summary>
        /// The absolute path of the application settings file
        /// </summary>
        public static readonly string SettingsFilePath = Path.Combine(ApplicationStartupPath, SettingsFileName);
        /// <summary>
        /// The Dictionary provider of all common settings. Names are stored as their xpath names
        /// </summary>
        private static Dictionary<string, string> CommonSettings = new Dictionary<string, string>();
        private static XmlDocument SettingsDocument;
        public static bool LoadSettings()
        {

            return true;
        }
        private static void LoadSettingsLegacy()
        {

        }
        private static void LoadSettingsV1()
        {

        }
        public static bool SaveSettings()
        {

            return true;
        }
        private static void LoadDefaultSettings()
        {

        }
        #endregion
        
        #region ThirdParty Modpack Settings

        #endregion
    }
}
