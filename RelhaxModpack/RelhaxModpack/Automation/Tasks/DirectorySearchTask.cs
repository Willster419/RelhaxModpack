using RelhaxModpack.Database;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class DirectorySearchTask : DirectoryTask, IXmlSerializable
    {
        public const string TaskCommandName = "directory_search";

        public override string Command { get { return TaskCommandName; } }

        public string SearchPath { get; set; }

        public string Recursive { get; set; }

        public string MacroPrefix { get; set; }

        protected bool recursive;

        protected string[] searchResults;

        protected bool ableToParseRecursive = false;

        #region Xml Serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(SearchPath), nameof(Recursive), nameof(MacroPrefix) }).ToArray();
        }
        #endregion

        #region Task Execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            SearchPath = ProcessMacro(nameof(SearchPath), SearchPath);
            Recursive = ProcessMacro(nameof(Recursive), Recursive);
            MacroPrefix = ProcessMacro(nameof(MacroPrefix), MacroPrefix);

            if (bool.TryParse(Recursive, out bool result))
            {
                ableToParseRecursive = true;
                recursive = result;
            }
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();
            if (ValidateCommandStringNullEmptyTrue(nameof(SearchPath), SearchPath))
                return;
            if (ValidateCommandStringNullEmptyTrue(nameof(Recursive), Recursive))
                return;
            if (ValidateCommandStringNullEmptyTrue(nameof(MacroPrefix), MacroPrefix))
                return;

            if (ValidateCommandFalse(ableToParseRecursive, string.Format("Unable to parse the arg Recursive from given string {0}", Recursive)))
                return;
        }

        public async override Task RunTask()
        {
            searchResults = FileUtils.DirectorySearch(DirectoryPath, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly, false, SearchPath);

            if (searchResults == null || searchResults.Count() == 0)
                return;

            for (int i = 0; i < searchResults.Count(); i++)
            {
                string macroNameToAdd = string.Format("{0}_{1}", MacroPrefix, i);
                string macroValueToAdd = searchResults[i];
                Logging.Info("Creating macro, Name: {0}, Value: {1}", macroNameToAdd, macroValueToAdd);
                Macros.Add(new AutomationMacro() { MacroType = MacroType.Local, Name = macroNameToAdd, Value = macroValueToAdd });
            }
        }

        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultTrue(searchResults == null, "The searchResult array returned null"))
                return;

            if (ProcessTaskResultTrue(searchResults.Count() == 0, "The searchResult array returned 0 results"))
                return;
        }
        #endregion
    }
}
