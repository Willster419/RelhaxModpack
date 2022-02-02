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
    public abstract class HtmlParser
    {
        public string HtmlPath { get; set; }

        public string ResultString { get; protected set; }

        public bool WriteHtmlToDisk { get; set; }

        public string HtmlFilePath { get; set; }

        public HtmlNodeNavigator ResultNode { get; protected set; }

        protected string htmlText;

        public HtmlParser()
        {

        }

        public HtmlParser(string htmlpath)
        {
            this.HtmlPath = htmlpath;
        }

        public HtmlParser(string htmlpath, bool writeHtmlToDisk, string htmlfilepath = null) : this(htmlpath)
        {
            this.WriteHtmlToDisk = writeHtmlToDisk;
            this.HtmlFilePath = htmlfilepath;
        }

        public virtual async Task<HtmlXpathParserExitCode> RunParserAsync(string htmlPath)
        {
            this.HtmlPath = htmlPath;
            return await RunParserAsync();
        }

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

        protected abstract Task<bool> GetHtmlDocumentAsync();
    }
}
