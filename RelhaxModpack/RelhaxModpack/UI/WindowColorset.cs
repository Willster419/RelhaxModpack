using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// Contains all color definitions to apply for that window, including the list of individual component color definitions
    /// </summary>
    public class WindowColorset
    {
        /// <summary>
        /// The type object for the type of window this colorset belongs to
        /// </summary>
        public Type WindowType { get; set; } = null;

        /// <summary>
        /// The background color of the window
        /// </summary>
        public CustomBrush BackgroundBrush { get; set; } = null;

        /// <summary>
        /// A list of custom colors to apply for individual UI components
        /// </summary>
        public Dictionary<string, ComponentColorset> ComponentColorsets { get; set; } = null;

        /// <summary>
        /// Flag for if this window colorset has had it's original (default) theme backed up
        /// </summary>
        public bool ColorsetBackedUp { get; set; } = false;
    }
}
