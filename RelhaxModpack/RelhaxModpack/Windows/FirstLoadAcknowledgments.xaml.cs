using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using RelhaxModpack.Settings;

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
        /// Flag used to indicate if this is the first time the user is upgrading from Relhax V1 (old application) to Relhax V2.
        /// </summary>
        public bool FirstLoadToV2 { get; set; }

        /// <summary>
        /// Create an instance of the FirstLoadAcknowledgments window
        /// </summary>
        public FirstLoadAcknowledgments(ModpackSettings modpackSettings) : base(modpackSettings)
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
            if (FirstLoadToV2)
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
