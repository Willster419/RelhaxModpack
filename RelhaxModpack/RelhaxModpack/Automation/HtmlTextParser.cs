using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    /// <summary>
    /// An HtmlTextParser allows for HtmlPath navigation of an html document already loaded as a string.
    /// </summary>
    public class HtmlTextParser : HtmlParser
    {
        /// <summary>
        /// Create an instance of the HtmlTextParser class.
        /// </summary>
        public HtmlTextParser() : base()
        {

        }

        /// <summary>
        /// Create an instance of the HtmlTextParser class.
        /// </summary>
        /// <param name="htmlPath">The HtmlPath string to use for navigating the html document.</param>
        public HtmlTextParser(string htmlPath) : base(htmlPath)
        {

        }

        /// <summary>
        /// Create an instance of the HtmlTextParser class.
        /// </summary>
        /// <param name="htmlPath">The HtmlPath string to use for navigating the html document.</param>
        /// <param name="writeHtmlToDisk">Flag to determine if the HtmlParser should write the contents of the html string to disk for debug.</param>
        /// <param name="htmlText">The html document to parse.</param>
        /// <param name="htmlFilepath">The relative or absolute IO path on disk to write the html string to disk. Must include the file name.</param>
        public HtmlTextParser(string htmlPath, bool writeHtmlToDisk, string htmlText, string htmlFilepath = null) : base(htmlPath, writeHtmlToDisk, htmlFilepath)
        {
            this.htmlText = htmlText;
        }

        /// <summary>
        /// Ensures that the html text is already loaded into the instance.
        /// </summary>
        /// <returns>True if the html text is already loaded, false otherwise.</returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<bool> GetHtmlDocumentAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (string.IsNullOrEmpty(htmlText))
            {
                Logging.Info(LogOptions.ClassName, "htmlText is empty");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Set the html document string.
        /// </summary>
        /// <param name="htmlText">The html document to parse.</param>
        public void SetHtmlText(string htmlText)
        {
            this.htmlText = htmlText;
        }
    }
}
