using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The type of browser engine to use when running a browser for a single task.
    /// </summary>
    public enum BrowserType
    {
        /// <summary>
        /// Use the WebBrowser (IE/Trident) implementation.
        /// </summary>
        WebBrowser,

        /// <summary>
        /// Use the WebView (Edge) implementation (v1).
        /// </summary>
        WebView,

        /// <summary>
        /// Use the WebView (Edge) implementation (v2).
        /// </summary>
        WebView2
    }
}
