using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Logging
{
    /// <summary>
    /// The different log files currently used in the modpack
    /// </summary>
    public enum Logfiles
    {
        /// <summary>
        /// The default modpack logfile
        /// </summary>
        Application,
        /// <summary>
        /// logfile for when installing mods
        /// </summary>
        Installer,
        /// <summary>
        /// logfile for when uninstalling mods
        /// </summary>
        Uninstaller
    }
    /// <summary>
    /// The level of severity of the log message
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Debug message
        /// </summary>
        Debug,
        /// <summary>
        /// Informational message
        /// </summary>
        Info,
        /// <summary>
        /// A problem, but can be worked around
        /// </summary>
        Warning,
        /// <summary>
        /// Something is wrong, something may not work
        /// </summary>
        Error,
        /// <summary>
        /// Something is wrong, something will not work
        /// </summary>
        Exception,
        /// <summary>
        /// The application is closing now
        /// </summary>
        ApplicationHalt
    }
    /// <summary>
    /// A static constant refrence to common logging variables and common log refrences
    /// </summary>
    public static class Logging
    {
        /// <summary>
        /// The filename of the application log file
        /// </summary>
        public const string ApplicationLogFilename = "Relhax.log";
        /// <summary>
        /// The filename of the old application log file
        /// </summary>
        public const string OldApplicationLogFilename = "RelHaxLog.txt";
        /// <summary>
        /// The name of the install log file
        /// </summary>
        public const string InstallLogFilename = "TODO";
        /// <summary>
        /// the name of the uninstall log file
        /// </summary>
        public const string UninstallLogFilename = "TODO";
        /// <summary>
        /// Provides a constant refrence to the log file
        /// </summary>
        public static Logfile ApplicationLogfile;
        /// <summary>
        /// Initialize the logging subsystem for the appilcation
        /// </summary>
        /// <returns>True if sucessfull initialization, false otherwise</returns>
        public static bool InitApplicationLogging()
        {

            return true;
        }
    }
}
