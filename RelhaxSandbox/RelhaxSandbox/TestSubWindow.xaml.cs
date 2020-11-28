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
using System.Windows.Shapes;

namespace RelhaxSandbox
{
    /// <summary>
    /// Interaction logic for TestSubWindow.xaml
    /// </summary>
    public partial class TestSubWindow : SubWindow
    {
        public TestSubWindow()
        {
            InitializeComponent();
        }

        ~TestSubWindow()
        {
            //this does not occur in the intended manor and should not be used for finding memory leaks
            //not for finding memory leaks in WPF windows, anyway
            //the GC fires this when it wants to, or when the GC collect lines are set AND the button is pressed again
        }
    }
}
