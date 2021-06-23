using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Xml.XPath;
using HtmlAgilityPack;
using RelhaxModpack.Common;
using RelhaxModpack.Utilities.Enums;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace RelhaxModpack.Automation
{
    public class HtmlBrowserParser
    {
        public string HtmlPath { get; set; }

        public WebBrowser Browser { get; set; }

        public bool ThreadMode { get; private set; }

        public int WaitTimeMs { get; set; } = 2000;

        public int WaitCounts { get; set; } = 3;

        public string Url { get; set; }

        public bool WriteHtmlToDisk { get; set; }

        public string HtmlFilePath { get; set; }

        public string ResultString { get; private set; }

        public HtmlNodeNavigator ResultNode { get; private set; }

        private int browserFinishedLoadingScriptsCounter;

        private bool browserDocumentCompleted;

        private bool browserNavigated;

        private string lastUrl;

        private string htmlText;

        private bool browserFailed;

        private Dispatcher browserDispatcher;

        public HtmlBrowserParser()
        {

        }

        public HtmlBrowserParser(string htmlpath, string url, int waitTimeMs, int waitCounts)
        {
            this.HtmlPath = htmlpath;
            this.Url = url;
            this.WaitTimeMs = waitTimeMs;
            this.WaitCounts = waitCounts;
        }

        public HtmlBrowserParser(string htmlpath, string url, int waitTimeMs, int waitCounts, bool writeHtmlToDisk, string htmlFilePath, WebBrowser browser)
        {
            this.HtmlPath = htmlpath;
            this.Url = url;
            this.WaitTimeMs = waitTimeMs;
            this.WaitCounts = waitCounts;
            this.WriteHtmlToDisk = writeHtmlToDisk;
            this.HtmlFilePath = htmlFilePath;
            this.Browser = browser;
        }

        public async Task<HtmlBrowserParserExitCode> RunParserAsync(string url, string htmlPath)
        {
            Url = url;
            HtmlPath = htmlPath;
            return await RunParserAsync();
        }

        public async Task<HtmlBrowserParserExitCode> RunParserAsync()
        {
            bool runBrowserCleanup = false;

            //check if the url is valid
            //https://stackoverflow.com/a/3808841/3128017
            if (string.IsNullOrEmpty(Url))
            {
                Logging.Error(LogOptions.ClassName, "The URL is empty");
                return HtmlBrowserParserExitCode.InvalidParameters;
            }

            if (!UrlIsValid(Url))
            {
                Logging.Info(LogOptions.ClassName, "The URL is invalid");
                return HtmlBrowserParserExitCode.InvalidParameters;
            }

            if (string.IsNullOrEmpty(HtmlPath))
            {
                Logging.Info(LogOptions.ClassName, "The HtmlPath is empty");
                return HtmlBrowserParserExitCode.InvalidParameters;
            }

            if (WriteHtmlToDisk && string.IsNullOrEmpty(HtmlFilePath))
            {
                Logging.Info(LogOptions.ClassName, "WriteToDisk is high and filepath is empty");
                return HtmlBrowserParserExitCode.InvalidParameters;
            }

            if (!string.IsNullOrEmpty(lastUrl) && lastUrl.Equals(Url))
            {
                Logging.Info(LogOptions.ClassName, "The last URL did not change, we can skip the browser run");
            }
            else
            {
                runBrowserCleanup = true;
                Logging.Info(LogOptions.ClassName, "The last URL changed or is null, load the browser page");
                lastUrl = Url;
                if (!await RunBrowserAsync())
                {
                    return HtmlBrowserParserExitCode.ErrorBrowserNavigation;
                }
            }

            if (!RunHtmlPathSearch())
            {
                return HtmlBrowserParserExitCode.ErrorHtmlParsing;
            }

            if (runBrowserCleanup)
            {
                Browser.Navigated -= Browser_Navigated;
                Browser.DocumentCompleted -= Browser_DocumentCompleted;
                WindowsInterop.SecurityAlertDialogWillBeShown -= this.WindowsInterop_SecurityAlertDialogWillBeShown;
                WindowsInterop.Unhook();

                if (ThreadMode)
                    CleanupBrowser();
            }

            return HtmlBrowserParserExitCode.None;
        }

        private async Task<bool> RunBrowserAsync()
        {
            //reset internals
            browserFinishedLoadingScriptsCounter = 0;
            browserDocumentCompleted = false;
            browserNavigated = false;
            browserDispatcher = null;
            browserFailed = false;
            ResultString = string.Empty;
            ResultNode = null;
            ThreadMode = Browser == null;

            //run browser enough to get scripts parsed to get download link
            if (ThreadMode)
                RunBrowserOnUIThread();
            else
                RunBrowser(false);

            //wait for browser events to finish
            while (!(browserDocumentCompleted && browserNavigated))
            {
                await Task.Delay(WaitTimeMs);
                Logging.Debug(LogOptions.ClassName, "browserDocumentCompleted: {0}, browserNavigated: {1}", browserDocumentCompleted.ToString(), browserNavigated.ToString());
            }

            if (browserFailed)
            {
                Logging.Error(LogOptions.ClassName, "The browser failed to navigate");
            }

            if (!browserFailed)
            {
                //this wait allows the browser to finish loading external scripts
                Logging.Debug(LogOptions.ClassName, "The browser task events completed, wait additional {0} counts", WaitCounts);
                while (browserFinishedLoadingScriptsCounter < WaitCounts)
                {
                    await Task.Delay(WaitTimeMs);
                    Logging.Debug(LogOptions.ClassName, "Waiting {0} of {1} counts", ++browserFinishedLoadingScriptsCounter, WaitCounts);
                }

                if (ThreadMode)
                {
                    browserDispatcher.Invoke(() => {
                        htmlText = Browser.Document.Body.OuterHtml;
                    });
                }
                else
                {
                    htmlText = Browser.Document.Body.OuterHtml;
                }

                if (WriteHtmlToDisk)
                {
                    //save to string
                    Logging.Info(LogOptions.ClassName, "Writing HTML to {0}", HtmlFilePath);
                    try
                    {
                        File.WriteAllText(HtmlFilePath, Browser.Document.Body.OuterHtml);
                    }
                    catch (Exception ex2)
                    {
                        Logging.Exception("Failed to write to disk");
                        Logging.Exception(ex2.ToString());
                        return false;
                    }
                    Logging.Info(LogOptions.ClassName, "The browser reports all loading done, save html to string");
                }
            }

            return true;
        }

        private bool RunHtmlPathSearch()
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlText);
            HtmlNodeNavigator navigator = (HtmlNodeNavigator)document.CreateNavigator();
            //https://stackoverflow.com/questions/1390568/how-can-i-match-on-an-attribute-that-contains-a-certain-string
            //sample htmlPath to get download link: @"//a[contains(@class, 'ModDetails_hidden')]//@href"
            //HtmlNodeCollection clientVersionNode = node.SelectNodes(@"//div[contains(@class, 'ModDetails_label')]");
            Logging.Debug(LogOptions.ClassName, "Searching using html path: {0}", HtmlPath);
            try
            {
                ResultNode = navigator.SelectSingleNode(HtmlPath) as HtmlNodeNavigator;
            }
            catch (XPathException ex)
            {
                Logging.Exception(ex.ToString());
                return false;
            }

            if (ResultNode == null)
            {
                Logging.Info(LogOptions.ClassName, "Result was not found");
                return false;
            }
            else
            {
                Logging.Info(LogOptions.ClassName, "HtmlPath results in node value '{0}' of type '{1}'", ResultNode.InnerXml, ResultNode.NodeType.ToString());
                Logging.Info(LogOptions.ClassName, "Result value as text: {0}\nResult inner html: {1}\nResult outer html: {2}", ResultNode.Value, ResultNode.InnerXml, ResultNode.OuterXml);
                ResultString = ResultNode.ToString();
                return true;
            }
        }

        private void Browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Logging.Debug(LogOptions.ClassName, "The browser reports document completed");
            browserDocumentCompleted = true;
        }

        private void Browser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            Logging.Debug(LogOptions.ClassName, "The browser reports navigation completed");
            browserNavigated = true;
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

        private bool WindowsInterop_SecurityAlertDialogWillBeShown(Boolean blnIsSSLDialog)
        {
            // Return true to ignore and not show the 
            // "Security Alert" dialog to the user
            return true;
        }

        private void RunBrowserOnUIThread()
        {
            Thread thread = new Thread(() =>
            {
                RunBrowser(true);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        private void RunBrowser(bool createNew)
        {
            //setup browser events and params
            if (ThreadMode)
            {
                Browser = new WebBrowser();
                browserDispatcher = Dispatcher.CurrentDispatcher;
            }

            Browser.ScriptErrorsSuppressed = true;
            Browser.Navigated += Browser_Navigated;
            Browser.DocumentCompleted += Browser_DocumentCompleted;

            //setup windows interop events and begin listening for them
            WindowsInterop.SecurityAlertDialogWillBeShown += new GenericDelegate<Boolean, Boolean>(this.WindowsInterop_SecurityAlertDialogWillBeShown);
            WindowsInterop.Hook();

            Logging.Debug(LogOptions.ClassName, "Running Navigate() method to load browser at URL {0}", Url);
            try
            {
                Browser.Navigate(Url);
            }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
                browserNavigated = true;
                browserDocumentCompleted = true;
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
                Browser.Dispose();
                Browser = null;
            });
            browserDispatcher.ShutdownFinished += (sender, args) =>
            {
                browserDispatcher = null;
            };
            browserDispatcher.InvokeShutdown();
        }
    }
}
