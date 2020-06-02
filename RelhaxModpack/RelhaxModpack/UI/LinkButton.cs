using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// Wrapper class for the Button class of the System.Windows.Controls namespace. Adds the Link property.
    /// </summary>
    /// <remarks>Allows for starting a process from the Link value to a website</remarks>
    public class LinkButton : Button 
    {
        /// <summary>
        /// The URL for which to start the process based on
        /// </summary>
        public string Link { get; set; }
    }
}
