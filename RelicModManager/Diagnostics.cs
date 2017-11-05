using System;
using System.IO;
using System.Windows;
using System.Diagnostics;

namespace RelhaxModpack
{
    public partial class Diagnostics : RelhaxForum
    {
        private string MainWindowHeader = Translations.getTranslatedString("MainTextBox");
        public string TanksLocation { get; set; }

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
                MessageBox.Show(Translations.getTranslatedString("failedStartLauncherRepairMode"));
                StartWoTLauncherResult.Text = "";
                return;
            }
            StartWoTLauncherResult.Text = Translations.getTranslatedString("launcherRepairModeStarted");
        }

        private void CollectLogInfo_Click(object sender, System.EventArgs e)
        {
            Utils.AppendToLog("Collecting log files...");
            CollectLogInfoResult.Text = Translations.getTranslatedString("collectionLogInfo");

        }

        private void ChangeInstallation_Click(object sender, EventArgs e)
        {

        }
    }
}
