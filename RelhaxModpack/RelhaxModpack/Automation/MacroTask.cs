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
            AutomationMacro macro = Macros.Find(mac => mac.Name.Equals(MacroName));
            if (macro != null)
                Macros.Remove(macro);
        }
    }
}
