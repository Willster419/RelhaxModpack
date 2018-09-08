using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RelhaxModpack.Settings;

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
        private const string ApplicationLogfileTimestamp = "yyyy-MM-dd HH:mm:ss.fff";
        public const string ApplicationlogStartStop = "|------------------------------------------------------------------------------------------------|";
        /// <summary>
        /// Provides a constant refrence to the log file
        /// </summary>
        public static Logfile ApplicationLogfile;
        /// <summary>
        /// Provides a refrence to an instance of an install log file
        /// </summary>
        public static Logfile InstallLogfile;
        /// <summary>
        /// Provides a refrence to an instalce of an uninstall log file
        /// </summary>
        public static Logfile UninstallLogfile;
        private static bool FailedToWriteToLogWindowShown = false;
        /// <summary>
        /// Initialize the logging subsystem for the appilcation
        /// </summary>
        /// <returns>True if sucessfull initialization, false otherwise</returns>
        public static bool InitApplicationLogging()
        {
            if (ApplicationLogfile != null)
                throw new Utils.BadMemeException("only do this once jackass");
            string oldLogFilePath = Path.Combine(Settings.Settings.ApplicationStartupPath, OldApplicationLogFilename);
            string newLogFilePath = Path.Combine(Settings.Settings.ApplicationStartupPath, ApplicationLogFilename);
            //if the old log exists and the new one does not, move the logging to the new one
            try
            {
                if (File.Exists(oldLogFilePath) && !File.Exists(newLogFilePath))
                    File.Move(oldLogFilePath, newLogFilePath);
                Settings.Settings.FirstLoadToV2 = true;
            }
            catch
            {
                MessageBox.Show("Failed to move logfile");
                return false;
            }
            ApplicationLogfile = new Logfile(newLogFilePath, ApplicationLogfileTimestamp);
            if(!ApplicationLogfile.Init())
            {
                MessageBox.Show("Failed to initialize logfile, check file permissions");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Dispose of the application logging subsystem
        /// </summary>
        public static void DisposeApplicationLogging()
        {
            ApplicationLogfile.Dispose();
            ApplicationLogfile = null;
        }
        /// <summary>
        /// Writes a message to a logfile instance, if it exists
        /// </summary>
        /// <param name="message">The message to write</param>
        /// <param name="logfiles">The logfile to write to</param>
        /// <param name="logLevel">The level of severity of the message</param>
        public static void WriteToLog(string message, Logfiles logfiles = Logfiles.Application,LogLevel logLevel = LogLevel.Info)
        {
            Logfile fileToWriteTo = null;
            switch(logfiles)
            {
                case Logfiles.Application:
                    fileToWriteTo = ApplicationLogfile;
                    break;
                case Logfiles.Installer:
                    fileToWriteTo = InstallLogfile;
                    break;
                case Logfiles.Uninstaller:
                    fileToWriteTo = UninstallLogfile;
                    break;
            }
            //check if logfile is null
            if (fileToWriteTo == null)
            {
                //check if it's the application logfile
                if(fileToWriteTo == ApplicationLogfile)
                {
                    if(!FailedToWriteToLogWindowShown)
                    {
                        MessageBox.Show("Failed to write to application log: instance is null!");
                        FailedToWriteToLogWindowShown = true;
                    }
                }
                else
                {
                    WriteToLog(string.Format("Tried to write to null log instance: {0}", logfiles.ToString()), Logfiles.Application, LogLevel.Error);
                }
                return;
            }
            fileToWriteTo.Write(message, logLevel);
        }
    }
}
