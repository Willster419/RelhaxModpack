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
        private bool init = true;
        //for drag drop
        private bool IsPatchListScrolling = false;
        private bool RegressionsRunning = false;
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
            Logging.OnLoggingUIThreadReport += Logging_OnLoggingUIThreadReport;
            init = false;
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
            if (PatchTypeCombobox.SelectedItem == null)
                return;
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
        private void LogOutput_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if(RegressionsRunning)
                LogOutput.ScrollToEnd();
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
            if(FilePathType.SelectedItem == null)
            {
                Logging.Info("Invalid file path type");
                return;
            }
            switch(FilePathType.SelectedItem.ToString())
            {
                case "Absolute":
                    Logging.Info("Checking if absolute file path {0} exists...", FileToPatchTextbox.Text);
                    if(File.Exists(FileToPatchTextbox.Text))
                    {
                        Logging.Info("File Exists!");
                        patchToTest.File = FileToPatchTextbox.Text;
                        patchToTest.CompletePath = FileToPatchTextbox.Text;
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
            if(PatchTypeCombobox.SelectedItem == null)
            {
                Logging.Info("Invalid Patch Type, aborting");
                return;
            }
            patchToTest.Type = PatchTypeCombobox.SelectedItem as string;
            //check patch mode
            switch (patchToTest.Type)
            {
                case "regex":
                case "regx":
                    if(!string.IsNullOrWhiteSpace(PatchModeCombobox.SelectedItem as string))
                    {
                        Logging.Info("Type=regex, invalid patch type: {0}", PatchModeCombobox.SelectedItem as string);
                        Logging.Info("valid types are: (null)");
                        return;
                    }
                    //set the lines
                    if(!string.IsNullOrWhiteSpace(PatchLinesPathTextbox.Text))
                        patchToTest.Lines = PatchLinesPathTextbox.Text.Split(',');
                    break;
                case "xml":
                    //check if path/lines is valid (has string values)
                    if (string.IsNullOrWhiteSpace(PatchLinesPathTextbox.Text))
                    {
                        Logging.Info("invalid patch path or lines");
                        return;
                    }
                    if (!validXmlModes.Contains(PatchModeCombobox.SelectedItem as string))
                    {
                        Logging.Info("Type=xml, invalid patch type: {0}", PatchModeCombobox.SelectedItem as string);
                        Logging.Info("valid types are: {0}",string.Join(",",validXmlModes));
                        return;
                    }
                    patchToTest.Path = PatchLinesPathTextbox.Text;
                    break;
                case "json":
                    //check if path/lines is valid (has string values)
                    if (string.IsNullOrWhiteSpace(PatchLinesPathTextbox.Text))
                    {
                        Logging.Info("invalid patch path or lines");
                        return;
                    }
                    if (!validJsonModes.Contains(PatchModeCombobox.SelectedItem as string))
                    {
                        Logging.Info("Type=json, invalid patch type: {0}", PatchModeCombobox.SelectedItem as string);
                        Logging.Info("valid types are: {0}", string.Join(",", validJsonModes));
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
                Logging.Info("Types=json, followPathSetting must be false!");
                return;
            }
            //check search and replace
            if (string.IsNullOrWhiteSpace(PatchReplaceTextbox.Text) && string.IsNullOrWhiteSpace(PatchSearchTextbox.Text))
            {
                Logging.Info("patch repalce and search are blank, invalid patch");
                return;
            }
            if (string.IsNullOrWhiteSpace(PatchSearchTextbox.Text))
            {
                Logging.Warning("patch search is blank (is this the intent?)");
            }
            patchToTest.Search = PatchSearchTextbox.Text;
            if (string.IsNullOrWhiteSpace(PatchReplaceTextbox.Text))
            {
                Logging.Info("patch replace is blank (is this the intent?)");
            }
            patchToTest.Replace = PatchReplaceTextbox.Text;
            //put patch into patch test methods
            //set patch from editor to true to enable verbose logging
            if(!patchToTest.FromEditor)
                patchToTest.FromEditor = true;
            PatchUtils.RunPatch(patchToTest);
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
            if (init)
                return;
            if(PatchSettings.SaveSelectionBeforeLeave && PatchesList.SelectedItem != null)
            {
                SaveApplyPatch(PatchesList.SelectedItem as Patch);
            }
            if(PatchesList.SelectedItem as Patch != null)
            {
                DisplayPatch(PatchesList.SelectedItem as Patch);
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
            if((bool)OpenPatchfileDialog.ShowDialog())
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
                    Patch patch = new Patch() { FromEditor = true };
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
                            case "mode":
                                patch.Mode = element.InnerText.Trim();
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
                    Title = "Save xml patch",
                    AddExtension = true,
                    CheckPathExists = true,
                    RestoreDirectory = true,
                    DefaultExt = "xml",
                };
            }
            if((bool)SavePatchfileDialog.ShowDialog())
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

                    XmlElement mode = doc.CreateElement("mode");
                    mode.InnerText = patch.Mode;
                    xmlPatch.AppendChild(mode);

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
            Logging.Info("Regex regressions start");
            await Task.Run(() =>
            {
                Regression regression = new Regression(RegressionTypes.regex, BuildRegexUnittests());
                regression.RunRegressions();
            });
            Logging.Info("Regex regressions end");
            Dispatcher.Invoke(new Action(() => { RegressionsRunning = false; }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private async void XmlRegressionTesting_Click(object sender, RoutedEventArgs e)
        {
            RegressionsRunning = true;
            if (PatchSettings.SwitchToLogWhenTestingPatch)
            {
                RightSideTabControl.SelectedItem = LogOutputTab;
            }
            Logging.Info("Xml regressions start");
            await Task.Run(() =>
            {
                Regression regression = new Regression(RegressionTypes.xml, BuildXmlUnittests());
                regression.RunRegressions();
            });
            Logging.Info("Xml regressions end");
            Dispatcher.Invoke(new Action(() => { RegressionsRunning = false; }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private async void JsonRegressionTesting_Click(object sender, RoutedEventArgs e)
        {
            RegressionsRunning = true;
            if (PatchSettings.SwitchToLogWhenTestingPatch)
            {
                RightSideTabControl.SelectedItem = LogOutputTab;
            }
            Logging.Info("Json regressions start");
            await Task.Run(() =>
            {
                Regression regression = new Regression(RegressionTypes.json, BuildJsonUnittests());
                regression.RunRegressions();
            });
            Logging.Info("Json regressions end");
            Dispatcher.Invoke(new Action(() => { RegressionsRunning = false; }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private async void FollowPathRegressionTesting_Click(object sender, RoutedEventArgs e)
        {
            RegressionsRunning = true;
            if (PatchSettings.SwitchToLogWhenTestingPatch)
            {
                RightSideTabControl.SelectedItem = LogOutputTab;
            }
            Logging.Info("FollowPath regressions start");
            await Task.Run(() =>
            {
                Regression regression = new Regression(RegressionTypes.followPath, BuildFollowPathUnittests());
                regression.RunRegressions();
            });
            Logging.Info("FollowPath regressions end");
            Dispatcher.Invoke(new Action(() => { RegressionsRunning = false; }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private void Logging_OnLoggingUIThreadReport(string message)
        {
            //https://www.tutorialspoint.com/csharp/csharp_delegates.htm
            //https://docs.microsoft.com/en-us/dotnet/api/system.windows.threading.dispatcher.invoke?redirectedfrom=MSDN&view=netframework-4.6.1#examples
            //https://stackoverflow.com/questions/1951927/events-in-c-sharp-definition-and-example
            //https://stackoverflow.com/questions/4936459/dispatcher-begininvoke-cannot-convert-lambda-to-system-delegate
            Dispatcher.BeginInvoke(new Action(() => { LogOutput.AppendText(message + Environment.NewLine); }));
        }
        #endregion

        #region json regressions
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

        #region xml regressions
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

        #region regex regressions
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

        #region followPath regressions
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
                        Path = @"$.damageLogTest.enabled",
                        Search = ".*",
                        Replace = "false",
                        Type = "json"
                    }
                }
            };
        }
        #endregion
    }
}
