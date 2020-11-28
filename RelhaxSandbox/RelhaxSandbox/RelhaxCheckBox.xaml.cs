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

namespace RelhaxSandbox
{
    /// <summary>
    /// Interaction logic for RelhaxCheckBox.xaml
    /// </summary>
    public partial class RelhaxCheckBox : CheckBox, INotifyPropertyChanged
    {
        public SolidColorBrush CheckboxDisabledColorBrush { get; set; } = new SolidColorBrush(Colors.Red);
        private Color _CheckboxDisabledColor = Colors.Red;
        public Color CheckboxDisabledColor
        {
            get { return _CheckboxDisabledColor; }
            set
            {
                _CheckboxDisabledColor = value;
                OnPropertyChanged(nameof(CheckboxDisabledColor));
            }
        }

        private Visibility _PopularModVisability = Visibility.Hidden;
        public Visibility PopularModVisability
        {
            get { return _PopularModVisability; }
            set
            {
                _PopularModVisability = value;
                OnPropertyChanged(nameof(PopularModVisability));
            }
        }

        public RelhaxCheckBox()
        {
            InitializeComponent();
        }
        //https://stackoverflow.com/questions/34651123/wpf-binding-a-background-color-initializes-but-not-updating
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            var handle = PropertyChanged;
            if (handle != null)
                handle(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
