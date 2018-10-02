using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace RelhaxModpack
{
    /// <summary>
    /// Contains all properties that can be over-ridden or used when a 3rd party modpacker wants
    /// to use the Relhax install engine
    /// </summary>
    public static class ThirdPartySettings
    {
        #region 3rd party custom property settings
        //TODO get the defaults from other settings classes
        //TODO seperate them into required and optional
        public static string DefaultPackageStartAddress {get; private set;}= Settings.DefaultStartAddress;
        public static string DefaultPackageEndAddress {get; private set;} = Settings.DefaultEndAddress;
        public static string CustomIconPath {get; private set;} = "";
        public static string CustomApplication {get; private set;} = "Relhax Modpack";
        public static string CustomApplicationName {get; private set;} = "RelhaxModpack.exe";
        public static string BetaDatabaseURL {get; private set;} = "";//leave blank or write False to disable
        //TODO:
        //option to disable beta application chain
        //options for the 3 forum buttons
        //public static string DisableBetaApplicationDistribtion {get; private set;} = "True";
        //public static string DisableBetaDatabaseDistribution {get; private set;} = true;
        public static string StableDatabaseURL {get; private set;} = "";
        public static string ManagerInfoURL {get; private set;} = "";
        public static string SupportedClientsURL {get; private set;} = "";
        public static string DonationURL {get; private set;} = "";
        public static string WebsiteURL {get; private set;}
        #endregion
        //publicly accessable boolean variable to use for custom overwrites in the code later
        public static bool Use3rdPartySettings = false;
        /// <summary>
        /// Load the custom 3rd party settings from XML, if they exist
        /// </summary>
        public static void LoadSettings(string settingsFile)
        {
            Logging.WriteToLog("Checking for third party settings...");
            if(!File.Exists(settingsFile))
            {
                Logging.WriteToLog("does NOT exist");
                return;
            }
            Logging.WriteToLog("DOES exist, loading in 3rd party mode");
            Use3rdPartySettings = true;
            //switch if xml file exists
            //load xml based on all possible get options
        }
        
        public static void ApplyThirdPartyWindowSettings(Window w)
        {
            //stub TODO
            if(w is MainWindow mainWindow)
            {
                //custom code to disable or rename extra features
            }
        }
    }
}
