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

        protected bool DatabaseManagerExitResult { get; set; } = false;

        bool customPathMacroError = false;

        protected bool useCustomPath = false;

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(CustomDatabasePath) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            bool customPathEmptyBefore = string.IsNullOrEmpty(CustomDatabasePath);
            CustomDatabasePath = ProcessMacro(nameof(ProcessMacro), CustomDatabasePath);
            bool custompathEmptyAfter = string.IsNullOrEmpty(CustomDatabasePath);

            customPathMacroError = !customPathEmptyBefore && custompathEmptyAfter;
        }

        public override void ValidateCommands()
        {
            if (ValidateCommandTrue(customPathMacroError, "The custom database path was a value before macro, and empty after the macro"))
                return;

            if (ValidateCommandTrue(DatabaseManager == null, string.Format("DatabaseManager is null (This is an internal application error)")))
                return;
        }

        public override async Task RunTask()
        {
            useCustomPath = !string.IsNullOrEmpty(CustomDatabasePath);
        }

        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultTrue(!DatabaseManagerExitResult, string.Format("{0} returned false", nameof(DatabaseManagerExitResult))))
                return;
        }
        #endregion
    }
}
