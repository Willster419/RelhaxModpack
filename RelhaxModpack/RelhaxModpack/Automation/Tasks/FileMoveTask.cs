using RelhaxModpack.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// A FileMoveTask will allow a file to be moved within a single volume by modification of the file table rather then a copy.
    /// </summary>
    public class FileMoveTask : FileDestinationTask
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "file_move";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        /// <summary>
        /// Flag to indicate if the file move was successful.
        /// </summary>
        protected bool fileMoveResult;

        #region Task execution
        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task RunTask()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            base.RunTask();

            if (!destinationDeleteResult)
            {
                fileMoveResult = false;
                return;
            }

            Logging.Info("Moving file from location {0} to location {1}", SourceFilePath, DestinationFilePath);
            fileMoveResult = FileUtils.FileMove(SourceFilePath, DestinationFilePath);
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            if (!ProcessTaskResultFalse(fileMoveResult, "The file move operation failed"))
                return;
        }
        #endregion
    }
}
