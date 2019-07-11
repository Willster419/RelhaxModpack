using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RelhaxModpack
{
    public class SelectionCheckBox : CheckBox
    {
        public string Directory { get; set; }
        public List<string> NameList { get; set; }
        public ulong SizeOnDisk { get; set; }
    }
}
