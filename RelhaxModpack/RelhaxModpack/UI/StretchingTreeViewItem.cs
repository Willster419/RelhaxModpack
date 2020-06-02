using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// Allows for the header control to stretch all the way across a filling container
    /// </summary>
    /// <remarks>see https://stackoverflow.com/questions/35556975/horizontalalignment-stretch-not-working-in-treeviewitem </remarks>
    public class StretchingTreeViewItem : TreeViewItem
    {
        /// <summary>
        /// Create an instance of the StretchingTreeViewItem UI component
        /// </summary>
        public StretchingTreeViewItem()
        {
            this.Loaded += new RoutedEventHandler(StretchingTreeViewItem_Loaded);
        }

        private void StretchingTreeViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            // The purpose of this code is to stretch the Header Content all the way across the TreeView. 
            if (this.VisualChildrenCount > 0)
            {
                Grid grid = this.GetVisualChild(0) as Grid;
                if (grid != null && grid.ColumnDefinitions.Count == 3)
                {
                    // Remove the middle column which is set to Auto and let it get replaced with the 
                    // last column that is set to Star.
                    grid.ColumnDefinitions.RemoveAt(1);
                }
            }
        }

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
