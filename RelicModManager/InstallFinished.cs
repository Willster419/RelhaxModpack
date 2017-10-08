using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace RelhaxModpack
{
    public partial class InstallFinished : Form
    {
        //private instance variables
        private string TanksLocation;
        private const int titleBar = 23;//set origionally for 23
        int difference;
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
            //setting UI color
            Settings.setUIColor(this);
            //font scaling
            this.AutoScaleMode = Settings.appScalingMode;
            this.Font = Settings.appFont;
            if (Settings.appScalingMode == System.Windows.Forms.AutoScaleMode.Dpi)
            {
                this.Scale(new SizeF(Settings.scaleSize, Settings.scaleSize));
            }
            //title bar height
            //get the size of the title bar window
            Rectangle screenRektangle = RectangleToScreen(this.ClientRectangle);
            int titleHeight = screenRektangle.Top - this.Top;
            //largest possible is 46
            //mine (programmed for) is 23
            if (titleHeight > titleBar)
            {
                difference = titleHeight - titleBar;
            }
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
            System.Diagnostics.Process.Start(WoTEXELocation);
            this.Close();
        }

        private void StartWoTLauncherButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(WoTLauncherLocation);
            this.Close();
        }

        private void StartXVMStatButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(string.Format(XVMURL, Translations.getTranslatedString("xvmUrlLocalisation")));
            this.Close();
        }

        private void CloseApplicationButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
