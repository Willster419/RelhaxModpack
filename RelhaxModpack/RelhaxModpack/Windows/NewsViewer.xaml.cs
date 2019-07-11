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
    /// Interaction logic for NewsViewer.xaml
    /// </summary>
    public partial class NewsViewer : RelhaxWindow
    {
        public NewsViewer()
        {
            InitializeComponent();
        }

        private async void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //write that we're currently loading
            DatabaseUpdateText.Text = ApplicationUpdateText.Text = Translations.GetTranslatedString("loading");

            //get the strings
            using (PatientWebClient client = new PatientWebClient())
            {
                if(ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Stable)
                    DatabaseUpdateText.Text = await Task.Run(() => { return Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "databaseUpdate.txt"); });
                else
                    DatabaseUpdateText.Text = await client.DownloadStringTaskAsync(Settings.DatabaseNotesUrl);

                if (ModpackSettings.ApplicationDistroVersion == ApplicationVersions.Stable)
                    ApplicationUpdateText.Text = await Task.Run(() => { return Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "releaseNotes.txt"); });
                else
                    ApplicationUpdateText.Text = await client.DownloadStringTaskAsync(Settings.ApplicationNotesBetaUrl);
            }
        }
    }
}
