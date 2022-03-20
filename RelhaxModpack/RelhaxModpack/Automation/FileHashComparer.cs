using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RelhaxModpack.UI;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack.Automation
{
    /// <summary>
    /// The FileHashComparer class enables comparison of 2 files by calculating if the MD5 hash values are equal.
    /// </summary>
    public class FileHashComparer
    {
        /// <summary>
        /// The maximum number of bytes to use for each part of the MD5 calculation.
        /// </summary>
        public const int BYTE_CHUNKS = 4096;

        /// <summary>
        /// The calculated MD5 hash of file A.
        /// </summary>
        public StringBuilder HashAStringBuilder { get; protected set; }

        /// <summary>
        /// The calculated MD5 hash of file B.
        /// </summary>
        public StringBuilder HashBStringBuilder { get; protected set; }

        /// <summary>
        /// Get if the calculation of file A was successful.
        /// </summary>
        public bool HashACalculated { get; protected set; }

        /// <summary>
        /// Get if the calculation of tile B was successful.
        /// </summary>
        public bool HashBCalculated { get; protected set; }

        /// <summary>
        /// Get or set the progress reporting object for file A.
        /// </summary>
        public IProgress<RelhaxProgress> ProgressA { get; set; }

        /// <summary>
        /// Get or set the progress reporting object for file B.
        /// </summary>
        public IProgress<RelhaxProgress> ProgressB { get; set; }

        /// <summary>
        /// Get or set the operation cancellation token for file A.
        /// </summary>
        public CancellationToken CancellationTokenA { get; set; }

        /// <summary>
        /// Get or set the operation cancellation token for file B.
        /// </summary>
        public CancellationToken CancellationTokenB { get; set; }

        /// <summary>
        /// The MD5 hash calculation objects for each file.
        /// </summary>
        protected MD5 md5HashA, md5HashB;

        /// <summary>
        /// The progress reporting objects to use for reporting progress for each file's calculation.
        /// </summary>
        protected RelhaxProgress hashProgressA, hashProgressB;

        /// <summary>
        /// Create an instance of the FileHashComparer class.
        /// </summary>
        public FileHashComparer()
        {
            hashProgressA = new RelhaxProgress() { ChildCurrentProgress = "barWithTextChild" };
            hashProgressB = new RelhaxProgress() { ChildCurrentProgress = "barWithTextChild" };
        }

        /// <summary>
        /// Compute the MD5 hash of file A from a location on disk.
        /// </summary>
        /// <param name="filenameA">The path to file A.</param>
        public async Task ComputeHashA(string filenameA)
        {
            if (!File.Exists(filenameA))
            {
                Logging.Error(LogOptions.ClassName, "The supplied file path {0} does not exist", filenameA);
                HashACalculated = false;
                return;
            }

            FileStream streamA;
            try
            {
                streamA = new FileStream(filenameA, FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex)
            {
                Logging.Exception("Failed to open file stream to file A {0}", filenameA);
                Logging.Exception(ex.ToString());
                HashACalculated = false;
                return;
            }
            await ComputeHashA(streamA);
            streamA.Dispose();
        }

        /// <summary>
        /// Compute the MD5 hash of file A from a Stream object.
        /// </summary>
        /// <param name="streamA">The stream to access file A.</param>
        public async Task ComputeHashA(Stream streamA)
        {
            HashAStringBuilder = new StringBuilder();
            HashACalculated = await ComputeHash(streamA, md5HashA, hashProgressA, ProgressA, HashAStringBuilder, CancellationTokenA);
        }

        /// <summary>
        /// Compute the MD5 hash of file B from a location on disk.
        /// </summary>
        /// <param name="filenameB">The path to file B.</param>
        public async Task ComputeHashB(string filenameB)
        {
            if (!File.Exists(filenameB))
            {
                Logging.Error(LogOptions.ClassName, "The supplied file path {0} does not exist", filenameB);
                HashBCalculated = false;
                return;
            }

            FileStream streamB;
            try
            {
                streamB = new FileStream(filenameB, FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex)
            {
                Logging.Exception("Failed to open files stream to file B {0}", filenameB);
                Logging.Exception(ex.ToString());
                HashBCalculated = false;
                return;
            }
            await ComputeHashB(streamB);
            streamB.Dispose();
        }

        /// <summary>
        /// Compute the MD5 hash of file B from a Stream object.
        /// </summary>
        /// <param name="streamB">The stream to access file B.</param>
        public async Task ComputeHashB(Stream streamB)
        {
            HashBStringBuilder = new StringBuilder();
            HashBCalculated = await ComputeHash(streamB, md5HashB, hashProgressB, ProgressB, HashBStringBuilder, CancellationTokenB);
        }

        /// <summary>
        /// Compute the MD5 hash of the given stream object and save the result to a StringBuilder.
        /// </summary>
        /// <param name="stream">The data stream to compute.</param>
        /// <param name="md5hash">The MD5 hash implementation.</param>
        /// <param name="progress">The progress object.</param>
        /// <param name="Reporter">The progress reporter object.</param>
        /// <param name="builder">The StringBuilder to hold the calculated hash value.</param>
        /// <param name="cancellationToken">The operation cancellation token.</param>
        /// <returns>True if the MD5 calculation succeeds, false otherwise.</returns>
        protected async Task<bool> ComputeHash(Stream stream, MD5 md5hash, RelhaxProgress progress, IProgress<RelhaxProgress> Reporter, StringBuilder builder, CancellationToken cancellationToken)
        {
            using (md5hash = MD5.Create())
            {
                byte[] buffer = new byte[BYTE_CHUNKS];
                int numBytesRead = 0;
                progress.ChildTotal = (int)stream.Length;
                progress.ChildCurrent = 0;
                Reporter?.Report(progress);

                try
                {
                    //we need to use TransformFinalBlock()
                    //for the final calculation rather then TransformBlock
                    while (true)
                    {
                        numBytesRead = await stream.ReadAsync(buffer, 0, BYTE_CHUNKS);
                        progress.ChildCurrent += numBytesRead;

                        if (stream.Position >= stream.Length)
                        {
                            md5hash.TransformFinalBlock(buffer, 0, numBytesRead);
                            break;
                        }
                        else
                        {
                            md5hash.TransformBlock(buffer, 0, numBytesRead, null, 0);
                        }

                        ThrowIfCancellationRequested(cancellationToken);
                        Reporter?.Report(progress);
                    }

                    //output final hash entry and save to Hash property
                    for (int i = 0; i < md5hash.Hash.Length; i++)
                    {
                        builder.Append(md5hash.Hash[i].ToString("x2"));
                    }

                    Logging.Info(LogOptions.ClassName, "Hash for stream {0} calculated to be {1}", stream, builder.ToString());
                    progress.ChildCurrent = progress.ChildTotal;
                    Reporter?.Report(progress);
                }
                catch (OperationCanceledException)
                {
                    Logging.Info("The calculation was canceled");
                    return false;
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex.ToString());
                    return false;
                }
            }
            return true;
        }

        private void ThrowIfCancellationRequested(CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
