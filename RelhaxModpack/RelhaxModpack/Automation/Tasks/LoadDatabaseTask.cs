using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class LoadDatabaseTask : DatabaseTask, IDatabaseTask, IXmlSerializable
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "load_database";

        public override string Command { get { return TaskCommandName; } }

        protected DatabaseLoadFailCode failCode;

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();

            //CustomDatabasePath *could* be empty, if the user wants to use the one set in his AutomationSettings path
            if (!string.IsNullOrWhiteSpace(CustomDatabasePath))
            {
                string directoryPath = Path.GetDirectoryName(CustomDatabasePath);
                //the directory path could be null if the user wants to load/save the database right here and is using a relative path
                if (ValidateCommandTrue(!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath), string.Format("The path to load the database does not exist: {0}", directoryPath)))
                    return;
            }
        }

        public override async Task RunTask()
        {
            await base.RunTask();
            failCode = await DatabaseManager.LoadDatabaseTestAsync(useCustomPath ? CustomDatabasePath : AutomationSettings.DatabaseSavePath);

            if (failCode == DatabaseLoadFailCode.None)
            {
                AutomationSequence.UpdateDatabasePackageList();
                AutomationSequence.UpdateCurrentDatabasePackage();
                AutomationSequence.AutomationSequencer?.UpdateDatabasePackageList();
            }
        }

        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultFalse(failCode == DatabaseLoadFailCode.None, string.Format("Database load result returned {0}", failCode.ToString())))
                return;
        }
        #endregion
    }
}
