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
        private bool LoadSelection = false;
        private string FileToLoad = "";
        public event DeveloperSelectionsClosedDelagate OnDeveloperSelectionsClosed;
        private WebClient client;
        

        public DeveloperSelectionsViewer()
        {
            InitializeComponent();
        }

        private async void OnApplicationLoading(object sender, RoutedEventArgs e)
        {
            //add loading message to stack panel
            DeveloperSelectionsTextHeader.Text = Translations.GetTranslatedString("loadingDevTranslations");
            ContinueButton.IsEnabled = false;
            string selectionsXMlString = string.Empty;
            using (client = new WebClient())
            {
                try
                {
                    selectionsXMlString = await client.DownloadStringTaskAsync(Settings.SelectionsRoot + Settings.SelectionsXml);
                }
                catch(Exception ex)
                {
                    Logging.Exception(ex.ToString());
                    MessageBox.Show(Translations.GetTranslatedString("failedToParseSelections"));
                    Close();
                }
            }
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(selectionsXMlString);
            }
            catch (XmlException xmlex)
            {
                Logging.Exception(xmlex.ToString());
                MessageBox.Show(Translations.GetTranslatedString("failedToParseSelections"));
                return;
            }
            //set text to say "pick a selection" instead of "loading selections"
            DeveloperSelectionsTextHeader.Text = Translations.GetTranslatedString("SelectSelection");
            //load selections into stackpanel
            foreach (XmlNode node in XMLUtils.GetXMLNodesFromXPath(doc, "//selections/selection"))
            {
                DeveloperSelectionsStackPanel.Children.Add(new RadioButton()
                {
                    Content = node.Attributes["displayName"],
                    ToolTip = Translations.GetTranslatedString("lastModified") + " " + node.Attributes["lastModified"],
                    Tag = node.InnerText
                });
            }
            //enable the button to select them
            ContinueButton.IsEnabled = true;
        }

        private void OnApplicationClosed(object sender, EventArgs e)
        {
            if (OnDeveloperSelectionsClosed != null)
            {
                if(!FileToLoad.Equals("LOCAL"))
                    LoadSelection = false;
                foreach(RadioButton button in DeveloperSelectionsStackPanel.Children)
                {
                    if((bool)button.IsChecked)
                    {
                        LoadSelection = true;
                        FileToLoad = (string)button.Tag;
                        break;
                    }
                }
                OnDeveloperSelectionsClosed(this, new DevleoperSelectionsClosedEWventArgs()
                {
                    LoadSelection = LoadSelection,
                    FileToLoad = FileToLoad
                });
            }
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSelection = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSelection = false;
            FileToLoad = string.Empty;
            Close();
        }

        private void LocalFile_Click(object sender, RoutedEventArgs e)
        {
            LoadSelection = true;
            FileToLoad = "LOCAL";
            Close();
        }
    }
}
