using RelhaxModpack.Database;
using RelhaxModpack.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public abstract class FileDestinationTask : FileTask, IXmlSerializable
    {
        public string DestinationFilePath { get; set; } = string.Empty;

        protected bool destinationDeleteResult = true;

        #region Xml Serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(DestinationFilePath) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();

            DestinationFilePath = ProcessMacro(nameof(DestinationFilePath), DestinationFilePath);
        }

        public override Task RunTask()
        {
            Logging.Info("Checking if destination file {0} already exists", DestinationFilePath);

            if (File.Exists(DestinationFilePath))
            {
                Logging.Info("Destination file already exists, delete it");
                destinationDeleteResult = FileUtils.FileDelete(DestinationFilePath);
            }

            if (destinationDeleteResult)
            {
                Logging.Info("Destination file deleted");
            }
            else
            {
                Logging.Info("Destination file failed to delete, abort task");
            }

            return null;
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (ValidateCommandTrueNew(string.IsNullOrEmpty(DestinationFilePath), string.Format("DestinationPath is empty string")))
                return;

            if (ValidateCommandTrueNew(!File.Exists(DestinationFilePath), string.Format("DestinationPath of {0} file does not exist", DestinationFilePath)))
                return;
        }
        #endregion
    }
}
