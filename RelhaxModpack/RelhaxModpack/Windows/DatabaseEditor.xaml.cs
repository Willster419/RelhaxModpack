using Microsoft.Win32;
using RelhaxModpack.DatabaseComponents;
using RelhaxModpack.UIComponents;
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

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for DatabaseEditor.xaml
    /// </summary>
    public partial class DatabaseEditor : RelhaxWindow
    {
        //private
        private EditorSettings EditorSettings;
        private List<DatabasePackage> GlobalDependencies = new List<DatabasePackage>();
        private List<Dependency> Dependencies = new List<Dependency>();
        private List<Category> ParsedCategoryList = new List<Category>();
        private OpenFileDialog OpenDatabaseDialog;
        private SaveFileDialog SaveDatabaseDialog;
        private OpenFileDialog OpenZipFileDialog;
        private SaveFileDialog SaveZipFileDialog;
        private OpenFileDialog OpenPictureDialog;
        private System.Windows.Forms.Timer DragDropTimer = new System.Windows.Forms.Timer() { Enabled = false, Interval = 1000 };
        private TreeViewItem ItemToExpand;
        private Point BeforeDragDropPoint;
        private bool IsScrolling = false;
        //private bool AlreadyLoggedMouseMove = false;
        private bool AlreadyLoggedScroll = false;
        private bool Init = true;
        private object SelectedItem = null;
        private Preview Preview;
        private bool UnsavedChanges = false;
        System.Windows.Forms.Timer ReselectOldItem = new System.Windows.Forms.Timer() { Interval = 50 };
        private string[] UIHeaders = new string[]
        {
            "-----Global Dependencies-----",
            "-----Dependencies-----",
        };

        //public
        /// <summary>
        /// Indicates if this editor instance was launched from the MainWindow or from command line
        /// </summary>
        /// <remarks>This changes the behavior of the logging for the editor</remarks>
        public bool LaunchedFromMainWindow = false;

        #region Stuff
        /// <summary>
        /// Create an instance of the DatabaseEditor
        /// </summary>
        public DatabaseEditor()
        {
            InitializeComponent();
        }

        private void OnApplicationLoad(object sender, RoutedEventArgs e)
        {
            ReselectOldItem.Tick += AwesomeHack_Tick;
            Logging.Editor("Editor start");
            EditorSettings = new EditorSettings();
            Logging.Editor("Loading editor settings");
            if (!Settings.LoadSettings(Settings.EditorSettingsFilename, typeof(EditorSettings), null, EditorSettings))
            {
                Logging.Editor("Failed to load editor settings, using defaults", LogLevel.Warning);
            }
            else
            {
                Logging.Editor("Editor settings loaded success");
            }

#warning forcing sort categories to be off
            Logging.Editor("forcing sort categories off", LogLevel.Warning);
            EditorSettings.SortDatabaseList = false;

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
                    Logging.Editor("file does not exist");
                }
            }

            //load the trigger box with trigger options
            LoadedTriggersComboBox.Items.Clear();
            foreach (Trigger t in InstallerComponents.InstallEngine.Triggers)
            {
                LoadedTriggersComboBox.Items.Add(t.Name);
            }

            //hook up timer
            DragDropTimer.Tick += OnDragDropTimerTick;
            SearchBox.Items.Clear();

            //set the items for the triggers combobox. this only needs to be done once anyways
            LoadedTriggersComboBox.Items.Clear();
            foreach (string s in InstallerComponents.InstallEngine.CompleteTriggerList)
                LoadedTriggersComboBox.Items.Add(s);
            Init = false;

            if (!CommandLineSettings.SkipUpdate)
            {
                if (!LaunchedFromMainWindow)
                {
                    Task.Run(async () =>
                    {
                        if (!await Utils.IsManagerUptoDate(Utils.GetApplicationVersion()))
                        {
                            MessageBox.Show("Your application is out of date. Please launch the application normally to update");
                        }
                    });
                }
            }
            else
            {
                Logging.Editor("Skipping update check from command line setting", LogLevel.Warning);
            }
        }

        private void AwesomeHack_Tick(object sender, EventArgs e)
        {
            if(SelectedItem is DatabasePackage db)
            {
                db.EditorTreeViewItem.IsSelected = true;
                ReselectOldItem.Enabled = false;
                ReselectOldItem.Stop();
            }
            else if (SelectedItem is EditorComboBoxItem edit)
            {
                edit.Package.EditorTreeViewItem.IsSelected = true;
                ReselectOldItem.Enabled = false;
                ReselectOldItem.Stop();
            }
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

        private void RelhaxWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (UnsavedChanges)
            {
                if (MessageBox.Show("You have unsaved changes, return to editor?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
            if (!Logging.IsLogDisposed(Logfiles.Editor))
            {
                if (Logging.IsLogOpen(Logfiles.Editor))
                    Logging.Editor("Saving editor settings");
                if (Settings.SaveSettings(Settings.EditorSettingsFilename, typeof(EditorSettings), null, EditorSettings))
                    if (Logging.IsLogOpen(Logfiles.Editor))
                        Logging.Editor("Editor settings saved");
                Logging.DisposeLogging(Logfiles.Editor);
            }
        }

        private int GetMaxPatchGroups()
        {
            return Utils.GetMaxPatchGroupNumber(Utils.GetFlatList(GlobalDependencies, Dependencies, null, ParsedCategoryList));
        }

        private int GetMaxInstallGroups()
        {
            return Utils.GetMaxInstallGroupNumber(Utils.GetFlatList(GlobalDependencies, Dependencies, null, ParsedCategoryList));
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
            LoadDatabaseView(GlobalDependencies, Dependencies, ParsedCategoryList);
            LoadInstallView(GlobalDependencies, Dependencies, ParsedCategoryList);
            LoadPatchView(GlobalDependencies, Dependencies, ParsedCategoryList);
        }

        private void LoadDatabaseView(List<DatabasePackage> globalDependencies, List<Dependency> dependnecies, List<Category> parsedCategoryList, int numToAddEnd = 5)
        {
            //clear and reset
            DatabaseTreeView.Items.Clear();
            //RESET UI TODO? or don't do it?

            //if user requests, sort the lists like the selection list does
            if (EditorSettings.SortDatabaseList)
                Utils.SortDatabase(parsedCategoryList);

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

            //add the category, then add each level recursivly
            foreach (Category cat in parsedCategoryList)
            {
                TreeViewItem CategoryHeader = new TreeViewItem() { Header = cat };
                DatabaseTreeView.Items.Add(CategoryHeader);
                LoadUI(CategoryHeader, cat.Packages);
            }

            //adding the spacing that dirty wants...
            for (int i = 0; i < numToAddEnd; i++)
            {
                DatabaseTreeView.Items.Add(string.Empty);
            }
        }

        private void LoadInstallView(List<DatabasePackage> globalDependencies, List<Dependency> dependnecies, List<Category> parsedCategoryList, int numToAddEnd = 5)
        {
            //load the install and patch groups
            InstallGroupsTreeView.Items.Clear();
            //make a flat list (can be used in patchGroup as well)
            List<DatabasePackage> allFlatList = Utils.GetFlatList(GlobalDependencies, dependnecies, null, parsedCategoryList);
            //make an array of group headers
            TreeViewItem[] installGroupHeaders = new TreeViewItem[Utils.GetMaxInstallGroupNumber(allFlatList) + 1];
            //for each group header, get the list of packages that have an equal install group number
            //hey while we're at it let's add the items to the instal group dispaly box
            PackageInstallGroupDisplay.Items.Clear();
            for (int i = 0; i < installGroupHeaders.Count(); i++)
            {
                PackageInstallGroupDisplay.Items.Add(i);
                installGroupHeaders[i] = new TreeViewItem() { Header = string.Format("---Install Group {0}---", i), Tag = i };
                InstallGroupsTreeView.Items.Add(installGroupHeaders[i]);
                installGroupHeaders[i].Items.Clear();
                foreach (DatabasePackage packageWithEqualGroupNumber in allFlatList.Where(package => package.InstallGroup == i).ToList())
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

        private void LoadPatchView(List<DatabasePackage> globalDependencies, List<Dependency> dependnecies, List<Category> parsedCategoryList, int numToAddEnd = 5)
        {
            //do the same for patchgroups
            PatchGroupsTreeView.Items.Clear();
            //make a flat list (can be used in patchGroup as well)
            List<DatabasePackage> allFlatList = Utils.GetFlatList(GlobalDependencies, dependnecies, null, parsedCategoryList);
            TreeViewItem[] patchGroupHeaders = new TreeViewItem[Utils.GetMaxPatchGroupNumber(allFlatList) + 1];
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
            SaveSelectionBeforeLeaveSetting.IsChecked = EditorSettings.SaveSelectionBeforeLeave;
            SortCategoriesSetting.IsChecked = EditorSettings.SortDatabaseList;
            ApplyBehaviorDefaultSetting.IsChecked = EditorSettings.ApplyBehavior == ApplyBehavior.Default ? true : false;
            ApplyBehaviorApplyTriggersSaveSetting.IsChecked = EditorSettings.ApplyBehavior == ApplyBehavior.ApplyTriggersSave ? true : false;
            ApplyBehaviorSaveTriggersApplySetting.IsChecked = EditorSettings.ApplyBehavior == ApplyBehavior.SaveTriggersApply ? true : false;
            ShowConfirmOnPackageApplySetting.IsChecked = EditorSettings.ShowConfirmationOnPackageApply;
            ShowConfirmOnPackageAddRemoveEditSetting.IsChecked = EditorSettings.ShowConfirmationOnPackageAddRemoveMove;
            DefaultSaveLocationSetting.Text = EditorSettings.DefaultEditorSaveLocation;
            FtpUpDownAutoCloseTimoutSlider.Value = EditorSettings.FTPUploadDownloadWindowTimeout;
            FtpUpDownAutoCloseTimoutDisplayLabel.Text = EditorSettings.FTPUploadDownloadWindowTimeout.ToString();
            SaveDatabaseLegacySetting.IsChecked = EditorSettings.SaveAsDatabaseVersion == DatabaseXmlVersion.Legacy ? true : false;
            SaveDatabaseOnePointOneSetting.IsChecked = EditorSettings.SaveAsDatabaseVersion == DatabaseXmlVersion.OnePointOne ? true : false;
            SelectAutoUpdateWorkDirectoryTextbox.Text = EditorSettings.AutoUpdaterWorkDirectory;
        }
        #endregion

        #region Other UI events

        private void LeftTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Init)
                return;
            if (LeftTabView.SelectedItem is TabItem selectedTab)
            {
                if (selectedTab.Equals(DatabaseViewTab))
                {
                    RightTab.IsEnabled = true;
                    SearchBox.IsEnabled = true;
                    RemoveDatabaseObjectButton.IsEnabled = true;
                    MoveDatabaseObjectButton.IsEnabled = true;
                    AddDatabaseObjectButton.IsEnabled = true;
                    //check if the database is actually loaded before Loading the database view
                    if (GlobalDependencies.Count == 0)
                    {
                        Logging.Editor("Database is not yet loaded, skipping UI loading");
                    }
                    else
                    {
                        Logging.Editor("Database is loaded, UI loading()");
                        LoadDatabaseView(GlobalDependencies, Dependencies, ParsedCategoryList);
                    }
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
                    else
                    {
                        Logging.Editor("Database is loaded, UI loading()");
                        LoadInstallView(GlobalDependencies, Dependencies, ParsedCategoryList);
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
                    else
                    {
                        Logging.Editor("Database is loaded, UI loading()");
                        LoadPatchView(GlobalDependencies, Dependencies, ParsedCategoryList);
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

        private void ResetRightPanels(DatabasePackage package)
        {
            Logging.Editor("ResetRightPanels(), package type = {0}, name= {1}", LogLevel.Info, package == null ? "(null)" : package.GetType().ToString(), package == null ? "(null)" : package.PackageName);
            //for each tab, disable all components. then enable them back of tye type of database object
            List<Control> controlsToDisable = new List<Control>();
            foreach (TabItem tabItem in RightTab.Items)
            {
                foreach (FrameworkElement element in Utils.GetAllWindowComponentsLogical(tabItem, false))
                {
                    //if it's a common element used in the panel, then disable it
                    if (element is CheckBox || element is ComboBox || element is Button || element is TextBox || element is ListBox)
                        controlsToDisable.Add((Control)element);
                    //also clear it's data for each type
                    if (element is CheckBox box)
                        box.IsChecked = false;
                    else if (element is ComboBox cbox)
                    {
                        if (cbox.Name.Equals(nameof(PackageInstallGroupDisplay)) || cbox.Name.Equals(nameof(PackagePatchGroupDisplay)) ||
                            cbox.Name.Equals(nameof(LoadedDependenciesList)) || cbox.Name.Equals(nameof(LoadedTriggersComboBox)) ||
                            cbox.Name.Equals(nameof(LoadedLogicsList)) || cbox.Name.Equals(nameof(PackageTypeDisplay)) ||
                            cbox.Name.Equals(nameof(MediaTypesList)))
                        {
                            cbox.SelectedIndex = -1;
                            continue;
                        }
                        else
                            cbox.Items.Clear();
                    }
                    else if (element is TextBox tbox && !tbox.Name.Equals(nameof(CurrentSupportedTriggers)))
                        tbox.Text = string.Empty;
                    else if (element is ListBox lbox)
                        lbox.Items.Clear();
                }
            }
            //there's a couple that don't need to be disabled
            if (controlsToDisable.Contains(CurrentSupportedTriggers))
                controlsToDisable.Remove(CurrentSupportedTriggers);
            else
                throw new BadMemeException("but it's there i swear");
            //disable the components
            foreach (Control control in controlsToDisable)
                control.IsEnabled = false;

            //enable components by type
            if (package == null)
            {
                foreach (FrameworkElement control in Utils.GetAllWindowComponentsLogical(DependenciesTab, false))
                {
                    if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                        control.IsEnabled = true;
                }
                PackageNameDisplay.IsEnabled = true;
                ApplyButton.IsEnabled = true;
            }
            else if (package is DatabasePackage)
            {
                //basic tab is always difficult
                PackagePackageNameDisplay.IsEnabled = true;
                PackageStartAddressDisplay.IsEnabled = true;
                PackageZipFileDisplay.IsEnabled = true;
                PackageEndAddressDisplay.IsEnabled = true;
                PackageDevURLDisplay.IsEnabled = true;
                PackageVersionDisplay.IsEnabled = true;
                PackageAuthorDisplay.IsEnabled = true;
                PackageInstallGroupDisplay.IsEnabled = true;
                PackagePatchGroupDisplay.IsEnabled = true;
                PackageLastUpdatedDisplay.IsEnabled = true;
                PackageLogAtInstallDisplay.IsEnabled = true;
                PackageEnabledDisplay.IsEnabled = true;//kinda meta
                ApplyButton.IsEnabled = true;
                ZipDownload.IsEnabled = true;
                ZipUload.IsEnabled = true;
                //all have internal notes and triggers
                foreach (FrameworkElement control in Utils.GetAllWindowComponentsLogical(TriggersTab, false))
                {
                    if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                        control.IsEnabled = true;
                }
                foreach (FrameworkElement control in Utils.GetAllWindowComponentsLogical(InternalNotesTab, false))
                {
                    if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                        control.IsEnabled = true;
                }
                if (package is Dependency dependency || package is SelectablePackage spackage)
                {
                    //dependency and selectable package both have dependencies
                    foreach (FrameworkElement control in Utils.GetAllWindowComponentsLogical(DependenciesTab, false))
                    {
                        if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                            control.IsEnabled = true;
                    }
                    //conflicting packages gets used for showing elements that are used by the dependency
                    foreach (FrameworkElement control in Utils.GetAllWindowComponentsLogical(ConflictingPackagesTab, false))
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
                        //enable remaining tabs
                        foreach (FrameworkElement control in Utils.GetAllWindowComponentsLogical(DescriptionTab, false))
                        {
                            if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                                control.IsEnabled = true;
                        }
                        foreach (FrameworkElement control in Utils.GetAllWindowComponentsLogical(UpdateNotesTab, false))
                        {
                            if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                                control.IsEnabled = true;
                        }
                        foreach (FrameworkElement control in Utils.GetAllWindowComponentsLogical(MediasTab, false))
                        {
                            if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                                control.IsEnabled = true;
                        }
                        foreach (FrameworkElement control in Utils.GetAllWindowComponentsLogical(UserDatasTab, false))
                        {
                            if (control is CheckBox || control is ComboBox || control is Button || control is TextBox || control is ListBox)
                                control.IsEnabled = true;
                        }
                    }
                }
            }

            //reload the list of all dependencies to make sure it's always accurate
            LoadedDependenciesList.Items.Clear();
            foreach (Dependency d in Dependencies)
                LoadedDependenciesList.Items.Add(d);
        }
        #endregion

        #region Show and Apply database methods

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
                if (SelectedItem is EditorComboBoxItem || SelectedItem is DatabasePackage)
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
                        ReselectOldItem.Enabled = true;
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
        }

        private void ShowDatabaseCategory(Category category)
        {
            ResetRightPanels(null);
            Logging.Editor("ShowDatabaseCategory(), category showing = {0}", LogLevel.Info, category.Name);
            foreach (DatabaseLogic logic in category.Dependencies)
                PackageDependenciesDisplay.Items.Add(logic);
            PackageNameDisplay.Text = category.Name;
        }

        private void ShowDatabasePackage(DatabasePackage package)
        {
            ResetRightPanels(package);
            Logging.Editor("ShowDatabaseObject(), package showing = {0}", LogLevel.Info, package.PackageName);
            //load all items in the databasePackage level first
            //basic tab
            //set text field texts
            PackagePackageNameDisplay.Text = package.PackageName;
            PackageStartAddressDisplay.Text = package.StartAddress;
            PackageZipFileDisplay.Text = package.ZipFile;
            PackageEndAddressDisplay.Text = package.EndAddress;
            PackageVersionDisplay.Text = package.Version;
            PackageAuthorDisplay.Text = package.Author;
            PackageLastUpdatedDisplay.Text = Utils.ConvertFiletimeTimestampToDate(package.Timestamp);

            //locate and select the patchGroup and installGroup of the package
            foreach (int i in PackageInstallGroupDisplay.Items)
            {
                if (i == package.InstallGroup)
                {
                    PackageInstallGroupDisplay.SelectedItem = i;
                    break;
                }
            }
            foreach (int i in PackagePatchGroupDisplay.Items)
            {
                if (i == package.PatchGroup)
                {
                    PackagePatchGroupDisplay.SelectedItem = i;
                    break;
                }
            }

            //some checkboxes
            PackageLogAtInstallDisplay.IsChecked = package.LogAtInstall;
            PackageEnabledDisplay.IsChecked = package.Enabled;

            //devURL
            //each url is separated by newline characters "\n"
            //should be displayed with newlines already, so no change needed
            PackageDevURLDisplay.Text = Utils.MacroReplace(package.DevURL, ReplacementTypes.TextUnescape);

            //internal notes
            PackageInternalNotesDisplay.Text = Utils.MacroReplace(package.InternalNotes, ReplacementTypes.TextUnescape);

            //triggers
            foreach (string s in package.Triggers)
                PackageTriggersDisplay.Items.Add(s);

            //then handle if dependency
            if (package is Dependency dependency)
            {
                //display all dependencies that the selected dependency uses
                foreach (DatabaseLogic d in dependency.Dependencies)
                    PackageDependenciesDisplay.Items.Add(d);

                //change the "conflicting packages" tab into a "dependency usage" tab
                ConflictingPackagesTab.Header = "Dependency Usage";
                ConflictingPackagesMessagebox.Text = "Above is list packages that use this dependency";

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
                foreach (SelectablePackage selectablePackage in Utils.GetFlatSelectablePackageList(ParsedCategoryList))
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
                PackageShowInSearchListDisplay.IsChecked = selectablePackage.ShowInSearchList;
                PackageNameDisplay.Text = selectablePackage.Name;
                PackageTypeDisplay.SelectedItem = selectablePackage.Type;
                PackageLevelDisplay.Text = selectablePackage.Level.ToString();
                PackageVisibleDisplay.IsChecked = selectablePackage.Visible;
                //PackageDescriptionDisplay.Text = Utils.MacroReplace(selectablePackage.Description,ReplacementTypes.TextUnescape);
                //PackageUpdateNotesDisplay.Text = Utils.MacroReplace(selectablePackage.UpdateComment,ReplacementTypes.TextUnescape);
                PackageDescriptionDisplay.Text = selectablePackage.Description;
                PackageUpdateNotesDisplay.Text = selectablePackage.UpdateComment;
                foreach (DatabaseLogic d in selectablePackage.Dependencies)
                    PackageDependenciesDisplay.Items.Add(d);
                foreach (Media media in selectablePackage.Medias)
                    PackageMediasDisplay.Items.Add(media);
                foreach (UserFile data in selectablePackage.UserFiles)
                    PackageUserdatasDisplay.Items.Add(data);
                PackageConflictingPackagesDisplay.Items.Clear();
                foreach (string s in selectablePackage.ConflictingPackages)
                    PackageConflictingPackagesDisplay.Items.Add(s);

                //set the conflicting packages tab
                ConflictingPackagesTab.Header = "Conflicting Packages";
                ConflictingPackagesMessagebox.Text = "To add a package to the list, search it above and right click it";
                ConflictingPackagesRemoveConflictingPackage.IsEnabled = true;
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
                category.Dependencies.Clear();
                foreach (DatabaseLogic logic in PackageDependenciesDisplay.Items)
                    category.Dependencies.Add(logic);

                //there now are unsaved changes
                UnsavedChanges = true;
            }
            else
                Logging.Editor("Category was not modified, no change to set");
            return true;
        }

        private bool DependenciesWereModified(List<DatabaseLogic> dependencies)
        {
            //check if counts are equal. if not, then modifications exist
            if (dependencies.Count() != PackageDependenciesDisplay.Items.Count)
                return true;

            int i = 0;
            //check packagename, notflag, logic
            foreach (DatabaseLogic logic in PackageDependenciesDisplay.Items)
            {
                if (!logic.Equals(dependencies[i]))
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

        private bool UserFilesWereModified(List<UserFile> userFiles)
        {
            if (userFiles.Count != PackageUserdatasDisplay.Items.Count)
                return true;

            int i = 0;
            foreach (UserFile file in PackageUserdatasDisplay.Items)
            {
                if (!file.Equals(userFiles[i]))
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
                if (!media.Equals(Medias[i]))
                    return true;
                i++;
            }

            return false;
        }

        private bool ConflictingPackagesModified(List<string> conflicts)
        {
            if (conflicts.Count != PackageConflictingPackagesDisplay.Items.Count)
                return true;

            int i = 0;
            foreach (string conflict in PackageConflictingPackagesDisplay.Items)
            {
                if (!conflict.Equals(conflicts[i]))
                    return true;
                i++;
            }

            return false;
        }

        private bool CategoryWasModified(Category category)
        {
            if (!category.Name.Equals(PackageNameDisplay.Text))
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
            if (!package.StartAddress.Equals(PackageStartAddressDisplay.Text))
                return true;
            if (!package.EndAddress.Equals(PackageEndAddressDisplay.Text))
                return true;

            //devURL is separated by newlines for array list, so it's not necessary to escape
            if (!package.DevURL.Equals(Utils.MacroReplace(PackageDevURLDisplay.Text, ReplacementTypes.TextEscape)))
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
            if (!package.InternalNotes.Equals(Utils.MacroReplace(PackageInternalNotesDisplay.Text, ReplacementTypes.TextEscape)))
                return true;
            if (!package.ZipFile.Equals(PackageZipFileDisplay.Text))
                return true;

            //dependency
            if (package is Dependency dependency)
            {
                if (DependenciesWereModified(dependency.Dependencies))
                    return true;
            }

            //see if it's a selectablePackage
            else if (package is SelectablePackage selectablePackage)
            {
                if (!selectablePackage.ShowInSearchList.Equals((bool)PackageShowInSearchListDisplay.IsChecked))
                    return true;
                if (!selectablePackage.PopularMod.Equals((bool)PackagePopularModDisplay.IsChecked))
                    return true;
                if (!selectablePackage.GreyAreaMod.Equals((bool)PackageGreyAreaModDisplay.IsChecked))
                    return true;
                if (!selectablePackage.Visible.Equals((bool)PackageVisibleDisplay.IsChecked))
                    return true;
                if (!selectablePackage.Name.Equals(PackageNameDisplay.Text))
                    return true;
                if (!selectablePackage.Type.Equals((SelectionTypes)PackageTypeDisplay.SelectedItem))
                    return true;
                //if (!selectablePackage.Description.Equals(Utils.MacroReplace(PackageDescriptionDisplay.Text,ReplacementTypes.TextEscape)))
                //    return true;
                //if (!selectablePackage.UpdateComment.Equals(Utils.MacroReplace(PackageUpdateNotesDisplay.Text,ReplacementTypes.TextEscape)))
                //    return true;
                if (!selectablePackage.Description.Equals(PackageDescriptionDisplay.Text))
                    return true;
                if (!selectablePackage.UpdateComment.Equals(PackageUpdateNotesDisplay.Text))
                    return true;

                if (DependenciesWereModified(selectablePackage.Dependencies))
                    return true;

                if (UserFilesWereModified(selectablePackage.UserFiles))
                    return true;

                if (MediasModified(selectablePackage.Medias))
                    return true;

                if (ConflictingPackagesModified(selectablePackage.ConflictingPackages))
                    return true;
            }
            return false;
        }

        private bool ApplyDatabasePackage(DatabasePackage package)
        {
            Logging.Editor("ApplyDatabasePackage(), package saving = {0}", LogLevel.Info, package.PackageName);

            //check if the to save packagename is unique
            if (!PackagePackageNameDisplay.Text.Equals(package.PackageName))
            {
                Logging.Editor("packageName is new, checking if it is unique");
                if (Utils.IsDuplicateName(Utils.GetFlatList(GlobalDependencies, Dependencies, null, ParsedCategoryList), PackagePackageNameDisplay.Text))
                {
                    MessageBox.Show(string.Format("Duplicate packageName: {0} is already used", PackagePackageNameDisplay.Text));
                    return false;
                }
            }

            //check if package was actually modified before saving all these delicious properties
            if (!PackageWasModified(package))
            {
                Logging.Editor("package was not modified, don't apply anything");
                return true;
            }

            //only check for missing zip file if there is text be begin with
            if (!string.IsNullOrWhiteSpace(PackageZipFileDisplay.Text))
            {
                if (!package.ZipFile.Equals(PackageZipFileDisplay.Text) && !Path.GetExtension(PackageZipFileDisplay.Text).Equals(".zip"))
                {
                    MessageBox.Show("no zip in file extension, was this a mistake?");
                    return false;
                }
            }

            Logging.Editor("package was modified, saving changes to memory and setting changes switch");

            //save everything from the UI into the package
            //save package elements first
            package.PackageName = PackagePackageNameDisplay.Text;
            package.StartAddress = PackageStartAddressDisplay.Text;
            package.EndAddress = PackageEndAddressDisplay.Text;

            //devURL is separated by newlines for array list, so it's not necessary to escape
            package.DevURL = Utils.MacroReplace(PackageDevURLDisplay.Text, ReplacementTypes.TextEscape);
            package.Version = PackageVersionDisplay.Text;
            package.Author = PackageAuthorDisplay.Text;
            package.InstallGroup = (int)PackageInstallGroupDisplay.SelectedItem;
            package.PatchGroup = (int)PackagePatchGroupDisplay.SelectedItem;
            package.LogAtInstall = (bool)PackageLogAtInstallDisplay.IsChecked;
            package.Enabled = (bool)PackageEnabledDisplay.IsChecked;
            package.InternalNotes = Utils.MacroReplace(PackageInternalNotesDisplay.Text, ReplacementTypes.TextEscape);

            //if the zipfile was updated, then update the last modified date
            if (!package.ZipFile.Equals(PackageZipFileDisplay.Text))
            {
                package.CRC = "f";
                package.ZipFile = PackageZipFileDisplay.Text;
                package.Timestamp = Utils.GetCurrentUniversalFiletimeTimestamp();
                PackageLastUpdatedDisplay.Text = Utils.ConvertFiletimeTimestampToDate(package.Timestamp);
            }

            //see if it's a dependency
            if (package is Dependency dependency)
            {
                dependency.Dependencies.Clear();
                foreach (DatabaseLogic dl in PackageDependenciesDisplay.Items)
                    dependency.Dependencies.Add(dl);
            }

            //see if it's a selectablePackage
            else if (package is SelectablePackage selectablePackage)
            {
                selectablePackage.ShowInSearchList = (bool)PackageShowInSearchListDisplay.IsChecked;
                selectablePackage.PopularMod = (bool)PackagePopularModDisplay.IsChecked;
                selectablePackage.GreyAreaMod = (bool)PackageGreyAreaModDisplay.IsChecked;
                selectablePackage.Visible = (bool)PackageVisibleDisplay.IsChecked;
                selectablePackage.Name = PackageNameDisplay.Text;
                selectablePackage.Type = (SelectionTypes)PackageTypeDisplay.SelectedItem;
                //selectablePackage.Description = Utils.MacroReplace(PackageDescriptionDisplay.Text,ReplacementTypes.TextEscape);
                //selectablePackage.UpdateComment = Utils.MacroReplace(PackageUpdateNotesDisplay.Text,ReplacementTypes.TextEscape);
                selectablePackage.Description = PackageDescriptionDisplay.Text;
                selectablePackage.UpdateComment = PackageUpdateNotesDisplay.Text;

                selectablePackage.Dependencies.Clear();
                foreach (DatabaseLogic dl in PackageDependenciesDisplay.Items)
                    selectablePackage.Dependencies.Add(dl);

                selectablePackage.UserFiles.Clear();
                foreach (UserFile uf in PackageUserdatasDisplay.Items)
                    selectablePackage.UserFiles.Add(uf);

                selectablePackage.Medias.Clear();
                foreach (Media m in PackageMediasDisplay.Items)
                    selectablePackage.Medias.Add(m);

                selectablePackage.ConflictingPackages.Clear();
                foreach (string s in PackageConflictingPackagesDisplay.Items)
                    selectablePackage.ConflictingPackages.Add(s);
            }

            //there now are unsaved changes
            UnsavedChanges = true;
            return true;
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
                Logging.Editor("database packages detected to be the same, aborting dragDrop");
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
                    packageToMove = new SelectablePackage(packageToMove, false);
                }
                else if (packageCurrentlyOver is Dependency)
                {
                    packageToMove = new Dependency(packageToMove, false);
                }
                else
                {
                    packageToMove = new DatabasePackage(packageToMove, false);
                }

                //the packageName needs to stay unique as well
                int i = 0;
                string origName = packageToMove.PackageName;
                while (Utils.GetFlatList(GlobalDependencies, Dependencies, null, ParsedCategoryList).Where(package => package.PackageName.Equals(packageToMove.PackageName)).Count() > 0)
                    packageToMove.PackageName = string.Format("{0}_{1}", origName, i++);
                Logging.Editor("New package name is {0}", LogLevel.Info, packageToMove.PackageName);
            }

            Logging.Editor("for insert process, packageCurrentlyOver type is {0}, packageToMove type is {1}", LogLevel.Info, packageCurrentlyOver.GetType().Name, packageToMove.GetType().Name);
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
                    packageToMove = new DatabasePackage(packageToMove, false);
                GlobalDependencies.Insert(GlobalDependencies.IndexOf(packageCurrentlyOver) + 1, (DatabasePackage)packageToMove);
            }

            //at this point if the destination is a selectale package, then it's refrences need to be updated
            if (packageCurrentlyOver is SelectablePackage selectablePackageCurrentlyOver)
            {
                Logging.Editor("packageCurrentlyOver is selectablePackage, updating refrences");
                //packageToMove needs to be casted to a SelectablePackage to have it's refrences updated
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
            Logging.Editor("updating treeview");
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
            Utils.BuildLevelPerPackage(ParsedCategoryList);
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
                Logging.Editor("packToWorkOn is null (not EditorComboboxItem or Databasepackage",LogLevel.Error);
                MessageBox.Show("packToWorkOn is null (not EditorComboboxItem or Databasepackage), abort");
                return;
            }

            //make sure it actually has a zip file to download
            //first check if it's an editor combobox item (selected from checkbox) or the direct item
            if (string.IsNullOrWhiteSpace(packToWorkOn.ZipFile))
            {
                MessageBox.Show("no zip file to download");
                return;
            }

            //make sure FTP credentials are at least entered
            if (string.IsNullOrWhiteSpace(EditorSettings.BigmodsPassword) || string.IsNullOrWhiteSpace(EditorSettings.BigmodsUsername))
            {
                MessageBox.Show("Missing FTP credentials");
                return;
            }
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
                    //InitialDirectory = Settings.ApplicationStartupPath,
                    Title = "Select destination for zip file",
                    FileName = packToWorkOn.ZipFile
                };
            }
            else
            {
                SaveZipFileDialog.FileName = packToWorkOn.ZipFile;
            }
            if (!(bool)SaveZipFileDialog.ShowDialog())
                return;
            //make and run the uploader instance
            DatabaseEditorDownload name = new DatabaseEditorDownload()
            {
                ZipFilePathDisk = SaveZipFileDialog.FileName,
                ZipFilePathOnline = string.Format("{0}{1}/", PrivateStuff.BigmodsFTPUsersRoot, Settings.WoTModpackOnlineFolderVersion),
                ZipFileName = Path.GetFileName(packToWorkOn.ZipFile),
                Credential = new NetworkCredential(EditorSettings.BigmodsUsername, EditorSettings.BigmodsPassword),
                Upload = false,
                PackageToUpdate = null,
                Countdown = EditorSettings.FTPUploadDownloadWindowTimeout
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

            //make sure FTP credentials are at least entered
            if (string.IsNullOrWhiteSpace(EditorSettings.BigmodsPassword) || string.IsNullOrWhiteSpace(EditorSettings.BigmodsUsername))
            {
                MessageBox.Show("Missing FTP credentials");
                return;
            }

            DatabasePackage packToWorkOn = null;
            if (SelectedItem is EditorComboBoxItem ecbi)
                packToWorkOn = ecbi.Package;
            else if (SelectedItem is DatabasePackage pack)
                packToWorkOn = pack;

            if (packToWorkOn == null)
            {
                Logging.Editor("packToWorkOn is null (not EditorComboboxItem or Databasepackage", LogLevel.Error);
                MessageBox.Show("packToWorkOn is null (not EditorComboboxItem or Databasepackage), abort");
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
                    //don't set InitialDirectory to allow it to remember last folder
                    //InitialDirectory = Settings.ApplicationStartupPath,
                    Multiselect = false,
                    Title = "Select zip file to upload"
                };

            if ((bool)OpenZipFileDialog.ShowDialog() && File.Exists(OpenZipFileDialog.FileName))
            {
                zipFileToUpload = OpenZipFileDialog.FileName;
            }
            else
                return;

            //make and run the uploader instance
            DatabaseEditorDownload name = new DatabaseEditorDownload()
            {
                ZipFilePathDisk = zipFileToUpload,
                ZipFilePathOnline = string.Format("{0}{1}/", PrivateStuff.BigmodsFTPUsersRoot, Settings.WoTModpackOnlineFolderVersion),
                ZipFileName = Path.GetFileName(zipFileToUpload),
                Credential = new NetworkCredential(EditorSettings.BigmodsUsername, EditorSettings.BigmodsPassword),
                Upload = true,
                PackageToUpdate = packToWorkOn,
                Countdown = EditorSettings.FTPUploadDownloadWindowTimeout
            };
            name.OnEditorUploadDownloadClosed += OnEditorUploadFinished;
            name.Show();
        }

        private void OnEditorUploadFinished(object sender, EditorUploadDownloadEventArgs e)
        {
            Logging.Editor("upload finished, applying change");
            UnsavedChanges = true;
            if (e.Package == null)
            {
                Logging.Editor("upload was media, applying media change");
                //uploaded media
                Logging.Editor("Upload of {0} success, adding entry in editor", LogLevel.Info, e.UploadedFilename);
                SelectablePackage selectedPackage = GetSelectablePackage(SelectedItem);
                Media m = new Media()
                {
                    MediaType = MediaType.Picture,
                    URL = string.Format("{0}{1}", e.UploadedFilepathOnline, e.UploadedFilename).Replace("ftp:", "http:")
                };
                selectedPackage.Medias.Add(m);

                PackageMediasDisplay.Items.Clear();
                foreach (Media logic in selectedPackage.Medias)
                    PackageMediasDisplay.Items.Add(logic);
            }
            //else uploaded package zipfile entry
            else
            {
                Logging.Editor("upload was package zipfile, checking if currently displayed");
                DatabasePackage selectedItem = null;
                if (SelectedItem is DatabasePackage dp)
                    selectedItem = dp;
                else if (SelectedItem is EditorComboBoxItem editorComboBoxItem)
                    selectedItem = editorComboBoxItem.Package;

                string tempZipName = e.Package.ZipFile;
                //update the package crc and timestamp values
                e.Package.CRC = "f";
                e.Package.Timestamp = Utils.GetCurrentUniversalFiletimeTimestamp();

                if (selectedItem.Equals(e.Package))
                {
                    Logging.Editor("it's currently displayed, updating entry for display");
                    ApplyDatabaseObject(e.Package);
                    e.Package.ZipFile = tempZipName;
                    ShowDatabasePackage(e.Package);
                }
                else
                {
                    Logging.Editor("it's currently not displayed, updating entry for not display");
                }
            }
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
            switch (EditorSettings.SaveAsDatabaseVersion)
            {
                case DatabaseXmlVersion.Legacy:
                    XmlUtils.SaveDatabase(DefaultSaveLocationSetting.Text, Settings.WoTClientVersion, Settings.WoTModpackOnlineFolderVersion,
                GlobalDependencies, Dependencies, ParsedCategoryList, DatabaseXmlVersion.Legacy);
                    break;
                case DatabaseXmlVersion.OnePointOne:
                    XmlUtils.SaveDatabase(Path.Combine(Path.GetDirectoryName(DefaultSaveLocationSetting.Text), Settings.BetaDatabaseV2RootFilename),
                        Settings.WoTClientVersion, Settings.WoTModpackOnlineFolderVersion, GlobalDependencies, Dependencies, ParsedCategoryList, DatabaseXmlVersion.OnePointOne);
                    break;
            }

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
                    InitialDirectory = string.IsNullOrWhiteSpace(DefaultSaveLocationSetting.Text) ? Settings.ApplicationStartupPath :
                    Directory.Exists(Path.GetDirectoryName(DefaultSaveLocationSetting.Text)) ? DefaultSaveLocationSetting.Text : Settings.ApplicationStartupPath,
                    Title = string.Format("Save Database: version = {0}", EditorSettings.SaveAsDatabaseVersion.ToString())
                };

            //then show it and only continue if a selection was committed
            if (!(bool)SaveDatabaseDialog.ShowDialog())
                return;

            if (string.IsNullOrWhiteSpace(DefaultSaveLocationSetting.Text) ||
                    !Path.GetDirectoryName(SaveDatabaseDialog.FileName).Equals(Path.GetDirectoryName(DefaultSaveLocationSetting.Text)))
                if (MessageBox.Show("Use this as default save location?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    DefaultSaveLocationSetting.Text = SaveDatabaseDialog.FileName;

            //actually save
            switch (EditorSettings.SaveAsDatabaseVersion)
            {
                case DatabaseXmlVersion.Legacy:
                    XmlUtils.SaveDatabase(SaveDatabaseDialog.FileName, Settings.WoTClientVersion, Settings.WoTModpackOnlineFolderVersion,
                GlobalDependencies, Dependencies, ParsedCategoryList, DatabaseXmlVersion.Legacy);
                    break;
                case DatabaseXmlVersion.OnePointOne:
                    XmlUtils.SaveDatabase(Path.Combine(Path.GetDirectoryName(SaveDatabaseDialog.FileName), Settings.BetaDatabaseV2RootFilename),
                        Settings.WoTClientVersion, Settings.WoTModpackOnlineFolderVersion, GlobalDependencies, Dependencies, ParsedCategoryList, DatabaseXmlVersion.OnePointOne);
                    break;
            }

            UnsavedChanges = false;
        }

        private void LoadAsDatabaseButton_Click(object sender, RoutedEventArgs e)
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

            //the file exists, load it
            XmlDocument doc = XmlUtils.LoadXmlDocument(fileToLoad, XmlLoadType.FromFile);
            if (doc == null)
            {
                MessageBox.Show("Failed to load the database, check the logfile");
                Logging.Editor("doc is null from LoadXmlDocument(fileToload, xmlType)");
                return;
            }
            if (!XmlUtils.ParseDatabase(doc, GlobalDependencies, Dependencies, ParsedCategoryList, Path.GetDirectoryName(fileToLoad)))
            {
                MessageBox.Show("Failed to load the database, check the logfile");
                return;
            }

            //build internal database links
            Utils.BuildLinksRefrence(ParsedCategoryList, true);
            Utils.BuildLevelPerPackage(ParsedCategoryList);

            //set the onlineFolder and version
            //for the onlineFolder version: //modInfoAlpha.xml/@onlineFolder
            //for the folder version: //modInfoAlpha.xml/@version
            Settings.WoTClientVersion = XmlUtils.GetXmlStringFromXPath(doc, "//modInfoAlpha.xml/@version");
            Settings.WoTModpackOnlineFolderVersion = XmlUtils.GetXmlStringFromXPath(doc, "//modInfoAlpha.xml/@onlineFolder");
            LoadUI(GlobalDependencies, Dependencies, ParsedCategoryList);
            UnsavedChanges = false;
        }

        private void OnLoadDatabaseClick(object sender, RoutedEventArgs e)
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

            //actually load
            //the file exists, load it
            XmlDocument doc = XmlUtils.LoadXmlDocument(DefaultSaveLocationSetting.Text, XmlLoadType.FromFile);
            if (doc == null)
            {
                MessageBox.Show("Failed to load the database, check the logfile");
                Logging.Editor("doc is null from LoadXmlDocument(fileToload, xmlType)");
                return;
            }
            if (!XmlUtils.ParseDatabase(doc, GlobalDependencies, Dependencies, ParsedCategoryList, Path.GetDirectoryName(DefaultSaveLocationSetting.Text)))
            {
                MessageBox.Show("Failed to load the database, check the logfile");
                return;
            }

            //build internal database links
            Utils.BuildLinksRefrence(ParsedCategoryList, true);
            Utils.BuildLevelPerPackage(ParsedCategoryList);

            //set the onlineFolder and version
            //for the onlineFolder version: //modInfoAlpha.xml/@onlineFolder
            //for the folder version: //modInfoAlpha.xml/@version
            Settings.WoTClientVersion = XmlUtils.GetXmlStringFromXPath(doc, "//modInfoAlpha.xml/@version");
            Settings.WoTModpackOnlineFolderVersion = XmlUtils.GetXmlStringFromXPath(doc, "//modInfoAlpha.xml/@onlineFolder");
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
                    InitialDirectory = string.IsNullOrWhiteSpace(DefaultSaveLocationSetting.Text) ? Settings.ApplicationStartupPath :
                    Directory.Exists(Path.GetDirectoryName(DefaultSaveLocationSetting.Text)) ? DefaultSaveLocationSetting.Text : Settings.ApplicationStartupPath,
                    Title = "Select path to save database to. NOTE: It is only selecting path, does not save"
                };
            if (!(bool)SaveDatabaseDialog.ShowDialog())
                return;
            DefaultSaveLocationSetting.Text = SaveDatabaseDialog.FileName;
        }
        #endregion

        #region Database Add/Move/Remove buttons

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
            EditorAddRemove addRemove = new EditorAddRemove()
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

            //make the window and show it
            EditorAddRemove addRemove = new EditorAddRemove()
            {
                GlobalDependencies = GlobalDependencies,
                Dependencies = Dependencies,
                ParsedCategoryList = ParsedCategoryList,
                EditOrAdd = false,
                AddSameLevel = true,
                SelectedPackage = null
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
        #endregion

        #region Right side package modify buttons

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

            Logging.Editor("adding dependency to component");
            IComponentWithDependencies component = null;
            //convert it out of editorComboBoxItem if it is in one
            if (SelectedItem is EditorComboBoxItem editorComboBoxItem)
            {
                if (editorComboBoxItem.Package is IComponentWithDependencies componentWithDependencies)
                    component = componentWithDependencies;
            }
            else if (SelectedItem is IComponentWithDependencies componentWithDependencies)
            {
                component = componentWithDependencies;
            }
            else
            {
                Logging.Editor("SelectedItem is invalid type: {0}", LogLevel.Info, SelectedItem.GetType().ToString());
                return;
            }

            //check the list of databaselogic in the item, make sure we're not trying to add a duplicate item
            foreach (DatabaseLogic logic in component.DependenciesProp)
            {
                if (logic.PackageName.Equals((LoadedDependenciesList.SelectedItem as Dependency).PackageName))
                {
                    MessageBox.Show("Dependency already exists in package");
                    return;
                }
            }
            component.DependenciesProp.Add(new DatabaseLogic()
            {
                PackageName = (LoadedDependenciesList.SelectedItem as Dependency).PackageName,
                Logic = (Logic)LoadedLogicsList.SelectedItem,
                NotFlag = (bool)DependenciesNotFlag.IsChecked
            }
            );

            //update the UI
            PackageDependenciesDisplay.Items.Clear();
            foreach (DatabaseLogic logic in component.DependenciesProp)
                PackageDependenciesDisplay.Items.Add(logic);

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

        private void DependenciesRemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            //remove the selected item from the UI display
            PackageDependenciesDisplay.Items.Remove(PackageDependenciesDisplay.SelectedItem);

            //remove it from the list of logics/dependencies of the selected item
            if (SelectedItem is IComponentWithDependencies packageLogic)
            {
                packageLogic.DependenciesProp.Remove(PackageDependenciesDisplay.SelectedItem as DatabaseLogic);
            }

            UnsavedChanges = true;
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
            Logging.Editor("adding media");

            SelectablePackage selectedPackage = GetSelectablePackage(SelectedItem);
            foreach (Media media in selectedPackage.Medias)
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
            selectedPackage.Medias.Add(m);

            PackageMediasDisplay.Items.Clear();
            foreach (Media logic in selectedPackage.Medias)
                PackageMediasDisplay.Items.Add(logic);

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
            Logging.Editor("applying media edit from component");

            SelectablePackage selectedPackage = GetSelectablePackage(SelectedItem);
            Media selectedMediaInUI = (PackageMediasDisplay.SelectedItem as Media);
            Media mediainList = selectedPackage.Medias.Find(med => med.MediaType.Equals(selectedMediaInUI.MediaType) && med.URL.Equals(selectedMediaInUI.URL));

            mediainList.MediaType = (MediaType)MediaTypesList.SelectedItem;
            mediainList.URL = MediaTypesURL.Text;

            //update the UI
            PackageMediasDisplay.Items.Clear();
            foreach (Media logic in selectedPackage.Medias)
                PackageMediasDisplay.Items.Add(logic);

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

        private void MediaRemoveMediaButton_Click(object sender, RoutedEventArgs e)
        {
            Logging.Editor("removing media from component");

            SelectablePackage selectedPackage = GetSelectablePackage(SelectedItem);
            Media selectedMediaInUI = (PackageMediasDisplay.SelectedItem as Media);
            Media mediainList = selectedPackage.Medias.Find(med => med.MediaType.Equals(selectedMediaInUI.MediaType) && med.URL.Equals(selectedMediaInUI.URL));

            //remove from internal list
            selectedPackage.Medias.Remove(mediainList);

            //update UI
            PackageMediasDisplay.Items.Clear();
            foreach (Media logic in selectedPackage.Medias)
                PackageMediasDisplay.Items.Add(logic);

            UnsavedChanges = true;
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

            //get the path to upload to
            if (OpenPictureDialog == null)
                OpenPictureDialog = new OpenFileDialog()
                {
                    AddExtension = true,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    //InitialDirectory = Settings.ApplicationStartupPath,
                    Multiselect = true,
                    Title = "Select image file to upload"
                };
            if (!(bool)OpenPictureDialog.ShowDialog())
                return;

            //select path to upload to on server
            EditorSelectMediaUploadLocation selectUploadLocation = new EditorSelectMediaUploadLocation()
            {
                Credential = new NetworkCredential(EditorSettings.BigmodsUsername, EditorSettings.BigmodsPassword)
            };
            if (!(bool)selectUploadLocation.ShowDialog())
                return;

            //start upload
            foreach (string mediaToUploadPath in OpenPictureDialog.FileNames)
            {
                string mediaToUploadFilename = Path.GetFileName(mediaToUploadPath);
                DatabaseEditorDownload name = new DatabaseEditorDownload()
                {
                    ZipFilePathDisk = mediaToUploadPath,
                    ZipFilePathOnline = selectUploadLocation.UploadPath,
                    ZipFileName = mediaToUploadFilename,
                    Credential = new NetworkCredential(EditorSettings.BigmodsUsername, EditorSettings.BigmodsPassword),
                    Upload = true,
                    PackageToUpdate = null,
                    Countdown = EditorSettings.FTPUploadDownloadWindowTimeout
                };
                //this needs to be changed to a show() with event handler made for on exit
                name.OnEditorUploadDownloadClosed += OnEditorUploadFinished;
                name.Show();
            }
        }

        private void MediaPreviewSelectedMediaButton_Click(object sender, RoutedEventArgs e)
        {
            if (PackageMediasDisplay.SelectedIndex == -1)
            {
                MessageBox.Show("Invalid Index");
                return;
            }
            if (PackageMediasDisplay.SelectedItem is Media media)
            {
                SelectablePackage package = new SelectablePackage()
                {
                    PackageName = "TEST_PREVIEW",
                    Name = "TEST_PREVIEW"
                };
                package.Medias.Add(media);
                if (Preview != null)
                {
                    Preview = null;
                }
                Preview = new Preview()
                {
                    Package = package,
                    EditorMode = true
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
                throw new BadMemeException("no");
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
            SelectablePackage package = new SelectablePackage()
            {
                PackageName = "TEST_PREVIEW",
                Name = "TEST_PREVIEW"
            };
            package.Medias.Add(new Media()
            {
                URL = MediaTypesURL.Text,
                MediaType = (MediaType)MediaTypesList.SelectedItem
            });
            if (Preview != null)
            {
                Preview = null;
            }
            Preview = new Preview()
            {
                Package = package,
                EditorMode = true
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

        private void UserdataAddUserdataButton_Click(object sender, RoutedEventArgs e)
        {
            //check if valid input
            if (string.IsNullOrWhiteSpace(UserDataEditBox.Text))
            {
                MessageBox.Show("no user data path specified");
                return;
            }
            //check if already exists
            SelectablePackage selectedPackage = GetSelectablePackage(SelectedItem);
            foreach (UserFile userfile in selectedPackage.UserFiles)
            {
                if (userfile.Pattern.Equals(UserDataEditBox.Text))
                {
                    MessageBox.Show("user data already exists");
                    return;
                }
            }
            Logging.Editor("adding userData {0}", LogLevel.Info, UserDataEditBox.Text);

            selectedPackage.UserFiles.Add(new UserFile { Pattern = UserDataEditBox.Text });

            //update UI
            PackageUserdatasDisplay.Items.Clear();
            foreach (UserFile logic in selectedPackage.UserFiles)
                PackageUserdatasDisplay.Items.Add(logic);

            UnsavedChanges = true;
        }

        private void UserdataApplyEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (PackageUserdatasDisplay.SelectedIndex < 0)
            {
                MessageBox.Show("Invalid selection");
                return;
            }

            SelectablePackage selectedPackage = GetSelectablePackage(SelectedItem);
            foreach (UserFile userfile in selectedPackage.UserFiles)
            {
                if (userfile.Pattern.Equals(UserDataEditBox.Text))
                {
                    MessageBox.Show("user data already exists");
                    return;
                }
            }
            Logging.Editor("editing userData", LogLevel.Info);

            UserFile UserFileInUi = (PackageUserdatasDisplay.SelectedItem as UserFile);
            UserFile file = selectedPackage.UserFiles.Find(med => med.Pattern.Equals(UserFileInUi.Pattern));

            file.Pattern = UserDataEditBox.Text;

            //update UI
            PackageUserdatasDisplay.Items.Clear();
            foreach (UserFile logic in selectedPackage.UserFiles)
                PackageUserdatasDisplay.Items.Add(logic);

            UnsavedChanges = true;
        }

        private void PackageUserdatasDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PackageUserdatasDisplay.SelectedItem == null)
                return;
            UserDataEditBox.Text = (PackageUserdatasDisplay.SelectedItem as UserFile).Pattern;
        }

        private void UserdataRemoveUserdata_Click(object sender, RoutedEventArgs e)
        {
            Logging.Editor("removing userdata from component");

            SelectablePackage selectedPackage = GetSelectablePackage(SelectedItem);
            UserFile UserFileInUi = (PackageUserdatasDisplay.SelectedItem as UserFile);
            UserFile file = selectedPackage.UserFiles.Find(med => med.Pattern.Equals(UserFileInUi.Pattern));
            selectedPackage.UserFiles.Remove(file);

            //update UI
            PackageUserdatasDisplay.Items.Clear();
            foreach (UserFile logic in selectedPackage.UserFiles)
                PackageUserdatasDisplay.Items.Add(logic);

            UnsavedChanges = true;
        }

        private void TriggerAddSelectedTrigger_Click(object sender, RoutedEventArgs e)
        {
            if (LoadedTriggersComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Invalid selection");
                return;
            }

            SelectablePackage selectedPackage = GetSelectablePackage(SelectedItem);
            foreach (string s in selectedPackage.Triggers)
            {
                if (s.Equals(LoadedTriggersComboBox.SelectedItem as string))
                {
                    MessageBox.Show("trigger already exists");
                    return;
                }
            }
            Logging.Editor("adding trigger");

            selectedPackage.Triggers.Add(LoadedTriggersComboBox.SelectedItem as string);

            //update UI
            PackageTriggersDisplay.Items.Clear();
            foreach (string trigger in selectedPackage.Triggers)
                PackageTriggersDisplay.Items.Add(trigger);

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

        private void TriggerRemoveTrigger_Click(object sender, RoutedEventArgs e)
        {
            if (PackageTriggersDisplay.SelectedItem == null)
            {
                MessageBox.Show("Invalid selection");
                return;
            }

            Logging.Editor("removing trigger from component");
            SelectablePackage selectedPackage = GetSelectablePackage(SelectedItem);
            selectedPackage.Triggers.Remove(PackageTriggersDisplay.SelectedItem as string);

            //update UI
            PackageTriggersDisplay.Items.Clear();
            foreach (string trigger in selectedPackage.Triggers)
                PackageTriggersDisplay.Items.Add(trigger);

            UnsavedChanges = true;
        }

        private void ConflictingPackagesRemoveConflictingPackage_Click(object sender, RoutedEventArgs e)
        {
            if (PackageConflictingPackagesDisplay.SelectedItem == null)
            {
                MessageBox.Show("Invalid selection");
                return;
            }

            SelectablePackage selectedPackage = GetSelectablePackage(SelectedItem);
            selectedPackage.ConflictingPackages.Remove(PackageConflictingPackagesDisplay.SelectedItem as string);

            //update UI
            PackageConflictingPackagesDisplay.Items.Clear();
            foreach (string confligt in selectedPackage.ConflictingPackages)
                PackageConflictingPackagesDisplay.Items.Add(confligt);

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

        private void SaveSelectionBeforeLeaveSetting_Click(object sender, RoutedEventArgs e)
        {
            EditorSettings.SaveSelectionBeforeLeave = (bool)SaveSelectionBeforeLeaveSetting.IsChecked;
        }

        private void SortCategoriesSetting_Click(object sender, RoutedEventArgs e)
        {
            EditorSettings.SortDatabaseList = (bool)SortCategoriesSetting.IsChecked;
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

        private void ShowConfirmOnPackageApplySetting_Click(object sender, RoutedEventArgs e)
        {
            EditorSettings.ShowConfirmationOnPackageApply = (bool)ShowConfirmOnPackageApplySetting.IsChecked;
        }

        private void ShowConfirmOnPackageAddRemoveEditSetting_Click(object sender, RoutedEventArgs e)
        {
            EditorSettings.ShowConfirmationOnPackageAddRemoveMove = (bool)ShowConfirmOnPackageAddRemoveEditSetting.IsChecked;
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
                    searchComponents.AddRange(Utils.GetFlatList(GlobalDependencies, Dependencies, null, ParsedCategoryList).Where(term => term.PackageName.ToLower().Contains(searchTerm.ToLower())));
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
                    if (e.RightButton == MouseButtonState.Pressed && ConflictingPackagesTab.IsVisible && SelectedItem != null)
                    {
                        Logging.Editor("Mouse right click with trigger add, checking if already exists");
                        SelectablePackage selectedPackage = GetSelectablePackage(SelectedItem);
                        foreach (string s in selectedPackage.ConflictingPackages)
                        {
                            if (s.Equals(item.Package.PackageName))
                            {
                                Logging.Editor("Mouse right click with conflicting packages add, skipping adding cause already exists: {0}", LogLevel.Info, item.Package.PackageName);
                                MessageBox.Show("conflict packagename already exists");
                                return;
                            }
                        }
                        Logging.Editor("Mouse right click with conflicting packages add, does not exist, adding");

                        selectedPackage.ConflictingPackages.Add(item.Package.PackageName);

                        //update UI
                        PackageConflictingPackagesDisplay.Items.Clear();
                        foreach (string conflict in selectedPackage.ConflictingPackages)
                            PackageConflictingPackagesDisplay.Items.Add(conflict);

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
                SelectDatabaseObject(item.Package, null);
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

        private void SaveDatabaseSetting_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)SaveDatabaseLegacySetting.IsChecked)
                EditorSettings.SaveAsDatabaseVersion = DatabaseXmlVersion.Legacy;
            else if ((bool)SaveDatabaseOnePointOneSetting.IsChecked)
                EditorSettings.SaveAsDatabaseVersion = DatabaseXmlVersion.OnePointOne;
        }

        private void LaunchAutoUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            AutoUpdatePackageWindow autoUpdatePackageWindow = new AutoUpdatePackageWindow()
            {
                Packages = Utils.GetFlatList(GlobalDependencies, Dependencies, null, ParsedCategoryList),
                WorkingDirectory = EditorSettings.AutoUpdaterWorkDirectory,
                Credential = new NetworkCredential(EditorSettings.BigmodsUsername, EditorSettings.BigmodsPassword)
            };
            autoUpdatePackageWindow.ShowDialog();
        }

        private void SelectAutoUpdateWorkDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog openFolderDialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Multiselect = false,
                Title = "Select folder"
            };
            if(openFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SelectAutoUpdateWorkDirectoryTextbox.Text = openFolderDialog.FileName;
            }
        }

        private void SelectAutoUpdateWorkDirectoryTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditorSettings.AutoUpdaterWorkDirectory = SelectAutoUpdateWorkDirectoryTextbox.Text;
            LaunchAutoUpdateButton.IsEnabled = !string.IsNullOrWhiteSpace(EditorSettings.AutoUpdaterWorkDirectory);
        }
    }
}
