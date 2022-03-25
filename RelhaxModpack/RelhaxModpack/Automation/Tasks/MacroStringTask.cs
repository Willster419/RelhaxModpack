using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// Provides skeleton framework for getting a string value to use for saving to a macro.
    /// </summary>
    public abstract class MacroStringTask : MacroTask
    {
        /// <summary>
        /// The value to save the result of from GetStringValue.
        /// </summary>
        /// <seealso cref="GetStringValue"/>
        protected string stringReturnValue { get; set; } = string.Empty;

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            AutomationMacro macro = Macros.Find(mac => mac.Name.Equals(MacroName));
            if (ProcessTaskResultTrue(macro == null, "Could not find newly created macro in list"))
                return;
        }

        /// <summary>
        /// Gets the string to use for the macro's value and saves it to stringReturnValue.
        /// </summary>
        /// <seealso cref="stringReturnValue"/>
        protected abstract Task GetStringValue();

        /// <summary>
        /// Process escape characters that may exist from the mechanism that provided the resultant string.
        /// </summary>
        /// <remarks>For example, processing HTML escape characters from a downloaded HTML page.</remarks>
        protected virtual void ProcessEscapeCharacters()
        {
            //this method intentionally left blank
        }
    }
}
