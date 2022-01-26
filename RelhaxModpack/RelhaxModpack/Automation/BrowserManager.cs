using Microsoft.Toolkit.Forms.UI.Controls;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RelhaxModpack.Automation
{
    public delegate void BrowserManagerDelegate(object sender, EventArgs e);

    public class BrowserManager : IDisposable
    {
        public event BrowserManagerDelegate OnDocumentCompleted;

        public event BrowserManagerDelegate OnNavigationCompleted;

        private WebView webView;

        private WebBrowser webBrowser;

        private BrowserType browserType;

        private bool isLoaded = false;

        private bool isSubscribed = false;

        public BrowserManager(BrowserType  browserType)
        {
            this.browserType = browserType;
        }

        public BrowserManager(WebView webView)
        {
            if (webView == null)
                throw new NullReferenceException();

            this.browserType = BrowserType.WebView;
            this.webView = webView;
            isLoaded = true;
        }

        public BrowserManager(WebBrowser webBrowser)
        {
            if (webBrowser == null)
                throw new NullReferenceException();

            this.browserType = BrowserType.WebBrowser;
            this.webBrowser = webBrowser;
            isLoaded = true;
        }

        public BrowserType BrowserType
        { get { return browserType; } }

        public Control Browser
        {
            get
            {
                switch (browserType)
                {
                    case BrowserType.WebBrowser:
                        return webBrowser;
                    case BrowserType.WebView:
                        return webView;
                    case BrowserType.WebView2:
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public int Height
        {
            get
            {
                if (Browser == null)
                    throw new NullReferenceException();
                return Browser.Height;
            }
            set
            {
                if (Browser == null)
                    throw new NullReferenceException();
                Browser.Height = value;
            }
        }

        public int Width
        {
            get
            {
                if (Browser == null)
                    throw new NullReferenceException();
                return Browser.Width;
            }
            set
            {
                if (Browser == null)
                    throw new NullReferenceException();
                Browser.Width = value;
            }
        }

        public bool? IsDisposed
        { 
            get
            {
                if (Browser == null)
                    return null;
                else 
                    return Browser.IsDisposed;
            } 
        }

        public bool? IsLoaded
        {
            get
            {
                if (Browser == null)
                    return null;
                else
                    return isLoaded;
            }
        }

        public bool IsSubscribed
        {
            get
            {
                if (Browser == null)
                    return false;
                else
                    return isSubscribed;
            }
        }

        public string GetHtmlDocument()
        {
            switch (browserType)
            {
                case BrowserType.WebBrowser:
                    return webBrowser.Document.Body.OuterHtml;
                case BrowserType.WebView:
                    return webView.InvokeScript("eval", new string[] { "document.documentElement.outerHTML;" });
                case BrowserType.WebView2:
                default:
                    throw new NotImplementedException();
            }
        }

        public void Init(bool @override = false)
        {
            if (isLoaded && !@override)
                return;
            switch (browserType)
            {
                case BrowserType.WebBrowser:
                    webBrowser = new WebBrowser()
                    {
                        ScriptErrorsSuppressed = true
                    };
                    break;
                case BrowserType.WebView:
                    webView = new WebView()
                    {
                        IsScriptNotifyAllowed = false
                    };
                    break;
                case BrowserType.WebView2:
                default:
                    throw new NotImplementedException();
            }
            isLoaded = true;
        }

        public void Subscribe()
        {
            if (Browser == null)
                throw new NullReferenceException();

            if (isSubscribed)
            {
                Logging.Warning(LogOptions.ClassName, "Browser is alreay subscribed, ignoring request");
                return;
            }

            switch (browserType)
            {
                case BrowserType.WebBrowser:
                    webBrowser.Navigated += WebBrowser_Navigated;
                    webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
                    break;
                case BrowserType.WebView:
                    webView.NavigationCompleted += WebView_NavigationCompleted;
                    webView.DOMContentLoaded += WebView_DOMContentLoaded;
                    break;
                case BrowserType.WebView2:
                default:
                    throw new NotImplementedException();
            }
            isSubscribed = true;
        }

        public void Unsubscribe()
        {
            if (Browser == null)
                throw new NullReferenceException();

            if (!isSubscribed)
            {
                Logging.Warning(LogOptions.ClassName, "Browser is already unsubscribed, ignoring request");
                return;
            }

            switch (browserType)
            {
                case BrowserType.WebBrowser:
                    webBrowser.Navigated -= WebBrowser_Navigated;
                    webBrowser.DocumentCompleted -= WebBrowser_DocumentCompleted;
                    break;
                case BrowserType.WebView:
                    webView.NavigationCompleted -= WebView_NavigationCompleted;
                    webView.DOMContentLoaded -= WebView_DOMContentLoaded;
                    break;
                case BrowserType.WebView2:
                default:
                    throw new NotImplementedException();
            }
            isSubscribed = false;
        }

        private void WebView_DOMContentLoaded(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlDOMContentLoadedEventArgs e)
        {
            OnDocumentCompleted?.Invoke(this, e);
        }

        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            OnDocumentCompleted?.Invoke(this, e);
        }

        private void WebView_NavigationCompleted(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlNavigationCompletedEventArgs e)
        {
            OnNavigationCompleted?.Invoke(this, e);
        }

        private void WebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            OnNavigationCompleted?.Invoke(this, e);
        }

        public void Navigate(string url)
        {
            switch (browserType)
            {
                case BrowserType.WebBrowser:
                    webBrowser.Navigate(url);
                    break;
                case BrowserType.WebView:
                    webView.Navigate(url);
                    break;
                case BrowserType.WebView2:
                default:
                    throw new NotImplementedException();
            }
        }

        public void Cancel()
        {
            if (Browser == null)
                return;
            switch (browserType)
            {
                case BrowserType.WebBrowser:
                    webBrowser.Stop();
                    break;
                case BrowserType.WebView:
                    webView.Stop();
                    break;
                case BrowserType.WebView2:
                default:
                    throw new NotImplementedException();
            }
        }

        public void Cleanup()
        {
            Cancel();
            Dispose();
        }

        public void Dispose()
        {
            if (Browser != null)
                Unsubscribe();

            switch (browserType)
            {
                case BrowserType.WebBrowser:
                    if (webBrowser != null)
                    {
                        if (!webBrowser.IsDisposed)
                        {
                            webBrowser.Dispose();
                        }
                        webBrowser = null;
                    }
                    break;
                case BrowserType.WebView:
                    if (webView != null)
                    {
                        if (!webView.IsDisposed)
                        {
                            webView.Dispose();
                        }
                        webView = null;
                    }
                    break;
                case BrowserType.WebView2:
                default:
                    throw new NotImplementedException();
            }

            if (OnDocumentCompleted != null)
                OnDocumentCompleted = null;

            if (OnNavigationCompleted != null)
                OnNavigationCompleted = null;
        }
    }
}
