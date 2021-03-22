using RelhaxModpack.Database;
using RelhaxModpack.UI;
using RelhaxModpack.Utilities;
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
    public class DownloadManager : IDisposable
    {
        public const int BYTE_CHUNKS = 4096;

        public string Hash { get; protected set; } = string.Empty;

        public int RetryCount { get; set; } = 3;

        public string UrlBase { get; set; }

        public string DownloadLocationBase { get; set; }

        public IProgress<RelhaxDownloadProgress> Progress { get; set; }

        public CancellationToken CancellationToken { get; set; }

        private WebClient webClient;
        private Stream stream;
        private MD5 md5Hash;
        private FileStream filestream;
        private RelhaxDownloadProgress downloadProgress;

        //RelhaxDownloadProgress params and exit codes TODO

        public DownloadManager()
        {
            webClient = new WebClient();
            md5Hash = MD5.Create();
            downloadProgress = new RelhaxDownloadProgress();
        }

        public async Task DownloadPackagesAsync(List<DatabasePackage> packagesToDownload)
        {
            if (string.IsNullOrEmpty(UrlBase))
            {
                throw new BadMemeException("UrlBase is empty/null");
            }

            if (string.IsNullOrEmpty(DownloadLocationBase))
            {
                throw new BadMemeException("DownloadLocationBase is empty/null");
            }

            if (Progress == null)
            {
                Logging.Warning("Progress is null, no progress will be reported for this download operation");
            }

            if (CancellationToken == null)
            {
                Logging.Warning("CancellationToken is null, no cancellations will be acknowledged for this download operation");
            }

            foreach (DatabasePackage package in packagesToDownload)
            {
                Logging.Debug(Utilities.Enums.LogOptions.ClassName, "Download of package {0} from formed URL {1}", package.PackageName, UrlBase + package.ZipFile);
                await DownloadPackageAsync(package);
            }
        }

        public async Task DownloadPackageAsync(DatabasePackage package)
        {
            string downloadUrl = UrlBase + package.ZipFile;
            string downloadLocation = Path.Combine(DownloadLocationBase, package.ZipFile);
            Logging.Debug("Delete {0} if exists", downloadLocation);
            if (File.Exists(downloadLocation))
                FileUtils.FileDelete(downloadLocation);

            ThrowIfCancellationRequested();
            Progress.Report(downloadProgress);

            //open the stream to the file to download. A fail here means that the file might not exist
            bool retry = true;
            while (retry)
            {
                for(int failCount = 1; failCount <= RetryCount; failCount++)
                {
                    try
                    {
                        using (stream = webClient.OpenRead(downloadUrl))
                        using (filestream = new FileStream(downloadLocation, FileMode.Create, FileAccess.Write))
                        {
                            byte[] buffer = new byte[BYTE_CHUNKS];
                            ThrowIfCancellationRequested();
                            Progress.Report(downloadProgress);

                            while (true)
                            {
                                int readBytes = stream.Read(buffer, 0, BYTE_CHUNKS);
                                if (readBytes < BYTE_CHUNKS)
                                {
                                    StringBuilder sBuilder = new StringBuilder();
                                    md5Hash.TransformFinalBlock(buffer, 0, readBytes);
                                    filestream.Write(buffer, 0, readBytes);

                                    for (int i = 0; i < md5Hash.Hash.Length; i++)
                                    {
                                        sBuilder.Append(md5Hash.Hash[i].ToString("x2"));
                                    }
                                    Hash = sBuilder.ToString();

                                    if ((!Hash.Equals("f") && (!Hash.Equals(package.CRC))))
                                    {
                                        //download failed, the hash doesn't match. try again
                                        if (failCount == 3)
                                        {
                                            //this is the third time the file download has failed. something is wrong here.
                                            throw new BadMemeException(string.Format("The file download hash failed to match. Downloaded = {0}, Database = {1}", Hash, package.CRC));
                                        }
                                    }
                                    else
                                    {
                                        if (Hash.Equals("f"))
                                        {
                                            Logging.Info("The hash was calculated to be {0}, but package reports {1} to mean it's a beta package that can change. Skipping saving to database.", Hash, package.CRC);
                                        }
                                        else
                                        {
                                            //save to local database cache file
                                            //TODO
                                        }
                                        ThrowIfCancellationRequested();
                                        Progress.Report(downloadProgress);

                                        filestream.Close();
                                        md5Hash.Clear();
                                        stream.Close();
                                        package.DownloadFlag = false;

                                        //set the failCount for loop to jump out and the userRetry to stop asking
                                        failCount = 4;
                                        retry = false;
                                    }
                                    break;
                                }
                                else
                                {
                                    md5Hash.TransformBlock(buffer, 0, BYTE_CHUNKS, buffer, 0);
                                    filestream.Write(buffer, 0, BYTE_CHUNKS);
                                    ThrowIfCancellationRequested();
                                    Progress.Report(downloadProgress);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (e is WebException webEx && webEx.Status == WebExceptionStatus.RequestCanceled)
                        {
                            Logging.Info("Download request canceled");
                            throw new OperationCanceledException(CancellationToken);
                        }
                        else if (e is OperationCanceledException opex)
                        {
                            Logging.Info("I meant to throw this");
                            throw opex;
                        }
                        else
                        {
                            Logging.Exception(e.ToString());
                            Logging.Info("FailCount: {0}", failCount);

                            if (failCount == RetryCount)
                            {
                                //3 strikes you're out
                                Logging.Error("Failed to download the file {0}", package.ZipFile);
                                string message = string.Format("{0} {1} \"{2}\" {3}",
                                    Translations.GetTranslatedString("failedToDownload1"), Environment.NewLine, package.ZipFile, Translations.GetTranslatedString("failedToDownload2"));
                                DialogResult result = MessageBox.Show(message, Translations.GetTranslatedString("failedToDownloadHeader"), MessageBoxButtons.RetryCancel);
                                switch (result)
                                {
                                    case DialogResult.Retry:
                                        //keep retry as true
                                        break;
                                    case DialogResult.Abort:
                                        ThrowIfCancellationRequested();
                                        Progress.Report(downloadProgress);
                                        break;
                                }
                            }
                        }
                    }
                }

            }
        }

        public void Dispose()
        {
            ((IDisposable)webClient).Dispose();
            ((IDisposable)md5Hash).Dispose();
            ((IDisposable)stream)?.Dispose();
            ((IDisposable)filestream)?.Dispose();
        }

        private void ThrowIfCancellationRequested()
        {
            if (CancellationToken != null)
                CancellationToken.ThrowIfCancellationRequested();
        }
    }
}
