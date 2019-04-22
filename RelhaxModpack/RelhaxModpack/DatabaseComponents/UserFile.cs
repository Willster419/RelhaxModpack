using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack
{
    public class UserFile
    {
        // this could be a single file or a search pattern with * or ?
        public string Pattern = "";

        //TEMP MAKE stuff below that's to remove properties so it's not picked up by fieldInfo

        // will try to speed up the restore backup function in case of ClanIcons, the "backup folder" will be pushed back at once (and not file by file)
        public bool placeBeforeExtraction { get; set; } = false;//TODO: remove?

        // this entry will be pßroceeded in any case (if package is checked), even if "save user data" option is "false"
        public bool systemInitiated { get; set; } = false;//TODO: remove

        //make this a property to be ignored also by xml saving
        //contain the list of actuall files saved. it's the full path inlcuding the file name
        public List<string> Files_saved { get; set; } = new List<string>();

        public override string ToString()
        {
            return Pattern + (placeBeforeExtraction ?  " (pre)" : "") + (systemInitiated ? " (sys)" : "");
        }
    }
}
