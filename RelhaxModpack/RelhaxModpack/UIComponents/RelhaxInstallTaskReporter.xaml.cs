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
    /// Interaction logic for RelhaxInstallTaskReporter.xaml
    /// </summary>
    public partial class RelhaxInstallTaskReporter : UserControl
    {
        public RelhaxInstallTaskReporter()
        {
            InitializeComponent();
        }

        #region Properties
        public bool IsActive
        {
            get
            { return IsActive; }
            set
            { IsActive = value; }
        }
        #endregion

        #region Property changed code
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
