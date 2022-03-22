using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class MacroSubstringHtmlTask : MacroSubstringTask, IXmlSerializable, ICancelOperation, IHtmlParseTask
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "macro_substring_html";

        public string HtmlPath { get; set; }

        public string Url { get; set; }

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        protected HtmlWebscrapeParser htmlXpathParser;

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

        protected override async Task GetStringValue()
        {
            Logging.AutomationRunner("Running web scrape execution code");
            htmlXpathParser = new HtmlWebscrapeParser(HtmlPath, Url, false, null);
            parserExitCode = await htmlXpathParser.RunParserAsync();

            stringWithValue = htmlXpathParser.ResultString;
            ProcessEscapeCharacters();
        }

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
