using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public abstract class MacroSubstringTask : MacroStringTask, IXmlSerializable
    {
        public string StartIndex { get; set; }

        public string Length { get; set; } = "-1";

        protected int length { get; set; } = -1;

        protected int startIndex { get; set; }

        protected string stringWithValue { get; set; } = string.Empty;

        #region Xml serialization
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
            GetStringValue();

            if (string.IsNullOrEmpty(stringWithValue))
            {
                Logging.Debug("String value for processing is empty");
                return;
            }

            //split it and if debug mode, output the string split values
            if (length > 0)
            {
                if (startIndex + length > stringWithValue.Length)
                {
                    Logging.Error("The requested length of the substring {0} is greater then the length of the original string {1}", startIndex + length, stringWithValue.Length);
                    return;
                }
                stringReturnValue = stringWithValue.Substring(startIndex, length);
            }
            else
            {
                if (startIndex >= stringWithValue.Length)
                {
                    Logging.Error("The requested start index {0} is greater then or equal to the length of the original string {1}", startIndex, stringWithValue.Length);
                    return;
                }
                stringReturnValue = stringReturnValue.Substring(startIndex);
            }

            Logging.Info("Creating macro, Name: {0}, Value: {1}", MacroName, stringReturnValue);
            Macros.Add(new AutomationMacro() { MacroType = MacroType.Local, Name = MacroName, Value = stringReturnValue });
        }

        protected abstract void GetStringValue();

        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();
        }
        #endregion
    }
}
