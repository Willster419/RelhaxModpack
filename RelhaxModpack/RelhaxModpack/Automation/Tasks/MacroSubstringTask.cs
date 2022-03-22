using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public abstract class MacroSubstringTask : MacroStringTask, IXmlSerializable
    {
        public string StartIndex { get; set; }

        public string Length { get; set; } = "-1";

        protected int startIndex;

        protected int length = -1;

        protected string stringWithValue { get; set; } = string.Empty;

        protected bool taskCompleted = false;

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(StartIndex), nameof(Length) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();

            //first process macros on the strings
            StartIndex = ProcessMacro(nameof(StartIndex), StartIndex);
            Length = ProcessMacro(nameof(Length), Length);

            //then process the internal data types
            if (int.TryParse(Length, out int result))
                length = result;
            if (int.TryParse(StartIndex, out int result_))
                startIndex = result_;
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (ValidateCommandStringNullEmptyTrue(nameof(StartIndex), StartIndex))
                return;
        }

        public override async Task RunTask()
        {
            await base.RunTask();
            await GetStringValue();

            if (string.IsNullOrEmpty(stringWithValue))
            {
                Logging.Debug("String value for processing is empty");
                return;
            }

            if (length > 0)
            {
                if (startIndex + length > stringWithValue.Length)
                {
                    Logging.Error("The requested length of the substring {0} is greater then the length of the original string {1}", startIndex + length, stringWithValue.Length);
                    return;
                }

                try
                {
                    stringReturnValue = stringWithValue.Substring(startIndex, length);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Logging.Error(ex.Message);
                    return;
                }
            }
            else
            {
                if (startIndex >= stringWithValue.Length)
                {
                    Logging.Error("The requested start index {0} is greater then or equal to the length of the original string {1}", startIndex, stringWithValue.Length);
                    return;
                }

                try
                {
                    stringReturnValue = stringWithValue.Substring(startIndex);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Logging.Error(ex.Message);
                    return;
                }
            }

            Logging.Info("Creating macro, Name: {0}, Value: {1}", MacroName, stringReturnValue);
            Macros.Add(new AutomationMacro() { MacroType = MacroType.Local, Name = MacroName, Value = stringReturnValue });
            taskCompleted = true;
        }

        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();

            if (ProcessTaskResultFalse(taskCompleted, "The task failed to complete, check above error messages"))
                return;
        }
        #endregion
    }
}
