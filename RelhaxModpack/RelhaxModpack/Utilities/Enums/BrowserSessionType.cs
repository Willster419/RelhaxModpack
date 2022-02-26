using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The type of browser engine to use when running a browser session during task execution.
    /// </summary>
    public enum BrowserSessionType
    {
        /// <summary>
        /// Use the WebClient implementation.
        /// </summary>
        WebClient,

        /// <summary>
        /// Use the HttpClient implementation.
        /// </summary>
        HttpClient,

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
