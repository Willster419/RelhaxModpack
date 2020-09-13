using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Microsoft.Win32;
using RelhaxModpack.Patches;
using RelhaxModpack.Properties;
using RelhaxModpack.Utilities;
using RelhaxModpack.Windows;
using RelhaxModpack.Xml;
using RelhaxModpack.Utilities.Enums;

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
        bool exceptionShown = false;
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
            if (Logging.IsLogOpen(Logfiles.Application))
                Logging.Info("Disposing log");
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
                    Logging.Debug("An assembly was loaded via AssemblyResolve: {0}", dllName);
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
            Logging.Info(string.Format("| Relhax Modpack version {0}", CommonUtils.GetApplicationVersion()));
            Logging.Info(string.Format("| Build version {0}, from date {1}", Settings.ApplicationVersion.ToString(), CommonUtils.GetCompileTime()));
            Logging.Info(string.Format("| Running on OS {0}", Environment.OSVersion.ToString()));

            //parse command line arguments here
            //get the command line args for testing of auto install
            CommandLineSettings.ParseCommandLine(Environment.GetCommandLineArgs());

            if (!ModpackSettings.ValidFrameworkVersion)
            {
                //https://github.com/Willster419/RelhaxModpack/issues/90
                //try getting .net framework information
                //https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed
                //https://docs.microsoft.com/en-us/dotnet/api/system.environment.version?view=netcore-3.1
                //https://stackoverflow.com/questions/19096841/how-to-get-the-version-of-the-net-framework-being-targeted
                Logging.Debug(".NET Framework version information");
                int frameworkVersion = -1;
                try
                {
                    RegistryKey key = RegistryUtils.GetRegistryKeys(new RegistrySearch() { Root = Registry.LocalMachine, Searchpath = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" });
                    Logging.Debug("Registry: {0}", key.Name);
                    foreach (string subkey in key.GetValueNames())
                    {
                        object value = key.GetValue(subkey);
                        Logging.Debug("Registry: Subkey={0}, Value={1}", subkey, value.ToString());
                        if (subkey.ToLower().Equals("release"))
                        {
                            if (int.TryParse(value.ToString(), out int result))
                                frameworkVersion = result;
                            else
                                Logging.Error("Unable to parse release value: {0}", value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex.ToString());
                }

                Logging.Info("Minimum required .NET Framework version: {0}, Installed: {1}", Settings.MinimumDotNetFrameworkVersionRequired, frameworkVersion);

                if (frameworkVersion == -1)
                {
                    Logging.Error("Failed to get .NET Framework version from the registry");
                    MessageBox.Show("failedToGetDotNetFrameworkVersion");
                }
                else if (frameworkVersion < Settings.MinimumDotNetFrameworkVersionRequired)
                {
                    Logging.Error("Invalid .NET Framework version (less then 4.8)");
                    if (MessageBox.Show("invalidDotNetFrameworkVersion","",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        CommonUtils.StartProcess(Settings.DotNetFrameworkLatestDownloadURL);
                    }
                }
                else
                {
                    Logging.Info("Valid .NET Framework version");
                    ModpackSettings.ValidFrameworkVersion = true;
                }
            }

            Logging.Debug("Starting application in {0} mode", CommandLineSettings.ApplicationMode.ToString());
            //switch into application modes based on mode enum
            switch(CommandLineSettings.ApplicationMode)
            {
                case ApplicationMode.Updater:
                    ModpackToolbox updater = new ModpackToolbox();
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
                    Environment.Exit(0);
                    return;
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
                    Environment.Exit(0);
                    return;
                case ApplicationMode.PatchDesigner:
                    PatchDesigner patcher = new PatchDesigner();
                    CloseApplicationLog(true);

                    //start updater logging system
                    if (!Logging.Init(Logfiles.PatchDesigner))
                    {
                        MessageBox.Show("Failed to initialize logfile for patcher");
                        Current.Shutdown((int)ReturnCodes.LogfileError);
                        return;
                    }
                    Logging.WriteHeader(Logfiles.PatchDesigner);
                    patcher.ShowDialog();

                    //stop updater logging system
                    CloseLog(Logfiles.PatchDesigner);
                    patcher = null;
                    Current.Shutdown(0);
                    Environment.Exit(0);
                    return;
                case ApplicationMode.Patcher:
                    Logging.Info("Running patch mode");
                    if(CommandLineSettings.PatchFilenames.Count == 0)
                    {
                        Logging.Error("0 patch files parsed from command line!");
                        Current.Shutdown(-3);
                        Environment.Exit(-3);
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
                            XmlUtils.AddPatchesFromFile(patchList, file);
                        }
                        if(patchList.Count == 0)
                        {
                            Logging.Error("0 patches parsed from files!");
                            Current.Shutdown(-4);
                            Environment.Exit(-4);
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
                        Environment.Exit((int)exitCode);
                    }
                    return;
            }
        }

        //https://stackoverflow.com/questions/793100/globally-catch-exceptions-in-a-wpf-application
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (!exceptionShown)
            {
                exceptionShown = true;
                if (!Logging.IsLogDisposed(Logfiles.Application) && Logging.IsLogOpen(Logfiles.Application))
                {
                    Logging.WriteToLog(e.Exception.ToString(), Logfiles.Application, LogLevel.ApplicationHalt);
                }
                exceptionCaptureDisplay.ExceptionText = e.Exception.ToString();
                exceptionCaptureDisplay.ShowDialog();
                CloseApplicationLog(true);
            }
        }
    }
}
