using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.UIComponents
{
    public class EditorComboBoxItem
    {
        public DatabasePackage Package { get; set; }
        public string DisplayName
        {
            get
            {
                return Package == null ? "(null)" : Package.PackageName;
            }
        }
        public EditorComboBoxItem(DatabasePackage package)
        {
            Package = package;
        }
        public override string ToString()
        {
            return DisplayName;
        }
    }
}
