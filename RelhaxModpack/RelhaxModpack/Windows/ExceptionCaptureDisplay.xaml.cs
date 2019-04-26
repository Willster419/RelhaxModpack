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
    /// Interaction logic for ExceptionCaptureDisplay.xaml
    /// </summary>
    public partial class ExceptionCaptureDisplay : RelhaxWindow
    {
        public ExceptionCaptureDisplay()
        {
            InitializeComponent();
        }

        public string ExceptionText
        {
            get { return ExceptionCaptureText.Text; }
            set { ExceptionCaptureText.Text = value; }
        }
    }
}
