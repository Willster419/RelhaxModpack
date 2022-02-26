using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// A SelectionListTabControl provides additional functionality that upon a selection being changed, has each SelectionListTabItem check if any packages in itself are checked.
    /// </summary>
    /// <seealso cref="SelectionListTabItem"/>
    public class SelectionListTabControl : TabControl
    {
        /// <summary>
        /// Create an instance of the SelectionListTabControl.
        /// </summary>
        public SelectionListTabControl() : base()
        {
            
        }

        /// <summary>
        /// Including raising the SelectionChanged event, checks each SelectionListTabItem if any packages in itself are checked.
        /// </summary>
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            foreach (SelectionListTabItem tabControl in this.Items)
            {
                tabControl.IsChildPackageChecked = tabControl.Package.AnyPackagesChecked();
            }
        }
    }
}
