using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// Downloads a zip file of the sequence's database package entry to disk.
    /// </summary>
    public class PackageDownloadTask : PackageTransferTask
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "package_download";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (ValidateCommandTrue(string.IsNullOrEmpty(DatabasePackage.ZipFile), string.Format("Package {0} does not have a zip file to download", DatabasePackage.PackageName)))
                return;

            string directoryPath = Path.GetDirectoryName(FilePath);
            //the directory path could be null if the user wants to load/save the database right here and is using a relative path
            if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Logging.Info("The directory path {0} was not found to exist and was created", directoryPath);
            }
        }

        #region Task execution
        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            using (WebClient = new WebClient { Credentials = new NetworkCredential(AutomationSettings.BigmodsUsername, AutomationSettings.BigmodsPassword) })
            {
                string serverPath = string.Format("{0}{1}/{2}", PrivateStuff.BigmodsFTPUsersRoot, WoTOnlineFolderVersion, DatabasePackage.ZipFile);

                Logging.Info(Logfiles.AutomationRunner, "Downloading package");
                Logging.Debug(Logfiles.AutomationRunner, "Download zip url = {0}, file = {1}", serverPath, FilePath);
                //https://stackoverflow.com/questions/2953403/c-sharp-passing-method-as-the-argument-in-a-method
                if (DatabaseAutomationRunner != null)
                {
                    WebClient.DownloadProgressChanged += DatabaseAutomationRunner.DownloadProgressChanged;
                    WebClient.DownloadFileCompleted += DatabaseAutomationRunner.DownloadFileCompleted;
                }
                try
                {
                    if (DatabaseAutomationRunner != null)
                    {
                        if (DatabaseAutomationRunner.DownloadProgressChanged != null)
                        {
                            long filesize = await FtpUtils.FtpGetFilesizeAsync(serverPath, WebClient.Credentials);
                            DatabaseAutomationRunner.ProgressChanged.Invoke(null, new ProgressChangedEventArgs(69, filesize));
                        }
                    }
                    await WebClient.DownloadFileTaskAsync(serverPath, FilePath);
                    TransferSuccess = true;
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
                        WebClient.DownloadProgressChanged -= DatabaseAutomationRunner.DownloadProgressChanged;
                        WebClient.DownloadFileCompleted -= DatabaseAutomationRunner.DownloadFileCompleted;
                    }
                }
            }
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();

            if (ProcessTaskResultTrue(!File.Exists(FilePath), string.Format("The download FilePath {0} does not exist", FilePath)))
                return;
        }
        #endregion
    }
}
