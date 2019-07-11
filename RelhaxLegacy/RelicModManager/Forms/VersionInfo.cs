using System;
using System.Windows.Forms;

namespace RelhaxModpack
{
    //window that is displayed when an update to the application is detected
    public partial class VersionInfo : RelhaxForum
    {
        public VersionInfo()
        {
            InitializeComponent();
        }
        //handler for when the user accepts the new version download
        private void updateAcceptButton_Click(object sender, EventArgs e)
        {
            //set the result ot yes and close
            DialogResult = DialogResult.Yes;
            this.Close();
        }
        //handler for when the user declines the new version download
        private void updateDeclineButton_Click(object sender, EventArgs e)
        {
            //set the result to no and close
            DialogResult = DialogResult.Cancel;
        }
        //handler for before the window is displayed
        private void VersionInfo_Load(object sender, EventArgs e)
        {
            updateAcceptButton.Text = Translations.GetTranslatedString(updateAcceptButton.Name);
            updateDeclineButton.Text = Translations.GetTranslatedString(updateDeclineButton.Name);
            newVersionAvailableLabel.Text = Translations.GetTranslatedString(newVersionAvailableLabel.Name);
            updateQuestionLabel.Text = Translations.GetTranslatedString(updateQuestionLabel.Name);
            problemsUpdatingLabel.Text = Translations.GetTranslatedString(problemsUpdatingLabel.Name);
            clickHereUpdateLabel.Text = Translations.GetTranslatedString(clickHereUpdateLabel.Name);
            downloadedVersionInfo.Text = Settings.BetaApplication ? Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "releaseNotes_beta.txt") : Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "releaseNotes.txt");
            clickHereUpdateLabel.Left = problemsUpdatingLabel.Left + problemsUpdatingLabel.Width;
        }

        private void clickHereUpdateLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // System.Diagnostics.Process.Start("https://github.com/Willster419/RelhaxModpack/releases/latest");
            Utils.CallBrowser("https://github.com/Willster419/RelhaxModpack/releases/latest");
        }

        private void downloadedVersionInfo_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            // System.Diagnostics.Process.Start(e.LinkText);
            Utils.CallBrowser(e.LinkText);
        }
    }
}
