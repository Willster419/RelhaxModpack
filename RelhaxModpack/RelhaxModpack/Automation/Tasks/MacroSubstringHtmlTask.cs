using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// Allows for creation of a macro from a parsed HTML web page (without JavaScsript), using the Substring method.
    /// </summary>
    public class MacroSubstringHtmlTask : MacroSubstringTask, IXmlSerializable, ICancelOperation, IHtmlParseTask
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "macro_substring_html";

        /// <summary>
        /// The HtmlPath argument to use for parsing.
        /// </summary>
        public string HtmlPath { get; set; }

        /// <summary>
        /// The url to get the web page to parse.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        /// <summary>
        /// The HtmlPath parser to use to processing the HTML web page.
        /// </summary>
        protected HtmlWebscrapeParser htmlXpathParser;

        /// <summary>
        /// The exit code from the HtmlPath parser when executed.
        /// </summary>
        protected HtmlXpathParserExitCode parserExitCode;

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(HtmlPath), nameof(Url) }).ToArray();
        }
        #endregion

        #region Task execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            HtmlPath = ProcessMacro(nameof(HtmlPath), HtmlPath);
            Url = ProcessMacro(nameof(Url), Url);
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (ValidateCommandStringNullEmptyTrue(nameof(HtmlPath), HtmlPath))
                return;
            if (ValidateCommandStringNullEmptyTrue(nameof(Url), Url))
                return;
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            await base.RunTask();
        }

        /// <summary>
        /// Get the string to use for macro creation by preparing and running the HtmlParser and saving its result.
        /// </summary>
        protected override async Task GetStringValue()
        {
            Logging.AutomationRunner("Running web scrape execution code");
            htmlXpathParser = new HtmlWebscrapeParser(HtmlPath, Url, false, null);
            parserExitCode = await htmlXpathParser.RunParserAsync();

            stringWithValue = htmlXpathParser.ResultString;
            ProcessEscapeCharacters();
        }

        /// <summary>
        /// Replace HTML/XML escape characters with their literals.
        /// </summary>
        protected override void ProcessEscapeCharacters()
        {
            base.ProcessEscapeCharacters();
            if (string.IsNullOrEmpty(stringWithValue))
                return;
            else
                stringWithValue = stringWithValue.Replace("&quot;", "\"").Replace("&apos;", "'").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&");
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();

            if (ProcessTaskResultFalse(parserExitCode == HtmlXpathParserExitCode.None, string.Format("The html browser parser exited with code {0}", parserExitCode)))
                return;
        }

        /// <summary>
        /// Sends a cancellation request to task's current operation.
        /// </summary>
        public virtual void Cancel()
        {
            if (htmlXpathParser != null)
                htmlXpathParser.Cancel();
        }
        #endregion
    }
}
