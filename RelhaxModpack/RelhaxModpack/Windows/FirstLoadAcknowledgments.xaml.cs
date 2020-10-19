using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for FirstLoadAcknowledgments.xaml
    /// </summary>
    public partial class FirstLoadAcknowledgments : RelhaxWindow
    {
        /// <summary>
        /// Gets and sets is the user has agreed to the Acknowledgments before allowing him/her to use the application
        /// </summary>
        public bool UserAgreed { get; private set; } = false;

        /// <summary>
        /// Create an instance of the FirstLoadAcknowledgments window
        /// </summary>
        public FirstLoadAcknowledgments()
        {
            InitializeComponent();
        }

        private void Link_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
            //Link clicking event that opens the browser
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.FirstLoadToV2)
                V2UpgradeNoticeText.Visibility = Visibility.Visible;
        }

        private void CheckForContinueButton(object sender, RoutedEventArgs e)
        {
            UserAgreed = ContinueButton.IsEnabled = (bool)license_Agree.IsChecked && (bool)collect_stats.IsChecked && (bool)mod_hoster.IsChecked && (bool)support.IsChecked;
        }

        private void Continue_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
