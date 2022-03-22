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
    public class BrowserSessionDownloadFileTask : BrowserSessionUrlTask, ICancelOperation
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "browser_session_download_file";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        public string DestinationPath { get; set; } = string.Empty;

        protected bool downloaded = false;

        protected CancellationTokenSource cancellationTokenSource;

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(DestinationPath) }).ToArray();
        }
        #endregion

        #region Task execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            DestinationPath = ProcessMacro(nameof(DestinationPath), DestinationPath);
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            base.ValidateCommands();
            if (ValidateCommandStringNullEmptyTrue(nameof(DestinationPath), DestinationPath))
                return;
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            CreateLastDownloadFilenameMacro();
            DownloadSetup();
            await DownloadFile();
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultFalse(downloaded, "The download failed"))
                return;
            if (ProcessTaskResultFalse(File.Exists(DestinationPath), string.Format("The file {0} does not exist after task completes", DestinationPath)))
                return;
        }

        protected void DownloadSetup()
        {
            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Verifying that destination path ({0}) exists and deleting file if exists", DestinationPath);
            string directoryPath = Path.GetDirectoryName(DestinationPath);
            if ((!string.IsNullOrWhiteSpace(directoryPath)) && (!Directory.Exists(directoryPath)))
                Directory.CreateDirectory(directoryPath);
            if (File.Exists(DestinationPath))
                File.Delete(DestinationPath);
        }

        protected virtual async Task DownloadFile()
        {
            cancellationTokenSource = new CancellationTokenSource();
            if (DatabaseAutomationRunner != null)
            {
                BrowserSessionManager.DownloadProgress += BrowserSessionManager_DownloadProgress;
            }

            try
            {
                await BrowserSessionManager.DownloadFileAsync(Url, DestinationPath, cancellationTokenSource.Token);
                downloaded = true;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
            }
            finally
            {
                if (DatabaseAutomationRunner != null)
                {
                    BrowserSessionManager.DownloadProgress -= BrowserSessionManager_DownloadProgress;
                }
            }
        }

        private void BrowserSessionManager_DownloadProgress(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            DatabaseAutomationRunner.DownloadProgressChanged?.Invoke(this, e);
        }

        public virtual void Cancel()
        {
            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();
        }
        #endregion
    }
}
