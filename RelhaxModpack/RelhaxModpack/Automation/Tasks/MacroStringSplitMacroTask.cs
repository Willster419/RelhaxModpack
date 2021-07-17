using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class MacroStringSplitMacroTask : MacroStringSplitTask, IXmlSerializable
    {
        public const string TaskCommandName = "macro_string_split_macro";

        public override string Command { get { return TaskCommandName; } }

        public string InputText { get; set; }

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(InputText) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            InputText = ProcessMacro(nameof(InputText), InputText);
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (ValidateCommandStringNullEmptyTrue(nameof(InputText), InputText))
                return;
        }

        public override async Task RunTask()
        {
            await base.RunTask();
        }

        protected override async Task GetStringValue()
        {
            stringWithValue = InputText;
        }

        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();
        }
        #endregion
    }
}
