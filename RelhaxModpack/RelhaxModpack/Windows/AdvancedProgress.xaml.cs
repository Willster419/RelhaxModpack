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
using RelhaxModpack.InstallerComponents;
using RelhaxModpack.UIComponents;

namespace RelhaxModpack.Windows
{   ///I exist as a branch
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class AdvancedProgress : Window
    {
        public AdvancedProgress()
        {
            InitializeComponent();
        }

        public void OnReportAdvancedProgress(RelhaxInstallerProgress progress)
        {
            //sample of what you could do...
            switch(progress.InstallStatus)
            {

            }
        }

        private void Advanced_Installer_Categories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
