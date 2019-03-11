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

namespace RelhaxWPFConvert
{
    /// <summary>
    /// Interaction logic for RelhaxCheckBox.xaml
    /// </summary>
    public partial class RelhaxCheckBox : CheckBox
    {
        public SolidColorBrush CheckboxDisabledColorBrush { get; set; } = new SolidColorBrush(Colors.Red);

        public RelhaxCheckBox()
        {
            InitializeComponent();
        }
    }
}
