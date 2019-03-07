using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net;
using System.IO;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for DatabaseEditorDownload.xaml
    /// </summary>
    public partial class DatabaseEditorDownload : RelhaxWindow
    {

        public string ZipFilePathDisk;
        public string ZipFilePathOnline;
        public NetworkCredential Credential;
        public bool Upload;

        public DatabaseEditorDownload()
        {
            InitializeComponent();
        }

        private async void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //set the UI parameters based on upload or download
            switch(Upload)
            {
                case true:
                    OpenFodlerButton.IsEnabled = false;
                    OpenFodlerButton.Visibility = Visibility.Hidden;
                    OpenFileButton.IsEnabled = false;
                    OpenFileButton.Visibility = Visibility.Hidden;
                    break;
                case false:
                    OpenFodlerButton.IsEnabled = true;
                    OpenFodlerButton.Visibility = Visibility.Visible;
                    OpenFileButton.IsEnabled = true;
                    OpenFileButton.Visibility = Visibility.Visible;
                    break;
            }
            ProgressBody.Text = string.Format("{0} {1} {2} FTP folder {3}", Upload ? "Uploading" : "Downloading",
                Path.GetFileName(ZipFilePathDisk), Upload ? "to" : "from", Settings.WoTModpackOnlineFolderVersion);
            ProgressHeader.Text = string.Format("{0} 0 of 0 kb", Upload ? "Downloaded" : "Uploaded");
            using (WebClient client = new WebClient() { Credentials=Credential })
            {
                client.UploadProgressChanged += Client_UploadProgressChanged;
                await client.UploadFileTaskAsync(ZipFilePathOnline, ZipFilePathDisk);
            }
        }

        private void Client_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            
        }

        private void OpenFodlerButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
