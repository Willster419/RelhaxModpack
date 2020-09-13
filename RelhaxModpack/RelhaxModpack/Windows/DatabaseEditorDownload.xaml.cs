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
using System.Timers;
using System.Windows.Threading;
using RelhaxModpack.Utilities;
using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Event args returned to the editor for when an FTP upload or download is complete
    /// </summary>
    public class EditorUploadDownloadEventArgs : EventArgs
    {
        /// <summary>
        /// The package that was just uploaded
        /// </summary>
        public DatabasePackage Package;

        /// <summary>
        /// The path to the file that was uploaded or downloaded
        /// </summary>
        public string UploadedFilename;

        /// <summary>
        /// The FTP path to the field that was uploaded or downloaded
        /// </summary>
        public string UploadedFilepathOnline;
    }

    /// <summary>
    /// The delegate for invocation of when the FTP upload or download finishes
    /// </summary>
    /// <param name="sender">The sending object</param>
    /// <param name="e">The Upload or download event arguments</param>
    public delegate void EditorUploadDownloadClosed(object sender, EditorUploadDownloadEventArgs e);

    /// <summary>
    /// Interaction logic for DatabaseEditorDownload.xaml
    /// </summary>
    public partial class DatabaseEditorDownload : RelhaxWindow
    {
        //public
        /// <summary>
        /// The path to the zip file on the disk
        /// </summary>
        public string ZipFilePathDisk;

        /// <summary>
        /// The FTP path to the zip file
        /// </summary>
        public string ZipFilePathOnline;

        /// <summary>
        /// The complete name of the Zip file
        /// </summary>
        public string ZipFileName;

        /// <summary>
        /// The FTP credentials
        /// </summary>
        public NetworkCredential Credential;

        /// <summary>
        /// Flag to indicate upload or download. True is upload, false is download
        /// </summary>
        public bool Upload;

        /// <summary>
        /// The package being updated. A null package with Upload=true indicates the item being uploaded is a media
        /// </summary>
        public DatabasePackage PackageToUpdate;

        /// <summary>
        /// The event callback used for the editor when an upload or download is finished
        /// </summary>
        public event EditorUploadDownloadClosed OnEditorUploadDownloadClosed;

        /// <summary>
        /// The timeout, in seconds, until the window will automatically close
        /// </summary>
        public uint Countdown = 0;

        //private
        private WebClient client = null;
        private string CompleteFTPPath = string.Empty;
        private long FTPDownloadFilesize = -1;
        private DispatcherTimer timer = null;

        /// <summary>
        /// Create an instance of the DatabaseEditorDownlaod class
        /// </summary>
        public DatabaseEditorDownload()
        {
            InitializeComponent();
        }

        private async void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //init timer
            timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, Timer_Elapsed, this.Dispatcher) { IsEnabled = false };

            //set the open folder and file button
            //if uploading, the buttons are invalid, don't show then
            //if downloading, the buttons are valid to show, but not enabled until the download is complete
            switch(Upload)
            {
                case true:
                    OpenFodlerButton.Visibility = Visibility.Hidden;
                    OpenFileButton.Visibility = Visibility.Hidden;
                    break;
                case false:
                    OpenFodlerButton.Visibility = Visibility.Visible;
                    OpenFileButton.Visibility = Visibility.Visible;
                    OpenFodlerButton.IsEnabled = false;
                    OpenFileButton.IsEnabled = false;
                    break;
            }

            //set header
            if(!Upload)
            {
                //download
                ProgressBody.Text = string.Format("Downloading {0} from FTP folder {1}", Path.GetFileName(ZipFilePathDisk), Settings.WoTModpackOnlineFolderVersion);
            }
            else if(PackageToUpdate == null)
            {
                //upload to medias
                ProgressBody.Text = string.Format("Uploading {0} to FTP folder Medias/...", ZipFileName);
            }
            else
            {
                //upload to bigmods
                ProgressBody.Text = string.Format("Uploading {0} to FTP folder {1}", Path.GetFileName(ZipFilePathDisk), Settings.WoTModpackOnlineFolderVersion);
            }

            //set body initial text
            ProgressHeader.Text = string.Format("{0} 0 kb of 0 kb", Upload ? "Uploaded" : "Downloaded");

            CompleteFTPPath = string.Format("{0}{1}", ZipFilePathOnline, ZipFileName);
            using (client = new WebClient() { Credentials=Credential })
            {
                switch(Upload)
                {
                    case true:
                        //before uploading, make sure it doesn't exist first
                        ProgressHeader.Text = "Checking if file exists on server...";
                        Logging.Editor("Checking if {0} already exists on the server in folder {1}", LogLevel.Info, ZipFileName, Settings.WoTModpackOnlineFolderVersion);
                        string[] listOfFilesOnServer = await FtpUtils.FtpListFilesFoldersAsync(ZipFilePathOnline, Credential);
                        if (listOfFilesOnServer.Contains(ZipFileName) && MessageBox.Show("File already exists, overwrite?", "", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                        {
                            Logging.Editor("DOES exist and user said don't overwrite, aborting");
                            ProgressHeader.Text = "Canceled";
                            return;
                        }
                        client.UploadProgressChanged += Client_UploadProgressChanged;
                        //write handler for if upload or download was canceled
                        client.UploadFileCompleted += Client_DownloadUploadFileCompleted;

                        Logging.Editor("STARTING FTP UPLOAD");
                        try
                        {
                            await client.UploadFileTaskAsync(CompleteFTPPath, ZipFilePathDisk);
                        }
                        catch (Exception ex)
                        {
                            Logging.Editor("FTP UPLOAD Failed");
                            Logging.Editor(ex.ToString());
                        }

                        Logging.Editor("FTP UPLOAD COMPLETE");
                        if (PackageToUpdate == null)
                            Logging.Editor("FTP media upload complete");
                        else
                        {
                            Logging.Editor("FTP zip package upload complete, changing zipFile entry for package {0} from", LogLevel.Info, PackageToUpdate.PackageName);
                            Logging.Editor("\"{0}\"{1}to{2}", LogLevel.Info, PackageToUpdate.ZipFile, Environment.NewLine, Environment.NewLine);
                            Logging.Editor("\"{0}\"", LogLevel.Info, ZipFileName);
                            PackageToUpdate.ZipFile = ZipFileName;
                        }
                        
                        if (OnEditorUploadDownloadClosed != null)
                        {
                            OnEditorUploadDownloadClosed(this, new EditorUploadDownloadEventArgs()
                            {
                                Package = PackageToUpdate,
                                UploadedFilename = ZipFileName,
                                UploadedFilepathOnline = ZipFilePathOnline
                            });
                        }
                        CancelButton.IsEnabled = false;
                        break;
                    case false:
                        client.DownloadProgressChanged += Client_DownloadProgressChanged;
                        //write handler for if upload or download was canceled
                        client.DownloadFileCompleted += Client_DownloadUploadFileCompleted;
                        Logging.Editor("STARTING FTP DOWNLOAD");
                        try
                        {
                            FTPDownloadFilesize = await FtpUtils.FtpGetFilesizeAsync(CompleteFTPPath, Credential);
                            await client.DownloadFileTaskAsync(CompleteFTPPath, ZipFilePathDisk);
                            Logging.Editor("FTP DOWNLOAD COMPLETE ({0})", LogLevel.Info, ZipFileName);
                        }
                        catch (Exception ex)
                        {
                            Logging.Editor("FTP download failed");
                            Logging.Editor(ex.ToString());
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
            StartTimerForClose();
        }

        private void StartTimerForClose()
        {
            if(Countdown == 0)
            {
                Logging.Editor("Countdown is 0, do not close");
            }
            else
            {
                Logging.Editor("Countdown is > 0, starting");
                TimeoutClose.Visibility = Visibility.Visible;
                timer.Start();
                TimeoutClose.Text = Countdown.ToString();
            }
        }

        private void Timer_Elapsed(object sender, EventArgs e)
        {
            TimeoutClose.Text = (--Countdown).ToString();
            if (Countdown == 0)
            {
                Logging.Editor("countdown complete, closing the window");
                timer.Stop();
                Close();
            }
        }

        private async void Client_DownloadUploadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if(e.Cancelled)
            {
                Logging.Editor("FTP upload or download cancel detected from UI thread, handling");
                switch (Upload)
                {
                    case true:
                        //delete file on server
                        Logging.Editor("deleting file on server");
                        await FtpUtils.FtpDeleteFileAsync(CompleteFTPPath, Credential);
                        break;
                    case false:
                        Logging.Editor("deleting file on disk");
                        File.Delete(ZipFilePathDisk);
                        break;
                }
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //https://stackoverflow.com/questions/4591059/download-file-from-ftp-with-progress-totalbytestoreceive-is-always-1
            if (ProgressProgressBar.Maximum != FTPDownloadFilesize)
                ProgressProgressBar.Maximum = FTPDownloadFilesize;
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
                Logging.Editor("OpenFolder button pressed but path {0} does not exist!", LogLevel.Info, Path.GetDirectoryName(ZipFilePathDisk));
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
                Logging.Editor("OpenFile button pressed but file {0} does not exist!", LogLevel.Info, ZipFilePathDisk);
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
            Logging.Editor("Cancel pressed, Upload={0}", LogLevel.Info, Upload.ToString());
            ProgressHeader.Text = "Canceled";
            Logging.Editor("Canceling upload or download operation");
            client.CancelAsync();
        }
    }
}
