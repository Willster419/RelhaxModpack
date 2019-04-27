using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
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
    /// Interaction logic for FirstLoadAknowledgements.xaml
    /// </summary>
    public partial class FirstLoadAknowledgements : RelhaxWindow
    {
        public FirstLoadAknowledgements() => InitializeComponent();

        public bool UserAgreed { get; set; } = false;

        private void Link_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
            //Link clicking event that opens the browser
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void CheckForContinueButton(object sender, RoutedEventArgs e)
        {
            UserAgreed = continue_Button.IsEnabled = (bool)license_Agree.IsChecked && (bool)collect_stats.IsChecked && (bool)mod_hoster.IsChecked && (bool)support.IsChecked;
        }

        private void Continue_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
