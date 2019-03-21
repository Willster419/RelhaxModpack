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
using System.Xml;
using Microsoft.Win32;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for PatchTester.xaml
    /// </summary>
    public partial class PatchTester : RelhaxWindow
    {

        private PatchSettings PatchSettings;
        private XmlDocument LoadedPatchesDocument;
        private List<Patch> LoadedPatches;
        private Patch LoadedPatch;
        private OpenFileDialog OpenPatchfileDialog;
        private SaveFileDialog SavePatchfileDialog;
        private SaveFileDialog SelectAppPathDialog;
        private SaveFileDialog SelectVersionDirDialog;
        private bool UnsavedChanges = false;

        public PatchTester()
        {
            InitializeComponent();
        }

        private void RelhaxWindow_Closed(object sender, EventArgs e)
        {
            if (UnsavedChanges)
            {
                if (MessageBox.Show("You have unsaved changes, return to patcher?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    return;
            }
            if (!Logging.IsLogDisposed(Logfiles.Application))
            {
                Logging.WriteToLog("Saving patcher settings");
                if (Settings.SaveSettings(Settings.PatcherSettingsFilename, typeof(PatchSettings), null, PatchSettings))
                    Logging.WriteToLog("Patcher settings saved");
            }
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //load settings
            PatchSettings = new PatchSettings();
            Logging.Info("Loading patcher settings");
            if (!Settings.LoadSettings(Settings.PatcherSettingsFilename, typeof(PatchSettings), null, PatchSettings))
                Logging.Warning("Failed to load patcher settings, using defaults");
            else
                Logging.Info("Successfully loaded patcher settings");
            //load empty patch definition
            PatchesList.Items.Clear();
            LoadedPatch = new Patch() { };
            PatchesList.Items.Add(LoadedPatch);
        }

        #region Settings
        private void LoadSettingsToUI()
        {

        }

        private void AppMacroTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void VersionDirMacroTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void SaveOnSelectionChangeBheckbox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void SwitchToLogTabCheckbox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ApplyBehavior_Changed(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Other UI stuff

        private void PatchTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!PatchTypeCombobox.IsDropDownOpen)
                return;
        }

        private void DisplayPatch()
        {

        }

        private void ApplyPatchToMemory()
        {

        }
        #endregion

        #region Buttons
        private void LocateFileToPatchButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TestPatchButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ApplyChangesButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LoadPatchXmlButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SavePatchXmlButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemovePatchButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddPatchButton_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Drag drop code

        #endregion
    }
}
