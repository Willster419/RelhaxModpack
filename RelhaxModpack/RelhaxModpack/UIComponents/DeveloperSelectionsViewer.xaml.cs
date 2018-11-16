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
using System.Net;
using System.IO;
using System.Xml;

namespace RelhaxModpack.Windows
{
    public class DevleoperSelectionsClosedEWventArgs : EventArgs
    {
        public bool LoadSelection = false;
        public string FileToLoad = "";
    }
    public delegate void DeveloperSelectionsClosedDelagate(object sender, DevleoperSelectionsClosedEWventArgs e);
    /// <summary>
    /// Interaction logic for DeveloperSelectionsViewer.xaml
    /// </summary>
    public partial class DeveloperSelectionsViewer : RelhaxWindow
    {
        //init it to false so that it only will get changed to true at that one point when it works
        private bool loadSelection = false;
        private string fileToLoad = "";
        public event DeveloperSelectionsClosedDelagate OnDeveloperSelectionsClosed;
        private WebClient client;
        private bool currentlyDownloading = false;
        private string databaseURL = "http://wotmods.relhaxmodpack.com/RelhaxModpack/Rescoruces/developerSelections/developerSelections.zip";
        private string databaseSaveLocation = Path.Combine(Settings.RelhaxTempFolder, "developerSelections.zip");

        public DeveloperSelectionsViewer()
        {
            InitializeComponent();
        }

        private void OnApplicationLoading(object sender, RoutedEventArgs e)
        {
            //add loading message to stack panel
            string forHeader = Translations.GetTranslatedString("loadingDevTranslations");
            //add it to the thing here
            currentlyDownloading = true;
            using (client = new WebClient())
            {
                client.DownloadFileCompleted += OnSelectionsDownloaded;
                try
                {
                    if (File.Exists(databaseSaveLocation))
                        File.Delete(databaseSaveLocation);
                    client.DownloadFileAsync(new Uri(databaseURL), databaseSaveLocation);
                }
                catch(Exception ex)
                {
                    Logging.WriteToLog(ex.ToString(), Logfiles.Application, LogLevel.Exception);
                    MessageBox.Show(Translations.GetTranslatedString("failedToDownload") + "DeveloperSelections.zip");
                    this.Close();
                }
            }
        }

        private void OnSelectionsDownloaded(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if(e.Error != null)
            {
                //quit now
                //TODO
                return;
            }
            string selectionsXMlString = Utils.GetStringFromZip(databaseSaveLocation,"selections.xml");
            if(string.IsNullOrWhiteSpace(selectionsXMlString))
            {
                Logging.WriteToLog("Failed to parse selections.xml from developerSelections.zip", Logfiles.Application, LogLevel.Error);
                MessageBox.Show(Translations.GetTranslatedString("failedToParse") + "DeveloperSelections.zip");
                return;
            }
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(selectionsXMlString);
            }
            catch (XmlException xmlex)
            {
                Logging.WriteToLog(xmlex.ToString(), Logfiles.Application, LogLevel.Exception);
                MessageBox.Show(Translations.GetTranslatedString("failedToDownload") + "DeveloperSelections.zip");
                return;
            }
        }

        private void OnApplicationClosed(object sender, EventArgs e)
        {
            if (OnDeveloperSelectionsClosed != null)
            {
                OnDeveloperSelectionsClosed(this, new DevleoperSelectionsClosedEWventArgs()
                {
                    LoadSelection = loadSelection,
                    FileToLoad = fileToLoad
                });
            }
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            this.loadSelection = true;
            //get filename of zip xml file to load
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.loadSelection = false;
            this.fileToLoad = string.Empty;
            this.Close();
        }

        private void LocalFile_Click(object sender, RoutedEventArgs e)
        {
            this.loadSelection = true;
            this.fileToLoad = "LOCAL";
            this.Close();
        }
    }
}
