using RelhaxModpack.Database;
using System.Linq;
using System.IO;
using RelhaxModpack.Utilities.Enums;
using System.Threading.Tasks;
using System;

namespace RelhaxModpack.Automation
{
    public abstract class DatabaseTask : AutomationTask, IDatabaseTask, IXmlSerializable
    {
        public string CustomDatabasePath { get; set; } = string.Empty;

        protected DatabaseManager DatabaseManager { get { return AutomationSequence.DatabaseManager; } }

        protected bool DatabaseManagerExitResult { get; set; }

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(CustomDatabasePath) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            CustomDatabasePath = ProcessMacro(nameof(ProcessMacro), CustomDatabasePath);
        }

        public override void ValidateCommands()
        {
            if (DatabaseManager == null)
            {
                ExitCode = 1;
                ErrorMessage = string.Format("ExitCode {0}: DatabaseManager is null (This is an internal application error)", ExitCode);
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
                return;
            }
        }

        public override void ProcessTaskResults()
        {
            if (!DatabaseManagerExitResult)
            {
                ExitCode = 2;
                ErrorMessage = string.Format("ExitCode {0}: {1} returned false", ExitCode, nameof(DatabaseManagerExitResult));
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
            }
        }
        #endregion
    }
}
