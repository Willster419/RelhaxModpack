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
            updateAcceptButton.Text = Translations.getTranslatedString(updateAcceptButton.Name);
            updateDeclineButton.Text = Translations.getTranslatedString(updateDeclineButton.Name);
            newVersionAvailableLabel.Text = Translations.getTranslatedString(newVersionAvailableLabel.Name);
            updateQuestionLabel.Text = Translations.getTranslatedString(updateQuestionLabel.Name);
            problemsUpdatingLabel.Text = Translations.getTranslatedString(problemsUpdatingLabel.Name);
            clickHereUpdateLabel.Text = Translations.getTranslatedString(clickHereUpdateLabel.Name);
            downloadedVersionInfo.Text = Program.betaApplication ? Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "releaseNotes_beta.txt") : Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "releaseNotes.txt");
        }

        private void clickHereUpdateLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Willster419/RelhaxModpack/releases/latest");
        }

        private void downloadedVersionInfo_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
    }
}
