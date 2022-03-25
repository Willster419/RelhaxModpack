using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// A MacroStringInputMacroTask allows for creation of a macro by using an already existing macro and a custom string retrieval implementation.
    /// </summary>
    /// <remarks>The name of the macro to use (that already exists in the list of macros) is specified rather then passing the macro as an input text value.</remarks>
    public abstract class MacroStringInputMacroTask : MacroStringTask, IXmlSerializable
    {
        /// <summary>
        /// The name of the macro in the list of macros to use as input text.
        /// </summary>
        public string InputMacroName { get; set; }

        /// <summary>
        /// The result of the retrieval of the given macro by name.
        /// </summary>
        /// <seealso cref="InputMacroName"/>
        protected string inputMacroText;

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(InputMacroName) }).ToArray();
        }
        #endregion

        #region Task execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            InputMacroName = ProcessMacro(nameof(InputMacroName), InputMacroName);
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute. Additionally assigns the result of finding the macro by InputMacroName to inputMacroText.
        /// </summary>
        public override void ValidateCommands()
        {
            base.ValidateCommands();
            if (ValidateCommandStringNullEmptyTrue(nameof(InputMacroName), InputMacroName))
                return;
            AutomationMacro result = Macros.Find(mac => mac.Name.Equals(InputMacroName));
            if (ValidateCommandTrue(result == null, string.Format("The macro \"{0}\" was not found in the macro list", InputMacroName)))
                return;
            inputMacroText = result.Value;
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            await base.RunTask();
            bool gotString = GetStringReturnValue();
            if (!gotString)
            {
                Logging.Error("String return value gave false exit result");
                return;
            }
            Logging.Info("Creating macro, Name: {0}, Value: {1}", MacroName, stringReturnValue);
            Macros.Add(new AutomationMacro() { MacroType = MacroType.Local, Name = MacroName, Value = stringReturnValue });
        }

        /// <summary>
        /// Do not call GetStringValue() for any task deriving from MacroStringInputMacroTask
        /// </summary>
        protected override Task GetStringValue()
        {
            Logging.Error("Do not call GetStringValue() for any task deriving from MacroStringInputMacroTask (did you mean to implement GetStringReturnValue?)");
            throw new NotImplementedException();
        }

        /// <summary>
        /// Runs a task implementation to parse inputMacroText to get a string result saved to stringReturnValue
        /// </summary>
        /// <returns>True if the implementation was successful, false otherwise.</returns>
        protected abstract bool GetStringReturnValue();

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();
        }
        #endregion
    }
}
