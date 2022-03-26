using Microsoft.Toolkit.Forms.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RelhaxModpack.Automation
{
    /// <summary>
    /// Provides the ability to browse web pages using the WebView browser (edge) API.
    /// </summary>
    /// <seealso cref="WebView"/>
    public class AutomationWebViewWrapper : IAutomationBrowserSession, IAutomationBrowser, IDisposable
    {
        /// <summary>
        /// The WebView instance.
        /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
        protected WebView webView;
#pragma warning restore CS0618 // Type or member is obsolete

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
        public event BrowserManagerDelegate DocumentCompleted;

        /// <summary>
        /// Occurs when the web browser has finished navigating to the new content. This does not mean that the new content has been fully parsed.
        /// </summary>
        /// <seealso cref="DocumentCompleted"/>
        public event BrowserManagerDelegate NavigationCompleted;

        /// <summary>
        /// Returns the wrapped WebView instance as a Control object.
        /// </summary>
        public Control Browser { get { return webView; } }

        /// <summary>
        /// Create an instance of the AutomationWebViewWrapper class.
        /// </summary>
        /// <remarks>When created, it is also creating the wrapped WebView instance.</remarks>
        public AutomationWebViewWrapper()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            webView = new WebView() { IsScriptNotifyAllowed = false, IsJavaScriptEnabled = true };
#pragma warning restore CS0618 // Type or member is obsolete
            try
            {
                ((ISupportInitialize)webView).BeginInit();
                ((ISupportInitialize)webView).EndInit();
            }
            catch
            { }
            webView.NavigationCompleted += WebView_NavigationCompleted;
            webView.DOMContentLoaded += WebView_DOMContentLoaded;
        }

        private void WebView_DOMContentLoaded(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlDOMContentLoadedEventArgs e)
        {
            DocumentCompleted?.Invoke(this, e);
        }

        private void WebView_NavigationCompleted(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlNavigationCompletedEventArgs e)
        {
            NavigationCompleted?.Invoke(this, e);
        }

        /// <summary>
        /// Get or set the height of the WebView instance.
        /// </summary>
        public int Height
        {
            get { return webView.Height; }
            set { webView.Height = value; }
        }

        /// <summary>
        /// Get or set the width of the WebView instance.
        /// </summary>
        public int Width
        {
            get { return webView.Width; }
            set { webView.Width = value; }
        }

        /// <summary>
        /// Return the currently parsed html document from the WebView instance.
        /// </summary>
        /// <returns>The currently parsed html document as a string.</returns>
        /// <remarks>This implementation does not return the header, only the document.documentElement.outerHTML</remarks>
        public string GetHtmlDocument()
        {
            return webView.InvokeScript("eval", new string[] { "document.documentElement.outerHTML;" });
        }

        /// <summary>
        /// Start a navigation to the given url.
        /// </summary>
        /// <param name="url">The resource to navigate to.</param>
        /// <remarks>In this browser implementation, the navigation is asynchronous and must be listened for with the document and navigation completed events.</remarks>
        public void Navigate(string url)
        {
            webView.Navigate(url);
        }

        /// <summary>
        /// Cancels navigation to a url resource.
        /// </summary>
        public void Cancel()
        {
            webView.Stop();
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
            throw new NotImplementedException();
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

        /// <summary>
        /// Release unmanaged resources by the WebView instance and closes all open events.
        /// </summary>
        public void Dispose()
        {
            ((IDisposable)webView).Dispose();
            if (DocumentCompleted != null)
                DocumentCompleted = null;

            if (NavigationCompleted != null)
                NavigationCompleted = null;
        }
    }
}
