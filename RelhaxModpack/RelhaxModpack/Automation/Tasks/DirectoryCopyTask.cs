using RelhaxModpack.Common;
using RelhaxModpack.Database;
using RelhaxModpack.UI;
using RelhaxModpack.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class DirectoryCopyTask : DirectorySearchTask, IXmlSerializable, ICancelOperation
    {
        public const string TaskCommandName = "directory_copy";

        public override string Command { get { return TaskCommandName; } }

        public string DestinationPath { get; set; }

        protected bool good = false;

        protected bool reportingProgress { get { return DatabaseAutomationRunner != null; } }

        protected RelhaxProgress relhaxProgress;

        protected Progress<RelhaxProgress> progress;

        protected CancellationTokenSource cancellationTokenSource;

        protected FileCopier fileCopier;

        #region Xml Serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(DestinationPath) }).ToArray();
        }
        #endregion

        #region Task Execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            DestinationPath = ProcessMacro(nameof(DestinationPath), DestinationPath);
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();
            if (ValidateCommandStringNullEmptyTrue(nameof(DestinationPath), DestinationPath))
                return;
        }

        public async override Task RunTask()
        {
            await base.RunTask();
            if (searchResults == null || searchResults.Count() == 0)
                return;

            if (!Directory.Exists(DestinationPath))
            {
                Logging.Debug("DestinationPath {0} does not exist, create", DestinationPath);
                Directory.CreateDirectory(DestinationPath);
            }

            cancellationTokenSource = new CancellationTokenSource();

            if (reportingProgress)
            {
                relhaxProgress = new RelhaxProgress()
                {
                    ChildCurrentProgress = "barChildTextParent",
                    ChildCurrent = 0,
                    ChildTotal = 0,
                    ParrentCurrent = 0,
                    ParrentTotal = 0
                };

                progress = new Progress<RelhaxProgress>();

                progress.ProgressChanged += DatabaseAutomationRunner.RelhaxProgressChanged;
            }

            await Task.Run(async () =>
            {
                try
                {
                    fileCopier = new FileCopier(relhaxProgress)
                    {
                        CancellationToken = this.cancellationTokenSource.Token,
                        Reporter = this.progress
                    };

                    if (reportingProgress)
                        relhaxProgress.ParrentTotal = searchResults.Count();

                    //copy each file over
                    foreach (string sourceFile in searchResults)
                    {
                        if (reportingProgress)
                        {
                            relhaxProgress.ParrentCurrent++;
                            (progress as IProgress<RelhaxProgress>).Report(relhaxProgress);
                        }

                        string destinationFile = sourceFile.Replace(DirectoryPath, DestinationPath);
                        string destinationPath = Path.GetDirectoryName(destinationFile);
                        if (!Directory.Exists(destinationPath))
                            Directory.CreateDirectory(destinationPath);

                        bool result = await fileCopier.CopyFileAsync(sourceFile, destinationFile);
                        if (!result)
                            return;
                    }

                    if (reportingProgress)
                    {
                        relhaxProgress.ParrentTotal = relhaxProgress.ParrentCurrent;
                        (progress as IProgress<RelhaxProgress>).Report(relhaxProgress);
                    }

                    good = true;
                }
                catch (OperationCanceledException)
                {
                    good = false;
                    return;
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex.ToString());
                    good = false;
                    return;
                }
                finally
                {
                    cancellationTokenSource.Dispose();
                    if (reportingProgress)
                    {
                        progress.ProgressChanged -= DatabaseAutomationRunner.RelhaxProgressChanged;
                    }
                }
            });
        }

        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultFalse(good, "The copy process failed"))
                return;
        }

        public virtual void Cancel()
        {
            cancellationTokenSource.Cancel();
        }
        #endregion
    }
}
