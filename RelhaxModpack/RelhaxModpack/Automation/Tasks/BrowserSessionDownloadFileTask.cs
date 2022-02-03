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
        public const string TaskCommandName = "browser_session_download_file";

        public override string Command { get { return TaskCommandName; } }

        public string DestinationPath { get; set; } = string.Empty;

        protected bool downloaded = false;

        protected CancellationTokenSource cancellationTokenSource;

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(DestinationPath) }).ToArray();
        }
        #endregion

        #region Task execution
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

        public override async Task RunTask()
        {
            CreateLastDownloadFilenameMacro();
            DownloadSetup();
            await DownloadFile();
        }

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
