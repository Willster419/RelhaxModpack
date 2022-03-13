using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    /// <summary>
    /// Provides the ability to browse web pages using the headless implementation of the WebClient API.
    /// </summary>
    /// <remarks>Headless means that there is no display interface, and thus, no javascript parsing.</remarks>
    /// <seealso cref="WebClient"/>
    public class AutomationWebClient : WebClient, IAutomationBrowserSession, IDisposable
    {
        /// <summary>
        /// Occurs when download progress of a file has changed.
        /// </summary>
        public event BrowserSessionManagerDelegate DownloadProgress;

        /// <summary>
        /// The cancellation token to enable user cancellation of asynchronous operations.
        /// </summary>
        protected CancellationToken cancellationToken;

        /// <summary>
        /// A cookie manager to enable sharing of cookies between operations.
        /// </summary>
        protected CookieContainer cookieContainer = new CookieContainer();

        /// <summary>
        /// Returns an HTTPWebRequest object for the given url navigation, enabling sharing of cookies from previous requests.
        /// </summary>
        /// <param name="u">The url that is being navigated to.</param>
        /// <returns>The HttpWebRequest object.</returns>
        /// <seealso cref="HttpWebRequest"/>
        /// <seealso href="https://stackoverflow.com/questions/34323143/downloading-large-google-drive-files-with-webclient-in-c-sharp"/>
        protected override WebRequest GetWebRequest(Uri u)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(u);
            request.CookieContainer = cookieContainer;
            //https://dennymichael.net/2013/06/14/c-enable-automatic-decompression-on-system-net-webclient/
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            return request;
        }

        /// <summary>
        /// Raises the DownloadProgress event and can throw a cancel exception if the download operation was canceled by the user.
        /// </summary>
        /// <param name="e">The download progress changed argument instance</param>
        protected override void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
        {
            base.OnDownloadProgressChanged(e);

            if (cancellationToken != null && cancellationToken != default)
                cancellationToken.ThrowIfCancellationRequested();

            if (this.CanRaiseEvents)
                DownloadProgress?.Invoke(this, e);
        }

        /// <summary>
        /// Adds or updates a request header for all web requests.
        /// </summary>
        /// <param name="name">The name of the header.</param>
        /// <param name="value">The value of the header.</param>
        public void SetHeader(string name, string value)
        {
            this.Headers.Add(name, value);
        }

        /// <summary>
        /// Removes a request header from the browser implementation for all web requests.
        /// </summary>
        /// <param name="name">The name of the header.</param>
        public void RemoveHeader(string name)
        {
            this.Headers.Remove(name);
        }

        /// <summary>
        /// Downloads a web resource as a file to a given path.
        /// </summary>
        /// <param name="url">The url of the resource to download.</param>
        /// <param name="filepath">The path to the destination file. The path can be relative or absolute.</param>
        public async Task DownloadFileAsync(string url, string filepath)
        {
            await this.DownloadFileTaskAsync(url, filepath);
        }

        /// <summary>
        /// Downloads a web resource as a file to a given path.
        /// </summary>
        /// <param name="url">The url of the resource to download.</param>
        /// <param name="filepath">The path to the destination file. The path can be relative or absolute.</param>
        /// <param name="token">The cancellation token to enable cancellation of the operation.</param>
        public async Task DownloadFileAsync(string url, string filepath, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                throw new OperationCanceledException();

            cancellationToken = token;

            await this.DownloadFileTaskAsync(url, filepath);
            cancellationToken = default;
        }

        /// <summary>
        /// Sends an HTTP GET request to the given url.
        /// </summary>
        /// <param name="url">The url and GET parameters to use for the request.</param>
        /// <returns>The request's html response.</returns>
        public async Task<string> GetRequestStringAsync(string url)
        {
            return await this.DownloadStringTaskAsync(url);
        }

        /// <summary>
        /// Sends an HTTP POST request to the given url.
        /// </summary>
        /// <param name="url">The url to send the POST request to.</param>
        /// <param name="postData">The post parameters.</param>
        /// <param name="contentType">The content type parameter of the post request</param>
        /// <returns>The request's html response.</returns>
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
