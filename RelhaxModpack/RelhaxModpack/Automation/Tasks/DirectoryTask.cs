using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public abstract class DirectoryTask : AutomationTask, IXmlSerializable
    {
        public string DirectoryPath { get; set; }

        #region Xml Serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(DirectoryPath) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            DirectoryPath = ProcessMacro(nameof(DirectoryPath), DirectoryPath);
        }

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
