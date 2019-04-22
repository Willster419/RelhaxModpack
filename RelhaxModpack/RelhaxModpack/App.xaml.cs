using System;
using System.Windows;
using RelhaxModpack.Windows;

namespace RelhaxModpack
{
    /// <summary>
    /// Application return error codes
    /// </summary>
    public enum ReturnCodes
    {
        /// <summary>
        /// No error occured
        /// </summary>
        Sucess = 0,
        /// <summary>
        /// Error with logfile creation
        /// </summary>
        LogfileError = 1
    }
    /// <summary>
    /// The build distribution version of the application
    /// </summary>
    public enum ApplicationVersions
    {
        /// <summary>
        /// The stable distribution for all users
        /// </summary>
        Stable,
        /// <summary>
        /// The beta distrubution, for advanced users, may have new features or improvements, and bugs
        /// </summary>
        Beta,
        /// <summary>
        /// The alpha distribution. Should never be publicly distributed
        /// </summary>
        Alpha
    }
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //when application is brought to forground
        private void Application_Activated(object sender, EventArgs e)
        {

        }

        //when application is sent to background (not active window)?
        private void Application_Deactivated(object sender, EventArgs e)
        {

        }

        //when application is closing (cannot be stopped)
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            CloseApplicationLog(true);
        }

        private void CloseApplicationLog(bool showCloseMessage)
        {
            if (!Logging.IsLogDisposed(Logfiles.Application))
            {
                if (showCloseMessage)
                    Logging.WriteToLog("Application closing");
                Logging.WriteToLog(Logging.ApplicationlogStartStop);
                Logging.DisposeLogging(Logfiles.Application);
            }
        }

        //when application is starting for first time
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //init loggine here
            //"The application failed to open a logfile. Eithor check your file permissions or move the application to a folder with write access"
            if (!Logging.InitApplicationLogging(Logfiles.Application, Logging.ApplicationLogFilename))
            {
                MessageBox.Show(Translations.GetTranslatedString("appFailedCreateLogfile"));
                Shutdown((int)ReturnCodes.LogfileError);
            }
            Logging.WriteToLog(Logging.ApplicationlogStartStop);
            Logging.WriteToLog(string.Format("| Relhax Modpack version {0}", Utils.GetApplicationVersion()));
            Logging.WriteToLog(string.Format("| Build version {0}, from date {1}", Settings.ApplicationVersion.ToString(), Utils.GetCompileTime()));
            Logging.WriteToLog(string.Format("| Running on OS {0}", Environment.OSVersion.ToString()));
            //parse command line arguements here
            //get the command line args for testing of auto install
            CommandLineSettings.ParseCommandLine(Environment.GetCommandLineArgs());
            Logging.Info("starting application in {0} mode", CommandLineSettings.ApplicationMode.ToString());
            //switch into application modes based on mode enum
            switch(CommandLineSettings.ApplicationMode)
            {
                case ApplicationMode.Updater:
                    DatabaseUpdater updater = new DatabaseUpdater();
                    //stop application logging system
                    CloseApplicationLog(false);
                    //start updater logging system
                    if (!Logging.InitApplicationLogging(Logfiles.Application, Logging.ApplicationUpdaterLogFilename))
                    {
                        MessageBox.Show("Failed to initialize logfile (Do you have multiple windows open?");
                        Current.Shutdown((int)ReturnCodes.LogfileError);
                        return;
                    }
                    Logging.WriteToLog(Logging.ApplicationlogStartStop);
                    updater.ShowDialog();
                    //stop updater logging system
                    CloseApplicationLog(false);
                    updater = null;
                    Current.Shutdown(0);
                    break;
                case ApplicationMode.Editor:
                    DatabaseEditor editor = new DatabaseEditor();
                    //stop application logging system
                    CloseApplicationLog(false);
                    //start updater logging system
                    if (!Logging.InitApplicationLogging(Logfiles.Application, Logging.ApplicationEditorLogFilename))
                    {
                        MessageBox.Show("Failed to initialize logfile (Do you have multiple windows open?");
                        Current.Shutdown((int)ReturnCodes.LogfileError);
                        return;
                    }
                    Logging.WriteToLog(Logging.ApplicationlogStartStop);
                    editor.ShowDialog();
                    //stop updater logging system
                    CloseApplicationLog(false);
                    editor = null;
                    Current.Shutdown(0);
                    break;
                case ApplicationMode.PatchDesigner:
                    PatchTester patcher = new PatchTester();
                    //stop application logging system
                    CloseApplicationLog(false);
                    //start updater logging system
                    if (!Logging.InitApplicationLogging(Logfiles.Application, Logging.ApplicationPatchDesignerLogFilename))
                    {
                        MessageBox.Show("Failed to initialize logfile (Do you have multiple windows open?");
                        Current.Shutdown((int)ReturnCodes.LogfileError);
                        return;
                    }
                    Logging.WriteToLog(Logging.ApplicationlogStartStop);
                    patcher.ShowDialog();
                    //stop updater logging system
                    CloseApplicationLog(false);
                    patcher = null;
                    Current.Shutdown(0);
                    break;
                case ApplicationMode.Patcher:
                    Logging.Info("Running patch: {0}", CommandLineSettings.PatchFilename);
                    throw new BadMemeException("TODO");
                    //shutdown based on patch return type
                    break;
            }
        }

        //https://stackoverflow.com/questions/793100/globally-catch-exceptions-in-a-wpf-application
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (!Logging.IsLogDisposed(Logfiles.Application) && Logging.IsLogOpen(Logfiles.Application))
                Logging.WriteToLog(e.Exception.ToString(), Logfiles.Application, LogLevel.ApplicationHalt);
            MessageBox.Show(e.Exception.ToString());
        }
    }
}
