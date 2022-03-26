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
    /// <summary>
    /// Performs a file download on a url resource by parsing an HtmlPath result of the web page (excludes parsed from JavaScript).
    /// </summary>
    public class DownloadHtmlTask : DownloadStaticTask, IXmlSerializable, IHtmlParseTask, ICancelOperation
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public new const string TaskCommandName = "download_html";

        /// <summary>
        /// The HtmlPath argument to use for parsing.
        /// </summary>
        public string HtmlPath { get; set; } = string.Empty;

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
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(HtmlPath) }).ToArray();
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
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            base.ValidateCommands();
            if (ValidateCommandTrue(string.IsNullOrEmpty(HtmlPath), string.Format("HtmlPath is empty")))
                return;
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            await SetupUrl();
            if (ExitCode == AutomationExitCode.None)
                await DownloadFile();
        }

        /// <summary>
        /// Prepares and runs the Html parser object and sets the result of the parser as this task's url.
        /// </summary>
        protected async virtual Task SetupUrl()
        {
            Logging.AutomationRunner("Running web scrape execution code");
            htmlXpathParser = new HtmlWebscrapeParser(HtmlPath, Url, false, null);
            parserExitCode = await htmlXpathParser.RunParserAsync();

            Url = htmlXpathParser.ResultString;
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
        /// Cancels the download operation.
        /// </summary>
        public override void Cancel()
        {
            if (htmlXpathParser != null)
                htmlXpathParser.Cancel();

            base.Cancel();
        }
        #endregion
    }
}
