using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RelhaxModpack.UI
{
    //https://stackoverflow.com/questions/35556975/horizontalalignment-stretch-not-working-in-treeviewitem
    /// <summary>
    /// A tree view that allows for stretch horizontal alignment of the header item
    /// </summary>
    public class StretchingTreeView : TreeView
    {
        /// <summary>
        /// Overrides the parent GetContainerForItemOverride() method
        /// </summary>
        /// <returns>A new StretchingTreeViewItem object</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new StretchingTreeViewItem();
        }

        /// <summary>
        /// Overrides the parent IsItemItsOwnContainerOverride() method
        /// </summary>
        /// <param name="item">The item to test</param>
        /// <returns>True if the item is of StretchingTreeViewItem class, false otherwise</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is StretchingTreeViewItem;
        }
    }
}
