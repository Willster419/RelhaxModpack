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

namespace RelhaxModpack.UIComponents
{
    /// <summary>
    /// Interaction logic for RelhaxWPFCheckBox.xaml
    /// </summary>
    public partial class RelhaxWPFCheckBox : CheckBox, IPackageUIComponent, INotifyPropertyChanged
    {
        public RelhaxWPFCheckBox()
        {
            InitializeComponent();
        }
        public SelectablePackage Package { get; set; }
        public void OnEnabledChanged(bool Enabled)
        {
            IsEnabled = Enabled;
        }
        public void OnCheckedChanged(bool Checked)
        {
            IsChecked = Checked;
        }
        public Brush TextColor
        {
            get
            { return Foreground; }
            set
            { Foreground = value; }
        }
        public Brush PanelColor
        {
            get
            {
                return Package.ParentBorder == null ? null : Package.ParentBorder.Background;
            }
            set
            {
                if (Package.ParentBorder != null)
                    Package.ParentBorder.Background = value;
            }
        }
        #region Data UI Binding
        private Color _DisabledColor = Colors.DarkGray;
        public Color DisabledColor
        {
            get
            {
                return _DisabledColor;
            }
            set
            {
                _DisabledColor = value;
                OnPropertyChanged(nameof(DisabledColor));
            }
        }
        //https://stackoverflow.com/questions/34651123/wpf-binding-a-background-color-initializes-but-not-updating
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            var handle = PropertyChanged;
            if (handle != null)
                handle(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
