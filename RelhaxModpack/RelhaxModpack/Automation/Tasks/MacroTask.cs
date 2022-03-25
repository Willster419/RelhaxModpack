using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// Provides saving of text to a macro.
    /// </summary>
    public abstract class MacroTask : AutomationTask
    {
        /// <summary>
        /// The name of the macro to create.
        /// </summary>
        public string MacroName { get; set; }

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(MacroName) }).ToArray();
        }
        #endregion

        #region Task execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            MacroName = ProcessMacro(nameof(MacroName), MacroName);
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            if (ValidateCommandTrue(string.IsNullOrEmpty(MacroName), "The argument MacroName is empty string"))
                return;
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task RunTask()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            CheckIfMacroExits();
        }

        /// <summary>
        /// Checks if a macro of MacroName already exists, and if so, if it can be overridden.
        /// </summary>
        protected virtual void CheckIfMacroExits()
        {
            Logging.Debug("Checking for if macro {0} already exists", MacroName);
            AutomationMacro macro = Macros.Find(mac => mac.Name.Equals(MacroName));
            if (macro != null)
            {
                if (macro.MacroType == Utilities.Enums.MacroType.ApplicationDefined)
                {
                    Logging.Error("Cannot replace value of application defined macro {0}", macro.Name);
                    return;
                }
                else if (macro.MacroType == Utilities.Enums.MacroType.Global)
                {
                    Logging.Warning("Replacing global macro {0}", macro.Name);
                }
                Logging.Debug("Macro found, removing");
                Macros.Remove(macro);
            }
        }
        #endregion
    }
}
