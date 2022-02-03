using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public abstract class BrowserSessionMacroTask : BrowserSessionUrlTask
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
            base.ProcessMacros();
            MacroName = ProcessMacro(nameof(MacroName), MacroName);
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();
            if (ValidateCommandTrue(string.IsNullOrEmpty(MacroName), "The arg MacroName is empty string"))
                return;
        }

        protected virtual bool CheckIfMacroExits()
        {
            Logging.Debug("Checking for if macro {0} already exists", MacroName);
            AutomationMacro macro = Macros.Find(mac => mac.Name.Equals(MacroName));
            if (macro != null)
            {
                if (macro.MacroType == Utilities.Enums.MacroType.ApplicationDefined)
                {
                    Logging.Error("Cannot replace value of application defined macro {0}", macro.Name);
                    return false;
                }
                else if (macro.MacroType == Utilities.Enums.MacroType.Global)
                {
                    Logging.Warning("Replacing global macro {0}", macro.Name);
                }
                Logging.Debug("Macro found, removing");
                Macros.Remove(macro);
            }
            return true;
        }

        protected virtual void CreateMacro(string value)
        {
            Logging.Info("Creating macro, Name: {0}, Value: {1}", MacroName, value);
            Macros.Add(new AutomationMacro() { MacroType = MacroType.Local, Name = MacroName, Value = value });
        }
        #endregion
    }
}
