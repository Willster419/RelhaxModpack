using HtmlAgilityPack;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace RelhaxModpack.Automation
{
    /// <summary>
    /// An HtmlParser class enables retrieval of a string of html and parsing for specific values using HtmlPath.
    /// </summary>
    public abstract class HtmlParser
    {
        /// <summary>
        /// The HtmlPath string to use for navigating the html document.
        /// </summary>
        public string HtmlPath { get; set; }

        /// <summary>
        /// The result of the HtmlPath navigation, if there is a result. Otherwise null/empty.
        /// </summary>
        public string ResultString { get; protected set; }

        /// <summary>
        /// Flag to determine if the HtmlParser should write the contents of the html string to disk for debug.
        /// </summary>
        public bool WriteHtmlToDisk { get; set; }

        /// <summary>
        /// The relative or absolute IO path on disk to write the html string to disk. Must include the file name.
        /// </summary>
        public string HtmlFilePath { get; set; }

        /// <summary>
        /// The result of the HtmlPath navigation, if there is a result. Otherwise null/empty.
        /// </summary>
        public HtmlNodeNavigator ResultNode { get; protected set; }

        /// <summary>
        /// The retrieved html document.
        /// </summary>
        protected string htmlText;

        /// <summary>
        /// Create an instance of the HtlmParser class.
        /// </summary>
        public HtmlParser()
        {

        }

        /// <summary>
        /// Create an instance of the HtlmParser class.
        /// </summary>
        /// <param name="htmlpath">The HtmlPath string to use for navigating the html document.</param>
        public HtmlParser(string htmlpath)
        {
            this.HtmlPath = htmlpath;
        }

        /// <summary>
        /// Create an instance of the HtlmParser class.
        /// </summary>
        /// <param name="htmlpath">The HtmlPath string to use for navigating the html document.</param>
        /// <param name="writeHtmlToDisk">Determine if the HtmlParser should write the contents of the html string to disk for debug.</param>
        /// <param name="htmlfilePath">The relative or absolute IO path on disk to write the html string to disk. Must include the file name.</param>
        public HtmlParser(string htmlpath, bool writeHtmlToDisk, string htmlfilePath = null) : this(htmlpath)
        {
            this.WriteHtmlToDisk = writeHtmlToDisk;
            this.HtmlFilePath = htmlfilePath;
        }

        /// <summary>
        /// Run the parser to retrieve the html string and navigate it via HtmlPath
        /// </summary>
        /// <param name="htmlPath">The HtmlPath string to use for navigating the html document.</param>
        /// <returns>The exit code during the parsing operation.</returns>
        /// <seealso cref="HtmlXpathParserExitCode"/>
        public virtual async Task<HtmlXpathParserExitCode> RunParserAsync(string htmlPath)
        {
            this.HtmlPath = htmlPath;
            return await RunParserAsync();
        }

        /// <summary>
        /// Run the parser to retrieve the html string and navigate it via HtmlPath
        /// </summary>
        /// <returns>The exit code during the parsing operation.</returns>
        /// <seealso cref="HtmlXpathParserExitCode"/>
        public virtual async Task<HtmlXpathParserExitCode> RunParserAsync()
        {
            if (!ProcessVariables())
                return HtmlXpathParserExitCode.InvalidParameters;

            if (!await GetHtmlDocumentAsync())
            {
                return HtmlXpathParserExitCode.ErrorHtmlText;
            }

            if (WriteHtmlToDisk)
            {
                if (!TryWriteHtmlToDisk())
                    return HtmlXpathParserExitCode.ErrorSavingToDisk;
            }

            if (!RunHtmlPathSearch())
            {
                return HtmlXpathParserExitCode.ErrorHtmlParsing;
            }

            return HtmlXpathParserExitCode.None;
        }

        /// <summary>
        /// Process class variables to ensure it is setup properly.
        /// </summary>
        /// <returns>True if variables are set and parsed correctly, false otherwise.</returns>
        protected virtual bool ProcessVariables()
        {
            if (string.IsNullOrEmpty(HtmlPath))
            {
                Logging.Info(LogOptions.ClassName, "The HtmlPath is empty");
                return false;
            }

            if (WriteHtmlToDisk && string.IsNullOrEmpty(HtmlFilePath))
            {
                Logging.Info(LogOptions.ClassName, "WriteToDisk is true and filepath is empty (a filepath must be specified, relative or absolute)");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tries to write the html string to disk.
        /// </summary>
        /// <returns>True if the operation succeeds, false otherwise.</returns>
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

        /// <summary>
        /// Navigate the html document string with the given HtmlPath to get a result.
        /// </summary>
        /// <returns>True if the operation succeeds, false otherwise.</returns>
        protected bool RunHtmlPathSearch()
        {
            ResultString = string.Empty;
            ResultNode = null;
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

        /// <summary>
        /// Gets the html document loaded into a string variable
        /// </summary>
        /// <returns>True if the operation succeeds, false otherwise.</returns>
        protected abstract Task<bool> GetHtmlDocumentAsync();
    }
}
