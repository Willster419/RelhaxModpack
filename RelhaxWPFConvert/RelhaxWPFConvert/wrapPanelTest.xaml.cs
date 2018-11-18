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

namespace RelhaxWPFConvert
{
    /// <summary>
    /// Interaction logic for wrapPanelTest.xaml
    /// </summary>
    public partial class wrapPanelTest : Window
    {
        public wrapPanelTest()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            wrapper.Children.Clear();

            for (int i = 0; i < 15; i++)
            {
                TabItem ti = new TabItem()
                {
                    Header = "TEST_TEST_" + i,
                    //HorizontalAlignment = HorizontalAlignment.Left,
                    //VerticalAlignment = VerticalAlignment.Center
                };
                if (i == 5)
                    ti.Header = "TEST_TEST_TEST_TEST_TEST_TEST_TEST_" + i;
                wrapper.Children.Add(ti);
            }
        }
    }
}
