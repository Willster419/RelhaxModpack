using RelhaxModpack.Utilities;
using System.Windows;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack
{
    /// <summary>
    /// A static constant reference to common logging variables and common log references
    /// </summary>
    public static class Logging
    {
        /// <summary>
        /// The filename of the application log file
        /// </summary>
        public const string ApplicationLogFilename = "Relhax.log";

        /// <summary>
        /// The filename of the updater log file
        /// </summary>
        public const string ApplicationUpdaterLogFilename = "RelhaxUpdater.log";

        /// <summary>
        /// The filename of the editor log file
        /// </summary>
        public const string ApplicationEditorLogFilename = "RelhaxEditor.log";

        /// <summary>
        /// The filename of the patch designer
        /// </summary>
        public const string ApplicationPatchDesignerLogFilename = "RelhaxPatchDesigner.log";

        /// <summary>
        /// The filename of the old application log file
        /// </summary>
        public const string OldApplicationLogFilename = "RelHaxLog.txt";

        /// <summary>
        /// The name of the install log file
        /// </summary>
        public const string InstallLogFilename = "installedRelhaxFiles.log";

        /// <summary>
        /// The name of the backup install log file. During an install process, it backups the current one to provide a history of 1.
        /// </summary>
        public const string InstallLogFilenameBackup = "installedRelhaxFiles.bak";

        /// <summary>
        /// the name of the uninstall log file
        /// </summary>
        public const string UninstallLogFilename = "uninstallRelhaxFiles.log";

        /// <summary>
        /// The name of the backup uninstall log file. During an uninstall process, it backups the current one to provide a history of 1.
        /// </summary>
        public const string UninstallLogFilenameBackup = "uninstallRelhaxFiles.bak";

        /// <summary>
        /// The string time format for log entries
        /// </summary>
        public const string ApplicationLogfileTimestamp = "yyyy-MM-dd HH:mm:ss.fff";
        
        /// <summary>
        /// The header and end that shows the start and stop of the application log file
        /// </summary>
        public const string ApplicationlogStartStop = "|------------------------------------------------------------------------------------------------|";

        /// <summary>
        /// Provides a constant reference to the log file
        /// </summary>
        private static Logfile ApplicationLogfile;

        /// <summary>
        /// Provides a reference to an instance of an install log file
        /// </summary>
        private static Logfile InstallLogfile;

        /// <summary>
        /// Provides a reference to an instance of an uninstall log file
        /// </summary>
        private static Logfile UninstallLogfile;

        private static Logfile PatcherLogfile;

        private static Logfile EditorLogfile;

        private static Logfile UpdaterLogfile;

        private static bool FailedToWriteToLogWindowShown = false;

        /// <summary>
        /// Initialize the logging system for the application
        /// </summary>
        /// <param name="logfile">The log file to initialize</param>
        /// <param name="logfilePath">The custom path of where to initialize the file</param>
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
                case Logfiles.PatchDesigner:
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
#pragma warning disable IDE0068 // Use recommended dispose pattern
            fileToWriteTo = new Logfile(logfilePath, ApplicationLogfileTimestamp);
#pragma warning restore IDE0068 // Use recommended dispose pattern

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
                case Logfiles.PatchDesigner:
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

        /// <summary>
        /// Get the instance of the logfile type
        /// </summary>
        /// <param name="logfile">The logfile enumeration type you would like</param>
        /// <returns>The instance of the log file</returns>
        public static Logfile GetLogfile(Logfiles logfile)
        {
            Logfile fileToGet = null;

            //assign it here first to make sure it's null
            switch (logfile)
            {
                case Logfiles.Application:
                    fileToGet = ApplicationLogfile;
                    break;
                case Logfiles.Installer:
                    fileToGet = InstallLogfile;
                    break;
                case Logfiles.Uninstaller:
                    fileToGet = UninstallLogfile;
                    break;
                case Logfiles.Editor:
                    fileToGet = EditorLogfile;
                    break;
                case Logfiles.PatchDesigner:
                    fileToGet = PatcherLogfile;
                    break;
                case Logfiles.Updater:
                    fileToGet = UpdaterLogfile;
                    break;
            }

            return fileToGet;
        }

        /// <summary>
        /// Checks if the logfile is disposed
        /// </summary>
        /// <param name="file">The logfile to check</param>
        /// <returns>True if the logfile object is null</returns>
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
                case Logfiles.PatchDesigner:
                    return PatcherLogfile == null ? true : false;
                case Logfiles.Updater:
                    return UpdaterLogfile == null ? true : false;
                case Logfiles.Application:
                default:
                    return ApplicationLogfile == null ? true : false;
            }
        }

        /// <summary>
        /// Checks if the logfile is instanced and can be written to using the CanWrite property
        /// </summary>
        /// <param name="file">The logfile to check</param>
        /// <returns>True if the logfile is open and available to be written to</returns>
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

                case Logfiles.PatchDesigner:
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
        /// <param name="logfile">The logfile to dispose</param>
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
                case Logfiles.PatchDesigner:
                    PatcherLogfile.Dispose();
                    PatcherLogfile = null;
                    break;
                case Logfiles.Updater:
                    UpdaterLogfile.Dispose();
                    UpdaterLogfile = null;
                    break;
            }
        }

        /// <summary>
        /// Writes the ApplicationlogStartStop constant to the logfile
        /// </summary>
        /// <param name="logfile">The logfile to write the header on</param>
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
                case Logfiles.PatchDesigner:
                    PatcherLogfile.Write(ApplicationlogStartStop);
                    break;
                case Logfiles.Updater:
                    UpdaterLogfile.Write(ApplicationlogStartStop);
                    break;
            }
        }

        /// <summary>
        /// Attempts to write to a specified logfile. No error is logged if it cannot
        /// </summary>
        /// <remarks>It checks to make sure the logfile isn't disposed, and then to make sure the logfile is open in a valid instance</remarks>
        /// <param name="message">The message to write</param>
        /// <param name="logfile">The logfile to write to</param>
        /// <param name="logLevel">The level of severity of the message. If not Application log, this parameter is ignored</param>
        public static void TryWriteToLog(string message, Logfiles logfile, LogLevel logLevel)
        {
            if (!IsLogDisposed(logfile) && IsLogOpen(logfile))
                WriteToLog(message, logfile, logLevel);
        }

        /// <summary>
        /// Attempts to write to a specified logfile. No error is logged if it cannot
        /// </summary>
        /// <param name="messageFormat">The formatted string to be passed into the string.Format() method</param>
        /// <param name="logfile">The logfile to write to</param>
        /// <param name="logLevel">The level of severity of the message. If not Application log, this parameter is ignored</param>
        /// <param name="args">The arguments to be passed into the string.Format() method</param>
        public static void TryWriteToLog(string messageFormat, Logfiles logfile, LogLevel logLevel, params object[] args)
        {
            TryWriteToLog(string.Format(messageFormat, args), logfile, logLevel);
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
                case Logfiles.PatchDesigner:
                    fileToWriteTo = PatcherLogfile;
                    break;
                case Logfiles.Installer:
                    fileToWriteTo = InstallLogfile;
                    break;
                case Logfiles.Uninstaller:
                    fileToWriteTo = UninstallLogfile;
                    break;
            }
            //check if the application logfile is null and the application is now in a new mode
            if(fileToWriteTo == null && CommandLineSettings.ApplicationMode != ApplicationMode.Default)
            {
                switch (CommandLineSettings.ApplicationMode)
                {
                    case ApplicationMode.Editor:
                        fileToWriteTo = EditorLogfile;
                        break;
                    case ApplicationMode.PatchDesigner:
                        fileToWriteTo = PatcherLogfile;
                        break;
                    case ApplicationMode.Updater:
                        fileToWriteTo = UpdaterLogfile;
                        break;
                }
            }
            //check if logfile is null
            else if (fileToWriteTo == null)
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

        /// <summary>
        /// Writes a message to a logfile instance, if it exists
        /// </summary>
        /// <param name="messageFormat">The formatted string to be passed into the string.Format() method</param>
        /// <param name="logfile">The logfile to write to</param>
        /// <param name="level">The severity level of the message. Will be written as part of the format</param>
        /// <param name="args">The arguments to be passed into the string.Format() method</param>
        public static void WriteToLog(string messageFormat, Logfiles logfile, LogLevel level, params object[] args)
        {
            WriteToLog(string.Format(messageFormat, args),logfile,level);
        }

        /// <summary>
        /// Writes a debug level message to the application log
        /// </summary>
        /// <param name="message">The message</param>
        public static void Debug(string message)
        {
            WriteToLog(message, Logfiles.Application, LogLevel.Debug);
        }

        /// <summary>
        /// Writes a debug level message to the application log
        /// </summary>
        /// <param name="message">The formatted string to be passed into the string.Format() method</param>
        /// <param name="args">The arguments to be passed into the string.Format() method</param>
        public static void Debug(string message, params object[] args)
        {
            WriteToLog(message, Logfiles.Application, LogLevel.Debug, args);
        }

        /// <summary>
        /// Writes an debug level message to the application log
        /// </summary>
        /// <param name="options">Log append options to include class name, method name, or both</param>
        /// <param name="message">The formatted string to be passed into the string.Format() method</param>
        public static void Debug(LogOptions options, string message)
        {
            switch (options)
            {
                case LogOptions.ClassName:
                    WriteToLog(string.Format("[{0}]: {1}", CommonUtils.GetExecutingClassName(), message), Logfiles.Application, LogLevel.Debug);
                    break;

                case LogOptions.MethodAndClassName:
                    WriteToLog(string.Format("[{0}@{1}]: {2}", CommonUtils.GetExecutingMethodName(), CommonUtils.GetExecutingClassName(), message), Logfiles.Application, LogLevel.Debug);
                    break;

                case LogOptions.MethodName:
                    WriteToLog(string.Format("[{0}]: {1}", CommonUtils.GetExecutingMethodName(), message), Logfiles.Application, LogLevel.Debug);
                    break;

                case LogOptions.None:
                    WriteToLog(message, Logfiles.Application, LogLevel.Debug);
                    break;
            }
        }

        /// <summary>
        /// Writes an debug (info) level message to the application log
        /// </summary>
        /// <param name="options">Log append options to include class name, method name, or both</param>
        /// <param name="message">The formatted string to be passed into the string.Format() method</param>
        /// <param name="args">The arguments to be passed into the string.Format() method</param>
        public static void Debug(LogOptions options, string message, params object[] args)
        {
            switch (options)
            {
                case LogOptions.ClassName:
                    WriteToLog(string.Format("[{0}]: {1}", CommonUtils.GetExecutingClassName(), message), Logfiles.Application, LogLevel.Debug, args);
                    break;

                case LogOptions.MethodAndClassName:
                    WriteToLog(string.Format("[{0}@{1}]: {2}", CommonUtils.GetExecutingMethodName(), CommonUtils.GetExecutingClassName(), message), Logfiles.Application, LogLevel.Debug, args);
                    break;

                case LogOptions.MethodName:
                    WriteToLog(string.Format("[{0}]: {1}", CommonUtils.GetExecutingMethodName(), message), Logfiles.Application, LogLevel.Debug, args);
                    break;

                case LogOptions.None:
                    WriteToLog(message, Logfiles.Application, LogLevel.Debug, args);
                    break;
            }
        }

        /// <summary>
        /// Writes an information (info) level message to the application log
        /// </summary>
        /// <param name="message">The message</param>
        public static void Info(string message)
        {
            WriteToLog(message, Logfiles.Application, LogLevel.Info);
        }

        /// <summary>
        /// Writes an information (info) level message to the application log
        /// </summary>
        /// <param name="message">The formatted string to be passed into the string.Format() method</param>
        /// <param name="args">The arguments to be passed into the string.Format() method</param>
        public static void Info(string message, params object[] args)
        {
            WriteToLog(message, Logfiles.Application, LogLevel.Info, args);
        }

        /// <summary>
        /// Writes an information (info) level message to the application log
        /// </summary>
        /// <param name="options">Log append options to include class name, method name, or both</param>
        /// <param name="message">The formatted string to be passed into the string.Format() method</param>
        public static void Info(LogOptions options, string message)
        {
            switch (options)
            {
                case LogOptions.ClassName:
                    WriteToLog(string.Format("[{0}]: {1}", CommonUtils.GetExecutingClassName(), message), Logfiles.Application, LogLevel.Info);
                    break;

                case LogOptions.MethodAndClassName:
                    WriteToLog(string.Format("[{0}@{1}]: {2}", CommonUtils.GetExecutingMethodName(), CommonUtils.GetExecutingClassName(), message), Logfiles.Application, LogLevel.Info);
                    break;

                case LogOptions.MethodName:
                    WriteToLog(string.Format("[{0}]: {1}", CommonUtils.GetExecutingMethodName(), message), Logfiles.Application, LogLevel.Info);
                    break;

                case LogOptions.None:
                    WriteToLog(message, Logfiles.Application, LogLevel.Info);
                    break;
            }
        }

        /// <summary>
        /// Writes an information (info) level message to the application log
        /// </summary>
        /// <param name="options">Log append options to include class name, method name, or both</param>
        /// <param name="message">The formatted string to be passed into the string.Format() method</param>
        /// <param name="args">The arguments to be passed into the string.Format() method</param>
        public static void Info(LogOptions options, string message, params object[] args)
        {
            switch(options)
            {
                case LogOptions.ClassName:
                    WriteToLog(string.Format("[{0}]: {1}", CommonUtils.GetExecutingClassName(), message), Logfiles.Application, LogLevel.Info, args);
                    break;

                case LogOptions.MethodAndClassName:
                    WriteToLog(string.Format("[{0}@{1}]: {2}", CommonUtils.GetExecutingMethodName(), CommonUtils.GetExecutingClassName(), message), Logfiles.Application, LogLevel.Info, args);
                    break;

                case LogOptions.MethodName:
                    WriteToLog(string.Format("[{0}]: {1}", CommonUtils.GetExecutingMethodName(), message), Logfiles.Application, LogLevel.Info, args);
                    break;

                case LogOptions.None:
                    WriteToLog(message, Logfiles.Application, LogLevel.Info, args);
                    break;
            }
        }

        /// <summary>
        /// Writes a warning level message to the application log
        /// </summary>
        /// <param name="message">The message</param>
        public static void Warning(string message)
        {
            WriteToLog(message, Logfiles.Application, LogLevel.Warning);
        }

        /// <summary>
        /// Writes a warning level message to the application log
        /// </summary>
        /// <param name="message">The formatted string to be passed into the string.Format() method</param>
        /// <param name="args">The arguments to be passed into the string.Format() method</param>
        public static void Warning(string message, params object[] args)
        {
            WriteToLog(message, Logfiles.Application, LogLevel.Warning, args);
        }

        /// <summary>
        /// Writes a warning level message to the application log
        /// </summary>
        /// <param name="options">Log append options to include class name, method name, or both</param>
        /// <param name="message">The formatted string to be passed into the string.Format() method</param>
        public static void Warning(LogOptions options, string message)
        {
            switch (options)
            {
                case LogOptions.ClassName:
                    WriteToLog(string.Format("[{0}]: {1}", CommonUtils.GetExecutingClassName(), message), Logfiles.Application, LogLevel.Warning);
                    break;

                case LogOptions.MethodAndClassName:
                    WriteToLog(string.Format("[{0}@{1}]: {2}", CommonUtils.GetExecutingMethodName(), CommonUtils.GetExecutingClassName(), message), Logfiles.Application, LogLevel.Warning);
                    break;

                case LogOptions.MethodName:
                    WriteToLog(string.Format("[{0}]: {1}", CommonUtils.GetExecutingMethodName(), message), Logfiles.Application, LogLevel.Warning);
                    break;

                case LogOptions.None:
                    WriteToLog(message, Logfiles.Application, LogLevel.Warning);
                    break;
            }
        }

        /// <summary>
        /// Writes a warning level message to the application log
        /// </summary>
        /// <param name="options">Log append options to include class name, method name, or both</param>
        /// <param name="message">The formatted string to be passed into the string.Format() method</param>
        /// <param name="args">The arguments to be passed into the string.Format() method</param>
        public static void Warning(LogOptions options, string message, params object[] args)
        {
            switch (options)
            {
                case LogOptions.ClassName:
                    WriteToLog(string.Format("[{0}]: {1}", CommonUtils.GetExecutingClassName(), message), Logfiles.Application, LogLevel.Warning, args);
                    break;

                case LogOptions.MethodAndClassName:
                    WriteToLog(string.Format("[{0}@{1}]: {2}", CommonUtils.GetExecutingMethodName(), CommonUtils.GetExecutingClassName(), message), Logfiles.Application, LogLevel.Warning, args);
                    break;

                case LogOptions.MethodName:
                    WriteToLog(string.Format("[{0}]: {1}", CommonUtils.GetExecutingMethodName(), message), Logfiles.Application, LogLevel.Warning, args);
                    break;

                case LogOptions.None:
                    WriteToLog(message, Logfiles.Application, LogLevel.Warning, args);
                    break;
            }
        }

        /// <summary>
        /// Writes an error level message to the application log
        /// </summary>
        /// <param name="message">The message</param>
        public static void Error(string message)
        {
            WriteToLog(message, Logfiles.Application, LogLevel.Error);
        }

        /// <summary>
        /// Writes a error level message to the application log
        /// </summary>
        /// <param name="message">The formatted string to be passed into the string.Format() method</param>
        /// <param name="args">The arguments to be passed into the string.Format() method</param>
        public static void Error(string message, params object[] args)
        {
            WriteToLog(message, Logfiles.Application, LogLevel.Error, args);
        }

        /// <summary>
        /// Writes an error level message to the application log
        /// </summary>
        /// <param name="options">Log append options to include class name, method name, or both</param>
        /// <param name="message">The formatted string to be passed into the string.Format() method</param>
        public static void Error(LogOptions options, string message)
        {
            switch (options)
            {
                case LogOptions.ClassName:
                    WriteToLog(string.Format("[{0}]: {1}", CommonUtils.GetExecutingClassName(), message), Logfiles.Application, LogLevel.Error);
                    break;

                case LogOptions.MethodAndClassName:
                    WriteToLog(string.Format("[{0}@{1}]: {2}", CommonUtils.GetExecutingMethodName(), CommonUtils.GetExecutingClassName(), message), Logfiles.Application, LogLevel.Error);
                    break;

                case LogOptions.MethodName:
                    WriteToLog(string.Format("[{0}]: {1}", CommonUtils.GetExecutingMethodName(), message), Logfiles.Application, LogLevel.Error);
                    break;

                case LogOptions.None:
                    WriteToLog(message, Logfiles.Application, LogLevel.Error);
                    break;
            }
        }

        /// <summary>
        /// Writes an error level message to the application log
        /// </summary>
        /// <param name="options">Log append options to include class name, method name, or both</param>
        /// <param name="message">The formatted string to be passed into the string.Format() method</param>
        /// <param name="args">The arguments to be passed into the string.Format() method</param>
        public static void Error(LogOptions options, string message, params object[] args)
        {
            switch (options)
            {
                case LogOptions.ClassName:
                    WriteToLog(string.Format("[{0}]: {1}", CommonUtils.GetExecutingClassName(), message), Logfiles.Application, LogLevel.Error, args);
                    break;

                case LogOptions.MethodAndClassName:
                    WriteToLog(string.Format("[{0}@{1}]: {2}", CommonUtils.GetExecutingMethodName(), CommonUtils.GetExecutingClassName(), message), Logfiles.Application, LogLevel.Error, args);
                    break;

                case LogOptions.MethodName:
                    WriteToLog(string.Format("[{0}]: {1}", CommonUtils.GetExecutingMethodName(), message), Logfiles.Application, LogLevel.Error, args);
                    break;

                case LogOptions.None:
                    WriteToLog(message, Logfiles.Application, LogLevel.Error, args);
                    break;
            }
        }

        /// <summary>
        /// Writes an exception level message to the application log
        /// </summary>
        /// <param name="message">The message</param>
        public static void Exception(string message)
        {
            WriteToLog(message, Logfiles.Application, LogLevel.Error);
        }

        /// <summary>
        /// Writes an exception level message to the application log
        /// </summary>
        /// <param name="message">The formatted string to be passed into the string.Format() method</param>
        /// <param name="args">The arguments to be passed into the string.Format() method</param>
        public static void Exception(string message, params object[] args)
        {
            WriteToLog(message, Logfiles.Application, LogLevel.Exception, args);
        }

        /// <summary>
        /// Writes a message to the Installer log
        /// </summary>
        /// <param name="message">The message</param>
        public static void Installer(string message)
        {
            InstallLogfile.Write(message);
        }

        /// <summary>
        /// Writes a message to the Installer log
        /// </summary>
        /// <param name="message">The formatted string to be passed into the string.Format() method</param>
        /// <param name="args">The arguments to be passed into the string.Format() method</param>
        public static void Installer(string message, params object[] args)
        {
            InstallLogfile.Write(string.Format(message, args));
        }

        /// <summary>
        /// Writes a message to the Uninstaller log
        /// </summary>
        /// <param name="message">The message</param>
        public static void Uninstaller(string message)
        {
            InstallLogfile.Write(message);
        }

        /// <summary>
        /// Writes a message to the Uninstaller log
        /// </summary>
        /// <param name="message">The formatted string to be passed into the string.Format() method</param>
        /// <param name="args">The arguments to be passed into the string.Format() method</param>
        public static void Uninstaller(string message, params object[] args)
        {
            InstallLogfile.Write(string.Format(message, args));
        }

        /// <summary>
        /// Writes a message to the Editor logfile
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="level">The level of severity included into the string format</param>
        public static void Editor(string message, LogLevel level = LogLevel.Info)
        {
            WriteToLog(message, Logfiles.Editor, level);
        }

        /// <summary>
        /// Writes a message to the Editor logfile
        /// </summary>
        /// <param name="message">The formatted string to be passed into the string.Format() method</param>
        /// <param name="level">The level of severity included into the string format</param>
        /// <param name="args">The arguments to be passed into the string.Format() method</param>
        public static void Editor(string message, LogLevel level = LogLevel.Info, params object[] args)
        {
            WriteToLog(message, Logfiles.Editor, level, args);
        }

        /// <summary>
        /// Writes a message to the Updater logfile
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="level">The level of severity included into the string format</param>
        public static void Updater(string message, LogLevel level = LogLevel.Info)
        {
            WriteToLog(message, Logfiles.Updater, level);
        }

        /// <summary>
        /// Writes a message to the Updater logfile
        /// </summary>
        /// <param name="message">The formatted string to be passed into the string.Format() method</param>
        /// <param name="level">The level of severity included into the string format</param>
        /// <param name="args">The arguments to be passed into the string.Format() method</param>
        public static void Updater(string message, LogLevel level = LogLevel.Info, params object[] args)
        {
            WriteToLog(message, Logfiles.Updater, level, args);
        }

        /// <summary>
        /// Writes a message to the Patcher logfile
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="level">The level of severity included into the string format</param>
        public static void Patcher(string message, LogLevel level = LogLevel.Info)
        {
            WriteToLog(message, Logfiles.PatchDesigner, level);
        }

        /// <summary>
        /// Writes a message to the Patcher logfile
        /// </summary>
        /// <param name="message">The formatted string to be passed into the string.Format() method</param>
        /// <param name="level">The level of severity included into the string format</param>
        /// <param name="args">The arguments to be passed into the string.Format() method</param>
        public static void Patcher(string message, LogLevel level = LogLevel.Info, params object[] args)
        {
            WriteToLog(message, Logfiles.PatchDesigner, level, args);
        }
    }
}
