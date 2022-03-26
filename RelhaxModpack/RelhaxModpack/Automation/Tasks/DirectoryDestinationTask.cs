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
    /// Provides a definition for tasks that have a destination for their operation.
    /// </summary>
    public abstract class DirectoryDestinationTask : DirectorySearchTask, IXmlSerializable
    {
        /// <summary>
        /// The destination of where to download the file to, including the file name. If the path to the file doesn't exist, then it will be created.
        /// </summary>
        public string DestinationPath { get; set; } = string.Empty;

        #region Xml Serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(DestinationPath) }).ToArray();
        }
        #endregion

        #region Task Execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            DestinationPath = ProcessMacro(nameof(DestinationPath), DestinationPath);
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            base.ValidateCommands();
            if (ValidateCommandStringNullEmptyTrue(nameof(DestinationPath), DestinationPath))
                return;
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            await base.RunTask();

            if (!Directory.Exists(DestinationPath))
            {
                Logging.Debug("DestinationPath {0} does not exist, create", DestinationPath);
                Directory.CreateDirectory(DestinationPath);
            }
        }
            #endregion
    }
}
