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

        public override string Command { get { return TaskCommandName; } }

        protected bool fileDeleteResult;

        #region Task execution
        public override async Task RunTask()
        {
            Logging.Info("Deleting file at location {0}", SourceFilePath);

            fileDeleteResult = FileUtils.FileDelete(SourceFilePath);
        }

        public override void ProcessTaskResults()
        {
            if (!ProcessTaskResultFalse(fileDeleteResult, "The file delete operation failed"))
                return;
        }
        #endregion
    }
}
