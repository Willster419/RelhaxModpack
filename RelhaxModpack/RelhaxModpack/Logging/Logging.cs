using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace RelhaxModpack
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
        Uninstaller,

        Editor,

        Patcher,

        Updater
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

    public delegate void LoggingUIThreadReport(string message);

    /// <summary>
    /// A static constant refrence to common logging variables and common log refrences
    /// </summary>
    public static class Logging
    {
        /// <summary>
        /// The filename of the application log file
        /// </summary>
        public const string ApplicationLogFilename = "Relhax.log";
        public const string ApplicationUpdaterLogFilename = "RelhaxUpdater.log";
        public const string ApplicationEditorLogFilename = "RelhaxEditor.log";
        public const string ApplicationPatchDesignerLogFilename = "RelhaxPatchDesigner.log";
        /// <summary>
        /// The filename of the old application log file
        /// </summary>
        public const string OldApplicationLogFilename = "RelHaxLog.txt";
        /// <summary>
        /// The name of the install log file
        /// </summary>
        public const string InstallLogFilename = "installedRelhaxFiles.log";
        public const string InstallLogFilenameBackup = "installedRelhaxFiles.bak";
        /// <summary>
        /// the name of the uninstall log file
        /// </summary>
        public const string UninstallLogFilename = "uninstallRelhaxFiles.log";
        public const string UninstallLogFilenameBackup = "uninstallRelhaxFiles.bak";
        public const string ApplicationLogfileTimestamp = "yyyy-MM-dd HH:mm:ss.fff";
        /// <summary>
        /// The header and end that shows the start and stop of the application log file
        /// </summary>
        public const string ApplicationlogStartStop = "|------------------------------------------------------------------------------------------------|";
        /// <summary>
        /// Provides a constant refrence to the log file
        /// </summary>
        private static Logfile ApplicationLogfile;
        /// <summary>
        /// Provides a refrence to an instance of an install log file
        /// </summary>
        private static Logfile InstallLogfile;
        /// <summary>
        /// Provides a refrence to an instalce of an uninstall log file
        /// </summary>
        private static Logfile UninstallLogfile;
        private static Logfile PatcherLogfile;
        private static Logfile EditorLogfile;
        private static Logfile UpdaterLogfile;
        private static bool FailedToWriteToLogWindowShown = false;
        public static event LoggingUIThreadReport OnLoggingUIThreadReport;
        /// <summary>
        /// Initialize the logging subsystem for the application
        /// </summary>
        /// <returns>True if successful initialization, false otherwise</returns>
        public static bool Init(Logfiles logfile, string logfilePath = null)
        {
            Logfile fileToWriteTo = null;

            //assign it here first to make sure it's null
            switch (logfile)
            {
                case Logfiles.Application:
                    fileToWriteTo = ApplicationLogfile;
                    logfilePath = ApplicationLogFilename;
                    break;
                case Logfiles.Installer:
                    fileToWriteTo = InstallLogfile;
                    break;
                case Logfiles.Uninstaller:
                    fileToWriteTo = UninstallLogfile;
                    break;
                case Logfiles.Editor:
                    fileToWriteTo = EditorLogfile;
                    logfilePath = ApplicationEditorLogFilename;
                    break;
                case Logfiles.Patcher:
                    fileToWriteTo = PatcherLogfile;
                    logfilePath = ApplicationPatchDesignerLogFilename;
                    break;
                case Logfiles.Updater:
                    fileToWriteTo = UpdaterLogfile;
                    logfilePath = ApplicationUpdaterLogFilename;
                    break;
            }

            if (fileToWriteTo != null)
                throw new BadMemeException("only do this once jackass");
            fileToWriteTo = new Logfile(logfilePath, ApplicationLogfileTimestamp);

            //now that it's newed, the reference needs to be reverse assigned
            switch (logfile)
            {
                case Logfiles.Application:
                    ApplicationLogfile = fileToWriteTo;
                    break;
                case Logfiles.Installer:
                    InstallLogfile = fileToWriteTo;
                    break;
                case Logfiles.Uninstaller:
                    UninstallLogfile = fileToWriteTo;
                    break;
                case Logfiles.Editor:
                    EditorLogfile = fileToWriteTo;
                    break;
                case Logfiles.Patcher:
                    PatcherLogfile = fileToWriteTo;
                    break;
                case Logfiles.Updater:
                    UpdaterLogfile = fileToWriteTo;
                    break;
            }

            if (!fileToWriteTo.Init())
            {
                MessageBox.Show(string.Format("Failed to initialize logfile {0}, check file permissions", logfilePath));
                return false;
            }
            return true;
        }

        public static bool IsLogDisposed(Logfiles file)
        {
            switch (file)
            {
                case Logfiles.Installer:
                    return InstallLogfile == null ? true : false;
                case Logfiles.Uninstaller:
                    return UninstallLogfile == null ? true : false;
                case Logfiles.Editor:
                    return EditorLogfile == null ? true : false;
                case Logfiles.Patcher:
                    return PatcherLogfile == null ? true : false;
                case Logfiles.Updater:
                    return UpdaterLogfile == null ? true : false;
                case Logfiles.Application:
                default:
                    return ApplicationLogfile == null ? true : false;
            }
        }

        public static bool IsLogOpen(Logfiles file)
        {
            switch (file)
            {
                case Logfiles.Installer:
                    if (InstallLogfile == null)
                        return false;
                    return InstallLogfile.CanWrite;

                case Logfiles.Uninstaller:
                    if (UninstallLogfile == null)
                        return false;
                    return UninstallLogfile.CanWrite;

                case Logfiles.Editor:
                    if (EditorLogfile == null)
                        return false;
                    return EditorLogfile.CanWrite;

                case Logfiles.Patcher:
                    if (PatcherLogfile == null)
                        return false;
                    return PatcherLogfile.CanWrite;

                case Logfiles.Updater:
                    if (UpdaterLogfile == null)
                        return false;
                    return UpdaterLogfile.CanWrite;

                case Logfiles.Application:
                default:
                    if (ApplicationLogfile == null)
                        return false;
                    return ApplicationLogfile.CanWrite;
            }
        }
        /// <summary>
        /// Dispose of the application logging subsystem
        /// </summary>
        public static void DisposeLogging(Logfiles logfile)
        {
            switch (logfile)
            {
                case Logfiles.Application:
                    ApplicationLogfile.Dispose();
                    ApplicationLogfile = null;
                    break;
                case Logfiles.Installer:
                    InstallLogfile.Dispose();
                    InstallLogfile = null;
                    break;
                case Logfiles.Uninstaller:
                    UninstallLogfile.Dispose();
                    UninstallLogfile = null;
                    break;
                case Logfiles.Editor:
                    EditorLogfile.Dispose();
                    EditorLogfile = null;
                    break;
                case Logfiles.Patcher:
                    PatcherLogfile.Dispose();
                    PatcherLogfile = null;
                    break;
                case Logfiles.Updater:
                    UpdaterLogfile.Dispose();
                    UpdaterLogfile = null;
                    break;
            }
        }

        public static void WriteHeader(Logfiles logfile)
        {
            switch(logfile)
            {
                case Logfiles.Application:
                    ApplicationLogfile.Write(ApplicationlogStartStop);
                    break;
                case Logfiles.Editor:
                    EditorLogfile.Write(ApplicationlogStartStop);
                    break;
                case Logfiles.Patcher:
                    PatcherLogfile.Write(ApplicationlogStartStop);
                    break;
                case Logfiles.Updater:
                    UpdaterLogfile.Write(ApplicationlogStartStop);
                    break;
            }
        }

        /// <summary>
        /// Writes a message to a logfile instance, if it exists
        /// </summary>
        /// <param name="message">The message to write</param>
        /// <param name="logfiles">The logfile to write to</param>
        /// <param name="logLevel">The level of severity of the message. If not Application log, this parameter is ignored</param>
        public static void WriteToLog(string message, Logfiles logfiles, LogLevel logLevel)
        {
            Logfile fileToWriteTo = null;
            switch(logfiles)
            {
                case Logfiles.Application:
                    fileToWriteTo = ApplicationLogfile;
                    break;
                case Logfiles.Updater:
                    fileToWriteTo = UpdaterLogfile;
                    break;
                case Logfiles.Editor:
                    fileToWriteTo = EditorLogfile;
                    break;
                case Logfiles.Patcher:
                    fileToWriteTo = PatcherLogfile;
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
            if (logfiles == Logfiles.Patcher)
            {
#warning this is hackey and should be fixed
                string temp = fileToWriteTo.Write(message, logLevel);
                if(OnLoggingUIThreadReport != null)
                {
                    OnLoggingUIThreadReport(temp);
                }
            }
            else
            {
                fileToWriteTo.Write(message, logLevel);
            }
        }

        public static void WriteToLog(string messageFormat, Logfiles logfile, LogLevel level, params object[] args)
        {
            WriteToLog(string.Format(messageFormat, args),logfile,level);
        }

        public static void Debug(string message)
        {
            WriteToLog(message, Logfiles.Application, LogLevel.Debug);
        }

        public static void Debug(string message, params object[] args)
        {
            WriteToLog(message, Logfiles.Application, LogLevel.Debug, args);
        }

        public static void Info(string message)
        {
            WriteToLog(message, Logfiles.Application, LogLevel.Info);
        }

        public static void Info(string message, params object[] args)
        {
            WriteToLog(message, Logfiles.Application, LogLevel.Info, args);
        }

        public static void Warning(string message)
        {
            WriteToLog(message, Logfiles.Application, LogLevel.Warning);
        }

        public static void Warning(string message, params object[] args)
        {
            WriteToLog(message, Logfiles.Application, LogLevel.Warning, args);
        }

        public static void Error(string message)
        {
            WriteToLog(message, Logfiles.Application, LogLevel.Error);
        }

        public static void Error(string message, params object[] args)
        {
            WriteToLog(message, Logfiles.Application, LogLevel.Error, args);
        }

        public static void Exception(string message)
        {
            WriteToLog(message, Logfiles.Application, LogLevel.Error);
        }

        public static void Exception(string message, params object[] args)
        {
            WriteToLog(message, Logfiles.Application, LogLevel.Exception, args);
        }

        public static void Installer(string message)
        {
            InstallLogfile.Write(message);
        }

        public static void Installer(string message, params object[] args)
        {
            InstallLogfile.Write(string.Format(message, args));
        }

        public static void Uninstaller(string message)
        {
            InstallLogfile.Write(message);
        }

        public static void Uninstaller(string message, params object[] args)
        {
            InstallLogfile.Write(string.Format(message, args));
        }

        public static void Editor(string message, LogLevel level = LogLevel.Info)
        {
            WriteToLog(message, Logfiles.Editor, level);
        }

        public static void Editor(string message, LogLevel level = LogLevel.Info, params object[] args)
        {
            WriteToLog(message, Logfiles.Editor, level, args);
        }

        public static void Updater(string message, LogLevel level = LogLevel.Info)
        {
            WriteToLog(message, Logfiles.Updater, level);
        }

        public static void Updater(string message, LogLevel level = LogLevel.Info, params object[] args)
        {
            WriteToLog(message, Logfiles.Updater, level, args);
        }

        public static void Patcher(string message, LogLevel level = LogLevel.Info)
        {
            WriteToLog(message, Logfiles.Patcher, level);
        }

        public static void Patcher(string message, LogLevel level = LogLevel.Info, params object[] args)
        {
            WriteToLog(message, Logfiles.Patcher, level, args);
        }
    }
}
