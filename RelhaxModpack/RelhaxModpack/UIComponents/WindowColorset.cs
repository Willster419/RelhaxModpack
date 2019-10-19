using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.UIComponents
{
    public class WindowColorset
    {
        public Type WindowType { get; set; } = null;

        public CustomBrush BackgroundBrush { get; set; } = null;

        public Dictionary<string, ComponentColorset> ComponentColorsets { get; set; } = null;

        public bool ColorsetBackedUp { get; set; } = false;
    }
}
