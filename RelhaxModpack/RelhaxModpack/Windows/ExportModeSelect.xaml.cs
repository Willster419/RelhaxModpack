using RelhaxModpack.Common;
using RelhaxModpack.Database;
using RelhaxModpack.Settings;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Structs;
using RelhaxModpack.Xml;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for ExportModeSelect.xaml
    /// </summary>
    public partial class ExportModeSelect : RelhaxWindow
    {

        private List<VersionInfos> VersionInfosList = new List<VersionInfos>();

        /// <summary>
        /// The selection version info struct that was selected from the user selection
        /// </summary>
        public VersionInfos SelectedVersionInfo { get; private set; }

        /// <summary>
        /// Create an instance of the ExportModeSelect class
        /// </summary>
        public ExportModeSelect(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Logging.Debug("loading supported_clients from zip file");

            //parse each online folder to list type string
            VersionInfosList.Clear();
            string xmlString = FileUtils.GetStringFromZip(((App)Application.Current).ManagerInfoZipfile, ApplicationConstants.SupportedClients);
            XmlNodeList supportedClients = XmlUtils.GetXmlNodesFromXPath(xmlString, "//versions/version", ApplicationConstants.SupportedClients);
            VersionInfosList = new List<VersionInfos>();
            foreach (XmlNode node in supportedClients)
            {
                VersionInfos newVersionInfo = new VersionInfos()
                {
                    WoTOnlineFolderVersion = node.Attributes["folder"].Value,
                    WoTClientVersion = node.InnerText
                };
                VersionInfosList.Add(newVersionInfo);
            }

            //load them into the list with tag holding info
            ExportSelectVersionPanel.Children.Clear();
            foreach (VersionInfos versionInfo in VersionInfosList)
            {
                RadioButton button = new RadioButton()
                {
                    Tag = versionInfo,
                    Content = string.Format("{0} = {1}", Translations.GetTranslatedString("ExportModeMinorVersion"), versionInfo.WoTClientVersion)
                };
                ExportSelectVersionPanel.Children.Add(button);
            }

            //select the first one
            (ExportSelectVersionPanel.Children[0] as RadioButton).IsChecked = true;
        }

        private void ExportCancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ExportContinueButton_Click(object sender, RoutedEventArgs e)
        {
            //find the one that is selected
            foreach(RadioButton button in ExportSelectVersionPanel.Children)
            {
                if((bool)button.IsChecked)
                {
                    SelectedVersionInfo = (VersionInfos)button.Tag;
                    DialogResult = true;
                    Close();
                }
            }
        }
    }
}
