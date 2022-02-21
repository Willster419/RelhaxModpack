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
    public class SelectionListTabItem : TabItem, INotifyPropertyChanged, IOnCheckedComponent
    {
        static SelectionListTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectionListTabItem), new FrameworkPropertyMetadata(typeof(SelectionListTabItem)));
        }

        public SelectionListTabItem() : base()
        {

        }

        public SelectablePackage Package { get; set; }

        protected bool _isChildPackageChecked = false;

        public bool IsChildPackageChecked
        {
            get { return _isChildPackageChecked; }
            set
            {
                _isChildPackageChecked = value;
                OnPropertyChanged(nameof(IsChildPackageChecked));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

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
