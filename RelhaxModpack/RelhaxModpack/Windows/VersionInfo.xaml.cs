using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Net;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for VersionInfo.xaml
    /// </summary>
    public partial class VersionInfo : RelhaxWindow
    {
        /// <summary>
        /// Gets if the user selected to accept the update
        /// </summary>
        public bool ConfirmUpdate { get; private set; } = false;

        /// <summary>
        /// Create an instance of the VersionInfo class
        /// </summary>
        public VersionInfo()
        {
            InitializeComponent();
        }

        private void OnManualUpdatelinkClick(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            //https://stackoverflow.com/questions/10238694/example-using-hyperlink-in-wpf
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void OnYesButtonClick(object sender, RoutedEventArgs e)
        {
            ConfirmUpdate = true;
            Close();
        }

        private void OnNoButtonClick(object sender, RoutedEventArgs e)
        {
            ConfirmUpdate = false;
            Close();
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1)
            {
                Logging.Debug("[VersionInfo]: Windows 7 detected, enabling TLS 1.1 and 1.2");
                System.Net.ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Ssl3 |
                    SecurityProtocolType.Tls |
                    SecurityProtocolType.Tls11 |
                    SecurityProtocolType.Tls12;
            }

            //update the text box with the latest version
            ApplicationUpdateNotes.Text = Translations.GetTranslatedString("loadingApplicationUpdateNotes");
            ViewUpdateNotesOnGoogleTranslate.TheHyperlink.Click += TheHyperlink_Click;
            using (WebClient client = new WebClient())
            {
                Uri temp = new Uri((ModpackSettings.ApplicationDistroVersion == ApplicationVersions.Stable) ?
                    Settings.ApplicationNotesStableUrl : Settings.ApplicationNotesBetaUrl);
                client.DownloadStringCompleted += (senderr, args) =>
                {
                    if(args.Error != null)
                    {
                        Logging.Exception("Failed to get update notes");
                        Logging.Exception(args.Error.ToString());
                        ApplicationUpdateNotes.Text = Translations.GetTranslatedString("failedToGetUpdateNotes");
                    }
                    else
                        ApplicationUpdateNotes.Text = args.Result;
                };
                client.DownloadStringAsync(temp);
            }
        }

        private void TheHyperlink_Click(object sender, RoutedEventArgs e)
        {
            CommonUtils.OpenInGoogleTranslate(ApplicationUpdateNotes.Text);
        }
    }
}
