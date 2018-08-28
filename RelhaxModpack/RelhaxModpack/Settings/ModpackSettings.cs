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
        //The Dictionary provider of all common settings. Names are stored as their xpath names
        private static Dictionary<string, string> CommonSettings = new Dictionary<string, string>();
        //The document for the main 
        private static XmlDocument SettingsDocument;
        /// <summary>
        /// Initializes the Settings (should only be done on application start) and determinds which version of Settings loader method to use
        /// </summary>
        /// <returns></returns>
        public static bool LoadSettings()
        {
            if(SettingsDocument != null)
            {
                //TODO: logging message here
                return false;
            }
            SettingsDocument = new XmlDocument();
            return true;
        }
        //for loading legacy style settings
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
