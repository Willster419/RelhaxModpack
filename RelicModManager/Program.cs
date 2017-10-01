using System;
using System.IO;
using System.Linq;
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
        public static string configName = "";
        [STAThread]
        static void Main()
        {
            Utils.appendToLog("|------------------------------------------------------------------------------------------------|");
            Utils.appendToLog("Main Entry point launched");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // delete RelicCopyUpdate.bat at start (it is only needed at updates, so kill it)
            try
            {
                string updateBatPath = Path.Combine(Application.StartupPath, "RelicCopyUpdate.bat");
                if (System.IO.File.Exists(updateBatPath))
                    System.IO.File.Delete(updateBatPath);
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("Main", "delete RelicCopyUpdate.bat", ex);
            }
            //get the command line args for testing of auto install
            string[] commandArgs = Environment.GetCommandLineArgs();
            //log command line
            Utils.appendToLog("command line: " + string.Join(" ", commandArgs));
            for (int i = 0; i < commandArgs.Count(); i++)
            {
                //check what type of arg each one is
                if (Regex.IsMatch(commandArgs[i], @"test$"))
                {
                    Utils.appendToLog("/test detected, loading in test mode");
                    testMode = true;
                }
                else if (Regex.IsMatch(commandArgs[i], @"skip-update$"))
                {
                    Utils.appendToLog("/skip-update detected, skipping update of application");
                    skipUpdate = true;
                }
                else if (Regex.IsMatch(commandArgs[i], @"patchday$"))
                {
                    Utils.appendToLog("/patchday detected, welcome database manager");
                    patchDayTest = true;
                }
                else if (Regex.IsMatch(commandArgs[i], @"auto-install$"))
                {
                    Utils.appendToLog("/auto-install detected");
                    autoInstall = true;
                    //parse the config file and advance the counter
                    configName = commandArgs[++i];
                }
                else if (Regex.IsMatch(commandArgs[i], @"crccheck2$"))
                {
                    Utils.appendToLog("(DEPRECATED) /crccheck2 detected, loading in crccheck2 mode");
                    Application.Run(new CRCCHECK2());
                    return;
                }
                else if (Regex.IsMatch(commandArgs[i], @"crccheck$"))
                {
                    Utils.appendToLog("(DEPRECATED) /crccheck detected, loading in crccheck mode");
                    Application.Run(new CRCCheck());
                    return;
                }
                else if (Regex.IsMatch(commandArgs[i], @"patchcheck$"))
                {
                    Utils.appendToLog("/patchcheck detected, loading in patch design mode");
                    Application.Run(new PatchTester());
                    return;
                }
                else if (Regex.IsMatch(commandArgs[i], @"databaseupdate$"))
                {
                    Utils.appendToLog("/databaseupdate detected, loading in database update mode");
                    Application.Run(new CRCFileSizeUpdate());
                    return;
                }
                else if (Regex.IsMatch(commandArgs[i], @"databaseoutput$"))
                {
                    Utils.appendToLog("/databaseoutput detected, loading in database output mode");
                    Application.Run(new DatabaseListGenerater());
                    return;
                }
                else if (Regex.IsMatch(commandArgs[i], @"databaseedit$"))
                {
                    Utils.appendToLog("/databaseedit detected, loading in database edit mode");
                    Application.Run(new DatabaseEditor());
                    return;
                }
            }
            //load the translations
            Utils.appendToLog("Loading translation hashes");
            try
            {
                Translations.loadHashes();
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("Main", "loadHashes", ex);
            }
            Utils.appendToLog("Attempting to load MainWindow");
            Application.Run(new MainWindow());
        }
    }
}
