using System.Collections.Generic;
using System.Windows.Forms;

namespace RelhaxModpack
{
    class SelectionCheckBox : CheckBox
    {
       public string Directory { get; set; }
       public List<string> NameList { get; set; }
    }
}
