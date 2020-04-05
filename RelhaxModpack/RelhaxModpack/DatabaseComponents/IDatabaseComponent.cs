using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RelhaxModpack.DatabaseComponents
{
    public interface IDatabaseComponent
    {
        /// <summary>
        /// When a databasePackage, the internal packageName. When category, the category name
        /// </summary>
        string ComponentInternalName { get; }

        /// <summary>
        /// Reference for the UI element of this package in the database editor
        /// </summary>
        TreeViewItem EditorTreeViewItem { get; set; }

        /// <summary>
        /// A list of database managers who are known to maintain this category
        /// </summary>
        string Maintainers { get; set; }
    }
}
