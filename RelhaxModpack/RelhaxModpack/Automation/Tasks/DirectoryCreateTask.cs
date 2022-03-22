using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class DirectoryCreateTask : DirectoryTask, IXmlSerializable
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "create_directory";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        #region Task execution
        public override void ValidateCommands()
        {
            //don't run the base because it will check to make sure the directory exists, which we know it doesn't
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
