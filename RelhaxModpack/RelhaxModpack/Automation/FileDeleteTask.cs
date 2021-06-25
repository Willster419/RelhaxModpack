using RelhaxModpack.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class FileDeleteTask : FileTask
    {
        public const string TaskCommandName = "file_delete";

        public override string Command { get { return TaskCommandName; } }

        protected bool fileDeleteResult;

        #region Task execution
        public override Task RunTask()
        {
            Logging.Info("Deleting file at location {0}", SourceFilePath);

            fileDeleteResult = FileUtils.FileDelete(SourceFilePath);

            return null;
        }

        public override void ProcessTaskResults()
        {
            if (!ProcessTaskResultFalse(fileDeleteResult, "The file delete operation failed"))
                return;
        }
        #endregion
    }
}
