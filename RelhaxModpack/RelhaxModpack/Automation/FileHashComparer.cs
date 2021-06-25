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
    public class FileHashComparer
    {
        public const int BYTE_CHUNKS = 4096;

        public StringBuilder StreamAHash { get; protected set; }

        public StringBuilder StreamBHash { get; protected set; }

        public bool HashACalculated { get; protected set; }

        public bool HashBCalculated { get; protected set; }

        public IProgress<RelhaxProgress> ProgressA { get; set; }

        public IProgress<RelhaxProgress> ProgressB { get; set; }

        public CancellationToken CancellationTokenA { get; set; }

        public CancellationToken CancellationTokenB { get; set; }

        protected MD5 md5HashA, md5HashB;

        protected RelhaxProgress hashProgressA, hashProgressB;

        public FileHashComparer()
        {
            hashProgressA = new RelhaxProgress();
            hashProgressB = new RelhaxProgress();
        }

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
                Logging.Exception("Failed to open filestream to file A {0}", filenameA);
                Logging.Exception(ex.ToString());
                HashACalculated = false;
                return;
            }
            await ComputeHashA(streamA);
            streamA.Dispose();
        }

        public async Task ComputeHashA(Stream streamA)
        {
            HashACalculated = await ComputeHash(streamA, md5HashA, hashProgressA, ProgressA, "A", StreamAHash, CancellationTokenA);
        }

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
                Logging.Exception("Failed to open filestream to file B {0}", filenameB);
                Logging.Exception(ex.ToString());
                HashBCalculated = false;
                return;
            }
            await ComputeHashB(streamB);
            streamB.Dispose();
        }

        public async Task ComputeHashB(Stream streamB)
        {
            HashBCalculated = await ComputeHash(streamB, md5HashB, hashProgressB, ProgressB, "B", StreamBHash, CancellationTokenB);
        }

        protected async Task<bool> ComputeHash(Stream stream, MD5 md5hash, RelhaxProgress progress, IProgress<RelhaxProgress> Reporter, string streamName, StringBuilder builder, CancellationToken cancellationToken)
        {
            using (md5hash = MD5.Create())
            {
                byte[] buffer = new byte[BYTE_CHUNKS];
                int numBytesRead = 0;
                progress.ChildTotal = (int)stream.Length;
                progress.ChildCurrent = 0;
                Reporter.Report(progress);

                try
                {
                    while (stream.Position < stream.Length)
                    {
                        numBytesRead = await stream.ReadAsync(buffer, 0, BYTE_CHUNKS);
                        progress.ChildCurrent += numBytesRead;
                        md5hash.TransformBlock(buffer, 0, numBytesRead, buffer, 0);

                        Reporter.Report(progress);
                        ThrowIfCancellationRequested(cancellationToken);
                    }

                    md5hash.TransformFinalBlock(buffer, 0, numBytesRead);

                    //output final hash entry and save to Hash property
                    builder = new StringBuilder();
                    for (int i = 0; i < md5hash.Hash.Length; i++)
                    {
                        builder.Append(md5hash.Hash[i].ToString("x2"));
                    }
                    Logging.Info(LogOptions.ClassName, "Hash for stream {0} calculated to be {1}", stream, StreamAHash.ToString());
                    Reporter.Report(progress);
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
