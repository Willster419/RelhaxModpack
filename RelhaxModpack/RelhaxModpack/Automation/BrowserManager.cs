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
    /// <summary>
    /// The delegate to invoke when a browser event occurs.
    /// </summary>
    /// <param name="sender">The sending object.</param>
    /// <param name="e">Event arguments.</param>
    public delegate void BrowserManagerDelegate(object sender, EventArgs e);

    /// <summary>
    /// Provides a managed implementation to use a browser of any API to navigate and extract text from a web page.
    /// </summary>
    public class BrowserManager : IAutomationBrowser, IDisposable
    {
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
        /// The time, in milliseconds, to wait after the first NavigationCompleted event is fired (or a previous wait).
        /// </summary>
        /// <remarks>The total wait time can be calculated as WaitTimeMs * WaitCounts.</remarks>
        /// <seealso cref="WaitCounts"/>
        public int WaitTimeMs { get; set; } = 2000;

        /// <summary>
        /// The number of times the browser manager should wait for WaitTimeMs after the first NavigationCompleted event is fired.
        /// </summary>
        /// <remarks>The total wait time can be calculated as WaitTimeMs * WaitCounts.</remarks>
        /// <seealso cref="WaitTimeMs"/>
        public int WaitCounts { get; set; } = 3;

        /// <summary>
        /// Get the web browser as a control object.
        /// </summary>
        protected IAutomationBrowser browser;

        /// <summary>
        /// The type of browser object that is created in this BrowserManager instance.
        /// </summary>
        protected BrowserType browserType;

        /// <summary>
        /// The dispatcher used to create and interface with the browser object.
        /// </summary>
        protected Dispatcher dispatcher;

        /// <summary>
        /// Used to determine if the browser object is being managed on a thread other then the one this manager instance is using.
        /// </summary>
        protected bool OnCustomThread;

        /// <summary>
        /// The synchronization mechanism used for waiting until a browser has finished asynchronously loading a web page. This is thread safe.
        /// </summary>
        protected ManualResetEvent manualResetEvent;

        /// <summary>
        /// The counter for how many waits of WaitCounts have been completed.
        /// </summary>
        /// <seealso cref="WaitCounts"/>
        protected int browserFinishedLoadingScriptsCounter;

        /// <summary>
        /// Occurs when the browser object has been instanced.
        /// </summary>
        public event BrowserManagerDelegate BrowserCreated;

        /// <summary>
        /// Creates an instance of the BrowserManager class.
        /// </summary>
        /// <param name="browserType">The type of browser to create.</param>
        /// <param name="dispatcher">The dispatcher to use for creating and interfacing with the browser object.</param>
        /// <remarks>If the dispatcher argument is null, then a new UI thread will be created and a dispatcher will be assigned.</remarks>
        public BrowserManager(BrowserType browserType, Dispatcher dispatcher = null)
        {
            this.browserType = browserType;
            this.dispatcher = dispatcher;
            this.OnCustomThread = this.dispatcher == null;
        }

        /// <summary>
        /// Get the type of browser object that was created.
        /// </summary>
        public BrowserType BrowserType
        { get { return browserType; } }

        /// <summary>
        /// Get the browser as a control object.
        /// </summary>
        public Control Browser
        {
            get
            { return browser.Browser; }
        }

        /// <summary>
        /// Gets or sets the height of the browser control.
        /// </summary>
        public int Height
        {
            get
            { return Browser.Height; }
            set
            { Browser.Height = value; }
        }

        /// <summary>
        /// Gets or sets the width of the browser control.
        /// </summary>
        public int Width
        {
            get
            { return Browser.Width; }
            set
            { Browser.Width = value; }
        }

        /// <summary>
        /// Return the currently parsed html document from the browser object.
        /// </summary>
        /// <returns>The currently parsed html document as a string.</returns>
        /// <remarks>Most implementations only return the document's "outer html" section, which may not include the document header.</remarks>
        public string GetHtmlDocument()
        {
            string htmlDocument = null;
            this.dispatcher.Invoke(() => htmlDocument = browser.GetHtmlDocument());
            return htmlDocument;
        }

        /// <summary>
        /// Start a navigation to the given url.
        /// </summary>
        /// <param name="url">The resource to navigate to.</param>
        public void Navigate(string url)
        {
            browser.Navigate(url);
        }

        /// <summary>
        /// Creates a thread to be used to manage the browser object, if dispatcher is null.
        /// </summary>
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

        /// <summary>
        /// Asynchronously tells the browser to navigate to a web page and wait for the designated amount of time and counts.
        /// </summary>
        /// <param name="url">The url to have the browser navigate to.</param>
        /// <param name="token">The cancellation token to allow the user to cancel the web page navigation.</param>
        /// <returns>True if the web navigation completed successfully, false otherwise.</returns>
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

        /// <summary>
        /// Initializes the browser object.
        /// </summary>
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

        /// <summary>
        /// Cancels a browser navigation operation.
        /// </summary>
        public void Cancel()
        {
            Logging.Debug(LogOptions.ClassName, "Cancel called for Browser Manager");
            this.browser.Cancel();
            DisposeBrowser();
            this.manualResetEvent.Set();
        }

        /// <summary>
        /// Releases all resources used by the BrowserManger instance.
        /// </summary>
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

        /// <summary>
        /// Dispose of the browser object.
        /// </summary>
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

        /// <summary>
        /// Shutdown the dispatcher object.
        /// </summary>
        /// <remarks>This should only be called when the dispatcher has been created inside this class. Otherwise you could be invoking shutdown on the currently running dispatcher (that the application is running on).</remarks>
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
