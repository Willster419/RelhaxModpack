using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RelhaxModpack.Automation
{
    /// <summary>
    /// Provides the ability to browse web pages using the WebBrowser (trident) API.
    /// </summary>
    /// <seealso cref="WebBrowser"/>
    public class AutomationWebBrowser : WebBrowser, IAutomationBrowserSession, IAutomationBrowser, IDisposable
    {
        /// <summary>
        /// Occurs when download progress of a file has changed.
        /// </summary>
#pragma warning disable CS0067
        public event BrowserSessionManagerDelegate DownloadProgress;
#pragma warning restore CS0067

        /// <summary>
        /// Occurs when a web page has been fully downloaded and rendered by the browser.
        /// </summary>
        /// <remarks>This may fire multiple times if the loaded page has dynamic content (e.g. AJAX, javascript).</remarks>
        /// <seealso cref="NavigationCompleted"/>
        new public event BrowserManagerDelegate DocumentCompleted;

        /// <summary>
        /// Occurs when the web browser has finished navigating to the new content. This does not mean that the new content has been fully parsed.
        /// </summary>
        /// <seealso cref="DocumentCompleted"/>
        public event BrowserManagerDelegate NavigationCompleted;

        /// <summary>
        /// Returns this browser as a Control object.
        /// </summary>
        public Control Browser { get { return this; } }

        /// <summary>
        /// Creates an instance of the AutomationWebBrowser class.
        /// </summary>
        public AutomationWebBrowser() : base()
        {
            this.ScriptErrorsSuppressed = true;
        }

        /// <summary>
        /// Return the currently parsed html document from the browser implementation.
        /// </summary>
        /// <returns>The currently parsed html document as a string.</returns>
        /// <remarks>This implementation does not return the header, only the document body's outerHtml</remarks>
        /// <seealso cref="WebBrowser.Document"/>
        public string GetHtmlDocument()
        {
            return this.Document.Body.OuterHtml;
        }

        /// <summary>
        /// Cancels the current navigation operation.
        /// </summary>
        public void Cancel()
        {
            this.Stop();
        }

        /// <summary>
        /// This has not been implemented.
        /// </summary>
        public void SetHeader(string name, string value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This has not been implemented.
        /// </summary>
        public void RemoveHeader(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Raises the NavigationCompleted event.
        /// </summary>
        /// <param name="e">The navigation completed event arguments.</param>
        protected override void OnNavigated(WebBrowserNavigatedEventArgs e)
        {
            base.OnNavigated(e);
            NavigationCompleted?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the DocumentCompleted event.
        /// </summary>
        /// <param name="e">The document completed event arguments.</param>
        protected override void OnDocumentCompleted(WebBrowserDocumentCompletedEventArgs e)
        {
            base.OnDocumentCompleted(e);
            DocumentCompleted?.Invoke(this, e);
        }

        /// <summary>
        /// This has not been implemented.
        /// </summary>
        public async Task DownloadFileAsync(string url, string filepath)
        {
            await DownloadFileAsync(url, filepath, default);
        }

        /// <summary>
        /// This has not been implemented.
        /// </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task DownloadFileAsync(string url, string filepath, CancellationToken token)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            throw new NotImplementedException("soon tm");
        }

        /// <summary>
        /// This has not been implemented.
        /// </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<string> GetRequestStringAsync(string url)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This has not been implemented.
        /// </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<string> PostRequestStringAsync(string url, string postData, string contentType)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            throw new NotImplementedException();
        }
    }
}
