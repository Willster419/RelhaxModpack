using System;
using System.Linq;
using System.Windows;
using System.Net;
using System.IO;
using System.Windows.Threading;
using RelhaxModpack.Utilities;
using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Utilities.ClassEventArgs;
using RelhaxModpack.Settings;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// The delegate for invocation of when the FTP upload or download finishes
    /// </summary>
    /// <param name="sender">The sending object</param>
    /// <param name="e">The Upload or download event arguments</param>
    public delegate void EditorTransferWindowClosed(object sender, EditorTransferEventArgs e);

    /// <summary>
    /// Interaction logic for DatabaseEditorTrnasferWindow.xaml
    /// </summary>
    public partial class DatabaseEditorTransferWindow : RelhaxWindow
    {
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
        /// Enumeration flag to indicate uploading or downloading
        /// </summary>
        public EditorTransferMode TransferMode = EditorTransferMode.DownloadZip;

        /// <summary>
        /// The package being updated. A null package with Upload=true indicates the item being uploaded is a media
        /// </summary>
        public DatabasePackage PackageToUpdate;

        /// <summary>
        /// The event callback used for the editor when an upload or download is finished
        /// </summary>
        public event EditorTransferWindowClosed OnEditorUploadDownloadClosed;

        /// <summary>
        /// The timeout, in seconds, until the window will automatically close
        /// </summary>
        public uint Countdown = 0;

        /// <summary>
        /// The reference to the editor settings object. Must be supplied
        /// </summary>
        public EditorSettings EditorSettings = null;

        /// <summary>
        /// Get or set the name of the FTP folder to use for logging reporting and user display.
        /// </summary>
        /// <remarks>The full path to the file on disk and on the FTP server are handled by other variables</remarks>
        /// <seealso cref="ZipFilePathDisk"/>
        /// <seealso cref="ZipFilePathOnline"/>
        public string WoTModpackOnlineFolderVersion { get; set; }

        private WebClient client = null;
        private string CompleteFTPPath = string.Empty;
        private long FTPDownloadFilesize = -1;
        private DispatcherTimer timer = null;

        /// <summary>
        /// Create an instance of the DatabaseEditorDownlaod class
        /// </summary>
        public DatabaseEditorTransferWindow(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
        }

        private async void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (EditorSettings == null)
                throw new NullReferenceException();

            //init timer
            timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, Timer_Elapsed, this.Dispatcher) { IsEnabled = false };

            //setup UI based on transfer type
            OpenFodlerButton.Visibility = (TransferMode == EditorTransferMode.DownloadZip) ? Visibility.Visible : Visibility.Hidden;
            OpenFileButton.Visibility = (TransferMode == EditorTransferMode.DownloadZip) ? Visibility.Visible : Visibility.Hidden;

            //set log based on file upload type
            switch (TransferMode)
            {
                case EditorTransferMode.UploadZip:
                    ProgressBody.Text = string.Format("Uploading {0} to FTP folder {1}", Path.GetFileName(ZipFilePathDisk), WoTModpackOnlineFolderVersion);
                    break;
                case EditorTransferMode.UploadMedia:
                    ProgressBody.Text = string.Format("Uploading {0} to FTP folder Medias/...", ZipFileName);
                    break;
                case EditorTransferMode.DownloadZip:
                    ProgressBody.Text = string.Format("Downloading {0} from FTP folder {1}", Path.GetFileName(ZipFilePathDisk), WoTModpackOnlineFolderVersion);
                    break;
            }

            //set initial text(s)
            ProgressHeader.Text = string.Format("{0} 0 kb of 0 kb", TransferMode.ToString());
            CompleteFTPPath = string.Format("{0}{1}", ZipFilePathOnline, ZipFileName);

            using (client = new WebClient() { Credentials=Credential })
            {
                switch(TransferMode)
                {
                    case EditorTransferMode.UploadZip:
                    case EditorTransferMode.UploadMedia:
                        //before uploading, make sure it doesn't exist first (zipfile or media)
                        ProgressHeader.Text = "Checking if file exists on server...";
                        Logging.Editor("Checking if {0} already exists on the server in folder {1}", LogLevel.Info, ZipFileName, WoTModpackOnlineFolderVersion);
                        string[] listOfFilesOnServer = await FtpUtils.FtpListFilesFoldersAsync(ZipFilePathOnline, Credential);
                        if (listOfFilesOnServer.Contains(ZipFileName) && MessageBox.Show("File already exists, overwrite?", "", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                        {
                            Logging.Editor("DOES exist and user said don't overwrite, aborting");
                            ProgressHeader.Text = "Canceled";
                            return;
                        }

                        //attach upload event handlers
                        client.UploadProgressChanged += Client_UploadProgressChanged;
                        client.UploadFileCompleted += Client_DownloadUploadFileCompleted;

                        //run the FTP upload
                        Logging.Editor("Starting FTP upload of {0} from folder {1}", LogLevel.Info, ZipFileName, WoTModpackOnlineFolderVersion);
                        try
                        {
                            await client.UploadFileTaskAsync(CompleteFTPPath, ZipFilePathDisk);
                            Logging.Editor("FTP upload complete of {0}", LogLevel.Info, ZipFileName);

                            //run upload event handler
                            OnEditorUploadDownloadClosed?.Invoke(this, new EditorTransferEventArgs()
                            {
                                Package = PackageToUpdate,
                                UploadedFilename = ZipFileName,
                                UploadedFilepathOnline = ZipFilePathOnline,
                                TransferMode = this.TransferMode
                            });
                            DeleteFileButton.IsEnabled = true;

                            if (TransferMode == EditorTransferMode.UploadZip && EditorSettings.DeleteUploadLocallyUponCompletion)
                            {
                                DeleteFileButton_Click(null, null);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logging.Editor("FTP UPLOAD Failed");
                            Logging.Editor(ex.ToString());
                        }
                        finally
                        {
                            CancelButton.IsEnabled = false;
                        }
                        break;
                    case EditorTransferMode.DownloadZip:
                        //attach download event handlers
                        client.DownloadProgressChanged += Client_DownloadProgressChanged;
                        client.DownloadFileCompleted += Client_DownloadUploadFileCompleted;

                        //run the FTP download
                        Logging.Editor("Starting FTP download of {0} from folder {1}", LogLevel.Info, ZipFileName, WoTModpackOnlineFolderVersion);
                        try
                        {
                            FTPDownloadFilesize = await FtpUtils.FtpGetFilesizeAsync(CompleteFTPPath, Credential);
                            await client.DownloadFileTaskAsync(CompleteFTPPath, ZipFilePathDisk);
                            Logging.Editor("FTP download complete of {0}", LogLevel.Info, ZipFileName);
                            DeleteFileButton.IsEnabled = true;
                        }
                        catch (Exception ex)
                        {
                            Logging.Editor("FTP download failed of {0}", LogLevel.Info, ZipFileName);
                            Logging.Editor(ex.ToString());
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
                TimeoutClose.Text = string.Format("Closing window in {0}", Countdown.ToString());
            }
        }

        private void Timer_Elapsed(object sender, EventArgs e)
        {
            TimeoutClose.Text = string.Format("Closing window in {0}", (--Countdown).ToString());
            if (Countdown == 0)
            {
                Logging.Editor("Countdown complete, closing the window");
                timer.Stop();
                Close();
            }
        }

        private async void Client_DownloadUploadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if(e.Cancelled)
            {
                Logging.Editor("FTP upload or download cancel detected from UI thread, handling");
                switch (TransferMode)
                {
                    case EditorTransferMode.UploadZip:
                    case EditorTransferMode.UploadMedia:
                        Logging.Editor("Deleting file on server");
                        await FtpUtils.FtpDeleteFileAsync(CompleteFTPPath, Credential);
                        break;
                    case EditorTransferMode.DownloadZip:
                        Logging.Editor("Deleting file on disk");
                        File.Delete(ZipFilePathDisk);
                        break;
                }
                Close();
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //https://stackoverflow.com/questions/4591059/download-file-from-ftp-with-progress-totalbytestoreceive-is-always-1
            if (ProgressProgressBar.Maximum != FTPDownloadFilesize)
                ProgressProgressBar.Maximum = FTPDownloadFilesize;
            ProgressProgressBar.Value = e.BytesReceived;
            ProgressHeader.Text = string.Format("{0} {1} of {2}", "Downloaded",
                FileUtils.SizeSuffix((ulong)e.BytesReceived, 1, true, false), FileUtils.SizeSuffix((ulong)FTPDownloadFilesize, 1, true, false));
        }

        private void Client_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            if (ProgressProgressBar.Maximum != e.TotalBytesToSend)
                ProgressProgressBar.Maximum = e.TotalBytesToSend;
            ProgressProgressBar.Value = e.BytesSent;
            ProgressHeader.Text = string.Format("{0} {1} of {2}", "Uploaded",
                FileUtils.SizeSuffix((ulong)e.BytesSent, 1, true, false), FileUtils.SizeSuffix((ulong)e.TotalBytesToSend, 1, true, false));
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

        private void OpenFileButton_OpenFileClick(object sender, RoutedEventArgs e)
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
            Logging.Editor("Cancel pressed, TransferMode={0}", LogLevel.Info, TransferMode.ToString());
            ProgressHeader.Text = "Canceled";
            Logging.Editor("Canceling upload or download operation");
            client.CancelAsync();
        }

        private void DeleteFileButton_Click(object sender, RoutedEventArgs e)
        {
            switch (EditorSettings.UploadZipDeleteIsActuallyMove)
            {
                //true = move the file if the destination file exists
                case true:
                    OpenFileButton.IsEnabled = !DeleteIsMove();
                    break;
                //false = delete the file
                case false:
                    OpenFileButton.IsEnabled = !DeleteIsDelete();
                    break;
            }
        }

        private bool DeleteIsDelete()
        {
            Logging.Editor("{0} = {1}, deleting file", LogLevel.Info, nameof(EditorSettings.UploadZipDeleteIsActuallyMove), EditorSettings.UploadZipDeleteIsActuallyMove.ToString());
            try
            {
                File.Delete(ZipFilePathDisk);
                return true;
            }
            catch (Exception ex)
            {
                Logging.Editor(ex.ToString(), LogLevel.Exception);
                return false;
            }
        }

        private bool DeleteIsMove()
        {
            Logging.Editor("{0} = {1}, moving file to {2}", LogLevel.Info, nameof(EditorSettings.UploadZipDeleteIsActuallyMove), EditorSettings.UploadZipDeleteIsActuallyMove.ToString(), EditorSettings.UploadZipMoveFolder);
            if (Directory.Exists(EditorSettings.UploadZipMoveFolder))
            {
                try
                {
                    string destinationFile = Path.Combine(EditorSettings.UploadZipMoveFolder, Path.GetFileName(ZipFilePathDisk));
                    if (File.Exists(destinationFile))
                    {
                        Logging.Editor("Destination file {0} already exists, overriding", LogLevel.Info, destinationFile);
                        File.Delete(destinationFile);
                    }
                    File.Move(ZipFilePathDisk, destinationFile);
                    return true;
                }
                catch (Exception ex)
                {
                    Logging.Editor(ex.ToString(), LogLevel.Exception);
                }
            }
            else if (string.IsNullOrEmpty(EditorSettings.UploadZipMoveFolder))
            {
                Logging.Editor("Path for {0} is empty, no action taken", LogLevel.Warning, nameof(EditorSettings.UploadZipMoveFolder));
            }
            else
            {
                Logging.Editor("Path '{0}' does not exist, no action taken", LogLevel.Warning, EditorSettings.UploadZipMoveFolder);
            }
            return false;
        }
    }
}
