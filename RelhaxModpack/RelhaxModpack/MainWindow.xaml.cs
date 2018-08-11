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
using System.Windows.Forms;

namespace RelhaxModpack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Creates the instance of the MainWindow class
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TheMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //create the notify icon
            NotifyIcon relhaxIcon = new NotifyIcon()
            {
                Visible = true,
                Icon = Properties.Resources.modpack_icon,
                Text = Title
            };
        }
    }
}
