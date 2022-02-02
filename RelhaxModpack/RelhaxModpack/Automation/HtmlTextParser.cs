using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class HtmlTextParser : HtmlParser
    {
        public HtmlTextParser() : base()
        {

        }

        public HtmlTextParser(string htmlPath) : base(htmlPath)
        {

        }

        public HtmlTextParser(string htmlPath, bool writeHtmlToDisk, string htmlText, string htmlFilepath = null) : base(htmlPath, writeHtmlToDisk, htmlFilepath)
        {
            this.htmlText = htmlText;
        }

        protected override async Task<bool> GetHtmlDocumentAsync()
        {
            if (string.IsNullOrEmpty(htmlText))
            {
                Logging.Info(LogOptions.ClassName, "htmlText is empty");
                return false;
            }

            return true;
        }

        public void SetHtmlText(string htmlText)
        {
            this.htmlText = htmlText;
        }
    }
}
