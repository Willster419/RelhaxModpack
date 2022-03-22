using RelhaxModpack.Database;
using RelhaxModpack.UI;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class DirectoryCompareInverseTask : DirectoryCompareTask, IXmlSerializable, ICancelOperation
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "directory_compare_inverse";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        #region Xml Serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes();
        }
        #endregion

        #region Task Execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            base.ProcessMacros();
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            base.ValidateCommands();
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            await base.RunTask();
        }

        protected override async Task<bool> RunFileHashing()
        {
            calculationProgress = new Progress<RelhaxProgress>();
            cancellationTokenSource = new CancellationTokenSource();
            progress = new RelhaxProgress() { ChildCurrentProgress = "barWithTextChild", ChildCurrent = 0, ChildTotal = directoryFilesA.Length };
            fileHashComparer = new FileHashComparer()
            {
                CancellationTokenA = cancellationTokenSource.Token,
                CancellationTokenB = cancellationTokenSource.Token,
            };

            if (DatabaseAutomationRunner != null)
            {
                calculationProgress.ProgressChanged += DatabaseAutomationRunner.RelhaxProgressChanged;
            }

            try
            {
                for (int i = 0; i < directoryFilesA.Length; i++)
                {
                    await fileHashComparer.ComputeHashA(directoryFilesA[i]);
                    await fileHashComparer.ComputeHashB(directoryFilesB[i]);

                    string fileAHash = fileHashComparer.HashAStringBuilder?.ToString();
                    string fileBHash = fileHashComparer.HashBStringBuilder?.ToString();
                    Logging.Debug("File A hash: {0}", fileAHash);
                    Logging.Debug("File B hash: {0}", fileBHash);

                    AutomationCompareManager.AddCompare(this,
                        new AutomationCompareFile(DirectoryComparePathA, DirectoryComparePathB, AutomationCompareMode.NoMatchStop, directoryFilesA[i], fileAHash, directoryFilesB[i], fileBHash));
                    progress.ChildCurrent++;
                    (calculationProgress as IProgress<RelhaxProgress>).Report(progress);

                    if (cancellationTokenSource.Token.IsCancellationRequested)
                        cancellationTokenSource.Cancel();
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
                return false;
            }
            finally
            {
                cancellationTokenSource.Dispose();
                if (DatabaseAutomationRunner != null)
                {
                    calculationProgress.ProgressChanged -= DatabaseAutomationRunner.RelhaxProgressChanged;
                }
            }
            return true;
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();
        }

        protected override void RunSearch()
        {
            base.RunSearch();
        }

        public override void Cancel()
        {
            base.Cancel();
        }
        #endregion
    }
}
