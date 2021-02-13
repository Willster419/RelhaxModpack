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
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RelhaxModpack.Automation
{
    public class DownloadBrowserTask : DownloadHtmlTask, IDownloadTask, IXmlSerializable
    {
        public int WaitTimeMs { get; } = 3000;

        public int Retries { get; } = 3;

        public override string Command { get; } = "download_browser";

        protected WebBrowser Browser = null;

        public Dispatcher BrowserDispatcher = null;

        protected string HtmlString = null;

        protected int BrowserFinishedLoadingScriptsCounter = 0;

        protected bool BrowserLoaded = false;

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(WaitTimeMs), nameof(Retries) }).ToArray();
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
            if (Retries <= 0)
            {
                ExitCode = 1;
                ErrorMessage = string.Format("ExitCode {0}: Retries must be greater then 0. Current value: {1}", ExitCode, Retries.ToString());
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
                return;
            }
        }

        public async override Task RunTask()
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
            HtmlNode node = document.DocumentNode;
            //https://stackoverflow.com/questions/1390568/how-can-i-match-on-an-attribute-that-contains-a-certain-string
            //sample html xpath: //div[contains(@class, 'ModDetails_label')]
            HtmlNode resultNode = node.SelectSingleNode(HtmlPath);
            Logging.Debug(Logfiles.AutomationRunner, "Htmlpath results in node value '{0}' of type '{1}'", resultNode.InnerText, resultNode.NodeType.ToString());
            Url = resultNode.InnerText;

            Logging.AutomationRunner("Using DownloadStaticTask explicit cast to invoke RunTask()", LogLevel.Debug);
            (this as DownloadStaticTask).RunTask();
        }

        protected async Task RunBrowserAsync()
        {
            Dispatcher dispatcher = this.BrowserDispatcher == null ? App.Current.Dispatcher : this.BrowserDispatcher;
            await dispatcher.Invoke(RunBrowserRealAsync);
        }

        protected async Task RunBrowserRealAsync()
        {
            using (Browser = new WebBrowser())
            {
                //set event handler for browser to be done loading
                Logging.Debug(Logfiles.AutomationRunner, "Setting browser loadCompleted event handler");
                Browser.LoadCompleted += (sendahh, endArgs) =>
                {
                    Logging.Debug(Logfiles.AutomationRunner, "The browser reports load completed");
                    BrowserLoaded = true;
                };

                Browser.Navigating += (sendahh, endArgs) =>
                {
                    //suppress script errors message window
                    Logging.Debug(Logfiles.AutomationRunner, "Setting browser script suppression");
                    //https://stackoverflow.com/questions/1298255/how-do-i-suppress-script-errors-when-using-the-wpf-webbrowser-control
                    dynamic activeX = this.Browser.GetType().InvokeMember("ActiveXInstance", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                        null, this.Browser, new object[] { });
                    activeX.Silent = true;
                };

                //run browser enough to get scripts parsed to get download link
                Logging.Debug(Logfiles.AutomationRunner, "Running async task to load browser and wait for it to finish");
                bool temp = Browser.IsInitialized;
                Browser.BeginInit();
                Browser.EndInit();
                Browser.Navigate(Url);

                //this wait allows the browser to finish loading external scripts
                while (!((BrowserFinishedLoadingScriptsCounter >= 3) && BrowserLoaded))
                {
                    await Task.Delay(WaitTimeMs);
                    Logging.Debug(Logfiles.AutomationRunner, "The browser task delay has elapsed, BrowserLoadingCounter is set to {0}", ++BrowserFinishedLoadingScriptsCounter);
                }

                Logging.Info(Logfiles.AutomationRunner, "The browser reports that the task loading is done, save html to string");
                var doc = Browser.Document as mshtml.HTMLDocument;
                HtmlString = doc.body.outerHTML;
            }
        }

        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();
        }
        #endregion
    }
}
