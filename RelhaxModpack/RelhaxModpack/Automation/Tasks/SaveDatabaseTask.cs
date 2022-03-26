using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// Saves a package database from the default or a custom location.
    /// </summary>
    public class SaveDatabaseTask : DatabaseTask, IXmlSerializable
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "save_database";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

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
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            base.ProcessMacros();
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            base.ValidateCommands();

            //CustomDatabasePath *could* be empty, if the user wants to use the one set in his AutomationSettings path
            if (!string.IsNullOrWhiteSpace(CustomDatabasePath))
            {
                string directoryPath = Path.GetDirectoryName(CustomDatabasePath);
                //the directory path could be null if the user wants to load/save the database right here and is using a relative path
                if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Logging.Info("The directory path {0} was not found to exist and was created", directoryPath);
                }
            }
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            await base.RunTask();
            DatabaseManager.SaveDatabase(useCustomPath ? CustomDatabasePath : AutomationSettings.DatabaseSavePath);
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            //stub - nothing to validate
        }
        #endregion
    }
}
