using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public abstract class PackageTransferTask : DatabasePackageTask, IXmlSerializable, ICancelOperation
    {
        public string FilePath { get; set; } = string.Empty;

        public string WoTOnlineFolderVersion { get { return AutomationSequence.DatabaseManager.WoTOnlineFolderVersion; } }

        protected WebClient WebClient;

        protected bool TransferSuccess = false;

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(FilePath) }).ToArray();
        }
        #endregion

        #region Task execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            FilePath = ProcessMacro(nameof(FilePath), FilePath);
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (ValidateCommandTrue(string.IsNullOrEmpty(FilePath), string.Format("FilePath is null/empty")))
                return;

            if (ValidateCommandTrue(string.IsNullOrEmpty(AutomationSettings.BigmodsUsername), string.Format("AutomationSettings.BigmodsUsername is null/empty")))
                return;

            if (ValidateCommandTrue(string.IsNullOrEmpty(AutomationSettings.BigmodsPassword), string.Format("AutomationSettings.BigmodsPassword is null/empty")))
                return;
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            if (!ProcessTaskResultFalse(TransferSuccess, "The transfer reported a failure. Check the log for more information"))
                return;
        }
        #endregion

        public virtual void Cancel()
        {
            if (WebClient != null)
                WebClient.CancelAsync();
        }
    }
}
