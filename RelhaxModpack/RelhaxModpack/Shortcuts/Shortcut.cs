using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack.Shortcuts
{
    /// <summary>
    /// Represents instructions on how to create a shortcut
    /// </summary>
    public class Shortcut
    {
        /// <summary>
        /// The target of the shortcut
        /// </summary>
        public string Path = string.Empty;

        /// <summary>
        /// The name for the shortcut
        /// </summary>
        public string Name = string.Empty;

        /// <summary>
        /// Flag for in the installer to actually create the shortcut
        /// </summary>
        public bool Enabled = false;

        /// <summary>
        /// String representation of the object
        /// </summary>
        /// <returns>The name and target of the shortcut</returns>
        public override string ToString()
        {
            return string.Format("Name={0} Target={1}", Name, Path);
        }
    }

    
}
