using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// An interface that requires the implementing task to use properties needed for HtmlPath navigation.
    /// </summary>
    /// <remarks>HtmlPath navigation is used across multiple abstract parent classes of AutomationTask, and this ensures they all use the same argument names.</remarks>
    public interface IHtmlParseTask
    {
        /// <summary>
        /// The HtmlPath argument to use for parsing.
        /// </summary>
        string HtmlPath { get; set; }
    }
}
