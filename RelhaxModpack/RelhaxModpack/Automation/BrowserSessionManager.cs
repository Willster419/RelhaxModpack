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
    /// <summary>
    /// A BrowserSessionManager provides an implementation to manage a browser of any API by providing common methods that can be mapped to automation tasks (e.g. browser_session_set_header).
    /// </summary>
    public class BrowserSessionManager : IAutomationBrowserSession, IDisposable
    {
        /// <summary>
        /// Occurs when there is progress in the operation of downloading a file  to disk.
        /// </summary>
        public event BrowserSessionManagerDelegate DownloadProgress;

        /// <summary>
        /// The wrapped/managed browser object.
        /// </summary>
        protected IAutomationBrowserSession browserSession;

        /// <summary>
        /// Create an instance of the BrowserSessionManager class.
        /// </summary>
        /// <param name="sessionType">The type of browser to create (while browser API to use).</param>
        /// <seealso cref="BrowserSessionType"/>
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

        /// <summary>
        /// Adds or updates a request header for all web requests in the browser.
        /// </summary>
        /// <param name="name">The name of the header.</param>
        /// <param name="value">The value of the header.</param>
        public void SetHeader(string name, string value)
        {
            browserSession.SetHeader(name, value);
        }

        /// <summary>
        /// Removes a request header from the browser for all web requests.
        /// </summary>
        /// <param name="name">The name of the header.</param>
        public void RemoveHeader(string name)
        {
            browserSession.RemoveHeader(name);
        }

        /// <summary>
        /// Instructs the browser to download a web resource as a file to a given path.
        /// </summary>
        /// <param name="url">The url of the resource to download.</param>
        /// <param name="filepath">The path to the destination file. The path can be relative or absolute.</param>
        public async Task DownloadFileAsync(string url, string filepath)
        {
            await browserSession.DownloadFileAsync(url, filepath);
        }

        /// <summary>
        /// Instructs the browser to download a web resource as a file to a given path.
        /// </summary>
        /// <param name="url">The url of the resource to download.</param>
        /// <param name="filepath">The path to the destination file. The path can be relative or absolute.</param>
        /// <param name="token">The cancellation token to enable cancellation of the operation.</param>
        public async Task DownloadFileAsync(string url, string filepath, CancellationToken token)
        {
            await browserSession.DownloadFileAsync(url, filepath, token);
        }

        /// <summary>
        /// Instructs the browser to send an HTTP GET request to the given url.
        /// </summary>
        /// <param name="url">The url and GET parameters to use for the request.</param>
        /// <returns>The request's html response.</returns>
        public async Task<string> GetRequestStringAsync(string url)
        {
            return await browserSession.GetRequestStringAsync(url);
        }

        /// <summary>
        /// Instructs the browser to send an HTTP POST request to the given url.
        /// </summary>
        /// <param name="url">The url to send the POST request to.</param>
        /// <param name="postData">The post parameters.</param>
        /// <param name="contentType">The content type parameter of the post request</param>
        /// <returns>The request's html response.</returns>
        public async Task<string> PostRequestStringAsync(string url, string postData, string contentType)
        {
            return await browserSession.PostRequestStringAsync(url, postData, contentType);
        }

        /// <summary>
        /// Releases unmanaged objects from the browser.
        /// </summary>
        public void Dispose()
        {
            if (DownloadProgress != null)
                DownloadProgress = null;
            browserSession.Dispose();
        }
    }
}
