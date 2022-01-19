using HtmlAgilityPack;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace RelhaxModpack.Automation
{
    public class HtmlWebscrapeParser : ICancelOperation
    {
        public string HtmlPath { get; set; }

        public string Url { get; set; }

        public bool WriteHtmlToDisk { get; set; }

        public string HtmlFilePath { get; set; }

        public string ResultString { get; private set; }

        public HtmlNodeNavigator ResultNode { get; private set; }

        protected string lastUrl;

        protected string htmlText;

        protected WebClient client;

        public HtmlWebscrapeParser()
        {

        }

        public HtmlWebscrapeParser(string htmlpath, string url)
        {
            this.HtmlPath = htmlpath;
            this.Url = url;
        }

        public HtmlWebscrapeParser(string htmlpath, string url, bool writeHtmlToDisk, string htmlFilePath): this(htmlpath, url)
        {
            this.WriteHtmlToDisk = writeHtmlToDisk;
            this.HtmlFilePath = htmlFilePath;
        }

        public virtual async Task<HtmlXpathParserExitCode> RunParserAsync(string url, string htmlPath)
        {
            Url = url;
            HtmlPath = htmlPath;
            return await RunParserAsync();
        }

        public virtual async Task<HtmlXpathParserExitCode> RunParserAsync()
        {
            //check if the url is valid
            //https://stackoverflow.com/a/3808841/3128017
            if (string.IsNullOrEmpty(Url))
            {
                Logging.Error(LogOptions.ClassName, "The URL is empty");
                return HtmlXpathParserExitCode.InvalidParameters;
            }

            if (!UrlIsValid(Url))
            {
                Logging.Info(LogOptions.ClassName, "The URL is invalid");
                return HtmlXpathParserExitCode.InvalidParameters;
            }

            if (string.IsNullOrEmpty(HtmlPath))
            {
                Logging.Info(LogOptions.ClassName, "The HtmlPath is empty");
                return HtmlXpathParserExitCode.InvalidParameters;
            }

            if (WriteHtmlToDisk && string.IsNullOrEmpty(HtmlFilePath))
            {
                Logging.Info(LogOptions.ClassName, "WriteToDisk is high and filepath is empty");
                return HtmlXpathParserExitCode.InvalidParameters;
            }

            if (!string.IsNullOrEmpty(lastUrl) && lastUrl.Equals(Url))
            {
                Logging.Info(LogOptions.ClassName, "The last URL did not change, we can skip the browser run");
            }
            else
            {
                Logging.Info(LogOptions.ClassName, "The last URL changed or is null, load the browser page");
                lastUrl = Url;
                if (!await GetHtmlDocumentAsync())
                {
                    return HtmlXpathParserExitCode.ErrorBrowserNavigation;
                }
            }

            if (!RunHtmlPathSearch())
            {
                return HtmlXpathParserExitCode.ErrorHtmlParsing;
            }

            return HtmlXpathParserExitCode.None;
        }

        protected virtual async Task<bool> GetHtmlDocumentAsync()
        {
            //reset internals
            ResultString = string.Empty;
            ResultNode = null;

            using (client = new WebClient())
            {
                //https://stackoverflow.com/questions/2953403/c-sharp-passing-method-as-the-argument-in-a-method
                Logging.Info(Logfiles.AutomationRunner, "Download html webpage to string");
                try
                {
                    byte[] bytes = Encoding.Default.GetBytes(await client.DownloadStringTaskAsync(Url));
                    htmlText = Encoding.UTF8.GetString(bytes);
                }
                catch (OperationCanceledException) { }
                catch (WebException wex)
                {
                    if (wex.Status != WebExceptionStatus.RequestCanceled)
                    {
                        Logging.Exception("Failed to download URL as string");
                        Logging.Exception(wex.ToString());
                    }
                    return false;
                }
            }

            if (WriteHtmlToDisk)
            {
                //save to string
                if (!TryWriteHtmlToDisk())
                    return false;
            }

            return true;
        }

        protected bool TryWriteHtmlToDisk()
        {
            Logging.Info(LogOptions.ClassName, "Writing HTML to {0}", HtmlFilePath);
            try
            {
                File.WriteAllText(HtmlFilePath, htmlText);
            }
            catch (Exception ex2)
            {
                Logging.Exception("Failed to write to disk");
                Logging.Exception(ex2.ToString());
                return false;
            }
            Logging.Info(LogOptions.ClassName, "The browser reports all loading done, save html to string");
            return true;
        }

        protected bool RunHtmlPathSearch()
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
                Logging.Info(LogOptions.ClassName, "HtmlPath results in node of type '{0}'", ResultNode.NodeType.ToString());
                Logging.Info(LogOptions.ClassName, "Result value as text: {0}\nResult inner html: {1}\nResult outer html: {2}", ResultNode.Value, ResultNode.InnerXml, ResultNode.OuterXml);
                ResultString = ResultNode.ToString();
                if (ResultNode.NodeType == XPathNodeType.Root)
                {
                    Logging.Info("The type is root, returning inner html value");
                    ResultString = ResultNode.InnerXml;
                }
                return true;
            }
        }

        protected bool UrlIsValid(string url)
        {
            return url.StartsWith("http://") || url.StartsWith("https://");
        }

        public virtual void Cancel()
        {
            if (client != null)
                client.CancelAsync();
        }
    }
}
