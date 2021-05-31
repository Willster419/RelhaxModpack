using RelhaxModpack.Common;
using RelhaxModpack.Settings;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
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

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for NewsViewer.xaml
    /// </summary>
    public partial class NewsViewer : RelhaxWindow
    {
        /// <summary>
        /// Create an instance of the NewsViewer window
        /// </summary>
        public NewsViewer(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
        }

        private async void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //write that we're currently loading
            string loadingString = Translations.GetTranslatedString("loading");
            DatabaseUpdateText.Text = ApplicationUpdateText.Text = loadingString;
            ViewNewsOnGoogleTranslate.TheHyperlink.Click += TheHyperlink_Click;

            //get the strings
            using (PatientWebClient client = new PatientWebClient())
            {
                try
                {
                    DatabaseUpdateText.Text = await client.DownloadStringTaskAsync(ApplicationConstants.DatabaseNotesUrl);
                    ApplicationUpdateText.Text = await client.DownloadStringTaskAsync(ApplicationConstants.ApplicationVersion == ApplicationVersions.Stable?
                        ApplicationConstants.ApplicationNotesStableUrl : ApplicationConstants.ApplicationNotesBetaUrl);
                }
                catch (Exception ex)
                {
                    Logging.Error("Failed to get news information");
                    Logging.Exception(ex.ToString());

                    if (DatabaseUpdateText.Text.Equals(loadingString))
                        DatabaseUpdateText.Text = Translations.GetTranslatedString("failedToGetNews");

                    ApplicationUpdateText.Text = Translations.GetTranslatedString("failedToGetNews");
                }
            }
        }

        private void TheHyperlink_Click(object sender, RoutedEventArgs e)
        {
            CommonUtils.OpenInGoogleTranslate(database_Update_Tab.IsSelected ? DatabaseUpdateText.Text : ApplicationUpdateText.Text);
        }
    }
}
