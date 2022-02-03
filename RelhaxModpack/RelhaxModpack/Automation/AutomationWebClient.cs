using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class AutomationWebClient : WebClient, IAutomationBrowserSession, IDisposable
    {
        public event BrowserSessionManagerDelegate DownloadProgress;

        protected CancellationToken cancellationToken;

        //enable sharing of cookies between connections
        protected CookieContainer cookieContainer = new CookieContainer();

        //https://stackoverflow.com/questions/34323143/downloading-large-google-drive-files-with-webclient-in-c-sharp
        protected override WebRequest GetWebRequest(Uri u)
        {
            var r = (HttpWebRequest)base.GetWebRequest(u);
            r.CookieContainer = cookieContainer;
            return r;
        }

        protected override void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
        {
            base.OnDownloadProgressChanged(e);

            if (cancellationToken != null && cancellationToken != default)
                cancellationToken.ThrowIfCancellationRequested();

            if (this.CanRaiseEvents)
                DownloadProgress?.Invoke(this, e);
        }

        public void SetHeader(string name, string value)
        {
            this.Headers.Add(name, value);
        }

        public void RemoveHeader(string name)
        {
            this.Headers.Remove(name);
        }

        public async Task DownloadFileAsync(string url, string filepath)
        {
            await this.DownloadFileTaskAsync(url, filepath);
        }

        public async Task DownloadFileAsync(string url, string filepath, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                throw new OperationCanceledException();

            cancellationToken = token;

            await this.DownloadFileTaskAsync(url, filepath);
            cancellationToken = default;
        }

        public async Task<string> GetRequestStringAsync(string url)
        {
            return await this.DownloadStringTaskAsync(url);
        }

        public async Task<string> PostRequestStringAsync(string url, string postData, string contentType)
        {
            string oldContent = this.Headers.Get("Content-Type");
            this.Headers[HttpRequestHeader.ContentType] = contentType;
            string result = await this.UploadStringTaskAsync(url, "POST", postData);
            if (string.IsNullOrEmpty(oldContent))
                this.Headers.Remove("Content-Type");
            else
                this.Headers[HttpRequestHeader.ContentType] = oldContent;
            return result;
        }
    }
}
