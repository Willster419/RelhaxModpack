using RelhaxModpack.Common;
using RelhaxModpack.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// A FileCopyTask allows for the copy of a single file from source to destination.
    /// </summary>
    public class FileCopyTask : FileDestinationTask, ICancelOperation
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "file_copy";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        /// <summary>
        /// Flag to indicate if the copy operation was successful.
        /// </summary>
        protected bool fileCopyResult;

        /// <summary>
        /// The implementation to copy the source file to the destination path, keeping metadata information.
        /// </summary>
        protected FileCopier fileCopier;

        /// <summary>
        /// The implementation to provide progress updates to subscribed listeners.
        /// </summary>
        protected Progress<RelhaxProgress> copyProgress;

        /// <summary>
        /// The cancellation token.
        /// </summary>
        protected CancellationTokenSource cancellationTokenSource;

        #region Task execution
        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
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

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            if (!ProcessTaskResultFalse(fileCopyResult, "The file copy operation failed"))
                return;
        }

        /// <summary>
        /// Sends a cancellation request to task's current operation.
        /// </summary>
        public virtual void Cancel()
        {
            cancellationTokenSource.Cancel();
        }
        #endregion
    }
}
