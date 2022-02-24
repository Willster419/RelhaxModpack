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
    /// <summary>
    /// The FileCopier class provides an implementation to, as you may have guessed, copy files. Provides progress and cancellation to a user thread.
    /// </summary>
    public class FileCopier
    {
        /// <summary>
        /// The maximum size of data to receive for each read/write operation.
        /// </summary>
        public const int BYTE_CHUNKS = 4096;

        /// <summary>
        /// A progress implementation for reporting progress operations back to a waiting thread.
        /// </summary>
        public IProgress<RelhaxProgress> Reporter { get; set; }

        /// <summary>
        /// A cancellation token to allow for user cancellation of the async operation.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// The complete path to the file to copy.
        /// </summary>
        public string SourceFile { get; set; }

        /// <summary>
        /// The complete path of the destination file to copy to.
        /// </summary>
        public string DestinationFile { get; set; }

        /// <summary>
        /// The object to use to report progress with to the user thread.
        /// </summary>
        protected RelhaxProgress progress;

        /// <summary>
        /// Create an instance of the FileCopier class.
        /// </summary>
        public FileCopier()
        {
            progress = new RelhaxProgress() { };
        }

        /// <summary>
        /// Create an instance of the FileCopier class.
        /// </summary>
        /// <param name="progress">The object to use to report progress to the user thread.</param>
        public FileCopier(RelhaxProgress progress)
        {
            this.progress = progress;
        }

        /// <summary>
        /// Create an instance of the FilelCopier class.
        /// </summary>
        /// <param name="sourceFile">The path to file to copy from.</param>
        /// <param name="destinationFile">The path to the file to write to.</param>
        public FileCopier(string sourceFile, string destinationFile) : this()
        {
            this.SourceFile = sourceFile;
            this.DestinationFile = destinationFile;
        }

        /// <summary>
        /// Start a copy operation.
        /// </summary>
        /// <param name="sourceFile">The path to file to copy from.</param>
        /// <param name="destinationFile">The path to the file to write to.</param>
        /// <returns>True if the file copy succeeded, false otherwise.</returns>
        public async Task<bool> CopyFileAsync(string sourceFile, string destinationFile)
        {
            this.SourceFile = sourceFile;
            this.DestinationFile = destinationFile;
            return await CopyFileAsync();
        }

        /// <summary>
        /// Start a copy operation.
        /// </summary>
        /// <returns>True if the file copy succeeded, false otherwise.</returns>
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
                Logging.Debug(LogOptions.ClassName, "Starting file copy operation");
                Logging.Debug(LogOptions.ClassName, $"Source: {SourceFile}");
                Logging.Debug(LogOptions.ClassName, $"Destination: {DestinationFile}");

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

                Logging.Debug(LogOptions.ClassName, "The file copy operation completed");

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
