using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class ShellExecuteTask : AutomationTask, IXmlSerializable
    {
        public string Cmd { get; set; } = string.Empty;

        public string Wd { get; set; } = string.Empty;

        public override string Command { get; } = "shell_exec";

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(Wd), nameof(Cmd) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ValidateCommands()
        {
            throw new NotImplementedException();
        }

        public override void RunTask()
        {
            throw new NotImplementedException();
        }

        public override void ProcessTaskResults()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
