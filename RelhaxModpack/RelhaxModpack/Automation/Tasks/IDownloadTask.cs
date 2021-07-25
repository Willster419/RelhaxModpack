using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// A task that implements this interface implies that it will always have a destination location for the resultant file
    /// </summary>
    public interface IDownloadTask
    {
        string DestinationPath { get; set; }

        string Url { get; set; }
    }
}
