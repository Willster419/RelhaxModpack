using RelhaxModpack.Database;
using RelhaxModpack.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// A FileDestinationTask provides an implementation for file tasks that have a destination.
    /// </summary>
    public abstract class FileDestinationTask : FileSourceTask, IXmlSerializable
    {
        /// <summary>
        /// The path to the destination file.
        /// </summary>
        public string DestinationFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Flag to indicate if the file at the destination path (if it existed) was deleted successfully.
        /// </summary>
        protected bool destinationDeleteResult = true;

        #region Xml Serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(DestinationFilePath) }).ToArray();
        }
        #endregion

        #region Task execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            base.ProcessMacros();

            DestinationFilePath = ProcessMacro(nameof(DestinationFilePath), DestinationFilePath);
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (ValidateCommandTrue(string.IsNullOrEmpty(DestinationFilePath), string.Format("DestinationPath is empty string")))
                return;
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task RunTask()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            string directoryPath = Path.GetDirectoryName(DestinationFilePath);
            Logging.Info("Checking if destination folder {0} already exists", directoryPath);
            if ((!string.IsNullOrWhiteSpace(directoryPath)) && (!Directory.Exists(directoryPath)))
            {
                Directory.CreateDirectory(directoryPath);
            }

            Logging.Info("Checking if destination file {0} already exists", DestinationFilePath);
            if (File.Exists(DestinationFilePath))
            {
                Logging.Info("Destination file already exists, delete it");
                destinationDeleteResult = FileUtils.FileDelete(DestinationFilePath);

                if (destinationDeleteResult)
                {
                    Logging.Info("Destination file deleted");
                }
                else
                {
                    Logging.Info("Destination file failed to delete, abort task");
                }
            }
        }
        #endregion
    }
}
