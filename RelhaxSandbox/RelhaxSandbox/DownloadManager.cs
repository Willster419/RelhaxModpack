using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxSandbox
{
    //public delegate void OnDownloadProgressDelegate(object sender, DownloadProgressChangedEventArgs e);

    public class DownloadManager : IDisposable
    {
        public const int BYTE_CHUNKS = 4096;
        //public event OnDownloadProgressDelegate OnDownloadProgress;

        protected WebClient webClient;
        protected Stream stream;
        protected MD5 md5Hash;
        protected FileStream filestream;
        public string Hash { get; protected set; } = string.Empty;

        public int RetryCount { get; set; } = 3;

        public DownloadManager()
        {
            webClient = new WebClient();
            md5Hash = MD5.Create();
        }

        public void DownloadPackages(string downloadLink, string downloadLocation)
        {
            stream = webClient.OpenRead(downloadLink);
            if (File.Exists(downloadLocation)) File.Delete(downloadLocation);
            filestream = new FileStream(downloadLocation, FileMode.Create, FileAccess.Write);
            byte[] buffer = new byte[BYTE_CHUNKS];

            while(true)
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

                    filestream.Close();
                    md5Hash.Clear();
                    stream.Close();
                    break;
                }
                else
                {
                    md5Hash.TransformBlock(buffer, 0, BYTE_CHUNKS, buffer, 0);
                    filestream.Write(buffer, 0, BYTE_CHUNKS);
                }
            }
        }

        public void Dispose()
        {
            ((IDisposable)webClient).Dispose();
            ((IDisposable)stream).Dispose();
            ((IDisposable)md5Hash).Dispose();
            ((IDisposable)filestream).Dispose();
        }
    }
}
