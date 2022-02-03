using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public abstract class BrowserSessionParseTask : BrowserSessionMacroTask, IHtmlParseTask
    {
        public string ParseResult { get; set; }

        public string HtmlPath { get; set; } = string.Empty;

        public string WriteHtmlResult { get; set; }

        protected bool parseResult;

        protected bool writeHtmlResult = false;

        protected string htmlText;

        protected string htmlPathResult;

        protected HtmlTextParser htmlTextParser;

        protected HtmlXpathParserExitCode parserExitCode;

        protected bool htmlStringGotten;

        protected bool macroSetup;

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(ParseResult), nameof(HtmlPath), nameof(WriteHtmlResult) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            parseResult = bool.Parse(ProcessMacro(nameof(ParseResult), ParseResult));
            if (parseResult)
            {
                HtmlPath = ProcessMacro(nameof(HtmlPath), HtmlPath);
                writeHtmlResult = bool.Parse(ProcessMacro(nameof(WriteHtmlResult), WriteHtmlResult));
            }
        }

        public override void ValidateCommands()
        {
            //don't call base, need to do it manually because parsing macro is based on parseResult
            if (ValidateCommandTrue(string.IsNullOrEmpty(Url), string.Format("The parameter {0} is null or empty", nameof(Url))))
                return;
            if (ValidateCommandTrue(parseResult && string.IsNullOrEmpty(MacroName), "The arg MacroName is empty string"))
                return;
            if (ValidateCommandTrue(parseResult && string.IsNullOrEmpty(HtmlPath), string.Format("ParseResult is true but HtmlPath is null or empty")))
                return;
            if (ValidateCommandTrue(parseResult && string.IsNullOrEmpty(WriteHtmlResult), string.Format("ParseResult is true but WriteHtmlResult is null or empty")))
                return;
        }

        public override async Task RunTask()
        {
            htmlStringGotten = await GetHtmlString();
            if (!htmlStringGotten)
                return;
            if (string.IsNullOrEmpty(htmlText) && parseResult)
            {
                return;
            }
            else if (parseResult)
            {
                macroSetup = CheckIfMacroExits();
                if (!macroSetup)
                {
                    return;
                }
                ParseHtmlResult();
                if (parserExitCode == HtmlXpathParserExitCode.None)
                {
                    CreateMacro(htmlPathResult);
                }
            }
        }

        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultFalse(htmlStringGotten, "Failed to get the Html string"))
                return;
            if (ProcessTaskResultTrue(string.IsNullOrEmpty(htmlText) && parseResult, "Cannot parse an empty response string"))
                return;
            if (ProcessTaskResultTrue(parseResult && !macroSetup, "Failed to create desired macro name when parsing html result"))
                return;
            if (ProcessTaskResultFalse(parserExitCode == HtmlXpathParserExitCode.None, string.Format("The html parser exited with code {0}", parserExitCode)))
                return;
        }

        public virtual async Task<HtmlXpathParserExitCode> ParseHtmlResult()
        {
            Logging.Debug("Parsing HTML result with HtmlPath {0}", HtmlPath);
            htmlTextParser = new HtmlTextParser(HtmlPath, writeHtmlResult, htmlText, ID + ".html");
            parserExitCode = await htmlTextParser.RunParserAsync();
            htmlPathResult = htmlTextParser.ResultString;
            htmlPathResult = htmlPathResult.Replace("&quot;", "\"").Replace("&apos;", "'").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&");
            return parserExitCode;
        }

        protected abstract Task<bool> GetHtmlString();
        #endregion
    }
}
