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
    /// <summary>
    /// Allows for creation of a macro from a parsed HTML web page (with JavaScsript), using the Split method.
    /// </summary>
    public class MacroStringSplitBrowserTask : MacroStringSplitHtmlTask, IXmlSerializable, IHtmlParseTask
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public new const string TaskCommandName = "macro_string_split_browser";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        /// <summary>
        /// The time, in milliseconds, to wait after the first NavigationCompleted event is fired (or a previous wait).
        /// </summary>
        /// <remarks>The total wait time can be calculated as WaitTimeMs * WaitCounts.</remarks>
        /// <seealso cref="WaitCounts"/>
        public string WaitTimeMs { get; set; } = "1000";

        /// <summary>
        /// The number of times the browser should wait for WaitTimeMs after the first NavigationCompleted event is fired.
        /// </summary>
        /// <remarks>The total wait time can be calculated as WaitTimeMs * WaitCounts.</remarks>
        /// <seealso cref="WaitTimeMs"/>
        public string WaitCounts { get; set; } = "3";

        /// <summary>
        /// Gets or sets the height of the browser.
        /// </summary>
        public string BrowserHeight { get; set; } = "0";

        /// <summary>
        /// Gets or sets the width of the browser.
        /// </summary>
        public string BrowserWidth { get; set; } = "0";

        /// <summary>
        /// Sets the browser api implementation to use for this browser download operation.
        /// </summary>
        public string BrowserEngine { get; set; } = BrowserType.WebBrowser.ToString();

        /// <summary>
        /// Parsed result of the argument WaitTimeMs.
        /// </summary>
        /// <seealso cref="WaitTimeMs"/>
        protected int waitTimeMs;

        /// <summary>
        /// Parsed result of the argument WaitCounts.
        /// </summary>
        /// <seealso cref="WaitCounts"/>
        protected int waitCounts;

        /// <summary>
        /// Parsed result of the argument BrowserHeight.
        /// </summary>
        /// <seealso cref="BrowserHeight"/>
        protected int browserHeight = 0;

        /// <summary>
        /// Parsed result of the argument BrowserWidth.
        /// </summary>
        /// <seealso cref="BrowserWidth"/>
        protected int browserWidth = 0;

        /// <summary>
        /// Parsed result of the argument BrowserEngine.
        /// </summary>
        /// <seealso cref="BrowserEngine"/>
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

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            await base.RunTask();
            (htmlXpathParser as HtmlBrowserParser).Dispose();
        }

        /// <summary>
        /// Ensures this application can use the latest version of the embedded IE browser, and gets the string to use for macro creation by preparing and running the HtmlParser and saving its result.
        /// </summary>
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

        /// <summary>
        /// Sends a cancellation request to task's current operation.
        /// </summary>
        public override void Cancel()
        {
            base.Cancel();
        }
        #endregion
    }
}
