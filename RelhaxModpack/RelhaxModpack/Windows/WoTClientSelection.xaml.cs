using RelhaxModpack.Settings;
using RelhaxModpack.UI;
using RelhaxModpack.Utilities;
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
using Microsoft.Win32;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Common;
using System.IO;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for WoTClientSelection.xaml
    /// </summary>
    public partial class WoTClientSelection : RelhaxWindow
    {
        /// <summary>
        /// The selected wot client path to use. A null value means none was selected. It will include the exe.
        /// </summary>
        public string SelectedPath = null;

        /// <summary>
        /// Create an instance of the WoTClientSelection Window
        /// </summary>
        public WoTClientSelection(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //get client paths
            List<string> clientPaths = RegistryUtils.AutoFindWoTDirectoryList();

            //load selections into stackpanel
            bool firstOne = true;
            foreach (string path in clientPaths)
            {
                RadioButton selectionButton = new RadioButton()
                {
                    Content = path,
                    Tag = path,
                    IsChecked = firstOne
                };
                ClientSelectionsStackPanel.Children.Add(selectionButton);
                firstOne = false;
            }
        }

        private void ClientSelectionsManualFind_Click(object sender, RoutedEventArgs e)
        {
            //show a standard WoT selection window from manual find WoT.exe
            OpenFileDialog manualWoTFind = new OpenFileDialog()
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "WorldOfTanks.exe|WorldOfTanks.exe",
                Title = Translations.GetTranslatedString("selectWOTExecutable"),
                Multiselect = false,
                ValidateNames = true
            };
            if ((bool)manualWoTFind.ShowDialog())
            {
                SelectedPath = manualWoTFind.FileName;
                SelectedPath = SelectedPath.Replace(ApplicationConstants.WoT32bitFolderWithSlash, string.Empty).Replace(ApplicationConstants.WoT64bitFolderWithSlash, string.Empty);
                Logging.Info(LogOptions.ClassName, "Selected WoT install manually: {0}", SelectedPath);
                DialogResult = true;
                Close();
            }
            else
            {
                Logging.Info(LogOptions.ClassName, "User canceled manual selection");
            }
        }

        private void ClientSelectionsCancelButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedPath = null;
            DialogResult = false;
            Close();
        }

        private void ClientSelectionsContinueButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (RadioButton button in ClientSelectionsStackPanel.Children)
            {
                if ((bool)button.IsChecked)
                {
                    SelectedPath = (string)button.Tag;
                    break;
                }
            }
            Logging.Info(LogOptions.ClassName, "Selected WoT install from list: {0}", SelectedPath);
            DialogResult = true;
            Close();
        }
    }
}
