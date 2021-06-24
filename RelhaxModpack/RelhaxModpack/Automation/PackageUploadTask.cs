using RelhaxModpack.Utilities;
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

            if (ValidateCommand(string.IsNullOrEmpty(ZipFileName), string.Format("ExitCode {0}: ZipFileName is null or empty", ExitCode)))
                return;

            if (ValidateCommand(!File.Exists(FilePath), string.Format("ExitCode {0}: The filepath {1} does not exist", ExitCode, FilePath)))
                return;
        }

        public override async Task RunTask()
        {
            NetworkCredential networkCredential = new NetworkCredential(AutomationSettings.BigmodsUsername, AutomationSettings.BigmodsPassword);
            string serverPath = string.Format("{0}{1}", PrivateStuff.BigmodsFTPUsersRoot, WoTOnlineFolderVersion);
            string uploadUrl = string.Format("{0}/{1}", serverPath, ZipFileName);
            Logging.Info("Checking if {0} already exists on the server in folder {1}", ZipFileName, WoTOnlineFolderVersion);
            string[] listOfFilesOnServer = await FtpUtils.FtpListFilesFoldersAsync(serverPath, networkCredential);
            int duplicateIncriment = 1;
            string nameWithNoExtension = Path.GetFileNameWithoutExtension(ZipFileName);
            string extension = Path.GetExtension(ZipFileName);
            string newFilename = ZipFileName;
            while (listOfFilesOnServer.Contains(newFilename))
            {
                Logging.Info("Filename already exists on server, giving it a unique name");
                newFilename = string.Format("{0}_{1}.{2}", nameWithNoExtension, duplicateIncriment++.ToString(), extension);
                Logging.Info("Propose {0}", newFilename);
            }
            ZipFileName = newFilename;

            using (WebClient = new WebClient { Credentials = networkCredential })
            {

                Logging.Info(Logfiles.AutomationRunner, "Uploading package");
                Logging.Debug(Logfiles.AutomationRunner, "Upload zip url = {0}, file = {1}", uploadUrl, FilePath);
                //https://stackoverflow.com/questions/2953403/c-sharp-passing-method-as-the-argument-in-a-method
                if (DatabaseAutomationRunner != null)
                {
                    WebClient.UploadProgressChanged += DatabaseAutomationRunner.UploadProgressChanged;
                    WebClient.UploadFileCompleted += DatabaseAutomationRunner.UploadFileCompleted;
                }
                try
                {
                    await WebClient.UploadFileTaskAsync(serverPath, FilePath);
                    DatabasePackage.ZipFile = ZipFileName;
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
                        WebClient.UploadProgressChanged -= DatabaseAutomationRunner.UploadProgressChanged;
                        WebClient.UploadFileCompleted -= DatabaseAutomationRunner.UploadFileCompleted;
                    }
                }
            }
        }
        #endregion
    }
}
