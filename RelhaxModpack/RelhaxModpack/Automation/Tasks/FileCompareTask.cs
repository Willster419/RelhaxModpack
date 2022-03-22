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
    public class FileCompareTask : AutomationTask, IXmlSerializable, ICancelOperation
    {
        public const string TaskCommandName = "file_compare";

        public override string Command { get {return TaskCommandName;} }

        public string FileA { get; set; }

        public string FileB { get; set; }

        protected FileHashComparer fileHashComparer;

        protected string fileAHash, fileBHash = string.Empty;

        protected Progress<RelhaxProgress> calculationProgress;

        protected CancellationTokenSource cancellationTokenSource;

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(FileA), nameof(FileB) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            FileA = ProcessMacro(nameof(FileA), FileA);
            FileB = ProcessMacro(nameof(FileB), FileB);
        }

        public override void ValidateCommands()
        {
            if (ValidateCommandTrue(string.IsNullOrEmpty(FileA), "The arg FileA is empty string"))
                return;
            if (ValidateCommandTrue(string.IsNullOrEmpty(FileB), "The arg FileB is empty string"))
                return;

            if (ValidateCommandTrue(!File.Exists(FileA), string.Format("The path for FileA, {0}, does not exist", FileA)))
                return;
            if (ValidateCommandTrue(!File.Exists(FileB), string.Format("The path for FileB, {0}, does not exist", FileB)))
                return;
        }

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

        public virtual void Cancel()
        {
            cancellationTokenSource.Cancel();
        }
        #endregion
    }
}
