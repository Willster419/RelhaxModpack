using RelhaxModpack.Database;
using System.Linq;
using System.IO;
using RelhaxModpack.Utilities.Enums;
using System.Threading.Tasks;
using System;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// A DatabaseTask provides an implementation for managing the loaded package database.
    /// </summary>
    public abstract class DatabaseTask : AutomationTask, IXmlSerializable
    {
        /// <summary>
        /// A custom location to use for loading or saving a database. This corresponds to the root database file, "database.xml".
        /// </summary>
        public string CustomDatabasePath { get; set; } = string.Empty;

        /// <summary>
        /// The database manager to load or save the database.
        /// </summary>
        protected DatabaseManager DatabaseManager { get { return AutomationSequence.DatabaseManager; } }

        /// <summary>
        /// Flag to indicate if the task argument CustomDatabasePath was parsed to a null or empty string (an error with the macros).
        /// </summary>
        bool customPathMacroError = false;

        /// <summary>
        /// Flag to indicate if the CustomDatabasePath task argument should be used for loading or saving the database.
        /// </summary>
        protected bool useCustomPath = false;

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(CustomDatabasePath) }).ToArray();
        }
        #endregion

        #region Task execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            bool customPathEmptyBefore = string.IsNullOrEmpty(CustomDatabasePath);
            CustomDatabasePath = ProcessMacro(nameof(ProcessMacro), CustomDatabasePath);
            bool custompathEmptyAfter = string.IsNullOrEmpty(CustomDatabasePath);

            customPathMacroError = !customPathEmptyBefore && custompathEmptyAfter;
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            if (ValidateCommandTrue(customPathMacroError, "The custom database path was a value before macro, and empty after the macro"))
                return;

            if (ValidateCommandTrue(DatabaseManager == null, string.Format("DatabaseManager is null (This is an internal application error)")))
                return;
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task RunTask()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            useCustomPath = !string.IsNullOrEmpty(CustomDatabasePath);
        }
        #endregion
    }
}
