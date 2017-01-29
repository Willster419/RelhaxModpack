using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;

namespace RelicModManager
{
    public partial class VersionInfo : Form
    {

        public DialogResult result;

        public VersionInfo()
        {
            InitializeComponent();
        }

        private void updateAcceptButton_Click(object sender, EventArgs e)
        {
            result = DialogResult.Yes;
            this.Close();
        }

        private void updateDeclineButton_Click(object sender, EventArgs e)
        {
            result = DialogResult.No;
            this.Close();
        }

        private void VersionInfo_Load(object sender, EventArgs e)
        {
            string temp = newVersionAvailableLabel.Text;
            newVersionAvailableLabel.Text = "Loading...";
            Application.DoEvents();
            WebClient wc = new WebClient();
            downloadedVersionInfo.Text = wc.DownloadString("https://dl.dropboxusercontent.com/u/44191620/RelicMod/releaseNotes.txt");
            newVersionAvailableLabel.Text = temp;
        }
    }
}
