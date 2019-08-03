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
        /// No error occurred
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
        /// The beta distribution, for advanced users, may have new features or improvements, and bugs
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
        //when application is brought to foreground
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

        private void CloseLog(Logfiles logfile)
        {
            if (!Logging.IsLogDisposed(logfile))
                Logging.DisposeLogging(logfile);
        }

        private void CloseApplicationLog(bool showCloseMessage)
        {
            if (!Logging.IsLogDisposed(Logfiles.Application))
            {
                if (showCloseMessage && Logging.IsLogOpen(Logfiles.Application))
                {
                    Logging.Info("Application closing");
                    Logging.WriteHeader(Logfiles.Application);
                }
                Logging.DisposeLogging(Logfiles.Application);
            }
        }

        //when application is starting for first time
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //handle any assembly resolves
            //https://stackoverflow.com/a/19806004/3128017
            AppDomain.CurrentDomain.AssemblyResolve += (sender2, bargs) =>
            {
                string dllName = new AssemblyName(bargs.Name).Name + ".dll";
                Assembly assem = Assembly.GetExecutingAssembly();
                string resourceName = assem.GetManifestResourceNames().FirstOrDefault(rn => rn.EndsWith(dllName));
                using (Stream stream = assem.GetManifestResourceStream(resourceName))
                {
                    byte[] assemblyData = new byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    Logging.Debug("an assembly was loaded via AssemblyResolve: {0}", dllName);
                    return Assembly.Load(assemblyData);
                }
            };
            //init logging here
            //"The application failed to open a logfile. Either check your file permissions or move the application to a folder with write access"
            if (!Logging.Init(Logfiles.Application))
            {
                MessageBox.Show(Translations.GetTranslatedString("appFailedCreateLogfile"));
                Shutdown((int)ReturnCodes.LogfileError);
            }
            Logging.WriteHeader(Logfiles.Application);
            Logging.Info(string.Format("| Relhax Modpack version {0}", Utils.GetApplicationVersion()));
            Logging.Info(string.Format("| Build version {0}, from date {1}", Settings.ApplicationVersion.ToString(), Utils.GetCompileTime()));
            Logging.Info(string.Format("| Running on OS {0}", Environment.OSVersion.ToString()));
            //parse command line arguments here
            //get the command line args for testing of auto install
            CommandLineSettings.ParseCommandLine(Environment.GetCommandLineArgs());
            Logging.Debug("starting application in {0} mode", CommandLineSettings.ApplicationMode.ToString());
            //switch into application modes based on mode enum
            switch(CommandLineSettings.ApplicationMode)
            {
                case ApplicationMode.Updater:
                    DatabaseUpdater updater = new DatabaseUpdater();
                    CloseApplicationLog(true);

                    //start updater logging system
                    if (!Logging.Init(Logfiles.Updater))
                    {
                        MessageBox.Show("Failed to initialize logfile for updater");
                        Current.Shutdown((int)ReturnCodes.LogfileError);
                        return;
                    }
                    Logging.WriteHeader(Logfiles.Updater);
                    updater.ShowDialog();

                    //stop updater logging system
                    CloseLog(Logfiles.Updater);
                    updater = null;
                    Current.Shutdown(0);
                    break;
                case ApplicationMode.Editor:
                    DatabaseEditor editor = new DatabaseEditor();
                    CloseApplicationLog(true);

                    //start updater logging system
                    if (!Logging.Init(Logfiles.Editor))
                    {
                        MessageBox.Show("Failed to initialize logfile for editor");
                        Current.Shutdown((int)ReturnCodes.LogfileError);
                        return;
                    }
                    Logging.WriteHeader(Logfiles.Editor);
                    editor.ShowDialog();
                    //stop updater logging system
                    CloseLog(Logfiles.Editor);
                    editor = null;
                    Current.Shutdown(0);
                    break;
                case ApplicationMode.PatchDesigner:
                    PatchTester patcher = new PatchTester();
                    CloseApplicationLog(true);

                    //start updater logging system
                    if (!Logging.Init(Logfiles.Patcher))
                    {
                        MessageBox.Show("Failed to initialize logfile for patcher");
                        Current.Shutdown((int)ReturnCodes.LogfileError);
                        return;
                    }
                    Logging.WriteHeader(Logfiles.Patcher);
                    patcher.ShowDialog();

                    //stop updater logging system
                    CloseLog(Logfiles.Patcher);
                    patcher = null;
                    Current.Shutdown(0);
                    break;
                case ApplicationMode.Patcher:
                    Logging.Info("Running patch mode");
                    if(CommandLineSettings.PatchFilenames.Count == 0)
                    {
                        Logging.Error("0 patch files parsed from command line!");
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
