using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.XPath;
using HtmlAgilityPack;
using Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT;
using Microsoft.Toolkit.Forms.UI.Controls;
using RelhaxModpack.Common;
using RelhaxModpack.Utilities.Enums;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using System.ComponentModel;

namespace RelhaxModpack.Automation
{
    public class HtmlBrowserParser : HtmlWebscrapeParser
    {
        public int WaitTimeMs { get; set; } = 2000;

        public int WaitCounts { get; set; } = 3;

        public int BrowserWidth { get; set; } = 0;

        public int BrowserHeight { get; set; } = 0;

        public WebView Browser { get; set; }

        public bool ThreadMode { get; private set; }

        private int browserFinishedLoadingScriptsCounter;

        private bool browserContentLoaded;

        private bool browserNavigationCompleted;

        private bool browserFailed;

        private string lastLastUrl;

        private Dispatcher browserDispatcher;

        public HtmlBrowserParser() : base()
        {

        }

        public HtmlBrowserParser(string htmlpath, string url, int waitTimeMs, int waitCounts) : base(htmlpath, url)
        {
            this.WaitTimeMs = waitTimeMs;
            this.WaitCounts = waitCounts;
        }

        public HtmlBrowserParser(string htmlpath, string url, int waitTimeMs, int waitCounts, bool writeHtmlToDisk, string htmlFilePath, WebView browser) : base(htmlpath, url, writeHtmlToDisk, htmlFilePath)
        {
            this.WaitTimeMs = waitTimeMs;
            this.WaitCounts = waitCounts;
            this.Browser = browser;
        }

        public override async Task<HtmlXpathParserExitCode> RunParserAsync(string url, string htmlPath)
        {
            return await base.RunParserAsync(url, htmlPath);
        }

        public override async Task<HtmlXpathParserExitCode> RunParserAsync()
        {
            lastLastUrl = this.lastUrl;

            HtmlXpathParserExitCode exitCode = await base.RunParserAsync();

            if (!string.IsNullOrEmpty(lastLastUrl) && lastLastUrl.Equals(this.lastUrl))
            {
                Logging.Debug(LogOptions.ClassName, "The browser was not run, no need to cleanup");
            }
            else
            {
                Logging.Debug(LogOptions.ClassName, "The browser was run, needs to be cleanup");
                if (Browser != null)
                {
                    Browser.NavigationCompleted -= Browser_NavigationCompleted;
                    Browser.DOMContentLoaded -= Browser_DOMContentLoaded;
                }

                if (ThreadMode)
                    CleanupBrowser();
            }

            return exitCode;
        }

        protected override async Task<bool> GetHtmlDocumentAsync()
        {
            //reset internals
            browserFinishedLoadingScriptsCounter = 0;
            browserContentLoaded = false;
            browserNavigationCompleted = false;
            browserDispatcher = null;
            browserFailed = false;
            ThreadMode = Browser == null;

            //run browser enough to get scripts parsed to get download link
            if (ThreadMode)
                RunBrowserOnUIThread();
            else
                RunBrowser();

            //wait for browser events to finish
            while (!(browserContentLoaded && browserNavigationCompleted))
            {
                await Task.Delay(WaitTimeMs);
                Logging.Info(LogOptions.ClassName, "browserContentLoaded: {0}, browserNavigationCompleted: {1}", browserContentLoaded.ToString(), browserNavigationCompleted.ToString());
            }

            if (browserFailed)
            {
                Logging.Error(LogOptions.ClassName, "The browser failed to navigate");
            }

            if (!browserFailed)
            {
                //this wait allows the browser to finish loading external scripts
                Logging.Info(LogOptions.ClassName, "The browser task events completed, wait additional {0} counts", WaitCounts);
                while (browserFinishedLoadingScriptsCounter < WaitCounts)
                {
                    await Task.Delay(WaitTimeMs);
                    Logging.Info(LogOptions.ClassName, "Waiting {0} of {1} counts", ++browserFinishedLoadingScriptsCounter, WaitCounts);
                }

                string tempHtmlText = string.Empty;
                if (ThreadMode)
                {
                    browserDispatcher.Invoke(() => {
                        tempHtmlText = GetBrowserHtml();
                    });
                }
                else
                {
                    tempHtmlText = GetBrowserHtml();
                }
                byte[] bytes = Encoding.Default.GetBytes(tempHtmlText);
                htmlText = Encoding.UTF8.GetString(bytes);

                if (WriteHtmlToDisk)
                {
                    //save to string
                    if (!TryWriteHtmlToDisk())
                        return false;
                }
            }

            return true;
        }

        protected virtual string GetBrowserHtml()
        {
            return Browser.InvokeScript("eval", new string[] { "document.documentElement.outerHTML;" });
        }

        private void Browser_DOMContentLoaded(object sender, WebViewControlDOMContentLoadedEventArgs e)
        {
            WebView browser = (sender as WebView);
            //for some sites, it doesn't load all html unless you're scrolled enough (or the height/width is enough)
            if (BrowserHeight > 0 && browser.Height != BrowserHeight)
                browser.Height = BrowserHeight;
            if (BrowserWidth > 0 && browser.Width != BrowserWidth)
                browser.Width = BrowserWidth;

            Logging.Info(LogOptions.ClassName, "The browser reports document completed");
            browserContentLoaded = true;
        }

        private void Browser_NavigationCompleted(object sender, WebViewControlNavigationCompletedEventArgs e)
        {
            Logging.Info(LogOptions.ClassName, "The browser reports navigation completed");
            browserNavigationCompleted = true;
        }

        private void RunBrowserOnUIThread()
        {
            Thread thread = new Thread(() =>
            {
                RunBrowser();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        private void RunBrowser()
        {
            //setup browser events and params
            if (ThreadMode)
            {
                Browser = new WebView();
                ((ISupportInitialize)Browser).BeginInit();
                ((ISupportInitialize)Browser).EndInit();
                browserDispatcher = Dispatcher.CurrentDispatcher;
            }

            Browser.NavigationCompleted += Browser_NavigationCompleted;
            Browser.DOMContentLoaded += Browser_DOMContentLoaded;

            Logging.Info(LogOptions.ClassName, "Running Navigate() method to load browser at URL {0}", Url);
            try
            {
                Browser.Navigate(Url);
            }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
                browserNavigationCompleted = true;
                browserContentLoaded = true;
                browserFailed = true;
            }

            if (ThreadMode)
            {
                Dispatcher.Run();
            }
        }

        private void CleanupBrowser()
        {
            browserDispatcher.Invoke(() =>
            {
                if (Browser != null)
                {
                    Browser.Stop();
                    Browser.Dispose();
                    Browser = null;
                }
            });
            browserDispatcher.ShutdownFinished += (sender, args) =>
            {
                browserDispatcher = null;
            };
            browserDispatcher.InvokeShutdown();
        }

        public override void Cancel()
        {
            base.Cancel();

            if (ThreadMode && browserDispatcher != null)
            {
                if (Browser != null)
                {
                    Browser.NavigationCompleted -= Browser_NavigationCompleted;
                    Browser.DOMContentLoaded -= Browser_DOMContentLoaded;
                }

                CleanupBrowser();
            }
        }
    }
}
