using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public abstract class PackageTransferTask : AutomationTask, IXmlSerializable
    {
        public string FilePath { get; set; } = string.Empty;

        public DatabasePackage DatabasePackage { get { return AutomationSequence.Package; } }

        public string WoTOnlineFolderVersion { get { return AutomationSequence.DatabaseManager.WoTOnlineFolderVersion; } }

        protected WebClient WebClient;

        protected NetworkCredential Credential;

        protected bool TransferSuccess = false;

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(FilePath) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            FilePath = ProcessMacro(nameof(FilePath), FilePath);
        }

        public override void ValidateCommands()
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                ExitCode = 1;
                ErrorMessage = string.Format("ExitCode {0}: FilePath is null/empty", ExitCode);
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
                return;
            }

            if (DatabasePackage == null)
            {
                ExitCode = 1;
                ErrorMessage = string.Format("ExitCode {0}: DatabasePackage is null (This is an internal application error)", ExitCode);
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
                return;
            }

            if (string.IsNullOrEmpty(AutomationSettings.BigmodsUsername))
            {
                ExitCode = 1;
                ErrorMessage = string.Format("ExitCode {0}: AutomationSettings.BigmodsUsername is null/empty", ExitCode);
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
                return;
            }

            if (string.IsNullOrEmpty(AutomationSettings.BigmodsPassword))
            {
                ExitCode = 1;
                ErrorMessage = string.Format("ExitCode {0}: AutomationSettings.BigmodsPassword is null/empty", ExitCode);
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
                return;
            }
        }

        public override void ProcessTaskResults()
        {
            if (!TransferSuccess)
            {
                ExitCode = 4;
                ErrorMessage = string.Format("{0} {1}: The transfer reported a failure. Check the log for more information", nameof(ExitCode), ExitCode);
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
            }
        }
        #endregion
    }
}
