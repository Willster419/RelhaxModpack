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
        private System.Windows.Forms.Timer DragDropTimer = new System.Windows.Forms.Timer() {Enabled = false, Interval = 1000 };
        private TreeViewItem ItemToExpand;
        private Point BeforeDragDropPoint;
        private bool IsScrolling = false;
        private bool AlreadyLoggedMouseMove = false;
        private bool AlreadyLoggedScroll = false;
        private string[] UIHeaders = new string[]
        {
            "-----Global Dependencies-----",
            "-----Dependencies-----",
        };

        public DatabaseEditor()
        {
            InitializeComponent();
        }

        private void OnApplicationLoad(object sender, RoutedEventArgs e)
        {
            EditorSettings = new EditorSettings();
            Logging.Info("Loading editor settings");
            if(!Settings.LoadSettings(Settings.EditorSettingsFilename, typeof(EditorSettings), null,EditorSettings))
            {
                Logging.Info("Failed to load editor settings, using defaults");
            }
            else
            {
                Logging.Info("Editor settings loaded success");
            }
            //check if we are loading the document auto from the command line
            if(!string.IsNullOrWhiteSpace(CommandLineSettings.EditorAutoLoadFileName))
            {
                Logging.Info("Attempting to auto-load xml file from {0}", CommandLineSettings.EditorAutoLoadFileName);
                if(File.Exists(CommandLineSettings.EditorAutoLoadFileName))
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
            foreach(Trigger t in InstallerComponents.InstallEngine.Triggers)
            {
                TriggerSelectionComboBox.Items.Add(t.Name);
            }
            //hook up timer
            DragDropTimer.Tick += OnDragDropTimerTick;
        }

        private void OnDragDropTimerTick(object sender, EventArgs e)
        {
            DragDropTimer.Stop();
            if(ItemToExpand.Header is EditorComboBoxItem item)
            {
                if(item.Package is SelectablePackage sp)
                {
                    if(sp.Packages.Count > 0)
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

        private void OnLoadDatabaseClick(object sender, RoutedEventArgs e)
        {
            string fileToLoad = string.Empty;
            //check if it's from the auto load function or not
            if(sender != null)
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

        private void LoadUI(List<DatabasePackage> globalDependencies, List<Dependency> dependnecies, List<Category> parsedCategoryList, int numToAddEnd = 5)
        {
            //clear and reset
            DatabaseTreeView.Items.Clear();
            //RESET UI TODO? or don't do it?
            //create treeviewItems for each entry
            //first make the globalDependencies header
            TreeViewItem globalDependenciesHeader = new TreeViewItem() {Header = UIHeaders[0]};
            //add it to the main view
            DatabaseTreeView.Items.Add(globalDependenciesHeader);
            //loop to add all the global dependencies to a treeview item, which is a new comboboxitem, which is the package and displayname
            foreach(DatabasePackage globalDependency in GlobalDependencies)
            {
                globalDependenciesHeader.Items.Add(new TreeViewItem() { Header = new EditorComboBoxItem(globalDependency, globalDependency.PackageName)});
            }

            //same for dependencies
            TreeViewItem dependenciesHeader = new TreeViewItem() { Header = UIHeaders[1] };
            DatabaseTreeView.Items.Add(dependenciesHeader);
            foreach (DatabasePackage dependency in Dependencies)
            {
                dependenciesHeader.Items.Add(new TreeViewItem() { Header = new EditorComboBoxItem(dependency, dependency.PackageName) });
            }

            //add the category, then add each level recursivly
            foreach (Category cat in parsedCategoryList)
            {
                TreeViewItem CategoryHeader = new TreeViewItem() { Header = cat };
                DatabaseTreeView.Items.Add(CategoryHeader);
                LoadUI(CategoryHeader, cat.Packages);
            }

            //adding the spacing that dirty wants...
            for(int i = 0; i < numToAddEnd; i++)
            {
                DatabaseTreeView.Items.Add(string.Empty);
            }

            //load the install and patch groups
            InstallGroupsTreeView.Items.Clear();
            //make a flat list (can be used in patchGroup as well)
            List<DatabasePackage> allFlatList = Utils.GetFlatList(GlobalDependencies, dependnecies, null, parsedCategoryList);
            //make an array of group headers
            TreeViewItem[] installGroupHeaders = new TreeViewItem[Utils.GetMaxInstallGroupNumber(allFlatList)+1];
            //for each group header, get the list of packages that have an equal install group number
            for (int i = 0; i < installGroupHeaders.Count(); i++)
            {
                installGroupHeaders[i] = new TreeViewItem() { Header = string.Format("---Install Group {0}---", i) };
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

            //do the same for patchgroups
            PatchGroupsTreeView.Items.Clear();
            TreeViewItem[] patchGroupHeaders = new TreeViewItem[Utils.GetMaxPatchGroupNumber(allFlatList)+1];
            //for each group header, get the list of packages that have an equal patch group number
            for (int i = 0; i < patchGroupHeaders.Count(); i++)
            {
                patchGroupHeaders[i] = new TreeViewItem() { Header = string.Format("---Patch Group {0}---", i) };
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
            foreach(SelectablePackage package in packages)
            {
                //make a TVI for it
                TreeViewItem packageTVI = new TreeViewItem() { Header = new EditorComboBoxItem(package, package.PackageName) };
                //and have the parent add it
                parent.Items.Add(packageTVI);
                if (package.Packages.Count > 0)
                    LoadUI(packageTVI, package.Packages);
            }
        }

        #region Drag Drop code
        private void OnTreeViewDrop(object sender, DragEventArgs e)
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
                if (itemCurrentlyOver.Header is EditorComboBoxItem packageCurrentlyOver && itemToMove.Header is EditorComboBoxItem packageToMove)
                {
                    //make sure that the source and destination are not the same
                    if (packageCurrentlyOver.Package.Equals(packageToMove.Package))
                        return;
                    switch(e.Effects)
                    {
                        case DragDropEffects.Copy:

                            break;
                        case DragDropEffects.Move:
                            //remove the treeviewItem from the UI list
                            //add the package to the new area (below)
                            if (itemToMove.Parent is TreeViewItem parentItemToMove)
                            {
                                if(itemCurrentlyOver.Parent is TreeViewItem parentItemOver)
                                {
                                    parentItemToMove.Items.Remove(itemToMove);
                                    parentItemOver.Items.Insert(parentItemOver.Items.IndexOf(itemCurrentlyOver) + 1, itemToMove);
                                }
                            }
                            //remove the package from the internal list
                            //add the treeviewitem to the new area (below)
                            if (packageToMove.Package is SelectablePackage selectablePackageToMove)
                            {
                                if (packageCurrentlyOver.Package is SelectablePackage selectablePackageCurrentlyOver)
                                {
                                    selectablePackageToMove.Parent.Packages.Remove(selectablePackageToMove);
                                    selectablePackageCurrentlyOver.Packages.Insert(selectablePackageCurrentlyOver.Packages.IndexOf(selectablePackageCurrentlyOver) + 1, selectablePackageToMove);
                                }
                                else break;
                            }
                            else break;
                            break;
                    }
                }
            }
        }

        private void OnTreeViewDragOver(object sender, DragEventArgs e)
        {
            if (!(sender is TreeView tv))
                return;
            TreeView treeView = (TreeView)sender;
            string moveOrCopy = string.Empty;
            if(treeView.Equals(DatabaseTreeView))
            {

            }
            else
            {

            }
            if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                e.Effects = DragDropEffects.Copy;
                moveOrCopy = "Copy";
            }
            else
            {
                e.Effects = DragDropEffects.Move;
                moveOrCopy = "Move";
            }
            DragDropTest.Text = "";
            DragDropTest.Visibility = Visibility.Hidden;
            //first check as the UI level, make sure we are looking at treeviewItems
            if (e.Source is TreeViewItem itemCurrentlyOver && treeView.SelectedItem is TreeViewItem itemToMove)
            {
                if (itemCurrentlyOver.Header is EditorComboBoxItem packageCurrentlyOver && itemToMove.Header is EditorComboBoxItem packageToMove)
                {
                    //check if the left or right control keys are pressed or not (copy or move)
                    if (DragDropTest.Visibility == Visibility.Hidden)
                        DragDropTest.Visibility = Visibility.Visible;
                    //make sure it's not same item
                    if (packageCurrentlyOver.Package.Equals(packageToMove.Package))
                    {
                        DragDropTest.Text = "Item can't be itself!";
                        return;
                    }
                    DragDropTest.Text = string.Format("{0} {1} below {2}", moveOrCopy, packageToMove.DisplayName, packageCurrentlyOver.DisplayName);
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
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
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
            Logging.Debug("MouseDown, leftButton={0}, saving mouse location if pressed",e.LeftButton.ToString());
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                BeforeDragDropPoint = e.GetPosition(treeView);
            }
        }

        private void OnTreeViewScroll(object sender, ScrollChangedEventArgs e)
        {
            //https://stackoverflow.com/questions/14583234/disable-drag-and-drop-when-scrolling
            if(!AlreadyLoggedScroll)
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
            }
        }
        #endregion

        private void LogAtInstallCB_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void VisibleCB_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void EnabledCB_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ZipDownload_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ZipUload_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveDatabaseButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveAsDatabaseButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveDatabaseObjectButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MoveDatabaseObjectButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddDatabaseObjectButton_Click(object sender, RoutedEventArgs e)
        {

        }

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
    }
}
