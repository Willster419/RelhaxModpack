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
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        bool exceptionShown = false;
        ExceptionCaptureDisplay exceptionCaptureDisplay = new ExceptionCaptureDisplay();

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
            if (!Logging.Init(Logfiles.Application))
            {
                //check if it's because the file already exists, or some other actual reason
                //if the file exists, and it's locked (init failed), then check if also the command line is creating a new window
                if (File.Exists(Logging.GetLogfile(Logfiles.Application).Filepath) && CommandLineSettings.ArgsOpenCustomWindow)
                {
                    //a custom window will be open, so open the log in a custom name (with the application mode). it's ok in this case, because the likely cause
                    //is that the modpack is open in a regular window and now we want to open a custom window
                    Logging.DisposeLogging(Logfiles.Application);
                    if (!Logging.Init(Logfiles.Application, Logging.ApplicationTempLogFilename))
                    {
                        MessageBox.Show(string.Format("Failed to initialize logfile {0}, check file permissions", Logging.ApplicationTempLogFilename));
                        Shutdown((int)ReturnCodes.LogfileError);
                        return;
                    }
                }
                else
                {
                    if (!Translations.TranslationsLoaded)
                        Translations.LoadTranslations(false);
                    //old message from Logging.Init
                    //MessageBox.Show(string.Format("Failed to initialize logfile {0}, check file permissions", logfilePath));
                    MessageBox.Show(Translations.GetTranslatedString("appFailedCreateLogfile"));
                    Shutdown((int)ReturnCodes.LogfileError);
                    return;
                }
            }

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

            Logging.WriteHeader(Logfiles.Application);
            Logging.Info(string.Format("| Relhax Modpack version {0}", CommonUtils.GetApplicationVersion()));
            Logging.Info(string.Format("| Build version {0}, from date {1}", Settings.ApplicationVersion.ToString(), CommonUtils.GetCompileTime()));
            Logging.Info(string.Format("| Running on OS {0}", Environment.OSVersion.ToString()));

            //parse command line arguments given to the application
            CommandLineSettings.ParseCommandLine(Environment.GetCommandLineArgs());

            if ((!CommandLineSettings.ArgsOpenCustomWindow) && (!ModpackSettings.ValidFrameworkVersion))
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

            //switch into application modes based on mode enum
            Logging.Debug("Starting application in {0} mode", CommandLineSettings.ApplicationMode.ToString());
            switch (CommandLineSettings.ApplicationMode)
            {
                case ApplicationMode.Updater:
                    ModpackToolbox updater = new ModpackToolbox();

                    //close application log if open
                    if(Logging.IsLogOpen(Logfiles.Application))
                        CloseApplicationLog(true);

                    //start updater logging
                    if (!Logging.Init(Logfiles.Updater))
                    {
                        MessageBox.Show("Failed to initialize logfile for updater");
                        Current.Shutdown((int)ReturnCodes.LogfileError);
                        return;
                    }
                    Logging.WriteHeader(Logfiles.Updater);

                    //redirect application log file to the modpack toolbox
                    if (!Logging.RedirectLogOutput(Logfiles.Application, Logfiles.Updater))
                        Logging.Error(Logfiles.Updater, LogOptions.MethodName, "Failed to redirect messages from application to modpack toolbox");

                    //show window
                    updater.ShowDialog();

                    //stop updater logging
                    CloseLog(Logfiles.Updater);
                    DeleteCustomLogIfExists();
                    updater = null;
                    Current.Shutdown((int)ReturnCodes.Success);
                    Environment.Exit((int)ReturnCodes.Success);
                    return;
                case ApplicationMode.Editor:
                    DatabaseEditor editor = new DatabaseEditor();

                    //close application log if open
                    if (Logging.IsLogOpen(Logfiles.Application))
                        CloseApplicationLog(true);

                    //start updater logging
                    if (!Logging.Init(Logfiles.Editor))
                    {
                        MessageBox.Show("Failed to initialize logfile for editor");
                        Current.Shutdown((int)ReturnCodes.LogfileError);
                        return;
                    }
                    Logging.WriteHeader(Logfiles.Editor);

                    //redirect application log file to the editor
                    if (!Logging.RedirectLogOutput(Logfiles.Application, Logfiles.Editor))
                        Logging.Error(Logfiles.Editor, LogOptions.MethodName, "Failed to redirect messages from application to editor");

                    //show window
                    editor.ShowDialog();

                    //stop updater logging
                    CloseLog(Logfiles.Editor);
                    DeleteCustomLogIfExists();
                    editor = null;
                    Current.Shutdown((int)ReturnCodes.Success);
                    Environment.Exit((int)ReturnCodes.Success);
                    return;
                case ApplicationMode.PatchDesigner:
                    PatchDesigner patcher = new PatchDesigner();

                    //close application log if open
                    if (Logging.IsLogOpen(Logfiles.Application))
                        CloseApplicationLog(true);

                    //start updater logging
                    if (!Logging.Init(Logfiles.PatchDesigner))
                    {
                        MessageBox.Show("Failed to initialize logfile for patcher");
                        Current.Shutdown((int)ReturnCodes.LogfileError);
                        return;
                    }
                    Logging.WriteHeader(Logfiles.PatchDesigner);

                    //redirect application log file to the patch designer
                    if (!Logging.RedirectLogOutput(Logfiles.Application, Logfiles.PatchDesigner))
                        Logging.Error(Logfiles.PatchDesigner, LogOptions.MethodName, "Failed to redirect messages from application to patch designer");

                    //show window
                    patcher.ShowDialog();

                    //stop patch designer logging
                    CloseLog(Logfiles.PatchDesigner);
                    DeleteCustomLogIfExists();
                    patcher = null;
                    Current.Shutdown((int)ReturnCodes.Success);
                    Environment.Exit((int)ReturnCodes.Success);
                    return;
                case ApplicationMode.AutomationRunner:
                    DatabaseAutomationRunner automationRunner = new DatabaseAutomationRunner();

                    //close application log if open
                    if (Logging.IsLogOpen(Logfiles.Application))
                        CloseApplicationLog(true);

                    //start DatabaseAutomationRunner logging
                    if (!Logging.Init(Logfiles.AutomationRunner))
                    {
                        MessageBox.Show("Failed to initialize logfile for DatabaseAutomationRunner");
                        Current.Shutdown((int)ReturnCodes.LogfileError);
                        return;
                    }
                    Logging.WriteHeader(Logfiles.AutomationRunner);

                    //redirect application log file to the automation runner
                    if (!Logging.RedirectLogOutput(Logfiles.Application, Logfiles.AutomationRunner))
                        Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, "Failed to redirect messages from application to automation runner");

                    //show window
                    automationRunner.ShowDialog();

                    //stop patch designer logging
                    CloseLog(Logfiles.AutomationRunner);
                    DeleteCustomLogIfExists();
                    automationRunner = null;
                    Current.Shutdown((int)ReturnCodes.Success);
                    Environment.Exit((int)ReturnCodes.Success);
                    return;
                case ApplicationMode.Patcher:
                    Logging.Info("Running patch mode");

                    //check that at least one patch file was specified from command line
                    if(CommandLineSettings.PatchFilenames.Count == 0)
                    {
                        Logging.Error("0 patch files parsed from command line!");
                        Current.Shutdown((int)ReturnCodes.PatcherNoSpecifiedFiles);
                        Environment.Exit((int)ReturnCodes.PatcherNoSpecifiedFiles);
                    }
                    else
                    {
                        //parse patch objects from command line file list
                        List<Patch> patchList = new List<Patch>();
                        foreach(string file in CommandLineSettings.PatchFilenames)
                        {
                            if(!File.Exists(file))
                            {
                                Logging.Warning("Skipping file path {0}, not found", file);
                                continue;
                            }
                            Logging.Info("Adding patches from file {0}", file);
                            XmlUtils.AddPatchesFromFile(patchList, file);
                        }

                        //check for at least one patchfile was parsed
                        if(patchList.Count == 0)
                        {
                            Logging.Error("0 patches parsed from files!");
                            Current.Shutdown((int)ReturnCodes.PatcherNoPatchesParsed);
                            Environment.Exit((int)ReturnCodes.PatcherNoPatchesParsed);
                        }

                        //set default patch return code
                        PatchExitCode exitCode = PatchExitCode.Success;

                        //always return on worst condition
                        int i = 1;
                        foreach(Patch p in patchList)
                        {
                            Logging.Info("Running patch {0} of {1}", i++, patchList.Count);
                            PatchExitCode exitCodeTemp = PatchUtils.RunPatch(p);
                            if ((int)exitCodeTemp < (int)exitCode)
                                exitCode = exitCodeTemp;
                        }

                        Logging.Info("Patching finished, exit code {0} ({1})", (int)exitCode, exitCode.ToString());
                        CloseApplicationLog(true);
                        //add 100 to ensure it's a patch unique error code
                        Current.Shutdown(((int)exitCode) + 100);
                        Environment.Exit(((int)exitCode) + 100);
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

        private void DeleteCustomLogIfExists()
        {
            if (File.Exists(Logging.ApplicationTempLogFilename))
                File.Delete(Logging.ApplicationTempLogFilename);
        }
    }
}
