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

        //make this a property to be ignored also by xml saving
        //contain the list of actuall files saved. it's the full path inlcuding the file name
        public List<string> Files_saved { get; set; } = new List<string>();

        public override string ToString()
        {
            return Pattern;
        }
    }
}
