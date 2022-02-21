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
using RelhaxModpack.Common;
using RelhaxModpack.Settings;

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
        public VersionInfo(ModpackSettings modpackSettings) : base(modpackSettings)
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

        /// <summary>
        /// Method that occurs when the key up event is fired. Sets the update confirm to false and runs the base method.
        /// </summary>
        /// <param name="sender">The object that sent the request.</param>
        /// <param name="e">The key event args to go with the event.</param>
        protected override void OnKeyUp(object sender, KeyEventArgs e)
        {
            ConfirmUpdate = false;
            base.OnKeyUp(sender, e);
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CommonUtils.CheckAndEnableTLS();

            //update the text box with the latest version
            ApplicationUpdateNotes.Text = Translations.GetTranslatedString("loadingApplicationUpdateNotes");
            ViewUpdateNotesOnGoogleTranslate.TheHyperlink.Click += TheHyperlink_Click;
            using (WebClient client = new WebClient())
            {
                Uri temp = new Uri((ModpackSettings.ApplicationDistroVersion == ApplicationVersions.Stable) ?
                    ApplicationConstants.ApplicationNotesStableUrl : ApplicationConstants.ApplicationNotesBetaUrl);
                client.DownloadStringCompleted += (senderr, args) =>
                {
                    if(args.Error != null)
                    {
                        Logging.Exception("Failed to get update notes");
                        Logging.Exception(args.Error.ToString());
                        ApplicationUpdateNotes.Text = Translations.GetTranslatedString("failedToLoadUpdateNotes");
                    }
                    else
                        ApplicationUpdateNotes.Text = args.Result;
                };
                client.DownloadStringAsync(temp);
            }

            VersionInfoYesText.Text = SetFirstCharToUpper(VersionInfoYesText.Text);
            VersionInfoNoText.Text = SetFirstCharToUpper(VersionInfoNoText.Text);
        }

        private string SetFirstCharToUpper(string stringToSetUpper)
        {
            return char.ToUpper(stringToSetUpper[0]) + stringToSetUpper.Substring(1);
        }

        private void TheHyperlink_Click(object sender, RoutedEventArgs e)
        {
            CommonUtils.OpenInGoogleTranslate(ApplicationUpdateNotes.Text);
        }
    }
}
