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
    public class HtmlBrowserParser : HtmlWebscrapeParser, IDisposable
    {
        public int WaitTimeMs { get; set; } = 2000;

        public int WaitCounts { get; set; } = 3;

        public int BrowserWidth { get; set; } = 0;

        public int BrowserHeight { get; set; } = 0;

        public BrowserType BrowserType { get; private set; }

        public BrowserManager BrowserManager { get; protected set; }

        protected Dispatcher browserDispatcher;

        public event BrowserManagerDelegate BrowserCreated;

        protected CancellationTokenSource cancellationTokenSource;

        public HtmlBrowserParser(BrowserType browserType) : base()
        {
            this.BrowserType = browserType;
            cancellationTokenSource = new CancellationTokenSource();
        }

        public HtmlBrowserParser(string htmlpath, string url, int waitTimeMs, int waitCounts, bool writeHtmlToDisk, string htmlFilePath, BrowserType browserType)
            : base(htmlpath, url, writeHtmlToDisk, htmlFilePath)
        {
            this.WaitTimeMs = waitTimeMs;
            this.WaitCounts = waitCounts;
            this.BrowserType = browserType;
            cancellationTokenSource = new CancellationTokenSource();
        }

        public HtmlBrowserParser(string htmlpath, string url, int waitTimeMs, int waitCounts, bool writeHtmlToDisk, string htmlFilePath, BrowserType browserType, Dispatcher browserDispatcher)
            : base(htmlpath, url, writeHtmlToDisk, htmlFilePath)
        {
            this.WaitTimeMs = waitTimeMs;
            this.WaitCounts = waitCounts;
            this.BrowserType = browserType;
            this.browserDispatcher = browserDispatcher;
            cancellationTokenSource = new CancellationTokenSource();
        }

        protected override async Task<bool> GetHtmlDocumentAsync()
        {
            BrowserManager = new BrowserManager(BrowserType, browserDispatcher) { WaitCounts = this.WaitCounts, WaitTimeMs = this.WaitTimeMs };
            BrowserManager.BrowserCreated += (sender, args) =>
            {
                BrowserManager.Height = this.BrowserHeight;
                BrowserManager.Width = this.BrowserWidth;
                this.BrowserCreated?.Invoke(this, args);
            };

            //run browser enough to get scripts parsed to get download link
            bool browserResult = await BrowserManager.NavigateWithDelayAsync(this.Url, cancellationTokenSource.Token);
            if (!browserResult)
            {
                Logging.Error(LogOptions.ClassName, "The browser failed to navigate");
                return false;
            }

            htmlText = BrowserManager.GetHtmlDocument();
            if (string.IsNullOrEmpty(htmlText))
            {
                Logging.Error(LogOptions.ClassName, "The browser failed to navigate");
                return false;
            }

            return true;
        }

        public override void Cancel()
        {
            base.Cancel();
            cancellationTokenSource.Cancel();
            BrowserManager.Cancel();
        }

        public void Dispose()
        {
            BrowserManager.BrowserCreated -= this.BrowserCreated;
            ((IDisposable)BrowserManager).Dispose();
            ((IDisposable)cancellationTokenSource).Dispose();
        }
    }
}
