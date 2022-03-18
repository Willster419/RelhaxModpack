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
    /// <summary>
    /// An HtmlWebscrapeParser class enables retrieval of a string of html via downloading a web page resource (without javascript) and parsing for specific values using HtmlPath.
    /// </summary>
    public class HtmlWebscrapeParser : HtmlParser, ICancelOperation
    {
        /// <summary>
        /// The location to the web page resource to download.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The last downloaded web page resource.
        /// </summary>
        /// <remarks>If this equals the current web page to download, then the html string will be reused.</remarks>
        protected string lastUrl;

        /// <summary>
        /// The client to use to download the web page resource.
        /// </summary>
        protected WebClient client;

        /// <summary>
        /// Create an instance of the HtmlWebscrapeParser class.
        /// </summary>
        public HtmlWebscrapeParser() : base()
        {

        }

        /// <summary>
        /// Create an instance of the HtmlWebscrapeParser class.
        /// </summary>
        /// <param name="htmlpath">The HtmlPath string to use for navigating the html document.</param>
        /// <param name="url"></param>
        public HtmlWebscrapeParser(string htmlpath, string url) : base(htmlpath)
        {
            this.Url = url;
        }

        /// <summary>
        /// Create an instance of the HtmlWebscrapeParser class.
        /// </summary>
        /// <param name="htmlpath">The HtmlPath string to use for navigating the html document.</param>
        /// <param name="url">The location to the web page resource to download.</param>
        /// <param name="writeHtmlToDisk">Determine if the HtmlParser should write the contents of the html string to disk for debug.</param>
        /// <param name="htmlFilePath">The relative or absolute IO path on disk to write the html string to disk. Must include the file name.</param>
        public HtmlWebscrapeParser(string htmlpath, string url, bool writeHtmlToDisk, string htmlFilePath = null) : base(htmlpath, writeHtmlToDisk, htmlFilePath)
        {
            this.Url = url;
        }

        /// <summary>
        /// Run the parser to retrieve the html string and navigate it via HtmlPath
        /// </summary>
        /// <param name="url">The location to the web page resource to download.</param>
        /// <param name="htmlPath">The HtmlPath string to use for navigating the html document.</param>
        /// <returns>The exit code during the parsing operation.</returns>
        /// <seealso cref="HtmlXpathParserExitCode"/>
        public virtual async Task<HtmlXpathParserExitCode> RunParserAsync(string url, string htmlPath)
        {
            Url = url;
            HtmlPath = htmlPath;
            return await RunParserAsync();
        }

        /// <summary>
        /// Process class variables to ensure it is setup properly.
        /// </summary>
        /// <returns>True if variables are set and parsed correctly, false otherwise.</returns>
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

        /// <summary>
        /// Uses the web client to download the html document at the Url.
        /// </summary>
        /// <returns>True if the document was downloaded to a string, false otherwise.</returns>
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
                Logging.Debug(Logfiles.AutomationRunner, "Download html web page to string");
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

        /// <summary>
        /// Ensures that the url starts with the http(s):// protocol to ensure it's a web resource to download
        /// </summary>
        /// <param name="url">The location to the web page resource to download.</param>
        /// <returns>True if the url starts with http(s), false otherwise.</returns>
        protected bool UrlIsValid(string url)
        {
            return url.StartsWith("http://") || url.StartsWith("https://");
        }

        /// <summary>
        /// Cancels the web client's web page navigation if it's running.
        /// </summary>
        public virtual void Cancel()
        {
            if (client != null)
                client.CancelAsync();
        }
    }
}
