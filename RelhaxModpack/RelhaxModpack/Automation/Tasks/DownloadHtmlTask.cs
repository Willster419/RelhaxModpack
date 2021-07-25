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

namespace RelhaxModpack.Automation.Tasks
{
    public class DownloadHtmlTask : DownloadStaticTask, IDownloadTask, IXmlSerializable
    {
        public const string TaskCommandName = "download_html";

        public string HtmlPath { get; set; } = string.Empty;

        public override string Command { get { return TaskCommandName; } }

        protected HtmlWebscrapeParser htmlXpathParser;

        protected HtmlXpathParserExitCode parserExitCode;

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(HtmlPath) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            HtmlPath = ProcessMacro(nameof(HtmlPath), HtmlPath);
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();
            if (ValidateCommandTrue(string.IsNullOrEmpty(HtmlPath), string.Format("HtmlPath is empty")))
                return;
        }

        public async override Task RunTask()
        {
            await SetupUrl();
            if (ExitCode == AutomationExitCode.None)
                await DownloadFile();
        }

        protected async virtual Task SetupUrl()
        {
            Logging.AutomationRunner("Running web scrape execution code");
            htmlXpathParser = new HtmlWebscrapeParser(HtmlPath, Url, false, null);
            parserExitCode = await htmlXpathParser.RunParserAsync();

            Url = htmlXpathParser.ResultString;
        }

        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();

            if (ProcessTaskResultFalse(parserExitCode == HtmlXpathParserExitCode.None, string.Format("The html browser parser exited with code {0}", parserExitCode)))
                return;
        }

        public override void Cancel()
        {
            if (htmlXpathParser != null)
                htmlXpathParser.Cancel();

            base.Cancel();
        }
        #endregion
    }
}
