using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// An interface than when implemented implies that the UI component has a reference to the Package object and a method to handle when the Package object gets the Checked Property toggled.
    /// </summary>
    /// <seealso cref="SelectablePackage.Checked"/>
    public interface IOnCheckedComponent
    {
        /// <summary>
        /// The package associated with this UI component
        /// </summary>
        SelectablePackage Package { get; set; }

        /// <summary>
        /// Method for when the checked property changes
        /// </summary>
        /// <param name="Checked">The value of the checked property</param>
        void OnCheckedChanged(bool Checked);
    }
}
