using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public abstract class MacroTask : AutomationTask
    {
        public string MacroName { get; set; }

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(MacroName) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            MacroName = ProcessMacro(nameof(MacroName), MacroName);
        }

        public override void ValidateCommands()
        {
            if (ValidateCommandTrue(string.IsNullOrEmpty(MacroName), "The arg MacroName is empty string"))
                return;
        }

        public override async Task RunTask()
        {
            Logging.Debug("Checking for if macro {0} already exists", MacroName);
            AutomationMacro macro = Macros.Find(mac => mac.Name.Equals(MacroName));
            if (macro != null)
            {
                Logging.Debug("Macro found, removing");
                Macros.Remove(macro);
            }
        }
        #endregion
    }
}
