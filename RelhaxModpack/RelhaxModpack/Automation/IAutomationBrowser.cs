using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RelhaxModpack.Automation
{
    /// <summary>
    /// Provides an interface of all commonly used functions for all browser API implementations to use when running automation tasks.
    /// </summary>
    public interface IAutomationBrowser : IDisposable
    {
        /// <summary>
        /// Occurs when a web page has been fully downloaded and rendered by the browser.
        /// </summary>
        /// <remarks>This may fire multiple times if the loaded page has dynamic content (e.g. AJAX, javascript).</remarks>
        /// <seealso cref="NavigationCompleted"/>
        event BrowserManagerDelegate DocumentCompleted;

        /// <summary>
        /// Occurs when the web browser has finished navigating to the new content. This does not mean that the new content has been fully parsed.
        /// </summary>
        /// <seealso cref="DocumentCompleted"/>
        event BrowserManagerDelegate NavigationCompleted;

        /// <summary>
        /// Get the web browser implementation as a control object.
        /// </summary>
        Control Browser { get; }

        /// <summary>
        /// Gets or sets the height of the browser control.
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// Gets or sets the width of the browser control.
        /// </summary>
        int Width { get; set; }

        /// <summary>
        /// Return the currently parsed html document from the browser implementation.
        /// </summary>
        /// <returns>The currently parsed html document as a string.</returns>
        /// <remarks>Most implementations only return the document's "outer html" section, which may not include the document header.</remarks>
        string GetHtmlDocument();

        /// <summary>
        /// Start a navigation to the given url.
        /// </summary>
        /// <param name="url">The resource to navigate to.</param>
        /// <remarks>In most browser implementations, the navigation is asynchronous and must be listened for with the document and navigation completed events.</remarks>
        void Navigate(string url);

        /// <summary>
        /// Cancels a browser navigation operation.
        /// </summary>
        void Cancel();
    }
}
