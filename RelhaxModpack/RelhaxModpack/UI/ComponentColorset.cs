using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// A ComponentColorset is a custom color object to apply to a particular ID'd UI object in the window
    /// </summary>
    /// <remarks>Highlight and Selected colors are always changed (if at all) by WPF databound properties at the ClassColorset level</remarks>
    public class ComponentColorset
    {
        /// <summary>
        /// The ID of the UI object in the window of which the component colors will be applied to
        /// </summary>
        public string ID { get; set; } = string.Empty;

        /// <summary>
        /// The Background brush color
        /// </summary>
        public CustomBrush BackgroundBrush { get; set; } = null;

        /// <summary>
        /// The Foreground brush color
        /// </summary>
        public CustomBrush ForegroundBrush { get; set; } = null;
    }
}
