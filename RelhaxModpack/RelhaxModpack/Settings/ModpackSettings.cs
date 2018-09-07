using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace RelhaxModpack.Settings
{
    /// <summary>
    /// Provides access to all settings used in the modpack.
    /// </summary>
    //TODO: make a static settings class, make modapck and Ui settings initializeable so to use dependencyproperties
    public class ModpackSettings
    {
        
        /// <summary>
        /// The absolute path of the application settings file
        /// </summary>
        //public static readonly string SettingsFilePath = Path.Combine(ApplicationStartupPath, SettingsFileName);
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
        public static bool SaveSettings()
        {

            return true;
        }
    }
}
