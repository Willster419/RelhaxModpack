using System;
using System.Net;
using System.Windows.Forms;

namespace RelhaxModpack
{
    //window that is displayed when an update to the application is detected
    public partial class VersionInfo : RelhaxForum
    {
        //the result returned form this window
        public DialogResult result;

        public VersionInfo()
        {
            InitializeComponent();
        }
        //handler for when the user accepts the new version download
        private void updateAcceptButton_Click(object sender, EventArgs e)
        {
            //set the result ot yes and close
            result = DialogResult.Yes;
            this.Close();
        }
        //handler for when the user declines the new version download
        private void updateDeclineButton_Click(object sender, EventArgs e)
        {
            //set the result to no and close
            result = DialogResult.No;
            this.Close();
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
            VersionInfo_SizeChanged(null, null);
        }

        private void clickHereUpdateLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Willster419/RelhaxModpack/releases/latest");
        }

        private void downloadedVersionInfo_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void VersionInfo_SizeChanged(object sender, EventArgs e)
        {
            downloadedVersionInfo.Size = new System.Drawing.Size(this.Size.Width - 36, this.Size.Height - updateDeclineButton.Size.Height - downloadedVersionInfo.Location.Y - 59 - 3);
            updateDeclineButton.Location = new System.Drawing.Point(newVersionAvailableLabel.Location.X, this.Size.Height - updateDeclineButton.Size.Height - 59);
            updateAcceptButton.Location = new System.Drawing.Point(this.Size.Width - updateAcceptButton.Size.Width - 24, updateDeclineButton.Location.Y);
            problemsUpdatingLabel.Location = new System.Drawing.Point(problemsUpdatingLabel.Location.X, this.Size.Height - 56);
            clickHereUpdateLabel.Location = new System.Drawing.Point(clickHereUpdateLabel.Location.X, this.Size.Height - 56);
            int middle = (this.Size.Width / 2) - (updateQuestionLabel.Size.Width / 2);
            updateQuestionLabel.Location = new System.Drawing.Point(middle, updateDeclineButton.Location.Y + 3);
        }
    }
}
