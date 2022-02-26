using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// A SelectionListTabItem provides additional functionality over a TabItem to indicate if child packages inside this ui component's package are enabled and checked.
    /// It enables highlight logic for ui interaction in the selection list.
    /// </summary>
    public class SelectionListTabItem : TabItem, INotifyPropertyChanged, IOnCheckedComponent
    {
        /// <summary>
        /// Override framework metadata from TabItem to this class's SelectionlistTabItem.
        /// </summary>
        static SelectionListTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectionListTabItem), new FrameworkPropertyMetadata(typeof(SelectionListTabItem)));
        }

        /// <summary>
        /// Create an instance of the SelectionListTabitem class.
        /// </summary>
        public SelectionListTabItem() : base()
        {

        }

        /// <summary>
        /// The package associated with this UI component
        /// </summary>
        public SelectablePackage Package { get; set; }

        private bool _isChildPackageChecked = false;

        /// <summary>
        /// Get or set if this component's package has any child packages that are enabled and checked.
        /// </summary>
        public bool IsChildPackageChecked
        {
            get { return _isChildPackageChecked; }
            set
            {
                _isChildPackageChecked = value;
                OnPropertyChanged(nameof(IsChildPackageChecked));
            }
        }

        /// <summary>
        /// Occurs after a property that uses OnPropertyChanged has been set.
        /// </summary>
        /// <seealso cref="OnPropertyChanged(string)"/>
        /// <seealso cref="INotifyPropertyChanged"/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called from a property in this class that wants to tell PropertyChanged listeners that it has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        /// <seealso cref="PropertyChanged"/>
        /// <seealso cref="INotifyPropertyChanged"/>
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Handles ui logic processing when the associated package's checked property is set.
        /// </summary>
        /// <param name="Checked">The checked value from the package.</param>
        public void OnCheckedChanged(bool Checked)
        {
            if (Package.Visible && Package.IsStructureVisible)
            {
                if (Checked || Package.AnyPackagesChecked())
                {
                    this.IsChildPackageChecked = true;
                }
                else
                {
                    this.IsChildPackageChecked = false;
                }
            }
        }
    }
}
