using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public abstract class DirectoryDestinationTask : DirectorySearchTask, IXmlSerializable
    {
        public string DestinationPath { get; set; }

        #region Xml Serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(DestinationPath) }).ToArray();
        }
        #endregion

        #region Task Execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            DestinationPath = ProcessMacro(nameof(DestinationPath), DestinationPath);
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();
            if (ValidateCommandStringNullEmptyTrue(nameof(DestinationPath), DestinationPath))
                return;
        }

        public async override Task RunTask()
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
