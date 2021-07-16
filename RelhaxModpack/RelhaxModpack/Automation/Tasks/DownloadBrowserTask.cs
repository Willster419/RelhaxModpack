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

        protected string HtmlString = null;

        protected int waitTimeMs { get; set; }

        protected int waitCounts { get; set; }

        protected int BrowserFinishedLoadingScriptsCounter = 0;

        protected bool BrowserLoaded = false;

        protected bool BrowserNavigated = false;

        protected Dispatcher browserDispatcher = null;

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(WaitTimeMs), nameof(WaitCounts) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            waitTimeMs = int.Parse(ProcessMacro(nameof(WaitTimeMs), WaitTimeMs));
            waitCounts = int.Parse(ProcessMacro(nameof(WaitCounts), WaitCounts));
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();
            if (ValidateCommandTrue(waitTimeMs <= 0, string.Format("waitTimeMs must be greater then 0. Current value: {0}", waitTimeMs.ToString())))
                return;

            if (ValidateCommandTrue(waitCounts <= 0, string.Format("Retries must be greater then 0. Current value: {0}", waitCounts.ToString())))
                return;
        }

        public async override Task RunTask()
        {
            await base.RunTask();
        }

        protected async override Task SetupUrl()
        {
            //add registry entry to use latest IE for script parsing
            Logging.AutomationRunner("Setting application to use latest version of IE for embedded browser", LogLevel.Debug);
            RegistryUtils.SetRegisterKeyForIEVersion(IERegistryVersion.IE11Forced);

            Logging.AutomationRunner("Running Browser execution code");
            htmlXpathParser = new HtmlBrowserParser(HtmlPath, Url, waitTimeMs, waitCounts, false, null, null);
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
