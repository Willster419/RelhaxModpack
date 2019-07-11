using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RelhaxModpack
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static bool testMode = false;
        public static bool autoInstall = false;
        public static bool skipUpdate = false;
        public static bool patchDayTest = false;
        public static bool saveSettings = false;
        public static bool databaseUpdateOnline = false;
        public static bool silentStart = false;
        public static bool forceVisible = false;
        public static bool forceEnabled = false;
        public static bool updateFileKey = false;
        public static bool editorAutoLoad = false;
        public static bool betaAppSetFromCommandLine = false;
        public static bool betaDBSetFromCommandLine = false;
        public static string updateKeyFile = "";
        public static string configName = "";
        public static string editorDatabaseFile = "";

        public enum Compressed
        {
            None = 0,
            Yes,
            No
        }

        public enum ProgramVersion
        {
            Stable = 0,
            Beta = 1,
            Alpha = 2
        }
        public static ProgramVersion Version = ProgramVersion.Stable;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Logging.Manager("|------------------------------------------------------------------------------------------------|");
            Logging.Manager("Main Entry point launched");
            
            //loading embeded dlls from the application
            //https://www.codeproject.com/articles/528178/load-dll-from-embedded-resource
            string defaultResourcePath = "RelhaxModpack.Resources.";
            Dictionary<string, Compressed> librariesMultiPlatform = new Dictionary<string, Compressed>()
            {
                { "DotNetZip.dll", Compressed.No },
                { "Newtonsoft.Json.dll", Compressed.No },
                { "NAudio.dll", Compressed.No },
                { "nvtt32.zip", Compressed.Yes },
                { "nvtt64.zip", Compressed.Yes },
                { "FreeImage32.zip", Compressed.Yes },
                { "FreeImage64.zip", Compressed.Yes }
            };
            //load multiplatform
            foreach (var s in librariesMultiPlatform)
            {
                string resourcePath = defaultResourcePath + s.Key;
                try
                {
                    EmbeddedAssembly.Load(resourcePath, s.Key);
                    Logging.Manager("loading managed library: " + s.Key);
                    AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                    continue;
                }
                catch
                {
                    // if EmbeddedAssembly.Load failes, it should be an unmanaged dll
                    try
                    {
                        EmbeddedUnmanagedDll.ExtractEmbeddedDlls(resourcePath, s.Key, s.Value == Compressed.Yes, out string filename);
                        Logging.Manager(string.Format("extracted unmanaged library: {0}{1}", s.Key, s.Value == Compressed.Yes ? " (" + filename + ")" : ""));
                        continue;
                    }
                    catch (Exception ex)
                    {
                        MainWindow.errorCounter++;
                        Logging.Manager(string.Format("failed to handle library: {0} ({1})", s.Key, ex.Message));
                    }
                }
            }

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            // delete RelicCopyUpdate.bat at start (it is only needed at updates, so kill it)
            try
            {
                string updateBatPath = Path.Combine(Application.StartupPath, "RelicCopyUpdate.bat");
                if (System.IO.File.Exists(updateBatPath))
                    System.IO.File.Delete(updateBatPath);
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("Main", "delete RelicCopyUpdate.bat", ex);
            }
            //get the command line args for testing of auto install
            string[] commandArgs = Environment.GetCommandLineArgs();
            //log command line
            Logging.Manager("command line: " + string.Join(" ", commandArgs));
            for (int i = 0; i < commandArgs.Count(); i++)
            {
                //check what type of arg each one is
                if (Regex.IsMatch(commandArgs[i], @"restart-wait$"))
                {
                    Logging.Manager("/restart-wait detected, sleep 1000ms");
                    System.Threading.Thread.Sleep(1000);
                }
                else if (Regex.IsMatch(commandArgs[i], @"test$"))
                {
                    Logging.Manager("/test detected, loading in test mode");
                    testMode = true;
                }
                else if (Regex.IsMatch(commandArgs[i], @"skip-update$"))
                {
                    Logging.Manager("/skip-update detected, skipping update of application");
                    skipUpdate = true;
                }
                else if (Regex.IsMatch(commandArgs[i], @"patchday$"))
                {
                    Logging.Manager("/patchday detected, welcome database manager");
                    patchDayTest = true;
                }
                else if (Regex.IsMatch(commandArgs[i], @"beta-database$"))
                {
                    Logging.Manager("/beta-database detected, welcome beta tester");
                    Settings.BetaDatabase = true;
                    betaDBSetFromCommandLine = true;
                }
                else if (Regex.IsMatch(commandArgs[i], @"beta-application"))
                {
                    Logging.Manager("/beta-application detected, welcome beta tester");
                    Settings.BetaApplication = true;
                    betaAppSetFromCommandLine = true;
                }
                else if (Regex.IsMatch(commandArgs[i], @"silent-start"))
                {
                    Logging.Manager("/silent-start detected, loading in silent mode");
                    silentStart = true;
                }
                else if (Regex.IsMatch(commandArgs[i], @"auto-install$"))
                {
                    Logging.Manager("/auto-install detected");
                    autoInstall = true;
                    //parse the config file and advance the counter
                    configName = commandArgs[++i];
                }
                else if(Regex.IsMatch(commandArgs[i], @"updateKeyFile$"))
                {
                    Logging.Manager("/updateKeyFile detected");
                    updateFileKey = true;
                    updateKeyFile = commandArgs[++i];
                }
                else if (Regex.IsMatch(commandArgs[i], @"editorAutoLoad$"))
                {
                    editorAutoLoad = true;
                    editorDatabaseFile = commandArgs[++i];
                    Logging.Manager("/editorAutoLoad detected, attempting to auto load database from " + editorDatabaseFile);
                }
                else if (Regex.IsMatch(commandArgs[i], @"forceVisible$"))
                {
                    Logging.Manager("/forceVisible detected, loading all invisible mods in selection list");
                    forceVisible = true;
                }
                else if (Regex.IsMatch(commandArgs[i], @"forceEnabled$"))
                {
                    Logging.Manager("/forceEnabled detected, loading all visible mods as enabled");
                    forceEnabled = true;
                }
                else if (Regex.IsMatch(commandArgs[i], @"patchcheck$"))
                {
                    Logging.Manager("/patchcheck detected, loading in patch design mode");
                    Application.Run(new PatchTester());
                    return;
                }
                else if (Regex.IsMatch(commandArgs[i], @"databaseupdate$"))
                {
                    Logging.Manager("/databaseupdate detected, loading in database update mode");
                    if (skipUpdate)
                    {
                        Logging.Manager("if calling databaseupdate, /skip-update will be ignored");
                        skipUpdate = false;
                    }
                    Application.Run(new DatabaseUpdater());
                    return;
                }
                else if (Regex.IsMatch(commandArgs[i], @"databaseedit$"))
                {
                    Logging.Manager("/databaseedit detected, loading in database edit mode");
                    if (skipUpdate)
                    {
                        Logging.Manager("if calling databaseedit, /skip-update will be ignored");
                        skipUpdate = false;
                    }
                    Application.Run(new DatabaseEditor());
                    return;
                }
            }
            //load the translations
            Logging.Manager("Loading translation hashes");
            try
            {
                Translations.LoadHashes();
            }
            catch (Exception ex)
            {
                MainWindow.errorCounter++;
                Logging.Manager(string.Format("failed to load hash tables: {0}", ex.Message));
            }

            //start the background taskbar form
            Logging.Manager("Loading main window");
            MainWindow mw = new MainWindow();
            if (silentStart)
                mw.WindowState = FormWindowState.Minimized;
            Application.Run(mw);
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }
    }
}
