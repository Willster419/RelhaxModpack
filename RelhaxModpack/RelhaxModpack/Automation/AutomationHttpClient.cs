using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class AutomationHttpClient : HttpClient, IAutomationBrowserSession, IDisposable
    {
        public event BrowserSessionManagerDelegate DownloadProgress;

        protected const int BUFFER_SIZE = 4096;

        public void SetHeader(string name, string value)
        {
            this.DefaultRequestHeaders.Add(name, value);
        }

        public async Task<string> GetRequestStringAsync(string url)
        {
            return await this.GetStringAsync(url);
        }

        public async Task<string> PostRequestStringAsync(string url, string postData)
        {
            return await this.PostRequestStringAsync(url, postData);
        }

        public async Task DownloadFileAsync(string url, string filepath)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));
            if (string.IsNullOrEmpty(filepath))
                throw new ArgumentNullException(nameof(filepath));
            await DownloadFileAsync(url, filepath, default);
        }

        public async Task DownloadFileAsync(string url, string filepath, CancellationToken token)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));
            if (string.IsNullOrEmpty(filepath))
                throw new ArgumentNullException(nameof(filepath));

            HttpResponseMessage response = await this.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            Stream fileDownload = await response.Content.ReadAsStreamAsync();
            long contentLength = (response.Content.Headers.ContentLength == null) ? 0 : (long)response.Content.Headers.ContentLength;
            int progressPercent = 0;
            int totalRead = 0;
            if (token != null && token != default)
                token.ThrowIfCancellationRequested();

            using (FileStream filestream = new FileStream(filepath, FileMode.Create))
            {
                byte[] buffer = new byte[BUFFER_SIZE];
                int ammountRead = await fileDownload.ReadAsync(buffer, 0, BUFFER_SIZE);

                while (ammountRead != 0)
                {
                    await filestream.WriteAsync(buffer, 0, ammountRead);
                    ammountRead = await fileDownload.ReadAsync(buffer, 0, BUFFER_SIZE);
                    totalRead += ammountRead;
                    progressPercent = (totalRead / (int)contentLength) * 100;
                    DownloadProgress?.Invoke(this, CreateDownloadProgressChangedEventArgsInstance(progressPercent, null, totalRead, contentLength));
                    if (token != null && token != default)
                        token.ThrowIfCancellationRequested();
                }
                await filestream.FlushAsync();
            }
        }

        protected DownloadProgressChangedEventArgs CreateDownloadProgressChangedEventArgsInstance(int progressPercentage, object userToken, long bytesReceived, long totalBytesToReceive)
        {
            //https://social.msdn.microsoft.com/Forums/vstudio/en-US/71681799-5832-4296-b08c-2de45acf45f6/construct-class-with-internal-constructor
            //https://referencesource.microsoft.com/#System/net/System/Net/webclient.cs,959800b8c9dc738d
            //internal DownloadProgressChangedEventArgs(int progressPercentage, object userToken, long bytesReceived, long totalBytesToReceive)
            ConstructorInfo[] constructors = typeof(DownloadProgressChangedEventArgs).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = new object[] { progressPercentage, userToken, bytesReceived, totalBytesToReceive };
            DownloadProgressChangedEventArgs args = (DownloadProgressChangedEventArgs)constructors[0].Invoke(parameters);
            return args;
        }
    }
}
