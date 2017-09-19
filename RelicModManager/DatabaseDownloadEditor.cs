using System.Windows.Forms;
using System.Net;
using System.IO;

namespace RelhaxModpack
{
    public partial class DatabaseDownloadEditor : Form
    {
        private string DownloadURL = "";
        private string SaveLocation = "";
        private string ZipFileName = "";
        private WebClient downloader;
        public DatabaseDownloadEditor(string downloadURL, string zipFileName)
        {
            InitializeComponent();
            DownloadURL = downloadURL;
            ZipFileName = zipFileName;
        }

        private void DatabaseDownloadEditor_Load(object sender, System.EventArgs e)
        {
            using (downloader = new WebClient())
            {
                SaveDownloadDialog.FileName = ZipFileName;
                if (SaveDownloadDialog.ShowDialog() == DialogResult.Cancel)
                    return;
                downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                SaveLocation = SaveDownloadDialog.FileName;
                downloader.DownloadFileAsync(new System.Uri(DownloadURL), SaveLocation);
            }
        }

        private void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadProgressBar.Value = e.ProgressPercentage;
            string downloadStatus = "downloading " + ZipFileName + "...";
            DownloadingLabel.Text = downloadStatus;
            downloadStatus = "" + e.BytesReceived / 1024 + " kb of " + e.TotalBytesToReceive / 1024 + "kb";
            SizeToDownload.Text = downloadStatus;
        }

        private void Downloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (e.Error.Message.Equals("The request was aborted: The request was canceled."))
                {
                    if (File.Exists(SaveLocation))
                        File.Delete(SaveLocation);
                    this.Close();
                    return;
                }
                else
                {
                    MessageBox.Show(e.Error.ToString());
                    return;
                }
            }
            else
            {
                DownloadingLabel.Text = "Download Complete";
                OpenFileButton.Enabled = true;
                OpenFolderButton.Enabled = true;
                CancelButton.Enabled = false;
            }
        }

        private void OpenFolderButton_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start(Path.GetDirectoryName(SaveLocation));
        }

        private void OpenFileButton_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start(SaveLocation);
        }

        private void DatabaseDownloadEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
        }

        private void CancelButton_Click(object sender, System.EventArgs e)
        {
            downloader.CancelAsync();
        }
    }
}
