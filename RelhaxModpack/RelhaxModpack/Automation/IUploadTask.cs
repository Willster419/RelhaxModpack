using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public interface IUploadTask
    {
        string SourcePath { get; set; }

        string Url { get; set; }
    }
}
