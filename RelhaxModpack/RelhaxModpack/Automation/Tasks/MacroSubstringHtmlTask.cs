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
        public const string TaskCommandName = "macro_substring_html";

        public string HtmlPath { get; set; }

        public string Url { get; set; }

        public override string Command { get { return TaskCommandName; } }

        protected HtmlWebscrapeParser htmlXpathParser;

        protected HtmlXpathParserExitCode parserExitCode;

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(HtmlPath), nameof(Url) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            HtmlPath = ProcessMacro(nameof(HtmlPath), HtmlPath);
            Url = ProcessMacro(nameof(Url), Url);
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (ValidateCommandStringNullEmptyTrue(nameof(HtmlPath), HtmlPath))
                return;
            if (ValidateCommandStringNullEmptyTrue(nameof(Url), Url))
                return;
        }

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
            stringWithValue = stringWithValue.Replace("&quot;", "\"").Replace("&apos;", "'").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&");
        }

        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();

            if (ProcessTaskResultFalse(parserExitCode == HtmlXpathParserExitCode.None, string.Format("The html browser parser exited with code {0}", parserExitCode)))
                return;
        }

        public virtual void Cancel()
        {
            if (htmlXpathParser != null)
                htmlXpathParser.Cancel();
        }
        #endregion
    }
}
