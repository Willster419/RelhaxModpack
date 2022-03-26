using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// A DirectoryTask is a base class to use for directory based tasks.
    /// </summary>
    public abstract class DirectoryTask : AutomationTask, IXmlSerializable
    {
        /// <summary>
        /// A directory path to use for a derived task.
        /// </summary>
        public string DirectoryPath { get; set; }

        #region Xml Serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(DirectoryPath) }).ToArray();
        }
        #endregion

        #region Task execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            DirectoryPath = ProcessMacro(nameof(DirectoryPath), DirectoryPath);
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            if (ValidateCommandStringNullEmptyTrue(nameof(DirectoryPath), DirectoryPath))
                return;
            if (ValidateCommandFalse(Directory.Exists(DirectoryPath), string.Format("DirectoryPath of {0} file does not exist", DirectoryPath)))
                return;
        }
        #endregion
    }
}
