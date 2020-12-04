using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media;
using System.Text;
using RelhaxModpack.Xml;
using RelhaxModpack.Utilities;
using RelhaxModpack.Patches;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Utilities.ClassEventArgs;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for PatchDesigner.xaml
    /// </summary>
    public partial class PatchDesigner : RelhaxWindow
    {
        /// <summary>
        /// Indicates if this editor instance was launched from the MainWindow or from command line
        /// </summary>
        /// <remarks>This changes the behavior of the logging for the editor</remarks>
        public bool LaunchedFromMainWindow = false;

        private PatchSettings PatchSettings;
        private OpenFileDialog OpenPatchfileDialog;
        private OpenFileDialog OpenFileToPatchDialog;
        private SaveFileDialog SavePatchfileDialog;
        private bool UnsavedChanges = false;
        private bool init = true;

        //for drag drop
        private bool IsPatchListScrolling = false;
        private bool RegressionsRunning = false;
        private Point BeforeDragDropPoint;

        //for the pop out replace in case it's a lot to replace
        private PopOutReplacePatchDesigner popOutReplacePatchDesigner = null;
        private Brush FileToPatchBrush = null;
        private Brush PatchFilePathBrush = null;

        //internal selected patch
        private Patch SelectedPatch = null;

        //valid xml modes to put into mode combobox
        private readonly string[] validXmlModes = new string[]
        {
            "add",
            "edit",
            "remove"
        };

        //valid json modes to put into mode combobox
        private readonly string[] validJsonModes = new string[]
        {
            "add",
            "arrayadd",
            "remove",
            "arrayremove",
            "edit",
            "arrayedit",
            "arrayclear"
        };

        /// <summary>
        /// Create an instance of the PatchDesigner window
        /// </summary>
        public PatchDesigner()
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

            if (!Logging.IsLogDisposed(Logfiles.PatchDesigner))
                Logging.Patcher("Saving patcher settings",LogLevel.Info);
            
            if (Settings.SaveSettings(Settings.PatcherSettingsFilename, typeof(PatchSettings), null, PatchSettings))
                if (!Logging.IsLogDisposed(Logfiles.PatchDesigner))
                    Logging.Patcher("Patcher settings saved", LogLevel.Info);

            if (!Logging.IsLogDisposed(Logfiles.PatchDesigner))
                Logging.DisposeLogging(Logfiles.PatchDesigner);
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //load settings
            PatchSettings = new PatchSettings();
            Logging.Patcher("Loading patcher settings", LogLevel.Info);
            if (!Settings.LoadSettings(Settings.PatcherSettingsFilename, typeof(PatchSettings), null, PatchSettings))
                Logging.Patcher("Failed to load patcher settings, using defaults", LogLevel.Error);
            else
                Logging.Patcher("Successfully loaded patcher settings", LogLevel.Info);
            LoadSettingsToUI();

            //load empty patch definition
            PatchesList.Items.Clear();

            //attach the log output to the logfile
            Logging.GetLogfile(Logfiles.PatchDesigner).OnLogfileWrite += Logging_OnLoggingUIThreadReport;

            //save current brushes
            PatchFilePathBrush = PatchFilePathTextbox.Background;
            FileToPatchBrush = PatchFilePathTextbox.Background;

            //by default, set the locate file type to absolute (absolute)
            FilePathTypeCombobox.SelectedIndex = 1;

            if (!LaunchedFromMainWindow)
            {
                Task.Run(async () =>
                {
                    if (!await CommonUtils.IsManagerUptoDate(CommonUtils.GetApplicationVersion()))
                    {
                        MessageBox.Show("Your application is out of date. Please launch the application normally to update");
                    }
                });
            }

            //add one patch as default
            PatchesList.Items.Add(new Patch() { FromEditor = true });
            PatchesList.SelectedIndex = 0;
            SelectedPatch = PatchesList.Items[0] as Patch;

            init = false;
        }

        private void PopOutReplacePatchDesigner_Closed(object sender, EventArgs e)
        {
            PopOutReplaceBlockCB.IsChecked = false;
            PopOutReplaceBlockCB_Click(null, null);
        }

        #region Settings
        private void LoadSettingsToUI()
        {
            VersionDirMacroTextbox.Text = PatchSettings.VersiondirMacro;
            AppMacroTextbox.Text = PatchSettings.AppMacro;
            SaveOnSelectionChangeBheckbox.IsChecked = PatchSettings.SaveSelectionBeforeLeave;
            switch(PatchSettings.ApplyBehavior)
            {
                case ApplyBehavior.ApplyTriggersSave:
                    ApplyBehaviorApplyTriggersSave.IsChecked = true;
                    break;
                case ApplyBehavior.Default:
                    ApplyBehaviorDefault.IsChecked = true;
                    break;
                case ApplyBehavior.SaveTriggersApply:
                    ApplyBehaviorSaveTribbersApply.IsChecked = true;
                    break;
            }
            SwitchToLogTabCheckbox.IsChecked = PatchSettings.SwitchToLogWhenTestingPatch;
        }

        private void AppMacroTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PatchSettings.AppMacro = AppMacroTextbox.Text;
        }

        private void VersionDirMacroTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PatchSettings.VersiondirMacro = VersionDirMacroTextbox.Text;
        }

        private void SaveOnSelectionChangeBheckbox_Click(object sender, RoutedEventArgs e)
        {
            PatchSettings.SaveSelectionBeforeLeave = (bool)SaveOnSelectionChangeBheckbox.IsChecked;
        }

        private void SwitchToLogTabCheckbox_Click(object sender, RoutedEventArgs e)
        {
            PatchSettings.SwitchToLogWhenTestingPatch = (bool)SwitchToLogTabCheckbox.IsChecked;
        }

        private void ApplyBehavior_Changed(object sender, RoutedEventArgs e)
        {
            if ((bool)ApplyBehaviorDefault.IsChecked)
                PatchSettings.ApplyBehavior = ApplyBehavior.Default;
            else if ((bool)ApplyBehaviorApplyTriggersSave.IsChecked)
                PatchSettings.ApplyBehavior = ApplyBehavior.ApplyTriggersSave;
            else if ((bool)ApplyBehaviorSaveTribbersApply.IsChecked)
                PatchSettings.ApplyBehavior = ApplyBehavior.SaveTriggersApply;
        }
        #endregion

        #region Other UI stuff

        private void PatchTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PatchTypeCombobox.IsDropDownOpen)
            {
                //user selection

            }
            else
            {
                //program (internal) selection

            }
            if (PatchTypeCombobox.SelectedItem == null)
                return;
            //if the selection is json, enable the follow path selection box. else disable
            PatchModeCombobox.Items.Clear();
            if (PatchTypeCombobox.SelectedItem.Equals("json"))
            {
                PatchFollowPathSetting.IsEnabled = true;
                PatchModeCombobox.IsEnabled = true;
                PatchLinesPathHeader.Text = "Path";

                //also fill mode with json options
                foreach (string s in validJsonModes)
                {
                    PatchModeCombobox.Items.Add(s);
                }
            }
            else if (PatchTypeCombobox.SelectedItem.Equals("xml"))
            {
                PatchFollowPathSetting.IsEnabled = false;
                PatchModeCombobox.IsEnabled = true;
                PatchLinesPathHeader.Text = "Path";

                foreach (string s in validXmlModes)
                {
                    PatchModeCombobox.Items.Add(s);
                }
            }
            else//regex
            {
                PatchFollowPathSetting.IsEnabled = false;
                PatchModeCombobox.IsEnabled = false;
                PatchLinesPathHeader.Text = "Line(s)";
            }
        }

        private void PatchVersionCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //only patch versions 2+ will support the followPath option
            if(PatchVersionCombobox.SelectedIndex == 0)
            {
                PatchFollowPathSetting.IsChecked = false;
                PatchFollowPathSetting.IsEnabled = false;
            }
            else
            {
                PatchFollowPathSetting.IsEnabled = true;
            }
        }

        private void DisplayPatch(Patch patch)
        {
            //reset to nothing, then only set if the patch option is valid
            PatchFilePathTextbox.Clear();
            PatchPathCombobox.SelectedItem = null;
            PatchTypeCombobox.SelectedItem = null;
            PatchModeCombobox.SelectedItem = null;
            PatchFollowPathSetting.IsChecked = false;
            PatchLinesPathTextbox.Clear();
            PatchSearchTextbox.Clear();
            PatchReplaceTextbox.Clear();

            if(popOutReplacePatchDesigner != null)
                popOutReplacePatchDesigner.PatchReplaceTextbox.Clear();

            if (!string.IsNullOrWhiteSpace(patch.File))
                PatchFilePathTextbox.Text = patch.File;

            if (!string.IsNullOrWhiteSpace(patch.PatchPath))
                switch(patch.PatchPath.ToLower())
                {
                    default:
                        Logging.Patcher("Unknown patchPath: {0}", LogLevel.Error, patch.PatchPath);
                        break;
                    case @"{app}":
                    case "app":
                        PatchPathCombobox.SelectedIndex = 0;
                        break;
                    case @"{appdata}":
                    case "appdata":
                        PatchPathCombobox.SelectedIndex = 1;
                        break;
                }

            if (!string.IsNullOrWhiteSpace(patch.Type))
                PatchTypeCombobox.SelectedItem = patch.Type;

            if (!string.IsNullOrWhiteSpace(patch.Mode))
                PatchModeCombobox.SelectedItem = patch.Mode;

            //set the version. it's at least version 1
            PatchVersionCombobox.SelectedItem = patch.Version;

            //only set the followPath setting if the version is > 1
            //else it is set off by the selectedValueChanged event in PatchVersionCombobox
            if (patch.Version > 1)
            {
                PatchFollowPathSetting.IsChecked = patch.FollowPath;
            }
            else if (patch.FollowPath && (patch.Version == 1 || !patch.Type.Equals(Patch.TypeJson)))
            {
                Logging.Patcher("Patch option followPath can't be enabled (not supported). Disabling.", LogLevel.Error);
                Logging.Patcher("Version: {0}, Type: {1}", LogLevel.Error, patch.Version, patch.Type);
                patch.FollowPath = false;
            }

            if (patch.Type.Equals(Patch.TypeRegex1) || patch.Type.Equals(Patch.TypeRegex2))
            {
                PatchModeCombobox.IsEnabled = false;
                if (patch.Lines == null || patch.Lines.Count() == 0)
                    PatchLinesPathTextbox.Clear();
                else if (patch.Lines.Count() > 0)
                    PatchLinesPathTextbox.Text = string.Join(",", patch.Lines);
            }
            else
            {
                PatchModeCombobox.IsEnabled = true;
                if (!string.IsNullOrWhiteSpace(patch.Path))
                    PatchLinesPathTextbox.Text = patch.Path;
            }

            if (!string.IsNullOrWhiteSpace(patch.Search))
                PatchSearchTextbox.Text = patch.Search;

            if (!string.IsNullOrWhiteSpace(patch.Replace))
                PatchReplaceTextbox.Text = patch.Replace;
        }

        private void SaveApplyPatch(Patch patch)
        {
            //check to make sure at least valid settings are set before saving
            if(PatchPathCombobox.SelectedItem == null)
            {
                MessageBox.Show("invalid patch path selection");
                return;
            }
            else if (PatchTypeCombobox.SelectedItem == null)
            {
                MessageBox.Show("invalid patch type selection");
                return;
            }
            else if (PatchModeCombobox.SelectedItem == null)
            {
                MessageBox.Show("invalid patch mode selection");
                return;
            }

            UnsavedChanges = true;

            //save all UI settings to patch object
            patch.PatchPath = PatchPathCombobox.SelectedItem as string;
            patch.Type = PatchTypeCombobox.SelectedItem as string;
            patch.Mode = PatchModeCombobox.SelectedItem as string;
            patch.Version = (int)PatchVersionCombobox.SelectedItem;
            patch.FollowPath = (bool)PatchFollowPathSetting.IsChecked;
            patch.File = PatchFilePathTextbox.Text;

            if (patch.Type.ToLower().Equals("regex"))
            {
                patch.Lines = PatchLinesPathTextbox.Text.Split(',');
            }
            else
            {
                patch.Path = PatchLinesPathTextbox.Text;
            }

            patch.Search = PatchSearchTextbox.Text;
            patch.Replace = PatchReplaceTextbox.Text;

            if(PatchSettings.ApplyBehavior == ApplyBehavior.ApplyTriggersSave)
            {
                SavePatchXmlButton_Click(null, null);
            }

            PatchesList.Items.Refresh();
        }

        //scrolling constant to keep most recent log addition present on the screen
        private void LogOutput_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if(RegressionsRunning)
                LogOutput.ScrollToEnd();
        }

        private void PopOutReplaceBlockCB_Click(object sender, RoutedEventArgs e)
        {
            if((bool)PopOutReplaceBlockCB.IsChecked)
            {
                popOutReplacePatchDesigner = new PopOutReplacePatchDesigner();
                popOutReplacePatchDesigner.Closed += PopOutReplacePatchDesigner_Closed;
                popOutReplacePatchDesigner.Show();
                PatchReplaceTextbox.IsEnabled = false;
                popOutReplacePatchDesigner.PatchReplaceTextbox.Text = PatchReplaceTextbox.Text;
            }
            else
            {
                PatchReplaceTextbox.IsEnabled = true;
                PatchReplaceTextbox.Text = popOutReplacePatchDesigner.PatchReplaceTextbox.Text;
                popOutReplacePatchDesigner.Close();
                popOutReplacePatchDesigner = null;
            }
        }

        private void FilePathTypeCombobox_Selected(object sender, RoutedEventArgs e)
        {
            switch(FilePathTypeCombobox.SelectedIndex)
            {
                case 0://absolute
                    PatchFilePathTextbox.Background = Brushes.BlanchedAlmond;
                    FileToPatchTextbox.Background = FileToPatchBrush;
                    break;
                case 1://relative
                    FileToPatchTextbox.Background = Brushes.BlanchedAlmond;
                    PatchFilePathTextbox.Background = PatchFilePathBrush;
                    break;
            }
        }

        private void LocateFileToPatchButton_Click(object sender, RoutedEventArgs e)
        {
            if(OpenFileToPatchDialog == null)
            {
                OpenFileToPatchDialog = new OpenFileDialog()
                {
                    Title = "Select a file to test patches on",
                    AddExtension = true,
                    CheckPathExists = true,
                    Multiselect = false,
                    CheckFileExists = true,
                    Filter = "all files|*.*"
                };
            }
            if((bool)OpenFileToPatchDialog.ShowDialog())
            {
                FileToPatchTextbox.Text = OpenFileToPatchDialog.FileName;
            }
        }

        private void TestPatchButton_Click(object sender, RoutedEventArgs e)
        {
            //test from selection components
            if(PatchSettings.SwitchToLogWhenTestingPatch)
            {
                RightSideTabControl.SelectedItem = LogOutputTab;
            }
            Logging.Patcher("TestPatch() start", LogLevel.Info);
            Logging.Patcher("Checking UI elements for valid patch information...", LogLevel.Info);
            //make new patch element
            Patch patchToTest = new Patch();

            //check input from UI left panel side:
            //file location
            Logging.Patcher("File to Patch location mode: {0}", LogLevel.Info, FilePathTypeCombobox.SelectedItem ?? "(null)");
            if(FilePathTypeCombobox.SelectedItem == null)
            {
                Logging.Patcher("Invalid file path type", LogLevel.Info);
                return;
            }
            switch(FilePathTypeCombobox.SelectedItem.ToString())
            {
                case "Absolute":
                    Logging.Patcher("Checking if absolute file path {0} exists...", LogLevel.Info, FileToPatchTextbox.Text);
                    if(File.Exists(FileToPatchTextbox.Text))
                    {
                        Logging.Patcher("File Exists!", LogLevel.Info);
                        patchToTest.File = FileToPatchTextbox.Text;
                        patchToTest.CompletePath = FileToPatchTextbox.Text;
                    }
                    else
                    {
                        Logging.Patcher("File does not exist, aborting", LogLevel.Info);
                        return;
                    }
                    break;
                case "Relative":
                    Logging.Patcher("Using relative macro {0}", LogLevel.Info, PatchPathCombobox.SelectedItem ?? "(null)");
                    string completePathForPatchFile;
                    switch(PatchPathCombobox.SelectedItem.ToString())
                    {
                        case "app":
                            if (Directory.Exists(PatchSettings.AppMacro))
                            {
                                Logging.Patcher("app macro folder path exists...", LogLevel.Info);
                                if(FileToPatchTextbox.Text.Contains("versiondir") && string.IsNullOrWhiteSpace(PatchSettings.VersiondirMacro))
                                {
                                    Logging.Patcher("versiondir macro found, but versiondir patch setting macro is blank, aborting", LogLevel.Info);
                                    return;
                                }
                                completePathForPatchFile = PatchSettings.AppMacro + FileToPatchTextbox.Text;
                                completePathForPatchFile = MacroUtils.MacroReplace(completePathForPatchFile, ReplacementTypes.ZipFilePath);
                            }
                            else
                            {
                                Logging.Patcher("app macro folder path does not exist, aborting", LogLevel.Info);
                                return;
                            }
                            break;
                        case "appData":
                            if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)))
                            {
                                Logging.Patcher("appData macro folder path exists...", LogLevel.Info);
                                completePathForPatchFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + FileToPatchTextbox.Text;
                            }
                            else
                            {
                                Logging.Patcher("appData macro folder path does not exist, aborting", LogLevel.Info);
                                return;
                            }
                            break;
                        default:
                            Logging.Patcher("Invalid path macro", LogLevel.Info);
                            return;
                    }
                    Logging.Patcher("relative path built as {0}", LogLevel.Info, completePathForPatchFile);
                    if(File.Exists(completePathForPatchFile))
                    {
                        Logging.Patcher("File exists!", LogLevel.Info);
                        patchToTest.File = FileToPatchTextbox.Text;
                        patchToTest.CompletePath = completePathForPatchFile;
                    }
                    else
                    {
                        Logging.Patcher("File does not exist, aborting (did you forget to add \"\\\" to the beginning of the path?", LogLevel.Info);
                    }
                    break;
                default:
                    Logging.Patcher("Invalid file path type, aborting", LogLevel.Info);
                    return;
            }

            //check patch type
            if(PatchTypeCombobox.SelectedItem == null)
            {
                Logging.Patcher("Invalid Patch Type, aborting", LogLevel.Info);
                return;
            }
            patchToTest.Type = PatchTypeCombobox.SelectedItem as string;

            //check patch mode
            switch (patchToTest.Type.ToLower())
            {
                case "regex":
                case "regx":
                    if(!string.IsNullOrWhiteSpace(PatchModeCombobox.SelectedItem as string))
                    {
                        Logging.Patcher("Type=regex, invalid patch type: {0}", LogLevel.Error, PatchModeCombobox.SelectedItem as string);
                        Logging.Patcher("valid types are: (null)");
                        return;
                    }
                    //set the lines
                    if(string.IsNullOrWhiteSpace(PatchLinesPathTextbox.Text))
                    {
                        Logging.Patcher("Type=regex, Lines to patch is blank", LogLevel.Error);
                        return;
                    }
                    else
                    {
                        patchToTest.Lines = PatchLinesPathTextbox.Text.Split(',');
                    }
                    break;
                case "xml":
                    //check if path/lines is valid (has string values)
                    if (string.IsNullOrWhiteSpace(PatchLinesPathTextbox.Text))
                    {
                        Logging.Patcher("invalid xpath", LogLevel.Error);
                        return;
                    }
                    if (!validXmlModes.Contains((PatchModeCombobox.SelectedItem as string).ToLower()))
                    {
                        Logging.Patcher("Type=xml, invalid patch type: {0}", LogLevel.Error, PatchModeCombobox.SelectedItem as string);
                        Logging.Patcher("valid types are: {0}", LogLevel.Error, string.Join(",",validXmlModes));
                        return;
                    }
                    patchToTest.Path = PatchLinesPathTextbox.Text;
                    break;
                case "json":
                    //check if path/lines is valid (has string values)
                    if (string.IsNullOrWhiteSpace(PatchLinesPathTextbox.Text))
                    {
                        Logging.Patcher("invalid jsonpath");
                        return;
                    }
                    if (!validJsonModes.Contains((PatchModeCombobox.SelectedItem as string).ToLower()))
                    {
                        Logging.Patcher("Type=json, invalid patch type: {0}", LogLevel.Info, PatchModeCombobox.SelectedItem as string);
                        Logging.Patcher("valid types are: {0}", LogLevel.Info, string.Join(",", validJsonModes));
                        return;
                    }
                    patchToTest.Path = PatchLinesPathTextbox.Text;
                    break;
                default:
                    throw new BadMemeException("invalid patch type, but you should probably make this a enum not strings");
            }
            patchToTest.Mode = PatchModeCombobox.SelectedItem as string;
            //check followPath true ONLY for json
            if(!patchToTest.Type.Equals("json") && (bool)PatchFollowPathSetting.IsChecked)
            {
                Logging.Patcher("Types=json, followPathSetting must be false!");
                return;
            }

            //check search and replace
            if (string.IsNullOrWhiteSpace(PatchReplaceTextbox.Text) && string.IsNullOrWhiteSpace(PatchSearchTextbox.Text))
            {
                Logging.Patcher("patch replace and search are blank, invalid patch");
                return;
            }

            if (string.IsNullOrWhiteSpace(PatchSearchTextbox.Text))
            {
                Logging.Patcher("patch search is blank (is this the intent?)",LogLevel.Warning);
            }
            patchToTest.Search = PatchSearchTextbox.Text;

            if (string.IsNullOrWhiteSpace(PatchReplaceTextbox.Text))
            {
                Logging.Patcher("patch replace is blank (is this the intent?)", LogLevel.Info);
            }
            patchToTest.Replace = PatchReplaceTextbox.Text;

            //put patch into patch test methods
            //set patch from editor to true to enable verbose logging
            if(!patchToTest.FromEditor)
                patchToTest.FromEditor = true;
            Logging.Patcher("Running patch...", LogLevel.Info);
            switch (PatchUtils.RunPatch(patchToTest))
            {
                case PatchExitCode.Error:
                    Logging.Patcher("Patch failed with errors. Check the log for details.", LogLevel.Error);
                    break;
                case PatchExitCode.Warning:
                    Logging.Patcher("Patch completed with warnings. Check the log for details.", LogLevel.Warning);
                    break;
                case PatchExitCode.Success:
                    Logging.Patcher("Patch completed successfully!", LogLevel.Info);
                    break;
            }
        }

        private void ApplyChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (init)
                return;

            Logging.Patcher("[ApplyChangesButton_Click]: Applying changes to patch: {0}", LogLevel.Info, SelectedPatchToString());
            if (SelectedPatch != null)
            {
                SaveApplyPatch(SelectedPatch);
            }
        }

        private void PatchesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (init)
                return;

            Logging.Patcher("[PatchesList_SelectionChanged]: SaveBeforeLeave = {0}, SelectedPatch = {1}", LogLevel.Info,
                PatchSettings.SaveSelectionBeforeLeave.ToString(), SelectedPatchToString());

            if (PatchesList.SelectedItem as Patch != null)
            {
                //save "old"
                if (PatchSettings.SaveSelectionBeforeLeave && SelectedPatch != null)
                {
                    SaveApplyPatch(SelectedPatch);
                }

                //set "new" to old and display old
                SelectedPatch = PatchesList.SelectedItem as Patch;
                DisplayPatch(SelectedPatch);
            }
            else
                Logging.Patcher("[PatchesList_SelectionChanged]: (PatchesList.SelectedItem as Patch) is null, skipping apply");
        }

        private void LoadPatchXmlButton_Click(object sender, RoutedEventArgs e)
        {
            if (OpenPatchfileDialog == null)
            {
                OpenPatchfileDialog = new OpenFileDialog()
                {
                    Title = "Select a patch xml file to load",
                    AddExtension = true,
                    CheckPathExists = true,
                    CheckFileExists = true,
                    Multiselect = false,
                    DefaultExt = "xml",
                    Filter = "*.xml|*.xml"
                };
            }
            if((bool)OpenPatchfileDialog.ShowDialog())
            {
                PatchesList.Items.Clear();
                List<Patch> patches = new List<Patch>();
                XmlUtils.AddPatchesFromFile(patches, OpenPatchfileDialog.FileName, Path.GetFileName(OpenPatchfileDialog.FileName));
                if (patches == null || patches.Count == 0)
                {
                    MessageBox.Show("Failed to load xml document, check the logs for more info");
                    return;
                }
                foreach (Patch p in patches)
                    PatchesList.Items.Add(p);
                PatchesList.SelectedIndex = 0;
            }
        }

        private void SavePatchXmlButton_Click(object sender, RoutedEventArgs e)
        {
            //validate that the list has all valid patches
            List<Patch> invalidPatches = Patch.GetInvalidPatchesForSave(PatchesList.Items);
            Logging.Patcher("[SavePatchXmlButton_Click]: ", LogLevel.Info);

            if(invalidPatches.Count > 0)
            {
                StringBuilder invalidPatchesSB = new StringBuilder();
                string invalidHeader = "The following patches are in an invalid state and cannot be saved:";
                invalidPatchesSB.Append(invalidHeader);
                Logging.Patcher(invalidHeader);
                for (int i = 0; i < invalidPatches.Count; i++)
                {
                    string invalidPatch = string.Format("List Index = {0}, patch = [{1}]", PatchesList.Items.IndexOf(invalidPatches[i]), invalidPatches[i].ToString());
                    Logging.Patcher(invalidPatch);
                    invalidPatchesSB.Append("\n" + invalidPatch);
                }
                MessageBox.Show(invalidPatchesSB.ToString());
                return;
            }

            //create save dialog
            if (SavePatchfileDialog == null)
            {
                SavePatchfileDialog = new SaveFileDialog()
                {
                    Title = "Save xml patch",
                    AddExtension = true,
                    CheckPathExists = true,
                    DefaultExt = "xml",
                    Filter = "*.xml|*.xml"
                };
            }
            if((bool)SavePatchfileDialog.ShowDialog())
            {
                //if apply behavior is set to trigger apply first, then run apply
                if(PatchSettings.ApplyBehavior == ApplyBehavior.SaveTriggersApply && SelectedPatch != null)
                {
                    Logging.Patcher("[SavePatchXmlButton_Click]: ApplyBehavior = SaveTriggersApply, running ApplyPatch() first", LogLevel.Info);
                    SaveApplyPatch(SelectedPatch);
                }

                //save patches to XmlDocuemnt
                XmlDocument doc = XmlUtils.SavePatchToXmlDocument(PatchesList.Items.OfType<Patch>().ToList());
                
                //delete the file if it currently exists
                if (File.Exists(SavePatchfileDialog.FileName))
                    File.Delete(SavePatchfileDialog.FileName);

                doc.Save(SavePatchfileDialog.FileName);
                UnsavedChanges = false;
                Logging.Patcher("[SavePatchXmlButton_Click]: Patch saved", LogLevel.Info);
            }
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            LogOutput.Text = string.Empty;
        }

        private void RemovePatchButton_Click(object sender, RoutedEventArgs e)
        {
            if (PatchesList.SelectedItem == null)
            {
                Logging.Patcher("[RemovePatchButton_Click]: PatchesList.SelectedItem is null, skip", LogLevel.Info);
                return;
            }

            Logging.Patcher("[RemovePatchButton_Click]: Removing selected patch: {0}", LogLevel.Info, SelectedPatchToString());

            //remove item and if at least one, set it to the lowest
            SelectedPatch = null;
            int index = PatchesList.SelectedIndex;

            PatchesList.SelectionChanged -= PatchesList_SelectionChanged;
            PatchesList.Items.Remove(PatchesList.SelectedItem);
            PatchesList.SelectionChanged += PatchesList_SelectionChanged;

            if (PatchesList.Items.Count > index)
            {
                PatchesList.SelectedIndex = index;
            }
            else if (PatchesList.Items.Count == index)
            {
                PatchesList.SelectedIndex = PatchesList.Items.Count - 1;
            }
            else
            {
                PatchesList.SelectedIndex = -1;
            }
            SelectedPatch = PatchesList.SelectedItem as Patch;
        }

        private void AddPatchButton_Click(object sender, RoutedEventArgs e)
        {
            PatchesList.Items.Add(new Patch() { FromEditor = true });
        }
        #endregion

        #region Drag drop code
        private void PatchesList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if(Mouse.LeftButton == MouseButtonState.Pressed)
            {
                IsPatchListScrolling = true;
            }
        }

        private void PatchesList_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                IsPatchListScrolling = false;
            }
        }

        private void PatchesList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                BeforeDragDropPoint = e.GetPosition(PatchesList);
            }
        }

        private void PatchesList_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && IsDragConfirmed(e.GetPosition(PatchesList)) && !IsPatchListScrolling)
            {
                if (PatchesList.SelectedItem is Patch patch)
                {
                    DragDrop.DoDragDrop(PatchesList, patch, DragDropEffects.Move);
                }
            }
        }

        private void PatchesList_DragOver(object sender, DragEventArgs e)
        {

        }

        private void PatchesList_Drop(object sender, DragEventArgs e)
        {
            if(PatchesList.SelectedItem is Patch patchToMove)
            {
                if(e.OriginalSource is TextBlock block && block.DataContext is Patch patchOver)
                {
                    PatchesList.Items.Remove(patchToMove);
                    PatchesList.Items.Insert(PatchesList.Items.IndexOf(patchOver) + 1, patchToMove);
                }
            }
        }

        //https://stackoverflow.com/questions/19391135/prevent-drag-drop-when-scrolling
        private bool IsDragConfirmed(Point point)
        {
            bool horizontalMovement = Math.Abs(point.X - BeforeDragDropPoint.X) >
                 SystemParameters.MinimumHorizontalDragDistance;
            bool verticalMovement = Math.Abs(point.Y - BeforeDragDropPoint.Y) >
                 SystemParameters.MinimumVerticalDragDistance;
            return (horizontalMovement | verticalMovement);
        }

        #endregion

        #region Regression Testing
        private async void RegexRegressionTesting_Click(object sender, RoutedEventArgs e)
        {
            RegressionsRunning = true;
            if (PatchSettings.SwitchToLogWhenTestingPatch)
            {
                RightSideTabControl.SelectedItem = LogOutputTab;
            }
            Logging.Patcher("Regex regressions start");
            await Task.Run(() =>
            {
                Regression regression = new Regression(PatchRegressionTypes.regex, BuildRegexUnittests());
                regression.RunRegressions();
                regression.Dispose();
            });
            Logging.Patcher("Regex regressions end");
            Dispatcher.Invoke(new Action(() => { RegressionsRunning = false; }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private async void XmlRegressionTesting_Click(object sender, RoutedEventArgs e)
        {
            RegressionsRunning = true;
            if (PatchSettings.SwitchToLogWhenTestingPatch)
            {
                RightSideTabControl.SelectedItem = LogOutputTab;
            }
            Logging.Patcher("Xml regressions start");
            await Task.Run(() =>
            {
                Regression regression = new Regression(PatchRegressionTypes.xml, BuildXmlUnittests());
                regression.RunRegressions();
                regression.Dispose();
            });
            Logging.Patcher("Xml regressions end");
            Dispatcher.Invoke(new Action(() => { RegressionsRunning = false; }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private async void JsonRegressionTesting_Click(object sender, RoutedEventArgs e)
        {
            RegressionsRunning = true;
            if (PatchSettings.SwitchToLogWhenTestingPatch)
            {
                RightSideTabControl.SelectedItem = LogOutputTab;
            }
            Logging.Patcher("Json regressions start");
            await Task.Run(() =>
            {
                Regression regression = new Regression(PatchRegressionTypes.json, BuildJsonUnittests());
                regression.RunRegressions();
                regression.Dispose();
            });
            Logging.Patcher("Json regressions end");
            Dispatcher.Invoke(new Action(() => { RegressionsRunning = false; }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private async void FollowPathRegressionTesting_Click(object sender, RoutedEventArgs e)
        {
            RegressionsRunning = true;
            if (PatchSettings.SwitchToLogWhenTestingPatch)
            {
                RightSideTabControl.SelectedItem = LogOutputTab;
            }
            Logging.Patcher("FollowPath regressions start");
            await Task.Run(() =>
            {
                Regression regression = new Regression(PatchRegressionTypes.followPath, BuildFollowPathUnittests());
                regression.RunRegressions();
                regression.Dispose();
            });
            Logging.Patcher("FollowPath regressions end");
            Dispatcher.Invoke(new Action(() => { RegressionsRunning = false; }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private void Logging_OnLoggingUIThreadReport(object sender, LogMessageEventArgs e)
        {
            //https://www.tutorialspoint.com/csharp/csharp_delegates.htm
            //https://docs.microsoft.com/en-us/dotnet/api/system.windows.threading.dispatcher.invoke?redirectedfrom=MSDN&view=netframework-4.6.1#examples
            //https://stackoverflow.com/questions/1951927/events-in-c-sharp-definition-and-example
            //https://stackoverflow.com/questions/4936459/dispatcher-begininvoke-cannot-convert-lambda-to-system-delegate
            Dispatcher.BeginInvoke(new Action(() => { LogOutput.AppendText(e.Message + Environment.NewLine); }));
        }
        #endregion

        #region Json regressions
        private List<UnitTest> BuildJsonUnittests()
        {
            return new List<UnitTest>()
            {
                new UnitTest()
                {
                    Description = "add test 1: basic add",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$",
                        Search = "",
                        Replace = "awesome/false",
                        Mode = "add"
                    }
                },
                new UnitTest()
                {
                    Description = "add test 2: repeat of basic add. should do nothing",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$",
                        Search = "",
                        Replace = "awesome/false",
                        Mode = "add"
                    }
                },
                new UnitTest()
                {
                    Description = "add test 3: same path as basic add, but different value to insert. should update the value",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$",
                        Search = "",
                        Replace = "awesome/true",
                        Mode = "add"
                    }
                },
                new UnitTest()
                {
                    Description = "add test 4: add of a new object as well as the path. should create object paths to value",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$",
                        Search = "",
                        Replace = "memes/awesome/true",
                        Mode = "add"
                    }
                },
                new UnitTest()
                {
                    Description = "add test 5: add of a new property to part object path that already exists. should add the value without overwriting the path",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$",
                        Search = "",
                        Replace = "memes/dank/true",
                        Mode = "add"
                    }
                },
                new UnitTest()
                {
                    Description = "add test 6: add of a new blank array",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$",
                        Search = "",
                        Replace = "memelist[array]",
                        Mode = "add"
                    }
                },
                new UnitTest()
                {
                    Description = "add test 7: add of a new blank object",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$",
                        Search = "",
                        Replace = "objectname[object]",
                        Mode = "add"
                    }
                },
                new UnitTest()
                {
                    Description = "add test 8: add of new property with slash escape",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$",
                        Search = "",
                        Replace = "memeville/spaces[sl]hangar_premium_v2",
                        Mode = "add"
                    }
                },
                new UnitTest()
                {
                    Description = "edit test 1: edit attempt of path that does not exist. should note it log and abort",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.fakePath",
                        Search = "",
                        Replace = "null",
                        Mode = "edit"
                    }
                },
                new UnitTest()
                {
                    Description = "edit test 2: object edit attempt of array. should note in log and abort",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.nations",
                        Search = "",
                        Replace = "null",
                        Mode = "edit"
                    }
                },
                new UnitTest()
                {
                    Description = "edit test 3: edit attempt of simple path. should change the one value",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.mode",
                        Search = "normal",
                        Replace = "epic",
                        Mode = "edit"
                    }
                },
                new UnitTest()
                {
                    Description = "edit test 4: edit attempt of simple path. should change the one value (false, should report value entry same and exit)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.mode",
                        Search = "epic",
                        Replace = "epic",
                        Mode = "edit"
                    }
                },
                new UnitTest()
                {
                    Description = "array edit test 1: edit of array of values. should change the last value in the array",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.ignorelist[*]",
                        Search = "ttest",
                        Replace = "test",
                        Mode = "arrayEdit"
                    }
                },
                new UnitTest()
                {
                    Description = "array edit test 2: edit of array of objects. should parse every value of 421 or above to be 420 (regex style)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = @"$.screensavers.starttime[*]",
                        Search = @"^[4-9][2-9][0-9]\d*$",
                        Replace = "420",
                        Mode = "arrayEdit"
                    }
                },
                new UnitTest()
                {
                    Description = "array edit test 3: edit of array of objects. should parse every value of 419 or below to be 420 (regex style)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = @"$.screensavers.starttime[*]",
                        Search = @"^([0123]?[0-9]?[0-9]|4[01][0-9]|41[0-9])$",
                        Replace = "420",
                        Mode = "arrayEdit"
                    }
                },
                new UnitTest()
                {
                    Description = "array edit test 4: edit array of objects. should parse every value less than 420 to be 420 (jsonpath style)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = @"$.screensavers2.starttime[?(@<420)]",
                        Search = ".*",
                        Replace = "420",
                        Mode = "arrayEdit"
                    }
                },
                new UnitTest()
                {
                    Description = "array edit test 5: edit array of objects. should parse every value more than 420 to be 420 (jsonpath style)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = @"$.screensavers2.starttime[?(@>420)]",
                        Search = ".*",
                        Replace = "420",
                        Mode = "arrayEdit"
                    }
                },
                new UnitTest()
                {
                    Description = "remove test 1: basic remove test with property",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.game_greeting2",
                        Search = ".*",
                        Replace = "",
                        Mode = "remove"
                    }
                },
                new UnitTest()
                {
                    Description = "remove test 2: advanced remove test with property matching (should not remove as text not matched)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.game_greeting",
                        Search = "not match this text",
                        Replace = "",
                        Mode = "remove"
                    }
                },
                new UnitTest()
                {
                    Description = "remove test 3: advanced remove test with property matching (should remove, text matched)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.game_greeting",
                        Search = "match this text",
                        Replace = "",
                        Mode = "remove"
                    }
                },
                new UnitTest()
                {
                    Description = "arrayAdd test 1: basic add of jValue at index 0",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.ignorelist",
                        Search = ".*",
                        Replace = "spaces[sl]urmom[index=0]",
                        Mode = "arrayAdd"
                    }
                },
                new UnitTest()
                {
                    Description = "arrayAdd test 2: basic add of jValue at index -1 (last)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.ignorelist",
                        Search = ".*",
                        Replace = "spaces[sl]urmom2[index=-1]",
                        Mode = "arrayAdd"
                    }
                },
                new UnitTest()
                {
                    Description = "arrayAdd test 3: attempt add of object to array of JValue, should fail",
                    ShouldPass = false,
                    Patch = new Patch()
                    {
                        Path = "$.ignorelist",
                        Search = ".*",
                        Replace = "enable/true[index=0]",
                        Mode = "arrayAdd"
                    }
                },
                new UnitTest()
                {
                    Description = "arrayAdd test 4: attempt add of jValue to array of object, should fail",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.sample_object_array",
                        Search = ".*",
                        Replace = "spaces[sl]urmom[index=0]",
                        Mode = "arrayAdd"
                    }
                },
                new UnitTest()
                {
                    Description = "arrayAdd test 5: basic add of object",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.sample_object_array",
                        Search = ".*",
                        Replace = "enable/true[index=0]",
                        Mode = "arrayAdd"
                    }
                },
                new UnitTest()
                {
                    Description = "arrayRemove test 1: basic remove of jValue \"test\"",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.ignorelist",
                        Search = "test",
                        Replace = "",
                        Mode = "arrayRemove"
                    }
                },
                new UnitTest()
                {
                    Description = "arrayRemove test 2: basic remove of jValue \"test3\" (does not exist, should fail)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.ignorelist",
                        Search = "test3",
                        Replace = "",
                        Mode = "arrayRemove"
                    }
                },
                new UnitTest()
                {
                    Description = "arrayRemove test 3: basic remove of jObject \"enable:true\"",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.sample_object_array",
                        Search = ".*",
                        Replace = "",
                        Mode = "arrayRemove"
                    }
                },
                new UnitTest()
                {
                    Description = "arrayClear test 1: basic clear of jValue \"username\"",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.ignorelist",
                        Search = "username",
                        Replace = "",
                        Mode = "arrayClear"
                    }
                },
                new UnitTest()
                {
                    Description = "arrayClear test 2: basic clear of jValue \"username\" (does not exist)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.ignorelist",
                        Search = "username",
                        Replace = "",
                        Mode = "arrayClear"
                    }
                },
                new UnitTest()
                {
                    Description = "arrayClear test 3: basic clear of object \".*\" (all)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.screensavers.starttime",
                        Search = ".*",
                        Replace = "",
                        Mode = "arrayClear"
                    }
                }
            };
        }
        #endregion

        #region Xml regressions
        private List<UnitTest> BuildXmlUnittests()
        {
            return new List<UnitTest>()
            {
                new UnitTest()
                {
                    Description = "add test 1: adding element with value",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        //type set in regression test
                        Path = "//audio_mods.xml/loadBanks",
                        Search = "",
                        Replace = "bank/sound_bank_name",
                        Mode = "add"
                    }
                },
                new UnitTest()
                {
                    Description = "add test 2: adding element with levels (with value)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "//audio_mods.xml/events",
                        Search = "",
                        Replace = "event/name/vo_ally_killed_by_player",
                        Mode = "add"
                    }
                },
                new UnitTest()
                {
                    Description = "add test 3: adding in element where child inner text equals",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "/audio_mods.xml/events/event[name = \"vo_ally_killed_by_player\"]",
                        Search = "",
                        Replace = "mod/simple_sounds_teamkill",
                        Mode = "add"
                    }
                },
                new UnitTest()
                {
                    Description = "add test 4: adding element with escape for slash",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "//audio_mods.xml/RTPCs",
                        Search = "",
                        Replace = "RTPC/RTPC[sl]volume_slider_name",
                        Mode = "add"
                    }
                },
                new UnitTest()
                {
                    Description = "edit test 1: edit of a value matching parameter to new value",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "//audio_mods.xml/random_property",
                        Search = "value",
                        Replace = "better_value",
                        Mode = "edit"
                    }
                },
                new UnitTest()
                {
                    Description = "edit test 2: edit of a value matching parameter to a new value (but match does not exist)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "//audio_mods.xml/random_property",
                        Search = "fake_value",
                        Replace = "more_fake_value",
                        Mode = "edit"
                    }
                },
                new UnitTest()
                {
                    Description = "edit test 3: edit of matching any value to a new value",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "//audio_mods.xml/random_property2",
                        Search = "",
                        Replace = "new_value",
                        Mode = "edit"
                    }
                },
                new UnitTest()
                {
                    Description = "remove test 1: remove matching element name",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "//audio_mods.xml/prop_to_remove",
                        Search = "",
                        Replace = "",
                        Mode = "remove"
                    }
                },
                new UnitTest()
                {
                    Description = "remove test 2: remove matching element name and value",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "//audio_mods.xml/prop_to_remove2",
                        Search = "remove_me",
                        Replace = "",
                        Mode = "remove"
                    }
                }
            };
        }
        #endregion

        #region Regex regressions
        private List<UnitTest> BuildRegexUnittests()
        {
            return new List<UnitTest>()
            {
                new UnitTest()
                {
                    Description = "multiple matches, only replaces on specified lines",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Lines = new string[] { "3", "5" },
                        Search = "should match",
                        Replace = "replaced",
                        Type = "regex"
                    }
                },
                new UnitTest()
                {
                    Description = "multiple matches, replaces all lines",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Lines = new string[] { },
                        Search = "should match",
                        Replace = "replaced",
                        Type = "regex"
                    }
                }
            };
        }
        #endregion

        #region FollowPath regressions
        private List<UnitTest> BuildFollowPathUnittests()
        {
            return new List<UnitTest>()
            {
                new UnitTest()
                {
                    Description = "disable damageLog, follow path @xvm.xc->check_01.xc, includes \"$ref\" inside file",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        FollowPath = true,
                        Path = @"$.damageLog.enabled",
                        Search = ".*",
                        Replace = "false",
                        Type = "json",
                        Mode = "edit"
                    }
                },
                new UnitTest()
                {
                    Description = "update hitlogHeader's updateEvent property to be on dank memes. followPath @xvm.xc->battleLabels.xc->battleLabelsTemplates.xc",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        FollowPath = true,
                        //@"$.screensavers2.starttime[?(@<420)]"
                        //https://support.smartbear.com/alertsite/docs/monitors/api/endpoint/jsonpath.html#filters
                        Path = @"$.battleLabels.formats[?(@ =~ /hitLogHeader/)].updateEvent",
                        Search = ".*",
                        Replace = "PY(ON_DANK_MEMES)",
                        Type = "json",
                        Mode = "edit"
                    }
                },
                new UnitTest()
                {
                    Description = "remove array reference entry of fire",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        FollowPath = true,
                        Path = @"$.battleLabels.formats",
                        Search = "fire",
                        Replace = "",
                        Type = "json",
                        Mode = "arrayRemove"
                    }
                },
                new UnitTest()
                {
                    Description = "part 1 of 4: add a object to playersPanel definition-> change link in root file from 'playersPanel' to 'def'",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        FollowPath = true,
                        Path = @"$.playersPanel",
                        Search = ".*",
                        Replace = @"[xvm_dollar][lbracket][quote]playersPanel.xc[quote][colon][quote]def[quote][xvm_rbracket]",
                        Type = "json",
                        Mode = "edit"
                    }
                },
                new UnitTest()
                {
                    Description = "part 2 of 4: add a object to playersPanel definition-> change link in root file from 'playersPanel' to 'def'",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        FollowPath = true,
                        Path = @"$.playersPanel",
                        Search = ".*",
                        Replace = @"newDef[object]",
                        Type = "json",
                        Mode = "add"
                    }
                },
                new UnitTest()
                {
                    Description = "part 3 of 4: add a object to playersPanel definition-> change link in root file from 'playersPanel' to 'def'",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        FollowPath = true,
                        Path = @"$.playersPanel.newDef",
                        Search = ".*",
                        Replace = @"isThisTheBestXvmParserEver/true",
                        Type = "json",
                        Mode = "add"
                    }
                },
                new UnitTest()
                {
                    Description = "part 4 of 4: add a object to playersPanel definition-> change link in root file back to 'def' to 'playersPanel'",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        FollowPath = true,
                        Path = @"$.playersPanel",
                        Search = ".*",
                        Replace = @"[xvm_dollar][lbracket][quote]playersPanel.xc[quote][colon][quote]playersPanel[quote][xvm_rbracket]",
                        Type = "json",
                        Mode = "edit"
                    }
                }
            };
        }
        #endregion

        private string SelectedPatchToString()
        {
            return SelectedPatch == null ? "(null)" : SelectedPatch.ToString();
        }
    }
}
