using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack
{
    public class UserFiles
    {
        // this could be a single file or a search pattern with * or ?
        public string Pattern = "";
        // will try to speed up the restore backup function in case of ClanIcons, the "backup folder" will be pushed back at once (and not file by file)
        public bool placeBeforeExtraction = false;
        // this entry will be pßroceeded in any case (if package is checked), even if "save user data" option is "false"
        public bool systemInitiated = false;

        public override string ToString()
        {
            return Pattern + (placeBeforeExtraction ?  " (pre)" : "") + (systemInitiated ? " (sys)" : "");
        }
    }
}
