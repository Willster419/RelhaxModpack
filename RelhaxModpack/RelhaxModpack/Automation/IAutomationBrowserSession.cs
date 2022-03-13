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
    /// Delegate for event access for when file download progress changes.
    /// </summary>
    /// <param name="sender">The class instance that raised the event.</param>
    /// <param name="e">The download progress changed arguments to use in the consuming class.</param>
    public delegate void BrowserSessionManagerDelegate(object sender, DownloadProgressChangedEventArgs e);

    /// <summary>
    /// Provides an interface of common implementation methods used for different web browser APIs
    /// </summary>
    public interface IAutomationBrowserSession : IDisposable
    {
        /// <summary>
        /// Provides event access for other classes to subscribe to when file download progress changes.
        /// </summary>
        event BrowserSessionManagerDelegate DownloadProgress;

        /// <summary>
        /// Adds or updates a request header for all web requests.
        /// </summary>
        /// <param name="name">The name of the header.</param>
        /// <param name="value">The value of the header.</param>
        void SetHeader(string name, string value);

        /// <summary>
        /// Removes a request header from the browser implementation for all web requests.
        /// </summary>
        /// <param name="name">The name of the header.</param>
        void RemoveHeader(string name);

        /// <summary>
        /// Downloads a web resource as a file to a given path.
        /// </summary>
        /// <param name="url">The url of the resource to download.</param>
        /// <param name="filepath">The path to the destination file. The path can be relative or absolute.</param>
        Task DownloadFileAsync(string url, string filepath);

        /// <summary>
        /// Downloads a web resource as a file to a given path.
        /// </summary>
        /// <param name="url">The url of the resource to download.</param>
        /// <param name="filepath">The path to the destination file. The path can be relative or absolute.</param>
        /// <param name="token">The cancellation token to enable cancellation of the operation.</param>
        Task DownloadFileAsync(string url, string filepath, CancellationToken token);

        /// <summary>
        /// Sends an HTTP GET request to the given url.
        /// </summary>
        /// <param name="url">The url and GET parameters to use for the request.</param>
        /// <returns>The request's html response.</returns>
        Task<string> GetRequestStringAsync(string url);

        /// <summary>
        /// Sends an HTTP POST request to the given url.
        /// </summary>
        /// <param name="url">The url to send the POST request to.</param>
        /// <param name="postData">The post parameters.</param>
        /// <param name="contentType">The content type parameter of the post request</param>
        /// <returns>The request's html response.</returns>
        Task<string> PostRequestStringAsync(string url, string postData, string contentType);
    }
}
