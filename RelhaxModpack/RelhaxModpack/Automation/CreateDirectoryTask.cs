using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class CreateDirectoryTask : AutomationTask, IXmlSerializable
    {
        public const string TaskCommandName = "create_directory";

        public override string Command { get { return TaskCommandName; } }

        public string DirectoryPath { get; set; }

        #region Xml Serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(DirectoryPath) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            DirectoryPath = ProcessMacro(nameof(DirectoryPath), DirectoryPath);
        }

        public override void ValidateCommands()
        {
            if (ValidateCommandStringNullEmptyTrue(nameof(DirectoryPath), DirectoryPath))
                return;
        }

        public async override Task RunTask()
        {
            Logging.Debug("Creating directory {0}", DirectoryPath);

            if (Directory.Exists(DirectoryPath))
                Logging.Debug("Directory path already exists");
            else
                Directory.CreateDirectory(DirectoryPath);
        }

        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultFalse(Directory.Exists(DirectoryPath), "The directory path does not exist"))
                return;
        }
        #endregion
    }
}
