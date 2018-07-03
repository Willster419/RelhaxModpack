using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack
{
    class VersionInfoHolder
    {
        public string LocalManagerVersion { get; set; } = "";
        public string OnlineManagerVersion { get; set; } = "";
        public string MinimumEditorVersion { get; set; } = "";
        public string DatabaseVersion { get; set; } = ""; 
    }
}
