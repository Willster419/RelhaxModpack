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
    /// A RelhaxBorder provides additional functionality over a Border to indicate if the child packages inside this border are enabled and checked.
    /// It enables highlight logic for ui interaction in the selection list.
    /// </summary>
    public class RelhaxBorder : Border, INotifyPropertyChanged
    {
        /// <summary>
        /// Override framework metadata from Border to this class's RelhaxBorder.
        /// </summary>
        static RelhaxBorder()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RelhaxBorder), new FrameworkPropertyMetadata(typeof(RelhaxBorder)));
        }

        /// <summary>
        /// Create an instance of the RelhaxBorder class.
        /// </summary>
        public RelhaxBorder() : base()
        {

        }

        private bool _isChildPackageChecked = false;

        /// <summary>
        /// Get or set if this border's associated package has any child packages that are enabled and checked.
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
    }
}
