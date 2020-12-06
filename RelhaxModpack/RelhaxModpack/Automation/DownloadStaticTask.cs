using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class DownloadStaticTask : AutomationTask, IDownloadTask, IXmlSerializable
    {
        public string DestinationPath { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public override string Command { get; } = "download_static";

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(DestinationPath), nameof(Url) }).ToArray();
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
