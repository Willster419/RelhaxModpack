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
            if (ValidateCommandTrue(string.IsNullOrEmpty(FilePath), string.Format("FilePath is null/empty")))
                return;

            if (ValidateCommandTrue(DatabasePackage == null, string.Format("DatabasePackage is null (This is an internal application error)")))
                return;

            if (ValidateCommandTrue(string.IsNullOrEmpty(AutomationSettings.BigmodsUsername), string.Format("AutomationSettings.BigmodsUsername is null/empty")))
                return;

            if (ValidateCommandTrue(string.IsNullOrEmpty(AutomationSettings.BigmodsPassword), string.Format("AutomationSettings.BigmodsPassword is null/empty")))
                return;
        }

        public override void ProcessTaskResults()
        {
            if (!ProcessTaskResultFalse(TransferSuccess, "The transfer reported a failure. Check the log for more information"))
                return;
        }
        #endregion
    }
}
