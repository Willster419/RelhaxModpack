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
    public class HtmlWebscrapeParser : HtmlParser, ICancelOperation
    {
        public string Url { get; set; }

        protected string lastUrl;

        protected WebClient client;

        public HtmlWebscrapeParser() : base()
        {

        }

        public HtmlWebscrapeParser(string htmlpath, string url) : base(htmlpath)
        {
            this.Url = url;
        }

        public HtmlWebscrapeParser(string htmlpath, string url, bool writeHtmlToDisk, string htmlFilePath = null) : base(htmlpath, writeHtmlToDisk, htmlFilePath)
        {
            this.Url = url;
        }

        public virtual async Task<HtmlXpathParserExitCode> RunParserAsync(string url, string htmlPath)
        {
            Url = url;
            HtmlPath = htmlPath;
            return await RunParserAsync();
        }

        protected override bool ProcessVariables()
        {
            if (!base.ProcessVariables())
                return false;

            //check if the url is valid
            //https://stackoverflow.com/a/3808841/3128017
            if (string.IsNullOrEmpty(Url))
            {
                Logging.Error(LogOptions.ClassName, "The URL is empty");
                return false;
            }

            if (!UrlIsValid(Url))
            {
                Logging.Info(LogOptions.ClassName, "The URL is invalid");
                return false;
            }

            return true;
        }

        protected override async Task<bool> GetHtmlDocumentAsync()
        {
            if (!string.IsNullOrEmpty(lastUrl) && lastUrl.Equals(Url))
            {
                Logging.Debug(LogOptions.ClassName, "The last URL did not change, we can skip the browser run");
                return true;
            }
            else
            {
                Logging.Debug(LogOptions.ClassName, "The last URL changed or is null, load the browser page");
                lastUrl = Url;
            }

            using (client = new WebClient() { Encoding = Encoding.UTF8 })
            {
                //https://stackoverflow.com/questions/2953403/c-sharp-passing-method-as-the-argument-in-a-method
                Logging.Debug(Logfiles.AutomationRunner, "Download html webpage to string");
                try
                {
                    htmlText = await client.DownloadStringTaskAsync(Url);
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

            return true;
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
