using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class MacroStringSplitFilenameTask : MacroStringSplitTask, IXmlSerializable
    {
        public const string TaskCommandName = "macro_string_split_filename";

        public override string Command { get { return TaskCommandName; } }

        public string FilePath { get; set; }

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(FilePath) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            FilePath = ProcessMacro(nameof(FilePath), FilePath);
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (ValidateCommandStringNullEmptyTrue(nameof(FilePath), FilePath))
                return;

            if (ValidateCommandFalse(File.Exists(FilePath), string.Format("The filepath {0} does not exist", FilePath)))
                return;
        }

        public override async Task RunTask()
        {
            await base.RunTask();
        }

        protected override void GetStringValue()
        {
            stringWithValue = Path.GetFileName(FilePath);
        }

        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();
        }
        #endregion
    }
}
