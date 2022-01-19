using HtmlAgilityPack;
using RelhaxModpack.Common;
using RelhaxModpack.Database;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace RelhaxModpack.Automation.Tasks
{
    public class DownloadBrowserTask : DownloadHtmlTask, IDownloadTask, IXmlSerializable
    {
        public const string TaskCommandName = "download_browser";

        public override string Command { get { return TaskCommandName; } }

        public string WaitTimeMs { get; set; } = "1000";

        public string WaitCounts { get; set; } = "3";

        public string BrowserHeight { get; set; } = "0";

        public string BrowserWidth { get; set; } = "0";

        public string BrowserEngine { get; set; } = BrowserType.WebBrowser.ToString();

        protected int waitTimeMs;

        protected int waitCounts;

        protected int browserHeight = 0;

        protected int browserWidth = 0;

        protected BrowserType browserEngine = BrowserType.WebBrowser;

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(WaitTimeMs), nameof(WaitCounts), nameof(BrowserHeight), nameof(BrowserWidth), nameof(BrowserEngine) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            waitTimeMs = int.Parse(ProcessMacro(nameof(WaitTimeMs), WaitTimeMs));
            waitCounts = int.Parse(ProcessMacro(nameof(WaitCounts), WaitCounts));
            browserHeight = int.Parse(ProcessMacro(nameof(BrowserHeight), BrowserHeight));
            browserWidth = int.Parse(ProcessMacro(nameof(BrowserWidth), BrowserWidth));
            browserEngine = (BrowserType)Enum.Parse(typeof(BrowserType), ProcessMacro(nameof(BrowserEngine), BrowserEngine));
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();
            if (ValidateCommandTrue(waitTimeMs <= 0, string.Format("waitTimeMs must be greater then 0. Current value: {0}", waitTimeMs.ToString())))
                return;
            if (ValidateCommandTrue(waitCounts <= 0, string.Format("Retries must be greater then 0. Current value: {0}", waitCounts.ToString())))
                return;
            if (ValidateCommandTrue(browserHeight < 0, string.Format("browserHeight must be greater then or equal to 0. Current value: {0}", browserHeight.ToString())))
                return;
            if (ValidateCommandTrue(browserWidth < 0, string.Format("browserWidth must be greater then or equal to 0. Current value: {0}", browserWidth.ToString())))
                return;
        }

        public async override Task RunTask()
        {
            await base.RunTask();
            (htmlXpathParser as HtmlBrowserParser).CleanupBrowser();
        }

        protected async override Task SetupUrl()
        {
            //add registry entry to use latest IE for script parsing
            Logging.AutomationRunner("Setting application to use latest version of IE for embedded browser", LogLevel.Debug);
            RegistryUtils.SetRegisterKeyForIEVersion(IERegistryVersion.IE11Forced);

            Logging.AutomationRunner("Running Browser execution code");
            htmlXpathParser = new HtmlBrowserParser(HtmlPath, Url, waitTimeMs, waitCounts, false, null, browserEngine) { BrowserHeight = this.browserHeight, BrowserWidth = this.browserWidth };
            parserExitCode = await htmlXpathParser.RunParserAsync();

            Url = htmlXpathParser.ResultString;
        }

        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();
        }

        public override void Cancel()
        {
            base.Cancel();
        }
        #endregion
    }
}
