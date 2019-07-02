using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        ExceptionCaptureDisplay exceptionCaptureDisplay = new ExceptionCaptureDisplay();
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
                {
                    Logging.WriteToLog("Application closing");
                    Logging.WriteToLog(Logging.ApplicationlogStartStop);
                }
                Logging.DisposeLogging(Logfiles.Application);
            }
        }

        private string TransferLogInfo(string filename)
        {
            if (!File.Exists(filename))
                return Logging.ApplicationlogStartStop;
            string textToReturn = File.ReadAllText(filename);
            File.Delete(filename);
            return textToReturn.Trim();
        }

        //when application is starting for first time
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //handle any assembly resolves
            //https://stackoverflow.com/a/19806004/3128017
            AppDomain.CurrentDomain.AssemblyResolve += (sender2, bargs) =>
            {
                string dllName = new AssemblyName(bargs.Name).Name + ".dll";
                Logging.Debug("an assembly was loaded via AssemblyResolve: {0}", dllName);
                Assembly assem = Assembly.GetExecutingAssembly();
                string resourceName = assem.GetManifestResourceNames().FirstOrDefault(rn => rn.EndsWith(dllName));
                if (resourceName == null) return null; // Not found, maybe another handler will find it
                using (var stream = assem.GetManifestResourceStream(resourceName))
                {
                    byte[] assemblyData = new byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };
            //init loggine here
            //"The application failed to open a logfile. Eithor check your file permissions or move the application to a folder with write access"
            if (!Logging.Init(Logfiles.Application, Logging.ApplicationLogTempFilename))
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
            Logging.Debug("starting application in {0} mode", CommandLineSettings.ApplicationMode.ToString());
            //switch into application modes based on mode enum
            switch(CommandLineSettings.ApplicationMode)
            {
                case ApplicationMode.Updater:
                    DatabaseUpdater updater = new DatabaseUpdater();
                    //stop application logging system
                    CloseApplicationLog(false);
                    //start updater logging system
                    if (!Logging.Init(Logfiles.Application, Logging.ApplicationUpdaterLogFilename))
                    {
                        MessageBox.Show("Failed to initialize logfile (Do you have multiple windows open?");
                        Current.Shutdown((int)ReturnCodes.LogfileError);
                        return;
                    }
                    Logging.WriteHeader(TransferLogInfo(Logging.ApplicationLogTempFilename));
                    updater.ShowDialog();
                    //stop updater logging system
                    CloseApplicationLog(true);
                    updater = null;
                    Current.Shutdown(0);
                    break;
                case ApplicationMode.Editor:
                    DatabaseEditor editor = new DatabaseEditor();
                    //stop application logging system
                    CloseApplicationLog(false);
                    //start updater logging system
                    if (!Logging.Init(Logfiles.Application, Logging.ApplicationEditorLogFilename))
                    {
                        MessageBox.Show("Failed to initialize logfile (Do you have multiple windows open?");
                        Current.Shutdown((int)ReturnCodes.LogfileError);
                        return;
                    }
                    Logging.WriteHeader(TransferLogInfo(Logging.ApplicationLogTempFilename));
                    editor.ShowDialog();
                    //stop updater logging system
                    CloseApplicationLog(true);
                    editor = null;
                    Current.Shutdown(0);
                    break;
                case ApplicationMode.PatchDesigner:
                    PatchTester patcher = new PatchTester();
                    //stop application logging system
                    CloseApplicationLog(false);
                    //start updater logging system
                    if (!Logging.Init(Logfiles.Application, Logging.ApplicationPatchDesignerLogFilename))
                    {
                        MessageBox.Show("Failed to initialize logfile (Do you have multiple windows open?");
                        Current.Shutdown((int)ReturnCodes.LogfileError);
                        return;
                    }
                    Logging.WriteHeader(TransferLogInfo(Logging.ApplicationLogTempFilename));
                    patcher.ShowDialog();
                    //stop updater logging system
                    CloseApplicationLog(true);
                    patcher = null;
                    Current.Shutdown(0);
                    break;
                case ApplicationMode.Patcher:
                    CloseApplicationLog(false);
                    Logging.Init(Logfiles.Application, Logging.ApplicationLogFilename);
                    Logging.WriteHeader(TransferLogInfo(Logging.ApplicationLogTempFilename));
                    Logging.Info("Running patch mode");
                    if(CommandLineSettings.PatchFilenames.Count == 0)
                    {
                        Logging.Error("0 patchfiles parsed from commandline!");
                        Current.Shutdown(-3);
                    }
                    else
                    {
                        List<Patch> patchList = new List<Patch>();
                        foreach(string file in CommandLineSettings.PatchFilenames)
                        {
                            if(!File.Exists(file))
                            {
                                Logging.Warning("skipping file path {0}, not found", file);
                                continue;
                            }
                            Logging.Info("adding patches from file {0}", file);
                            XMLUtils.AddPatchesFromFile(patchList, file);
                        }
                        if(patchList.Count == 0)
                        {
                            Logging.Error("0 patches parsed from files!");
                            Current.Shutdown(-4);
                        }
                        PatchExitCode exitCode = PatchExitCode.Success;
                        //always return on worst condition
                        int i = 1;
                        foreach(Patch p in patchList)
                        {
                            Logging.Info("running patch {0} of {1}", i++, patchList.Count);
                            PatchExitCode exitCodeTemp = PatchUtils.RunPatch(p);
                            if ((int)exitCodeTemp < (int)exitCode)
                                exitCode = exitCodeTemp;
                        }
                        Logging.Info("patching finished, exit code {0} ({1})", (int)exitCode, exitCode.ToString());
                        CloseApplicationLog(true);
                        Current.Shutdown((int)exitCode);
                    }
                    break;
                default:
                    CloseApplicationLog(false);
                    if (!Logging.Init(Logfiles.Application, Logging.ApplicationLogFilename))
                    {
                        MessageBox.Show("Failed to initialize logfile (Do you have multiple windows open?");
                        Current.Shutdown((int)ReturnCodes.LogfileError);
                        return;
                    }
                    Logging.WriteHeader(TransferLogInfo(Logging.ApplicationLogTempFilename));
                    break;
            }
        }

        //https://stackoverflow.com/questions/793100/globally-catch-exceptions-in-a-wpf-application
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (!Logging.IsLogDisposed(Logfiles.Application) && Logging.IsLogOpen(Logfiles.Application))
                Logging.WriteToLog(e.Exception.ToString(), Logfiles.Application, LogLevel.ApplicationHalt);
            exceptionCaptureDisplay.ExceptionText = e.Exception.ToString();
            exceptionCaptureDisplay.ShowDialog();
            CloseApplicationLog(true);
        }
    }
}
