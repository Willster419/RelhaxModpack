using Microsoft.Win32;
using RelhaxModpack.Database;
using RelhaxModpack.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Path = System.IO.Path;
using Microsoft.WindowsAPICodePack;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using RelhaxModpack.Xml;
using RelhaxModpack.Utilities;
using Trigger = RelhaxModpack.Database.Trigger;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Utilities.ClassEventArgs;
using System.Windows.Controls.Primitives;
using RelhaxModpack.Settings;
using RelhaxModpack.Common;
using System.ComponentModel;
using RelhaxModpack.Patching;
using RelhaxModpack.Atlases;
using RelhaxModpack.Shortcuts;
using RelhaxModpack.Installer;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for DatabaseEditor.xaml
    /// </summary>
    public partial class DatabaseEditor : RelhaxFeatureWindowWithChanges
    {
        /// <summary>
        /// The command line argument specified at application launch to show this window
        /// </summary>
        public const string CommandLineArg = "database-editor";

        /// <summary>
        /// The name of the logfile
        /// </summary>
        public const string LoggingFilename = "RelhaxEditor.log";

        private DatabaseManager databaseManager;
        private List<DatabasePackage> GlobalDependencies { get { return databaseManager.GlobalDependencies; } }
        private List<Dependency> Dependencies { get { return databaseManager.Dependencies; } }
        private List<Category> ParsedCategoryList { get { return databaseManager.ParsedCategoryList; } }
        private string WoTModpackOnlineFolderVersion { get { return databaseManager.WoTOnlineFolderVersion; } }
        private string WoTClientVersion { get { return databaseManager.WoTClientVersion; } }

        private EditorSettings EditorSettings = new EditorSettings();
        private OpenFileDialog OpenDatabaseDialog;
        private SaveFileDialog SaveDatabaseDialog;
        private OpenFileDialog OpenZipFileDialog;
        private SaveFileDialog SaveZipFileDialog;
        private OpenFileDialog OpenPictureDialog;
        private TreeViewItem ItemToExpand;
        private Point BeforeDragDropPoint;
        private bool IsScrolling = false;
        private bool AlreadyLoggedScroll = false;
        private object SelectedItem = null;
        private Preview Preview = null;
        private DispatcherTimer DragDropTimer = null;
        private DispatcherTimer ReselectOldItem = null;
        private string[] UIHeaders = new string[]
        {
            "-----Global Dependencies-----",
            "-----Dependencies-----",
        };
        private readonly string UpdatedPackageNewCRC = "f";
        private PopOutReplacePatchDesigner popOutReplacePatchDesigner = null;
        private Patcher Patcher = new Patcher() { DebugMode = false };

        #region Stuff
        /// <summary>
        /// Create an instance of the DatabaseEditor
        /// </summary>
        public DatabaseEditor(ModpackSettings modpackSettings, Logfiles logfile) : base (modpackSettings, logfile)
        {
            InitializeComponent();
            Settings = EditorSettings;
        }

        private void OnApplicationLoad(object sender, RoutedEventArgs e)
        {
            //check if we are loading the document auto from the command line
            LoadSettingsToUI();
            if (!string.IsNullOrWhiteSpace(CommandLineSettings.EditorAutoLoadFileName))
            {
                Logging.Editor("Attempting to auto-load xml file from {0}", LogLevel.Info, CommandLineSettings.EditorAutoLoadFileName);
                if (File.Exists(CommandLineSettings.EditorAutoLoadFileName))
                {
                    OnLoadDatabaseClick(null, null);
                }
                else
                {
                    Logging.Editor("File does not exist");
                }
            }

            //setup database manager
            databaseManager = new DatabaseManager(ModpackSettings, CommandLineSettings);

            //load the trigger box with trigger options
            LoadedTriggersComboBox.Items.Clear();
            foreach (Trigger t in InstallEngine.Triggers)
            {
                LoadedTriggersComboBox.Items.Add(t.Name);
            }

            //load the tag box with tag options
            LoadedTagsComboBox.Items.Clear();
            foreach (PackageTags packageTag in CommonUtils.GetEnumList<PackageTags>())
            {
                LoadedTagsComboBox.Items.Add(packageTag);
            }

            //init timers
            ReselectOldItem = new DispatcherTimer(TimeSpan.FromMilliseconds(50), DispatcherPriority.Normal, AwesomeHack_Tick, base.Dispatcher) { IsEnabled = false };
            DragDropTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, OnDragDropTimerTick, base.Dispatcher) { IsEnabled = false };

            //clear searchbox
            SearchBox.Items.Clear();

            //set the items for the triggers combobox. this only needs to be done once anyways
            LoadedTriggersComboBox.Items.Clear();
            foreach (string s in InstallEngine.CompleteTriggerList)
                LoadedTriggersComboBox.Items.Add(s);
            Init = false;
        }

        private void AwesomeHack_Tick(object sender, EventArgs e)
        {
            if(SelectedItem is DatabasePackage db)
            {
                db.EditorTreeViewItem.IsSelected = true;
            }
            else if (SelectedItem is EditorComboBoxItem edit)
            {
                edit.Package.EditorTreeViewItem.IsSelected = true;
            }
            ReselectOldItem.Stop();
        }

        private void OnDragDropTimerTick(object sender, EventArgs e)
        {
            DragDropTimer.Stop();
            if (ItemToExpand.Header is EditorComboBoxItem item)
            {
                if (item.Package is SelectablePackage sp)
                {
                    if (sp.Packages.Count > 0)
                    {
                        if (!ItemToExpand.IsExpanded)
                            ItemToExpand.IsExpanded = true;
                    }
                }
            }
        }

        private int GetMaxPatchGroups()
        {
            return databaseManager.GetMaxPatchGroupNumber();
        }

        private int GetMaxInstallGroups()
        {
            return databaseManager.GetMaxInstallGroupNumber();
        }

        private SelectablePackage GetSelectablePackage(object obj)
        {
            if (obj is SelectablePackage selectablePackage)
                return selectablePackage;

            else if (obj is EditorComboBoxItem editorComboBoxItem)
                if (editorComboBoxItem.Package is SelectablePackage selectablePackage2)
                    return selectablePackage2;

            return null;
        }

        private DatabasePackage GetDatabasePackage(object obj)
        {
            if (obj is DatabasePackage selectablePackage)
                return selectablePackage;

            else if (obj is EditorComboBoxItem editorComboBoxItem)
                if (editorComboBoxItem.Package is DatabasePackage databasePackage2)
                    return databasePackage2;

            return null;
        }

        private TreeViewItem GetPackageTreeViewItem(IDatabaseComponent componentWithID)
        {
            if (componentWithID is Category category)
                return category.EditorTreeViewItem;
            else if (componentWithID is DatabasePackage databasePackage)
                return databasePackage.EditorTreeViewItem;
            else
                return null;
        }
        #endregion

        #region Load UI Views

        private void LoadUI(List<DatabasePackage> globalDependencies, List<Dependency> dependnecies, List<Category> parsedCategoryList, int numToAddEnd = 5)
        {
            //reset the UI first
            ResetRightPanels(null);

            //also make the selected item null just in case
            if (SelectedItem != null)
            {
                Logging.Editor("from LoadUI(), selectedItem is not null, setting to null (user pressed a load database function) previous={0}", LogLevel.Info, SelectedItem.ToString());
                SelectedItem = null;
            }

            //load database views
            LoadDatabaseView();
            LoadInstallView();
            LoadPatchView();
        }

        private void LoadDatabaseView(int numToAddEnd = 5)
        {
            //clear and reset
            DatabaseTreeView.Items.Clear();

            //create treeviewItems for each entry
            //first make the globalDependencies header
            TreeViewItem globalDependenciesHeader = new TreeViewItem() { Header = UIHeaders[0] };
            //add it to the main view
            DatabaseTreeView.Items.Add(globalDependenciesHeader);
            //loop to add all the global dependencies to a treeview item, which is a new comboboxitem, which is the package and displayname
            foreach (DatabasePackage globalDependency in GlobalDependencies)
            {
                globalDependency.EditorTreeViewItem = new TreeViewItem() { Header = new EditorComboBoxItem(globalDependency) };
                globalDependenciesHeader.Items.Add(globalDependency.EditorTreeViewItem);
            }

            //same for dependencies
            TreeViewItem dependenciesHeader = new TreeViewItem() { Header = UIHeaders[1] };
            DatabaseTreeView.Items.Add(dependenciesHeader);
            foreach (DatabasePackage dependency in Dependencies)
            {
                dependency.EditorTreeViewItem = new TreeViewItem() { Header = new EditorComboBoxItem(dependency) };
                dependenciesHeader.Items.Add(dependency.EditorTreeViewItem);
            }

            //add the category, then add each level recursively
            foreach (Category cat in ParsedCategoryList)
            {
                cat.EditorTreeViewItem = new TreeViewItem() { Header = cat };
                DatabaseTreeView.Items.Add(cat.EditorTreeViewItem);
                LoadUI(cat.EditorTreeViewItem, cat.Packages);
            }

            //adding the spacing that dirty wants...
            for (int i = 0; i < numToAddEnd; i++)
            {
                DatabaseTreeView.Items.Add(string.Empty);
            }
        }

        private void LoadInstallView(int numToAddEnd = 5)
        {
            //load the install and patch groups
            InstallGroupsTreeView.Items.Clear();
            //make a flat list (can be used in patchGroup as well)
            List<DatabasePackage> allFlatList = databaseManager.GetFlatList();
            //make an array of group headers
            TreeViewItem[] installGroupHeaders = new TreeViewItem[databaseManager.GetMaxInstallGroupNumberWithOffset() + 1];
            //for each group header, get the list of packages that have an equal install group number
            //hey while we're at it let's add the items to the instal group dispaly box
            PackageInstallGroupDisplay.Items.Clear();
            for (int i = 0; i < installGroupHeaders.Count(); i++)
            {
                PackageInstallGroupDisplay.Items.Add(i);
                installGroupHeaders[i] = new TreeViewItem() { Header = string.Format("---Install Group {0}---", i), Tag = i };
                InstallGroupsTreeView.Items.Add(installGroupHeaders[i]);
                installGroupHeaders[i].Items.Clear();
                foreach (DatabasePackage packageWithEqualGroupNumber in allFlatList.Where(package => package.InstallGroupWithOffset == i).ToList())
                {
                    //add them to the install group headers
                    installGroupHeaders[i].Items.Add(new TreeViewItem() { Header = new EditorComboBoxItem(packageWithEqualGroupNumber) });
                }
            }
            //adding the spacing that dirty wants...
            for (int i = 0; i < numToAddEnd; i++)
            {
                InstallGroupsTreeView.Items.Add(string.Empty);
            }
        }

        private void LoadPatchView(int numToAddEnd = 5)
        {
            //do the same for patchgroups
            PatchGroupsTreeView.Items.Clear();
            //make a flat list (can be used in patchGroup as well)
            List<DatabasePackage> allFlatList = databaseManager.GetFlatList();
            TreeViewItem[] patchGroupHeaders = new TreeViewItem[databaseManager.GetMaxPatchGroupNumber() + 1];
            //for each group header, get the list of packages that have an equal patch group number
            PackagePatchGroupDisplay.Items.Clear();
            for (int i = 0; i < patchGroupHeaders.Count(); i++)
            {
                PackagePatchGroupDisplay.Items.Add(i);
                patchGroupHeaders[i] = new TreeViewItem() { Header = string.Format("---Patch Group {0}---", i), Tag = i };
                PatchGroupsTreeView.Items.Add(patchGroupHeaders[i]);
                patchGroupHeaders[i].Items.Clear();
                foreach (DatabasePackage packageWithEqualGroupNumber in allFlatList.Where(package => package.PatchGroup == i).ToList())
                {
                    patchGroupHeaders[i].Items.Add(new TreeViewItem() { Header = new EditorComboBoxItem(packageWithEqualGroupNumber) });
                }
            }
            //adding the spacing that dirty wants...
            for (int i = 0; i < numToAddEnd; i++)
            {
                PatchGroupsTreeView.Items.Add(string.Empty);
            }
        }

        private void LoadUI(TreeViewItem parent, List<SelectablePackage> packages)
        {
            foreach (SelectablePackage package in packages)
            {
                //make a TVI for it
                TreeViewItem packageTVI = new TreeViewItem() { Header = new EditorComboBoxItem(package) };
                //add the new tvi refrence to the package
                package.EditorTreeViewItem = packageTVI;
                //and have the parent add it
                parent.Items.Add(packageTVI);
                if (package.Packages.Count > 0)
                    LoadUI(packageTVI, package.Packages);
            }
        }

        private void LoadSettingsToUI()
        {
            BigmodsUsernameSetting.Text = EditorSettings.BigmodsUsername;
            BigmodsPasswordSetting.Text = EditorSettings.BigmodsPassword;

            DefaultSaveLocationSetting.Text = EditorSettings.DefaultEditorSaveLocation;

            SelectTransferWindowMovePathTextbox.Text = EditorSettings.UploadZipMoveFolder;

            SaveSelectionBeforeLeaveSetting.IsChecked = EditorSettings.SaveSelectionBeforeLeave;

            ShowConfirmOnPackageApplySetting.IsChecked = EditorSettings.ShowConfirmationOnPackageApply;
            ShowConfirmOnPackageAddRemoveEditSetting.IsChecked = EditorSettings.ShowConfirmationOnPackageAddRemoveMove;

            ApplyBehaviorDefaultSetting.IsChecked = EditorSettings.ApplyBehavior == ApplyBehavior.Default ? true : false;
            ApplyBehaviorApplyTriggersSaveSetting.IsChecked = EditorSettings.ApplyBehavior == ApplyBehavior.ApplyTriggersSave ? true : false;
            ApplyBehaviorSaveTriggersApplySetting.IsChecked = EditorSettings.ApplyBehavior == ApplyBehavior.SaveTriggersApply ? true : false;

            FtpUpDownAutoCloseTimoutSlider.Value = EditorSettings.FTPUploadDownloadWindowTimeout;
            FtpUpDownAutoCloseTimoutDisplayLabel.Text = EditorSettings.FTPUploadDownloadWindowTimeout.ToString();

            DatabaseTransferDeleteActuallyMove.IsChecked = EditorSettings.UploadZipDeleteIsActuallyMove;
            DatabaseTransferAutoDelete.IsChecked = EditorSettings.DeleteUploadLocallyUponCompletion;
        }
        #endregion

        #region Other UI events

        private void LeftTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Init)
                return;
            if (!(LeftTabView.SelectedItem is TabItem selectedTab))
                return;

            //disable apply whenever left tab views change
            ApplyButton.IsEnabled = false;

            if (selectedTab.Equals(DatabaseViewTab))
            {
                RightTab.IsEnabled = true;
                SearchBox.IsEnabled = true;
                RemoveDatabaseObjectButton.IsEnabled = true;
                MoveDatabaseObjectButton.IsEnabled = true;
                AddDatabaseObjectButton.IsEnabled = true;
            }
            else if (selectedTab.Equals(InstallGroupsTab))
            {
                RightTab.IsEnabled = false;
                SearchBox.IsEnabled = true;
                RemoveDatabaseObjectButton.IsEnabled = false;
                MoveDatabaseObjectButton.IsEnabled = false;
                AddDatabaseObjectButton.IsEnabled = false;
                if (GlobalDependencies.Count == 0)
                {
                    Logging.Editor("Database is not yet loaded, skipping UI loading");
                }
                else if (sender == null)
                {
                    Logging.Editor("Database is loaded but this call is not from UI event. Don't load install view.");
                }
                else
                {
                    Logging.Editor("Database is loaded and this call is from UI event. Load install view.");
                    LoadInstallView();
                }
            }
            else if (selectedTab.Equals(PatchGroupsTab))
            {
                RightTab.IsEnabled = false;
                SearchBox.IsEnabled = true;
                RemoveDatabaseObjectButton.IsEnabled = false;
                MoveDatabaseObjectButton.IsEnabled = false;
                AddDatabaseObjectButton.IsEnabled = false;
                if (GlobalDependencies.Count == 0)
                {
                    Logging.Editor("Database is not yet loaded, skipping UI loading");
                }
                else if (sender == null)
                {
                    Logging.Editor("Database is loaded but this call is not from UI event. Don't load patch view.");
                }
                else
                {
                    Logging.Editor("Database is loaded and this call is from UI event. Load patch view.");
                    LoadPatchView();
                }
            }
            else if (selectedTab.Equals(SettingsTab))
            {
                SearchBox.IsEnabled = false;
                RightTab.IsEnabled = false;
                RemoveDatabaseObjectButton.IsEnabled = false;
                MoveDatabaseObjectButton.IsEnabled = false;
                AddDatabaseObjectButton.IsEnabled = false;
            }

        }

        private void PackageDevURLDisplay_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //since it is multiple lines, split into array
            string[] DevURLs = PackageDevURLDisplay.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            int lastCount = 1;
            foreach (string DevURL in DevURLs)
            {
                lastCount += DevURL.Length;
                if (PackageDevURLDisplay.SelectionStart <= lastCount)
                {
                    Logging.Editor("DevURL selection parsed at selectionStart={0} (total={1}, current={2}, lines total={3}), opening URL as {4}", LogLevel.Info,
                            PackageDevURLDisplay.SelectionStart, PackageDevURLDisplay.Text.Length, DevURL.Length, DevURLs.Count(), DevURL.Trim());
                    try
                    {
                        System.Diagnostics.Process.Start(DevURL.Trim());
                    }
                    catch
                    {
                        Logging.Editor("Failed to open DevURL {0}", LogLevel.Info, DevURL.Trim());
                    }
                    return;
                }
            }
        }

        private void ResetRightPanels(DatabasePackage package, bool resetForComponent = true)
        {
            Logging.Editor("ResetRightPanels(), package type = {0}, name = {1}, resetForComponent = {2}",
                LogLevel.Info, package == null ? "(null)" : package.GetType().ToString(), package == null ? "(null)" : package.PackageName, resetForComponent);

            //for each tab, disable all UI components
            List<Control> controlsToDisable = new List<Control>();
            ComboBox[] comboboxesToSkip = new ComboBox[]
            {
                PackageInstallGroupDisplay,
                PackagePatchGroupDisplay,
                LoadedDependenciesList,
                LoadedTriggersComboBox,
                LoadedLogicsList,
                PackageTypeDisplay,
                MediaTypesList,
                LoadedTagsComboBox,
                PatchVersionCombobox,
                PatchPathCombobox,
                PatchTypeCombobox
            };
            foreach (TabItem tabItem in RightTab.Items)
            {
                foreach (FrameworkElement element in UiUtils.GetAllWindowComponentsLogical(tabItem, false))
                {
                    //if it's a common element used in the panel, then disable it
                    if (element is CheckBox || element is ComboBox || element is Button || element is TextBox || element is ListBox)
                    {
                        controlsToDisable.Add((Control)element);
                    }

                    //also clear it's data for each type
                    if (element is CheckBox box)
                    {
                        box.IsChecked = false;
                    }
                    else if (element is ComboBox cbox)
                    {
                        //don't clear these, they are static. just un-select anything that could be selected
                        if (comboboxesToSkip.Contains(cbox))
                        {
                            cbox.SelectedIndex = -1;
                            continue;
                        }
                        else
                        {
                            cbox.Items.Clear();
                        }
                    }
                    else if (element is TextBox tbox && !tbox.Name.Equals(nameof(CurrentSupportedTriggers)))
                    {
                        tbox.Text = string.Empty;
                    }
                    else if (element is ListBox lbox)
                    {
                        lbox.Items.Clear();
                    }
                    else if(element.Name.Equals(nameof(PackageLevelDisplay)) || element.Name.Equals(nameof(PackageCalculatedLevelDisplay)))
                    {
                        (element as TextBlock).Text = string.Empty;
                    }

                    //also clear the undo stack if enabled
                    if(element is TextBoxBase tbb && tbb.IsUndoEnabled)
                    {
                        //https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.primitives.textboxbase.isundoenabled?view=netframework-4.8#System_Windows_Controls_Primitives_TextBoxBase_IsUndoEnabled
                        //"Setting this property to false clears the undo stack"
                        tbb.IsUndoEnabled = false;
                        tbb.IsUndoEnabled = true;
                    }
                }
            }

            //disable the components
            foreach (Control control in controlsToDisable)
                control.IsEnabled = false;

            //process controls dependent on which view you're currently in (tab view)
            //(essentially this enables the tab, search and add/move/remove buttons when database view tab (left) is selected 
            LeftTabView_SelectionChanged(null, null);

            //enable components by type
            //package null = category
            if (resetForComponent)
            {
                if (package == null)
                {
                    foreach (FrameworkElement control in UiUtils.GetAllWindowComponentsLogical(DependenciesTab, false))
                    {
                        if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                            control.IsEnabled = true;
                    }
                    PackageNameDisplay.IsEnabled = true;
                    PackageMaintainersDisplay.IsEnabled = true;
                    ApplyButton.IsEnabled = true;
                    CategoryOffsetInstallGroupDisplay.IsEnabled = true;
                }
                else if (package is DatabasePackage)
                {
                    //basic tab is always difficult
                    PackagePackageNameDisplay.IsEnabled = true;
                    PackageMaintainersDisplay.IsEnabled = true;
                    PackageZipFileDisplay.IsEnabled = true;
                    PackageDevURLDisplay.IsEnabled = true;
                    PackageVersionDisplay.IsEnabled = true;
                    PackageAuthorDisplay.IsEnabled = true;
                    PackageInstallGroupDisplay.IsEnabled = true;
                    PackagePatchGroupDisplay.IsEnabled = true;
                    PackageUidDisplay.IsEnabled = true;
                    PackageLastUpdatedDisplay.IsEnabled = true;
                    PackageLogAtInstallDisplay.IsEnabled = true;
                    PackageEnabledDisplay.IsEnabled = true;//kinda meta
                    PackageDeprecatedDisplay.IsEnabled = true;
                    PackageMinimalistModeExcludeDisplay.IsEnabled = true;
                    ApplyButton.IsEnabled = true;
                    ZipDownload.IsEnabled = true;
                    ZipUload.IsEnabled = true;
                    PackageJustCheckedForUpdateButton.IsEnabled = true;

                    //all have internal notes and triggers
                    foreach (FrameworkElement control in UiUtils.GetAllWindowComponentsLogical(TriggersTab, false))
                    {
                        if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                            control.IsEnabled = true;
                    }
                    foreach (FrameworkElement control in UiUtils.GetAllWindowComponentsLogical(InternalNotesTab, false))
                    {
                        if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                            control.IsEnabled = true;
                    }
                    foreach (FrameworkElement control in UiUtils.GetAllWindowComponentsLogical(TagsTab, false))
                    {
                        if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                            control.IsEnabled = true;
                    }
                    foreach (FrameworkElement control in UiUtils.GetAllWindowComponentsLogical(PatchesTab, false))
                    {
                        if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                            control.IsEnabled = true;
                    }
                    foreach (FrameworkElement control in UiUtils.GetAllWindowComponentsLogical(XmlUnpackShortcutTab, false))
                    {
                        if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                            control.IsEnabled = true;
                    }
                    foreach (FrameworkElement control in UiUtils.GetAllWindowComponentsLogical(AtlasesTab, false))
                    {
                        if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                            control.IsEnabled = true;
                    }

                    if (package is Dependency dependency || package is SelectablePackage spackage)
                    {
                        //dependency and selectable package both have dependencies
                        foreach (FrameworkElement control in UiUtils.GetAllWindowComponentsLogical(DependenciesTab, false))
                        {
                            if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                                control.IsEnabled = true;
                        }

                        //conflicting packages gets used for showing elements that are used by the dependency
                        foreach (FrameworkElement control in UiUtils.GetAllWindowComponentsLogical(ConflictingPackagesTab, false))
                        {
                            if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                                control.IsEnabled = true;
                        }

                        if (package is SelectablePackage)
                        {
                            //enable remaining elements on basic tab
                            PackageNameDisplay.IsEnabled = true;
                            PackageTypeDisplay.IsEnabled = true;
                            PackageVisibleDisplay.IsEnabled = true;
                            PackagePopularModDisplay.IsEnabled = true;
                            PackageShowInSearchListDisplay.IsEnabled = true;
                            PackageGreyAreaModDisplay.IsEnabled = true;
                            PackageObfuscatedModDisplay.IsEnabled = true;
                            PackageFromWGmodsDisplay.IsEnabled = true;

                            //enable remaining tabs
                            foreach (FrameworkElement control in UiUtils.GetAllWindowComponentsLogical(DescriptionTab, false))
                            {
                                if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                                    control.IsEnabled = true;
                            }
                            foreach (FrameworkElement control in UiUtils.GetAllWindowComponentsLogical(UpdateNotesTab, false))
                            {
                                if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                                    control.IsEnabled = true;
                            }
                            foreach (FrameworkElement control in UiUtils.GetAllWindowComponentsLogical(MediasTab, false))
                            {
                                if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                                    control.IsEnabled = true;
                            }
                            foreach (FrameworkElement control in UiUtils.GetAllWindowComponentsLogical(UserDatasTab, false))
                            {
                                if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                                    control.IsEnabled = true;
                            }
                        }
                    }
                }
            }

            //reload the list of all dependencies to make sure it's always accurate
            LoadedDependenciesList.Items.Clear();
            foreach (Dependency d in Dependencies)
                LoadedDependenciesList.Items.Add(d);
        }

        private void PackageDisplayUrlParse_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is TextBox))
            {
                Logging.Editor("[MouseDoubleClick]: The MouseDoubleClick has been registered to an invalid component! {0}", LogLevel.Error, sender.GetType().ToString());
                return;
            }
            TextBox sender_ = (TextBox)sender;

            //split the textbox into http arrays
            string[] httpSplit = sender_.Text.Split(new string[] { "http" }, StringSplitOptions.RemoveEmptyEntries);
            int stringSplitLength = "http".Length;

            //find out where the curser selection is
            int sectionHttpStart = 0;
            int sectionHttpEnd = 0;
            int sectionSplitEnd = 0;
            for(int i = 0; i < httpSplit.Count(); i++)
            {
                string httpParse = httpSplit[i];

                //do a regex split to get the end words
                string[] httpParseRegex = Regex.Split(httpParse, @"\s");
                string httpParseRegexStart = httpParseRegex[0];
                //string httpParseRegexEnd = httpParseRegex[1];

                //get word index calculations with reference to where the curser selection is
                sectionHttpStart = sectionSplitEnd;
                sectionHttpEnd = sectionHttpStart + httpParseRegexStart.Length;
                sectionSplitEnd = sectionHttpStart + httpParse.Length;
                if (i > 0)
                    sectionSplitEnd += stringSplitLength;
                Logging.Editor("[MouseDoubleClick]: sectionHttpStart: {0}, sectionHttpEnd: {1}, sectionSplitEnd: {2}, selectionStart{3}",
                    LogLevel.Debug, sectionHttpStart, sectionHttpEnd, sectionSplitEnd, sender_.SelectionStart);

                if(sectionHttpStart <= sender_.SelectionStart && sender_.SelectionStart < sectionHttpEnd)
                {
                    Logging.Editor("[MouseDoubleClick]: Valid http index found to parse: {0}", LogLevel.Debug, i);
                    //split on end via whitespace character
                    string parsedHttpLink = Regex.Split(httpParse, @"\s")[0];
                    parsedHttpLink = string.Format("{0}{1}", "http", parsedHttpLink);
                    Logging.Editor("[MouseDoubleClick]: Parsed http string: {0}", LogLevel.Info, parsedHttpLink);
                    try
                    {
                        System.Diagnostics.Process.Start(parsedHttpLink);
                    }
                    catch
                    {
                        Logging.Editor("[MouseDoubleClick]: Failed to open DevURL {0}", LogLevel.Info, parsedHttpLink);
                    }
                    break;
                }
            }
            if(sectionSplitEnd == 0)
            {
                Logging.Editor("[MouseDoubleClick]: Never found an 'http' sequence to begin parsing", LogLevel.Info);
            }
        }

        #endregion

        #region Show database methods
        private void DatabaseTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //set handled parameter so that the parent events don't fire
            e.Handled = true;

            //check to make sure it's a TreeViewItem (should always be)
            if (DatabaseTreeView.SelectedItem is TreeViewItem selectedTreeViewItem)
            {
                //if the mouse is not over, then it was not user initiated
                bool anyUserKyesDown = Keyboard.IsKeyDown(Key.Enter) || Keyboard.IsKeyDown(Key.Up) || Keyboard.IsKeyDown(Key.Down) || Keyboard.IsKeyDown(Key.Left) || Keyboard.IsKeyDown(Key.Right);
                if (!(selectedTreeViewItem.IsMouseOver || anyUserKyesDown))
                    return;
                Logging.Editor("SelectedItemChanged(), selectedTreeViewItem.Header={0}", LogLevel.Info, selectedTreeViewItem.Header);
                SelectDatabaseObject(selectedTreeViewItem.Header, e.OldValue as TreeViewItem);
            }
        }

        private void SelectDatabaseObject(object obj, TreeViewItem previousTreeViewItemOfSelectedItem)
        {
            //check if we should save the item before updating what the current entry is
            if (EditorSettings.SaveSelectionBeforeLeave)
            {
                //only try to save database object if selected item is of correct type
                if (SelectedItem is EditorComboBoxItem || SelectedItem is IDatabaseComponent)
                {
                    if (ApplyDatabaseObject(SelectedItem))
                    {
                        if (previousTreeViewItemOfSelectedItem != null)
                        {
                            //trigger a UI update
                            object tempRef = previousTreeViewItemOfSelectedItem.Header;
                            previousTreeViewItemOfSelectedItem.Header = null;
                            previousTreeViewItemOfSelectedItem.Header = tempRef;
                        }
                    }
                    else
                    {
                        Logging.Editor("applyDatabaseObject failed, not changing entry");
                        //only start the hack when it's supposed to be used as a revert for changing back to a package selection
                        ReselectOldItem.Start();
                        return;
                    }
                }
                else
                {
                    Logging.Editor("SelectedItem is of wrong type, not counting for save before leave ()", LogLevel.Info, (SelectedItem == null? "(null)" : SelectedItem.ToString()));
                }
            }
            SelectedItem = obj;
            ShowDatabaseObject(SelectedItem);
        }

        private void ShowDatabaseObject(object obj)
        {
            if (obj is Category category)
                ShowDatabaseCategory(category);
            else if (obj is DatabasePackage package)
                ShowDatabasePackage(package);
            else if (obj is EditorComboBoxItem editorComboBoxItem)
                ShowDatabasePackage(editorComboBoxItem.Package);
            else
            {
                //it's one of those string filler things
                ResetRightPanels(null, false);
            }
        }

        private void ShowDatabaseCategory(Category category)
        {
            ResetRightPanels(null);
            Logging.Editor("ShowDatabaseCategory(), category showing = {0}", LogLevel.Info, category.Name);
            foreach (DatabaseLogic logic in category.Dependencies)
                PackageDependenciesDisplay.Items.Add(DatabaseLogic.Copy(logic));
            PackageNameDisplay.Text = category.Name;
            PackageMaintainersDisplay.Text = category.Maintainers;
            CategoryOffsetInstallGroupDisplay.IsChecked = category.OffsetInstallGroups;
        }

        private void ShowDatabasePackage(DatabasePackage package)
        {
            ResetRightPanels(package);
            Logging.Editor("ShowDatabaseObject(), package showing = {0}", LogLevel.Info, package.PackageName);
            //load all items in the databasePackage level first
            //basic tab
            //set text field texts
            PackagePackageNameDisplay.Text = package.PackageName;
            PackageMaintainersDisplay.Text = package.Maintainers;
            PackageZipFileDisplay.Text = package.ZipFile;
            PackageVersionDisplay.Text = package.Version;
            PackageAuthorDisplay.Text = package.Author;
            PackageUidDisplay.Text = package.UID;
            PackageLastUpdatedDisplay.Text = CommonUtils.ConvertFiletimeTimestampToDate(package.Timestamp);
            PackageLastCheckForUpdateDisplay.Text = CommonUtils.ConvertFiletimeTimestampToDate(package.LastUpdateCheck);
            PackageLastCheckForUpdateDisplay.Tag = package.LastUpdateCheck;

            //locate and select the patchGroup and installGroup of the package
            //if it can't, then extend the number of options until its there
            bool wasSelected = false;
            foreach (int i in PackageInstallGroupDisplay.Items)
            {
                if (i == package.InstallGroup)
                {
                    PackageInstallGroupDisplay.SelectedItem = i;
                    wasSelected = true;
                    break;
                }
            }
            if(!wasSelected)
            {
                int lastValue = (int)PackageInstallGroupDisplay.Items[PackageInstallGroupDisplay.Items.Count - 1];
                while (lastValue <= package.InstallGroup)
                {
                    PackageInstallGroupDisplay.Items.Add(++lastValue);
                }
            }

            wasSelected = false;
            foreach (int i in PackagePatchGroupDisplay.Items)
            {
                if (i == package.PatchGroup)
                {
                    PackagePatchGroupDisplay.SelectedItem = i;
                    break;
                }
            }
            if (!wasSelected)
            {
                int lastValue = (int)PackagePatchGroupDisplay.Items[PackagePatchGroupDisplay.Items.Count - 1];
                while (lastValue <= package.PatchGroup)
                {
                    PackagePatchGroupDisplay.Items.Add(++lastValue);
                }
            }

            //some checkboxes
            PackageLogAtInstallDisplay.IsChecked = package.LogAtInstall;
            PackageEnabledDisplay.IsChecked = package.Enabled;
            PackageDeprecatedDisplay.IsChecked = package.Deprecated;
            PackageMinimalistModeExcludeDisplay.IsChecked = package.MinimalistModeExclude;

            //devURL
            PackageDevURLDisplay.Text = MacroUtils.MacroReplace(package.DevURL,ReplacementTypes.TextUnescape);

            //internal notes
            PackageInternalNotesDisplay.Text = package.InternalNotesEscaped;

            //triggers (the lists were already cleared)
            foreach (string s in package.TriggersList)
                PackageTriggersDisplay.Items.Add(s);

            //tags (list was cleared like triggers from ResetRightPanels()
            foreach (PackageTags packageTag in package.Tags)
                PackageTagsDisplay.Items.Add(packageTag);

            //patches
            foreach (Patch patch in package.Patches)
                PackagePatchesDisplay.Items.Add(patch);

            //atlases
            foreach (Atlas atlas in package.Atlases)
                PackageAtlasesDisplay.Items.Add(atlas);

            //shortcuts
            foreach (Shortcut shortcut in package.Shortcuts)
                PackageShortcutDisplay.Items.Add(shortcut);

            //xml unpacks
            foreach (XmlUnpack xmlUnpack in package.XmlUnpacks)
                PackageXmlUnpackDisplay.Items.Add(xmlUnpack);

            //then handle if dependency
            if (package is Dependency dependency)
            {
                //display all dependencies that the selected dependency uses
                foreach (DatabaseLogic d in dependency.Dependencies)
                {
                    PackageDependenciesDisplay.Items.Add(DatabaseLogic.Copy(d));
                }

                //change the "conflicting packages" tab into a "dependency usage" tab
                ConflictingPackagesTab.Header = "Dependency Usage";
                ConflictingPackagesMessagebox.Text = "List packages that use this dependency";

                //set the property to use for the components added inside it
                PackageConflictingPackagesDisplay.DisplayMemberPath = "ComponentInternalName";

                //display all the dependencies and packages that use the selected dependency
                //check dependencies that use this dependency
                foreach (Dependency dependencyy in Dependencies)
                {
                    //don't add itself
                    if (dependencyy.Equals(dependency))
                        continue;

                    foreach (DatabaseLogic logic in dependencyy.Dependencies)
                        if (logic.PackageName.Equals(dependency.PackageName))
                            //the fact i'm not breaking can help determine if a package has the dependency listed twice
                            PackageConflictingPackagesDisplay.Items.Add(dependencyy);
                }

                //check selectablePackages that use this dependency
                foreach (SelectablePackage selectablePackage in databaseManager.GetFlatSelectablePackageList())
                {
                    foreach (DatabaseLogic logic in selectablePackage.Dependencies)
                        if (logic.PackageName.Equals(dependency.PackageName))
                            PackageConflictingPackagesDisplay.Items.Add(selectablePackage);
                }

                //check categories that use this dependency
                foreach(Category cat in ParsedCategoryList)
                {
                    foreach(DatabaseLogic logic in cat.Dependencies)
                    {
                        if (logic.PackageName.Equals(dependency.PackageName))
                            PackageConflictingPackagesDisplay.Items.Add(cat);
                    }
                }

                //also disable the "remove conflicting package" button since it won't work for these
                ConflictingPackagesRemoveConflictingPackage.IsEnabled = false;
            }
            //then handle if selectalbePackage
            else if (package is SelectablePackage selectablePackage)
            {
                PackagePopularModDisplay.IsChecked = selectablePackage.PopularMod;
                PackageGreyAreaModDisplay.IsChecked = selectablePackage.GreyAreaMod;
                PackageObfuscatedModDisplay.IsChecked = selectablePackage.ObfuscatedMod;
                PackageFromWGmodsDisplay.IsChecked = selectablePackage.FromWGmods;
                PackageShowInSearchListDisplay.IsChecked = selectablePackage.ShowInSearchList;
                PackageNameDisplay.Text = selectablePackage.Name;
                PackageTypeDisplay.SelectedItem = selectablePackage.Type;
                PackageLevelDisplay.Text = selectablePackage.Level.ToString();
                PackageCalculatedLevelDisplay.Text = selectablePackage.InstallGroupWithOffset.ToString();
                PackageVisibleDisplay.IsChecked = selectablePackage.Visible;
                PackageDescriptionDisplay.Text = selectablePackage.DescriptionEscaped;
                PackageUpdateNotesDisplay.Text = selectablePackage.UpdateCommentEscaped;

                foreach (DatabaseLogic d in selectablePackage.Dependencies)
                {
                    PackageDependenciesDisplay.Items.Add(DatabaseLogic.Copy(d));
                }

                foreach (Media media in selectablePackage.Medias)
                {
                    PackageMediasDisplay.Items.Add(Media.Copy(media));
                }

                foreach (UserFile data in selectablePackage.UserFiles)
                {
                    PackageUserFilesDisplay.Items.Add(data.Pattern);
                }

                //reset the property to use for the components added inside it
                PackageConflictingPackagesDisplay.Items.Clear();

                foreach (ConflictingPackage conflictingPackage in selectablePackage.ConflictingPackagesNew)
                {
                    PackageConflictingPackagesDisplay.Items.Add(new ListBoxItem() { Tag = conflictingPackage, Content = conflictingPackage.ToString() });
                }

                //set the conflicting packages tab
                ConflictingPackagesTab.Header = "Conflicting Packages";
                ConflictingPackagesMessagebox.Text = "To add a package to the list, search it above and right click it";
                ConflictingPackagesRemoveConflictingPackage.IsEnabled = true;
            }
        }
        #endregion

        #region Apply database methods
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            //check if we should ask a confirm first
            if (EditorSettings.ShowConfirmationOnPackageApply)
            {
                if (MessageBox.Show("Confirm to apply changes?", "", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    return;
            }
            //first make sure databaseTreeView selected item is treeviewitem
            if (DatabaseTreeView.SelectedItem is TreeViewItem selectedTreeViewItem)
            {
                ApplyDatabaseObject(selectedTreeViewItem.Header);
                //trigger a UI update
                object tempRef = selectedTreeViewItem.Header;
                selectedTreeViewItem.Header = null;
                selectedTreeViewItem.Header = tempRef;
            }
        }

        private bool ApplyDatabaseObject(object obj)
        {
            bool saveApplied = false;
            if (obj is Category category)
                saveApplied = ApplyDatabaseCategory(category);
            else if (obj is DatabasePackage package)
                saveApplied = ApplyDatabasePackage(package);
            else if (obj is EditorComboBoxItem editorComboBoxItem)
                saveApplied = ApplyDatabasePackage(editorComboBoxItem.Package);

            //if user requests apply to also save to disk, then do that now
            if (saveApplied)
            {
                if (EditorSettings.ApplyBehavior == ApplyBehavior.ApplyTriggersSave)
                {
                    SaveDatabaseButton_Click(null, null);
                }
            }
            else
            {
                Logging.Editor("apply failed, not saving", LogLevel.Warning);
            }
            return saveApplied;
        }

        private bool ApplyDatabaseCategory(Category category)
        {
            Logging.Editor("ApplyDatabaseCategory(), category saving= {0}", LogLevel.Info, category.Name);

            //check if any changes were actually made
            if (CategoryWasModified(category))
            {
                Logging.Editor("Category was modified, saving and setting flag");
                category.Name = PackageNameDisplay.Text;
                category.Maintainers = PackageMaintainersDisplay.Text;
                category.OffsetInstallGroups = (bool)CategoryOffsetInstallGroupDisplay.IsChecked;
                category.Dependencies.Clear();
                foreach (DatabaseLogic logic in PackageDependenciesDisplay.Items)
                    category.Dependencies.Add(DatabaseLogic.Copy(logic));

                //there now are unsaved changes
                UnsavedChanges = true;
            }
            else
                Logging.Editor("Category was not modified, no change to set");
            return true;
        }

        private bool ApplyDatabasePackage(DatabasePackage package)
        {
            Logging.Editor("ApplyDatabasePackage(), package saving = {0}", LogLevel.Info, package.PackageName);

            //check if the to save packagename is unique
            if (!PackagePackageNameDisplay.Text.Equals(package.PackageName))
            {
                Logging.Editor("PackageName is new, checking if it is unique");
                if (databaseManager.IsDuplicatePackageName(PackagePackageNameDisplay.Text))
                {
                    MessageBox.Show(string.Format("Duplicate packageName: {0} is already used", PackagePackageNameDisplay.Text));
                    return false;
                }
            }

            //check if package was actually modified before saving all these delicious properties
            if (!PackageWasModified(package))
            {
                Logging.Editor("Package was not modified, don't apply anything");
                return true;
            }

            //only check for missing zip file if there is text be begin with
            if (!string.IsNullOrWhiteSpace(PackageZipFileDisplay.Text))
            {
                if (!package.ZipFile.Equals(PackageZipFileDisplay.Text) && !Path.GetExtension(PackageZipFileDisplay.Text).Equals(".zip"))
                {
                    MessageBox.Show("No zip in file extension, was this a mistake?");
                    return false;
                }
            }

            //check if package UID changed. it shouldn't
            if (!package.UID.Equals(PackageUidDisplay.Text))
            {
                Logging.Editor("The package UID changed. Old = {0}, New = {1}", LogLevel.Error, package.UID, PackageUidDisplay);
                return false;
            }

            Logging.Editor("Package was modified, saving changes to memory and setting changes switch");

            //save everything from the UI into the package
            //save package elements first
            package.PackageName = PackagePackageNameDisplay.Text;
            package.Maintainers = PackageMaintainersDisplay.Text;

            //devURL is separated by newlines for array list, so it's not necessary to escape
            package.DevURL = MacroUtils.MacroReplace(PackageDevURLDisplay.Text, ReplacementTypes.TextEscape);
            package.Version = PackageVersionDisplay.Text;
            package.Author = PackageAuthorDisplay.Text;
            package.InstallGroup = (int)PackageInstallGroupDisplay.SelectedItem;
            package.PatchGroup = (int)PackagePatchGroupDisplay.SelectedItem;
            package.LogAtInstall = (bool)PackageLogAtInstallDisplay.IsChecked;
            package.Enabled = (bool)PackageEnabledDisplay.IsChecked;
            package.Deprecated = (bool)PackageDeprecatedDisplay.IsChecked;
            package.MinimalistModeExclude = (bool)PackageMinimalistModeExcludeDisplay.IsChecked;
            package.InternalNotes = MacroUtils.MacroReplace(PackageInternalNotesDisplay.Text, ReplacementTypes.TextEscape);
            package.Triggers = string.Join(",", PackageTriggersDisplay.Items.Cast<string>());
            package.Tags.Clear();
            package.Tags.AddRange(PackageTagsDisplay.Items.Cast<PackageTags>());

            //if the zipfile was updated, then update the last modified date
            if (!package.ZipFile.Equals(PackageZipFileDisplay.Text))
            {
                package.UpdateZipfile(PackageZipFileDisplay.Text);
            }

            //this gets dependencies and selectable packages
            if (package is IComponentWithDependencies componentWithDependencies)
            {
                componentWithDependencies.Dependencies.Clear();
                foreach (DatabaseLogic dl in PackageDependenciesDisplay.Items)
                {
                    componentWithDependencies.Dependencies.Add(DatabaseLogic.Copy(dl));
                }
            }

            //add the list items by making new versions
            package.Patches.Clear();
            foreach (Patch component in PackagePatchesDisplay.Items)
                package.Patches.Add(new Patch(component));

            package.XmlUnpacks.Clear();
            foreach (XmlUnpack component in PackageXmlUnpackDisplay.Items)
                package.XmlUnpacks.Add(new XmlUnpack(component));

            package.Atlases.Clear();
            foreach (Atlas component in PackageAtlasesDisplay.Items)
                package.Atlases.Add(new Atlas(component));

            package.Shortcuts.Clear();
            foreach (Shortcut component in PackageShortcutDisplay.Items)
                package.Shortcuts.Add(new Shortcut(component));

            //if it's a selectablePackage
            if (package is SelectablePackage selectablePackage)
            {
                selectablePackage.ShowInSearchList = (bool)PackageShowInSearchListDisplay.IsChecked;
                selectablePackage.PopularMod = (bool)PackagePopularModDisplay.IsChecked;
                selectablePackage.ObfuscatedMod = (bool)PackageObfuscatedModDisplay.IsChecked;
                selectablePackage.GreyAreaMod = (bool)PackageGreyAreaModDisplay.IsChecked;
                selectablePackage.FromWGmods = (bool)PackageFromWGmodsDisplay.IsChecked;
                selectablePackage.Visible = (bool)PackageVisibleDisplay.IsChecked;
                selectablePackage.Name = PackageNameDisplay.Text;
                selectablePackage.Type = (SelectionTypes)PackageTypeDisplay.SelectedItem;
                selectablePackage.Description = MacroUtils.MacroReplace(PackageDescriptionDisplay.Text,ReplacementTypes.TextEscape);
                selectablePackage.UpdateComment = MacroUtils.MacroReplace(PackageUpdateNotesDisplay.Text,ReplacementTypes.TextEscape);

                selectablePackage.ConflictingPackagesNew.Clear();
                foreach (ListBoxItem listBoxItem in PackageConflictingPackagesDisplay.Items)
                {
                    selectablePackage.ConflictingPackagesNew.Add(new ConflictingPackage(listBoxItem.Tag as ConflictingPackage));
                }

                selectablePackage.UserFiles.Clear();
                foreach (string uf in PackageUserFilesDisplay.Items)
                {
                    selectablePackage.UserFiles.Add(new UserFile() { Pattern = uf });
                }

                selectablePackage.Medias.Clear();
                foreach (Media m in PackageMediasDisplay.Items)
                {
                    selectablePackage.Medias.Add(Media.Copy(m));
                }
            }

            //there now are unsaved changes
            UnsavedChanges = true;
            return true;
        }
        #endregion

        #region Checks if a package was modified
        private bool DependenciesWereModified(List<DatabaseLogic> dependencies)
        {
            //check if counts are equal. if not, then modifications exist
            if (dependencies.Count() != PackageDependenciesDisplay.Items.Count)
                return true;

            int i = 0;
            //check packagename, notflag, logic
            foreach (DatabaseLogic logic in PackageDependenciesDisplay.Items)
            {
                DatabaseLogic logicInDatabase = dependencies[i];
                if (!logic.NotFlag.Equals(logicInDatabase.NotFlag))
                    return true;
                if (!logic.Logic.Equals(logicInDatabase.Logic))
                    return true;
                if (!logic.PackageName.Equals(logicInDatabase.PackageName))
                    return true;
                i++;
            }

            return false;
        }

        private bool TriggersWereModified(List<string> triggers)
        {
            if (triggers.Count != PackageTriggersDisplay.Items.Count)
                return true;

            int i = 0;
            foreach (string trigger in PackageTriggersDisplay.Items)
            {
                if (!trigger.Equals(triggers[i]))
                    return true;
                i++;
            }

            return false;
        }

        private bool TagsWereModified(PackageTagsList packageTagsList)
        {
            //first check if the list counts differ
            if (packageTagsList.Count() != PackageTagsDisplay.Items.Count)
                return true;

            //the order could have changed, which is still worth noting
            //check between the UI box and the package to save, tag by tag
            int i = 0;
            foreach (PackageTags packageTag in PackageTagsDisplay.Items)
            {
                if (!packageTag.Equals(packageTagsList[i]))
                    return true;

                i++;
            }

            //welp guess they're equal
            return false;
        }

        private bool UserFilesWereModified(List<UserFile> userFiles)
        {
            if (userFiles.Count != PackageUserFilesDisplay.Items.Count)
                return true;

            int i = 0;
            foreach (string file in PackageUserFilesDisplay.Items)
            {
                if (!file.Equals(userFiles[i].Pattern))
                    return true;
                i++;
            }

            return false;
        }

        private bool MediasModified(List<Media> Medias)
        {
            if (Medias.Count != PackageMediasDisplay.Items.Count)
                return true;

            int i = 0;
            foreach (Media media in PackageMediasDisplay.Items)
            {
                Media mediaInDatabase = Medias[i];
                if (!media.URL.Equals(mediaInDatabase.URL))
                    return true;
                if (!media.MediaType.Equals(mediaInDatabase.MediaType))
                    return true;
                i++;
            }

            return false;
        }

        private bool ConflictingPackagesModified(List<ConflictingPackage> conflictingPackages)
        {
            if (conflictingPackages.Count != PackageConflictingPackagesDisplay.Items.Count)
                return true;

            int i = 0;
            foreach (ListBoxItem conflict in PackageConflictingPackagesDisplay.Items)
            {
                ConflictingPackage packageInListbox = conflict.Tag as ConflictingPackage;
                if (!packageInListbox.IsEqual(conflictingPackages[i]))
                    return true;
                i++;
            }

            return false;
        }

        private bool InstructionsModified(List<Instruction> instructions, ItemCollection listboxCollection)
        {
            if (instructions.Count != listboxCollection.Count)
                return true;

            for (int i = 0; i < instructions.Count; i++)
            {
                Instruction instructionFromEditorList = listboxCollection[i] as Instruction;
                Instruction instructionFromDatabase = instructions[i];

                if (!instructionFromDatabase.InstructionsEqual(instructionFromEditorList))
                    return true;
            }

            return false;
        }

        private bool CategoryWasModified(Category category)
        {
            if (!category.Name.Equals(PackageNameDisplay.Text))
                return true;

            if (!category.Maintainers.Equals(PackageMaintainersDisplay.Text))
                return true;

            if (category.OffsetInstallGroups != ((bool)CategoryOffsetInstallGroupDisplay.IsChecked))
                return true;

            if (DependenciesWereModified(category.Dependencies))
                return true;

            return false;
        }

        private bool PackageWasModified(DatabasePackage package)
        {
            //save everything from the UI into the package
            //save package elements first
            if (!package.PackageName.Equals(PackagePackageNameDisplay.Text))
                return true;
            if (!package.Maintainers.Equals(PackageMaintainersDisplay.Text))
                return true;

            //devURL is separated by newlines for array list, so it's not necessary to escape
            if (!package.DevURL.Equals(MacroUtils.MacroReplace(PackageDevURLDisplay.Text, ReplacementTypes.TextEscape)))
                return true;
            if (!package.Version.Equals(PackageVersionDisplay.Text))
                return true;
            if (!package.Author.Equals(PackageAuthorDisplay.Text))
                return true;
            if (!package.InstallGroup.Equals((int)PackageInstallGroupDisplay.SelectedItem))
                return true;
            if (!package.PatchGroup.Equals((int)PackagePatchGroupDisplay.SelectedItem))
                return true;
            if (!package.LogAtInstall.Equals((bool)PackageLogAtInstallDisplay.IsChecked))
                return true;
            if (!package.Enabled.Equals((bool)PackageEnabledDisplay.IsChecked))
                return true;
            if (!package.Deprecated.Equals((bool)PackageDeprecatedDisplay.IsChecked))
                return true;
            if (!package.MinimalistModeExclude.Equals((bool)PackageMinimalistModeExcludeDisplay.IsChecked))
                return true;
            if (!package.InternalNotes.Equals(MacroUtils.MacroReplace(PackageInternalNotesDisplay.Text, ReplacementTypes.TextEscape)))
                return true;
            if (!package.ZipFile.Equals(PackageZipFileDisplay.Text))
                return true;
            if (package.LastUpdateCheck != (long)PackageLastCheckForUpdateDisplay.Tag)
                return true;

            if (TriggersWereModified(package.TriggersList))
                return true;

            if (TagsWereModified(package.Tags))
                return true;

            if (package is IComponentWithDependencies componentWithDependencies)
            {
                if (DependenciesWereModified(componentWithDependencies.Dependencies))
                    return true;
            }

            if (InstructionsModified(package.Patches.Cast<Instruction>().ToList(), PackagePatchesDisplay.Items))
                return true;

            if (InstructionsModified(package.Atlases.Cast<Instruction>().ToList(), PackageAtlasesDisplay.Items))
                return true;

            if (InstructionsModified(package.XmlUnpacks.Cast<Instruction>().ToList(), PackageXmlUnpackDisplay.Items))
                return true;

            if (InstructionsModified(package.Shortcuts.Cast<Instruction>().ToList(), PackageShortcutDisplay.Items))
                return true;

            //see if it's a selectablePackage
            if (package is SelectablePackage selectablePackage)
            {
                if (selectablePackage.ShowInSearchList != ((bool)PackageShowInSearchListDisplay.IsChecked))
                    return true;
                if (selectablePackage.PopularMod != ((bool)PackagePopularModDisplay.IsChecked))
                    return true;
                if (selectablePackage.GreyAreaMod != ((bool)PackageGreyAreaModDisplay.IsChecked))
                    return true;
                if (selectablePackage.ObfuscatedMod != ((bool)PackageObfuscatedModDisplay.IsChecked))
                    return true;
                if (selectablePackage.Visible != ((bool)PackageVisibleDisplay.IsChecked))
                    return true;
                if (selectablePackage.FromWGmods != ((bool)PackageFromWGmodsDisplay.IsChecked))
                    return true;

                if (!selectablePackage.Name.Equals(PackageNameDisplay.Text))
                    return true;
                if (!selectablePackage.Type.Equals((SelectionTypes)PackageTypeDisplay.SelectedItem))
                    return true;
                if (!selectablePackage.DescriptionEscaped.Equals(PackageDescriptionDisplay.Text))
                    return true;
                if (!selectablePackage.UpdateCommentEscaped.Equals(PackageUpdateNotesDisplay.Text))
                    return true;

                if (UserFilesWereModified(selectablePackage.UserFiles))
                    return true;

                if (MediasModified(selectablePackage.Medias))
                    return true;

                if (ConflictingPackagesModified(selectablePackage.ConflictingPackagesNew))
                    return true;
            }
            return false;
        }
        #endregion

        #region Drag Drop code for treeviews

        private void PerformDatabaseMoveAdd(TreeViewItem itemCurrentlyOver, TreeViewItem itemToMove, TreeViewItem parentItemToMove, TreeViewItem parentItemOver,
            DatabasePackage packageToMove, DatabasePackage packageCurrentlyOver, DragDropEffects effects, bool addBelowItem)
        {
            Logging.Editor("Starting PerformDatabaseMoveAdd function, itemCurrentlyOver={0}, itemToMove={1}, parentItemToMove={2}, parentItemOver={3}, packageToMove={4}," +
                " packageCurrentlyOver={5}, effects={6}, addBelowItem={7}", LogLevel.Info, itemCurrentlyOver.ToString(), itemToMove.ToString(), parentItemToMove.ToString(), parentItemOver.ToString(),
                packageToMove.PackageName, packageCurrentlyOver.PackageName, effects.ToString(), addBelowItem.ToString());

            //make sure that the source and destination are not the same
            if (packageCurrentlyOver.Equals(packageToMove) && !addBelowItem)
            {
                Logging.Editor("Package to add/move is the same as package currently over, aborting operation");
                return;
            }

            //if it's a move operation, then remove the element from it's original list
            if (effects == DragDropEffects.Move)
            {
                Logging.Editor("Effects is move, removing {0} from parent", LogLevel.Info, packageToMove.PackageName);
                if (packageToMove is SelectablePackage selectablePackageToMove)
                    selectablePackageToMove.Parent.Packages.Remove(selectablePackageToMove);
                else if (packageToMove is Dependency dependencyToMove)
                    Dependencies.Remove(dependencyToMove);
                else
                    GlobalDependencies.Remove(packageToMove);
            }

            //if it's a copy operation, then make a deep copy new element
            //default to make a selectablePackage copy, then cast down as needed
            //then assign it back to packageToMove
            if (effects == DragDropEffects.Copy)
            {
                Logging.Editor("Effects is copy, making new copy instance of {0}", LogLevel.Info, packageToMove.PackageName);
                if (packageCurrentlyOver is SelectablePackage)
                {
                    packageToMove = new SelectablePackage (packageToMove, false);
                }
                else if (packageCurrentlyOver is Dependency)
                {
                    packageToMove = new Dependency(packageToMove, false);
                }
                else
                {
                    packageToMove = new DatabasePackage(packageToMove);
                }

                //also make a new UID for the package as well
                packageToMove.UID = CommonUtils.GenerateUID(databaseManager.GetFlatList());

                //the packageName needs to stay unique as well
                int i = 0;
                string origName = packageToMove.PackageName;
                while (databaseManager.GetFlatList().Where(package => package.PackageName.Equals(packageToMove.PackageName)).Count() > 0)
                    packageToMove.PackageName = string.Format("{0}_{1}", origName, i++);
                Logging.Editor("New package name is {0}", LogLevel.Info, packageToMove.PackageName);
            }

            Logging.Editor("For insert process, packageCurrentlyOver type is {0}, packageToMove type is {1}", LogLevel.Info, packageCurrentlyOver.GetType().Name, packageToMove.GetType().Name);
            //insert packageToMove into corresponding list that it's over
            if (packageCurrentlyOver is SelectablePackage selectablePackageCurrentlyOverForInsert)
            {
                //we need to make a new item if it's subclassing. can't cast into a subclass
                if (!(packageToMove is SelectablePackage))
                    packageToMove = new SelectablePackage(packageToMove, false);

                //unless alt is pressed to copy new item inside
                if (addBelowItem)
                    selectablePackageCurrentlyOverForInsert.Packages.Add((SelectablePackage)packageToMove);
                else
                    selectablePackageCurrentlyOverForInsert.Parent.Packages.Insert(selectablePackageCurrentlyOverForInsert.Parent.Packages.IndexOf(selectablePackageCurrentlyOverForInsert) + 1, (SelectablePackage)packageToMove);
            }
            else if (packageCurrentlyOver is Dependency dependnecyCurrentlyOverForInsert)
            {
                if (!(packageToMove is Dependency))
                    packageToMove = new Dependency(packageToMove, false);
                Dependencies.Insert(Dependencies.IndexOf(dependnecyCurrentlyOverForInsert) + 1, (Dependency)packageToMove);
            }
            else
            {
                if ((packageToMove is Dependency) || (packageToMove is SelectablePackage))
                    packageToMove = new DatabasePackage(packageToMove);
                GlobalDependencies.Insert(GlobalDependencies.IndexOf(packageCurrentlyOver) + 1, (DatabasePackage)packageToMove);
            }

            //at this point if the destination is a selectable package, then it's references need to be updated
            if (packageCurrentlyOver is SelectablePackage selectablePackageCurrentlyOver)
            {
                Logging.Editor("PackageCurrentlyOver is selectablePackage, updating references");
                //packageToMove needs to be casted to a SelectablePackage to have it's references updated
                SelectablePackage packageToMoveCast = (SelectablePackage)packageToMove;
                packageToMoveCast.TopParent = selectablePackageCurrentlyOver.TopParent;
                packageToMoveCast.ParentCategory = selectablePackageCurrentlyOver.ParentCategory;
                //if alt was used, it's inside the selectable package currently over
                if (addBelowItem)
                {
                    packageToMoveCast.Parent = selectablePackageCurrentlyOver;
                }
                else
                {
                    packageToMoveCast.Parent = selectablePackageCurrentlyOver.Parent;
                }
            }

            //and edit the tree view list
            Logging.Editor("Updating treeview");
            //same as before
            TreeViewItem realItemToMove = itemToMove;
            //if move, remove
            if (effects == DragDropEffects.Move)
                parentItemToMove.Items.Remove(realItemToMove);

            //if copy, copy
            if (effects == DragDropEffects.Copy)
            {
                realItemToMove = new TreeViewItem() { Header = new EditorComboBoxItem(packageToMove) };
                //and also add it to the new packageToMove
                packageToMove.EditorTreeViewItem = realItemToMove;
            }

            if (addBelowItem && packageCurrentlyOver is SelectablePackage)
            {
                itemCurrentlyOver.Items.Add(realItemToMove);
            }
            else
            {
                parentItemOver.Items.Insert(parentItemOver.Items.IndexOf(itemCurrentlyOver) + 1, realItemToMove);
            }

            SearchBox.Items.Clear();

            //rebuild the levels as well
            databaseManager.ProcessDatabase();

            //and keep focus over the item we just moved
            if (!realItemToMove.IsSelected)
            {
                //this will cause it in the UI to be highlighted, but internal selection code will reject it because it's not "user initiated"
                realItemToMove.IsSelected = true;
                //so make it programatically selected this one time
                SelectDatabaseObject(realItemToMove.Header, null);
            }
            UnsavedChanges = true;
        }

        private void OnTreeViewDatabaseDrop(object sender, DragEventArgs e)
        {
            if (!(sender is TreeView tv))
                return;
            TreeView treeView = (TreeView)sender;
            //reset the textbox
            DragDropTest.Text = "";
            DragDropTest.Visibility = Visibility.Hidden;
            ItemToExpand = null;
            DragDropTimer.Stop();
            //make sure the source and destination are tree view items
            if (e.Source is TreeViewItem itemCurrentlyOver && treeView.SelectedItem is TreeViewItem itemToMove)
            {
                //make sure source and destination have the correct header information
                if (itemCurrentlyOver.Header is EditorComboBoxItem editorPackageCurrentlyOver && itemToMove.Header is EditorComboBoxItem editorPackageToMove)
                {
                    //remove the treeviewItem from the UI list
                    //add the package to the new area (below)
                    if (itemToMove.Parent is TreeViewItem parentItemToMove && itemCurrentlyOver.Parent is TreeViewItem parentItemOver)
                    {
                        PerformDatabaseMoveAdd(itemCurrentlyOver, itemToMove, parentItemToMove, parentItemOver, editorPackageToMove.Package, editorPackageCurrentlyOver.Package,
                            e.Effects, (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)));
                    }
                }
            }
        }

        private void OnTreeViewDatabaseDragOver(object sender, DragEventArgs e)
        {
            if (!(sender is TreeView tv))
                return;
            TreeView treeView = (TreeView)sender;
            string moveOrCopy = string.Empty;
            string belowOrInside = "below";
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                e.Effects = DragDropEffects.Copy;
                moveOrCopy = "Copy";
            }
            else
            {
                e.Effects = DragDropEffects.Move;
                moveOrCopy = "Move";
            }
            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                belowOrInside = "inside";
            DragDropTest.Text = "";
            //check if the left or right control keys are pressed or not (copy or move)
            if (DragDropTest.Visibility == Visibility.Hidden)
                DragDropTest.Visibility = Visibility.Visible;
            //first check as the UI level, make sure we are looking at treeviewItems
            if (e.Source is TreeViewItem itemCurrentlyOver && treeView.SelectedItem is TreeViewItem itemToMove)
            {
                if (itemCurrentlyOver.Header is EditorComboBoxItem packageCurrentlyOver && itemToMove.Header is EditorComboBoxItem packageToMove)
                {

                    //make sure it's not same item
                    if (packageCurrentlyOver.Package.Equals(packageToMove.Package))
                    {
                        DragDropTest.Text = "Item can't be itself!";
                        return;
                    }
                    //if the item we're moving is not a selectable package, it does not matter if alt is pressed or not
                    if (!(packageCurrentlyOver.Package is SelectablePackage))
                        belowOrInside = "below";
                    DragDropTest.Text = string.Format("{0} {1} {2} {3}", moveOrCopy, packageToMove.DisplayName, belowOrInside, packageCurrentlyOver.DisplayName);
                    if (ItemToExpand != itemCurrentlyOver)
                    {
                        ItemToExpand = itemCurrentlyOver;
                        DragDropTimer.Stop();
                        DragDropTimer.Start();
                    }
                }
                else
                    DragDropTest.Text = "Both items need to be database packages!";
            }
            else
                DragDropTest.Text = "Both items need to be inside the tree view!";
        }

        private void OnTreeViewGroupsDrop(object sender, DragEventArgs e)
        {
            if (!(sender is TreeView tv))
                return;
            TreeView treeView = (TreeView)sender;
            if (e.Source is TreeViewItem itemCurrentlyOver && treeView.SelectedItem is TreeViewItem itemToMove)
            {
                if (itemToMove.Header is EditorComboBoxItem editorItemToMove && itemCurrentlyOver.Header is string && itemCurrentlyOver.Tag is int i)
                {
                    //assign to internals
                    if (treeView.Equals(InstallGroupsTreeView))
                        editorItemToMove.Package.InstallGroup = i;
                    else
                        editorItemToMove.Package.PatchGroup = i;
                    //assign to UI
                    if (itemToMove.Parent is TreeViewItem itemToMoveParent)
                    {
                        itemToMoveParent.Items.Remove(itemToMove);
                        itemCurrentlyOver.Items.Insert(0, itemToMove);
                    }
                }
            }
            DragDropTest.Text = string.Empty;
            DragDropTest.Visibility = Visibility.Hidden;
        }

        private void OnTreeViewGroupsDragOver(object sender, DragEventArgs e)
        {
            if (!(sender is TreeView tv))
                return;
            TreeView treeView = (TreeView)sender;
            if (e.Source is TreeViewItem itemCurrentlyOver && treeView.SelectedItem is TreeViewItem itemToMove)
            {
                if (itemToMove.Header is EditorComboBoxItem editorItemToMove)
                {
                    if (itemCurrentlyOver.Header is string)
                    {
                        DragDropTest.Text = string.Format("Assign {0} to {1} group {2}", editorItemToMove.DisplayName, treeView.Equals(InstallGroupsTreeView) ? "Install" : "Patch", itemCurrentlyOver.Tag.ToString());
                    }
                    else
                    {
                        DragDropTest.Text = "You need to select a group header!";
                    }
                }
            }
            DragDropTest.Visibility = Visibility.Visible;
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

        //to get and stop the alt key from causing the event to fire extra. the issue is that OnTreeViewMouseMove fires when alt key is pressed, released, and mouse is moved and clicked
        //note this only happens with the alt key
        bool altKeyFired = false;
        private void DatabaseTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            altKeyFired = true;
        }

        private void DatabaseTreeView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!(sender is TreeView tv))
                return;
            TreeView treeView = (TreeView)sender;

            //check if the alt key is responsible for this one
            if (altKeyFired)
            {
                altKeyFired = false;
                return;
            }

            if (Keyboard.IsKeyDown(Key.LeftAlt))
            {
                //yes i know this does nothing but polling for the key seems to allow the alt key event queue to drain out
            }

            //make sure the mouse is pressed and the drag movement is confirmed
            bool isDragConfirmed = IsDragConfirmed(e.GetPosition(treeView));
            if (e.LeftButton == MouseButtonState.Pressed && isDragConfirmed && !IsScrolling)
            {
                Logging.Editor("MouseMove DragDrop movement accepted, leftButton={0}, isDragConfirmed={1}, IsScrolling={2}",
                    LogLevel.Info, e.LeftButton.ToString(), isDragConfirmed.ToString(), IsScrolling.ToString());
                if (treeView.SelectedItem is TreeViewItem itemToMove)
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) && treeView.Equals(DatabaseTreeView))
                    {
                        //DoDragDrop is blocking
                        DragDrop.DoDragDrop(treeView, itemToMove, DragDropEffects.Copy);
                    }
                    else
                    {
                        DragDrop.DoDragDrop(treeView, itemToMove, DragDropEffects.Move);
                    }
                }
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                //AlreadyLoggedMouseMove = true;
                //yeah...that got annoying real quick
                //Logging.Editor("MouseMove DragDrop movement not accepted, leftButton={0}, isDragConfirmed={1}, IsScrolling={2}", LogLevel.Info, e.LeftButton.ToString(), isDragConfirmed.ToString(), IsScrolling.ToString());
            }
        }

        private void OnTreeViewMouseDownPreview(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is TreeView tv))
                return;
            TreeView treeView = (TreeView)sender;
            //Logging.Editor("MouseDown, leftButton={0}, saving mouse location if pressed", LogLevel.Info, e.LeftButton.ToString());
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                BeforeDragDropPoint = e.GetPosition(treeView);
            }
        }

        private void OnTreeViewScroll(object sender, ScrollChangedEventArgs e)
        {
            //https://stackoverflow.com/questions/14583234/disable-drag-and-drop-when-scrolling
            if (!AlreadyLoggedScroll)
            {
                //Logging.Editor("ScrollChanged event fire, LeftButton={0}, setting IsScrolling to true if pressed", Mouse.LeftButton.ToString());
                AlreadyLoggedScroll = true;
            }
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                IsScrolling = true;
            }
            if (Mouse.LeftButton == MouseButtonState.Released && AlreadyLoggedScroll)
                AlreadyLoggedScroll = false;
        }

        private void OnTreeViewMouseUpPreview(object sender, MouseButtonEventArgs e)
        {
            //Logging.Editor("MouseUp, leftButton={0}, setting IsScrolling to false", e.LeftButton.ToString());
            if (e.LeftButton == MouseButtonState.Released)
            {
                IsScrolling = false;
                //AlreadyLoggedMouseMove = false;
                AlreadyLoggedScroll = false;
                if (DragDropTest.Visibility == Visibility.Visible)
                    DragDropTest.Visibility = Visibility.Hidden;
            }
        }
        #endregion

        #region Zip File Upload/Download buttons

        private void ZipDownload_Click(object sender, RoutedEventArgs e)
        {
            //make sure something is selected
            if (SelectedItem == null)
            {
                MessageBox.Show("No item selected");
                Logging.Editor("Tried to download a zip, but SelectedItem is null");
                return;
            }

            DatabasePackage packToWorkOn = null;
            if (SelectedItem is EditorComboBoxItem ecbi)
                packToWorkOn = ecbi.Package;
            else if (SelectedItem is DatabasePackage pack)
                packToWorkOn = pack;

            if(packToWorkOn == null)
            {
                Logging.Editor("PackToWorkOn is null (not EditorComboboxItem or Databasepackage",LogLevel.Error);
                MessageBox.Show("PackToWorkOn is null (not EditorComboboxItem or Databasepackage), abort");
                return;
            }

            //make sure it actually has a zip file to download
            //first check if it's an editor combobox item (selected from checkbox) or the direct item
            if (string.IsNullOrWhiteSpace(packToWorkOn.ZipFile))
            {
                MessageBox.Show("No zip file to download");
                return;
            }

            //make sure FTP credentials are at least entered
            if (string.IsNullOrWhiteSpace(EditorSettings.BigmodsPassword) || string.IsNullOrWhiteSpace(EditorSettings.BigmodsUsername))
            {
                MessageBox.Show("Missing FTP credentials");
                return;
            }

            //create the save zip file dialog if not already null
            if (SaveZipFileDialog == null)
            {
                SaveZipFileDialog = new SaveFileDialog()
                {
                    AddExtension = true,
                    CheckPathExists = true,
                    DefaultExt = "zip",
                    OverwritePrompt = true,
                    //don't set initial directory to allow for restore feature
                    //https://stackoverflow.com/questions/16078362/how-to-save-last-folder-in-openfiledialog
                    //https://stackoverflow.com/questions/4353487/what-does-the-filedialog-restoredirectory-property-actually-do
                    Title = "Select destination for zip file",
                    FileName = packToWorkOn.ZipFile
                };
            }
            else
            {
                SaveZipFileDialog.FileName = packToWorkOn.ZipFile;
            }

            //invoke and stop if user stops
            if (!(bool)SaveZipFileDialog.ShowDialog())
                return;

            //make and run the uploader instance
            DatabaseEditorTransferWindow name = new DatabaseEditorTransferWindow(this.ModpackSettings)
            {
                ZipFilePathDisk = SaveZipFileDialog.FileName,
                ZipFilePathOnline = string.Format("{0}{1}/", PrivateStuff.BigmodsFTPUsersRoot, WoTModpackOnlineFolderVersion),
                ZipFileName = Path.GetFileName(packToWorkOn.ZipFile),
                Credential = new NetworkCredential(EditorSettings.BigmodsUsername, EditorSettings.BigmodsPassword),
                TransferMode = EditorTransferMode.DownloadZip,
                PackageToUpdate = null,
                Countdown = EditorSettings.FTPUploadDownloadWindowTimeout,
                EditorSettings = EditorSettings,
                WoTModpackOnlineFolderVersion = this.WoTModpackOnlineFolderVersion
            };
            name.Show();
        }

        private void ZipUpload_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("No item selected");
                Logging.Editor("Tried to download a zip, but SelectedItem is null");
                return;
            }

            //make sure FTP credentials are entered
            if (string.IsNullOrWhiteSpace(EditorSettings.BigmodsPassword) || string.IsNullOrWhiteSpace(EditorSettings.BigmodsUsername))
            {
                MessageBox.Show("Missing FTP credentials");
                return;
            }

            //get the currently selected item in the editor UI
            DatabasePackage packToWorkOn = GetDatabasePackage(SelectedItem);
            if (packToWorkOn == null)
            {
                Logging.Editor("PackToWorkOn is null (not EditorComboboxItem or Databasepackage", LogLevel.Error);
                MessageBox.Show("PackToWorkOn is null (not EditorComboboxItem or Databasepackage), abort");
                return;
            }

            string zipFileToUpload = string.Empty;
            if (OpenZipFileDialog == null)
                OpenZipFileDialog = new OpenFileDialog()
                {
                    AddExtension = true,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    DefaultExt = "zip",
                    Multiselect = false,
                    Title = "Select zip file to upload"
                };

            if ((bool)OpenZipFileDialog.ShowDialog() && File.Exists(OpenZipFileDialog.FileName))
                zipFileToUpload = OpenZipFileDialog.FileName;
            else
                return;

            //make and run the uploader instance
            DatabaseEditorTransferWindow name = new DatabaseEditorTransferWindow(this.ModpackSettings)
            {
                ZipFilePathDisk = zipFileToUpload,
                ZipFilePathOnline = string.Format("{0}{1}/", PrivateStuff.BigmodsFTPUsersRoot, WoTModpackOnlineFolderVersion),
                ZipFileName = Path.GetFileName(zipFileToUpload),
                Credential = new NetworkCredential(EditorSettings.BigmodsUsername, EditorSettings.BigmodsPassword),
                TransferMode = EditorTransferMode.UploadZip,
                PackageToUpdate = packToWorkOn,
                Countdown = EditorSettings.FTPUploadDownloadWindowTimeout,
                EditorSettings = EditorSettings,
                WoTModpackOnlineFolderVersion = this.WoTModpackOnlineFolderVersion
            };
            name.OnEditorUploadDownloadClosed += OnEditorUploadFinished;
            name.Show();
        }

        private void OnEditorUploadFinished(object sender, EditorTransferEventArgs e)
        {
            Logging.Editor("OnEditorUploadFinished(): Upload finished, type = {0}", LogLevel.Info, e.TransferMode.ToString());

            DatabasePackage selectedItem = GetDatabasePackage(SelectedItem);

            switch (e.TransferMode)
            {
                case EditorTransferMode.UploadMedia:
                    Logging.Editor("Adding media entry {0} to package {1}", LogLevel.Info, e.UploadedFilename, e.Package.PackageName);
                    Media m = new Media()
                    {
                        MediaType = MediaType.Picture,
                        URL = string.Format("{0}{1}", e.UploadedFilepathOnline, e.UploadedFilename).Replace("ftp:", "https:")
                    };
                    SelectablePackage updatedPackage = e.Package as SelectablePackage;
                    updatedPackage.Medias.Add(m);

                    if (selectedItem.Equals(e.Package))
                    {
                        Logging.Editor("It's currently displayed, updating entry for display");
                        PackageMediasDisplay.Items.Add(m);
                        ApplyDatabaseObject(e.Package);
                        ShowDatabasePackage(e.Package);
                    }
                    break;
                case EditorTransferMode.UploadZip:
                    Logging.Editor("Changing zipFile entry for package {0} and updating time stamp/CRC", LogLevel.Info, e.Package.PackageName);
                    Logging.Editor("Old = {0}, New = {1}", LogLevel.Info, PackageZipFileDisplay.Text, e.UploadedFilename);
                    e.Package.ZipFile = e.UploadedFilename;
                    e.Package.CRC = UpdatedPackageNewCRC;
                    e.Package.Timestamp = CommonUtils.GetCurrentUniversalFiletimeTimestamp();

                    if (selectedItem.Equals(e.Package))
                    {
                        Logging.Editor("It's currently displayed, updating entry for display");
                        PackageZipFileDisplay.Text = e.Package.ZipFile;
                        ApplyDatabaseObject(e.Package);
                        ShowDatabasePackage(e.Package);
                    }
                    TriggerMirrorSyncButton_Click(null, null);
                    break;
            }

            UnsavedChanges = true;
        }
        #endregion

        #region Database Save/Load buttons

        private void SaveDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DefaultSaveLocationSetting.Text))
            {
                MessageBox.Show("Default save location is empty, please specify before using this button");
                return;
            }

            if (!Directory.Exists(Path.GetDirectoryName(DefaultSaveLocationSetting.Text)))
            {
                MessageBox.Show(string.Format("The save path\n{0}\ndoes not exist, please re-specify", Path.GetDirectoryName(DefaultSaveLocationSetting.Text)));
                return;
            }

            //if save triggers apply, then do it
            if (EditorSettings.ApplyBehavior == ApplyBehavior.SaveTriggersApply && SelectedItem != null)
            {
                ApplyDatabaseObject(SelectedItem);
            }

            //actually save
            databaseManager.SaveDatabase(EditorSettings.DefaultEditorSaveLocation, DatabaseManager.DocumentVersion1V2, XmlComponent.SchemaV1Dot2);

            UnsavedChanges = false;
        }

        private void SaveAsDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            //if save triggers apply, then do it
            if (EditorSettings.ApplyBehavior == ApplyBehavior.SaveTriggersApply && SelectedItem != null)
            {
                ApplyDatabaseObject(SelectedItem);
            }

            //create the save dialog if it doesn't already exist
            if (SaveDatabaseDialog == null)
                SaveDatabaseDialog = new SaveFileDialog()
                {
                    AddExtension = true,
                    CheckPathExists = true,
                    DefaultExt = "xml",
                    InitialDirectory = string.IsNullOrWhiteSpace(DefaultSaveLocationSetting.Text) ? ApplicationConstants.ApplicationStartupPath :
                    Directory.Exists(Path.GetDirectoryName(DefaultSaveLocationSetting.Text)) ? DefaultSaveLocationSetting.Text : ApplicationConstants.ApplicationStartupPath,
                    Title = string.Format("Save Database")
                };

            //then show it and only continue if a selection was committed
            if (!(bool)SaveDatabaseDialog.ShowDialog())
                return;

            if (string.IsNullOrWhiteSpace(DefaultSaveLocationSetting.Text) ||
                    !Path.GetDirectoryName(SaveDatabaseDialog.FileName).Equals(Path.GetDirectoryName(DefaultSaveLocationSetting.Text)))
                if (MessageBox.Show("Use this as default save location?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    DefaultSaveLocationSetting.Text = SaveDatabaseDialog.FileName;

            //actually save
            databaseManager.SaveDatabase(EditorSettings.DefaultEditorSaveLocation);

            UnsavedChanges = false;
        }

        private async void LoadAsDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            string fileToLoad = string.Empty;
            //check if it's from the auto load function or not
            if (sender != null)
            {
                //from gui button press
                if (OpenDatabaseDialog == null)
                    OpenDatabaseDialog = new OpenFileDialog()
                    {
                        AddExtension = true,
                        CheckFileExists = true,
                        CheckPathExists = true,
                        DefaultExt = "xml",
                        //InitialDirectory = Settings.ApplicationStartupPath,
                        Multiselect = false,
                        Title = "Load Database"
                    };
                if ((bool)OpenDatabaseDialog.ShowDialog() && File.Exists(OpenDatabaseDialog.FileName))
                {
                    fileToLoad = OpenDatabaseDialog.FileName;
                }
                else
                    return;
            }
            else
            {
                //from auto load function
                fileToLoad = CommandLineSettings.EditorAutoLoadFileName;
            }

            //load the database into the manager
            await databaseManager.LoadDatabaseTestAsync(fileToLoad);

            LoadUI(GlobalDependencies, Dependencies, ParsedCategoryList);
            UnsavedChanges = false;
        }

        private async void OnLoadDatabaseClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DefaultSaveLocationSetting.Text))
            {
                MessageBox.Show("Default save location is empty, please specify before using this button");
                return;
            }
            if (!File.Exists(DefaultSaveLocationSetting.Text))
            {
                MessageBox.Show(string.Format("The file\n{0}\ndoes not exist", DefaultSaveLocationSetting.Text));
                return;
            }

            //load the database into the manager
            await databaseManager.LoadDatabaseTestAsync(DefaultSaveLocationSetting.Text);

            LoadUI(GlobalDependencies, Dependencies, ParsedCategoryList);
            UnsavedChanges = false;
        }

        private void SelectDefaultSaveLocationButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveDatabaseDialog == null)
                SaveDatabaseDialog = new SaveFileDialog()
                {
                    AddExtension = true,
                    CheckPathExists = true,
                    //https://stackoverflow.com/questions/5512752/how-to-stop-overwriteprompt-when-creating-savefiledialog-using-getsavefilename
                    OverwritePrompt = false,
                    CheckFileExists = false,
                    DefaultExt = "xml",
                    InitialDirectory = string.IsNullOrWhiteSpace(DefaultSaveLocationSetting.Text) ? ApplicationConstants.ApplicationStartupPath :
                    Directory.Exists(Path.GetDirectoryName(DefaultSaveLocationSetting.Text)) ? DefaultSaveLocationSetting.Text : ApplicationConstants.ApplicationStartupPath,
                    Title = "Select path to save database to. NOTE: It is only selecting path, does not save"
                };
            if (!(bool)SaveDatabaseDialog.ShowDialog())
                return;
            DefaultSaveLocationSetting.Text = SaveDatabaseDialog.FileName;
        }
        #endregion

        #region Database Add/Move/Remove/Sync buttons

        private void RemoveDatabaseObjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditorSettings.ShowConfirmationOnPackageAddRemoveMove)
            {
                if (MessageBox.Show("Confirm this action?", "", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    return;
            }
            if (!(DatabaseTreeView.SelectedItem is TreeViewItem tvi2) || !(tvi2.Header is EditorComboBoxItem cbi2))
            {
                MessageBox.Show("Please select a package to perform action on");
                return;
            }
            if (DatabaseTreeView.SelectedItem is TreeViewItem tvi && tvi.Header is EditorComboBoxItem comboBoxItem && tvi.Parent is TreeViewItem parentTvi)
            {
                if (MessageBox.Show(string.Format("Are you sure you want to remove {0}?", comboBoxItem.DisplayName), "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (comboBoxItem.Package is SelectablePackage sp)
                    {
                        sp.Parent.Packages.Remove(sp);
                    }
                    else if (comboBoxItem.Package is Dependency d)
                    {
                        Dependencies.Remove(d);
                    }
                    else if (comboBoxItem.Package is DatabasePackage dp)
                    {
                        GlobalDependencies.Remove(dp);
                    }
                    parentTvi.Items.Remove(tvi);
                }
            }
            else
            {
                MessageBox.Show("Error, make sure selected item is a package");
            }
        }

        private void MoveDatabaseObjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditorSettings.ShowConfirmationOnPackageAddRemoveMove)
            {
                if (MessageBox.Show("Confirm this action?", "", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    return;
            }

            if (!(DatabaseTreeView.SelectedItem is TreeViewItem tvi2) || !(tvi2.Header is EditorComboBoxItem cbi2))
            {
                MessageBox.Show("Please select a package to perform action on");
                return;
            }
            EditorAddRemove addRemove = new EditorAddRemove(this.ModpackSettings)
            {
                GlobalDependencies = GlobalDependencies,
                Dependencies = Dependencies,
                ParsedCategoryList = ParsedCategoryList,
                EditOrAdd = true,
                AddSameLevel = true,
                SelectedPackage = null
            };

            if (!(bool)addRemove.ShowDialog())
                return;

            //selectedItem is itemToMove, currentlyOver is what you just pointed to
            if (DatabaseTreeView.SelectedItem is TreeViewItem itemToMove && itemToMove.Header is EditorComboBoxItem editorItemToMove
                && itemToMove.Parent is TreeViewItem parentItemToMove && addRemove.SelectedPackage.EditorTreeViewItem.Parent is TreeViewItem parentItemCurrentlyOver)
            {
                PerformDatabaseMoveAdd(addRemove.SelectedPackage.EditorTreeViewItem, itemToMove, parentItemToMove, parentItemCurrentlyOver, editorItemToMove.Package,
                    addRemove.SelectedPackage, DragDropEffects.Move, !addRemove.AddSameLevel);
            }
        }

        private void AddDatabaseObjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditorSettings.ShowConfirmationOnPackageAddRemoveMove)
            {
                if (MessageBox.Show("Confirm this action?", "", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    return;
            }
            if (!(DatabaseTreeView.SelectedItem is TreeViewItem tvi2) || !(tvi2.Header is EditorComboBoxItem cbi2))
            {
                MessageBox.Show("Please select a package to perform action on");
                return;
            }

            if (cbi2.Package == null)
                throw new BadMemeException("cbi2.Package is null");

            //make the window and show it
            EditorAddRemove addRemove = new EditorAddRemove(this.ModpackSettings)
            {
                GlobalDependencies = GlobalDependencies,
                Dependencies = Dependencies,
                ParsedCategoryList = ParsedCategoryList,
                EditOrAdd = false,
                AddSameLevel = true,
                SelectedPackage = null,
                DatabaseTreeviewSelectedItem = cbi2.Package
            };
            if (!(bool)addRemove.ShowDialog())
                return;

            //getting here means that we are confirming an add
            //selectedItem is itemToMove, currentlyOver is what you just pointed to
            if (DatabaseTreeView.SelectedItem is TreeViewItem itemToMove && itemToMove.Header is EditorComboBoxItem editorItemToMove
                && itemToMove.Parent is TreeViewItem parentItemToMove && addRemove.SelectedPackage.EditorTreeViewItem.Parent is TreeViewItem parentItemCurrentlyOver)
            {
                PerformDatabaseMoveAdd(addRemove.SelectedPackage.EditorTreeViewItem, itemToMove, parentItemToMove, parentItemCurrentlyOver, editorItemToMove.Package,
                    addRemove.SelectedPackage, DragDropEffects.Copy, !addRemove.AddSameLevel);
                DatabaseTreeView.Items.Refresh();
            }
        }

        private async void TriggerMirrorSyncButton_Click(object sender, RoutedEventArgs e)
        {
            TriggerMirrorSyncButton.IsEnabled = false;
            TriggerMirrorSyncButton.Content = "Running...";
            Logging.Editor("Running trigger manual sync script...");

            string resultText = null;
            bool result = await FtpUtils.TriggerMirrorSyncAsync();
            if (result)
                resultText = "SUCCESS!";
            else
                resultText = "ERROR!";

            TriggerMirrorSyncButton.Content = resultText;
            await Task.Delay(5000);

            TriggerMirrorSyncButton.IsEnabled = true;
            TriggerMirrorSyncButton.Content = "Trigger Sync";
        }
        #endregion

        #region Dependency modify buttons
        private void DependenciesAddSelected_Click(object sender, RoutedEventArgs e)
        {
            if (LoadedDependenciesList.SelectedIndex < 0)
            {
                MessageBox.Show("Invalid dependency selection");
                return;
            }
            if (LoadedLogicsList.SelectedIndex < 0)
            {
                MessageBox.Show("Invalid logic selection");
                return;
            }

            Logging.Editor("Adding dependency to component");

            //check the list of databaselogic in the item, make sure we're not trying to add a duplicate item
            foreach (DatabaseLogic logic in PackageDependenciesDisplay.Items)
            {
                if (logic.PackageName.Equals((LoadedDependenciesList.SelectedItem as Dependency).PackageName))
                {
                    MessageBox.Show("Dependency already exists in package");
                    return;
                }
            }

            //add to UI
            PackageDependenciesDisplay.Items.Add(new DatabaseLogic()
            {
                PackageName = (LoadedDependenciesList.SelectedItem as Dependency).PackageName,
                Logic = (Logic)LoadedLogicsList.SelectedItem,
                NotFlag = (bool)DependenciesNotFlag.IsChecked
            });

            UnsavedChanges = true;
        }

        private void DependenciesApplyEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (PackageDependenciesDisplay.SelectedItem == null)
                return;

            DatabaseLogic selectedLogic = (DatabaseLogic)PackageDependenciesDisplay.SelectedItem;
            selectedLogic.Logic = (Logic)LoadedLogicsList.SelectedItem;
            selectedLogic.NotFlag = (bool)DependenciesNotFlag.IsChecked;
        }

        private void DependenciesRemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            if (PackageDependenciesDisplay.SelectedItem == null)
                return;

            //remove the selected item from the UI display
            PackageDependenciesDisplay.Items.Remove(PackageDependenciesDisplay.SelectedItem);

            UnsavedChanges = true;
        }

        private void PackageDependenciesDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PackageDependenciesDisplay.SelectedItem == null)
                return;

            DatabaseLogic selectedLogic = (DatabaseLogic)PackageDependenciesDisplay.SelectedItem;

            LoadedLogicsList.SelectedItem = selectedLogic.Logic;
            DependenciesNotFlag.IsChecked = selectedLogic.NotFlag;

            LoadedDependenciesList.SelectedIndex = -1;

            //check if it exits in the list of all loaded dependencies (it should) and if it does select it
            foreach (Dependency dependency in LoadedDependenciesList.Items)
            {
                if (dependency.PackageName.Equals(selectedLogic.PackageName))
                {
                    LoadedDependenciesList.SelectedItem = dependency;
                    break;
                }
            }
        }
        #endregion

        #region Media preview buttons
        private void MediaPreviewSelectedMediaButton_Click(object sender, RoutedEventArgs e)
        {
            if (PackageMediasDisplay.SelectedIndex == -1)
            {
                MessageBox.Show("Invalid Index");
                return;
            }

            if (PackageMediasDisplay.SelectedItem is Media media)
            {
                media.SelectablePackageParent = GetSelectablePackage(SelectedItem);
                if (Preview != null)
                {
                    Preview = null;
                }
                Preview = new Preview(this.ModpackSettings)
                {
                    Medias = new List<Media>() { media },
                    EditorMode = true,
                    ComboBoxItemsInsideMode = false,
                    InvokedPackage = GetSelectablePackage(SelectedItem)
                };
                try
                {
                    Preview.ShowDialog();
                }
                finally
                {
                    Preview = null;
                }
            }
            else
                throw new BadMemeException("How is the type not Media. Something is wrong here.");
        }

        private void MediaPreviewEditMediaButton_Click(object sender, RoutedEventArgs e)
        {
            //input filtering
            if (string.IsNullOrWhiteSpace(MediaTypesURL.Text))
            {
                MessageBox.Show("Media URL must exist");
                return;
            }
            if (MediaTypesList.SelectedIndex == -1)
            {
                MessageBox.Show("Invalid Type");
                return;
            }

            Media testMedia = new Media()
            {
                URL = MediaTypesURL.Text,
                MediaType = (MediaType)MediaTypesList.SelectedItem,
                SelectablePackageParent = GetSelectablePackage(SelectedItem)
            };

            if (Preview != null)
            {
                Preview = null;
            }
            Preview = new Preview(this.ModpackSettings)
            {
                Medias = new List<Media>() { testMedia },
                EditorMode = true,
                ComboBoxItemsInsideMode = false,
                InvokedPackage = GetSelectablePackage(SelectedItem)
            };
            try
            {
                Preview.ShowDialog();
            }
            finally
            {
                Preview = null;
            }
        }
        #endregion

        #region Media modify buttons
        private void MediaAddMediaButton_Click(object sender, RoutedEventArgs e)
        {
            //input filtering
            if (string.IsNullOrWhiteSpace(MediaTypesURL.Text))
            {
                MessageBox.Show("Media URL must exist");
                return;
            }
            if (MediaTypesList.SelectedIndex == -1)
            {
                MessageBox.Show("Invalid Type");
                return;
            }
            Logging.Editor("Adding media");

            foreach (Media media in PackageMediasDisplay.Items)
            {
                if (media.URL.Equals(MediaTypesURL.Text))
                {
                    MessageBox.Show("Media URL already exists in list");
                }
            }

            Media m = new Media()
            {
                MediaType = (MediaType)MediaTypesList.SelectedItem,
                URL = MediaTypesURL.Text
            };
            PackageMediasDisplay.Items.Add(m);

            UnsavedChanges = true;
        }

        private void MediaApplyEditButton_Click(object sender, RoutedEventArgs e)
        {
            //input filtering
            if (string.IsNullOrWhiteSpace(MediaTypesURL.Text))
            {
                MessageBox.Show("Media URL must exist");
                return;
            }
            if (MediaTypesList.SelectedIndex == -1)
            {
                MessageBox.Show("Invalid Type");
                return;
            }
            if (PackageMediasDisplay.SelectedIndex < 0)
            {
                MessageBox.Show("Invalid media to apply edit to");
                return;
            }
            Logging.Editor("Applying media edit from component");

            Media selectedMediaInUI = (PackageMediasDisplay.SelectedItem as Media);

            selectedMediaInUI.MediaType = (MediaType)MediaTypesList.SelectedItem;
            selectedMediaInUI.URL = MediaTypesURL.Text;

            UnsavedChanges = true;
        }

        private void MediaRemoveMediaButton_Click(object sender, RoutedEventArgs e)
        {
            if (PackageMediasDisplay.SelectedItem == null)
                return;
            Logging.Editor("Removing media from UI list");

            Media selectedMediaInUI = (PackageMediasDisplay.SelectedItem as Media);

            //remove from UI list
            PackageMediasDisplay.Items.Remove(selectedMediaInUI);

            UnsavedChanges = true;
        }

        private void PackageMediasDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PackageMediasDisplay.SelectedItem == null)
                return;

            Media media = (Media)PackageMediasDisplay.SelectedItem;
            MediaTypesList.SelectedItem = media.MediaType;
            MediaTypesURL.Text = media.URL;
        }

        private void UploadMediaButton_Click(object sender, RoutedEventArgs e)
        {
            //initial checks
            //make sure FTP credentials are at least entered
            if (string.IsNullOrWhiteSpace(EditorSettings.BigmodsPassword) || string.IsNullOrWhiteSpace(EditorSettings.BigmodsUsername))
            {
                MessageBox.Show("Missing FTP credentials");
                return;
            }

            //get the currently selected item in the editor UI
            DatabasePackage packToWorkOn = GetDatabasePackage(SelectedItem);
            if (packToWorkOn == null)
            {
                Logging.Editor("PackToWorkOn is null (not EditorComboboxItem or Databasepackage", LogLevel.Error);
                MessageBox.Show("PackToWorkOn is null (not EditorComboboxItem or Databasepackage), abort");
                return;
            }

            //get the path to upload to
            if (OpenPictureDialog == null)
                OpenPictureDialog = new OpenFileDialog()
                {
                    AddExtension = true,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Multiselect = true,
                    Title = "Select image file to upload"
                };

            if (!(bool)OpenPictureDialog.ShowDialog())
                return;

            //select path to upload to on server
            EditorSelectMediaUploadLocation selectUploadLocation = new EditorSelectMediaUploadLocation(this.ModpackSettings)
            {
                Credential = new NetworkCredential(EditorSettings.BigmodsUsername, EditorSettings.BigmodsPassword)
            };
            if (!(bool)selectUploadLocation.ShowDialog())
                return;

            //start upload
            foreach (string mediaToUploadPath in OpenPictureDialog.FileNames)
            {
                string mediaToUploadFilename = Path.GetFileName(mediaToUploadPath);
                DatabaseEditorTransferWindow name = new DatabaseEditorTransferWindow(this.ModpackSettings)
                {
                    ZipFilePathDisk = mediaToUploadPath,
                    ZipFilePathOnline = selectUploadLocation.UploadPath,
                    ZipFileName = mediaToUploadFilename,
                    Credential = new NetworkCredential(EditorSettings.BigmodsUsername, EditorSettings.BigmodsPassword),
                    TransferMode = EditorTransferMode.UploadMedia,
                    PackageToUpdate = packToWorkOn,
                    Countdown = EditorSettings.FTPUploadDownloadWindowTimeout,
                    EditorSettings = EditorSettings,
                    WoTModpackOnlineFolderVersion = this.WoTModpackOnlineFolderVersion
                };
                //changed to a show() with event handler made for on exit
                name.OnEditorUploadDownloadClosed += OnEditorUploadFinished;
                name.Show();
            }
        }
        #endregion

        #region UserFile modify buttons
        private void UserdataAddUserdataButton_Click(object sender, RoutedEventArgs e)
        {
            //check if valid input
            if (string.IsNullOrWhiteSpace(UserDataEditBox.Text))
            {
                MessageBox.Show("No user file specified");
                return;
            }

            //check if already exists
            foreach (string userfile in PackageUserFilesDisplay.Items)
            {
                if (userfile.Equals(UserDataEditBox.Text))
                {
                    MessageBox.Show("User file already exists");
                    return;
                }
            }

            Logging.Editor("Adding UserFile {0}", LogLevel.Info, UserDataEditBox.Text);

            PackageUserFilesDisplay.Items.Add(UserDataEditBox.Text);

            UnsavedChanges = true;
        }

        private void UserdataApplyEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (PackageUserFilesDisplay.SelectedIndex < 0)
            {
                MessageBox.Show("Invalid selection");
                return;
            }

            foreach (string userfile in PackageUserFilesDisplay.Items)
            {
                if (userfile.Equals(UserDataEditBox.Text))
                {
                    MessageBox.Show("User file already exists");
                    return;
                }
            }

            Logging.Editor("Editing UserFile", LogLevel.Info);
            string patternInUI = (PackageUserFilesDisplay.SelectedItem as string);
            patternInUI = UserDataEditBox.Text;

            UnsavedChanges = true;
        }

        private void UserdataRemoveUserdata_Click(object sender, RoutedEventArgs e)
        {
            if (PackageUserFilesDisplay.SelectedItem == null)
                return;

            Logging.Editor("Removing UserFile from UI");
            PackageUserFilesDisplay.Items.Remove(PackageUserFilesDisplay.SelectedItem);

            UnsavedChanges = true;
        }

        private void PackageUserdatasDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PackageUserFilesDisplay.SelectedItem == null)
                return;

            UserDataEditBox.Text = (PackageUserFilesDisplay.SelectedItem as string);
        }
        #endregion

        #region Trigger and conflicting package modify buttons
        private void TriggerAddSelectedTrigger_Click(object sender, RoutedEventArgs e)
        {
            if (LoadedTriggersComboBox.SelectedItem == null)
            {
                MessageBox.Show("Invalid selection");
                return;
            }

            foreach (string s in PackageTriggersDisplay.Items)
            {
                if (s.Equals(LoadedTriggersComboBox.SelectedItem as string))
                {
                    MessageBox.Show("Trigger already exists");
                    return;
                }
            }

            Logging.Editor("Adding trigger to UI");
            PackageTriggersDisplay.Items.Add(LoadedTriggersComboBox.SelectedItem as string);

            UnsavedChanges = true;
        }

        private void TriggerRemoveTrigger_Click(object sender, RoutedEventArgs e)
        {
            if (PackageTriggersDisplay.SelectedItem == null)
            {
                MessageBox.Show("Invalid selection");
                return;
            }

            Logging.Editor("Removing trigger from component in UI");
            PackageTriggersDisplay.Items.Remove(PackageTriggersDisplay.SelectedItem);

            UnsavedChanges = true;
        }

        private void PackageTriggersDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PackageTriggersDisplay.SelectedItem == null)
                return;

            LoadedTriggersComboBox.SelectedIndex = -1;

            foreach (string s in LoadedTriggersComboBox.Items)
            {
                if (s.Equals(PackageTriggersDisplay.SelectedItem as string))
                {
                    LoadedTriggersComboBox.SelectedItem = s;
                }
            }
        }

        private void ConflictingPackagesRemoveConflictingPackage_Click(object sender, RoutedEventArgs e)
        {
            if (PackageConflictingPackagesDisplay.SelectedItem == null)
            {
                MessageBox.Show("Invalid selection");
                return;
            }

            //remove it from the current database, but also from the packages that it conflicts with
            ListBoxItem selectedListBoxItem = PackageConflictingPackagesDisplay.SelectedItem as ListBoxItem;
            ConflictingPackage packageBReference = selectedListBoxItem.Tag as ConflictingPackage;
            if (packageBReference == null)
                return;

            //we are in "package a", this entry (selected item) is "package b"
            //go to the package b selectablePacakge reference and remove the conflictingPackage entry to package a
            //then remove the package b conflictingPackage reference in package a
            SelectablePackage conflictingPackageB = packageBReference.ConflictingSelectablePackage;
            SelectablePackage conflictingPackageA = packageBReference.ParentSelectablePackage;
            if (conflictingPackageA == null || conflictingPackageB == null)
                return;

            ConflictingPackage packageAReference = conflictingPackageB.ConflictingPackagesNew.Find(pack => pack.ConflictingPackageUID.Equals(conflictingPackageA.UID));
            if (packageAReference == null)
                return;

            conflictingPackageB.ConflictingPackagesNew.Remove(packageAReference);
            PackageConflictingPackagesDisplay.Items.Remove(PackageConflictingPackagesDisplay.SelectedItem);

            UnsavedChanges = true;
        }
        #endregion

        #region Tags modify buttons
        private void TagAddSelectedTag_Click(object sender, RoutedEventArgs e)
        {
            //verify that a tag from the list is selected
            if (LoadedTagsComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("No tag selected to add");
                return;
            }

            //verify that tag to add does not already exist in it
            foreach (PackageTags packageTag in PackageTagsDisplay.Items)
            {
                if (packageTag.Equals((PackageTags)LoadedTagsComboBox.SelectedItem))
                {
                    MessageBox.Show("Tag already exists in package");
                    return;
                }
            }

            //valid add
            Logging.Editor("Adding tag '{0}'", LogLevel.Info, LoadedTagsComboBox.SelectedItem);
            PackageTagsDisplay.Items.Add(LoadedTagsComboBox.SelectedItem);
            UnsavedChanges = true;
        }

        private void TagRemoveTag_Click(object sender, RoutedEventArgs e)
        {
            if (PackageTagsDisplay.SelectedItem == null)
            {
                MessageBox.Show("No tag selected to remove");
                return;
            }

            //valid remove
            Logging.Editor("Removing tag from PackageTagsDisplay: '{0}'", LogLevel.Info, PackageTagsDisplay.SelectedItem);
            PackageTagsDisplay.Items.Remove(PackageTagsDisplay.SelectedItem);
            UnsavedChanges = true;
        }
        #endregion

        #region Settings tab events
        private void BigmodsUsernameSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditorSettings.BigmodsUsername = BigmodsUsernameSetting.Text;
        }

        private void BigmodsPasswordSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditorSettings.BigmodsPassword = BigmodsPasswordSetting.Text;
        }

        private void DefaultSaveLocationSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditorSettings.DefaultEditorSaveLocation = DefaultSaveLocationSetting.Text;
        }

        private void SelectTransferWindowMovePathTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditorSettings.UploadZipMoveFolder = SelectTransferWindowMovePathTextbox.Text;
        }

        private void SaveSelectionBeforeLeaveSetting_Click(object sender, RoutedEventArgs e)
        {
            EditorSettings.SaveSelectionBeforeLeave = (bool)SaveSelectionBeforeLeaveSetting.IsChecked;
        }

        private void ShowConfirmOnPackageApplySetting_Click(object sender, RoutedEventArgs e)
        {
            EditorSettings.ShowConfirmationOnPackageApply = (bool)ShowConfirmOnPackageApplySetting.IsChecked;
        }

        private void ShowConfirmOnPackageAddRemoveEditSetting_Click(object sender, RoutedEventArgs e)
        {
            EditorSettings.ShowConfirmationOnPackageAddRemoveMove = (bool)ShowConfirmOnPackageAddRemoveEditSetting.IsChecked;
        }

        private void DatabaseTransferDeleteActuallyMove_Click(object sender, RoutedEventArgs e)
        {
            EditorSettings.UploadZipDeleteIsActuallyMove = (bool)DatabaseTransferDeleteActuallyMove.IsChecked;
        }

        private void DatabaseTransferAutoDelete_Click(object sender, RoutedEventArgs e)
        {
            EditorSettings.DeleteUploadLocallyUponCompletion = (bool)DatabaseTransferAutoDelete.IsChecked;
        }

        private void SelectTransferWindowMovePathButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog openFolderDialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Multiselect = false,
                Title = "Select folder"
            };
            if (openFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SelectTransferWindowMovePathTextbox.Text = openFolderDialog.FileName;
            }
        }

        private void ApplyBehaviorSetting_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)ApplyBehaviorDefaultSetting.IsChecked)
                EditorSettings.ApplyBehavior = ApplyBehavior.Default;
            else if ((bool)ApplyBehaviorApplyTriggersSaveSetting.IsChecked)
                EditorSettings.ApplyBehavior = ApplyBehavior.ApplyTriggersSave;
            else if ((bool)ApplyBehaviorSaveTriggersApplySetting.IsChecked)
                EditorSettings.ApplyBehavior = ApplyBehavior.SaveTriggersApply;
        }

        private void FtpUpDownAutoCloseTimoutSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            EditorSettings.FTPUploadDownloadWindowTimeout = (uint)FtpUpDownAutoCloseTimoutSlider.Value;
            FtpUpDownAutoCloseTimoutDisplayLabel.Text = EditorSettings.FTPUploadDownloadWindowTimeout.ToString();
        }
        #endregion

        #region Searchbox code
        private void SearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            //Logging.Editor("[SearchBox_KeyUp]: Entered, search text = {0}, selectedIndex = {1}", LogLevel.Debug, SearchBox.Text, SearchBox.SelectedIndex);
            SearchBox.IsDropDownOpen = true;
            if (e.Key == Key.Down || e.Key == Key.Up)
            {
                //Logging.Editor("[SearchBox_KeyUp]: Key is down or up, e.Handled = true, selectedIndex = {0}", LogLevel.Debug, SearchBox.SelectedIndex);
                //stop the selection from key events!!!
                //https://www.codeproject.com/questions/183259/how-to-prevent-selecteditem-change-on-up-and-down (second answer)
                e.Handled = true;

                //if trying to navigate but there's noting selected, then select one
                if(SearchBox.Items.Count > 0 && SearchBox.SelectedIndex == -1)
                {
                    SearchBox.SelectedIndex = 0;
                }
            }
            else if (e.Key == Key.Enter)
            {
                //Logging.Editor("[SearchBox_KeyUp]: Key is enter, OnSearchBoxCommitted, search text = {0}, selectedIndex = {1}", LogLevel.Debug, SearchBox.Text, SearchBox.SelectedIndex);
                OnSearchBoxCommitted(SearchBox.SelectedItem as EditorSearchBoxItem, false);
            }
            else if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                //Logging.Editor("[SearchBox_KeyUp]: SearchBox.Text is null.empty, clean items, search text = {0}, selectedIndex = {1}", LogLevel.Debug, SearchBox.Text, SearchBox.SelectedIndex);
                SearchBox.Items.Clear();
                SearchBox.IsDropDownOpen = false;
                SearchBox.SelectedIndex = -1;
            }
            else if (SearchBox.Text.Length > 1)
            {
                //Logging.Editor("[SearchBox_KeyUp]: Process search, search text = {0}, selectedIndex = {1}", LogLevel.Debug, SearchBox.Text, SearchBox.SelectedIndex);
                //if something is currently selected, then changing the selected index later will loose focus on textbox part of combobox and cause the text to
                //highlight in the middle of typing. this will "eat" the first letter or two of the user's search
                if(SearchBox.SelectedIndex != -1)
                {
                    TextBox textBox = (TextBox)((ComboBox)sender).Template.FindName("PART_EditableTextBox", (ComboBox)sender);
                    //backup what the user was typing
                    string temp = SearchBox.Text;
                    //set the selected index to nothing. sets focus to dropdown
                    SearchBox.SelectedIndex = -1;
                    //restore the text. sets focus and highlights the combobox text
                    SearchBox.Text = temp;
                    //set the selection to the end (remove selection)
                    textBox.SelectionStart = ((ComboBox)sender).Text.Length;
                    textBox.SelectionLength = 0;
                }

                //split the search into an array based on using '*' search
                List<DatabasePackage> searchComponents = new List<DatabasePackage>();
                foreach (string searchTerm in SearchBox.Text.Split('*'))
                {
                    //get a list of components that match the search term
                    searchComponents.AddRange(databaseManager.GetFlatList().Where(term => term.PackageName.ToLower().Contains(searchTerm.ToLower())));
                }

                //remove duplicates
                searchComponents = searchComponents.Distinct().ToList();

                //clear and fill the search list again
                SearchBox.Items.Clear();
                foreach (DatabasePackage package in searchComponents)
                {
                    SearchBox.Items.Add(new EditorSearchBoxItem(package, package.PackageName)
                    {
                        IsEnabled = true,
                        Content = package.PackageName
                    });
                }
            }
        }

        private void SearchBox_DropDownOpened(object sender, EventArgs e)
        {
            //Logging.Editor("[SearchBox_DropDownOpened]: Entered, search text = {0}", LogLevel.Debug, SearchBox.Text);
            //https://stackoverflow.com/a/1444332
            //https://stackoverflow.com/a/40117557
            //When a comboxbox gains focus you can disable the text highlighting (i.e. by selecting no text upon the GotFocus event)
            TextBox textBox = (TextBox)((ComboBox)sender).Template.FindName("PART_EditableTextBox", (ComboBox)sender);
            textBox.SelectionStart = ((ComboBox)sender).Text.Length;
            textBox.SelectionLength = 0;
        }

        private void SearchBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Logging.Editor("[SearchBox_PreviewMouseDown]: Entered, search text = {0}", LogLevel.Debug, SearchBox.Text);
            if (!SearchBox.IsDropDownOpen)
            {
                return;
            }

            Logging.Editor("[SearchBox_PreviewMouseDown]: DropDown is open, search text = {0}", LogLevel.Debug, SearchBox.Text);
            foreach (EditorSearchBoxItem item in SearchBox.Items)
            {
                if (item.IsHighlighted && item.IsMouseOver)
                {
                    //if it's the right mouse and we're in the conflicting packages view, the user is trying to add the element
                    if (e.RightButton == MouseButtonState.Pressed && RightTab.SelectedItem.Equals(ConflictingPackagesTab) && SelectedItem != null)
                    {
                        SelectablePackage selectedPackage = GetSelectablePackage(SelectedItem);
                        if (!(item.Package is SelectablePackage))
                        {
                            MessageBox.Show("Conflicting package must be a selectable package");
                            return;
                        }
                        SelectablePackage conflictingPackage = item.Package as SelectablePackage;

                        if (selectedPackage.UID.Equals(conflictingPackage.UID))
                        {
                            MessageBox.Show("Can't add itself as a conflict");
                            return;
                        }

                        Logging.Editor("Mouse right click with conflictingPackages visible, checking if package entry already exists");
                        foreach (ConflictingPackage conflictingPackageFromSelectedPackage in selectedPackage.ConflictingPackagesNew)
                        {
                            if (conflictingPackageFromSelectedPackage.ConflictingPackageName.Equals(conflictingPackage.PackageName) && conflictingPackageFromSelectedPackage.ConflictingPackageUID.Equals(conflictingPackage.UID))
                            {
                                Logging.Editor("Mouse right click with conflicting packages add, skipping adding cause already exists: {0}", LogLevel.Info, conflictingPackage.PackageName);
                                MessageBox.Show("Conflict entry already exists");
                                return;
                            }
                        }
                        Logging.Editor("Mouse right click with conflicting packages add, does not exist, adding");

                        ListBoxItem lbi = new ListBoxItem();
                        ConflictingPackage conflictingPackageEntryOfSelectedPackage = new ConflictingPackage()
                        {
                            ConflictingPackageName = conflictingPackage.PackageName,
                            ConflictingPackageUID = conflictingPackage.UID,
                            ParentSelectablePackage = selectedPackage,
                            ConflictingSelectablePackage = conflictingPackage
                        };
                        lbi.Tag = conflictingPackageEntryOfSelectedPackage;
                        lbi.Content = (lbi.Tag as ConflictingPackage).ToString();
                        PackageConflictingPackagesDisplay.Items.Add(lbi);

                        //then add-back so that the conflicting package references are circular
                        conflictingPackage.ConflictingPackagesNew.Add(new ConflictingPackage()
                        {
                            ConflictingPackageName = selectedPackage.PackageName,
                            ConflictingPackageUID = selectedPackage.UID,
                            ParentSelectablePackage = conflictingPackage,
                            ConflictingSelectablePackage = selectedPackage
                        });

                        UnsavedChanges = true;
                    }
                    else
                    {
                        OnSearchBoxCommitted(item, true);
                    }
                }
            }
        }

        private void OnSearchBoxCommitted(EditorSearchBoxItem item, bool fromMouse)
        {
            if (item == null)
            {
                Logging.Editor("User tried to search from item that does not exist, stopping");
                Logging.Editor("searched text: {0}", LogLevel.Info, SearchBox.Text);
                return;
            }

            item.Package.EditorTreeViewItem.Focusable = true;
            item.Package.EditorTreeViewItem.Focus();
            Logging.Editor("OnSearchBoxCommitted(), invoking async dispatch to bring into view item: {0}", LogLevel.Info, item.Package.PackageName);
            Dispatcher.InvokeAsync(() =>
            {
                item.Package.EditorTreeViewItem.BringIntoView();
                item.Package.EditorTreeViewItem.IsSelected = true;
                SelectDatabaseObject(item.Package, GetPackageTreeViewItem(GetDatabasePackage(SelectedItem)));
            }, System.Windows.Threading.DispatcherPriority.Background);
        }
        #endregion

        #region Drag drop code for media items
        private void PackageMediasDisplay_DragOver(object sender, DragEventArgs e)
        {
            DragDropTest.Text = "";
            if (DragDropTest.Visibility == Visibility.Hidden)
                DragDropTest.Visibility = Visibility.Visible;
            //e.source is the list box curently
            //e.original source is textblock, and data context is media item currently over

            if (PackageMediasDisplay.SelectedItem is Media mediaToMove)
            {
                if (e.OriginalSource is TextBlock block && block.DataContext is Media mediaOver)
                {
                    if (mediaOver.URL.Equals(mediaToMove.URL))
                    {
                        DragDropTest.Text = "Item can't be itself!";
                        return;
                    }
                    //try to get the entire text to fit...
                    string toMoveText = mediaToMove.URL.Length > 80 ? mediaToMove.URL.Substring(0, 80) : mediaToMove.URL;
                    string overText = mediaOver.URL.Length > 90 ? mediaOver.URL.Substring(0, 90) : mediaOver.URL;
                    DragDropTest.Text = string.Format("Move {0} below\n{1}", toMoveText, overText);
                }
            }
            else
                DragDropTest.Text = "Both items must be media!";
        }

        private void PackageMediasDisplay_Drop(object sender, DragEventArgs e)
        {
            DragDropTest.Text = "";
            if (DragDropTest.Visibility == Visibility.Visible)
                DragDropTest.Visibility = Visibility.Hidden;
            //selected item is itemToMove
            if (PackageMediasDisplay.SelectedItem is Media mediaToMove)
            {
                if (e.OriginalSource is TextBlock block && block.DataContext is Media mediaOver)
                {
                    PackageMediasDisplay.Items.Remove(mediaToMove);
                    PackageMediasDisplay.Items.Insert(PackageMediasDisplay.Items.IndexOf(mediaOver) + 1, mediaToMove);
                }
            }
        }

        private void PackageMediasDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && IsDragConfirmed(e.GetPosition(PackageMediasDisplay)) && !IsScrolling)
            {
                if (PackageMediasDisplay.SelectedItem is Media media)
                {
                    DragDrop.DoDragDrop(PackageMediasDisplay, media, DragDropEffects.Move);
                }
            }
        }

        private void PackageMediasDisplay_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                BeforeDragDropPoint = e.GetPosition(PackageMediasDisplay);
            }
        }

        private void PackageMediasDisplay_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                IsScrolling = false;
                if (DragDropTest.Visibility == Visibility.Visible)
                    DragDropTest.Visibility = Visibility.Hidden;
            }
        }

        private void PackageMediasDisplay_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                IsScrolling = true;
            }
        }
        #endregion

        #region Double click jump code
        private void PackageDependenciesDisplay_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PackageDependenciesDisplay.SelectedItem == null)
            {
                Logging.Editor("PackageDependenciesDisplay_MouseDoubleClick(), selectedItem is null, don't jump", LogLevel.Debug, SelectedItem);
                return;
            }

            //items in the dependences list are DatabaseLogic
            Logging.Editor("PackageDependenciesDisplay_MouseDoubleClick(), selectedItem = {0}", LogLevel.Info, SelectedItem);
            Dispatcher.InvokeAsync(() =>
            {
                DatabaseLogic selectedLogic = PackageDependenciesDisplay.SelectedItem as DatabaseLogic;
                selectedLogic.DependencyPackageRefrence.EditorTreeViewItem.BringIntoView();
                selectedLogic.DependencyPackageRefrence.EditorTreeViewItem.IsSelected = true;
                SelectDatabaseObject(selectedLogic.DependencyPackageRefrence, GetPackageTreeViewItem(GetDatabasePackage(SelectedItem)));
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void PackageConflictingPackagesDisplay_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PackageConflictingPackagesDisplay.SelectedItem == null)
            {
                Logging.Editor("PackageConflictingPackagesDisplay_MouseDoubleClick(), selectedItem is null, don't jump", LogLevel.Debug, SelectedItem);
                return;
            }

            //items in list are conflict or DatabasePackage or category
            Logging.Editor("PackageConflictingPackagesDisplay_MouseDoubleClick(), selectedItem = {0}", LogLevel.Info, SelectedItem);
            Dispatcher.InvokeAsync(() =>
            {
                if (PackageConflictingPackagesDisplay.SelectedItem is DatabasePackage selectedPackage)
                {
                    selectedPackage.EditorTreeViewItem.BringIntoView();
                    selectedPackage.EditorTreeViewItem.IsSelected = true;
                    SelectDatabaseObject(selectedPackage, GetPackageTreeViewItem(GetDatabasePackage(SelectedItem)));
                }
                else if (PackageConflictingPackagesDisplay.SelectedItem is Category category)
                {
                    category.EditorTreeViewItem.BringIntoView();
                    category.EditorTreeViewItem.IsSelected = true;
                    SelectDatabaseObject(category, GetPackageTreeViewItem(GetDatabasePackage(SelectedItem)));
                }
                else if (PackageConflictingPackagesDisplay.SelectedItem is ListBoxItem listBoxItem)
                {
                    SelectablePackage conflictingPackage = null;
                    ConflictingPackage conflictingPackageEntry = listBoxItem.Tag as ConflictingPackage;
                    if (conflictingPackageEntry != null)
                        conflictingPackage = conflictingPackageEntry.ConflictingSelectablePackage;

                    if (conflictingPackage == null)
                    {
                        MessageBox.Show("Cannot add a package that isn't a SelectablePackage");
                        return;
                    }

                    conflictingPackage.EditorTreeViewItem.BringIntoView();
                    conflictingPackage.EditorTreeViewItem.IsSelected = true;
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }
        #endregion

        #region Other buttons
        private void PackageJustCheckedForUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem == null)
                return;

            DatabasePackage packToWorkOn = null;
            if (SelectedItem is EditorComboBoxItem ecbi)
                packToWorkOn = ecbi.Package;
            else if (SelectedItem is DatabasePackage pack)
                packToWorkOn = pack;

            if (packToWorkOn != null)
            {
                long newTimestamp = CommonUtils.GetCurrentUniversalFiletimeTimestamp();
                Logging.Editor("Updated the last check for update of package {0}. Old: {1}, New: {2}", LogLevel.Debug, packToWorkOn.PackageName, packToWorkOn.LastUpdateCheck, newTimestamp);
                packToWorkOn.LastUpdateCheck = newTimestamp;
                PackageLastCheckForUpdateDisplay.Text = CommonUtils.ConvertFiletimeTimestampToDate(packToWorkOn.LastUpdateCheck);
                PackageLastCheckForUpdateDisplay.Tag = packToWorkOn.LastUpdateCheck;
            }
        }
        #endregion

        #region Patch buttons and events
        private void PackagePatchesDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PackagePatchesDisplay.SelectedItem == null)
                return;

            Patch selectedPatch = PackagePatchesDisplay.SelectedItem as Patch;

            DisplayPatch(selectedPatch);
        }

        private void MoveUpPatchButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSelection(PackagePatchesDisplay, true);
        }

        private void MoveDownPatchButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSelection(PackagePatchesDisplay, false);
        }

        private void RemovePatchButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelection(PackagePatchesDisplay);
        }

        private void AddNewPatchButton_Click(object sender, RoutedEventArgs e)
        {
            PackagePatchesDisplay.Items.Add(new Patch());
        }

        private void ApplyPatchChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (PackagePatchesDisplay.SelectedItem == null)
                return;

            //check to make sure at least valid settings are set before saving
            if (PatchPathCombobox.SelectedItem == null)
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

            SaveApplyPatch(PackagePatchesDisplay.SelectedItem as Patch);
        }

        private void PatchVersionCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //only patch versions 2+ will support the followPath option
            if (PatchVersionCombobox.SelectedIndex == 0)
            {
                PatchFollowPathSetting.IsChecked = false;
                PatchFollowPathSetting.IsEnabled = false;
            }
            else
            {
                PatchFollowPathSetting.IsEnabled = true;
            }
        }

        private void PatchTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PatchTypeCombobox.SelectedItem == null)
                return;

            //if the selection is json, enable the follow path selection box. else disable
            PatchModeCombobox.Items.Clear();
            if (PatchTypeCombobox.SelectedItem.Equals(Patch.TypeJson))
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
            else if (PatchTypeCombobox.SelectedItem.Equals(Patch.TypeXml))
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

            if (popOutReplacePatchDesigner != null)
                popOutReplacePatchDesigner.PatchReplaceTextbox.Clear();

            if (!string.IsNullOrWhiteSpace(patch.File))
                PatchFilePathTextbox.Text = patch.File;

            if (!string.IsNullOrWhiteSpace(patch.PatchPath))
                switch (patch.PatchPath.ToLower())
                {
                    default:
                        Logging.Editor("Unknown patchPath: {0}, set to app", LogLevel.Error, patch.PatchPath);
                        PatchPathCombobox.SelectedItem = 0;
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

            if (PatchTypeCombobox.Items.Contains(patch.Type))
                PatchTypeCombobox.SelectedItem = patch.Type;
            else
                PatchTypeCombobox.SelectedIndex = 0;

            if (!string.IsNullOrWhiteSpace(patch.Mode))
                PatchModeCombobox.SelectedItem = patch.Mode;

            //set the version. it's at least version 1
            if (PatchVersionCombobox.Items.Contains(patch.Version))
                PatchVersionCombobox.SelectedItem = patch.Version;
            else
                PatchVersionCombobox.SelectedIndex = 0;

            //only set the followPath setting if the version is > 1
            //else it is set off by the selectedValueChanged event in PatchVersionCombobox
            if (patch.Version > 1)
            {
                PatchFollowPathSetting.IsChecked = patch.FollowPath;
            }
            else if (patch.FollowPath && (patch.Version == 1 || !patch.Type.Equals(Patch.TypeJson)))
            {
                Logging.Editor("Patch option followPath can't be enabled (not supported). Disabling.", LogLevel.Error);
                Logging.Editor("Version: {0}, Type: {1}", LogLevel.Error, patch.Version, patch.Type);
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
            UnsavedChanges = true;

            //save all UI settings to patch object
            patch.PatchPath = PatchPathCombobox.SelectedItem as string;
            patch.Type = PatchTypeCombobox.SelectedItem as string;
            patch.Mode = PatchModeCombobox.SelectedItem as string;
            patch.Version = (int)PatchVersionCombobox.SelectedItem;
            patch.FollowPath = (bool)PatchFollowPathSetting.IsChecked;
            patch.File = PatchFilePathTextbox.Text;

            if (patch.Type.ToLower().Equals(Patch.TypeRegex1) || patch.Type.ToLower().Equals(Patch.TypeRegex2))
            {
                patch.Line = PatchLinesPathTextbox.Text;
            }
            else
            {
                patch.Path = PatchLinesPathTextbox.Text;
            }

            patch.Search = PatchSearchTextbox.Text;
            patch.Replace = PatchReplaceTextbox.Text;

            PackagePatchesDisplay.Items.Refresh();
        }

        private void PopOutReplaceBlockCB_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)PopOutReplaceBlockCB.IsChecked)
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
        private void PopOutReplacePatchDesigner_Closed(object sender, EventArgs e)
        {
            PopOutReplaceBlockCB.IsChecked = false;
            PopOutReplaceBlockCB_Click(null, null);
        }

        private void TestPatchButton_Click(object sender, RoutedEventArgs e)
        {
            //file dialog to select file to patch
            Logging.Info("Select a file from the dialog window");
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select a file to patch",
                AddExtension = true,
                CheckPathExists = true,
                CheckFileExists = true,
                Multiselect = false
            };

            if (!(bool)openFileDialog.ShowDialog())
                return;

            //run patcher (see patch designer for reference)
            Logging.Editor("Checking UI elements for valid patch information...", LogLevel.Info);

            //make new patch element
            Patch patchToTest = new Patch(PackagePatchesDisplay.SelectedItem as Patch)
            {
                CompletePath = openFileDialog.FileName,
                File = openFileDialog.FileName
            };

            //check input from UI left panel side:
            //file location
            Logging.Editor("File to Patch location: {0}", LogLevel.Info, openFileDialog.FileName);

            //check patch type
            if (PatchTypeCombobox.SelectedItem == null)
            {
                Logging.Editor("Invalid Patch Type, aborting", LogLevel.Info);
                return;
            }
            patchToTest.Type = PatchTypeCombobox.SelectedItem as string;

            //check patch mode
            switch (patchToTest.Type.ToLower())
            {
                case Patch.TypeRegex1:
                case Patch.TypeRegex2:
                    if (!string.IsNullOrWhiteSpace(PatchModeCombobox.SelectedItem as string))
                    {
                        Logging.Editor("Type=regex, invalid patch type: {0}", LogLevel.Error, PatchModeCombobox.SelectedItem as string);
                        Logging.Editor("valid types are: (null)");
                        return;
                    }
                    //set the lines
                    if (string.IsNullOrWhiteSpace(PatchLinesPathTextbox.Text))
                    {
                        Logging.Editor("Type=regex, Lines to patch is blank", LogLevel.Error);
                        return;
                    }
                    else
                    {
                        patchToTest.Line = PatchLinesPathTextbox.Text;
                    }
                    break;
                case Patch.TypeXml:
                    //check if path/lines is valid (has string values)
                    if (string.IsNullOrWhiteSpace(PatchLinesPathTextbox.Text))
                    {
                        Logging.Editor("invalid xpath", LogLevel.Error);
                        return;
                    }
                    if (!Patch.ValidXmlModes.Contains((PatchModeCombobox.SelectedItem as string).ToLower()))
                    {
                        Logging.Editor("Type=xml, invalid patch type: {0}", LogLevel.Error, PatchModeCombobox.SelectedItem as string);
                        Logging.Editor("valid types are: {0}", LogLevel.Error, string.Join(",", Patch.ValidXmlModes));
                        return;
                    }
                    patchToTest.Path = PatchLinesPathTextbox.Text;
                    break;
                case Patch.TypeJson:
                    //check if path/lines is valid (has string values)
                    if (string.IsNullOrWhiteSpace(PatchLinesPathTextbox.Text))
                    {
                        Logging.Editor("invalid jsonpath");
                        return;
                    }
                    if (!Patch.ValidJsonModes.Contains((PatchModeCombobox.SelectedItem as string).ToLower()))
                    {
                        Logging.Editor("Type=json, invalid patch type: {0}", LogLevel.Info, PatchModeCombobox.SelectedItem as string);
                        Logging.Editor("valid types are: {0}", LogLevel.Info, string.Join(",", Patch.ValidJsonModes));
                        return;
                    }
                    patchToTest.Path = PatchLinesPathTextbox.Text;
                    break;
                default:
                    throw new BadMemeException("invalid patch type, but you should probably make this a enum not strings");
            }

            patchToTest.Mode = PatchModeCombobox.SelectedItem as string;
            //check followPath true ONLY for json
            if (!patchToTest.Type.Equals(Patch.TypeJson) && (bool)PatchFollowPathSetting.IsChecked)
            {
                Logging.Editor("Types=json, followPathSetting must be false!");
                return;
            }

            //check search and replace
            if (string.IsNullOrWhiteSpace(PatchReplaceTextbox.Text) && string.IsNullOrWhiteSpace(PatchSearchTextbox.Text))
            {
                Logging.Editor("patch replace and search are blank, invalid patch");
                return;
            }

            if (string.IsNullOrWhiteSpace(PatchSearchTextbox.Text))
            {
                Logging.Editor("patch search is blank (is this the intent?)", LogLevel.Warning);
            }
            patchToTest.Search = PatchSearchTextbox.Text;

            if (string.IsNullOrWhiteSpace(PatchReplaceTextbox.Text))
            {
                Logging.Editor("patch replace is blank (is this the intent?)", LogLevel.Info);
            }
            patchToTest.Replace = PatchReplaceTextbox.Text;

            //put patch into patch test methods
            Logging.Editor("Running patch...", LogLevel.Info);
            switch (Patcher.RunPatchFromEditor(patchToTest))
            {
                case PatchExitCode.Error:
                    Logging.Editor("Patch failed with errors. Check the log for details.", LogLevel.Error);
                    break;
                case PatchExitCode.Warning:
                    Logging.Editor("Patch completed with warnings. Check the log for details.", LogLevel.Warning);
                    break;
                case PatchExitCode.Success:
                    Logging.Editor("Patch completed successfully!", LogLevel.Info);
                    break;
            }
        }
        #endregion

        #region Atlas buttons and events
        private void PackageAtlasesDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PackageAtlasesDisplay.SelectedItem == null)
                return;
            DisplayAtlas(PackageAtlasesDisplay.SelectedItem as Atlas);
        }

        private void MoveUpAtlasButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSelection(PackageAtlasesDisplay, true);
        }

        private void MoveDownAtlasButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSelection(PackageAtlasesDisplay, false);
        }

        private void RemoveAtlasButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelection(PackageAtlasesDisplay);
        }

        private void AddNewAtlasButton_Click(object sender, RoutedEventArgs e)
        {
            PackageAtlasesDisplay.Items.Add(new Atlas());
        }

        private void ApplyAtlasChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (PackageAtlasesDisplay.SelectedItem == null)
                return;
            SaveAtlas(PackageAtlasesDisplay.SelectedItem as Atlas);
            PackageAtlasesDisplay.Items.Refresh();
        }

        private void DisplayAtlas(Atlas atlas)
        {
            AtlasPkgTextbox.Text = atlas.Pkg;
            AtlasDirectoryInArchiveTextbox.Text = atlas.DirectoryInArchive;
            AtlasAtlasFileTextbox.Text = atlas.AtlasFile;
            AtlasMapFileTextbox.Text = atlas.MapFile;
            AtlasPowOf2Checkbox.IsChecked = atlas.PowOf2;
            AtlasSquareCheckbox.IsChecked = atlas.Square;
            AtlasFastImagePackerCheckbox.IsChecked = atlas.FastImagePacker;
            AtlasPaddingTextbox.Text = atlas.Padding.ToString();
            AtlasAtlasWidthTextbox.Text = atlas.AtlasWidth.ToString();
            AtlasAtlasHeightTextbox.Text = atlas.AtlasHeight.ToString();
            AtlasAtlasSaveDirectoryTextbox.Text = atlas.AtlasSaveDirectory;
            AtlasImageFoldersTextbox.Text = string.Join(Environment.NewLine, atlas.ImageFolders);
        }

        private void SaveAtlas(Atlas atlas)
        {
            UnsavedChanges = true;
            atlas.Pkg = AtlasPkgTextbox.Text;
            atlas.DirectoryInArchive = AtlasDirectoryInArchiveTextbox.Text;
            atlas.AtlasFile = AtlasAtlasFileTextbox.Text;
            atlas.MapFile = AtlasMapFileTextbox.Text;
            atlas.PowOf2 = (bool)AtlasPowOf2Checkbox.IsChecked;
            atlas.Square = (bool)AtlasSquareCheckbox.IsChecked;
            atlas.FastImagePacker = (bool)AtlasFastImagePackerCheckbox.IsChecked;
            atlas.Padding = CommonUtils.ParseInt(AtlasPaddingTextbox.Text, 1);
            atlas.AtlasWidth = CommonUtils.ParseInt(AtlasAtlasWidthTextbox.Text, 0);
            atlas.AtlasHeight = CommonUtils.ParseInt(AtlasAtlasHeightTextbox.Text, 0);
            atlas.AtlasSaveDirectory = AtlasAtlasSaveDirectoryTextbox.Text;
            atlas.ImageFolders.Clear();
            foreach (string s in AtlasImageFoldersTextbox.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                atlas.ImageFolders.Add(s.Trim());
        }
        #endregion

        #region XmlUnpack buttons and events
        private void PackageXmlUnpackDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PackageXmlUnpackDisplay.SelectedItem == null)
                return;
            DisplayXmlUnpack(PackageXmlUnpackDisplay.SelectedItem as XmlUnpack);
        }

        private void MoveUpXmlUnpackButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSelection(PackageXmlUnpackDisplay, true);
        }

        private void MoveDownXmlUnpackButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSelection(PackageXmlUnpackDisplay, false);
        }

        private void RemoveXmlUnpackButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelection(PackageXmlUnpackDisplay);
        }

        private void AddNewXmlUnpackButton_Click(object sender, RoutedEventArgs e)
        {
            PackageXmlUnpackDisplay.Items.Add(new XmlUnpack());
        }

        private void ApplyXmlUnpackChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (PackageXmlUnpackDisplay.SelectedItem == null)
                return;
            SaveXmlUnpack(PackageXmlUnpackDisplay.SelectedItem as XmlUnpack);
            PackageXmlUnpackDisplay.Items.Refresh();
        }

        private void DisplayXmlUnpack(XmlUnpack xmlUnpack)
        {
            XmlUnpackPkgTextbox.Text = xmlUnpack.Pkg;
            XmlUnpackDirectoryInArchiveTextbox.Text = xmlUnpack.DirectoryInArchive;
            XmlUnpackFilenameTextbox.Text = xmlUnpack.FileName;
            XmlUnpackExtractDirectoryTextbox.Text = xmlUnpack.ExtractDirectory;
            XmlUnpackNewFilenameTextbox.Text = xmlUnpack.NewFileName;
        }

        private void SaveXmlUnpack(XmlUnpack xmlUnpack)
        {
            UnsavedChanges = true;
            xmlUnpack.Pkg = XmlUnpackPkgTextbox.Text;
            xmlUnpack.DirectoryInArchive = XmlUnpackDirectoryInArchiveTextbox.Text;
            xmlUnpack.FileName = XmlUnpackFilenameTextbox.Text;
            xmlUnpack.ExtractDirectory = XmlUnpackExtractDirectoryTextbox.Text;
            xmlUnpack.NewFileName = XmlUnpackNewFilenameTextbox.Text;
        }
        #endregion

        #region Shortcut buttons and events
        private void PackageShortcutDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PackageShortcutDisplay.SelectedItem == null)
                return;
            DisplayShortcut(PackageShortcutDisplay.SelectedItem as Shortcut);
        }

        private void MoveUpShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSelection(PackageShortcutDisplay, true);
        }

        private void MoveDownShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSelection(PackageShortcutDisplay, false);
        }

        private void RemoveShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelection(PackageShortcutDisplay);
        }

        private void AddNewShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            PackageShortcutDisplay.Items.Add(new Shortcut());
        }

        private void ApplyShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            if (PackageShortcutDisplay.SelectedItem == null)
                return;
            SaveShortcut(PackageShortcutDisplay.SelectedItem as Shortcut);
            PackageShortcutDisplay.Items.Refresh();
        }

        private void DisplayShortcut(Shortcut shortcut)
        {
            ShortcutPathTextbox.Text = shortcut.Path;
            ShortcutNameTextbox.Text = shortcut.Name;
            ShortcutEnabledCheckbox.IsChecked = shortcut.Enabled;
        }

        private void SaveShortcut(Shortcut shortcut)
        {
            UnsavedChanges = true;
            shortcut.Path = ShortcutPathTextbox.Text;
            shortcut.Name = ShortcutNameTextbox.Text;
            shortcut.Enabled = (bool)ShortcutEnabledCheckbox.IsChecked;
        }
        #endregion

        #region instruction generic methods
        private void MoveUpSelection(ListBox listbox)
        {
            if (listbox.SelectedIndex == 0)
                return;

            object itemToMove = listbox.SelectedItem;
            int oldIndex = listbox.SelectedIndex;
            listbox.Items.Remove(itemToMove);
            listbox.Items.Insert(oldIndex - 1, itemToMove);
        }

        private void MoveDownSelection(ListBox listbox)
        {
            if (listbox.SelectedIndex == listbox.Items.Count - 1)
                return;

            object itemToMove = listbox.SelectedItem;
            int oldIndex = listbox.SelectedIndex;
            listbox.Items.Remove(itemToMove);
            listbox.Items.Insert(oldIndex + 1, itemToMove);
        }

        private void MoveSelection(ListBox listbox, bool UpDown)
        {
            //up=true, down=false
            if (listbox.SelectedItem == null)
                return;

            if (listbox.Items.Count < 2)
                return;

            if (UpDown)
                MoveUpSelection(listbox);
            else
                MoveDownSelection(listbox);
        }

        private void RemoveSelection(ListBox listbox)
        {
            if (listbox.SelectedItem == null)
                return;

            listbox.Items.Remove(listbox.SelectedItem);
        }
        #endregion
    }
}
