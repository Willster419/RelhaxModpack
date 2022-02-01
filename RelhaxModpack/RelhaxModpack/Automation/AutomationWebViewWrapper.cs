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
    public class AutomationWebViewWrapper : IAutomationBrowserSession, IAutomationBrowser, IDisposable
    {
        protected WebView webView;

        public event BrowserSessionManagerDelegate DownloadProgress;

        public event BrowserManagerDelegate DocumentCompleted;

        public event BrowserManagerDelegate NavigationCompleted;

        public Control Browser { get { return webView; } }

        public AutomationWebViewWrapper()
        {
            webView = new WebView() { IsScriptNotifyAllowed = false, IsJavaScriptEnabled = true };
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

        public int Height
        {
            get { return webView.Height; }
            set { webView.Height = value; }
        }

        public int Width
        {
            get { return webView.Width; }
            set { webView.Width = value; }
        }

        public string GetHtmlDocument()
        {
            return webView.InvokeScript("eval", new string[] { "document.documentElement.outerHTML;" });
        }

        public void Navigate(string url)
        {
            webView.Navigate(url);
        }

        public void Cancel()
        {
            webView.Stop();
        }

        public void SetHeader(string name, string value)
        {
            throw new NotImplementedException();
        }

        public async Task DownloadFileAsync(string url, string filepath)
        {
            await DownloadFileAsync(url, filepath, default);
        }

        public async Task DownloadFileAsync(string url, string filepath, CancellationToken token)
        {
            throw new NotImplementedException("soon tm");
        }

        public async Task<string> GetRequestStringAsync(string url)
        {
            throw new NotImplementedException();
        }

        public async Task<string> PostRequestStringAsync(string url, string postData)
        {
            throw new NotImplementedException();
        }

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
