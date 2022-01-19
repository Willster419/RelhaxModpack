using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT;
using Microsoft.Toolkit.Forms.UI.Controls;
using RelhaxModpack.Utilities.Enums;
using System.ComponentModel;
using System.Windows.Forms;

namespace RelhaxModpack.Automation
{
    public class HtmlBrowserParser : HtmlWebscrapeParser
    {
        public int WaitTimeMs { get; set; } = 2000;

        public int WaitCounts { get; set; } = 3;

        public int BrowserWidth { get; set; } = 0;

        public int BrowserHeight { get; set; } = 0;

        public BrowserType BrowserType { get; private set; }

        public BrowserManager BrowserManager { get; set; }

        public bool ThreadMode { get; private set; }

        private int browserFinishedLoadingScriptsCounter;

        private bool browserContentLoaded;

        private bool browserNavigationCompleted;

        private bool browserFailed;

        private Dispatcher browserDispatcher;

        public HtmlBrowserParser(BrowserType browserType) : base()
        {
            this.BrowserType = browserType;
            ThreadMode = true;
        }

        public HtmlBrowserParser(string htmlpath, string url, int waitTimeMs, int waitCounts, BrowserType browserType) : base(htmlpath, url)
        {
            this.WaitTimeMs = waitTimeMs;
            this.WaitCounts = waitCounts;
            this.BrowserType = browserType;
            ThreadMode = true;
        }

        public HtmlBrowserParser(string htmlpath, string url, int waitTimeMs, int waitCounts, bool writeHtmlToDisk, string htmlFilePath, BrowserType browserType) : base(htmlpath, url, writeHtmlToDisk, htmlFilePath)
        {
            this.WaitTimeMs = waitTimeMs;
            this.WaitCounts = waitCounts;
            this.BrowserType = browserType;
            ThreadMode = true;
        }

        public HtmlBrowserParser(string htmlpath, string url, int waitTimeMs, int waitCounts, bool writeHtmlToDisk, string htmlFilePath, WebView browser) : base(htmlpath, url, writeHtmlToDisk, htmlFilePath)
        {
            if (browser == null)
                throw new NullReferenceException();
            this.WaitTimeMs = waitTimeMs;
            this.WaitCounts = waitCounts;
            this.BrowserManager = new BrowserManager(browser);
            BrowserType = BrowserManager.BrowserType;
            ThreadMode = false;
        }

        public HtmlBrowserParser(string htmlpath, string url, int waitTimeMs, int waitCounts, bool writeHtmlToDisk, string htmlFilePath, WebBrowser browser) : base(htmlpath, url, writeHtmlToDisk, htmlFilePath)
        {
            if (browser == null)
                throw new NullReferenceException();
            this.WaitTimeMs = waitTimeMs;
            this.WaitCounts = waitCounts;
            this.BrowserManager = new BrowserManager(browser);
            BrowserType = BrowserManager.BrowserType;
            ThreadMode = false;
        }

        public override async Task<HtmlXpathParserExitCode> RunParserAsync(string url, string htmlPath)
        {
            return await base.RunParserAsync(url, htmlPath);
        }

        public override async Task<HtmlXpathParserExitCode> RunParserAsync()
        {
            return await base.RunParserAsync();
        }

        protected override async Task<bool> GetHtmlDocumentAsync()
        {
            //reset internals
            browserFinishedLoadingScriptsCounter = 0;
            browserContentLoaded = false;
            browserNavigationCompleted = false;
            browserDispatcher = null;
            browserFailed = false;

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
                        tempHtmlText = BrowserManager.GetHtmlDocument();
                    });
                }
                else
                {
                    tempHtmlText = BrowserManager.GetHtmlDocument();
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
            if (ThreadMode && BrowserManager == null)
            {
                BrowserManager = new BrowserManager(this.BrowserType);
                BrowserManager.Init();
                browserDispatcher = Dispatcher.CurrentDispatcher;
            }

            if (!BrowserManager.IsSubscribed)
                BrowserManager.Subscribe();

            BrowserManager.OnNavigationCompleted += BrowserManager_OnNavigationCompleted;
            BrowserManager.OnDocumentCompleted += BrowserManager_OnDocumentCompleted;

            Logging.Info(LogOptions.ClassName, "Running Navigate() method to load browser at URL {0}", Url);
            try
            {
                BrowserManager.Navigate(Url);
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

        private void BrowserManager_OnDocumentCompleted(object sender, EventArgs e)
        {
            Control browser = (sender as BrowserManager).Browser;
            //for some sites, it doesn't load all html unless you're scrolled enough (or the height/width is enough)
            if (BrowserHeight > 0 && browser.Height != BrowserHeight)
                browser.Height = BrowserHeight;
            if (BrowserWidth > 0 && browser.Width != BrowserWidth)
                browser.Width = BrowserWidth;

            Logging.Info(LogOptions.ClassName, "The browser reports document completed");
            browserContentLoaded = true;
        }

        private void BrowserManager_OnNavigationCompleted(object sender, EventArgs e)
        {
            Logging.Info(LogOptions.ClassName, "The browser reports navigation completed");
            browserNavigationCompleted = true;
        }

        public void CleanupBrowser()
        {
            browserDispatcher.Invoke(() =>
            {
                if (BrowserManager != null)
                {
                    BrowserManager.Cleanup();
                    BrowserManager = null;
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
                if (BrowserManager != null && BrowserManager.IsSubscribed)
                {
                    BrowserManager.Unsubscribe();
                }

                CleanupBrowser();
            }
        }
    }
}
