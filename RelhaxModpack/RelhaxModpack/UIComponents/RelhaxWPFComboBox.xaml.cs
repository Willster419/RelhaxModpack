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
    /// Interaction logic for RelhaxWPFComboBox.xaml
    /// </summary>
    public partial class RelhaxWPFComboBox : ComboBox, IPackageUIComponent, INotifyPropertyChanged
    {
        public RelhaxWPFComboBox()
        {
            InitializeComponent();
        }
        public SelectablePackage Package { get; set; }
        public SelectionChangedEventHandler handler;
        public void OnEnabledChanged(bool Enabled)
        {

        }
        public void OnCheckedChanged(bool Checked)
        {

        }
        public Brush TextColor
        {
            get
            { return null; }
            set
            { }
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
        public void OnDropDownSelectionChanged(SelectablePackage spc, bool value)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                ComboBoxItem cbi = (ComboBoxItem)Items[i];
                if (cbi.Package.Equals(spc) && value && cbi.Package.Enabled)
                {
                    if (handler != null)
                        SelectionChanged -= handler;
                    SelectedItem = cbi;
                    if (handler != null)
                        SelectionChanged += handler;
                    continue;
                }//if value is false it will uncheck all the packages
                if (cbi.Package.Enabled && cbi.Package.Checked)
                    cbi.Package.Checked = false;
            }
            if (!value)
            {
                SelectionChanged -= handler;
                SelectedIndex = 0;
                SelectionChanged += handler;
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
