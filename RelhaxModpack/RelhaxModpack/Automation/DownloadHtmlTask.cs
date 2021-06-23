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
        public const string TaskCommandName = "download_html";

        public string HtmlPath { get; set; } = string.Empty;

        public override string Command { get { return TaskCommandName; } }

        protected HtmlWebscrapeParser htmlXpathParser;

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
            DownloadSetup();
            await SetupUrl();
            await DownloadFile();
        }

        protected async virtual Task SetupUrl()
        {
            Logging.AutomationRunner("Running web scrape execution code");
            htmlXpathParser = new HtmlWebscrapeParser(HtmlPath, Url, false, null);
            HtmlXpathParserExitCode exitCode = await htmlXpathParser.RunParserAsync();

            if (exitCode != HtmlXpathParserExitCode.None)
            {
                ExitCode = 3;
                ErrorMessage = string.Format("ExitCode {0}: The html browser parser exited with code {1}. Check the above log messages for more information.", exitCode);
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
                return;
            }

            Url = htmlXpathParser.ResultString;
        }

        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();
        }
        #endregion
    }
}
