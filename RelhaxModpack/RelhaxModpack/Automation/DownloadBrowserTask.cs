using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace RelhaxModpack.Automation
{
    public class DownloadBrowserTask : DownloadHtmlTask, IDownloadTask, IXmlSerializable
    {
        public int WaitTimeMs { get; } = 3000;

        public int Retries { get; } = 3;

        public override string Command { get; } = "download_browser";

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(WaitTimeMs), nameof(Retries) }).ToArray();
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
