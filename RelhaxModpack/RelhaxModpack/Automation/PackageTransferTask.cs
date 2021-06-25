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
            if (ValidateCommandTrue(string.IsNullOrEmpty(FilePath), string.Format("ExitCode {0}: FilePath is null/empty", ExitCode)))
                return;

            if (ValidateCommandTrue(DatabasePackage == null, string.Format("ExitCode {0}: DatabasePackage is null (This is an internal application error)", ExitCode)))
                return;

            if (ValidateCommandTrue(string.IsNullOrEmpty(AutomationSettings.BigmodsUsername), string.Format("ExitCode {0}: AutomationSettings.BigmodsUsername is null/empty", ExitCode)))
                return;

            if (ValidateCommandTrue(string.IsNullOrEmpty(AutomationSettings.BigmodsPassword), string.Format("ExitCode {0}: AutomationSettings.BigmodsPassword is null/empty", ExitCode)))
                return;
        }

        public override void ProcessTaskResults()
        {
            if (!ProcessTaskResultTrue(TransferSuccess, "The transfer reported a failure. Check the log for more information"))
                return;
        }
        #endregion
    }
}
