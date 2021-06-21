using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class PackageUploadTask : PackageTransferTask
    {
        public const string TaskCommandName = "package_upload";

        public override string Command { get { return TaskCommandName; } }

        public string ZipFileName { get; set; } = string.Empty;

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(ZipFileName) }).ToArray();
        }
        #endregion

        #region Task Execution
        public override void ProcessMacros()
        {
            ZipFileName = ProcessMacro(nameof(ZipFileName), ZipFileName);
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (string.IsNullOrEmpty(ZipFileName))
            {
                ExitCode = 1;
                ErrorMessage = string.Format("ExitCode {0}: ZipFileName is null or empty", ExitCode);
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
                return;
            }

            if (!File.Exists(FilePath))
            {
                ExitCode = 1;
                ErrorMessage = string.Format("ExitCode {0}: The filepath {1} does not exist", ExitCode, FilePath);
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
                return;
            }
        }

        public override async Task RunTask()
        {
            using (WebClient = new WebClient { Credentials = new NetworkCredential(AutomationSettings.BigmodsUsername, AutomationSettings.BigmodsPassword) })
            {
                string serverPath = string.Format("{0}{1}/{2}", PrivateStuff.BigmodsFTPUsersRoot, WoTOnlineFolderVersion, ZipFileName);

                Logging.Info(Logfiles.AutomationRunner, "Uploading package");
                Logging.Debug(Logfiles.AutomationRunner, "Upload zip url = {0}, file = {1}", serverPath, FilePath);
                //https://stackoverflow.com/questions/2953403/c-sharp-passing-method-as-the-argument-in-a-method
                if (DatabaseAutomationRunner != null)
                {
                    WebClient.DownloadProgressChanged += DatabaseAutomationRunner.DownloadProgressChanged;
                    WebClient.DownloadDataCompleted += DatabaseAutomationRunner.DownloadDataCompleted;
                }
                try
                {
                    await WebClient.UploadFileTaskAsync(serverPath, FilePath);
                    TransferSuccess = true;
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex.ToString());
                }
                finally
                {
                    if (DatabaseAutomationRunner != null)
                    {
                        WebClient.DownloadProgressChanged -= DatabaseAutomationRunner.DownloadProgressChanged;
                        WebClient.DownloadDataCompleted -= DatabaseAutomationRunner.DownloadDataCompleted;
                    }
                }
            }
        }
        #endregion
    }
}
