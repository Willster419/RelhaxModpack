using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// Uploads a zip file to the sequence's database package entry to the loaded database's online FTP folder.
    /// </summary>
    public class PackageUploadTask : PackageTransferTask
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "package_upload";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        /// <summary>
        /// The name of the zip file to store in the database package entry.
        /// </summary>
        public string ZipFileName { get; set; } = string.Empty;

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(ZipFileName) }).ToArray();
        }
        #endregion

        #region Task Execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            ZipFileName = ProcessMacro(nameof(ZipFileName), ZipFileName);
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (ValidateCommandTrue(string.IsNullOrEmpty(ZipFileName), string.Format("ZipFileName is null or empty")))
                return;

            if (ValidateCommandTrue(!File.Exists(FilePath), string.Format("The filepath {0} does not exist", FilePath)))
                return;
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            NetworkCredential networkCredential = new NetworkCredential(AutomationSettings.BigmodsUsername, AutomationSettings.BigmodsPassword);
            string serverPath = string.Format("{0}{1}", PrivateStuff.BigmodsFTPUsersRoot, WoTOnlineFolderVersion);
            Logging.Info("Checking if {0} already exists on the server in folder {1}", ZipFileName, WoTOnlineFolderVersion);
            string[] listOfFilesOnServer = await FtpUtils.FtpListFilesFoldersAsync(serverPath, networkCredential);
            int duplicateIncriment = 1;
            string nameWithNoExtension = Path.GetFileNameWithoutExtension(ZipFileName);
            string extension = Path.GetExtension(ZipFileName);
            string newFilename = ZipFileName;
            while (listOfFilesOnServer.Contains(newFilename))
            {
                Logging.Info("Filename already exists on server, giving it a unique name");
                newFilename = string.Format("{0}_{1}{2}", nameWithNoExtension, duplicateIncriment++.ToString(), extension);
                Logging.Info("Propose {0}", newFilename);
            }
            ZipFileName = newFilename;

            using (WebClient = new WebClient { Credentials = networkCredential })
            {
                string uploadUrl = string.Format("{0}/{1}", serverPath, ZipFileName);
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
                    await WebClient.UploadFileTaskAsync(uploadUrl, FilePath);
                    DatabasePackage.UpdateZipfile(ZipFileName);
                    FtpUtils.TriggerMirrorSyncAsync();
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
                        WebClient.UploadProgressChanged -= DatabaseAutomationRunner.UploadProgressChanged;
                        WebClient.UploadFileCompleted -= DatabaseAutomationRunner.UploadFileCompleted;
                    }
                }
            }
        }
        #endregion
    }
}
