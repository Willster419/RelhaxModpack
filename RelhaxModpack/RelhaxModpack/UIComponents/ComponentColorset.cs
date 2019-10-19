using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.UIComponents
{
    public class ComponentColorset
    {
        public string ID { get; set; } = string.Empty;

        public CustomBrush BackgroundBrush { get; set; } = null;

        public CustomBrush ForegroundBrush { get; set; } = null;
    }
}
