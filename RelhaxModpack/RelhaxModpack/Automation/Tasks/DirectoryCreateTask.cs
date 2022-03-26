using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// Creates a directory if it does not already exist.
    /// </summary>
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
        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            //don't run the base because it will check to make sure the directory exists, which we know it doesn't
            if (ValidateCommandStringNullEmptyTrue(nameof(DirectoryPath), DirectoryPath))
                return;
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task RunTask()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Logging.Debug("Creating directory {0}", DirectoryPath);

            if (Directory.Exists(DirectoryPath))
                Logging.Debug("Directory path already exists");
            else
                Directory.CreateDirectory(DirectoryPath);
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultFalse(Directory.Exists(DirectoryPath), "The directory path does not exist"))
                return;
        }
        #endregion
    }
}
