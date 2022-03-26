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
    /// A FileSourceTask provides an implementation for file tasks that have a source.
    /// </summary>
    public abstract class FileSourceTask : AutomationTask, IXmlSerializable
    {
        /// <summary>
        /// The path to the source file.
        /// </summary>
        public string SourceFilePath { get; set; } = string.Empty;

        #region Xml Serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(SourceFilePath) }).ToArray();
        }
        #endregion

        #region Task execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            SourceFilePath = ProcessMacro(nameof(SourceFilePath), SourceFilePath);
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            if (ValidateCommandTrue(string.IsNullOrEmpty(SourceFilePath), string.Format("SourceFilePath is empty string")))
                return;

            if (ValidateCommandTrue(!File.Exists(SourceFilePath), string.Format("SourceFilePath of {0} file does not exist", SourceFilePath)))
                return;
        }
        #endregion
    }
}
