using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    class FileExistsTask : FileTask, IXmlSerializable
    {
        public const string TaskCommandName = "file_exists";

        public override string Command { get { return TaskCommandName; } }

        #region Xml Serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
        }

        public override void ValidateCommands()
        {
            if (ValidateCommandTrue(string.IsNullOrEmpty(SourceFilePath), string.Format("SourceFilePath is empty string")))
                return;
        }

        public override Task RunTask()
        {
            //stub, not needed
            return null;
        }

        public override void ProcessTaskResults()
        {
            //"false" version means that the test being false is "bad"
            if (ProcessTaskResultFalse(File.Exists(SourceFilePath), string.Format("The file {0} does not exist", SourceFilePath)))
                return;
        }
        #endregion
    }
}
