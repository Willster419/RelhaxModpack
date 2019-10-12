using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RelhaxModpack.UIComponents
{
    public class CustomBrush
    {
        public Brush Brush = null;

        public bool IsValid = false;

        public bool IsBound = false;

        public string BoundPropertyName = string.Empty;
    }
}
