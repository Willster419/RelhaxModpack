using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace RelhaxModpack.Automation
{
    public class DownloadHtmlTask : DownloadStaticTask, IDownloadTask, IXmlSerializable
    {
        public string HtmlPath { get; set; } = string.Empty;

        public override string Command { get; } = "download_html";

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(HtmlPath) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ValidateCommands()
        {
            base.ValidateCommands();
            if (string.IsNullOrEmpty(HtmlPath))
            {
                ExitCode = 1;
                ErrorMessage = string.Format("ExitCode {0}: HtmlPath is empty", ExitCode);
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
                return;
            }
        }

        public async override Task RunTask()
        {
            using (WebClient = new WebClient())
            {
                //https://stackoverflow.com/questions/2953403/c-sharp-passing-method-as-the-argument-in-a-method
                Logging.Info(Logfiles.AutomationRunner, "Download html webpage to string");
                string webpageHtmlString =  await WebClient.DownloadStringTaskAsync(Url);
                Logging.Info(Logfiles.AutomationRunner, "Running htmlpath to get download file url");
                Logging.Debug(Logfiles.AutomationRunner, "The htmlpath used was {0}", HtmlPath);
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(webpageHtmlString);
                HtmlNode node = document.DocumentNode;
                //https://stackoverflow.com/questions/1390568/how-can-i-match-on-an-attribute-that-contains-a-certain-string
                //sample html xpath: //div[contains(@class, 'ModDetails_label')]
                HtmlNode resultNode = node.SelectSingleNode(HtmlPath);
                Logging.Debug(Logfiles.AutomationRunner, "Htmlpath results in node value '{0}' of type '{1}'", resultNode.InnerText, resultNode.NodeType.ToString());
                Url = resultNode.InnerText;
            }
            base.RunTask();
            //how to run from a grandbase example
            //(this as AutomationTask).RunTask();
        }

        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();
        }
        #endregion
    }
}
