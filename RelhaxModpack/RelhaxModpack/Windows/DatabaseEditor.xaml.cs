using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using System.Xml;
using Microsoft.Win32;
using RelhaxModpack.UIComponents;
using System.Net;
using Path = System.IO.Path;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for DatabaseEditor.xaml
    /// </summary>
    public partial class DatabaseEditor : RelhaxWindow
    {

        private EditorSettings EditorSettings;
        private XmlDocument XmlDatabase;
        private List<DatabasePackage> GlobalDependencies = new List<DatabasePackage>();
        private List<Dependency> Dependencies = new List<Dependency>();
        private List<Category> ParsedCategoryList = new List<Category>();
        private OpenFileDialog OpenDatabaseDialog;
        private SaveFileDialog SaveDatabaseDialog;
        private OpenFileDialog OpenZipFileDialog;
        private SaveFileDialog SaveZipFileDialog;
        private System.Windows.Forms.Timer DragDropTimer = new System.Windows.Forms.Timer() { Enabled = false, Interval = 1000 };
        private TreeViewItem ItemToExpand;
        private Point BeforeDragDropPoint;
        private bool IsScrolling = false;
        private bool AlreadyLoggedMouseMove = false;
        private bool AlreadyLoggedScroll = false;
        private bool Init = true;
        private DatabasePackage SelectedItem = null;
        private Preview Preview;
        private string[] UIHeaders = new string[]
        {
            "-----Global Dependencies-----",
            "-----Dependencies-----",
        };

        #region Stuff
        public DatabaseEditor()
        {
            InitializeComponent();
        }

        private void OnApplicationLoad(object sender, RoutedEventArgs e)
        {
            EditorSettings = new EditorSettings();
            Logging.Info("Loading editor settings");
            if (!Settings.LoadSettings(Settings.EditorSettingsFilename, typeof(EditorSettings), null, EditorSettings))
            {
                Logging.Info("Failed to load editor settings, using defaults");
            }
            else
            {
                Logging.Info("Editor settings loaded success");
            }
            //check if we are loading the document auto from the command line
            if (!string.IsNullOrWhiteSpace(CommandLineSettings.EditorAutoLoadFileName))
            {
                Logging.Info("Attempting to auto-load xml file from {0}", CommandLineSettings.EditorAutoLoadFileName);
                if (File.Exists(CommandLineSettings.EditorAutoLoadFileName))
                {
                    OnLoadDatabaseClick(null, null);
                }
                else
                {
                    Logging.Info("file does not exist");
                }
            }
            //load the trigger box with trigger options
            TriggerSelectionComboBox.Items.Clear();
            foreach (Trigger t in InstallerComponents.InstallEngine.Triggers)
            {
                TriggerSelectionComboBox.Items.Add(t.Name);
            }
            //hook up timer
            DragDropTimer.Tick += OnDragDropTimerTick;
            SearchBox.Items.Clear();
            //set the items for the triggers combobox. this only needs to be done once anyways
            TriggerSelectionComboBox.Items.Clear();
            foreach (string s in InstallerComponents.InstallEngine.CompleteTriggerList)
                TriggerSelectionComboBox.Items.Add(s);
            Init = false;
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

        private void OnApplicationClose(object sender, EventArgs e)
        {
            if (!Logging.IsLogDisposed(Logfiles.Application))
            {
                Logging.WriteToLog("Saving editor settings");
                if (Settings.SaveSettings(Settings.EditorSettingsFilename, typeof(EditorSettings), null, EditorSettings))
                    Logging.WriteToLog("Editor settings saved");
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

        #region Copy Methods

        private DatabasePackage CopyGlobalDependency(DatabasePackage packageToCopy)
        {
            DatabasePackage newPackage = new DatabasePackage()
            {
                PackageName = packageToCopy.PackageName,
                Version = packageToCopy.Version,
                Timestamp = packageToCopy.Timestamp,
                ZipFile = packageToCopy.ZipFile,
                Enabled = packageToCopy.Enabled,
                CRC = packageToCopy.CRC,
                StartAddress = packageToCopy.StartAddress,
                EndAddress = packageToCopy.EndAddress,
                LogAtInstall = packageToCopy.LogAtInstall,
                Triggers = new List<string>(),
                DevURL = packageToCopy.DevURL,
                InstallGroup = packageToCopy.InstallGroup,
                PatchGroup = packageToCopy.PatchGroup,
                _Enabled = packageToCopy._Enabled
            };
            //foreach (string s in packageToCopy.Triggers)
            //newPackage.Triggers.Add(s);
            return newPackage;
        }

        private Dependency CopyDependency(DatabasePackage packageToCopy)
        {
            Dependency dep = new Dependency()
            {
                PackageName = packageToCopy.PackageName,
                Version = packageToCopy.Version,
                Timestamp = packageToCopy.Timestamp,
                ZipFile = packageToCopy.ZipFile,
                Enabled = packageToCopy.Enabled,
                CRC = packageToCopy.CRC,
                StartAddress = packageToCopy.StartAddress,
                EndAddress = packageToCopy.EndAddress,
                LogAtInstall = packageToCopy.LogAtInstall,
                Triggers = new List<string>(),
                DevURL = packageToCopy.DevURL,
                InstallGroup = packageToCopy.InstallGroup,
                PatchGroup = packageToCopy.PatchGroup,
                _Enabled = packageToCopy._Enabled
            };
            dep.DatabasePackageLogic = new List<DatabaseLogic>();
            dep.Dependencies = new List<DatabaseLogic>();
            return dep;
        }

        private SelectablePackage CopySelectablePackage(DatabasePackage packageToCopy)
        {
            SelectablePackage sp = new SelectablePackage()
            {
                PackageName = packageToCopy.PackageName,
                Version = packageToCopy.Version,
                Timestamp = packageToCopy.Timestamp,
                ZipFile = packageToCopy.ZipFile,
                Enabled = packageToCopy.Enabled,
                CRC = packageToCopy.CRC,
                StartAddress = packageToCopy.StartAddress,
                EndAddress = packageToCopy.EndAddress,
                LogAtInstall = packageToCopy.LogAtInstall,
                Triggers = new List<string>(),
                DevURL = packageToCopy.DevURL,
                InstallGroup = packageToCopy.InstallGroup,
                PatchGroup = packageToCopy.PatchGroup,
                _Enabled = packageToCopy._Enabled
            };
            sp.Type = "multi";
            sp.Name = "WRITE_NEW_NAME";
            sp.Visible = true;
            sp.Size = 0;
            sp.UpdateComment = string.Empty;
            sp.Description = string.Empty;
            sp.PopularMod = false;
            sp._Checked = false;
            sp.Level = -2;
            sp.UserFiles = new List<UserFiles>();
            sp.Packages = new List<SelectablePackage>();
            sp.Medias = new List<Media>();
            sp.Dependencies = new List<DatabaseLogic>();
            sp.ConflictingPackages = new List<string>();
            sp.ShowInSearchList = true;
            return sp;
        }
        #endregion

        #region Other UI methods

        private void LoadUI(List<DatabasePackage> globalDependencies, List<Dependency> dependnecies, List<Category> parsedCategoryList, int numToAddEnd = 5)
        {
            LoadDatabaseView(GlobalDependencies, Dependencies, ParsedCategoryList);
            LoadInstallView(GlobalDependencies, Dependencies, ParsedCategoryList);
            LoadPatchView(GlobalDependencies, Dependencies, ParsedCategoryList);
        }

        private void LoadDatabaseView(List<DatabasePackage> globalDependencies, List<Dependency> dependnecies, List<Category> parsedCategoryList, int numToAddEnd = 5)
        {
            //clear and reset
            DatabaseTreeView.Items.Clear();
            //RESET UI TODO? or don't do it?
            //create treeviewItems for each entry
            //first make the globalDependencies header
            TreeViewItem globalDependenciesHeader = new TreeViewItem() { Header = UIHeaders[0] };
            //add it to the main view
            DatabaseTreeView.Items.Add(globalDependenciesHeader);
            //loop to add all the global dependencies to a treeview item, which is a new comboboxitem, which is the package and displayname
            foreach (DatabasePackage globalDependency in GlobalDependencies)
            {
                globalDependency.EditorTreeViewItem = new TreeViewItem() { Header = new EditorComboBoxItem(globalDependency, globalDependency.PackageName) };
                globalDependenciesHeader.Items.Add(globalDependency.EditorTreeViewItem);
            }

            //same for dependencies
            TreeViewItem dependenciesHeader = new TreeViewItem() { Header = UIHeaders[1] };
            DatabaseTreeView.Items.Add(dependenciesHeader);
            foreach (DatabasePackage dependency in Dependencies)
            {
                dependency.EditorTreeViewItem = new TreeViewItem() { Header = new EditorComboBoxItem(dependency, dependency.PackageName) };
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
                    installGroupHeaders[i].Items.Add(new TreeViewItem() { Header = new EditorComboBoxItem(packageWithEqualGroupNumber, packageWithEqualGroupNumber.PackageName) });
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
                    patchGroupHeaders[i].Items.Add(new TreeViewItem() { Header = new EditorComboBoxItem(packageWithEqualGroupNumber, packageWithEqualGroupNumber.PackageName) });
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
                TreeViewItem packageTVI = new TreeViewItem() { Header = new EditorComboBoxItem(package, package.PackageName) };
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
            WotmodsUsernameSetting.Text = EditorSettings.WotmodsUsername;
            WotmodsPasswordSetting.Text = EditorSettings.WotmodsPassword;
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
        }

        private void ShowDatabaseObject(DatabasePackage package)
        {
            if (package == null)
            {
                Logging.Error("SaveApplyDatabaseObject() package parameter is null, this should not happen!!");
                return;
            }
            //load all items in the databasePackage level first
            PackageNameDisplay.Text = string.Empty;
            PackageNameDisplay.IsEnabled = false;
            PackagePackageNameDisplay.Text = package.PackageName;
            PackageStartAddressDisplay.Text = package.StartAddress;
            PackageZipFileDisplay.Text = package.ZipFile;
            PackageEndAddressDisplay.Text = package.EndAddress;
            PackageDevURLDisplay.Text = package.DevURL;
            PackageVersionDisplay.Text = package.Version;
            PackageTypeDisplay.SelectedIndex = -1;
            PackageTypeDisplay.IsEnabled = false;
            PackageInstallGroupDisplay.SelectedIndex = -1;
            foreach(int i in PackageInstallGroupDisplay.Items)
            {
                if (i == package.InstallGroup)
                {
                    PackageInstallGroupDisplay.SelectedItem = i;
                    break;
                }
            }
            PackagePatchGroupDisplay.SelectedIndex = -1;
            foreach(int i in PackagePatchGroupDisplay.Items)
            {
                if(i == package.PatchGroup)
                {
                    PackagePatchGroupDisplay.SelectedItem = i;
                    break;
                }
            }
            PackageLevelDisplay.Text = string.Empty;
            PackageLevelDisplay.IsEnabled = false;
            PackageLastUpdatedDisplay.IsEnabled = false;
            PackageLastUpdatedDisplay.Text = string.Empty;
            PackageLogAtInstallDisplay.IsChecked = package.LogAtInstall;
            PackageVisibleDisplay.IsChecked = false;
            PackageVisibleDisplay.IsEnabled = false;
            PackageEnabledDisplay.IsChecked = package.Enabled;
            DescriptionTab.IsEnabled = false;
            PackageDescriptionDisplay.Text = string.Empty;
            UpdateNotesTab.IsEnabled = false;
            PackageUpdateNotesDisplay.Text = string.Empty;
            PackageDependenciesDisplay.Items.Clear();
            DependenciesTab.IsEnabled = false;
            PackageMediasDisplay.Items.Clear();
            MediasTab.IsEnabled = false;
            PackageUserdatasDisplay.Items.Clear();
            UserDatasTab.IsEnabled = false;
            PackageTriggersDisplay.Items.Clear();
            foreach (string s in package.Triggers)
                PackageTriggersDisplay.Items.Add(s);
            //then handle if dependency
            if(package is Dependency dependency)
            {
                DependenciesTab.IsEnabled = true;
                foreach (DatabaseLogic d in dependency.Dependencies)
                    PackageDependenciesDisplay.Items.Add(d);
            }
            //then handle if selectalbePackage
            else if (package is SelectablePackage selectablePackage)
            {
                PackageNameDisplay.IsEnabled = true;
                PackageNameDisplay.Text = selectablePackage.Name;
                PackageLevelDisplay.IsEnabled = true;
                PackageLevelDisplay.Text = selectablePackage.Level.ToString();
                PackageLastUpdatedDisplay.IsEnabled = true;
                PackageLastUpdatedDisplay.Text = Utils.ConvertFiletimeTimestampToDate(selectablePackage.Timestamp);
                DescriptionTab.IsEnabled = true;
                PackageDescriptionDisplay.Text = selectablePackage.Description;
                UpdateNotesTab.IsEnabled = true;
                PackageUpdateNotesDisplay.Text = selectablePackage.UpdateComment;
                DependenciesTab.IsEnabled = true;
                foreach (DatabaseLogic d in selectablePackage.Dependencies)
                    PackageDependenciesDisplay.Items.Add(d);
                MediasTab.IsEnabled = true;
                foreach (Media media in selectablePackage.Medias)
                    PackageMediasDisplay.Items.Add(media);
                UserDatasTab.IsEnabled = true;
                foreach (UserFiles data in selectablePackage.UserFiles)
                    PackageUserdatasDisplay.Items.Add(data);
            }
            //reload the list of all dependencies to make sure it's always accurate
            LoadedDependenciesList.Items.Clear();
            foreach (Dependency d in Dependencies)
                LoadedDependenciesList.Items.Add(d);
        }

        private void SaveApplyDatabaseObject(DatabasePackage package)
        {
            if (package == null)
            {
                Logging.Error("SaveApplyDatabaseObject() package parameter is null, this should not happen!!");
                return;
            }
            //save everything from the UI into the package
            //save package elements first
            
            //see if it's a dependency

            //see if it's a selectablePackage

            //reload the list of all dependencies to make sure it's always accurate
            LoadedDependenciesList.Items.Clear();
            foreach (Dependency d in Dependencies)
                LoadedDependenciesList.Items.Add(d);
            //if user requests apply to also save to disk, then do that now
            if (EditorSettings.ApplyBehavior == ApplyBehavior.ApplyTriggersSave)
            {
                SaveDatabaseButton_Click(null, null);
            }
        }
        #endregion

        #region Other UI events

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            //check if we should ask a confirm first
            if (EditorSettings.ShowConfirmationOnPackageApply && MessageBox.Show("Confirm to apply changes?", "", MessageBoxButton.YesNo) == MessageBoxResult.OK)
                if (DatabaseTreeView.SelectedItem is TreeViewItem selectedTreeViewItem && selectedTreeViewItem.Header is EditorComboBoxItem editorSelectedItem)
                {
                    SaveApplyDatabaseObject(editorSelectedItem.Package);
                }
        }

        private void DatabaseTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //first always check to make sure it's a package item
            if (DatabaseTreeView.SelectedItem is TreeViewItem selectedTreeViewItem && selectedTreeViewItem.Header is EditorComboBoxItem editorSelectedItem)
            {
                //check if we should save the item before updating what the current entry is
                if (EditorSettings.SaveSelectionBeforeLeave && SelectedItem != null)
                {
                    SaveApplyDatabaseObject(SelectedItem);
                }
                //set the item as the new selectedItem
                SelectedItem = editorSelectedItem.Package;
                //display the new selectedItem
                ShowDatabaseObject(SelectedItem);
            }
        }

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
                    LoadDatabaseView(GlobalDependencies, Dependencies, ParsedCategoryList);
                }
                else if (selectedTab.Equals(InstallGroupsTab))
                {
                    RightTab.IsEnabled = false;
                    SearchBox.IsEnabled = true;
                    RemoveDatabaseObjectButton.IsEnabled = false;
                    MoveDatabaseObjectButton.IsEnabled = false;
                    AddDatabaseObjectButton.IsEnabled = false;
                    LoadInstallView(GlobalDependencies, Dependencies, ParsedCategoryList);
                }
                else if (selectedTab.Equals(PatchGroupsTab))
                {
                    RightTab.IsEnabled = false;
                    SearchBox.IsEnabled = true;
                    RemoveDatabaseObjectButton.IsEnabled = false;
                    MoveDatabaseObjectButton.IsEnabled = false;
                    AddDatabaseObjectButton.IsEnabled = false;
                    LoadPatchView(GlobalDependencies, Dependencies, ParsedCategoryList);
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
            try
            {
                if (!string.IsNullOrWhiteSpace(PackageDevURLDisplay.Text))
                    System.Diagnostics.Process.Start(PackageDevURLDisplay.Text);
            }
            catch { }
        }
        #endregion

        #region Drag Drop code

        private void PerformDatabaseMoveAdd(TreeViewItem itemCurrentlyOver, TreeViewItem itemToMove, TreeViewItem parentItemToMove, TreeViewItem parentItemOver,
            DatabasePackage packageToMove, DatabasePackage packageCurrentlyOver, DragDropEffects effects, bool addBelowItem)
        {
            Logging.Debug("Starting PerformDatabaseMoveAdd function, itemCurrentlyOver={0}, itemToMove={1}, parentItemToMove={2}, parentItemOver={3}, packageToMove={4}," +
                " packageCurrentlyOver={5}, effects={6}, addBelowItem={7}", itemCurrentlyOver.ToString(), itemToMove.ToString(), parentItemToMove.ToString(), parentItemOver.ToString(),
                packageToMove.PackageName, packageCurrentlyOver.PackageName, effects.ToString(), addBelowItem.ToString());

            //make sure that the source and destination are not the same
            if (packageCurrentlyOver.Equals(packageToMove))
            {
                Logging.Debug("database packages detected to be the same, aborting dragDrop");
                return;
            }

            //if it's a move operation, then remove the element from it's original list
            if (effects == DragDropEffects.Move)
            {
                Logging.Debug("Effects is move, removing {0} from parent", packageToMove.PackageName);
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
                Logging.Debug("Effects is copy, making new copy instance of {0}", packageToMove.PackageName);
                if (packageCurrentlyOver is SelectablePackage)
                {
                    packageToMove = CopySelectablePackage(packageToMove);
                }
                else if (packageCurrentlyOver is Dependency)
                {
                    packageToMove = CopyDependency(packageToMove);
                }
                else
                {
                    packageToMove = CopyGlobalDependency(packageToMove);
                }
                //the packageName needs to stay unique as well
                int i = 0;
                string origName = packageToMove.PackageName;
                while (Utils.GetFlatList(GlobalDependencies, Dependencies, null, ParsedCategoryList).Where(package => package.PackageName.Equals(packageToMove.PackageName)).Count() > 0)
                    packageToMove.PackageName = string.Format("{0}_{1}", origName, i++);
                Logging.Debug("New package name is {0}", packageToMove.PackageName);
            }

            Logging.Debug("for insert process, packageCurrentlyOver type is {0}, packageToMove type is {1}", packageCurrentlyOver.GetType().Name, packageToMove.GetType().Name);
            //insert packageToMove into corresponding list that it's over
            if (packageCurrentlyOver is SelectablePackage selectablePackageCurrentlyOverFOrInsert)
            {
                //we need to make a new item if it's subclassing. can't cast into a subclass
                if (!(packageToMove is SelectablePackage))
                    packageToMove = CopySelectablePackage(packageToMove);
                //unless alt is pressed to copy new item inside
                if (addBelowItem)
                    selectablePackageCurrentlyOverFOrInsert.Packages.Add((SelectablePackage)packageToMove);
                else
                    selectablePackageCurrentlyOverFOrInsert.Parent.Packages.Insert(selectablePackageCurrentlyOverFOrInsert.Parent.Packages.IndexOf(selectablePackageCurrentlyOverFOrInsert) + 1, (SelectablePackage)packageToMove);
            }
            else if (packageCurrentlyOver is Dependency dependnecyCurrentlyOverForInsert)
            {
                if (!(packageToMove is Dependency))
                    packageToMove = CopyDependency(packageToMove);
                Dependencies.Insert(Dependencies.IndexOf(dependnecyCurrentlyOverForInsert) + 1, (Dependency)packageToMove);
            }
            else
            {
                if ((packageToMove is Dependency) || (packageToMove is SelectablePackage))
                    packageToMove = CopyGlobalDependency(packageToMove);
                GlobalDependencies.Insert(GlobalDependencies.IndexOf(packageCurrentlyOver) + 1, (DatabasePackage)packageToMove);
            }

            //at this point if the destination is a selectale package, then it's refrences need to be updated
            if (packageCurrentlyOver is SelectablePackage selectablePackageCurrentlyOver)
            {
                Logging.Debug("packageCurrentlyOver is selectablePackage, updating refrences");
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
            Logging.Debug("updating treeview");
            //same as before
            TreeViewItem realItemToMove = itemToMove;
            //if move, remove
            if (effects == DragDropEffects.Move)
                parentItemToMove.Items.Remove(realItemToMove);

            //if copy, copy
            if (effects == DragDropEffects.Copy)
            {
                realItemToMove = new TreeViewItem() { Header = new EditorComboBoxItem(packageToMove, packageToMove.PackageName) };
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
            //rebulid the levels as well
            Utils.BuildLevelPerPackage(ParsedCategoryList);
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

        private void OnTreeViewMouseMove(object sender, MouseEventArgs e)
        {
            if (!(sender is TreeView tv))
                return;
            TreeView treeView = (TreeView)sender;
            //make sure the mouse is pressed and the drag movement is confirmed
            bool isDragConfirmed = IsDragConfirmed(e.GetPosition(treeView));
            if (e.LeftButton == MouseButtonState.Pressed && isDragConfirmed && !IsScrolling)
            {
                Logging.Debug("MouseMove DragDrop movement accepted, leftButton={0}, isDragConfirmed={1}, IsScrolling={2}", e.LeftButton.ToString(), isDragConfirmed.ToString(), IsScrolling.ToString());
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
            else if (!AlreadyLoggedMouseMove)
            {
                AlreadyLoggedMouseMove = true;
                Logging.Debug("MouseMove DragDrop movement not accepted, leftButton={0}, isDragConfirmed={1}, IsScrolling={2}", e.LeftButton.ToString(), isDragConfirmed.ToString(), IsScrolling.ToString());
            }
        }

        private void OnTreeViewMouseDownPreview(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is TreeView tv))
                return;
            TreeView treeView = (TreeView)sender;
            Logging.Debug("MouseDown, leftButton={0}, saving mouse location if pressed", e.LeftButton.ToString());
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
                Logging.Debug("ScrollChanged event fire, LeftButton={0}, setting IsScrolling to true if pressed", Mouse.LeftButton.ToString());
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
            Logging.Debug("MouseUp, leftButton={0}, setting IsScrolling to false", e.LeftButton.ToString());
            if (e.LeftButton == MouseButtonState.Released)
            {
                IsScrolling = false;
                AlreadyLoggedMouseMove = false;
                AlreadyLoggedScroll = false;
                if (DragDropTest.Visibility == Visibility.Visible)
                    DragDropTest.Visibility = Visibility.Hidden;
            }
        }
        #endregion

        #region Zip File Upload/Download buttons

        private void ZipDownload_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ZipUload_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Database Save/Load buttons

        private void SaveDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            //if save triggers apply, then do it
            if (EditorSettings.ApplyBehavior == ApplyBehavior.SaveTriggersApply && SelectedItem != null)
            {
                SaveApplyDatabaseObject(SelectedItem);
            }
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
            //actually save
            throw new BadMemeException("TODO");
        }

        private void SaveAsDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            //if save triggers apply, then do it
            if (EditorSettings.ApplyBehavior == ApplyBehavior.SaveTriggersApply && SelectedItem != null)
            {
                SaveApplyDatabaseObject(SelectedItem);
            }
            if (SaveDatabaseDialog == null)
                SaveDatabaseDialog = new SaveFileDialog()
                {
                    AddExtension = true,
                    CheckPathExists = true,
                    DefaultExt = "xml",
                    InitialDirectory = string.IsNullOrWhiteSpace(DefaultSaveLocationSetting.Text) ? Settings.ApplicationStartupPath :
                    Directory.Exists(Path.GetDirectoryName(DefaultSaveLocationSetting.Text)) ? DefaultSaveLocationSetting.Text : Settings.ApplicationStartupPath,
                    Title = "Save Database"
                };
            if (!(bool)SaveDatabaseDialog.ShowDialog())
                return;
            //if what the user just specified is not the same as the current default, then ask to update it
            if (!Path.GetDirectoryName(SaveDatabaseDialog.FileName).Equals(Path.GetDirectoryName(DefaultSaveLocationSetting.Text)))
                if (MessageBox.Show("Use this as default save location?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    DefaultSaveLocationSetting.Text = SaveDatabaseDialog.FileName;
            //actually save
            throw new BadMemeException("TODO");
        }

        private void SelectDefaultSaveLocationButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveDatabaseDialog == null)
                SaveDatabaseDialog = new SaveFileDialog()
                {
                    AddExtension = true,
                    CheckPathExists = true,
                    DefaultExt = "xml",
                    InitialDirectory = string.IsNullOrWhiteSpace(DefaultSaveLocationSetting.Text) ? Settings.ApplicationStartupPath :
                    Directory.Exists(Path.GetDirectoryName(DefaultSaveLocationSetting.Text)) ? DefaultSaveLocationSetting.Text : Settings.ApplicationStartupPath,
                    Title = "Save Database"
                };
            if (!(bool)SaveDatabaseDialog.ShowDialog())
                return;
            DefaultSaveLocationSetting.Text = SaveDatabaseDialog.FileName;
        }

        private void OnLoadDatabaseClick(object sender, RoutedEventArgs e)
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
                        InitialDirectory = Settings.ApplicationStartupPath,
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
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(fileToLoad);
            }
            catch (XmlException ex)
            {
                Logging.Exception(ex.ToString());
                MessageBox.Show(ex.ToString());
                return;
            }
            if (!XMLUtils.ParseDatabase(doc, GlobalDependencies, Dependencies, ParsedCategoryList))
            {
                MessageBox.Show("Failed to load the database, check the logfile");
                return;
            }
            Utils.BuildLinksRefrence(ParsedCategoryList, true);
            Utils.BuildLevelPerPackage(ParsedCategoryList);
            LoadUI(GlobalDependencies, Dependencies, ParsedCategoryList);
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
                AddSaveLevel = true,
                SelectedPackage = null
            };
            if (!(bool)addRemove.ShowDialog())
                return;
            if (addRemove.SelectedPackage == null)
                throw new BadMemeException("i hate you all");
            //put the drag drop to a method to access it here TODO
            //selectedItem is itemToMove, currentlyOver is what you just pointed to
            if (DatabaseTreeView.SelectedItem is TreeViewItem itemToMove && itemToMove.Header is EditorComboBoxItem editorItemToMove
                && itemToMove.Parent is TreeViewItem parentItemToMove && addRemove.SelectedPackage.EditorTreeViewItem.Parent is TreeViewItem parentItemCurrentlyOver)
            {
                PerformDatabaseMoveAdd(addRemove.SelectedPackage.EditorTreeViewItem, itemToMove, parentItemToMove, parentItemCurrentlyOver, editorItemToMove.Package,
                    addRemove.SelectedPackage, DragDropEffects.Move, !addRemove.AddSaveLevel);
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
            EditorAddRemove addRemove = new EditorAddRemove()
            {
                GlobalDependencies = GlobalDependencies,
                Dependencies = Dependencies,
                ParsedCategoryList = ParsedCategoryList,
                EditOrAdd = false,
                AddSaveLevel = true,
                SelectedPackage = null
            };
            if (!(bool)addRemove.ShowDialog())
                return;
            if (addRemove.SelectedPackage == null)
                throw new BadMemeException("i hate you all");
            //put the drag drop to a method to access it here TODO
            //selectedItem is itemToMove, currentlyOver is what you just pointed to
            if (DatabaseTreeView.SelectedItem is TreeViewItem itemToMove && itemToMove.Header is EditorComboBoxItem editorItemToMove
                && itemToMove.Parent is TreeViewItem parentItemToMove && addRemove.SelectedPackage.EditorTreeViewItem.Parent is TreeViewItem parentItemCurrentlyOver)
            {
                PerformDatabaseMoveAdd(addRemove.SelectedPackage.EditorTreeViewItem, itemToMove, parentItemToMove, parentItemCurrentlyOver, editorItemToMove.Package,
                    addRemove.SelectedPackage, DragDropEffects.Copy, !addRemove.AddSaveLevel);
            }
        }
        #endregion

        #region Right side package modify buttons

        private void DependenciesAddSelected_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DependenciesRemoveSelected_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MediaAddMediaButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MediaApplyEditButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MediaRemoveMediaButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MediaPreviewSelectedMediaButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MediaPreviewEditMediaButton_Click(object sender, RoutedEventArgs e)
        {
            //input filtering
            if(string.IsNullOrWhiteSpace(MediaTypesURL.Text))
            {
                MessageBox.Show("Media URL must exist");
                return;
            }
            if(MediaTypesList.SelectedIndex == -1)
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

        private void UserdataApplyUsedataButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UserdataApplyEditButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UserdataRemoveUserdata_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TriggerAddSelectedTrigger_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TriggerRemoveTrigger_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Settings tab events

        private void WotmodsUsernameSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditorSettings.WotmodsUsername = WotmodsUsernameSetting.Text;
        }

        private void WotmodsPasswordSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditorSettings.WotmodsPassword = WotmodsPasswordSetting.Text;
        }

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

        private void SaveSelectionBeforeLeaveSetting_Checked(object sender, RoutedEventArgs e)
        {
            EditorSettings.SaveSelectionBeforeLeave = (bool)SaveSelectionBeforeLeaveSetting.IsChecked;
        }

        private void SortCategoriesSetting_Checked(object sender, RoutedEventArgs e)
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

        private void ShowConfirmOnPackageApplySetting_Checked(object sender, RoutedEventArgs e)
        {
            EditorSettings.ShowConfirmationOnPackageApply = (bool)ShowConfirmOnPackageApplySetting.IsChecked;
        }

        private void ShowConfirmOnPackageAddRemoveEditSetting_Checked(object sender, RoutedEventArgs e)
        {
            EditorSettings.ShowConfirmationOnPackageAddRemoveMove = (bool)ShowConfirmOnPackageAddRemoveEditSetting.IsChecked;
        }
        #endregion

        #region Searchbox code
        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            SearchBox.IsDropDownOpen = true;
        }

        private void SearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up)
            {
                //stop the selection from key events!!!
                //https://www.codeproject.com/questions/183259/how-to-prevent-selecteditem-change-on-up-and-down (second answer)
                e.Handled = true;
                SearchBox.IsDropDownOpen = true;
            }
            else if (e.Key == Key.Enter)
            {
                OnSearchBoxCommitted(SearchBox.SelectedItem as EditorSearchBoxItem, false);
            }
            else if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Items.Clear();
                SearchBox.IsDropDownOpen = false;
                SearchBox.SelectedIndex = -1;
            }
            else
            {
                //split the search into an array based on using '*' search
                List<DatabasePackage> searchComponents = new List<DatabasePackage>();
                foreach (string searchTerm in SearchBox.Text.Split('*'))
                {
                    //get a list of components that match the search term
                    searchComponents.AddRange(Utils.GetFlatList(GlobalDependencies, Dependencies, null, ParsedCategoryList).Where(term => term.PackageName.ToLower().Contains(searchTerm.ToLower())));
                }
                //assuming it maintains the order it previously had i.e. removing only when need to...
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
                SearchBox.IsDropDownOpen = true;
            }
        }

        private void SearchBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SearchBox.IsDropDownOpen)
            {
                foreach (EditorSearchBoxItem item in SearchBox.Items)
                {
                    if (item.IsHighlighted && item.IsMouseOver)
                    {
                        OnSearchBoxCommitted(item, true);
                    }
                }
            }
        }

        private void OnSearchBoxCommitted(EditorSearchBoxItem item, bool fromMouse)
        {
            item.Package.EditorTreeViewItem.Focusable = true;
            item.Package.EditorTreeViewItem.Focus();
            Dispatcher.InvokeAsync(() => item.Package.EditorTreeViewItem.BringIntoView(), System.Windows.Threading.DispatcherPriority.Background);
        }
        #endregion

        #region Upload/Download zipfile code

        #endregion
    }
}
