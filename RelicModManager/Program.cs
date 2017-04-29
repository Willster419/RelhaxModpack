using System;
using System.Collections.Generic;
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
        public static string configName = "";
        [STAThread]
        static void Main()
        {
            Settings.appendToLog("|------------------------------------------------------------------------------------------------|");
            Settings.appendToLog("Main Entry point launched");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //get the command line args for testing of auto install
            string[] commandArgs = Environment.GetCommandLineArgs();
            for (int i = 0; i < commandArgs.Count(); i++)
            {
                //check what type of arg each one is
                if (Regex.IsMatch(commandArgs[i], @"test$"))
                {
                    testMode = true;
                }
                else if (Regex.IsMatch(commandArgs[i], @"skip-update$"))
                {
                    skipUpdate = true;
                }
                else if (Regex.IsMatch(commandArgs[i], @"patchday$"))
                {
                    patchDayTest = true;
                }
                else if (Regex.IsMatch(commandArgs[i], @"auto-install$"))
                {
                    autoInstall = true;
                    //parse the config file and advance the counter
                    configName = commandArgs[++i];
                }
                else if (Regex.IsMatch(commandArgs[i], @"crccheck2$"))
                {
                    Application.Run(new CRCCHECK2());
                    return;
                }
                else if (Regex.IsMatch(commandArgs[i], @"crccheck$"))
                {
                    Application.Run(new CRCCheck());
                    return;
                }
                else if (Regex.IsMatch(commandArgs[i], @"patchcheck$"))
                {
                    Application.Run(new PatchTester());
                    return;
                }
                else if (Regex.IsMatch(commandArgs[i], @"databaseupdate$"))
                {
                    Application.Run(new CRCFileSizeUpdate());
                    return;
                }
            }
            //load the translations
            Settings.appendToLog("Loading translation hashes");
            Translations.loadHashes();
            Settings.appendToLog("Attempting to load MainWindow");
            try
            {
                Application.Run(new MainWindow());
            }
            catch (Exception ex)
            {
                Settings.appendToLog("EXCEPTION: Application load");
                Settings.appendToLog(ex.ToString());
            }
        }
    }
}
