using RelhaxModpack.Database;
using RelhaxModpack.UI;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RelhaxModpack.Common
{
    /// <summary>
    /// The DownloadManager class provides an implementation to calculate a file's md5 hash as it downloads and compare it at the end to ensure a complete download.
    /// </summary>
    public class DownloadManager : IDisposable
    {
        /// <summary>
        /// The maximum size of data to receive for each read/write operation.
        /// </summary>
        public const int BYTE_CHUNKS = 4096;

        /// <summary>
        /// The Md5 hash output result.
        /// </summary>
        public string Hash { get; protected set; } = string.Empty;

        /// <summary>
        /// The number of times that a failed download should be retried.
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// The url path to the download location. Typically, this is the folder path on the server up to the file itself to download.
        /// </summary>
        public string UrlBase { get; set; }

        /// <summary>
        /// The location to download the file to. Typically, this is a folder path on a disk up to the file itself.
        /// </summary>
        public string DownloadLocationBase { get; set; }

        /// <summary>
        /// A progress implementation for reporting progress operations back to a waiting thread.
        /// </summary>
        public IProgress<RelhaxDownloadProgress> Progress { get; set; }

        /// <summary>
        /// A cancellation token to allow for user cancellation of the async operation.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Gets if the instance has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; } = false;

        /// <summary>
        /// A synchronization mechanism that can allow a thread to wait until a download operation is completed.
        /// </summary>
        /// <remarks>This is used in the install engine, where a thread needs to wait until a download is completed before it can be extracted.</remarks>
        public ManualResetEvent ManualResetEvent;

        private WebClient webClient;
        private Stream stream;
        private MD5 md5Hash;
        private FileStream filestream;
        private RelhaxDownloadProgress downloadProgress;
        private Md5DatabaseManager databaseManager;

        /// <summary>
        /// Create an instance of the DownloadManager class.
        /// </summary>
        /// <remarks>A WebClient and Md5DatabaseManager are automatically created when this class is instanced.</remarks>
        public DownloadManager()
        {
            webClient = new WebClient();
            databaseManager = new Md5DatabaseManager();
        }

        /// <summary>
        /// Start a download operation for a list of packages.
        /// </summary>
        /// <param name="packagesToDownload">The list of packages to download.</param>
        /// <returns>The task object that holds the return data and context.</returns>
        public Task DownloadPackagesAsync(List<DatabasePackage> packagesToDownload)
        {
            if (string.IsNullOrEmpty(UrlBase))
            {
                throw new BadMemeException("UrlBase is empty/null");
            }

            if (string.IsNullOrEmpty(DownloadLocationBase))
            {
                throw new BadMemeException("DownloadLocationBase is empty/null");
            }

            if (packagesToDownload == null || packagesToDownload.Count == 0)
            {
                throw new BadMemeException("packagesToDownload is null or 0");
            }

            if (IsDisposed)
            {
                throw new ObjectDisposedException("DownloadManager");
            }

            if (Progress == null)
            {
                Logging.Info("Progress is null, no progress will be reported for this download operation");
            }

            if (CancellationToken == null)
            {
                Logging.Info("CancellationToken is null, no cancellations will be acknowledged for this download operation");
            }

            if (ManualResetEvent == null)
            {
                Logging.Info($"{nameof(ManualResetEvent)} is null, an awaiting thread cannot wait until a package is downloaded");
            }

            downloadProgress = new RelhaxDownloadProgress() { ParrentTotal = packagesToDownload.Count };

            return Task.Run(() => {
                //load md5 database manager before downloading packages
                if (!databaseManager.DatabaseLoaded)
                    databaseManager.LoadMd5Database(ApplicationConstants.MD5HashDatabaseXmlFile);

                for (int i = 0; i < packagesToDownload.Count; i++)
                {
                    DatabasePackage package = packagesToDownload[i];
                    Logging.Info(LogOptions.ClassName, "Download {0} of {1}, package {2} start", i + 1, packagesToDownload.Count, package.PackageName);
                    Logging.Debug(LogOptions.ClassName, "Download of package {0} from formed URL {1}", package.PackageName, UrlBase + package.ZipFile);
                    DownloadPackage(package);
                    Logging.Info(LogOptions.ClassName, "Download {0} of {1}, package {2} finish", i + 1, packagesToDownload.Count, package.PackageName);
                }
            });
        }

        private void DownloadPackage(DatabasePackage package)
        {
            if (!databaseManager.DatabaseLoaded)
                databaseManager.LoadMd5Database(ApplicationConstants.MD5HashDatabaseXmlFile);

            string downloadUrl = UrlBase + package.ZipFile;
            string downloadLocation = Path.Combine(DownloadLocationBase, package.ZipFile);

            Logging.Debug("Delete {0} if exists", downloadLocation);
            if (File.Exists(downloadLocation))
                FileUtils.FileDelete(downloadLocation);

            ThrowIfCancellationRequested();
            downloadProgress.DatabasePackage = package;
            downloadProgress.ChildCurrent = downloadProgress.ChildTotal = 0;
            downloadProgress.DownloadProgressState = DownloadProgressState.None;
            Progress?.Report(downloadProgress);

            //open the stream to the file to download. A fail here means that the file might not exist
            bool retry = true;
            while (retry)
            {
                for(int failCount = 1; failCount <= RetryCount; failCount++)
                {
                    try
                    {
                        Logging.Debug(LogOptions.ClassName, "Opening download stream and file write stream for {0}", package.ZipFile);
                        using (stream = webClient.OpenRead(downloadUrl))
                        using (filestream = new FileStream(downloadLocation, FileMode.Create, FileAccess.Write))
                        using (md5Hash = MD5.Create())
                        {
                            byte[] buffer = new byte[BYTE_CHUNKS];
                            ThrowIfCancellationRequested();
                            downloadProgress.DownloadProgressState = DownloadProgressState.OpenStreams;
                            int totalBytesToDownload = CommonUtils.ParseInt(webClient.ResponseHeaders[HttpResponseHeader.ContentLength], 1);
                            int totalBytesDownloaded = 0;
                            downloadProgress.ChildTotal = totalBytesToDownload;
                            RelhaxDownloadProgress initProg = new RelhaxDownloadProgress()
                            {
                                DownloadProgressState = DownloadProgressState.OpenStreams,
                                ParrentTotal = downloadProgress.ParrentTotal,
                                DatabasePackage = downloadProgress.DatabasePackage,
                                ChildCurrent = downloadProgress.ChildCurrent,
                                ChildTotal = downloadProgress.ChildTotal,
                            };
                            Progress?.Report(initProg);

                            while (true)
                            {
                                int readBytes = stream.Read(buffer, 0, BYTE_CHUNKS);
                                totalBytesDownloaded += readBytes;
                                downloadProgress.ChildCurrent = totalBytesDownloaded;
                                if (totalBytesDownloaded >= totalBytesToDownload)
                                {
                                    //write and hash final segment
                                    md5Hash.TransformFinalBlock(buffer, 0, readBytes);
                                    filestream.Write(buffer, 0, readBytes);

                                    //report progress
                                    downloadProgress.DownloadProgressState = DownloadProgressState.Download;
                                    Progress?.Report(downloadProgress);

                                    //output final hash entry and save to Hash property
                                    StringBuilder sBuilder = new StringBuilder();
                                    for (int i = 0; i < md5Hash.Hash.Length; i++)
                                    {
                                        sBuilder.Append(md5Hash.Hash[i].ToString("x2"));
                                    }
                                    Hash = sBuilder.ToString();
                                    Logging.Info(LogOptions.ClassName, "Hash for package {0} calculated to be {1}", package.PackageName, Hash);

                                    //close streams and clear hash entry
                                    //This method calls Dispose, specifying true to release all resources. 
                                    //https://docs.microsoft.com/en-us/dotnet/api/system.io.stream.close?view=netframework-4.8
                                    filestream.Close();
                                    stream.Close();
                                    md5Hash.Clear();

                                    if ((!package.CRC.Equals("f") && (!Hash.Equals(package.CRC))))
                                    {
                                        //download failed, the hash doesn't match. try again
                                        if (failCount == 3)
                                        {
                                            //this is the third time the file download has failed. something is wrong here.
                                            throw new BadMemeException(string.Format("The file download hash failed to match. Downloaded = {0}, Database = {1}", Hash, package.CRC));
                                        }
                                        else
                                        {
                                            Logging.Warning("The file download hash for zip file {0} failed to match. Downloaded = {1}, Database = {2}, try {3} of {4}", package.ZipFile, Hash, package.CRC, failCount, RetryCount);
                                        }
                                    }
                                    else
                                    {
                                        //save to local database cache file, even if the crc is "f". The loading in the selection list will skip getting the hash if the crc is f
                                        databaseManager.UpdateFileEntry(package.ZipFile, File.GetLastWriteTime(downloadLocation), Hash);
                                        databaseManager.SaveMd5Database(ApplicationConstants.MD5HashDatabaseXmlFile);

                                        //set download flag as false since it was successfully downloaded and recorded
                                        package.DownloadFlag = false;

                                        //set the failCount for loop to jump out and the userRetry to stop asking
                                        failCount = 4;
                                        retry = false;

                                        //report final progress and check for cancel
                                        downloadProgress.ParrentCurrent++;
                                        downloadProgress.DownloadProgressState = DownloadProgressState.DownloadCompleted;
                                        Progress?.Report(downloadProgress);
                                        ThrowIfCancellationRequested();

                                        if (ManualResetEvent != null)
                                        {
                                            ManualResetEvent.Set();
                                            ManualResetEvent.Reset();
                                        }
                                    }
                                    break;
                                }
                                else
                                {
                                    md5Hash.TransformBlock(buffer, 0, readBytes, null, 0);
                                    filestream.Write(buffer, 0, readBytes);
                                    ThrowIfCancellationRequested();
                                    downloadProgress.DownloadProgressState = DownloadProgressState.Download;
                                    Progress?.Report(downloadProgress);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (e is WebException webEx && webEx.Status == WebExceptionStatus.RequestCanceled)
                        {
                            Logging.Info(LogOptions.ClassName, "Download process canceled");
                            throw new OperationCanceledException(CancellationToken);
                        }
                        else if (e is OperationCanceledException opex)
                        {
                            Logging.Info(LogOptions.ClassName, "Download process canceled");
                            throw opex;
                        }
                        else
                        {
                            Logging.Exception(e.ToString());
                            Logging.Info("FailCount: {0}", failCount);

                            if (failCount == RetryCount)
                            {
                                Logging.Error("Failed to download the file {0}", package.ZipFile);
                                string message = string.Format("{0} {1} \"{2}\" {3}",
                                    Translations.GetTranslatedString("failedToDownload1"), Environment.NewLine, package.ZipFile, Translations.GetTranslatedString("failedToDownload2"));
                                DialogResult result = MessageBox.Show(message, Translations.GetTranslatedString("failedToDownloadHeader"), MessageBoxButtons.RetryCancel);
                                switch (result)
                                {
                                    case DialogResult.Retry:
                                        //keep retry as true
                                        break;
                                    case DialogResult.Cancel:
                                        throw new OperationCanceledException(CancellationToken);
                                }
                            }
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Releases all unmanaged resources used by the instance.
        /// </summary>
        public void Dispose()
        {
            ((IDisposable)webClient).Dispose();
            ((IDisposable)md5Hash)?.Dispose();
            ((IDisposable)stream)?.Dispose();
            ((IDisposable)filestream)?.Dispose();
            IsDisposed = true;
        }

        private void ThrowIfCancellationRequested()
        {
            if (CancellationToken != null)
                CancellationToken.ThrowIfCancellationRequested();
        }
    }
}
