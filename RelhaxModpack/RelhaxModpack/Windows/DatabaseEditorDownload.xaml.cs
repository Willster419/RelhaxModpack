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
    public class EditorUploadDownloadEventArgs : EventArgs
    {
        public DatabasePackage Package;
    }
    public delegate void EditorUploadDownloadClosed(object sender, EditorUploadDownloadEventArgs e);
    /// <summary>
    /// Interaction logic for DatabaseEditorDownload.xaml
    /// </summary>
    public partial class DatabaseEditorDownload : RelhaxWindow
    {

        public string ZipFilePathDisk;
        public string ZipFilePathOnline;
        public string ZipFileName;
        public NetworkCredential Credential;
        public bool Upload;
        public DatabasePackage PackageToUpdate;
        public event EditorUploadDownloadClosed OnEditorUploadDownloadClosed;

        private WebClient client;
        private string CompleteFTPPath;

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
                    OpenFodlerButton.Visibility = Visibility.Hidden;
                    OpenFileButton.Visibility = Visibility.Hidden;
                    break;
                case false:
                    OpenFodlerButton.Visibility = Visibility.Visible;
                    OpenFileButton.Visibility = Visibility.Visible;
                    break;
            }
            if(PackageToUpdate == null)
            {
                ProgressBody.Text = string.Format("{0} {1} {2} FTP folder {3}", "Uploading",
                ZipFileName, "to", "Medias/...");
            }
            else
            {
                ProgressBody.Text = string.Format("{0} {1} {2} FTP folder {3}", Upload ? "Uploading" : "Downloading",
                Path.GetFileName(ZipFilePathDisk), Upload ? "to" : "from", Settings.WoTModpackOnlineFolderVersion);
            }
            ProgressHeader.Text = string.Format("{0} 0 kb of 0 kb", Upload ? "Uploaded" : "Downloaded");
            CompleteFTPPath = string.Format("{0}{1}", ZipFilePathOnline, ZipFileName);
            using (client = new WebClient() { Credentials=Credential })
            {
                switch(Upload)
                {
                    case true:
                        //before uploading, make sure it doesn't exist first
                        ProgressHeader.Text = "Checking if file exists on server...";
                        Logging.Debug("Checking if {0} already exists on the server in folder {1}", ZipFileName, Settings.WoTModpackOnlineFolderVersion);
                        string[] listOfFilesOnServer = await Utils.FTPListFilesFoldersAsync(ZipFilePathOnline, Credential);
                        if (listOfFilesOnServer.Contains(ZipFileName) && MessageBox.Show("File already exists, overwrite?", "", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                        {
                            Logging.Debug("DOES exist and user said don't overwrite, aborting");
                            ProgressHeader.Text = "Canceled";
                            return;
                        }
                        client.UploadProgressChanged += Client_UploadProgressChanged;
                        //write handler for if upload or download was canceled
                        client.UploadFileCompleted += Client_DownloadUploadFileCompleted;
                        Logging.Debug("STARTING FTP UPLOAD");
                        try
                        {
                            await client.UploadFileTaskAsync(CompleteFTPPath, ZipFilePathDisk);
                            Logging.Debug("FTP UPLOAD COMPLETE");
                            //if we're uploading a package zip file, then PackageToUpdate is not null, and fire the event
                            if (PackageToUpdate != null)
                            {
                                Logging.Debug("FTP upload complete, changing zipFile entry for package {0} from", PackageToUpdate.PackageName);
                                Logging.Debug("\"{0}\"{1}to{2}", PackageToUpdate.ZipFile, Environment.NewLine, Environment.NewLine);
                                Logging.Debug("\"{0}\"", ZipFileName);
                                PackageToUpdate.ZipFile = ZipFileName;
                                if (OnEditorUploadDownloadClosed != null)
                                {
                                    OnEditorUploadDownloadClosed(this, new EditorUploadDownloadEventArgs()
                                    {
                                        Package = PackageToUpdate
                                    });
                                }
                            }
                            else
                            {
                                DialogResult = true;
                                Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            Logging.Info("FTP UPLOAD Failed");
                            Logging.Info(ex.ToString());
                            //MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            CancelButton.IsEnabled = false;
                        }
                        break;
                    case false:
                        client.DownloadProgressChanged += Client_DownloadProgressChanged;
                        //write handler for if upload or download was canceled
                        client.DownloadFileCompleted += Client_DownloadUploadFileCompleted;
                        Logging.Debug("STARTING FTP DOWNLOAD");
                        try
                        {
                            await client.DownloadFileTaskAsync(CompleteFTPPath, ZipFilePathDisk);
                            Logging.Debug("FTP download complete ({0})",ZipFileName);
                        }
                        catch (Exception ex)
                        {
                            Logging.Info("FTP download failed");
                            Logging.Info(ex.ToString());
                            //MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            CancelButton.IsEnabled = false;
                            OpenFileButton.IsEnabled = true;
                            OpenFodlerButton.IsEnabled = true;
                        }
                        break;
                }
            }
        }

        private async void Client_DownloadUploadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if(e.Cancelled)
            {
                Logging.Debug("FTP upload or download cancel detected from UI thread, handling");
                switch (Upload)
                {
                    case true:
                        //delete file on server
                        Logging.Debug("deleting file on server");
                        await Utils.FTPDeleteFileAsync(CompleteFTPPath, Credential);
                        break;
                    case false:
                        Logging.Debug("deleting file on disk");
                        File.Delete(ZipFilePathDisk);
                        break;
                }
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (ProgressProgressBar.Maximum != e.TotalBytesToReceive)
                ProgressProgressBar.Maximum = e.TotalBytesToReceive;
            ProgressProgressBar.Value = e.BytesReceived;
            ProgressHeader.Text = string.Format("{0} {1} kb of {2} kb", "Downloaded", e.BytesReceived / 1024, e.TotalBytesToReceive / 1024);
        }

        private void Client_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            if (ProgressProgressBar.Maximum != e.TotalBytesToSend)
                ProgressProgressBar.Maximum = e.TotalBytesToSend;
            ProgressProgressBar.Value = e.BytesSent;
            ProgressHeader.Text = string.Format("{0} {1} kb of {2} kb", "Uploaded", e.BytesSent / 1024, e.TotalBytesToSend / 1024);
        }

        private void OpenFodlerButton_Click(object sender, RoutedEventArgs e)
        {
            if(!Directory.Exists(Path.GetDirectoryName(ZipFilePathDisk)))
            {
                Logging.Error("OpenFolder button pressed but path {0} does not exist!", Path.GetDirectoryName(ZipFilePathDisk));
                return;
            }
            try
            {
                System.Diagnostics.Process.Start(Path.GetDirectoryName(ZipFilePathDisk));
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(ZipFilePathDisk))
            {
                Logging.Error("OpenFile button pressed but file {0} does not exist!", ZipFilePathDisk);
                return;
            }
            try
            {
                System.Diagnostics.Process.Start(ZipFilePathDisk);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Logging.Debug("Cancel pressed, Upload={0}", Upload.ToString());
            ProgressHeader.Text = "Canceled";
            Logging.Debug("Canceling upload or download operation");
            client.CancelAsync();
        }
    }
}
