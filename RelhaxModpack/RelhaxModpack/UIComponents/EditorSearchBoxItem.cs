using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.UIComponents
{
    public class EditorSearchBoxItem : System.Windows.Controls.ComboBoxItem
    {
        public DatabasePackage Package { get; set; }
        public string DisplayName { get; set; }
        public EditorSearchBoxItem(DatabasePackage package, string display)
        {
            Package = package;
            DisplayName = display;
        }
        public override string ToString()
        {
            return DisplayName;
        }
    }
}
