using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelhaxModpack.InstallerComponents;

namespace RelhaxModpack.UIComponents
{
    public class RelhaxProgress
    {
        //3 levels of stuff:
        //child stuff, parrent, total stuff
        public int ChildCurrent, ChildTotal;
        public string ChildCurrentProgress;
        public int ParrentCurrent, ParrentTotal;
        public string ParrentCurrentProgress;
        public int TotalCurrent, TotalTotal;//#meta
        public string TotalCurrentProgress;
        public string ReportMessage;
    }

    public class RelhaxInstallerProgress : RelhaxProgress
    {
        public long BytesProcessed, BytesTotal;
        public string Filename, EntryFilename;
        public InstallerExitCodes InstallStatus;
    }
}
