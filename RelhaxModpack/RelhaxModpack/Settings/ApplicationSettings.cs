using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml;
using System.ComponentModel;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack.Settings
{
    /// <summary>
    /// Handles all generic constants and statics used throughout the application
    /// </summary>
    public static class ApplicationSettings
    {
        /// <summary>
        /// The location of the WoT app data folder parsed at installation time
        /// </summary>
        public static string AppDataFolder = string.Empty;

        /// <summary>
        /// The location of the WoT installation directory parsed at installation time
        /// </summary>
        /// <remarks>The path is absolute, ending at "World_of_Tanks"</remarks>
        public static string WoTDirectory = string.Empty;

        /// <summary>
        /// The version information of WoT parsed at installation time
        /// </summary>
        /// <remarks>This info is gathered from the "version.xml" file from the game's root directory</remarks>
        public static string WoTClientVersion = string.Empty;

        /// <summary>
        /// The version of the online folder name containing the zip files for this game parsed at installation time
        /// </summary>
        /// <remarks>The online folders are done by major versions only i.e. 1.4.1, 1.5.0, etc. All zip files on 1.5.0.x are stored in this folder</remarks>
        public static string WoTModpackOnlineFolderVersion = string.Empty;

        /// <summary>
        /// The version of the database parsed upon application load
        /// </summary>
        public static string DatabaseVersion = string.Empty;

        /// <summary>
        /// Determines if this is the first time the application is loading, parsed upon application load
        /// </summary>
        /// <remarks>Done by checking if the settings file exists. If it is set to true in the application, it will be set to false again when it closes.</remarks>
        public static bool FirstLoad = false;

        /// <summary>
        /// Determines if while being the first time loading, if this is an upgrade operation to Relhax V2, parsed upon application load
        /// </summary>
        /// <remarks>Done by if FirstLoad is true and the Relhax V1 settings file exists</remarks>
        public static bool FirstLoadToV2 = false;

        /// <summary>
        /// The manager info zip in a program reference. Allows for multiple instances of the application to be active at the same time. Also saves milliseconds by not having to write to disk. Parsed upon application load.
        /// </summary>
        public static Ionic.Zip.ZipFile ManagerInfoZipfile = null;

        /// <summary>
        /// Flag to determine if the user running is intentionally using the alpha version (or if an Alpha version was accidentally distributed), parsed upon application load.
        /// </summary>
        public static bool TrueAlpha = false;
    }
}
