using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace RelhaxModpack
{
    public partial class InstallFinished : RelhaxForum
    {
        //private instance variables
        private string TanksLocation;
        private string WoTEXELocation;
        private string WoTLauncherLocation;
        private string XVMURL = "http://www.modxvm.com/{0}/";
        public InstallFinished(string tanksLocation)
        {
            InitializeComponent();
            TanksLocation = tanksLocation;
        }

        private void InstallFinished_Load(object sender, EventArgs e)
        {
            //apply translations
            InstallCompleteLabel.Text = Translations.getTranslatedString(InstallCompleteLabel.Name);
            StartTanksButton.Text = Translations.getTranslatedString(StartTanksButton.Name);
            StartWoTLauncherButton.Text = Translations.getTranslatedString(StartWoTLauncherButton.Name);
            StartXVMStatButton.Text = Translations.getTranslatedString(StartXVMStatButton.Name);
            CloseApplicationButton.Text = Translations.getTranslatedString(CloseApplicationButton.Name);
            //check if files are available to launch before actually displaying them
            WoTEXELocation = Path.Combine(TanksLocation, "WorldOfTanks.exe");
            WoTLauncherLocation = Path.Combine(TanksLocation, "WoTLauncher.exe");
            if (!File.Exists(WoTEXELocation))
                StartTanksButton.Enabled = false;
            if (!File.Exists(WoTLauncherLocation))
                StartWoTLauncherButton.Enabled = false;
        }

        private void StartTanksButton_Click(object sender, EventArgs e)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = Path.GetDirectoryName(WoTEXELocation);
            startInfo.FileName = WoTEXELocation;
            Process.Start(startInfo);
            this.Close();
        }

        private void StartWoTLauncherButton_Click(object sender, EventArgs e)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = Path.GetDirectoryName(WoTLauncherLocation);
            startInfo.FileName = WoTLauncherLocation;
            Process.Start(startInfo);
            this.Close();
        }

        private void StartXVMStatButton_Click(object sender, EventArgs e)
        {
            Process.Start(string.Format(XVMURL, Translations.getTranslatedString("xvmUrlLocalisation")));
            this.Close();
        }

        private void CloseApplicationButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
