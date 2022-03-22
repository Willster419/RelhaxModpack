using RelhaxModpack.Database;
using RelhaxModpack.UI;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class DirectoryCompareTask : DirectorySearchTask, IXmlSerializable, ICancelOperation
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "directory_compare";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        public string DirectoryComparePathA { get; set; }

        public string DirectoryComparePathB { get; set; }

        protected string[] directoryFilesA;

        protected string[] directoryFilesB;

        protected bool operationFinished = false;

        protected FileHashComparer fileHashComparer;

        protected RelhaxProgress relhaxProgress;

        protected Progress<RelhaxProgress> calculationProgress;

        protected RelhaxProgress progress;

        protected CancellationTokenSource cancellationTokenSource;

        #region Xml Serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Except(new string[] { nameof(DirectoryPath) }).Concat(new string[] { nameof(DirectoryComparePathA), nameof(DirectoryComparePathB) }).ToArray();
        }
        #endregion

        #region Task Execution
        public override void ProcessMacros()
        {
            DirectoryPath = DirectoryComparePathA;
            base.ProcessMacros();
            DirectoryComparePathA = ProcessMacro(nameof(DirectoryComparePathA), DirectoryComparePathA);
            DirectoryComparePathB = ProcessMacro(nameof(DirectoryComparePathB), DirectoryComparePathB);
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();
            if (ValidateCommandStringNullEmptyTrue(nameof(DirectoryComparePathA), DirectoryComparePathA))
                return;
            if (ValidateCommandStringNullEmptyTrue(nameof(DirectoryComparePathB), DirectoryComparePathB))
                return;

            if (ValidateCommandFalse(Directory.Exists(DirectoryComparePathA), string.Format("DirectoryComparePathA of {0} file does not exist", DirectoryComparePathA)))
                return;
            if (ValidateCommandFalse(Directory.Exists(DirectoryComparePathB), string.Format("DirectoryComparePathB of {0} file does not exist", DirectoryComparePathB)))
                return;
        }

        public async override Task RunTask()
        {
            //base.RunTask() will run RunSearch()
            await base.RunTask();

            //check that the same number of files was returned to check for hashing
            if (directoryFilesA.Length != directoryFilesB.Length)
            {
                Logging.Error("The number of files between directory a ({0}) and directory b ({1}) do not match", directoryFilesA.Length, directoryFilesB.Length);
                return;
            }

            //check that the relative paths match between the folders
            for (int i = 0; i < directoryFilesA.Length; i++)
            {
                string relativePathA = directoryFilesA[i].Replace(DirectoryComparePathA, string.Empty);
                string relativePathB = directoryFilesB[i].Replace(DirectoryComparePathB, string.Empty);

                if (!relativePathA.Equals(relativePathB))
                {
                    Logging.Error("At index {0}, relative path a ({1}) does not match relative path b ({2})", i, relativePathA, relativePathB);
                    return;
                }
            }

            //actually run the hashing
            operationFinished = await RunFileHashing();
        }

        protected virtual async Task<bool> RunFileHashing()
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
                        new AutomationCompareFile(DirectoryComparePathA, DirectoryComparePathB, AutomationCompareMode.NoMatchContinue, directoryFilesA[i], fileAHash, directoryFilesB[i], fileBHash));
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

        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultFalse(operationFinished, "The operation failed to finish, check the above log messages"))
                return;
        }

        protected override void RunSearch()
        {
            directoryFilesA = FileUtils.FileSearch(DirectoryComparePathA, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly, false, true, SearchPattern);
            directoryFilesB = FileUtils.FileSearch(DirectoryComparePathB, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly, false, true, SearchPattern);
        }

        public virtual void Cancel()
        {
            cancellationTokenSource.Cancel();
        }
        #endregion
    }
}
