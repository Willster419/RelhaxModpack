using RelhaxModpack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// An interface for all components in the database.
    /// </summary>
    public interface IDatabaseComponent : IComponentWithID
    {
        /// <summary>
        /// Reference for the UI element of this package in the database editor.
        /// </summary>
        TreeViewItem EditorTreeViewItem { get; set; }

        /// <summary>
        /// A list of database managers who are known to maintain this component.
        /// </summary>
        string Maintainers { get; set; }

        /// <summary>
        /// Returns a list of database managers who are known to maintain this component.
        /// </summary>
        List<string> MaintainersList { get; }
    }
}
