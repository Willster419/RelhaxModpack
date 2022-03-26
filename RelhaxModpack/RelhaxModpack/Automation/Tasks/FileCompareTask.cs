using RelhaxModpack.Database;
using RelhaxModpack.UI;
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
    /// <summary>
    /// A FileCompareTask allows for an MD5 comparison of two files to determine their contents are equal.
    /// </summary>
    /// <remarks>If the files are found to not be equal, the comparison is reported as type NoMatchContinue. Great for wotmod or resource files that may need an update for a new client patch.</remarks>
    /// <seealso cref="AutomationCompareMode"/>
    public class FileCompareTask : AutomationTask, IXmlSerializable, ICancelOperation
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "file_compare";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        /// <summary>
        /// The path to file A for comparison.
        /// </summary>
        public string FileA { get; set; }

        /// <summary>
        /// The path to file B for comparison.
        /// </summary>
        public string FileB { get; set; }

        /// <summary>
        /// The implementation to compare the two files.
        /// </summary>
        protected FileHashComparer fileHashComparer;

        /// <summary>
        /// The calculated MD5 hash values of each file.
        /// </summary>
        protected string fileAHash, fileBHash = string.Empty;

        /// <summary>
        /// The implementation to provide progress updates to subscribed listeners.
        /// </summary>
        protected Progress<RelhaxProgress> calculationProgress;

        /// <summary>
        /// The cancellation token.
        /// </summary>
        protected CancellationTokenSource cancellationTokenSource;

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(FileA), nameof(FileB) }).ToArray();
        }
        #endregion

        #region Task execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            FileA = ProcessMacro(nameof(FileA), FileA);
            FileB = ProcessMacro(nameof(FileB), FileB);
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            if (ValidateCommandTrue(string.IsNullOrEmpty(FileA), "The argument FileA is empty string"))
                return;
            if (ValidateCommandTrue(string.IsNullOrEmpty(FileB), "The argument FileB is empty string"))
                return;

            if (ValidateCommandTrue(!File.Exists(FileA), string.Format("The path for FileA, {0}, does not exist", FileA)))
                return;
            if (ValidateCommandTrue(!File.Exists(FileB), string.Format("The path for FileB, {0}, does not exist", FileB)))
                return;
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            calculationProgress = new Progress<RelhaxProgress>();
            cancellationTokenSource = new CancellationTokenSource();
            fileHashComparer = new FileHashComparer()
            {
                CancellationTokenA = cancellationTokenSource.Token,
                CancellationTokenB = cancellationTokenSource.Token,
                ProgressA = calculationProgress,
                ProgressB = calculationProgress
            };

            if (DatabaseAutomationRunner != null)
            {
                calculationProgress.ProgressChanged += DatabaseAutomationRunner.RelhaxProgressChanged;
            }

            try
            {
                await fileHashComparer.ComputeHashA(FileA);
                await fileHashComparer.ComputeHashB(FileB);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
            }
            finally
            {
                cancellationTokenSource.Dispose();
                if (DatabaseAutomationRunner != null)
                {
                    calculationProgress.ProgressChanged -= DatabaseAutomationRunner.RelhaxProgressChanged;
                }
            }

            fileAHash = fileHashComparer.HashAStringBuilder?.ToString();
            fileBHash = fileHashComparer.HashBStringBuilder?.ToString();
            Logging.Debug("File A hash: {0}", fileAHash);
            Logging.Debug("File B hash: {0}", fileBHash);
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultTrue(!fileHashComparer.HashACalculated, "Hash A failed to calculate"))
                return;
            if (ProcessTaskResultTrue(!fileHashComparer.HashBCalculated, "Hash B failed to calculate"))
                return;

            //getting to here means that successful hashes were calculated
            AutomationCompareFile compareFile = new AutomationCompareFile(Path.GetDirectoryName(FileA), Path.GetDirectoryName(FileB), AutomationCompareMode.NoMatchContinue, Path.GetFileName(FileA), fileAHash, Path.GetFileName(FileB), fileBHash);
            AutomationCompareManager.AddCompare(this, compareFile);
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
