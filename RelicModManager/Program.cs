using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;

namespace RelicModManager
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static bool testMode = false;
        public static bool autoInstall = false;
        public static string configName = "";
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainWindow temp = new MainWindow();
            if (!File.Exists(Application.StartupPath + "\\Ionic.Zip.dll"))
            {
                temp.extractEmbeddedResource(Application.StartupPath, "RelicModManager", new List<string>() { "Ionic.Zip.dll" });
            }
            if (!File.Exists(Application.StartupPath + "\\Newtonsoft.Json.dll"))
            {
                temp.extractEmbeddedResource(Application.StartupPath, "RelicModManager", new List<string>() { "Newtonsoft.Json.dll" });
            }
            temp.Dispose();
            //handler for using the embeded ionic zip dll
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                String resourceName = "AssemblyLoadingAndReflection." +
                   new AssemblyName(args.Name).Name + ".dll";
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RelicModManager.Ionic.Zip.dll"))
                {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }

            };
            //get the command line args for testing of auto install
            string[] commandArgs = Environment.GetCommandLineArgs();
            for (int i = 0; i < commandArgs.Count(); i++)
            {
                //check what type of arg each one is
                if (Regex.IsMatch(commandArgs[i], "test"))
                {
                    testMode = true;
                }
                else if (Regex.IsMatch(commandArgs[i], "auto-install"))
                {
                    autoInstall = true;
                    //parse the config file and advance the counter
                    configName = commandArgs[++i];
                }
                else if (Regex.IsMatch(commandArgs[i], "crccheck2"))
                {
                    Application.Run(new CRCCHECK2());
                    return;
                }
                else if (Regex.IsMatch(commandArgs[i], "crccheck"))
                {
                    Application.Run(new CRCCheck());
                    return;
                }
                else if (Regex.IsMatch(commandArgs[i], "patchcheck"))
                {
                    Application.Run(new PatchTester());
                    return;
                }
                else if (Regex.IsMatch(commandArgs[i], "databaseupdate"))
                {
                    Application.Run(new CRCFileSizeUpdate());
                    return;
                }
            }
            Application.Run(new MainWindow());
        }
    }
}
