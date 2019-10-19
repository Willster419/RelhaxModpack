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
        public Brush Brush { get; set; } = null;

        public bool IsValid { get; set; } = false;
    }

    public class CustomPropertyBrush : CustomBrush
    {
        public string BrushPropertyName { get; set; } = string.Empty;
    }
}
