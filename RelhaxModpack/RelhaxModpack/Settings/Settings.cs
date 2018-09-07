using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Settings
{
    #region Settings Enumerations

    #endregion
    //TODO: documentation
    public static class Settings
    {
        #region Settings constants
        /// <summary>
        /// The Startup root path of the application. Does not include the application name
        /// </summary>
        public static readonly string ApplicationStartupPath = System.AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// The absolute name of the application settings file
        /// </summary>
        public const string SettingsFileName = "RelHaxSettings.xml";
        #endregion

        #region Settings statics

        #endregion

        #region Settings refrences and init

        public static ModpackSettings ModpackSettings;

        public static UISettings UISettings;

        public static ThirdPartySettings ThirdPartySettings;

        public static bool InitSettings()
        {
            return true;
        }
        #endregion
    }
}
