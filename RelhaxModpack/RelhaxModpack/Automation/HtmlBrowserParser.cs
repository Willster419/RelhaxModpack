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
    /// <summary>
    /// An HtmlBrowserParser class enables retrieval of a string of html via downloading a rendered web page resource (with javascript) and parsing for specific values using HtmlPath.
    /// </summary>
    public class HtmlBrowserParser : HtmlWebscrapeParser, IDisposable
    {
        /// <summary>
        /// The time, in milliseconds, to wait after the first NavigationCompleted event is fired (or a previous wait).
        /// </summary>
        /// <remarks>The total wait time can be calculated as WaitTimeMs * WaitCounts.</remarks>
        /// <seealso cref="WaitCounts"/>
        public int WaitTimeMs { get; set; } = 2000;

        /// <summary>
        /// The number of times the browser should wait for WaitTimeMs after the first NavigationCompleted event is fired.
        /// </summary>
        /// <remarks>The total wait time can be calculated as WaitTimeMs * WaitCounts.</remarks>
        /// <seealso cref="WaitTimeMs"/>
        public int WaitCounts { get; set; } = 3;

        /// <summary>
        /// Gets or sets the height of the browser.
        /// </summary>
        public int BrowserWidth { get; set; } = 0;

        /// <summary>
        /// Gets or sets the width of the browser.
        /// </summary>
        public int BrowserHeight { get; set; } = 0;

        /// <summary>
        /// Get the type of browser implementation that was used inside the browser manager.
        /// </summary>
        public BrowserType BrowserType { get; private set; }

        /// <summary>
        /// Get the browser manager that runs the underlying browser API
        /// </summary>
        public BrowserManager BrowserManager { get; protected set; }

        /// <summary>
        /// The dispatcher used to create and interface with the browser manager.
        /// </summary>
        protected Dispatcher browserDispatcher;

        /// <summary>
        /// Occurs when the browser manager's BrowserCreated event is fired.
        /// </summary>
        public event BrowserManagerDelegate BrowserCreated;

        /// <summary>
        /// The cancellation token to enable the user to cancel the navigation operation.
        /// </summary>
        protected CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Create an instance of the HtmlBrowserParser class.
        /// </summary>
        /// <param name="browserType">The type of browser implementation to use for the browser manager.</param>
        public HtmlBrowserParser(BrowserType browserType) : base()
        {
            this.BrowserType = browserType;
            cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Create an instance of the HtmlBrowserParser class.
        /// </summary>
        /// <param name="htmlpath">The HtmlPath string to use for navigating the html document.</param>
        /// <param name="url">The location to the web page resource to download.</param>
        /// <param name="waitTimeMs">The time, in milliseconds, to wait after the first NavigationCompleted event is fired (or a previous wait).</param>
        /// <param name="waitCounts">The number of times the browser should wait for WaitTimeMs after the first NavigationCompleted event is fired.</param>
        /// <param name="writeHtmlToDisk">Flag to determine if the HtmlParser should write the contents of the html string to disk for debug.</param>
        /// <param name="htmlFilePath">The relative or absolute IO path on disk to write the html string to disk. Must include the file name.</param>
        /// <param name="browserType">The type of browser implementation to use inside the browser manager.</param>
        public HtmlBrowserParser(string htmlpath, string url, int waitTimeMs, int waitCounts, bool writeHtmlToDisk, string htmlFilePath, BrowserType browserType)
            : base(htmlpath, url, writeHtmlToDisk, htmlFilePath)
        {
            this.WaitTimeMs = waitTimeMs;
            this.WaitCounts = waitCounts;
            this.BrowserType = browserType;
            cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Create an instance of the HtmlBrowserParser class.
        /// </summary>
        /// <param name="htmlpath">The HtmlPath string to use for navigating the html document.</param>
        /// <param name="url">The location to the web page resource to download.</param>
        /// <param name="waitTimeMs">The time, in milliseconds, to wait after the first NavigationCompleted event is fired (or a previous wait).</param>
        /// <param name="waitCounts">The number of times the browser should wait for WaitTimeMs after the first NavigationCompleted event is fired.</param>
        /// <param name="writeHtmlToDisk">Flag to determine if the HtmlParser should write the contents of the html string to disk for debug.</param>
        /// <param name="htmlFilePath">The relative or absolute IO path on disk to write the html string to disk. Must include the file name.</param>
        /// <param name="browserType">The type of browser implementation to use inside the browser manager.</param>
        /// <param name="browserDispatcher">The dispatcher used to create and interface with the browser manager.</param>
        public HtmlBrowserParser(string htmlpath, string url, int waitTimeMs, int waitCounts, bool writeHtmlToDisk, string htmlFilePath, BrowserType browserType, Dispatcher browserDispatcher)
            : base(htmlpath, url, writeHtmlToDisk, htmlFilePath)
        {
            this.WaitTimeMs = waitTimeMs;
            this.WaitCounts = waitCounts;
            this.BrowserType = browserType;
            this.browserDispatcher = browserDispatcher;
            cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Uses the browser manager to download the html document at the Url.
        /// </summary>
        /// <returns>True if the document was downloaded to a string, false otherwise.</returns>
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

        /// <summary>
        /// Cancels the browser's navigation operation, if one is running.
        /// </summary>
        public override void Cancel()
        {
            base.Cancel();
            cancellationTokenSource.Cancel();
            BrowserManager.Cancel();
        }

        /// <summary>
        /// Dispose of the browser manager and un-subscribe events
        /// </summary>
        public void Dispose()
        {
            if (BrowserManager != null)
            {
                BrowserManager.BrowserCreated -= this.BrowserCreated;
                ((IDisposable)BrowserManager).Dispose();
            }
            else
            {
                Logging.Warning(LogOptions.MethodName, "BrowserManager is already null (is this the intent)?");
            }
            ((IDisposable)cancellationTokenSource).Dispose();
        }
    }
}
