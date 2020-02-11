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

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for RelhaxMessageWindow.xaml
    /// </summary>
    public partial class RelhaxMessageWindow : RelhaxWindow
    {
        /// <summary>
        /// Get or set the text of the window header (TextBlock)
        /// </summary>
        public string HeaderText
        {
            get { return MessageWindowHeader.Text; }
            set { MessageWindowHeader.Text = value; }
        }

        /// <summary>
        /// Get of set the text of the window body (TextBox)
        /// </summary>
        public string BodyText
        {
            get { return MessageWindowBody.Text; }
            set { MessageWindowBody.Text = value; }
        }

        /// <summary>
        /// Create an instance of the RelhaxMessageWindow class
        /// </summary>
        public RelhaxMessageWindow()
        {
            InitializeComponent();
        }

        private void RelhaxMessageWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
