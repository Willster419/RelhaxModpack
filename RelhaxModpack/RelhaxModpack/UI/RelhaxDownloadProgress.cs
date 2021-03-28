using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.UI
{
    public class RelhaxDownloadProgress : RelhaxProgress
    {
        public DatabasePackage DatabasePackage = null;

        public DownloadProgressState DownloadProgressState = DownloadProgressState.None;
    }
}
