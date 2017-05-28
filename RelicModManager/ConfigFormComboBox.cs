using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack
{
    public class ConfigFormComboBox : System.Windows.Forms.ComboBox, UIComponent
    {
        public Category catagory { get; set; }
        public Mod mod { get; set; }
        public Config config { get; set; }
    }
}
