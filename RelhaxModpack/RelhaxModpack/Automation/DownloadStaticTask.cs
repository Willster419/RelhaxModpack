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
        public string DestinationPath { get; set; }
    }
}
