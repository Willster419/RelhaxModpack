using RelhaxModpack.Database;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace RelhaxModpack.Automation.Tasks
{
    public class MacroStringSplitBrowserTask : MacroStringSplitHtmlTask, IXmlSerializable, IHtmlParseTask
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "macro_string_split_browser";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
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
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(WaitTimeMs), nameof(WaitCounts), nameof(BrowserHeight), nameof(BrowserWidth), nameof(BrowserEngine) }).ToArray();
        }
        #endregion

        #region Task execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            waitTimeMs = int.Parse(ProcessMacro(nameof(WaitTimeMs), WaitTimeMs));
            waitCounts = int.Parse(ProcessMacro(nameof(WaitCounts), WaitCounts));
            browserHeight = int.Parse(ProcessMacro(nameof(BrowserHeight), BrowserHeight));
            browserWidth = int.Parse(ProcessMacro(nameof(BrowserWidth), BrowserWidth));
            browserEngine = (BrowserType)Enum.Parse(typeof(BrowserType), ProcessMacro(nameof(BrowserEngine), BrowserEngine));
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
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
            (htmlXpathParser as HtmlBrowserParser).Dispose();
        }

        protected override async Task GetStringValue()
        {
            //add registry entry to use latest IE for script parsing
            Logging.AutomationRunner("Setting application to use latest version of IE for embedded browser", LogLevel.Debug);
            RegistryUtils.SetRegisterKeyForIEVersion(IERegistryVersion.IE11Forced);

            Logging.AutomationRunner("Running Browser execution code");
            htmlXpathParser = new HtmlBrowserParser(HtmlPath, Url, waitTimeMs, waitCounts, false, null, browserEngine) { BrowserHeight = this.browserHeight, BrowserWidth = this.browserWidth };
            parserExitCode = await htmlXpathParser.RunParserAsync();

            stringWithValue = htmlXpathParser.ResultString;
            ProcessEscapeCharacters();
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
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
