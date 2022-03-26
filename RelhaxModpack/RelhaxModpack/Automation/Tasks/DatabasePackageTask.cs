using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// A DatabasePackageTask provides a framework for handling of the current DatabasePackage, and access to the list of loaded packages.
    /// </summary>
    public abstract class DatabasePackageTask : AutomationTask
    {
        /// <summary>
        /// Get the DatabasePackage associated with this automation sequence.
        /// </summary>
        public DatabasePackage DatabasePackage { get { return AutomationSequence.Package; } }

        /// <summary>
        /// Get the list of loaded database packages.
        /// </summary>
        public List<DatabasePackage> DatabasePackages { get { return AutomationSequence.DatabasePackages; } }

        #region Task execution
        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            if (ValidateCommandTrue(DatabasePackage == null, string.Format("DatabasePackage is null (This is an internal application error)")))
                return;

            if (ValidateCommandTrue(DatabasePackages == null, string.Format("DatabasePackages is null (This is an internal application error)")))
                return;
        }
        #endregion
    }
}
