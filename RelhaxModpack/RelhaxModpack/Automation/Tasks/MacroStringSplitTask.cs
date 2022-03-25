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
    /// Allows for processing of text using the Split method, to be used for a macro.
    /// </summary>
    public abstract class MacroStringSplitTask : MacroStringTask, IXmlSerializable
    {
        /// <summary>
        /// The characters to use as the delimiter for the Split function.
        /// </summary>
        public string SplitCharacters { get; set; }

        /// <summary>
        /// The array index value to select from the result array of the Split function.
        /// </summary>
        public string Index { get; set; }

        /// <summary>
        /// Parsed result of the argument Index.
        /// </summary>
        /// <seealso cref="Index"/>
        protected int index { get; set; }

        /// <summary>
        /// The resultant string array from the Split operation.
        /// </summary>
        protected string[] stringSplit { get; set; }

        /// <summary>
        /// The string to perform the Split operation on.
        /// </summary>
        protected string stringWithValue { get; set; } = string.Empty;

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(SplitCharacters), nameof(Index) }).ToArray();
        }
        #endregion

        #region Task execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            base.ProcessMacros();

            //first process macros on the strings
            SplitCharacters = ProcessMacro(nameof(SplitCharacters), SplitCharacters);
            Index = ProcessMacro(nameof(Index), Index);

            //then process the internal data types
            if (int.TryParse(Index, out int result))
                index = result;
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (ValidateCommandStringNullEmptyTrue(nameof(SplitCharacters), SplitCharacters))
                return;

            if (ValidateCommandStringNullEmptyTrue(nameof(Index), Index))
                return;

            if (ValidateCommandFalse(index > -1, "Index must be 0 or greater"))
                return;
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            await base.RunTask();
            await GetStringValue();

            if (string.IsNullOrEmpty(stringWithValue))
            {
                Logging.Debug("String value for processing is empty");
                return;
            }

            //split it and if debug mode, output the string split values
            stringSplit = stringWithValue.Split(new string[] { SplitCharacters }, StringSplitOptions.RemoveEmptyEntries);
            Logging.Debug("String split count: {0}", stringSplit.Length);
            for (int i = 0; i < stringSplit.Length; i++)
            {
                Logging.Debug("Index {0}: {1}", i, stringSplit[i]);
            }

            if (index >= stringSplit.Count())
            {
                Logging.Error("The index value {0} is outside the range of the array count. The macro will not be created");
                return;
            }

            stringReturnValue = stringSplit[index];
            Logging.Info("Creating macro, Name: {0}, Value: {1}", MacroName, stringReturnValue);
            Macros.Add(new AutomationMacro() { MacroType = MacroType.Local, Name = MacroName, Value = stringReturnValue });
        }

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
