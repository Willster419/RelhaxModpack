using System;
using System.IO;
using System.Windows;
using System.Diagnostics;
using Ionic.Zip;
using System.Windows.Forms;

namespace RelhaxModpack
{
    public partial class Diagnostics : RelhaxForum
    {
        private string MainWindowHeader = Translations.getTranslatedString("MainTextBox");
        public string TanksLocation { get; set; }
        public string AppStartupPath { get; set; }

        public Diagnostics()
        {
            InitializeComponent();
        }

        private void Diagnostics_Load(object sender, System.EventArgs e)
        {
            MainTextBox.Text = MainWindowHeader;
            if (TanksLocation == null  || TanksLocation.Equals(""))
                TanksLocation = "none";
            SelectedInstallation.Text = Translations.getTranslatedString(SelectedInstallation.Name) + TanksLocation;
            LaunchWoTLauncher.Text = Translations.getTranslatedString(LaunchWoTLauncher.Name);
            CollectLogInfo.Text = Translations.getTranslatedString(CollectLogInfo.Name);
            //ChangeInstallation.Text = Translations.getTranslatedString(ChangeInstallation.Name);
        }

        private void LaunchWoTLauncher_Click(object sender, System.EventArgs e)
        {
            if (TanksLocation.Equals("none"))
                return;
            Utils.AppendToLog("Starting WoTLauncher with argument \"-integrity_default_client\"");
            StartWoTLauncherResult.Text = Translations.getTranslatedString("startingLauncherRepairMode");
            string filename = Path.Combine(TanksLocation, "WoTLauncher.exe");
            string formattedArguement = "-integrity_default_client";
            Utils.AppendToLog("Complete Command line: " + filename + " " + formattedArguement);
            try
            {
                Process.Start(filename, formattedArguement);
            }
            catch(Exception ex)
            {
                Utils.ExceptionLog("LaunchWoTLauncher_Click", ex);
                System.Windows.Forms.MessageBox.Show(Translations.getTranslatedString("failedStartLauncherRepairMode"));
                StartWoTLauncherResult.Text = "";
                return;
            }
            StartWoTLauncherResult.Text = Translations.getTranslatedString("launcherRepairModeStarted");
        }

        private void CollectLogInfo_Click(object sender, System.EventArgs e)
        {
            if (TanksLocation.Equals("none"))
                return;
            Utils.AppendToLog("Collecting log files...");
            CollectLogInfoResult.Text = Translations.getTranslatedString("collectionLogInfo");
            using (ZipFile zip = new ZipFile())
            {
                string newZipFileName = "";
                try
                {
                    string RelHaxLogPath = Path.Combine(AppStartupPath, "RelHaxLog.txt");
                    string InstalledRelhaxFiles = Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log");
                    string PythonLog = Path.Combine(TanksLocation, "python.log");
                    string SelectionXMlFile = "";
                    using (OpenFileDialog findSelectionXMLFile = new OpenFileDialog()
                    {
                        AddExtension = true,
                        CheckFileExists = true,
                        CheckPathExists = true,
                        DefaultExt = ".xml",
                        Filter = @" *.xml | *.xml",
                        InitialDirectory = Path.Combine(AppStartupPath, "RelHaxUserConfigs"),
                        Title = Translations.getTranslatedString("selectConfigFile")
                    })
                    {
                        if(!(findSelectionXMLFile.ShowDialog() == DialogResult.Cancel))
                        {
                            SelectionXMlFile = findSelectionXMLFile.FileName;
                        }
                    }
                    string[] filesToCollect = new string[] { RelHaxLogPath, InstalledRelhaxFiles, PythonLog, SelectionXMlFile };
                    foreach(string s in filesToCollect)
                    {
                        if (s.Equals(""))
                            continue;
                        if(File.Exists(s))
                        {
                            ZipEntry entry = zip.AddFile(s);
                            entry.FileName = Path.GetFileName(s);
                        }
                        else
                        {
                            Utils.AppendToLog("WARNING: file " + Path.GetFileName(s) + "does not exist!");
                        }
                    }
                    newZipFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "RelhaxModpackLogs_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".zip");
                    zip.Save(newZipFileName);
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog(ex);
                    CollectLogInfoResult.Text = Translations.getTranslatedString("failedCreateZipfile");
                    return;
                }
                Utils.AppendToLog("Zip file saved to" + newZipFileName);
                CollectLogInfoResult.Text = Translations.getTranslatedString("zipSavedTo") + newZipFileName;
            }
        }

        private void ChangeInstallation_Click(object sender, EventArgs e)
        {

        }
    }
}
