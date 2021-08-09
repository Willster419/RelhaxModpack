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
    public class DirectoryListTask : DirectorySearchTask, IXmlSerializable
    {
        public const string TaskCommandName = "directory_search";

        public override string Command { get { return TaskCommandName; } }

        public string MacroPrefix { get; set; }

        #region Xml Serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] {nameof(MacroPrefix) }).ToArray();
        }
        #endregion

        #region Task Execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            MacroPrefix = ProcessMacro(nameof(MacroPrefix), MacroPrefix);
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();
            if (ValidateCommandStringNullEmptyTrue(nameof(MacroPrefix), MacroPrefix))
                return;
        }

        public async override Task RunTask()
        {
            searchResults = FileUtils.FileSearch(DirectoryPath, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly, false, false, SearchPattern);
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
