using System;
using System.IO;
using System.Diagnostics;
using Ionic.Zip;
using System.Windows.Forms;
using System.Collections.Generic;

namespace RelhaxModpack
{
    public partial class Diagnostics : RelhaxForum
    {
        private string MainWindowHeader = Translations.GetTranslatedString("MainTextBox");
        public string TanksLocation { get; set; }
        public string AppStartupPath { get; set; }
        public MainWindow ParentWindow { get; set; }

        public Diagnostics()
        {
            InitializeComponent();
        }

        private void Diagnostics_Load(object sender, System.EventArgs e)
        {
            MainTextBox.Text = MainWindowHeader;
            if (TanksLocation == null  || TanksLocation.Equals(""))
                TanksLocation = "none";
            SelectedInstallation.Text = Translations.GetTranslatedString(SelectedInstallation.Name) + TanksLocation;
            LaunchWoTLauncher.Text = Translations.GetTranslatedString(LaunchWoTLauncher.Name);
            CollectLogInfo.Text = Translations.GetTranslatedString(CollectLogInfo.Name);
            ChangeInstall.Text = Translations.GetTranslatedString(ChangeInstall.Name);
        }

        private void LaunchWoTLauncher_Click(object sender, System.EventArgs e)
        {
            if (TanksLocation.Equals("none"))
                return;
            Logging.Manager("Starting WoTLauncher with argument \"-integrity_default_client\"");
            StartWoTLauncherResult.Text = Translations.GetTranslatedString("startingLauncherRepairMode");
            string filename = Path.Combine(TanksLocation, "WoTLauncher.exe");
            string formattedArguement = "-integrity_default_client";
            Logging.Manager("Complete Command line: " + filename + " " + formattedArguement);
            try
            {
                Process.Start(filename, formattedArguement);
            }
            catch(Exception ex)
            {
                Utils.ExceptionLog("LaunchWoTLauncher_Click", ex);
                System.Windows.Forms.MessageBox.Show(Translations.GetTranslatedString("failedStartLauncherRepairMode"));
                StartWoTLauncherResult.Text = "";
                return;
            }
            StartWoTLauncherResult.Text = Translations.GetTranslatedString("launcherRepairModeStarted");
        }

        private void CollectLogInfo_Click(object sender, System.EventArgs e)
        {
            if (TanksLocation.Equals("none"))
                return;
            Logging.Manager("Collecting log files...");
            CollectLogInfoResult.Text = Translations.GetTranslatedString("collectionLogInfo");
            using (ZipFile zip = new ZipFile())
            {
                string newZipFileName = "";
                try
                {
                    string RelHaxLogPath = Path.Combine(AppStartupPath, "RelHaxLog.txt");
                    string LastInstalledConfig = Path.Combine(AppStartupPath, "RelHaxUserConfigs", "lastInstalledConfig.xml");
                    string InstalledRelhaxFiles = Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log");
                    string UninstalledRelhaxFiles = Path.Combine(TanksLocation, "logs", "uninstall.log");
                    string PythonLog = Path.Combine(TanksLocation, "python.log");
                    string SelectionXMlFile = "";
                    List<string> filesToCollect = new List<string>(){ RelHaxLogPath, InstalledRelhaxFiles, UninstalledRelhaxFiles, PythonLog, SelectionXMlFile, LastInstalledConfig };
                    using (AddPicturesZip apz = new AddPicturesZip()
                    {
                        AppStartupPath = this.AppStartupPath
                    })
                    {
                        apz.ShowDialog();
                        if (!(apz.DialogResult == DialogResult.OK))
                            return;
                        foreach(object o in apz.listBox1.Items)
                        {
                            string s = (string)o;
                            filesToCollect.Add(s);
                        }
                    }
                    foreach(string s in filesToCollect)
                    {
                        if (s.Equals(""))
                            continue;
                        //verify that it's not already in there but from a different folder
                        int dupCunter = 0;
                        string nameInZipFile = Path.GetFileName(s);
                        foreach(ZipEntry ze in zip)
                        {
                            while (ze.FileName.Equals(nameInZipFile))
                                nameInZipFile = Path.GetFileName(s) + dupCunter++;
                        }
                        if(File.Exists(s))
                        {
                            ZipEntry entry = zip.AddFile(s);
                            entry.FileName = nameInZipFile;
                        }
                        else
                        {
                            Logging.Manager("WARNING: file " + s + " does not exist!");
                        }
                    }
                    newZipFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "RelhaxModpackLogs_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".zip");
                    zip.Save(newZipFileName);
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog(ex);
                    CollectLogInfoResult.Text = Translations.GetTranslatedString("failedCreateZipfile");
                    return;
                }
                Logging.Manager("Zip file saved to" + newZipFileName);
                CollectLogInfoResult.Text = Translations.GetTranslatedString("zipSavedTo") + newZipFileName;
            }
        }

        private void ChangeInstallation_Click(object sender, EventArgs e)
        {
            //attempt to locate the tanks directory
            if (ParentWindow.ManuallyFindTanks() == null)
            {
                //ParentWindow.ToggleUIButtons(true);
                return;
            }
            //parse all strings
            ParentWindow.tanksLocation = ParentWindow.tanksLocation.Substring(0, ParentWindow.tanksLocation.Length - 17);
            TanksLocation = ParentWindow.tanksLocation;
            SelectedInstallation.Text = Translations.GetTranslatedString(SelectedInstallation.Name) + TanksLocation;
            Logging.Manager(string.Format("tanksLocation parsed as {0}", ParentWindow.tanksLocation));
        }
    }
}
