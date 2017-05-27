using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack
{
    //an item to add ot a ComboBox
    class ComboBoxItem
    {
        public Config config { get; set; }
        public string displayName { get; set; }
        public ComboBoxItem(Config cfg, string display)
        {
            config = cfg;
            displayName = display;
        }
        public override string ToString()
        {
            return displayName;
        }
    }
}
