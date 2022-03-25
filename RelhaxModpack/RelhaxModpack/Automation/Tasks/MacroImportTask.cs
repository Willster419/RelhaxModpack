using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// Import of a list of macros from another automation sequence xml file or a macro definitions xml file.
    /// </summary>
    public class MacroImportTask : ImportTask
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "import_macro";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        /// <summary>
        /// The Xpath search pattern to use for getting a list of xml task objects.
        /// </summary>
        protected override string XpathExpression { get { return AutomationSequence.AutomationSequenceMacroDefinitionsXpath; } }

        /// <summary>
        /// Initializes objectList to hold AutomationMacro objects.
        /// </summary>
        protected override void CreateList()
        {
            objectList = new List<AutomationMacro>();
        }

        /// <summary>
        /// Perform and post-processing on the created macros and add them to the list of macros of this sequence.
        /// </summary>
        protected override void ImportList()
        {
            Logging.Debug("Setting each parsed macro as local, overriding if other local macro exists");

            List<AutomationMacro> macroList = objectList as List<AutomationMacro>;

            foreach (AutomationMacro macro in macroList)
            {
                macro.MacroType = MacroType.Local;

                //check if macro already exists
                AutomationMacro result = Macros.Find(mac => mac.Name.ToLower().Equals(macro.Name.ToLower()));
                if (result != null)
                {
                    if (result.MacroType != MacroType.Local)
                    {
                        Logging.Error("The parsed macro {{{0}}} already exists in the current macro list as macro type {1}, and cannot be overridden.", result.Name, result.MacroType.ToString());
                        continue;
                    }
                    else
                    {
                        Logging.Info("The parsed macro {{{0}}} already exists in the current macro list as macro type {1} with value {2}, and will be over-ridden", result.Name, result.MacroType.ToString(), result.Value);
                        result.Value = macro.Value;
                    }
                }
                else
                {
                    Logging.Info("Adding macro {{{0}}} with value '{1}'", macro.Name, macro.Value);
                    Macros.Add(macro);
                }
            }
        }
    }
}
