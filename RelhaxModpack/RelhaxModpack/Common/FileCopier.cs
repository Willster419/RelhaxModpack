using RelhaxModpack.UI;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RelhaxModpack.Common
{
    public class FileCopier
    {
        public const int BYTE_CHUNKS = 4096;

        public IProgress<RelhaxProgress> Reporter { get; set; }

        public CancellationToken CancellationToken { get; set; }

        public string SourceFile { get; set; }

        public string DestinationFile { get; set; }

        protected RelhaxProgress progress;

        protected bool verbose = false;

        public FileCopier()
        {
            progress = new RelhaxProgress() { };
        }

        public FileCopier(RelhaxProgress progress)
        {
            this.progress = progress;
        }

        public FileCopier(string sourceFile, string destinationFile) : this()
        {
            this.SourceFile = sourceFile;
            this.DestinationFile = destinationFile;
        }

        public async Task<bool> CopyFileAsync(string sourceFile, string destinationFile)
        {
            this.SourceFile = sourceFile;
            this.DestinationFile = destinationFile;
            return await CopyFileAsync();
        }

        public async Task<bool> CopyFileAsync()
        {
            if (string.IsNullOrEmpty(SourceFile))
                throw new NullReferenceException("SourceFile is null or empty");

            if (string.IsNullOrEmpty(DestinationFile))
                throw new NullReferenceException("DestinationFile is null or empty");

            if (!File.Exists(SourceFile))
                throw new BadMemeException("SourceFile does not exist");

            try
            {
                if (verbose)
                    Logging.Info(LogOptions.ClassName, "Starting file copy operation");

                //copy a file, allowing shared mode (we can copy a file currently open)
                //https://stackoverflow.com/questions/6167136/how-to-copy-a-file-while-it-is-being-used-by-another-process#:~:text=Well%2C%20another%20option%20is%20to%20copy%20the%20locked,by%20another%20process%2C%20bypassing%20the%20C%23%20File.Copy%20problem.
                using (FileStream SourceStream = File.Open(SourceFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (FileStream DestinationStream = File.Open(DestinationFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    byte[] buffer = new byte[BYTE_CHUNKS];
                    int numBytesRead = 0;

                    if (progress != null)
                    {
                        progress.ChildTotal = (int)SourceStream.Length;
                        progress.ChildCurrent = 0;
                    }
                    Reporter?.Report(progress);

                    while (SourceStream.Position < SourceStream.Length)
                    {
                        numBytesRead = await SourceStream.ReadAsync(buffer, 0, BYTE_CHUNKS, CancellationToken);
                        await DestinationStream.WriteAsync(buffer, 0, numBytesRead, CancellationToken);

                        if (progress != null)
                            progress.ChildCurrent += numBytesRead;
                        ThrowIfCancellationRequested();
                        Reporter?.Report(progress);
                    }
                }

                //https://stackoverflow.com/a/1054274/3128017
                File.SetCreationTime(DestinationFile, File.GetCreationTime(SourceFile));
                File.SetLastAccessTime(DestinationFile, File.GetLastAccessTime(SourceFile));
                File.SetLastWriteTime(DestinationFile, File.GetLastWriteTime(SourceFile));

                if (verbose)
                    Logging.Info(LogOptions.ClassName, "The file copy operation completed");

                if (progress != null)
                    progress.ChildCurrent = progress.ChildTotal;
                Reporter?.Report(progress);
                return true;
            }
            catch (OperationCanceledException)
            {
                Logging.Info(LogOptions.ClassName, "The calculation was canceled");
                return false;
            }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
                return false;
            }
        }

        private void ThrowIfCancellationRequested()
        {
            if (CancellationToken != null)
                CancellationToken.ThrowIfCancellationRequested();
        }
    }
}
