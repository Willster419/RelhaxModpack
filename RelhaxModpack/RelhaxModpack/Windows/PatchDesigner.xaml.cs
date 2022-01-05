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
using RelhaxModpack.Patching;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Utilities.ClassEventArgs;
using RelhaxModpack.Settings;
using RelhaxModpack.Installer;
using System.Reflection;
using System.ComponentModel;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for PatchDesigner.xaml
    /// </summary>
    public partial class PatchDesigner : RelhaxFeatureWindowWithChanges
    {
        /// <summary>
        /// The command line argument specified at application launch to show this window
        /// </summary>
        public const string CommandLineArg = "patch-designer";

        /// <summary>
        /// The name of the logfile
        /// </summary>
        public const string LoggingFilename = "RelhaxPatchDesigner.log";

        private PatchSettings PatchSettings = new PatchSettings();
        private OpenFileDialog OpenPatchfileDialog;
        private OpenFileDialog OpenFileToPatchDialog;
        private SaveFileDialog SavePatchfileDialog;
        private Patcher Patcher = new Patcher() { DebugMode = false };

        //for drag drop
        private bool IsPatchListScrolling = false;
        private Point BeforeDragDropPoint;

        //for the pop out replace in case it's a lot to replace
        private PopOutReplacePatchDesigner popOutReplacePatchDesigner = null;
        private Brush FileToPatchBrush = null;
        private Brush PatchFilePathBrush = null;

        //internal selected patch
        private Patch SelectedPatch = null;

        /// <summary>
        /// Create an instance of the PatchDesigner window
        /// </summary>
        public PatchDesigner(ModpackSettings modpackSettings, Logfiles logfile) : base(modpackSettings, logfile)
        {
            InitializeComponent();
            Settings = PatchSettings;
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettingsToUI();

            //load empty patch definition
            PatchesList.Items.Clear();

            //save current brushes
            PatchFilePathBrush = PatchFilePathTextbox.Background;
            FileToPatchBrush = PatchFilePathTextbox.Background;

            //by default, set the locate file type to absolute (absolute)
            FilePathTypeCombobox.SelectedIndex = 1;

            if (!LaunchedFromMainWindow)
            {
                Task.Run(async () =>
                {
                    if (!await CommonUtils.IsManagerUptoDate(CommonUtils.GetApplicationVersion(), ModpackSettings.ApplicationDistroVersion))
                    {
                        MessageBox.Show("Your application is out of date. Please launch the application normally to update");
                    }
                });
            }

            //add one patch as default
            PatchesList.Items.Add(new Patch());
            PatchesList.SelectedIndex = 0;
            SelectedPatch = PatchesList.Items[0] as Patch;

            Init = false;
        }

        private void PopOutReplacePatchDesigner_Closed(object sender, EventArgs e)
        {
            PopOutReplaceBlockCB.IsChecked = false;
            PopOutReplaceBlockCB_Click(null, null);
        }

        private string SelectedPatchToString()
        {
            return SelectedPatch == null ? "(null)" : SelectedPatch.ToString();
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
                foreach (string s in Patch.ValidJsonModes)
                {
                    PatchModeCombobox.Items.Add(s);
                }
            }
            else if (PatchTypeCombobox.SelectedItem.Equals("xml"))
            {
                PatchFollowPathSetting.IsEnabled = false;
                PatchModeCombobox.IsEnabled = true;
                PatchLinesPathHeader.Text = "Path";

                foreach (string s in Patch.ValidXmlModes)
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
                patch.Line = PatchLinesPathTextbox.Text;
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

        private void PopOutReplaceBlockCB_Click(object sender, RoutedEventArgs e)
        {
            if((bool)PopOutReplaceBlockCB.IsChecked)
            {
                popOutReplacePatchDesigner = new PopOutReplacePatchDesigner(this.ModpackSettings);
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
                        patchToTest.Line = PatchLinesPathTextbox.Text;
                    }
                    break;
                case "xml":
                    //check if path/lines is valid (has string values)
                    if (string.IsNullOrWhiteSpace(PatchLinesPathTextbox.Text))
                    {
                        Logging.Patcher("invalid xpath", LogLevel.Error);
                        return;
                    }
                    if (!Patch.ValidXmlModes.Contains((PatchModeCombobox.SelectedItem as string).ToLower()))
                    {
                        Logging.Patcher("Type=xml, invalid patch type: {0}", LogLevel.Error, PatchModeCombobox.SelectedItem as string);
                        Logging.Patcher("valid types are: {0}", LogLevel.Error, string.Join(",", Patch.ValidXmlModes));
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
                    if (!Patch.ValidJsonModes.Contains((PatchModeCombobox.SelectedItem as string).ToLower()))
                    {
                        Logging.Patcher("Type=json, invalid patch type: {0}", LogLevel.Info, PatchModeCombobox.SelectedItem as string);
                        Logging.Patcher("valid types are: {0}", LogLevel.Info, string.Join(",", Patch.ValidJsonModes));
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
            Logging.Patcher("Running patch...", LogLevel.Info);
            switch (Patcher.RunPatchFromEditor(patchToTest))
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
            if (Init)
                return;

            Logging.Patcher("[ApplyChangesButton_Click]: Applying changes to patch: {0}", LogLevel.Info, SelectedPatchToString());
            if (SelectedPatch != null)
            {
                SaveApplyPatch(SelectedPatch);
            }
        }

        private void PatchesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Init)
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
                List<Instruction> patches = new List<Instruction>();
                InstructionLoader loader = new InstructionLoader();
                loader.AddInstructionObjectsToList(OpenPatchfileDialog.FileName, patches, InstructionsType.Patch, Patch.PatchXmlSearchPath);
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
                XmlDocument doc = SavePatchToXmlDocument(PatchesList.Items.OfType<Patch>().ToList());
                
                //delete the file if it currently exists
                if (File.Exists(SavePatchfileDialog.FileName))
                    File.Delete(SavePatchfileDialog.FileName);

                doc.Save(SavePatchfileDialog.FileName);
                UnsavedChanges = false;
                Logging.Patcher("[SavePatchXmlButton_Click]: Patch saved", LogLevel.Info);
            }
        }

        private XmlDocument SavePatchToXmlDocument(List<Patch> PatchesList)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement patchHolder = doc.CreateElement("patchs");
            doc.AppendChild(patchHolder);
            int counter = 0;
            foreach (Patch patch in PatchesList)
            {
                Logging.Patcher("[SavePatchToXmlDocument]: Saving patch {0} of {1}: {2}", LogLevel.Info, ++counter, PatchesList.Count, patch.ToString());
                Logging.Patcher("{0}", LogLevel.Info, patch.DumpInfoToLog);
                XmlElement xmlPatch = doc.CreateElement("patch");
                patchHolder.AppendChild(xmlPatch);

                foreach (string property in patch.PropertiesToSerialize())
                {
                    string propertyNameToSave = property;
                    //make the first char lowercase
                    propertyNameToSave = propertyNameToSave[0].ToString().ToLower() + propertyNameToSave.Substring(1);

                    XmlElement element = doc.CreateElement(propertyNameToSave);
                    PropertyInfo propertyInfo = typeof(Patch).GetProperty(property);
                    element.InnerText = propertyInfo.GetValue(patch).ToString();
                    xmlPatch.AppendChild(element);
                }

                if (patch.Type.Equals("regex"))
                {
                    doc.RemoveChild(doc.SelectSingleNode("path"));
                }
                else
                {
                    doc.RemoveChild(doc.SelectSingleNode("line"));
                }
            }
            return doc;
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
            PatchesList.Items.Add(new Patch());
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
    }
}
