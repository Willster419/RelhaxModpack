using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class BrowserSessionManager : IAutomationBrowserSession, IDisposable
    {
        public event BrowserSessionManagerDelegate DownloadProgress;

        protected IAutomationBrowserSession browserSession;

        public BrowserSessionManager(BrowserSessionType sessionType)
        {
            switch (sessionType)
            {
                case BrowserSessionType.WebClient:
                    browserSession = new AutomationWebClient();
                    break;
                case BrowserSessionType.HttpClient:
                    HttpClientHandler handler = new HttpClientHandler()
                    {
                        UseDefaultCredentials = true,
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                        UseCookies = true
                    };
                    browserSession = new AutomationHttpClient(handler, true);
                    break;
                case BrowserSessionType.WebBrowser:
                    browserSession = new AutomationWebBrowser();
                    break;
                case BrowserSessionType.WebView:
                    browserSession = new AutomationWebViewWrapper();
                    break;
                case BrowserSessionType.WebView2:
                    throw new NotImplementedException();
            }
            browserSession.DownloadProgress += (sender, args) => { DownloadProgress?.Invoke(this, args); };
        }

        private void BrowserSession_DownloadProgress(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            DownloadProgress?.Invoke(this, e);
        }

        public void SetHeader(string name, string value)
        {
            browserSession.SetHeader(name, value);
        }

        public void RemoveHeader(string name)
        {
            browserSession.RemoveHeader(name);
        }

        public async Task DownloadFileAsync(string url, string filepath)
        {
            await browserSession.DownloadFileAsync(url, filepath);
        }

        public async Task DownloadFileAsync(string url, string filepath, CancellationToken token)
        {
            await browserSession.DownloadFileAsync(url, filepath, token);
        }

        public async Task<string> GetRequestStringAsync(string url)
        {
            return await browserSession.GetRequestStringAsync(url);
        }

        public async Task<string> PostRequestStringAsync(string url, string postData, string contentType)
        {
            return await browserSession.PostRequestStringAsync(url, postData, contentType);
        }

        public void Dispose()
        {
            if (DownloadProgress != null)
                DownloadProgress = null;
            browserSession.Dispose();
        }
    }
}
