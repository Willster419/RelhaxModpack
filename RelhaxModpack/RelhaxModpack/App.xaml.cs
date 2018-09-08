using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using RelhaxModpack.Logging;

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
            Logging.Logging.WriteToLog("Application closing");
            Logging.Logging.WriteToLog(Logging.Logging.ApplicationlogStartStop);
            Logging.Logging.DisposeApplicationLogging();
        }

        //when application is starting for first time
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //init loggine here
            if (!Logging.Logging.InitApplicationLogging())
            {
                MessageBox.Show("The application failed to open a logfile. Eithor check your file permissions or move the application to a folder with write access");
                Shutdown((int)ReturnCodes.LogfileError);
            }
            Logging.Logging.WriteToLog(Logging.Logging.ApplicationlogStartStop);
            Logging.Logging.WriteToLog(string.Format("| Relhax Modpack version {0}",Utils.Utils.GetApplicationVersion()));
            Logging.Logging.WriteToLog(string.Format("| Build version {0}, from date {1}", Settings.Settings.ApplicationVersion.ToString(), Utils.Utils.GetCompileTime()));
            Logging.Logging.WriteToLog(string.Format("| Running on OS {0}", Environment.OSVersion.ToString()));
            //parse command line arguements here
            //get the command line args for testing of auto install
            string[] commandArgs = Environment.GetCommandLineArgs();
            Logging.Logging.WriteToLog("command line: " + string.Join(" ", commandArgs));
            for (int i = 0; i < commandArgs.Count(); i++)
            {
                switch(commandArgs[i])
                {
                    case "test":
                        Logging.Logging.WriteToLog("loading in test mode");
                        Settings.Settings.DatabaseDistroVersion = Settings.DatabaseVersion.Test;
                        break;
                    case "skip-update":
                        Logging.Logging.WriteToLog("skipping updating");
                        Settings.Settings.SkipUpdate = true;
                        break;
                    case "silent-start":
                        Logging.Logging.WriteToLog("silent-start, loading in background");
                        Settings.Settings.SilentStart = true;
                        break;
                    case "auto-install":
                        Settings.Settings.AutoInstallFileName = commandArgs[++i];
                        Logging.Logging.WriteToLog("auto-install, attempting to parse user configuration file: " + Settings.Settings.AutoInstallFileName);
                        break;
                    case "updateKeyFile":
                        //get key file
                        Settings.Settings.UpdateKeyFileName = commandArgs[++i];
                        Logging.Logging.WriteToLog("updateKeyFile, loading keyfile " + Settings.Settings.UpdateKeyFileName);
                        break;
                    case "editorAutoLoad":
                        Settings.Settings.EditorAutoLoadFileName = commandArgs[++i];
                        Logging.Logging.WriteToLog("editorAutoLoad, loading databse from " + Settings.Settings.EditorAutoLoadFileName);
                        break;
                    case "forceVisible":
                        Settings.Settings.ForceVisible = true;
                        Logging.Logging.WriteToLog("forceVisible, loading all invisible mods in selection list");
                        break;
                    case "forceEnabled":
                        Settings.Settings.ForceEnabled = true;
                        Logging.Logging.WriteToLog("forceEnabled, loading all visible mods as enabled");
                        break;
                    case "patchcheck":
                        Settings.Settings.PatchCheck = true;
                        Logging.Logging.WriteToLog("patchcheck, loading in patch design mode");
                        break;
                    case "databaseupdate":
                        Settings.Settings.DatabaseUpdate = true;
                        Logging.Logging.WriteToLog("databaseupdate, loading in database update mode");
                        break;
                    case "databaseedit":
                        Settings.Settings.DatabaseEdit = true;
                        Logging.Logging.WriteToLog("databaseedit, loading in database edit mode");
                        break;
                }
            }
            //show loading window

        }
    }
}
