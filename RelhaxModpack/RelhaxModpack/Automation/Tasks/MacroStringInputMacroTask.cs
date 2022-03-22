using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public abstract class MacroStringInputMacroTask : MacroStringTask, IXmlSerializable
    {
        public string InputMacroName { get; set; }

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
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            InputMacroName = ProcessMacro(nameof(InputMacroName), InputMacroName);
        }

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

        public override async Task RunTask()
        {
            await base.RunTask();
            bool gotString = await GetStringReturnValue();
            if (!gotString)
            {
                Logging.Error("String return value gave false exit result");
                return;
            }
            Logging.Info("Creating macro, Name: {0}, Value: {1}", MacroName, stringReturnValue);
            Macros.Add(new AutomationMacro() { MacroType = MacroType.Local, Name = MacroName, Value = stringReturnValue });
        }

        protected override Task GetStringValue()
        {
            Logging.Error("Do not call GetStringValue() for any task deriving from MacroStringInputMacroTask (did you mean to implement GetStringReturnValue?)");
            throw new NotImplementedException();
        }

        //assign stringReturnValue in here
        protected abstract Task<bool> GetStringReturnValue();

        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();
        }
        #endregion
    }
}
