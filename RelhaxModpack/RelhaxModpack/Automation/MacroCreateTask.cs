using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class MacroCreateTask : MacroTask
    {
        public const string TaskCommandName = "macro_create";

        public override string Command { get { return TaskCommandName; } }

        public string MacroValue { get; set; }

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(MacroValue) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            MacroValue = ProcessMacro(nameof(MacroValue), MacroValue);
        }

        public override void ValidateCommands()
        {
            if (ValidateCommandTrue(string.IsNullOrEmpty(MacroValue), "The arg MacroValue is empty string"))
                return;
        }

        public override async Task RunTask()
        {
            Logging.Info("Deleting macro if already exists");
            await base.RunTask();

            Logging.Info("Creating macro, Name: {0}, Value: {1}", MacroName, MacroValue);
            Macros.Add(new AutomationMacro() { MacroType = MacroType.Local, Name = MacroName, Value = MacroValue });
        }

        public override void ProcessTaskResults()
        {
            AutomationMacro macro = Macros.Find(mac => mac.Name.Equals(MacroName));
            if (ProcessTaskResultTrue(macro == null, "Could not find newly created macro in list"))
                return;
        }
        #endregion
    }
}
