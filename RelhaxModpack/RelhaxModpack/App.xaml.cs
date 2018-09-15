using System;
using System.Linq;
using System.Windows;

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
            Logging.WriteToLog("Application closing");
            Logging.WriteToLog(Logging.ApplicationlogStartStop);
            Logging.DisposeApplicationLogging();
        }

        //when application is starting for first time
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //init loggine here
            if (!Logging.InitApplicationLogging())
            {
                MessageBox.Show("The application failed to open a logfile. Eithor check your file permissions or move the application to a folder with write access");
                Shutdown((int)ReturnCodes.LogfileError);
            }
            Logging.WriteToLog(Logging.ApplicationlogStartStop);
            Logging.WriteToLog(string.Format("| Relhax Modpack version {0}",Utils.GetApplicationVersion()));
            Logging.WriteToLog(string.Format("| Build version {0}, from date {1}", Settings.ApplicationVersion.ToString(), Utils.GetCompileTime()));
            Logging.WriteToLog(string.Format("| Running on OS {0}", Environment.OSVersion.ToString()));
            //parse command line arguements here
            //get the command line args for testing of auto install
            string[] commandArgs = Environment.GetCommandLineArgs();
            Logging.WriteToLog("command line: " + string.Join(" ", commandArgs));
            for (int i = 0; i < commandArgs.Count(); i++)
            {
                switch(commandArgs[i])
                {
                    case "test":
                        Logging.WriteToLog("test, loading in test mode");
                        CommandLineSettings.TestMode = true;
                        break;
                    case "skip-update":
                        Logging.WriteToLog("skip-update, skipping updating");
                        CommandLineSettings.SkipUpdate = true;
                        break;
                    case "silent-start":
                        Logging.WriteToLog("silent-start, loading in background");
                        CommandLineSettings.SilentStart = true;
                        break;
                    case "auto-install":
                        CommandLineSettings.AutoInstallFileName = commandArgs[++i];
                        Logging.WriteToLog("auto-install, attempting to parse user configuration file: " + CommandLineSettings.AutoInstallFileName);
                        break;
                    case "updateKeyFile":
                        //get key file
                        CommandLineSettings.UpdateKeyFileName = commandArgs[++i];
                        Logging.WriteToLog("updateKeyFile, loading keyfile " + CommandLineSettings.UpdateKeyFileName);
                        break;
                    case "editorAutoLoad":
                        CommandLineSettings.EditorAutoLoadFileName = commandArgs[++i];
                        Logging.WriteToLog("editorAutoLoad, loading databse from " + CommandLineSettings.EditorAutoLoadFileName);
                        break;
                    case "forceVisible":
                        CommandLineSettings.ForceVisible = true;
                        Logging.WriteToLog("forceVisible, loading all invisible mods in selection list");
                        break;
                    case "forceEnabled":
                        CommandLineSettings.ForceEnabled = true;
                        Logging.WriteToLog("forceEnabled, loading all visible mods as enabled");
                        break;
                    case "patchcheck":
                        CommandLineSettings.PatchCheck = true;
                        Logging.WriteToLog("patchcheck, loading in patch design mode");
                        break;
                    case "databaseupdate":
                        CommandLineSettings.DatabaseUpdate = true;
                        Logging.WriteToLog("databaseupdate, loading in database update mode");
                        break;
                    case "databaseedit":
                        CommandLineSettings.DatabaseEdit = true;
                        Logging.WriteToLog("databaseedit, loading in database edit mode");
                        break;
                }
            }
        }
    }
}
