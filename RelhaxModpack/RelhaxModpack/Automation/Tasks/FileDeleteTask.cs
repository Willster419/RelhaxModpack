using RelhaxModpack.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class FileDeleteTask : FileSourceTask
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "file_delete";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        protected bool fileDeleteResult;

        #region Task execution
        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            Logging.Info("Deleting file at location {0}", SourceFilePath);

            fileDeleteResult = FileUtils.FileDelete(SourceFilePath);
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            if (!ProcessTaskResultFalse(fileDeleteResult, "The file delete operation failed"))
                return;
        }
        #endregion
    }
}
