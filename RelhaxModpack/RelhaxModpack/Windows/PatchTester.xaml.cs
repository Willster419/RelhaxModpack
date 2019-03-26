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
using System.IO;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for PatchTester.xaml
    /// </summary>
    public partial class PatchTester : RelhaxWindow
    {

        private PatchSettings PatchSettings;
        private XmlDocument LoadedPatchesDocument;
        //private List<Patch> LoadedPatches;
        //private Patch LoadedPatch;
        private OpenFileDialog OpenPatchfileDialog;
        private OpenFileDialog OpenFileToPatchDialog;
        private SaveFileDialog SavePatchfileDialog;
        private SaveFileDialog SelectAppPathDialog;
        private SaveFileDialog SelectVersionDirDialog;
        private bool UnsavedChanges = false;
        //for drag drop
        private bool IsPatchListScrolling = false;
        private Point BeforeDragDropPoint;
        private string[] validXmlModes = new string[]
        {
            "add",
            "edit",
            "remove"
        };
        private string[] validJsonModes = new string[]
        {
            "add",
            "arrayAdd",
            "remove",
            "arrayRemove",
            "edit",
            "arrayEdit",
            "arrayClear"
        };

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
            LoadSettingsToUI();
            //load empty patch definition
            PatchesList.Items.Clear();
            AddPatchButton_Click(null, null);
            PatchesList.SelectedIndex = 0;
            //attach the log output to the logfile
            Logging.TextBox = LogOutput;
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

        private void SaveOnSelectionChangeBheckbox_Checked(object sender, RoutedEventArgs e)
        {
            PatchSettings.SaveSelectionBeforeLeave = (bool)SaveOnSelectionChangeBheckbox.IsChecked;
        }

        private void SwitchToLogTabCheckbox_Checked(object sender, RoutedEventArgs e)
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
            //if the selection is json, enable the follow path selection box. else disable
            PatchModeCombobox.Items.Clear();
            if (PatchTypeCombobox.SelectedItem.Equals("json"))
            {
                PatchFollowPathSetting.IsEnabled = true;
                //also fill mode with json options
                foreach(string s in validJsonModes)
                {
                    PatchModeCombobox.Items.Add(s);
                }
            }
            else if (PatchTypeCombobox.SelectedItem.Equals("xml"))
            {
                PatchFollowPathSetting.IsEnabled = false;
                foreach (string s in validXmlModes)
                {
                    PatchModeCombobox.Items.Add(s);
                }
            }
            else
            {
                PatchFollowPathSetting.IsEnabled = false;

            }
        }

        private void DisplayPatch(Patch patch)
        {
            //reset to nothing, then only set if the patch option is valid
            FileToPatchTextbox.Text = string.Empty;
            PatchPathCombobox.SelectedItem = null;
            PatchTypeCombobox.SelectedItem = null;
            PatchModeCombobox.SelectedItem = null;
            PatchFollowPathSetting.IsChecked = false;
            PatchLinesPathTextbox.Text = string.Empty;
            PatchSearchTextbox.Text = string.Empty;
            PatchReplaceTextbox.Text = string.Empty;

            if (!string.IsNullOrWhiteSpace(patch.File))
                FileToPatchTextbox.Text = patch.File;
            if (!string.IsNullOrWhiteSpace(patch.PatchPath))
                PatchPathCombobox.SelectedItem = patch.PatchPath;
            if (!string.IsNullOrWhiteSpace(patch.Type))
                PatchTypeCombobox.SelectedItem = patch.Type;
            if (!string.IsNullOrWhiteSpace(patch.Mode))
                PatchModeCombobox.SelectedItem = patch.Mode;
            PatchFollowPathSetting.IsChecked = patch.FollowPath;
            if (patch.Type.Equals("regex"))
            {
                if (patch.Lines.Count() > 0)
                    PatchLinesPathTextbox.Text = string.Join(",", patch.Lines);
            }
            else
            {
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
            UnsavedChanges = true;
            //save all UI settings to patch object
            patch.File = FileToPatchTextbox.Text;
            patch.PatchPath = PatchPathCombobox.SelectedItem as string;
            patch.Type = PatchTypeCombobox.SelectedItem as string;
            patch.Mode = PatchModeCombobox.SelectedItem as string;
            patch.FollowPath = (bool)PatchFollowPathSetting.IsChecked;
            if(patch.Type.Equals("Regex"))
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
        }
        #endregion

        #region Buttons
        private void LocateFileToPatchButton_Click(object sender, RoutedEventArgs e)
        {
            if(OpenFileToPatchDialog == null)
            {
                OpenFileToPatchDialog = new OpenFileDialog()
                {
                    Title = "Select a file to test patches on",
                    AddExtension = true,
                    CheckPathExists = true,
                    RestoreDirectory = true,
                    Multiselect = false,
                    CheckFileExists = true
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
            Logging.Info("TestPatch() start");
            Logging.Info("Checking UI elements for valid patch information...");
            //make new patch element
            Patch patchToTest = new Patch();
            //check input from UI left panel side

            //file location
            Logging.Info("File to Patch location mode: {0}", FilePathType.SelectedItem == null ? "(null)" : FilePathType.SelectedItem);
            switch(FilePathType.SelectedItem.ToString())
            {
                case "Absolute":
                    Logging.Info("Checking if absolute file path {0} exists...", FileToPatchTextbox.Text);
                    if(File.Exists(FileToPatchTextbox.Text))
                    {
                        Logging.Info("File Exists!");
                    }
                    else
                    {
                        Logging.Info("File does not exist, aborting");
                        return;
                    }
                    break;
                case "Relative":
                    Logging.Info("Using relative macro {0}", PatchPathCombobox.SelectedItem == null ? "(null)" : PatchPathCombobox.SelectedItem);
                    string completePathForPatchFile = string.Empty;
                    switch(PatchPathCombobox.SelectedItem.ToString())
                    {
                        case "app":
                            if (Directory.Exists(PatchSettings.AppMacro))
                            {
                                Logging.Info("app macro folder path exists...");
                                if(FileToPatchTextbox.Text.Contains("versiondir") && string.IsNullOrWhiteSpace(PatchSettings.VersiondirMacro))
                                {
                                    Logging.Info("versiondir macro found, but versiondir patch seting macro is blank, aborting");
                                    return;
                                }
                                completePathForPatchFile = PatchSettings.AppMacro + FileToPatchTextbox.Text;
                                completePathForPatchFile = Utils.MacroReplace(completePathForPatchFile, ReplacementTypes.ZipFilePath);
                            }
                            else
                            {
                                Logging.Info("app macro folder path does not exist, aborting");
                                return;
                            }
                            break;
                        case "appData":
                            if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)))
                            {
                                Logging.Info("appData macro folder path exists...");
                                completePathForPatchFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + FileToPatchTextbox.Text;
                            }
                            else
                            {
                                Logging.Info("appData macro folder path does not exist, aborting");
                                return;
                            }
                            break;
                        default:
                            Logging.Info("Invalid path macro");
                            return;
                    }
                    Logging.Info("relative path built as {0}", completePathForPatchFile);
                    if(File.Exists(completePathForPatchFile))
                    {
                        Logging.Info("File exists!");
                        patchToTest.File = FileToPatchTextbox.Text;
                        patchToTest.CompletePath = completePathForPatchFile;
                    }
                    else
                    {
                        Logging.Info("File does not exist, aborting (did you forget to add \"\\\" to the beginning of the path?");
                    }
                    break;
                default:
                    Logging.Info("Invalid file path type, aborting");
                    return;
            }
            //check patch type
            //check patch mode
            //check followPath true ONLY for json
            //check patch, search, replace

            //put patch into patch test methods
            //(also means to comment the patch methods more)
            //add setting for patch if from editor, if true (for duration of patch method) enable verbose logging
        }

        private void ApplyChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (PatchesList.SelectedItem != null)
            {
                SaveApplyPatch(PatchesList.SelectedItem as Patch);
            }
        }

        private void PatchesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(PatchSettings.SaveSelectionBeforeLeave && PatchesList.SelectedItem != null)
            {
                SaveApplyPatch(PatchesList.SelectedItem as Patch);
            }
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
                    RestoreDirectory = true,
                    Multiselect = false,
                    DefaultExt = "xml"
                };
            }
            if(!(bool)OpenPatchfileDialog.ShowDialog())
            {
                XmlDocument doc = XMLUtils.LoadXmlDocument(OpenPatchfileDialog.FileName, XmlLoadType.FromFile);
                if(doc == null)
                {
                    MessageBox.Show("Failed to load xml document, check the logs for more info");
                    return;
                }
                PatchesList.Items.Clear();
                foreach(XmlNode node in XMLUtils.GetXMLNodesFromXPath(doc,"//patchs/patch"))
                {
                    Patch patch = new Patch();
                    foreach(XmlElement element in ((XmlElement)node).ChildNodes)
                    {
                        switch(element.Name)
                        {
                            case "type":
                                switch(element.InnerText.ToLower().Trim())
                                {
                                    case "xvm":
                                        throw new BadMemeException("XVM IS NOT SUPPORTED PLEASE STOP USING IT");
                                        break;
                                    //legacy compatibility, regx -> regex
                                    case "regx":
                                        patch.Type = "regex";
                                        break;
                                    default:
                                        patch.Type = element.InnerText.ToLower().Trim();
                                        break;
                                }
                                break;
                            case "patchPath":
                                //remove "{" and "}"
                                string patchPathValue = element.InnerText.Replace("{", string.Empty).Replace("}", string.Empty).Trim();
                                switch (patchPathValue.ToLower().Trim())
                                {
                                    case "appdata":
                                        patch.PatchPath = "appData";
                                        break;
                                    default:
                                        patch.PatchPath = element.InnerText.ToLower().Trim();
                                        break;
                                }
                                break;
                            case "file":
                                patch.File = element.InnerText.Trim();
                                break;
                            case "path":
                                patch.Path = element.InnerText.Trim();
                                break;
                            case "line":
                                patch.Lines = element.InnerText.Split(',');
                                break;
                            case "search":
                                patch.Search = element.InnerText.Trim();
                                break;
                            case "replace":
                                patch.Replace = Utils.MacroReplace(element.InnerText,ReplacementTypes.TextUnescape).Trim();
                                break;
                        }
                    }
                    PatchesList.Items.Add(patch);
                }
            }
        }

        private void SavePatchXmlButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavePatchfileDialog == null)
            {
                SavePatchfileDialog = new SaveFileDialog()
                {
                    Title = "Select a file to test patches on",
                    AddExtension = true,
                    CheckPathExists = true,
                    RestoreDirectory = true,
                    DefaultExt = "xml",
                };
            }
            if(!(bool)SavePatchfileDialog.ShowDialog())
            {
                if(PatchSettings.ApplyBehavior == ApplyBehavior.SaveTriggersApply && PatchesList.SelectedItem != null)
                {
                    SaveApplyPatch(PatchesList.SelectedItem as Patch);
                }

                XmlDocument doc = new XmlDocument();
                XmlElement patchHolder = doc.CreateElement("patchs");
                doc.AppendChild(patchHolder);
                foreach(Patch patch in PatchesList.Items)
                {
                    XmlElement xmlPatch = doc.CreateElement("patch");
                    patchHolder.AppendChild(xmlPatch);

                    XmlElement type = doc.CreateElement("type");
                    type.InnerText = patch.Type;
                    xmlPatch.AppendChild(type);

                    XmlElement patchPath = doc.CreateElement("patchPath");
                    patchPath.InnerText = patch.PatchPath;
                    xmlPatch.AppendChild(patchPath);

                    XmlElement file = doc.CreateElement("file");
                    file.InnerText = patch.File;
                    xmlPatch.AppendChild(file);

                    if(patch.Type.Equals("regex"))
                    {
                        XmlElement line = doc.CreateElement("line");
                        line.InnerText = string.Join(",",patch.Lines);
                        xmlPatch.AppendChild(line);
                    }
                    else
                    {
                        XmlElement line = doc.CreateElement("path");
                        line.InnerText = patch.Path;
                        xmlPatch.AppendChild(line);
                    }

                    XmlElement search = doc.CreateElement("search");
                    search.InnerText = patch.Search;
                    xmlPatch.AppendChild(search);

                    XmlElement replace = doc.CreateElement("replace");
                    replace.InnerText = Utils.MacroReplace(patch.Replace,ReplacementTypes.TextEscape);
                    xmlPatch.AppendChild(replace);
                }
                
                doc.Save(SavePatchfileDialog.FileName);
                UnsavedChanges = false;
            }
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            LogOutput.Text = string.Empty;
        }

        private void RemovePatchButton_Click(object sender, RoutedEventArgs e)
        {
            if (PatchesList.SelectedItem != null)
                PatchesList.Items.Remove(PatchesList.SelectedItem);
        }

        private void AddPatchButton_Click(object sender, RoutedEventArgs e)
        {
            PatchesList.Items.Add(new Patch() { });
        }
        #endregion

        #region Drag drop code
        private void LogOutput_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if(Mouse.LeftButton == MouseButtonState.Pressed)
            {
                IsPatchListScrolling = true;
            }
        }

        private void LogOutput_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                IsPatchListScrolling = false;
            }
        }

        private void LogOutput_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                BeforeDragDropPoint = e.GetPosition(PatchesList);
            }
        }

        private void LogOutput_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && IsDragConfirmed(e.GetPosition(PatchesList)) && !IsPatchListScrolling)
            {
                if (PatchesList.SelectedItem is Patch patch)
                {
                    DragDrop.DoDragDrop(PatchesList, patch, DragDropEffects.Move);
                }
            }
        }

        private void LogOutput_DragOver(object sender, DragEventArgs e)
        {

        }

        private void LogOutput_Drop(object sender, DragEventArgs e)
        {
            if(PatchesList.SelectedItem is Patch patchToMove)
            {
                if(e.OriginalSource is Patch patchOver)
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
