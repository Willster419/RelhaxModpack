using RelhaxModpack.Common;
using RelhaxModpack.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class FileCopyTask : FileDestinationTask, ICancelOperation
    {
        public const string TaskCommandName = "file_copy";

        public override string Command { get { return TaskCommandName; } }

        protected bool fileCopyResult;

        protected FileCopier fileCopier;

        protected Progress<RelhaxProgress> copyProgress;

        protected CancellationTokenSource cancellationTokenSource;

        #region Task execution
        public override async Task RunTask()
        {
            base.RunTask();

            if (!destinationDeleteResult)
            {
                fileCopyResult = false;
                return;
            }

            Logging.Info("Copying file from location {0} to location {1}", SourceFilePath, DestinationFilePath);
            copyProgress = new Progress<RelhaxProgress>();
            cancellationTokenSource = new CancellationTokenSource();
            fileCopier = new FileCopier(SourceFilePath, DestinationFilePath) { CancellationToken = cancellationTokenSource.Token, Reporter = copyProgress };

            if (DatabaseAutomationRunner != null)
            {
                copyProgress.ProgressChanged += DatabaseAutomationRunner.RelhaxProgressChanged;
            }

            try
            {
                fileCopyResult = await fileCopier.CopyFileAsync();
            }
            catch (OperationCanceledException)
            {
                Logging.Info("The copy operation was canceled");
                fileCopyResult = false;
            }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
                fileCopyResult = false;
            }
            finally
            {
                cancellationTokenSource.Dispose();
                if (DatabaseAutomationRunner != null)
                {
                    copyProgress.ProgressChanged -= DatabaseAutomationRunner.RelhaxProgressChanged;
                }
            }
        }

        public override void ProcessTaskResults()
        {
            if (!ProcessTaskResultFalse(fileCopyResult, "The file copy operation failed"))
                return;
        }

        public virtual void Cancel()
        {
            cancellationTokenSource.Cancel();
        }
        #endregion
    }
}
