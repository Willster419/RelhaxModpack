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
    /// A BrowserSessionDownloadFileTask enables downloading a resource from a url to a file destination on disk.
    /// </summary>
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

        /// <summary>
        /// The destination of where to download the file to, including the file name. If the path to the file doesn't exist, then it will be created.
        /// </summary>
        public string DestinationPath { get; set; } = string.Empty;

        /// <summary>
        /// Flag for the download process to determine if the download completely and successfully.
        /// </summary>
        protected bool downloaded = false;

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

        /// <summary>
        /// Prepares the destination path for the download of the resource.
        /// </summary>
        protected void DownloadSetup()
        {
            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Verifying that destination path ({0}) exists and deleting file if exists", DestinationPath);
            string directoryPath = Path.GetDirectoryName(DestinationPath);
            if ((!string.IsNullOrWhiteSpace(directoryPath)) && (!Directory.Exists(directoryPath)))
                Directory.CreateDirectory(directoryPath);
            if (File.Exists(DestinationPath))
                File.Delete(DestinationPath);
        }

        /// <summary>
        /// Downloads the resource to disk using the given web api implementation.
        /// </summary>
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

        /// <summary>
        /// Sends a cancellation request to task's current operation.
        /// </summary>
        public virtual void Cancel()
        {
            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();
        }
        #endregion
    }
}
