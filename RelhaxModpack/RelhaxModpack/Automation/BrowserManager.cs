using Microsoft.Toolkit.Forms.UI.Controls;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Threading;

namespace RelhaxModpack.Automation
{
    public delegate void BrowserManagerDelegate(object sender, EventArgs e);

    public class BrowserManager : IAutomationBrowser, IDisposable
    {
        public event BrowserManagerDelegate DocumentCompleted;

        public event BrowserManagerDelegate NavigationCompleted;

        public int WaitTimeMs { get; set; } = 2000;

        public int WaitCounts { get; set; } = 3;

        protected IAutomationBrowser browser;

        protected BrowserType browserType;

        protected Dispatcher dispatcher;

        protected bool OnCustomThread;

        protected ManualResetEvent manualResetEvent;

        protected int browserFinishedLoadingScriptsCounter;

        public event BrowserManagerDelegate BrowserCreated;

        public BrowserManager(BrowserType browserType, Dispatcher dispatcher = null)
        {
            this.browserType = browserType;
            this.dispatcher = dispatcher;
            this.OnCustomThread = this.dispatcher == null;
        }

        public BrowserType BrowserType
        { get { return browserType; } }

        public Control Browser
        {
            get
            { return browser.Browser; }
        }

        public int Height
        {
            get
            { return Browser.Height; }
            set
            { Browser.Height = value; }
        }

        public int Width
        {
            get
            { return Browser.Width; }
            set
            { Browser.Width = value; }
        }

        public string GetHtmlDocument()
        {
            return browser.GetHtmlDocument();
        }

        public void Navigate(string url)
        {
            browser.Navigate(url);
        }

        protected void SetDispatcher()
        {
            if (dispatcher != null)
                return;

            Thread thread = new Thread(() =>
            {
                this.dispatcher = Dispatcher.CurrentDispatcher;
                Dispatcher.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            while (this.dispatcher == null)
                Thread.Sleep(1);
        }

        public async Task<bool> NavigateWithDelayAsync(string url, CancellationToken token)
        {
            browserFinishedLoadingScriptsCounter = 0;
            SetDispatcher();

            this.dispatcher.Invoke(() => this.CreateBrowser());

            manualResetEvent = new ManualResetEvent(false);

            browser.DocumentCompleted += (sender, args) =>
            {
                Logging.Debug("The browser reports document completed");
                manualResetEvent.Set();
            };

            Logging.Debug("Browser dispatch navigate start");
            this.dispatcher.InvokeAsync(() =>
            {
                this.Navigate(url);
            });
            Logging.Debug("Browser dispatch navigate finish, wait on ManualResetEvent");

            await Task.Run(() => { manualResetEvent.WaitOne(); });

            if (this.browser == null)
            {
                Logging.Debug("Browser was disposed, cancel or error happened");
                return false;
            }

            //now wait for the timeout
            //this wait allows the browser to finish loading external scripts
            Logging.Info(LogOptions.ClassName, "The browser task events completed, wait additional {0} counts", WaitCounts);
            while (browserFinishedLoadingScriptsCounter < WaitCounts)
            {
                if (token != null && token.IsCancellationRequested)
                {
                    Logging.Debug("Cancel happened");
                    return false;
                }
                await Task.Delay(WaitTimeMs);
                Logging.Info(LogOptions.ClassName, "Waiting {0} of {1} counts", ++browserFinishedLoadingScriptsCounter, WaitCounts);
            }

            return true;
        }

        protected void CreateBrowser()
        {
            switch (browserType)
            {
                case BrowserType.WebBrowser:
                    browser = new AutomationWebBrowser();
                    break;
                case BrowserType.WebView:
                    browser = new AutomationWebViewWrapper();
                    break;
                case BrowserType.WebView2:
                default:
                    throw new NotImplementedException();
            }
            browser.DocumentCompleted += DocumentCompleted;
            browser.NavigationCompleted += DocumentCompleted;
            BrowserCreated?.Invoke(this, new EventArgs());
        }

        public void Cancel()
        {
            Logging.Debug(LogOptions.ClassName, "Cancel called for Browser Manager");
            this.browser.Cancel();
            DisposeBrowser();
            this.manualResetEvent.Set();
        }

        public void Dispose()
        {
            DisposeBrowser();
            manualResetEvent.Dispose();
            if (this.OnCustomThread)
                ShutdownDispatcher();

            if (DocumentCompleted != null)
                DocumentCompleted = null;

            if (NavigationCompleted != null)
                NavigationCompleted = null;
        }

        public void DisposeBrowser()
        {
            this.dispatcher.Invoke(() =>
            {
                if (browser != null)
                {
                    if (!browser.Browser.IsDisposed)
                    {
                        browser.Cancel();
                        browser.Dispose();
                    }
                }
            });
        }

        protected void ShutdownDispatcher()
        {
            this.dispatcher.ShutdownFinished += (sender, args) =>
            {
                this.dispatcher = null;
            };
            this.dispatcher.InvokeShutdown();
        }
    }
}
