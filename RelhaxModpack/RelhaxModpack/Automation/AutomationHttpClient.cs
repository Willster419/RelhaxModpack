using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    /// <summary>
    /// Provides the ability to browse web pages using the headless implementation of the HttpClient API.
    /// </summary>
    /// <remarks>Headless means that there is no display interface, and thus, no javascript parsing.</remarks>
    /// <seealso cref="HttpClient"/>
    public class AutomationHttpClient : HttpClient, IAutomationBrowserSession, IDisposable
    {
        /// <summary>
        /// Occurs when download progress of a file has changed.
        /// </summary>
        public event BrowserSessionManagerDelegate DownloadProgress;

        /// <summary>
        /// The maximum number of bytes to download at a time during a download operation.
        /// </summary>
        protected const int BUFFER_SIZE = 4096;

        /// <summary>
        /// Create an instance of the AutomationHttpClient class.
        /// </summary>
        public AutomationHttpClient() : base()
        {

        }

        /// <summary>
        /// Create an instance of the AutomationHttpClient class.
        /// </summary>
        /// <param name="httpClientHandler">The HttpClientHandler to use for web requests.</param>
        public AutomationHttpClient(HttpClientHandler httpClientHandler) : base(httpClientHandler)
        {

        }

        /// <summary>
        /// Create an instance of the AutomationHttpClient class.
        /// </summary>
        /// <param name="httpClientHandler">The HttpClientHandler to use for web requests.</param>
        /// <param name="dispose">If true, dispose the handler when disposing this resource.</param>
        public AutomationHttpClient(HttpClientHandler httpClientHandler, bool dispose): base(httpClientHandler, dispose)
        {

        }

        /// <summary>
        /// Adds or updates a request header for all web requests.
        /// </summary>
        /// <param name="name">The name of the header.</param>
        /// <param name="value">The value of the header.</param>
        public void SetHeader(string name, string value)
        {
            this.DefaultRequestHeaders.Add(name, value);
        }

        /// <summary>
        /// Removes a request header from the browser implementation for all web requests.
        /// </summary>
        /// <param name="name">The name of the header.</param>
        public void RemoveHeader(string name)
        {
            this.DefaultRequestHeaders.Remove(name);
        }

        /// <summary>
        /// Sends an HTTP GET request to the given url.
        /// </summary>
        /// <param name="url">The url and GET parameters to use for the request.</param>
        /// <returns>The request's html response.</returns>
        public async Task<string> GetRequestStringAsync(string url)
        {
            HttpResponseMessage response = await this.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
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
            HttpContent httpContent = new StringContent(postData, Encoding.UTF8);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            HttpResponseMessage response = await this.PostAsync(url, httpContent);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Downloads a web resource as a file to a given path.
        /// </summary>
        /// <param name="url">The url of the resource to download.</param>
        /// <param name="filepath">The path to the destination file. The path can be relative or absolute.</param>
        public async Task DownloadFileAsync(string url, string filepath)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));
            if (string.IsNullOrEmpty(filepath))
                throw new ArgumentNullException(nameof(filepath));
            await DownloadFileAsync(url, filepath, default);
        }

        /// <summary>
        /// Downloads a web resource as a file to a given path.
        /// </summary>
        /// <param name="url">The url of the resource to download.</param>
        /// <param name="filepath">The path to the destination file. The path can be relative or absolute.</param>
        /// <param name="token">The cancellation token to enable cancellation of the operation.</param>
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
            int progressPercent;
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

        /// <summary>
        /// Creates an instance of the DownloadProgressChangedEventArgs class.
        /// </summary>
        /// <param name="progressPercentage">The percent of download progress.</param>
        /// <param name="userToken">A generic object to pass to subscribing classes.</param>
        /// <param name="bytesReceived">The total number of bytes that have been received so far in the download operation.</param>
        /// <param name="totalBytesToReceive">The total number of bytes that will be received in the download operation.</param>
        /// <returns></returns>
        /// <remarks>Why Microsoft thought it was a good idea to internalize this class and prevent others from easily using it is beyond me.</remarks>
        protected DownloadProgressChangedEventArgs CreateDownloadProgressChangedEventArgsInstance(int progressPercentage, object userToken, long bytesReceived, long totalBytesToReceive)
        {
            //https://social.msdn.microsoft.com/Forums/vstudio/en-US/71681799-5832-4296-b08c-2de45acf45f6/construct-class-with-internal-constructor
            //https://referencesource.microsoft.com/#System/net/System/Net/webclient.cs,959800b8c9dc738d
            //FOR REFERENCE:
            //internal DownloadProgressChangedEventArgs(int progressPercentage, object userToken, long bytesReceived, long totalBytesToReceive)
            ConstructorInfo[] constructors = typeof(DownloadProgressChangedEventArgs).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = new object[] { progressPercentage, userToken, bytesReceived, totalBytesToReceive };
            DownloadProgressChangedEventArgs args = (DownloadProgressChangedEventArgs)constructors[0].Invoke(parameters);
            return args;
        }
    }
}
