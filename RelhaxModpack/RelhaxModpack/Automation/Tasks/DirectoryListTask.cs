﻿using RelhaxModpack.Database;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class DirectoryListTask : DirectorySearchTask, IXmlSerializable
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "directory_search";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        public string MacroPrefix { get; set; }

        #region Xml Serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] {nameof(MacroPrefix) }).ToArray();
        }
        #endregion

        #region Task Execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            MacroPrefix = ProcessMacro(nameof(MacroPrefix), MacroPrefix);
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            base.ValidateCommands();
            if (ValidateCommandStringNullEmptyTrue(nameof(MacroPrefix), MacroPrefix))
                return;
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            searchResults = FileUtils.FileSearch(DirectoryPath, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly, false, false, SearchPattern);
            if (searchResults == null || searchResults.Count() == 0)
                return;

            for (int i = 0; i < searchResults.Count(); i++)
            {
                string macroNameToAdd = string.Format("{0}_{1}", MacroPrefix, i);
                string macroValueToAdd = searchResults[i];
                Logging.Info("Creating macro, Name: {0}, Value: {1}", macroNameToAdd, macroValueToAdd);
                Macros.Add(new AutomationMacro() { MacroType = MacroType.Local, Name = macroNameToAdd, Value = macroValueToAdd });
            }
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultTrue(searchResults == null, "The searchResult array returned null"))
                return;

            if (ProcessTaskResultTrue(searchResults.Count() == 0, "The searchResult array returned 0 results"))
                return;
        }
        #endregion
    }
}
