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
using System.Windows.Forms;
using System.Windows.Threading;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace RelhaxModpack.Automation
{
    public class DownloadBrowserTask : DownloadHtmlTask, IDownloadTask, IXmlSerializable
    {
        public int WaitTimeMs { get; } = 1000;

        public int WaitCounts { get; } = 3;

        public override string Command { get; } = "download_browser";

        protected WebBrowser Browser = null;

        protected string HtmlString = null;

        protected int BrowserFinishedLoadingScriptsCounter = 0;

        protected bool BrowserLoaded = false;

        protected bool BrowserNavigated = false;

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
            HtmlNode node = document.DocumentNode;
            //https://stackoverflow.com/questions/1390568/how-can-i-match-on-an-attribute-that-contains-a-certain-string
            //sample html xpath: //div[contains(@class, 'ModDetails_label')]
            HtmlNode resultNode = node.SelectSingleNode(HtmlPath);
            Logging.Debug(Logfiles.AutomationRunner, "Htmlpath results in node value '{0}' of type '{1}'", resultNode.InnerText, resultNode.NodeType.ToString());
            Url = resultNode.InnerText;
        }

        protected async Task RunBrowserAsync()
        {
            using (Browser = new WebBrowser())
            {
                Browser.ScriptErrorsSuppressed = true;
                //set event handler for browser to be done loading
                Browser.Navigated += (senda, args) =>
                {
                    Logging.Debug(Logfiles.AutomationRunner, "The browser reports navigation completed, wait for document completed and timeout");
                    BrowserNavigated = true;
                };

                Browser.DocumentCompleted += (sendahh, endArgs) =>
                {
                    Logging.Debug(Logfiles.AutomationRunner, "The browser reports document completed, wait for timeout");
                    BrowserLoaded = true;
                };

                //run browser enough to get scripts parsed to get download link
                Logging.Debug(Logfiles.AutomationRunner, "Running async task to load browser and wait for it to finish");
                Browser.Navigate(Url);

                //wait for browser events to finish
                while (!BrowserLoaded && !BrowserNavigated)
                {
                    await Task.Delay(WaitTimeMs);
                    Logging.Debug(Logfiles.AutomationRunner, "The browser task events completed, wait additional {0} counts", WaitCounts);
                }

                //this wait allows the browser to finish loading external scripts
                while (BrowserFinishedLoadingScriptsCounter <= WaitCounts)
                {
                    await Task.Delay(WaitTimeMs);
                    Logging.Debug(Logfiles.AutomationRunner, "Waiting {0} of {1} counts", ++BrowserFinishedLoadingScriptsCounter, WaitCounts);
                }

                Logging.Info(Logfiles.AutomationRunner, "The browser reports all loading done, save html to string");
                HtmlString = Browser.DocumentText;
            }
        }

        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();
        }
        #endregion
    }
}
