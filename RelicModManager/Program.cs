using System;
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
        public static bool ignoreResourseVersionFail = false;
        public static bool saveSettings = false;
        public static bool databaseUpdateOnline = false;
        public static bool betaDatabase = false;
        public static bool betaApplication = false;
        public static string configName = "";
        [STAThread]
        static void Main()
        {
            Logging.Manager("|------------------------------------------------------------------------------------------------|");
            Logging.Manager("Main Entry point launched");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //loading embeded dlls from the application
            //https://www.codeproject.com/articles/528178/load-dll-from-embedded-resource
            string resource1 = "RelhaxModpack.Resources.DotNetZip.dll";
            string resource2 = "RelhaxModpack.Resources.Newtonsoft.Json.dll";
            EmbeddedAssembly.Load(resource1, "DotNetZip.dll");
            EmbeddedAssembly.Load(resource2, "Newtonsoft.Json.dll");
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
                if (Regex.IsMatch(commandArgs[i], @"test$"))
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
                else if (Regex.IsMatch(commandArgs[i], @"ignoreresourseversionfail$"))
                {
                    Logging.Manager("/ignoreResourseVersionFail detected, welcome developer");
                    ignoreResourseVersionFail = true;
                }
                else if (Regex.IsMatch(commandArgs[i], @"beta-database$"))
                {
                    Logging.Manager("/beta-database detected, welcome beta tester");
                    betaDatabase = true;
                }
                else if (Regex.IsMatch(commandArgs[i], @"beta-application"))
                {
                    Logging.Manager("/beta-application detected, welcome beta tester");
                    betaApplication = true;
                }
                else if (Regex.IsMatch(commandArgs[i], @"auto-install$"))
                {
                    Logging.Manager("/auto-install detected");
                    autoInstall = true;
                    //parse the config file and advance the counter
                    configName = commandArgs[++i];
                }
                else if (Regex.IsMatch(commandArgs[i], @"crccheck2$"))
                {
                    Logging.Manager("(DEPRECATED) /crccheck2 detected, loading in crccheck2 mode");
                    Application.Run(new CRCCHECK2());
                    return;
                }
                else if (Regex.IsMatch(commandArgs[i], @"crccheck$"))
                {
                    Logging.Manager("(DEPRECATED) /crccheck detected, loading in crccheck mode");
                    Application.Run(new CRCCheck());
                    return;
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
                    Application.Run(new CRCFileSizeUpdate());
                    return;
                }
                else if (Regex.IsMatch(commandArgs[i], @"databaseoutput$"))
                {
                    Logging.Manager("/databaseoutput detected, loading in database output mode");
                    Application.Run(new DatabaseListGenerater());
                    return;
                }
                else if (Regex.IsMatch(commandArgs[i], @"databaseedit$"))
                {
                    Logging.Manager("/databaseedit detected, loading in database edit mode");
                    Application.Run(new DatabaseEditor());
                    return;
                }
            }
            //load the translations
            Logging.Manager("Loading translation hashes");
            try
            {
                Translations.loadHashes();
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("Main", "loadHashes", ex);
            }
            Logging.Manager("Attempting to load MainWindow");
            Application.Run(new MainWindow());
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }
    }
}
