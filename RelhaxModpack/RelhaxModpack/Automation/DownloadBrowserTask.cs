using HtmlAgilityPack;
using RelhaxModpack.Database;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace RelhaxModpack.Automation
{
    public class DownloadBrowserTask : DownloadHtmlTask, IDownloadTask, IXmlSerializable
    {
        public const string TaskCommandName = "download_browser";

        public override string Command { get { return TaskCommandName; } }

        public int WaitTimeMs { get; set; } = 1000;

        public int WaitCounts { get; set; } = 3;

        protected WebBrowser Browser = null;

        protected string HtmlString = null;

        protected int BrowserFinishedLoadingScriptsCounter = 0;

        protected bool BrowserLoaded = false;

        protected bool BrowserNavigated = false;

        Dispatcher browserDispatcher = null;

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(WaitTimeMs), nameof(WaitCounts) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ValidateCommands()
        {
            base.ValidateCommands();
            if (WaitTimeMs <= 0)
            {
                ExitCode = 1;
                ErrorMessage = string.Format("ExitCode {0}: WaitTimeMs must be greater then 0. Current value: {1}", ExitCode, WaitTimeMs.ToString());
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
                return;
            }
            if (WaitCounts <= 0)
            {
                ExitCode = 1;
                ErrorMessage = string.Format("ExitCode {0}: Retries must be greater then 0. Current value: {1}", ExitCode, WaitCounts.ToString());
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
                return;
            }
            if (!UrlIsValid(Url))
            {
                ExitCode = 1;
                ErrorMessage = string.Format("ExitCode {0}: The given URL is not valid: {1}", ExitCode, Url);
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
                return;
            }
        }

        public async override Task RunTask()
        {
            await base.RunTask();
        }

        protected async override Task SetupUrl()
        {
            //add registry entry to use latest IE for script parsing
            Logging.AutomationRunner("Setting application to use latest version of IE for embedded browser", LogLevel.Debug);
            RegistryUtils.SetRegisterKeyForIEVersion(IERegistryVersion.IE11Forced);

            Logging.AutomationRunner("Running Browser execution code");
            await RunBrowserAsync();

            //parse html from the browser
            Logging.Info(Logfiles.AutomationRunner, "Running htmlpath to get download file url");
            Logging.Debug(Logfiles.AutomationRunner, "The htmlpath used was {0}", HtmlPath);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(HtmlString);
            HtmlNodeNavigator navigator = (HtmlNodeNavigator)document.CreateNavigator();
            var result = navigator.SelectSingleNode(HtmlPath);
            if (result == null)
            {
                ExitCode = 3;
                ErrorMessage = string.Format("ExitCode {0}: The HtmlPath returned no results", ExitCode);
                return;
            }
            else
            {
                Logging.Debug(Logfiles.AutomationRunner, "Htmlpath results in node value '{0}' of type '{1}'", result.ToString(), result.NodeType.ToString());
                Url = result.ToString();
            }
        }

        protected async virtual Task RunBrowserAsync()
        {
            RunBrowserOnUIThread();

            Logging.Info("Browser will check at a rate of {0}ms, and then wait {1} times after", WaitTimeMs.ToString(), WaitCounts.ToString());
            //wait for browser events to finish
            while (!(BrowserLoaded && BrowserNavigated))
            {
                await Task.Delay(WaitTimeMs);
                Logging.Debug(Logfiles.AutomationRunner, "BrowserLoaded: {0}, BrowserNavigated: {1}", BrowserLoaded.ToString(), BrowserNavigated.ToString());
            }

            //this wait allows the browser to finish loading external scripts
            Logging.Debug(Logfiles.AutomationRunner, "The browser task events completed, wait additional {0} counts", WaitCounts);
            while (BrowserFinishedLoadingScriptsCounter <= WaitCounts)
            {
                await Task.Delay(WaitTimeMs);
                Logging.Debug(Logfiles.AutomationRunner, "Waiting {0} of {1} counts", ++BrowserFinishedLoadingScriptsCounter, WaitCounts);
            }

            Logging.Info(Logfiles.AutomationRunner, "The browser reports all loading done, save html to string");

            //cleanup from the browser
            browserDispatcher.Invoke(() =>
            {
                Browser.Dispose();
            });
            browserDispatcher.InvokeShutdown();
            browserDispatcher.ShutdownFinished += (sender, args) =>
            {
                browserDispatcher = null;
            };
        }

        protected virtual void RunBrowserOnUIThread()
        {
            Thread thread = new Thread(() =>
            {
                //get dispatcher to be able to invoke disposal later
                browserDispatcher = Dispatcher.CurrentDispatcher;

                //create and setup browser
                Browser = new WebBrowser();
                Browser.ScriptErrorsSuppressed = true;

                //set event handler for browser to be done loading
                Browser.Navigated += (senda, args) =>
                {
                    Logging.Debug(Logfiles.AutomationRunner, "The browser reports navigation completed");
                    BrowserNavigated = true;
                };

                Browser.DocumentCompleted += (sendahh, endArgs) =>
                {
                    Logging.Debug(Logfiles.AutomationRunner, "The browser reports document completed");
                    BrowserLoaded = true;
                    HtmlString = Browser.Document.Body.OuterHtml;
                };

                //run browser enough to get scripts parsed to get download link
                Logging.Debug(Logfiles.AutomationRunner, "Running async task to load browser and wait for it to finish");
                Browser.Navigate(Url);

                //start the windows message pump to the browser runs
                Dispatcher.Run();

            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();
        }

        private bool UrlIsValid(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    //Returns TRUE if the Status code == 200
                    response.Close();
                    return (response.StatusCode == HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                //Any exception will returns false.
                Logging.Exception(ex.ToString());
                return false;
            }
        }
        #endregion
    }
}
