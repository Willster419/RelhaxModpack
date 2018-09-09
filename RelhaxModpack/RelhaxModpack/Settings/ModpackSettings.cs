using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Windows;

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
        
        /// <summary>
        /// Initializes the Settings (should only be done on application start) and determinds which version of Settings loader method to use
        /// </summary>
        /// <returns></returns>
        public bool LoadSettings()
        {
            XmlDocument SettingsDocument = new XmlDocument();
            
            return true;
        }
        public bool SaveSettings()
        {
            XmlDocument SettingsDocument = new XmlDocument();
            return true;
        }
    }
}
