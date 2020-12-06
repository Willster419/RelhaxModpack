using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class DownloadHtmlTask : DownloadStaticTask, IDownloadTask, IXmlSerializable
    {
        public string HtmlPath { get; set; } = string.Empty;

        public override string Command { get; } = "download_html";

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(HtmlPath) }).ToArray();
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
