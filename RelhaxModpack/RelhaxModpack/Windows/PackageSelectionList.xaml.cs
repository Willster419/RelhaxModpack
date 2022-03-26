using Microsoft.Win32;
using RelhaxModpack.Common;
using RelhaxModpack.Database;
using RelhaxModpack.Settings;
using RelhaxModpack.UI;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.ClassEventArgs;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace RelhaxModpack.Windows
{
    #region Stuff
    /// <summary>
    /// The delegate to invoke when calling back to the sender for the SelectionClosed event
    /// </summary>
    /// <param name="sender">The sender (this)</param>
    /// <param name="e">The event arguments to send to the installer (MainWindow)</param>
    public delegate void SelectionListClosedDelegate(object sender, SelectionListEventArgs e);
    #endregion

    /// <summary>
    /// Interaction logic for PackageSelectionList.xaml
    /// </summary>
    public partial class PackageSelectionList : RelhaxWindow
    {
        /// <summary>
        /// The list of categories
        /// </summary>
        public List<Category> ParsedCategoryList { get { return databaseManager.ParsedCategoryList; } }

        /// <summary>
        /// The list of global dependencies
        /// </summary>
        public List<DatabasePackage> GlobalDependencies { get { return databaseManager.GlobalDependencies; } }

        /// <summary>
        /// The list of dependencies
        /// </summary>
        public List<Dependency> Dependencies { get { return databaseManager.Dependencies; } }

        /// <summary>
        /// The event that a caller can subscribe to wait for when the selection window actually closes, with arguments for the installation
        /// </summary>
        public event SelectionListClosedDelegate OnSelectionListReturn;

        /// <summary>
        /// Flag to determine if the current installation is started from auto install mode
        /// </summary>
        public bool AutoInstallMode { get; set; } = false;

        /// <summary>
        /// The WoT client version.
        /// </summary>
        public string WotClientVersion { get; set; }

        /// <summary>
        /// The Database version that the user is currently selecting from.
        /// </summary>
        public string DatabaseVersion { get; set; }

        /// <summary>
        /// The WoT client path that the user is selecting packages to install to.
        /// </summary>
        public string WoTDirectory { get; set; }

        /// <summary>
        /// Get the database manager object to use for loading the database.
        /// </summary>
        /// <remarks>If it is null upon loading the window, it will be created automatically.</remarks>
        public DatabaseManager DatabaseManager { get { return databaseManager; } }

        /// <summary>
        /// Flag to indicate if the window is loading application specific UI
        /// </summary>
        public bool LoadingUI { get; private set; } = false;

        private bool continueInstallation  = false;
        private ProgressIndicator loadingProgress = null;
        private Category UserCategory = null;
        private Preview previewWindow = null;
        private const int FLASH_TICK_INTERVAL = 250;
        private const int NUM_FLASH_TICKS = 5;
        private int numTicks = 0;
        private DispatcherTimer FlashTimer = null;
        private DatabaseManager databaseManager;
        private bool processingConflict = false;

        #region Boring stuff
        /// <summary>
        /// Create an instance of the PackageSelectionList window
        /// </summary>
        /// <param name="modpackSettings">The modpack settings object</param>
        /// <param name="commandLineSettings">The command line settings object.</param>
        public PackageSelectionList(ModpackSettings modpackSettings, CommandLineSettings commandLineSettings) : this(modpackSettings, commandLineSettings, null)
        {
            
        }

        /// <summary>
        /// Create an instance of the PackageSelectionList window
        /// </summary>
        /// <param name="modpackSettings">The modpack settings object</param>
        /// <param name="commandLineSettings">The command line settings object.</param>
        /// <param name="manager">The database manager object to use for loading the database</param>
        public PackageSelectionList(ModpackSettings modpackSettings, CommandLineSettings commandLineSettings, DatabaseManager manager) : base(modpackSettings)
        {
            InitializeComponent();
            databaseManager = manager;
            if (this.CommandLineSettings == null)
                this.CommandLineSettings = commandLineSettings;
            WindowState = WindowState.Minimized;
        }

        private void OnWindowLoadReportProgress(object sender, RelhaxProgress progress)
        {
            if (loadingProgress != null)
            {
                loadingProgress.Message = progress.ReportMessage;
                loadingProgress.ProgressValue = progress.ChildCurrent;
                loadingProgress.ProgressMaximum = progress.ChildTotal;
            }
        }

        private void OnContinueInstallation(object sender, RoutedEventArgs e)
        {
            //check if we should save last config and if so, then do so
            if(ModpackSettings.SaveLastSelection)
            {
                Logging.Debug("Saving selection from continue button, when saveLastSelection is true (save format V3)");
                SaveSelectionV3(ApplicationConstants.LastInstalledConfigFilepath, true);
            }
            continueInstallation = true;
            this.Close();
        }

        private void OnCancelInstallation(object sender, RoutedEventArgs e)
        {
            continueInstallation = false;
            this.Close();
        }

        private void RelhaxWindow_Closed(object sender, EventArgs e)
        {
            //close and dispose preview
            if (previewWindow != null)
            {
                previewWindow.Close();
                previewWindow = null;
            }

            //stop timer if on
            if (FlashTimer != null)
            {
                FlashTimer.IsEnabled = false;
                FlashTimer.Stop();
                if (FlashTimer.Tag != null)
                    FlashTimer.Tag = null;
                FlashTimer.Tick -= OnFlashTimerTick;
                FlashTimer = null;
            }

            //save width and height settings
            if (WindowState == WindowState.Maximized)
                ModpackSettings.ModSelectionFullscreen = true;
            else if (WindowState == WindowState.Normal)
                ModpackSettings.ModSelectionFullscreen = false;
            ModpackSettings.ModSelectionHeight = (int)Height;
            ModpackSettings.ModSelectionWidth = (int)Width;

            OnSelectionListReturn?.Invoke(this, new SelectionListEventArgs()
            {
                ContinueInstallation = continueInstallation,
                DatabaseManager = databaseManager,
                UserMods = UserCategory?.Packages,
                IsAutoInstall = AutoInstallMode,
                WoTModpackOnlineFolderFromDB = databaseManager.WoTOnlineFolderVersion
            });
        }

        private void OnFlashTimerTick(object sender, EventArgs e)
        {
            if(!(FlashTimer.Tag is SelectablePackage))
            {
                Logging.Error("FlashTimer.Tag is not of SelectablePackage type");
                return;
            }
            SelectablePackage packageToChange = (SelectablePackage)FlashTimer.Tag;

            if(!(packageToChange.UIComponent is IPackageUIComponent))
            {
                Logging.Error("packageToChange.UiComponent is not of Control type");
                return;
            }
            IPackageUIComponent control = packageToChange.UIComponent;

            switch (numTicks++)
            {
                case 0:
                    control.IsHighlightedForView = true;
                    break;
                case NUM_FLASH_TICKS:
                    //stop the timer and reset everything
                    control.IsHighlightedForView = false;
                    FlashTimer.Stop();
                    numTicks = 0;
                    break;
                default:
                    //toggle the color
                    control.IsHighlightedForView = !control.IsHighlightedForView;
                    break;
            }
        }

        private void TreeViewItem_Collapsed(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            //trigger the collapsed such that itself is expanded but other elements are collapsed
            TreeViewItem rootItem = sender as TreeViewItem;
            RelhaxWPFCheckBox wpfCheckBox = rootItem.Header as RelhaxWPFCheckBox;
            SelectablePackage rootPackage = wpfCheckBox.Package as SelectablePackage;
            //iterate to collapse each other item, then expand itself
            foreach (SelectablePackage package in rootPackage.Packages)
            {
                if (package.TreeViewItem != null)
                    if (package.TreeViewItem.IsExpanded)
                        package.TreeViewItem.IsExpanded = false;
            }
            rootPackage.TreeViewItem.IsExpanded = true;
        }

        private void OnUserModsTabSelected(object sender, RequestBringIntoViewEventArgs e)
        {
            if (ModpackSettings.DisplayUserModsWarning)
            {
                MessageBox.Show(Translations.GetTranslatedString("FirstTimeUserModsWarning"));
                ModpackSettings.DisplayUserModsWarning = false;
            }
        }
        #endregion

        #region UI INIT STUFF
        private void OnWindowLoad(object sender, RoutedEventArgs e)
        {
            //set the flag for currently loading the UI. It prevents search box or UI interaction code from happening as a failsafe
            LoadingUI = true;

            //init the timer (~3ms)
            //UI THREAD REQUIRED
            FlashTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(FLASH_TICK_INTERVAL), DispatcherPriority.Background, OnFlashTimerTick, this.Dispatcher) {IsEnabled = false };

            //create the loading window (~40ms)
            //UI THREAD REQUIRED
            loadingProgress = new ProgressIndicator(this.ModpackSettings)
            {
                ProgressMaximum = 8,
                ProgressMinimum = 0,
                Message = Translations.GetTranslatedString("loading")
            };

            //show the list and hide this window (~5ms)
            //UI THREAD REQUIRED
            loadingProgress.Show();
            Hide();

            //create progress reporter object. it doesn't report direct progress, but receives
            //reports from inside wherever the reporter is used. When the event
            //is fired, it's on the UI thread (not the thread that reported) (~3ms)
            //UI THREAD REQUIRED
            //https://blogs.msdn.microsoft.com/dotnet/2012/06/06/async-in-4-5-enabling-progress-and-cancellation-in-async-apis/
            Progress<RelhaxProgress> progressIndicator = new Progress<RelhaxProgress>();
            progressIndicator.ProgressChanged += OnWindowLoadReportProgress;

            //run non-UI thread required parts on separate thread (~50ms from top to here)
            Task.Run(() => LoadModSelectionList(progressIndicator));
        }

        private async void LoadModSelectionList(IProgress<RelhaxProgress> progressIndicator)
        {
            //create the progress object used in the reporter
            //NO UI THREAD REQUIRED
            RelhaxProgress loadProgress = new RelhaxProgress()
            {
                ChildTotal = 3,
                ChildCurrent = 1,
                ReportMessage = Translations.GetTranslatedString("readingDatabase")
            };

            //load the database
            if (databaseManager == null)
                databaseManager = new DatabaseManager(ModpackSettings, CommandLineSettings) { ManagerInfoZipfile = ((App)Application.Current).ManagerInfoZipfile };
            await databaseManager.LoadDatabaseAsync();

            //check local download cache (files, milliseconds to seconds)
            //NO UI THREAD REQUIRED
            //UI PROGRESS REPORTING
            List<DatabasePackage> flatList = databaseManager.GetFlatList();
            ModSelectionCheckMd5Hashes(progressIndicator, loadProgress, flatList);

            //sort the database for UI display (~3ms)
            //NO UI THREAD REQUIRED
            databaseManager.SortDatabase();

            //create new user mods category and add zip files in the user packages folder as SelectablePackage objects (~5ms)
            //NO UI THREAD REQUIRED
            InitUsermods();

            //link everything again now that the category exists (~10ms)
            //MUST HAPPEN AFTER InitDatabaseUI()
            //NO UI THREAD REQUIRED
            databaseManager.ProcessDatabase();

            //kick off running the UI part of the loading
            //UI THREAD REQUIRED
            loadProgress.ChildCurrent = 0;
            loadProgress.ReportMessage = Translations.GetTranslatedString("loadingUI");
            progressIndicator.Report(loadProgress);
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() => LoadModSelectionListUiComponents(loadProgress)));
        }

        private void ModSelectionCheckMd5Hashes(IProgress<RelhaxProgress> progress, RelhaxProgress loadProgress, List<DatabasePackage> flatList)
        {
            //check db cache of local files in download zip folder
            loadProgress.ChildCurrent++;
            loadProgress.ReportMessage = Translations.GetTranslatedString("verifyingDownloadCache");
            progress.Report(loadProgress);

            //check if the md5 hash database file exists, if not then make it
            Md5DatabaseManager md5DatabaseManager = new Md5DatabaseManager();
            md5DatabaseManager.LoadMd5Database(ApplicationConstants.MD5HashDatabaseXmlFile);

            //make a sublist of only packages where a zipfile exists (in the database)
            List<DatabasePackage> flatListZips = flatList.FindAll(package => !string.IsNullOrWhiteSpace(package.ZipFile));
            foreach (DatabasePackage package in flatListZips)
            {
                //make path for the zipfile
                string zipFile = Path.Combine(ApplicationConstants.RelhaxDownloadsFolderPath, package.ZipFile);

                //only look for a crc if the cache file exists
                if (!File.Exists(zipFile))
                {
                    //set the download flag since it doesn't exist
                    package.DownloadFlag = true;

                    //delete the entry if it exists
                    if (md5DatabaseManager.FileEntryWithoutTimeExists(package.ZipFile))
                        md5DatabaseManager.DeleteFileEntry(package.ZipFile);
                    continue;
                }
                else if (package.CRC.Equals("f"))
                {
                    //this package is under current testing, don't update the hash for it, but mark it as needing download
                    package.DownloadFlag = true;
                    continue;
                }

                //since file exists, report progress here
                loadProgress.ReportMessage = string.Format("{0} {1}",
                    Translations.GetTranslatedString("verifyingDownloadCache"), package.PackageName);
                progress.Report(loadProgress);

                //check if the entry in the database is up to date (filetime) with the filetime of the currently downloaded file
                //if it's not, then get the hash and update the hash and filetime
                if (!md5DatabaseManager.FileEntryUpToDate(package.ZipFile, File.GetLastWriteTime(zipFile)))
                {
                    string hash = FileUtils.CreateMD5Hash(zipFile);
                    if (hash.Equals("-1"))
                        throw new BadMemeException("A '-1' means the file doesn't exist. But it does. Or at least it should at this point.");
                    md5DatabaseManager.UpdateFileEntry(package.ZipFile, File.GetLastWriteTime(zipFile), hash);
                }

                //the file entry is up to date in the database, but it may be out of date with the modpack database
                string hashInDb = md5DatabaseManager.GetMd5HashFileEntry(package.ZipFile, File.GetLastWriteTime(zipFile));
                if (!package.CRC.Equals(hashInDb))
                {
                    Logging.Warning("Zip file {0} reports up to date by filetime, but local hash {1} does not match online hash {2}. Flag for re-download", package.ZipFile, hashInDb, package.CRC);
                    package.DownloadFlag = true;
                }
            }

            //and save the file
            md5DatabaseManager.SaveMd5Database(ApplicationConstants.MD5HashDatabaseXmlFile);
        }

        private void InitUsermods()
        {
            //init database components
            UserCategory = new Category()
            {
                OffsetInstallGroups = false
            };

            //get a list of all zip files in the folder
            string[] zipFilesUserMods = FileUtils.FileSearch(ApplicationConstants.RelhaxUserModsFolderPath, SearchOption.TopDirectoryOnly, false, false, @"*.zip", 5, 3, true);

            foreach (string s in zipFilesUserMods)
            {
                SelectablePackage sp = new SelectablePackage
                {
                    ZipFile = s,
                    Name = Path.GetFileNameWithoutExtension(s),
                    PackageName = Path.GetFileNameWithoutExtension(s),
                    Enabled = true,
                    Level = 0,
                    PatchGroup = 9,
                    InstallGroup = 9,
                    DownloadFlag = false,
                    ParentCategory = UserCategory
                };
                //circular reference because
                sp.Parent = sp.TopParent = sp;
                UserCategory.Packages.Add(sp);
            }
        }
        
        private void LoadModSelectionListUiComponents(RelhaxProgress loadProgress)
        {
            //initialize the categories lists and tab items (~50ms)
            //UI THREAD REQUIRED
            InitDatabaseUI(ParsedCategoryList);

            //run the loop for each category to create package UI objects (~8 sec)
            //UI THREAD REQUIRED
            //UI PROGRESS REPORTING
            UiUtils.AllowUIToUpdate();
            ModSelectionLoadUiList(loadProgress);

            //create user category UI objects
            //UI THREAD REQUIRED
            loadProgress.ReportMessage = Translations.GetTranslatedString("loadingUI");
            OnWindowLoadReportProgress(null, loadProgress);
            UiUtils.AllowUIToUpdate();
            AddUserMods();

            //get the status of selection list loading, what to do after load (autoInstall, select last installed, etc.) (?ms)
            //UI THREAD REQUIRED (checks UI components)
            SelectionListEventArgs args = GetSelectionStatus();

            //update text boxes and other text properties (~2ms)
            //UI THREAD REQUIRED
            InstallingTo.Text = string.Format(Translations.GetTranslatedString(InstallingTo.Name), WoTDirectory);
            InstallingAsWoTVersion.Text = string.Format(Translations.GetTranslatedString(InstallingAsWoTVersion.Name), WotClientVersion);
            SearchCB.Text = Translations.GetTranslatedString("searchComboBoxInitMessage");
            string databaseSubversionInfo = string.Empty;
            switch (ModpackSettings.DatabaseDistroVersion)
            {
                case DatabaseVersions.Test:
                    databaseSubversionInfo = "TEST";
                    break;
                case DatabaseVersions.Beta:
                    databaseSubversionInfo = ModpackSettings.BetaDatabaseSelectedBranch;
                    break;
                case DatabaseVersions.Stable:
                    databaseSubversionInfo = DatabaseVersion;
                    break;
            }
            UsingDatabaseVersion.Text = string.Format(Translations.GetTranslatedString(UsingDatabaseVersion.Name), ModpackSettings.DatabaseDistroVersion.ToString(), databaseSubversionInfo);

            //determined if the collapse and expand buttons should be visible (?ms)
            //UI THREAD REQUIRED
            switch (ModpackSettings.ModSelectionView)
            {
                case SelectionView.DefaultV2:
                    CollapseAllRealButton.IsEnabled = false;
                    CollapseAllRealButton.Visibility = Visibility.Hidden;
                    ExpandAllRealButton.IsEnabled = false;
                    ExpandAllRealButton.Visibility = Visibility.Hidden;
                    break;
                case SelectionView.Legacy:
                    CollapseAllRealButton.IsEnabled = true;
                    CollapseAllRealButton.Visibility = Visibility.Visible;
                    ExpandAllRealButton.IsEnabled = true;
                    ExpandAllButton.Visibility = Visibility.Visible;
                    break;
            }

            //set the selection window width, height (?ms)
            //UI THREAD REQUIRED
            Width = ModpackSettings.ModSelectionWidth;
            Height = ModpackSettings.ModSelectionHeight;

            //set the loading flag back to false
            LoadingUI = false;

            //close the loading window (?ms)
            //UI THREAD REQUIRED
            loadingProgress.Close();
            loadingProgress = null;

            //if auto install or one-click install, don't show the UI (~140ms from ModSelectionLoadUiList() to here)
            //UI THREAD REQUIRED
            if (AutoInstallMode || ModpackSettings.OneClickInstall || !string.IsNullOrEmpty(CommandLineSettings.AutoInstallFileName))
            {
                OnSelectionListReturn?.Invoke(this, args);
            }
            else
            {
                //show the UI for selection list, and if should be full-screen or not
                Show();
                WindowState = ModpackSettings.ModSelectionFullscreen ? WindowState.Maximized : WindowState.Normal;
            }
        }

        private void InitDatabaseUI(List<Category> parsedCategoryList)
        {
            //one time init of stuff goes here (init the tabGroup would have been nice if needed here)
            //just in case
            if (ModTabGroups.Items.Count > 0)
                ModTabGroups.Items.Clear();

            foreach (Category cat in parsedCategoryList)
            {
                //build per category tab here
                //like all the UI stuff and linking internally
                //make the tab page
                cat.TabPage = new SelectionListTabItem()
                {
                    Header = new TextBlock() { Text = cat.Name, Style = this.Resources["selectionTextBlockStyle"] as Style },
                    Tag = cat,
                    Package = cat.CategoryHeader
                };

                //init common stuff used between both
                cat.CategoryHeader.RelhaxWPFComboBoxList = new RelhaxWPFComboBox[2];
                cat.CategoryHeader.ChildStackPanel = new StackPanel();
                cat.CategoryHeader.TabIndex = cat.TabPage;
                cat.CategoryHeader.ChangeColorOnValueChecked =
                        (ModpackSettings.ModSelectionView == SelectionView.DefaultV2 && ModpackSettings.EnableColorChangeDefaultV2View) ||
                        (ModpackSettings.ModSelectionView == SelectionView.Legacy && ModpackSettings.EnableColorChangeLegacyView);
                cat.CategoryHeader.ModSelectionView = ModpackSettings.ModSelectionView;
                cat.CategoryHeader.ForceEnabled = ModpackSettings.ForceEnabled;
                cat.CategoryHeader.ForceVisible = ModpackSettings.ForceVisible;

                switch (ModpackSettings.ModSelectionView)
                {
                    case SelectionView.Legacy:
                        cat.CategoryHeader.TreeViewItem = new StretchingTreeViewItem()
                        {
                            Name = $"_{cat.CategoryHeader.PackageName}_TreeViewItem".Replace(".", "_").Replace("-", "_").Replace("&", "_"),
                            Background = Brushes.Transparent,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            HorizontalContentAlignment = HorizontalAlignment.Stretch,
                            IsExpanded = true
                        };
                        cat.CategoryHeader.TreeView = new StretchingTreeView()
                        {
                            Name = $"_{cat.CategoryHeader.PackageName}_TreeView".Replace(".", "_").Replace("-", "_").Replace("&", "_"),
                            Background = Brushes.Transparent,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            HorizontalContentAlignment = HorizontalAlignment.Stretch
                        };
                        cat.CategoryHeader.TreeView.MouseDown += Lsl_MouseDown;
                        cat.CategoryHeader.ChildBorder = new RelhaxBorder()
                        {
                            Name = $"_{cat.CategoryHeader.PackageName}_ChildRelhaxBorder".Replace(".", "_").Replace("-", "_").Replace("&", "_"),
                            BorderThickness = ModpackSettings.EnableBordersLegacyView ? new Thickness(1) : new Thickness(0),
                            Child = cat.CategoryHeader.ChildStackPanel,
                            Margin = new Thickness(-25, 0, 0, 0)
                        };
                        cat.CategoryHeader.ParentBorder = new RelhaxBorder()
                        {
                            Name = $"_{cat.CategoryHeader.PackageName}_ParentRelhaxBorder".Replace(".", "_").Replace("-", "_").Replace("&", "_"),
                            Child = cat.CategoryHeader.TreeView,
                            Padding = new Thickness(2)
                        };
                        if (cat.CategoryHeader.TreeView.Items.Count > 0)
                            cat.CategoryHeader.TreeView.Items.Clear();
                        cat.CategoryHeader.TreeViewItem.Items.Add(cat.CategoryHeader.ChildBorder);
                        cat.CategoryHeader.TreeViewItem.IsExpanded = true;
                        //for root element, hook into expandable element
                        cat.CategoryHeader.TreeViewItem.Collapsed += TreeViewItem_Collapsed;
                        cat.CategoryHeader.TreeViewItem.Expanded += (sender, e) => { e.Handled = true; };
                        RelhaxWPFCheckBox box = new RelhaxWPFCheckBox()
                        {
                            Name = $"_{cat.CategoryHeader.PackageName}_RelhaxWpfCheckBox".Replace(".", "_").Replace("-", "_").Replace("&", "_"),
                            Package = cat.CategoryHeader,
                            Content = cat.CategoryHeader.NameFormatted,
                            HorizontalAlignment = HorizontalAlignment.Left
                        };
                        box.Click += OnWPFComponentCheck;
                        cat.CategoryHeader.ParentUIComponent = cat.CategoryHeader.TopParentUIComponent = cat.CategoryHeader.UIComponent = box;
                        cat.CategoryHeader.TreeViewItem.Header = cat.CategoryHeader.UIComponent;
                        cat.CategoryHeader.TreeView.Items.Add(cat.CategoryHeader.TreeViewItem);
                        //TabPage -> ParentBorder -> TreeView -> TreeViewItem (CategoryHeader) -> UIComponent
                        cat.TabPage.Content = cat.CategoryHeader.ParentBorder;
                        cat.CategoryHeader.Packages = cat.Packages;
                        break;
                    case SelectionView.DefaultV2:
                        cat.CategoryHeader.ContentControl = new ContentControl();
                        cat.CategoryHeader.ParentStackPanel = new StackPanel();
                        cat.CategoryHeader.ParentBorder = new RelhaxBorder()
                        {
                            Child = cat.CategoryHeader.ParentStackPanel,
                            Padding = new Thickness(2)
                        };
                        cat.CategoryHeader.ScrollViewer = new ScrollViewer()
                        {
                            Content = cat.CategoryHeader.ParentBorder
                        };
                        //TabPage -> ScrollViewer -> ParentBorder -> ParentStackPanel
                        cat.TabPage.Content = cat.CategoryHeader.ScrollViewer;
                        //create checkbox for inside selecteionlist
                        RelhaxWPFCheckBox cb2 = new RelhaxWPFCheckBox()
                        {
                            Package = cat.CategoryHeader,
                            Content = cat.CategoryHeader.NameFormatted,
                            HorizontalAlignment = HorizontalAlignment.Left
                        };
                        cb2.Click += OnWPFComponentCheck;
                        //set it's parent and top parent to itself
                        cat.CategoryHeader.UIComponent = cat.CategoryHeader.ParentUIComponent = cat.CategoryHeader.TopParentUIComponent = cb2;
                        //create and link the child borderand stackpanel
                        cat.CategoryHeader.ChildBorder = new RelhaxBorder()
                        {
                            BorderThickness = ModpackSettings.EnableBordersDefaultV2View ? new Thickness(1) : new Thickness(0),
                            Child = cat.CategoryHeader.ChildStackPanel,
                            Padding = new Thickness(15, 0, 0, 0)
                        };
                        //add the category header item to the stack panel
                        cat.CategoryHeader.ParentStackPanel.Children.Add((Control)cat.CategoryHeader.UIComponent);
                        //add the child border to the parent stack panel
                        cat.CategoryHeader.ParentStackPanel.Children.Add(cat.CategoryHeader.ChildBorder);
                        cat.CategoryHeader.Packages = cat.Packages;
                        break;
                }
                ModTabGroups.Items.Add(cat.TabPage);

                //init some required UI components for all selectablePackages inside it
                foreach (SelectablePackage package in cat.GetFlatPackageList())
                {
                    package.RelhaxWPFComboBoxList = new RelhaxWPFComboBox[2];
                    switch (ModpackSettings.ModSelectionView)
                    {
                        case SelectionView.DefaultV2:
                            package.ContentControl = new ContentControl();
                            break;
                        case SelectionView.Legacy:
                            package.TreeViewItem = new StretchingTreeViewItem()
                            {
                                Name = $"_{package.PackageName}_TreeViewItem".Replace(".", "_").Replace("-", "_").Replace("&", "_"),
                                Background = System.Windows.Media.Brushes.Transparent,
                                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch
                            };
                            break;
                    }
                }
            }
        }

        private void ModSelectionLoadUiList(RelhaxProgress loadProgress)
        {
            //for each category, report category progress then schedule to load it
            loadProgress.ChildTotal = ParsedCategoryList.Count;

            foreach (Category cat in ParsedCategoryList)
            {
                //report the progress
                loadProgress.ChildCurrent++;
                loadProgress.ReportMessage = string.Format("{0} {1}", Translations.GetTranslatedString("loading"), cat.Name);
                OnWindowLoadReportProgress(null, loadProgress);
                UiUtils.AllowUIToUpdate();

                //then schedule the UI work
                AddPackage(cat.Packages);
            }
        }

        private void AddUserMods()
        {
            //NOTE: this is UI thread
            UserCategory.CategoryHeader = new SelectablePackage()
            {
                Name = string.Format("----------[{0}]----------", "USER"),
                Type = SelectionTypes.multi,
                Visible = true,
                Enabled = true,
                Level = -1,
                PackageName = string.Format("Category_User_Header", Name.Replace(' ', '_'))
            };

            UserCategory.CategoryHeader.ChangeColorOnValueChecked =
                        (ModpackSettings.ModSelectionView == SelectionView.DefaultV2 && ModpackSettings.EnableColorChangeDefaultV2View) ||
                        (ModpackSettings.ModSelectionView == SelectionView.Legacy && ModpackSettings.EnableColorChangeLegacyView);
            UserCategory.CategoryHeader.Parent = UserCategory.CategoryHeader;
            UserCategory.CategoryHeader.TopParent = UserCategory.CategoryHeader;
            UserCategory.CategoryHeader.ParentCategory = UserCategory;

            StackPanel userStackPanel = new StackPanel();
            SelectionListTabItem userTab = new SelectionListTabItem()
            {
                Name = "UserMods",
                Header = new TextBlock() { Text = Translations.GetTranslatedString("userMods"), Style = this.Resources["selectionTextBlockStyle"] as Style }
            };

            RelhaxBorder border = new RelhaxBorder()
            {
                BorderThickness = ModpackSettings.EnableBordersDefaultV2View ? new Thickness(1) : new Thickness(0),
                Child = userStackPanel
            };

            ScrollViewer viewer = new ScrollViewer()
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = border
            };

            userTab.Package = UserCategory.CategoryHeader;
            userTab.Package.ParentBorder = border;
            userTab.Package.ScrollViewer = viewer;
            userTab.RequestBringIntoView += OnUserModsTabSelected;
            userTab.Content = viewer;
            ModTabGroups.Items.Add(userTab);
            UserCategory.TabPage = userTab;
            UserCategory.CategoryHeader.TabIndex = userTab;

            foreach(SelectablePackage package in UserCategory.Packages)
            {
                RelhaxWPFCheckBox userMod = new RelhaxWPFCheckBox()
                {
                    Package = package,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    IsChecked = false,
                    IsEnabled = true,
                    Content = package.NameDisplay
                };
                package.Parent = UserCategory.CategoryHeader;
                package.TopParent = UserCategory.CategoryHeader;
                package.ParentBorder = border;
                package.ScrollViewer = viewer;
                package.Visible = true;
                package.ChangeColorOnValueChecked =
                        (ModpackSettings.ModSelectionView == SelectionView.DefaultV2 && ModpackSettings.EnableColorChangeDefaultV2View) ||
                        (ModpackSettings.ModSelectionView == SelectionView.Legacy && ModpackSettings.EnableColorChangeLegacyView);
                package.ModSelectionView = ModpackSettings.ModSelectionView;
                package.UIComponent = userMod;
                package.TabIndex = userTab;
                userMod.Click += OnUserPackageClick;
                userStackPanel.Children.Add(userMod);
            }
            UserCategory.CategoryHeader.Packages = UserCategory.Packages;
        }

        private SelectionListEventArgs GetSelectionStatus()
        {
            //process loading selections after loading UI
            XmlDocument SelectionsDocument = null;
            bool shouldLoadSomething = false;
            string shouldLoadSomethingFilepath = null;
            bool loadSuccess = false;
            bool isAutoInstall = AutoInstallMode || !string.IsNullOrEmpty(CommandLineSettings.AutoInstallFileName);
            bool selectionFileOutOfDate = false;

            //if test mode, don't load the "default_checked" document
            if (databaseManager.DatabaseDistroToLoad == DatabaseVersions.Test)
            {
                Logging.Debug("Test mode is active, don't load default_checked selection");
            }
            else if (AutoInstallMode || ModpackSettings.OneClickInstall)
            {
                //check that the file exists before trying to load it
                if (File.Exists(ModpackSettings.AutoOneclickSelectionFilePath))
                {
                    //load the custom selection file
                    Logging.Info("Loading selection file from {0}", ModpackSettings.AutoOneclickSelectionFilePath);
                    SelectionsDocument = XmlUtils.LoadXmlDocument(ModpackSettings.AutoOneclickSelectionFilePath, XmlLoadType.FromFile);
                    shouldLoadSomethingFilepath = ModpackSettings.AutoOneclickSelectionFilePath;
                    shouldLoadSomething = true;
                }
                else
                {
                    Logging.Warning("AutoInstall or OneClickInstall is true, but the file selection path does not exist:");
                    Logging.Warning(ModpackSettings.AutoOneclickSelectionFilePath);
                    MessageBox.Show(Translations.GetTranslatedString("configLoadFailed"));
                }
            }
            //else check and load the use selection from auto launch command line
            else if (!string.IsNullOrEmpty(CommandLineSettings.AutoInstallFileName))
            {
                string thePath = Path.Combine(ApplicationConstants.RelhaxUserSelectionsFolderPath, CommandLineSettings.AutoInstallFileName);
                Logging.Info("Loading selection file from {0}", thePath);
                SelectionsDocument = XmlUtils.LoadXmlDocument(thePath, XmlLoadType.FromFile);
                shouldLoadSomethingFilepath = thePath;
                shouldLoadSomething = true;
            }
            else if (ModpackSettings.SaveLastSelection)
            {
                if (!File.Exists(ApplicationConstants.LastInstalledConfigFilepath))
                {
                    Logging.Warning("LastInstalledConfigFile does not exist, loading as first time with check default mods");
                    SelectionsDocument = XmlUtils.LoadXmlDocument(FileUtils.GetStringFromZip(((App)Application.Current).ManagerInfoZipfile, ApplicationConstants.DefaultCheckedSelectionfile), XmlLoadType.FromString);
                    shouldLoadSomethingFilepath = null;
                    shouldLoadSomething = true;
                }
                else
                {
                    Logging.Info("Loading selection file from {0}", ApplicationConstants.LastInstalledConfigFilepath);
                    SelectionsDocument = XmlUtils.LoadXmlDocument(ApplicationConstants.LastInstalledConfigFilepath, XmlLoadType.FromFile);
                    shouldLoadSomethingFilepath = ApplicationConstants.LastInstalledConfigFilepath;
                    shouldLoadSomething = true;
                }
            }
            else
            {
                //load default checked mods
                SelectionsDocument = XmlUtils.LoadXmlDocument(FileUtils.GetStringFromZip(((App)Application.Current).ManagerInfoZipfile, ApplicationConstants.DefaultCheckedSelectionfile), XmlLoadType.FromString);
                shouldLoadSomethingFilepath = null;
                shouldLoadSomething = true;
            }

            //check if errors and if should load something
            if (shouldLoadSomething)
            {
                if (SelectionsDocument != null)
                {
                    loadSuccess = LoadSelection(SelectionsDocument, true, shouldLoadSomethingFilepath, out selectionFileOutOfDate);
                }
                else
                {
                    Logging.Error("Failed to load SelectionsDocument, AutoInstall={0}, OneClickInstall={1}, DatabaseDistro={2}, SaveSelection={3}",
                    AutoInstallMode, ModpackSettings.OneClickInstall, databaseManager.DatabaseDistroToLoad, ModpackSettings.SaveLastSelection);
                    Logging.Error("Failed to load SelectionsDocument, AutoSelectionFilePath={0}", ModpackSettings.AutoOneclickSelectionFilePath);
                }
            }

            //create the selection args object
            SelectionListEventArgs args = new SelectionListEventArgs()
            {
                ContinueInstallation = loadSuccess,
                DatabaseManager = databaseManager,
                UserMods = UserCategory.Packages,
                IsAutoInstall = isAutoInstall,
                IsSelectionOutOfDate = selectionFileOutOfDate,
                WoTModpackOnlineFolderFromDB = databaseManager.WoTOnlineFolderVersion
            };
            return args;
        }

        private void AddPackage(List<SelectablePackage> packages)
        {
            foreach(SelectablePackage package in packages)
            {
                //link the parent panels and border to childs
                package.ParentBorder = package.Parent.ChildBorder;
                package.ParentStackPanel = package.Parent.ChildStackPanel;

                //set UI properties, if we're going to run color change based on settings from the user
                package.ChangeColorOnValueChecked =
                    (ModpackSettings.ModSelectionView == SelectionView.DefaultV2 && ModpackSettings.EnableColorChangeDefaultV2View) ||
                    (ModpackSettings.ModSelectionView == SelectionView.Legacy && ModpackSettings.EnableColorChangeLegacyView);
                package.ModSelectionView = ModpackSettings.ModSelectionView;
                package.ForceEnabled = ModpackSettings.ForceEnabled;
                package.ForceVisible = ModpackSettings.ForceVisible;

                //check if we actually want to add it. if the program isn't forcing them to be enabled
                //and the mod reports being disabled, then don't add it to the UI
                //the counter needs to still be kept up to date with the list (the whole list includes invisible mods!)
                if (!ModpackSettings.ForceVisible && !package.Visible)
                    continue;

                //ok now actually load the UI stuff
                //parse command line stuff. if we're forcinfg it to be enabled or visable
                if (ModpackSettings.ForceVisible && !package.IsStructureVisible)
                    package.Visible = true;
                if (ModpackSettings.ForceEnabled && !package.IsStructureEnabled)
                    package.Enabled = true;

                //special code for the borders and stackpanels for child UI component display
                //if the child container for sub options hsa yet to be made AND there are sub options, make it
                if (package.ChildBorder == null && package.Packages.Count > 0)
                {
                    package.ChildStackPanel = new StackPanel() { Name = $"_{package.PackageName}_StackPanel".Replace(".", "_").Replace("-", "_").Replace("&", "_") };
                    package.ChildBorder = new RelhaxBorder()
                    {
                        Name = $"_{package.PackageName}_ChildRelhaxBorder".Replace(".", "_").Replace("-", "_").Replace("&", "_"),
                        BorderThickness = ModpackSettings.EnableBordersDefaultV2View ? new Thickness(1) : new Thickness(0),
                        Child = package.ChildStackPanel
                    };
                    //custom settings for each border
                    switch(ModpackSettings.ModSelectionView)
                    {
                        case SelectionView.DefaultV2:
                            package.ChildBorder.Padding = new Thickness(15, 0, 0, 0);
                            break;
                        case SelectionView.Legacy:
                            package.ChildBorder.Margin = new Thickness(-25, 0, 0, 0);
                            package.TreeViewItem.Items.Add(package.ChildBorder);
                            break;
                    }
                }

                //create a UI component for this package
                switch(package.Type)
                {
                    case SelectionTypes.single1:
                        package.UIComponent = new RelhaxWPFRadioButton()
                        {
                            Name = $"_{package.PackageName}_RelhaxWpfRadioButton".Replace(".", "_").Replace("-", "_").Replace("&", "_"),
                            ToolTip = package.ToolTipString,
                            Package = package,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            HorizontalContentAlignment = HorizontalAlignment.Left,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            //don't set a style so that it will use the default, which means it won't override applying the dark theme
                            //we don't want to do that because we want to use the checkBox's/radioButton's Foreground property
                            Content = new TextBlock() { Text = package.NameDisplay, Style = null },
                            IsEnabled = package.IsStructureEnabled,
                            IsChecked = false
                        };
                        ToolTipService.SetShowOnDisabled(package.UIComponent as RelhaxWPFRadioButton, true);
                        break;
                    case SelectionTypes.single_dropdown1:
                        DoComboboxStuff(package, 0);
                        break;
                    case SelectionTypes.single_dropdown2:
                        DoComboboxStuff(package, 1);
                        break;
                    case SelectionTypes.multi:
                        package.UIComponent = new RelhaxWPFCheckBox()
                        {
                            Name = $"_{package.PackageName}_RelhaxWpfCheckBox".Replace(".", "_").Replace("-", "_").Replace("&", "_"),
                            ToolTip = package.ToolTipString,
                            Package = package,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            HorizontalContentAlignment = HorizontalAlignment.Left,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            //don't set a style so that it will use the default, which means it won't override applying the dark theme
                            //we don't want to do that because we want to use the checkBox's/radioButton's Foreground property
                            Content = new TextBlock() { Text = package.NameDisplay, Style = null },
                            IsEnabled = package.IsStructureEnabled,
                            IsChecked = false                          
                        };
                        ToolTipService.SetShowOnDisabled(package.UIComponent as RelhaxWPFCheckBox, true);
                        break;
                }

                //attach events for user interaction
                if(package.UIComponent != null)
                {
                    //user selecting packages (Click) and previewing (MouseDown)
                    if (package.UIComponent is RadioButton rb)
                    {
                        rb.MouseDown += Generic_MouseDown;
                        rb.Click += OnWPFComponentCheck;
                    }
                    else if (package.UIComponent is CheckBox cb)
                    {
                        cb.MouseDown += Generic_MouseDown;
                        cb.Click += OnWPFComponentCheck;
                    }

                    //user right clicking packages for preview
                    switch (ModpackSettings.ModSelectionView)
                    {
                        case SelectionView.DefaultV2:
                            //Link the content control stuff (it allows for mousedown)
                            package.ContentControl.Content = package.UIComponent;
                            package.ContentControl.MouseRightButtonUp += Lsl_MouseDown;
                            //and add this uiComopnet to the stackpanel
                            package.Parent.ChildStackPanel.Children.Add(package.ContentControl);
                            break;
                        case SelectionView.Legacy:
                            //attach the UI component to the tree view
                            package.TreeViewItem.Header = package.UIComponent;
                            //disable the recursive handling of expansions
                            package.TreeViewItem.Expanded += (sender, e) => { e.Handled = true; };
                            package.TreeViewItem.Collapsed += (sender, e) => { e.Handled = true; };
                            //expand the tree view item
                            package.TreeViewItem.IsExpanded = !ModpackSettings.ShowOptionsCollapsedLegacy;
                            //and add the treeviewitem to the stackpanel
                            package.Parent.ChildStackPanel.Children.Add(package.TreeViewItem);
                            break;
                    }
                }

                //process child packages
                if (package.Packages.Count > 0)
                {
                    if(ModpackSettings.ModSelectionView == SelectionView.DefaultV2)
                    {
                        //if there are child packages, they will be in the child border
                        //so add the child border to the parent (where this package is) stackpanel
                        package.ParentStackPanel.Children.Add(package.ChildBorder);
                    }
                    AddPackage(package.Packages);
                }
            }
        }

        private void DoComboboxStuff(SelectablePackage package, int boxIndex)
        {
            if (package.Parent.RelhaxWPFComboBoxList[boxIndex] == null)
            {
                package.Parent.RelhaxWPFComboBoxList[boxIndex] = new RelhaxWPFComboBox()
                {
                    IsEditable = false,
                    IsEnabled = false,
                    ToolTip = package.ToolTipString,
                    MinWidth = 100,
                    MaxWidth = 420,//yes, really
                    HorizontalAlignment = HorizontalAlignment.Left,
                    AddedToList = false
                };
                ToolTipService.SetShowOnDisabled(package.Parent.RelhaxWPFComboBoxList[boxIndex], true);
                package.Parent.RelhaxWPFComboBoxList[boxIndex].SelectionCommitted += OnSingleDDPackageClick;
            }
            RelhaxComboBoxItem cbi = new RelhaxComboBoxItem(package, package.NameDisplay)
            {
                IsEnabled = package.IsStructureEnabled,
                Content = package.NameDisplay
            };
            package.Parent.RelhaxWPFComboBoxList[boxIndex].Items.Add(cbi);
            if (!package.Parent.RelhaxWPFComboBoxList[boxIndex].AddedToList)
            {
                //add it
                package.Parent.RelhaxWPFComboBoxList[boxIndex].AddedToList = true;
                package.Parent.RelhaxWPFComboBoxList[boxIndex].PreviewMouseRightButtonDown += Generic_MouseDown;
                if (package.Parent.RelhaxWPFComboBoxList[boxIndex].Items.Count > 0)
                {
                    package.Parent.RelhaxWPFComboBoxList[boxIndex].IsEnabled = true;
                    if (package.Parent.RelhaxWPFComboBoxList[boxIndex].SelectedIndex == -1)
                        package.Parent.RelhaxWPFComboBoxList[boxIndex].SelectedIndex = 0;
                }
                if (ModpackSettings.ModSelectionView == SelectionView.DefaultV2)
                {
                    package.Parent.ChildStackPanel.Children.Add(package.Parent.RelhaxWPFComboBoxList[boxIndex]);
                }
                else if (ModpackSettings.ModSelectionView == SelectionView.Legacy)
                {
                    package.TreeViewItem.Header = package.Parent.RelhaxWPFComboBoxList[boxIndex];
                    package.Parent.ChildStackPanel.Children.Add(package.TreeViewItem);
                }
            }
        }
        #endregion

        #region UI Interaction With Database
        /// <summary>
        /// Clears all selections in the given lists by setting the checked properties to false
        /// </summary>
        private void ClearSelections(List<SelectablePackage> packagesToClear)
        {
            foreach (SelectablePackage package in packagesToClear)
            {
                if (ModpackSettings.SaveDisabledMods && package.FlagForSelectionSave)
                {
                    Logging.Debug("SaveDisabledMods=True and package {0} FlagForSelectionSave is high, setting to low", package.Name);
                    package.FlagForSelectionSave = false;
                }
                package.Checked = false;
            }

            //process if the category header should be checked
            foreach (Category category in ParsedCategoryList)
                if (category.CategoryHeader != null && category.CategoryHeader.Checked)
                    category.CategoryHeader.Checked = false;
        }

        //generic handler for WPF components
        private void OnWPFComponentCheck(object sender, RoutedEventArgs e)
        {
            if (LoadingUI)
                return;

            if (sender is RelhaxWPFCheckBox)
            {
                OnMultiPackageClick(sender, e);
            }
            else if (sender is RelhaxWPFRadioButton)
            {
                OnSinglePackageClick(sender, e);
            }
        }

        //when a single/single1 mod is selected
        private void OnSinglePackageClick(object sender, EventArgs e)
        {
            if (LoadingUI)
                return;

            IPackageUIComponent ipc = (IPackageUIComponent)sender;
            SelectablePackage spc = ipc.Package;

            if (!processingConflict && spc.AnyConflictingPackages())
            {
                if (!ConflictingPackageContinueOptionA(sender, spc, e))
                    return;
            }

            if (!spc.IsStructureEnabled)
                return;

            //uncheck all packages at this level that are single
            foreach (SelectablePackage childPackage in spc.Parent.Packages)
            {
                if ((childPackage.Type == SelectionTypes.single1) && childPackage.Enabled)
                {
                    if (childPackage.Equals(spc))
                        continue;
                    childPackage.Checked = false;
                    PropagateDownNotChecked(childPackage);
                }
            }

            //check the actual package
            spc.Checked = true;

            //down
            PropagateChecked(spc, SelectionPropagationDirection.PropagateDown);

            //up
            PropagateChecked(spc, SelectionPropagationDirection.PropagateUp);
        }

        //when a single_dropdown mod is selected
        private void OnSingleDDPackageClick(object sender, EventArgs e)
        {
            if (LoadingUI)
                return;

            RelhaxWPFComboBox relhaxWPFComboBox = sender as RelhaxWPFComboBox;

            RelhaxComboBoxItem cb2 = relhaxWPFComboBox.SelectedItem as RelhaxComboBoxItem;

            SelectablePackage spc = cb2.Package;

            if (!processingConflict && spc.AnyConflictingPackages())
            {
                if (!ConflictingPackageContinueOptionA(sender, spc, e))
                    return;
            }

            if (!spc.IsStructureEnabled)
                return;

            foreach (SelectablePackage childPackage in spc.Parent.Packages)
            {
                if (childPackage.Equals(spc))
                    continue;
                //uncheck all packages of the same type
                if (childPackage.Type.Equals(spc.Type))
                {
                    if(childPackage.Checked)
                        childPackage.Checked = false;
                }
            }

            //verify selected is actually checked
            if (!spc.Checked)
                spc.Checked = true;

            //dropdown packages only need to propagate up when selected...
            PropagateChecked(spc, SelectionPropagationDirection.PropagateUp);
        }

        private void OnUserPackageClick(object sender, EventArgs e)
        {
            if (LoadingUI)
                return;

            IPackageUIComponent ipc = (IPackageUIComponent)sender;
            SelectablePackage spc = ipc.Package;

            if (!spc.Checked)
                spc.Checked = true;
            else
                spc.Checked = false;

            spc.TabIndex.OnCheckedChanged(spc.Checked);
        }

        //when a multi mod is selected
        private void OnMultiPackageClick(object sender, EventArgs e)
        {
            if (LoadingUI)
                return;

            IPackageUIComponent ipc = (IPackageUIComponent)sender;
            SelectablePackage spc = ipc.Package;

            if (!processingConflict && spc.AnyConflictingPackages())
            {
                if (!ConflictingPackageContinueOptionA(sender, spc, e))
                    return;
            }

            if (!spc.IsStructureEnabled)
                return;

            //can be enabled
            if (!spc.Checked)
            {
                //check it and propagate change
                spc.Checked = true;

                //down
                PropagateChecked(spc, SelectionPropagationDirection.PropagateDown);
                //up
                PropagateChecked(spc, SelectionPropagationDirection.PropagateUp);
            }
            else if (spc.Checked)
            {
                //uncheck it and propagate change
                spc.Checked = false;

                PropagateDownNotChecked(spc);
            }
        }

        //propagates the change back up the selection tree
        //can be sent from any component
        //true = up, false = down
        private void PropagateChecked(SelectablePackage spc, SelectionPropagationDirection direction)
        {
            //the parent of the package we just checked
            SelectablePackage parent;

            //if we're going up the tree, set the package to it's parent
            //else use itself
            if (direction == SelectionPropagationDirection.PropagateUp)
                parent = spc.Parent;
            else
                parent = spc;
            
            //for each type of required single selection, check if the package has them, and if any are enabled
            bool hasSingles = false;
            bool singleSelected = false;
            bool hasDD1 = false;
            bool DD1Selected = false;
            bool hasDD2 = false;
            bool DD2Selected = false;
            foreach (SelectablePackage childPackage in parent.Packages)
            {
                //if the package is enabled and it is of single type
                if ((childPackage.Type == SelectionTypes.single1) && childPackage.Enabled)
                {
                    //then this package does have single type packages
                    hasSingles = true;
                    //if it's checked, set that bool as well
                    if (childPackage.Checked)
                        singleSelected = true;
                }
                //same idea as above
                else if ((childPackage.Type == SelectionTypes.single_dropdown1) && childPackage.Enabled)
                {
                    hasDD1 = true;
                    if (childPackage.Checked)
                        DD1Selected = true;
                }
                else if (childPackage.Type == SelectionTypes.single_dropdown2 && childPackage.Enabled)
                {
                    hasDD2 = true;
                    if (childPackage.Checked)
                        DD2Selected = true;
                }
            }

            //if going up, will only ever see radio buttons (not dropDown)
            //check if this package is of single type, if it is then we need to unselect all other packages of this level
            if (direction == SelectionPropagationDirection.PropagateUp && (parent.Type == SelectionTypes.single1))
            {
                foreach (SelectablePackage childPackage in parent.Parent.Packages)
                {
                    if ((childPackage.Type == SelectionTypes.single1) && childPackage.Enabled)
                    {
                        if (!childPackage.Equals(parent))
                        {
                            childPackage.Checked = false;
                            PropagateDownNotChecked(childPackage);
                        }
                    }
                }
                //singleSelected = true;
            }
            if (hasSingles && !singleSelected)
            {
                //select one
                foreach (SelectablePackage childPackage in parent.Packages)
                {
                    if ((childPackage.Type == SelectionTypes.single1) && childPackage.Enabled)
                    {
                        childPackage.Checked = true;
                        PropagateChecked(childPackage, SelectionPropagationDirection.PropagateDown);
                        break;
                        //PropagateDownChecked(childPackage);
                    }
                }
            }
            if (hasDD1 && !DD1Selected)
            {
                //select one
                foreach (SelectablePackage childPackage in parent.Packages)
                {
                    if ((childPackage.Type == SelectionTypes.single_dropdown1) && childPackage.Enabled)
                    {
                        childPackage.Checked = true;
                        break;
                        //no need to propagate, dropdown has no children
                    }
                }
            }
            if (hasDD2 && !DD2Selected)
            {
                //select one
                foreach (SelectablePackage childPackage in parent.Packages)
                {
                    if (childPackage.Type == SelectionTypes.single_dropdown2 && childPackage.Enabled)
                    {
                        childPackage.Checked = true;
                        break;
                        //no need to propagate, dropdown has no children
                    }
                }
            }
            //last of all, check itself (if not checked already)
            parent.Checked = true;
            if (direction == SelectionPropagationDirection.PropagateUp)
                if (parent.Level >= 0)
                    //recursively propagate the change back up the selection list
                    PropagateChecked(parent, SelectionPropagationDirection.PropagateUp);
        }

        //propagates the change back up the selection tree
        //NOTE: the only component that can propagate up for a not checked is a multi
        private void PropagateUpNotChecked(SelectablePackage spc)
        {
            if (spc.Level == -1)
                return;
            //if nothing checked at this level, un-check the parent and propagate up not checked again
            bool anythingChecked = false;
            foreach (SelectablePackage childPackage in spc.Parent.Packages)
            {
                if (childPackage.Enabled && childPackage.Checked)
                    anythingChecked = true;
            }
            if (!anythingChecked)
            {
                spc.Parent.Checked = false;
                PropagateUpNotChecked(spc.Parent);
            }
        }

        //propagates the change down the selection tree
        private void PropagateDownNotChecked(SelectablePackage spc)
        {
            foreach (SelectablePackage childPackage in spc.Packages)
            {
                if (!childPackage.Enabled)
                    continue;
                childPackage.Checked = false;
                if (childPackage.Packages.Count > 0)
                    PropagateDownNotChecked(childPackage);
            }
        }

        private bool ConflictingPackageContinueOptionA(object sender, SelectablePackage selectablePackage, EventArgs e)
        {
            //returning true means continue with selection, "option A"
            ConflictingPackageDialog conflictingPackageDialog = new ConflictingPackageDialog(ModpackSettings, selectablePackage);
            conflictingPackageDialog.ShowDialog();

            if (conflictingPackageDialog.OptionASelected)
            {
                Logging.Debug("User selected option a, un-checking all conflicting packages");
                processingConflict = true;
                foreach (SelectablePackage conflictingPackage in selectablePackage.GetConflictingPackages())
                {
                    if (conflictingPackage.Enabled && conflictingPackage.Checked)
                    {
                        switch (conflictingPackage.Type)
                        {
                            case SelectionTypes.multi:
                                OnMultiPackageClick(conflictingPackage.UIComponent, e);
                                break;
                            case SelectionTypes.single1:
                                OnSinglePackageClick(conflictingPackage.UIComponent, e);
                                break;
                            case SelectionTypes.single_dropdown1:
                                OnSingleDDPackageClick(conflictingPackage.RelhaxWPFComboBoxList[0], e);
                                break;
                            case SelectionTypes.single_dropdown2:
                                OnSingleDDPackageClick(conflictingPackage.RelhaxWPFComboBoxList[1], e);
                                break;
                        }
                    }
                }
                processingConflict = false;
            }
            else
            {
                Logging.Debug("User selected option b, un-checking package user originally selected {0}", selectablePackage.PackageName);
                selectablePackage.Checked = false;
            }
            return conflictingPackageDialog.OptionASelected;
        }
        #endregion

        #region Preview Code
        //generic hander for when any mouse button is clicked for MouseDown Events
        void Generic_MouseDown(object sender, EventArgs e)
        {
            if (LoadingUI)
                return;

            //if it's not mouseEventArgs, then abort because we can't determine if it's a right click
            if (e == null && sender is IPackageUIComponent packageSender_)
            {
                Logging.Error(LogOptions.ClassName, "EventArgs is null for Generic_MouseDown() method of package {0}", packageSender_.Package.PackageName);
                return;
            }
            else if (sender is IPackageUIComponent packageSender__ && !packageSender__.Package.Enabled && e is MouseButtonEventArgs m_ && ModpackSettings.ModSelectionView == SelectionView.DefaultV2)
            {
                if (m_.ChangedButton != MouseButton.Right)
                    return;
                Logging.Debug("An event occurred over a disabled component \"{0}\", with default selection view. Event name: {1}", packageSender__.Package.PackageName, m_.RoutedEvent.Name);
            }
            else if (e is MouseEventArgs m)
            {
                if (m.RightButton != MouseButtonState.Pressed)
                    return;
            }
            else
            {
                Logging.Error(LogOptions.ClassName, "Unknown event type for mouse down event: {0}", e.GetType().ToString());
                return;
            }

            SelectablePackage spc = null;
            bool comboboxItemsInside = false;
            if (sender is IPackageUIComponent packageSender)
            {
                spc = packageSender.Package;
            }
            else if (sender is RelhaxWPFComboBox comboboxSender)
            {
                //temp enable all items so that the mouse over property can work
                bool[] wasDisabled = new bool[comboboxSender.Items.Count];
                int tracker = 0;
                foreach (RelhaxComboBoxItem itemInBox1 in comboboxSender.Items)
                {
                    if (!itemInBox1.IsEnabled)
                    {
                        itemInBox1.IsEnabled = true;
                        wasDisabled[tracker] = true;
                    }
                    else
                    {
                        wasDisabled[tracker] = false;
                    }

                    tracker++;
                }
                UiUtils.AllowUIToUpdate();

                //check to see if a specific item is highlighted
                //if so, it means that the user wants to preview a specific version
                //if not, then the user clicked on the combobox as a whole, so show all items in the box
                foreach (RelhaxComboBoxItem itemInBox in comboboxSender.Items)
                {
                    if (itemInBox.IsMouseOver)
                    {
                        spc = itemInBox.Package;
                        break;
                    }
                }
                for(int i = 0; i < wasDisabled.Count(); i++)
                {
                    if(wasDisabled[i])
                    {
                        (comboboxSender.Items[i] as RelhaxComboBoxItem).IsEnabled = false;
                    }
                }

                if (spc == null)
                {
                    //log that it's a combobox sender for the mode in the preview window
                    comboboxItemsInside = true;

                    //make a new temporary package with a custom preview items list
                    //get a temp known good package, doesn't matter what cause we want the parent
                    RelhaxComboBoxItem cbi = (RelhaxComboBoxItem)comboboxSender.Items[0];

                    //parent of item in combobox is header
                    SelectablePackage parentPackage = cbi.Package.Parent;
                    spc = new SelectablePackage();

                    spc.Medias.Clear();
                    foreach (SelectablePackage packageToGetMediaFrom in parentPackage.Packages)
                    {
                        spc.Medias.AddRange(packageToGetMediaFrom.Medias);
                    }
                }
            }

            if (spc == null)
            {
                Logging.Error("Unable to show preview from UI component: {0}", sender.ToString());
                return;
            }

            //check if the window reference exists and if it's loaded (not a closed window, can't re-open a closed window)
            //https://stackoverflow.com/a/49477128/3128017
            //https://stackoverflow.com/a/26124156/3128017
            if (previewWindow != null && previewWindow.IsLoaded)
            {
                //if its not a virgin preview window, use the currently existing one but refresh the contents
                previewWindow.ComboBoxItemsInsideMode = comboboxItemsInside;
                previewWindow.Medias = spc.Medias;
                previewWindow.InvokedPackage = spc;
                previewWindow.Refresh(false);
            }
            else
            {
                previewWindow = new Preview(this.ModpackSettings)
                {
                    ComboBoxItemsInsideMode = comboboxItemsInside,
                    Medias = spc.Medias,
                    InvokedPackage = spc
                };
                previewWindow.Show();
            }
        }

        //Handler for allowing right click of disabled mods (WPF)
        private void Lsl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (LoadingUI)
                return;
            IPackageUIComponent pkg = null;
            if (e.OriginalSource is ContentPresenter cp)
            {
                if (cp.Content is IPackageUIComponent ipc)
                {
                    pkg = ipc;
                }
            }
            if ((pkg != null) && (pkg.Package != null))
            {
                bool packageActuallyDisabled = false;
                SelectablePackage pack = pkg.Package;
                while (pack.Level > -1)
                {
                    if (!pack.Enabled)
                        packageActuallyDisabled = true;
                    pack = pack.Parent;
                }
                if (packageActuallyDisabled)
                {
                    //disabled component, display via generic handler
                    Generic_MouseDown(pkg, e);
                }
            }
        }
        #endregion

        #region Selection stuff
        private void OnSaveSelectionClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog selectSavePath = new SaveFileDialog()
            {
                InitialDirectory = ApplicationConstants.RelhaxUserSelectionsFolderPath,
                AddExtension = true,
                Filter = "XML files|*.xml",
                ValidateNames = true,
                Title = Translations.GetTranslatedString("SelectSelectionFileToSave")
            };
            if((bool)selectSavePath.ShowDialog())
                SaveSelectionV3(selectSavePath.FileName,false);
        }

        private void OnLoadSelectionClick(object sender, RoutedEventArgs e)
        {
            DeveloperSelectionsViewer selections = new DeveloperSelectionsViewer(this.ModpackSettings) { };
            selections.OnDeveloperSelectionsClosed += OnDeveloperSelectionsExit;
            selections.ShowDialog();
        }

        private async void OnDeveloperSelectionsExit(object sender, DevleoperSelectionsClosedEventArgs e)
        {
            if(!e.LoadSelection)
                return;

            if(string.IsNullOrWhiteSpace(e.FileToLoad))
            {
                Logging.WriteToLog("DeveloperSelections returned a blank selection to load when e.LoadSelection = true",Logfiles.Application, LogLevel.Error);
                MessageBox.Show(Translations.GetTranslatedString("failedLoadSelection"));
                return;
            }

            if(e.FileToLoad.Equals("LOCAL"))
            {
                OpenFileDialog selectLoadPath = new OpenFileDialog()
                {
                    InitialDirectory = ApplicationConstants.RelhaxUserSelectionsFolderPath,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    AddExtension = true,
                    Filter = "XML files|*.xml",
                    Title = Translations.GetTranslatedString("MainWindowSelectSelectionFileToLoad"),
                    Multiselect = false,
                    ValidateNames = true
                };

                if((bool)selectLoadPath.ShowDialog())
                {
                    XmlDocument doc = new XmlDocument();
                    try
                    {
                        doc.Load(selectLoadPath.FileName);
                    }
                    catch(XmlException ex)
                    {
                        Logging.WriteToLog(ex.ToString(),Logfiles.Application,LogLevel.Exception);
                        MessageBox.Show(Translations.GetTranslatedString("failedLoadSelection"));
                        return;
                    }
                    LoadSelection(doc,false, selectLoadPath.FileName, out bool selectionFileOutOfDate);
                }
            }
            else
            {
                //get the filename from the developer zip file
                string xmlString = string.Empty;
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        xmlString = await client.DownloadStringTaskAsync(ApplicationConstants.SelectionsRoot + e.FileToLoad);
                    }
                    catch (Exception ex)
                    {
                        Logging.Exception(ex.ToString());
                        MessageBox.Show(Translations.GetTranslatedString("failedToParseSelections"));
                        Close();
                    }
                }

                if (string.IsNullOrWhiteSpace(xmlString))
                {
                    Logging.WriteToLog("xmlString is null or empty", Logfiles.Application, LogLevel.Error);
                    MessageBox.Show(Translations.GetTranslatedString("failedLoadSelection"));
                    return;
                }

                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.LoadXml(xmlString);
                }
                catch(XmlException ex)
                {
                    Logging.WriteToLog(ex.ToString(),Logfiles.Application,LogLevel.Exception);
                    MessageBox.Show(Translations.GetTranslatedString("failedLoadSelection"));
                    return;
                }
                LoadSelection(doc,false,null, out bool selectionFileOutOfDate);
            }
        }

        private void OnClearSelectionsClick(object sender, RoutedEventArgs e)
        {
            Logging.Info("Clearing selections");

            //clear in lists
            ClearSelections(databaseManager.GetFlatSelectablePackageList());
            ClearSelections(UserCategory.GetFlatPackageList());
            UserCategory.CategoryHeader.Checked = false;
            UserCategory.TabPage.OnCheckedChanged(false);

            Logging.Info("Selections cleared");
            MessageBox.Show(Translations.GetTranslatedString("selectionsCleared"));
        }

        private bool LoadSelection(XmlDocument document, bool silent, string loadPath, out bool selectionFileOutOfDate)
        {
            //get the string version of the document, determine what to do from there
            string selectionVersion = XmlUtils.GetXmlStringFromXPath(document, "//mods/@ver");
            //attribute example: "//root/element/@attribute"

            string selectionVersionV3 = XmlUtils.GetXmlStringFromXPath(document, "/packages/@ver");
            if(string.IsNullOrEmpty(selectionVersion))
            {
                selectionVersion = selectionVersionV3;
            }

            selectionFileOutOfDate = false;

            Logging.Debug("SelectionVersion={0}", selectionVersion);
            switch(selectionVersion)
            {
                case "2.0":
                    //assume it is out of date, used for auto install, to trigger the installation
                    selectionFileOutOfDate = true;
                    return LoadSelectionV2(document, silent, loadPath);

                case "3.0":
                    return LoadSelectionV3(document, silent, loadPath, out selectionFileOutOfDate);

                default:
                    //log we don't know wtf it is
                    Logging.Warning("Unknown selection version: " + selectionVersion + ", aborting");
                    if(!silent)
                        MessageBox.Show(string.Format(Translations.GetTranslatedString("unknownselectionFileFormat"),selectionVersion));
                    return false;
            }
        }

        private bool LoadSelectionV2(XmlDocument document, bool silent, string loadPath)
        {
            //as of 2020-05-03, this format is deprecated. We'll still show it. For now.
            if(!silent && !string.IsNullOrEmpty(loadPath))
                MessageBox.Show(Translations.GetTranslatedString("selectionFormatOldV2"));

            if (!string.IsNullOrEmpty(loadPath))
                Logging.Info(LogOptions.MethodAndClassName, "This selection file is V2 and will be upgraded to V3. A V2 backup will be created");
            else
                Logging.Info(LogOptions.MethodAndClassName, "This selection file is V2 but upgrade will be ignored");

            //first uncheck everything
            List<SelectablePackage> packages = databaseManager.GetFlatSelectablePackageList();
            ClearSelections(packages);

            //get a list of all the mods currently in the selection
            XmlNodeList xmlSelections = document.SelectNodes("//mods/relhaxMods/mod");
            XmlNodeList xmluserSelections = document.SelectNodes("//mods/userMods/mod");

            //logging
            Logging.Debug("xmlSelections count: {0}", xmlSelections.Count);
            Logging.Debug("xmluserSelections count: {0}", xmluserSelections.Count);

            //save a list string of all the package names in the list for later
            List<string> stringSelections = new List<string>();
            List<string> stringUserSelections = new List<string>();
            List<string> disabledMods = new List<string>();
            List<SelectablePackage> brokenMods = null;

            foreach (XmlNode node in xmlSelections)
                stringSelections.Add(node.InnerText);
            foreach (XmlNode node in xmluserSelections)
                stringUserSelections.Add(node.InnerText);

            //check the mods in the actual list if it's in the list
            foreach (SelectablePackage package in packages)
            {
                //also check to only "check" the mod if it is visible OR if the command line settings to force visible all components
                if (stringSelections.Contains(package.PackageName) && (package.Visible || ModpackSettings.ForceVisible))
                {
                    stringSelections.Remove(package.PackageName);

                    //if it's the top level, check the category header
                    if (package.Level == 0 && !package.ParentCategory.CategoryHeader.Checked)
                    {
                        package.ParentCategory.CategoryHeader.Checked = true;
                        Logging.Info(LogOptions.MethodName, "Checking top header " + package.ParentCategory.CategoryHeader.NameFormatted);
                    }

                    //also check if the mod only if it's enabled OR is command line settings force enabled
                    if (package.Enabled || ModpackSettings.ForceEnabled)
                    {
                        package.Checked = true;
                        Logging.Info(LogOptions.MethodName, string.Format("Checking package {0}", package.CompletePath));
                    }
                    else
                    {
                        if (ModpackSettings.SaveDisabledMods)
                        {
                            Logging.Debug("SaveDisabledMods=True, flagging disabled mod {0} for future selection later", package.Name);
                            package.FlagForSelectionSave = true;
                        }
                        disabledMods.Add(package.CompletePath);
                        Logging.Info(LogOptions.MethodName, string.Format("\"{0}\" is a disabled mod", package.CompletePath));
                    }
                }
            }

            //do the same as above but for user mods
            foreach (SelectablePackage package in UserCategory.Packages)
            {
                if (stringUserSelections.Contains(Path.GetFileNameWithoutExtension(package.ZipFile)) && File.Exists(Path.Combine(ApplicationConstants.RelhaxUserModsFolderPath, package.ZipFile)))
                {
                    Logging.Info(LogOptions.MethodName, string.Format("Checking User Mod {0}", package.ZipFile));
                    package.Enabled = true;
                    package.Checked = true;
                    stringUserSelections.Remove(Path.GetFileNameWithoutExtension(package.ZipFile));
                }
            }

            //now check for the correct structure of mods
            brokenMods = IsValidStructure(ParsedCategoryList);
            Logging.Info(LogOptions.MethodName, "Broken mods structure count: " + brokenMods.Count);

            //
            int totalBrokenCount = disabledMods.Count + brokenMods.Count + stringSelections.Count + stringUserSelections.Count;
            if (totalBrokenCount > 0 && (AutoInstallMode || ModpackSettings.OneClickInstall) && ModpackSettings.AutoOneclickShowWarningOnSelectionsFail)
            {
                Logging.Info(LogOptions.MethodName, "Selection issues with auto or one click enabled, with message warning enabled. Show message.");
                MessageBoxResult result = MessageBox.Show(
                    Translations.GetTranslatedString("AutoOneclickSelectionErrorsContinueBody"),
                    Translations.GetTranslatedString("AutoOneclickSelectionErrorsContinueHeader"), MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    Logging.Info(LogOptions.MethodName, "User selected stop installation");
                    return false;
                }
            }

            //only report issues if silent is false. true means its doing something like auto selections or
            else if (!silent)
            {
                Logging.Info(LogOptions.MethodName, "Informing user of {0} disabled selections, {1} broken selections, {2} removed selections, {3} removed user selections",
                    disabledMods.Count, brokenMods.Count, stringSelections.Count, stringUserSelections.Count);
                SelectionFileIssuesDisplay window = new SelectionFileIssuesDisplay(this.ModpackSettings);
                int totalCount = disabledMods.Count + stringSelections.Count + stringUserSelections.Count + brokenMods.Count;
                if (disabledMods.Count > 0)
                {
                    totalCount -= disabledMods.Count;
                    //disabled selections
                    window.HeaderText = Translations.GetTranslatedString("modDeactivated");
                    window.BodyText = string.Join(Environment.NewLine, disabledMods);
                    window.Title = Translations.GetTranslatedString("selectionFileIssues");
                    window.ButtonText = Translations.GetTranslatedString(totalCount <= 0 ? "close" : "next");
                    window.ShowDialog();
                }
                if (stringSelections.Count > 0)
                {
                    totalCount -= stringSelections.Count;
                    //removed selections
                    window.HeaderText = Translations.GetTranslatedString("modsNotFoundTechnical");
                    window.BodyText = string.Join(Environment.NewLine, stringSelections);
                    window.Title = Translations.GetTranslatedString("selectionFileIssues");
                    window.ButtonText = Translations.GetTranslatedString(totalCount <= 0 ? "close" : "next");
                    window.ShowDialog();
                }
                if (stringUserSelections.Count > 0)
                {
                    totalCount -= stringUserSelections.Count;
                    //removed user selections
                    window.HeaderText = Translations.GetTranslatedString("modsNotFoundTechnical");
                    window.BodyText = string.Join(Environment.NewLine, stringUserSelections);
                    window.Title = Translations.GetTranslatedString("selectionFileIssues");
                    window.ButtonText = Translations.GetTranslatedString(totalCount <= 0 ? "close" : "next");
                    window.ShowDialog();
                }
                if (brokenMods.Count > 0)
                {
                    //removed structure user selections
                    window.HeaderText = Translations.GetTranslatedString("modsBrokenStructure");
                    window.BodyText = string.Join(Environment.NewLine, brokenMods.Select(package => package.CompletePath).ToArray());
                    window.Title = Translations.GetTranslatedString("selectionFileIssues");
                    window.ButtonText = Translations.GetTranslatedString("close");
                    window.ShowDialog();
                }
                window.Close();
                window = null;
            }
            else
            {
                Logging.Info(LogOptions.MethodName, "Silent = true, logging {0} disabled selections, {1} broken selections, {2} removed selections, {3} removed user selections",
                    disabledMods.Count, brokenMods.Count, stringSelections.Count, stringUserSelections.Count);
            }

            //if not a developer selection file (we have a valid load path, so user, or last loaded, or auto/oneClick), then copy this to a backup file and save/overwrite
            if(!string.IsNullOrEmpty(loadPath))
            {
                string backupFilename = string.Format("{0}_v2_backup.xml", Path.GetFileNameWithoutExtension(loadPath));
                string backupFilepath = Path.Combine(Path.GetDirectoryName(loadPath), backupFilename);
                Logging.Info(LogOptions.MethodName, "Saving old V2 format backup to {0}", backupFilepath);

                if(File.Exists(backupFilepath))
                {
                    Logging.Debug("File already exists, delete to create new");
                    File.Delete(backupFilepath);
                }

                FileUtils.FileMove(loadPath, backupFilepath, 3, 100);
                SaveSelectionV3(loadPath, true);
            }

            return true;
        }

        private bool LoadSelectionV3(XmlDocument document, bool silent, string loadPath, out bool selectionFileOutOfDate)
        {
            //check if it's 'direct load' type
            bool directLoad = false;
            string directLoadString = XmlUtils.GetXmlStringFromXPath(document, "/packages/@directLoad");
            if (!string.IsNullOrEmpty(directLoadString) && CommonUtils.ParseBool(directLoadString, out bool result_, false))
            {
                directLoad = result_;
                Logging.Debug(LogOptions.MethodName, "Parsed directLoad = {0}", directLoad);
            }

            //first uncheck everything
            ClearSelections(databaseManager.GetFlatSelectablePackageList());

            //get a list of all the mods currently in the selection
            XmlNodeList xmlGlobalSelections = document.SelectNodes("/packages/globalPackages/package");
            XmlNodeList xmlDependencySelections = document.SelectNodes("/packages/dependencyPackages/package");
            XmlNodeList xmlSelections = document.SelectNodes("/packages/relhaxPackages/package");
            XmlNodeList xmluserSelections = document.SelectNodes("/packages/userPackages/package");

            //logging
            Logging.Debug(LogOptions.MethodName, "xmlGlobalSelections count:     {0}", xmlGlobalSelections.Count);
            Logging.Debug(LogOptions.MethodName, "xmlDependencySelections count: {0}", xmlDependencySelections.Count);
            Logging.Debug(LogOptions.MethodName, "xmlSelections count:           {0}", xmlSelections.Count);
            Logging.Debug(LogOptions.MethodName, "xmlUserSelections count:       {0}", xmluserSelections.Count);

            //save a list of all the packages for later
            List<DatabasePackage> globalPackagesFromSelection = new List<DatabasePackage>();
            List<Dependency> dependenciesFromSelection = new List<Dependency>();
            List<DatabasePackage> userPackagesFromSelection = new List<DatabasePackage>();
            List<SelectablePackage> packagesFromSelection = new List<SelectablePackage>();

            List<SelectablePackage> packagesFromDatabase = databaseManager.GetFlatSelectablePackageList();

            List<SelectablePackage> brokenStructurePackages = new List<SelectablePackage>();
            List<SelectablePackage> removedPackages = new List<SelectablePackage>();
            List<SelectablePackage> disabledPackages = new List<SelectablePackage>();
            List<DatabasePackage> removedUserPackages = new List<DatabasePackage>();
            List<DatabasePackage> outOfDatePackages = new List<DatabasePackage>();

            //bools for determining if stuff is out of date
            bool globalsOutOfDate = false;
            bool dependenciesOutOfDate = false;
            bool packagesOutOfDate = false;
            bool packageNamesOutOfDate = false;
            bool userOutOfDate = false;

            //get all the globals and check if they are out of date
            Logging.Debug(LogOptions.MethodName, "Parsing global packages from selection file");
            foreach (XmlElement globalXml in xmlGlobalSelections)
            {
                DatabasePackage package = new DatabasePackage();

                foreach (string propertyName in package.AttributesToXmlParseSelectionFiles())
                {
                    LoadV3PropertiesFromXml(package, globalXml, propertyName);
                }

                globalPackagesFromSelection.Add(package);
            }

            Logging.Debug(LogOptions.MethodName, "Parsing dependency packages from selection file");
            foreach(XmlElement dependencyXml in xmlDependencySelections)
            {
                Dependency dependency = new Dependency();

                foreach(string propertyName in dependency.AttributesToXmlParseSelectionFiles())
                {
                    LoadV3PropertiesFromXml(dependency, dependencyXml, propertyName);
                }

                dependenciesFromSelection.Add(dependency);
            }

            //foreach selection, build a package entry for it. compare it with db to check if it's been updated
            Logging.Debug(LogOptions.MethodName, "Parsing selectablePackages from selection file");
            foreach (XmlElement selection in xmlSelections)
            {
                SelectablePackage package = new SelectablePackage();

                //add properties to it
                foreach (string propertyName in package.AttributesToXmlParseSelectionFiles())
                {
                    LoadV3PropertiesFromXml(package, selection, propertyName);
                }

                packagesFromSelection.Add(package);
            }

            //parse user selections
            Logging.Debug(LogOptions.MethodName, "Parsing userPackages from selection file");
            foreach (XmlElement userPackage in xmluserSelections)
            {
                DatabasePackage userPack = new DatabasePackage()
                {
                    PackageName = userPackage.Attributes["name"].InnerText,
                    ZipFile = Path.Combine(ApplicationConstants.RelhaxUserModsFolderPath, string.Format("{0}.zip", userPackage.Attributes["name"].InnerText)),
                    CRC = userPackage.Attributes["crc"].InnerText
                };
                userPackagesFromSelection.Add(userPack);
            }

            //select all the components based on the list from parsed selection file
            Logging.Debug(LogOptions.MethodName, "Processing packages for selection");
            foreach (SelectablePackage packageFromSelection in packagesFromSelection)
            {
                //get the package object from the parsed database list, based on UID property
                SelectablePackage packageFromDatabase = packagesFromDatabase.Find(pack => pack.UID.Equals(packageFromSelection.UID));

                //if it's null, then the package was removed
                if (packageFromDatabase == null)
                {
                    Logging.Info(LogOptions.MethodName, "Package {0} was removed from the database, since last selection save. UID={1}", packageFromSelection.PackageName, packageFromSelection.UID);
                    removedPackages.Add(packageFromSelection);
                    packagesOutOfDate = true;
                    continue;
                    //by not checking it in the database, then it won't be saved in the new save later
                }

                //if the package in the database was set invisible, then treat it as removed
                //unless force visible is on
                if (!ModpackSettings.ForceVisible && !packageFromDatabase.Visible)
                {
                    Logging.Info(LogOptions.MethodName, "Package {0} was hidden since last selection save, act as removed from the database. UID={1}", packageFromSelection.PackageName, packageFromSelection.UID);
                    removedPackages.Add(packageFromSelection);
                    packagesOutOfDate = true;
                    continue;
                }

                //if the package in the database was set disabled, then don't check it and packagesOutOfDate = true
                //unless force enabled is on
                if (!ModpackSettings.ForceEnabled)
                {
                    if (!packageFromDatabase.Enabled)
                    {
                        Logging.Info(LogOptions.MethodName, "Package {0} is disabled in database and still exists in selection. It won't be checked", packageFromDatabase.PackageName);
                        disabledPackages.Add(packageFromSelection);
                        //if setting is high for keeping disabled packages, then don't remove it from selection file
                        if (ModpackSettings.SaveDisabledMods)
                        {
                            Logging.Info(LogOptions.MethodName, "SaveDisabledMods is true, keep this package in the selection file by flagging");
                            packageFromDatabase.FlagForSelectionSave = true;

                            //we also save this value to disk, so if we find it when loading, then it's not the first time
                            //the user has had this package disabled
                            if (packageFromSelection.FlagForSelectionSave)
                            {
                                Logging.Debug(LogOptions.MethodName, "PackageFromSelection FlagForSelectionSave high, not first time loading file with disabled components. Don't trigger out of date");
                                continue;
                            }
                            else
                            {
                                Logging.Debug(LogOptions.MethodName, "PackageFromSelection FlagForSelectionSave low, first time loading file with disabled components. Can trigger out of date flag");
                                packagesOutOfDate = true;
                                continue;
                            }
                        }
                        else
                        {
                            Logging.Info(LogOptions.MethodName, "SaveDisabledMods is false, remove this package from selection file (don't check it)");
                            packagesOutOfDate = true;
                            continue;
                        }
                    }
                }

                //getting here means the package is visible and enabled and not removed
                if (packageFromDatabase.Enabled && packageFromDatabase.Visible)
                {
                    //check if there are any conflicting packages with it
                    if (packageFromDatabase.AnyConflictingPackages())
                    {
                        packageFromDatabase.ForceCheckPackage();
                    }
                    else
                    {
                        Logging.Info(LogOptions.MethodName, "Checking package {0}", packageFromDatabase.PackageName);
                        packageFromDatabase.Checked = true;
                    }
                }
                else
                    Logging.Error("Package {0} was processed to be ready for selection, but is not! Enabled={1}, Visible={2}", packageFromDatabase.Enabled, packageFromDatabase.Visible);
            }

            //now that database packages are checked, check for the correct structure of mods
            //brokenMods = IsValidStructure(ParsedCategoryList); <- V2 method of checking
            //note that this needs to run in a loop until no packages are unchecked due to invalid structure. here's why.
            //the structure is that it goes from top down, which means top level down that branch. If in level 3, it needs to disable
            //that package due to invalid structure, it won't unset 2 because that one had already been processed. If 3 was a single1
            //then that means that level 2 is invalid structure, which means we need to run the loop again
            bool needToRunLoopAgain = true;
            while (needToRunLoopAgain)
            {
                needToRunLoopAgain = false;
                foreach (SelectablePackage checkedPackage in packagesFromDatabase.FindAll(pac => pac.Enabled && pac.Checked && pac.Visible))
                {
                    if (!checkedPackage.IsStructureValid)
                    {
                        Logging.Info(LogOptions.MethodName, "Package {0} reports that it is invalid structure, needs to be unchecked", checkedPackage.PackageName);
                        brokenStructurePackages.Add(checkedPackage);
                        checkedPackage.Checked = false;

                        //also flag that we want to check if we need to run the loop again
                        needToRunLoopAgain = true;

                        //but we still want to save it in the selection list (rather then remove and the user would need to manually find it again)
                        checkedPackage.FlagForSelectionSave = true;

                        //only trigger out of date if it's the firs time loading this file with the structure invalid
                        SelectablePackage packageFromSelection = packagesFromSelection.Find(pc => pc.UID.Equals(checkedPackage.UID));
                        if (packageFromSelection.FlagForSelectionSave)
                        {
                            Logging.Debug(LogOptions.MethodName, "PackageFromSelection FlagForSelectionSave high, not first time loading file with disabled components. Don't trigger out of date");
                        }
                        else
                        {
                            Logging.Debug(LogOptions.MethodName, "PackageFromSelection FlagForSelectionSave low, first time loading file with disabled components. Can trigger out of date flag");
                            packagesOutOfDate = true;
                        }
                    }
                }
            }

            //set the top level package checkboxes to checked if a component inside them is checked
            foreach(Category category in ParsedCategoryList)
            {
                category.CategoryHeader.Checked = category.IsAnyPackageCheckedEnabledVisible();
            }

            //if direct load mode (like default checked), then don't run MaaS or any additional calculations
            if(directLoad)
            {
                Logging.Debug(LogOptions.MethodName, "DirectLoad = true, stopping here");
                selectionFileOutOfDate = false;
                return true;
            }

            //determine if packages are out of date
            Logging.Debug(LogOptions.MethodName, "Processing global packages for selection");
            if (GlobalDependencies.Count != globalPackagesFromSelection.Count)
                globalsOutOfDate = true;

            //compare each package and check if it's out of date
            foreach (DatabasePackage globalDependencyFromSelection in globalPackagesFromSelection)
            {
                DatabasePackage globalDependencyFromDatabase = GlobalDependencies.Find(pack => pack.UID.Equals(globalDependencyFromSelection.UID));
                if (globalDependencyFromDatabase == null)
                {
                    Logging.Info(LogOptions.MethodName, "Global package {0} from selection list was not found in the database list GlobalDependencies. Setting globasOutOfDate to true", globalDependencyFromSelection.PackageName);
                    globalsOutOfDate = true;
                    continue;
                }
                if (IsSelectionV3PackageOutOfDate(globalDependencyFromSelection, globalDependencyFromDatabase))
                {
                    Logging.Info(LogOptions.MethodName, "Global package {0} from selection list is out of date in comparison to the database list GlobalDependencies. Setting globasOutOfDate to true", globalDependencyFromSelection.PackageName);
                    globalsOutOfDate = true;
                    outOfDatePackages.Add(globalDependencyFromSelection);
                }
            }

            //check if dependencies are out of date
            Logging.Debug(LogOptions.MethodName, "Processing dependencies from selection");
            Logging.Debug(LogOptions.MethodName, "First calculate dependencies from currently selected packages");
            databaseManager.CalculateInstallLists(true, false);
            List<Dependency> dependenciesCalculatedFromLoadedSelection = databaseManager.DependenciesToInstall;

            Logging.Debug(LogOptions.MethodName, "Check if number if calculated dependencies == number of loaded dependencies from file");
            if (dependenciesCalculatedFromLoadedSelection.Count != dependenciesFromSelection.Count)
                dependenciesOutOfDate = true;
            Logging.Debug(LogOptions.MethodName, "Dependencies from selection count: {0}, calculated from loaded selection: {1}", dependenciesFromSelection.Count, dependenciesCalculatedFromLoadedSelection.Count);

            Logging.Debug(LogOptions.MethodName, "Check if any new ones exist in loaded list");
            //UIDs of the above lists
            List<string> UIDsDependenciesCalculatedFromLoadedSelection = dependenciesCalculatedFromLoadedSelection.Select(dep => dep.UID).ToList();
            List<string> UIDsDependenciesFromSelection = dependenciesFromSelection.Select(dep => dep.UID).ToList();
                
            //list of UIDs that exist in the dependencies loaded but NOT in loaded list. If count > 0 it means one has been added
            List<string> newUIDsInDependencesFromLoadedSelection = UIDsDependenciesCalculatedFromLoadedSelection.Except(UIDsDependenciesFromSelection).ToList();

            Logging.Debug(LogOptions.MethodName, "New dependencies loaded from selection count: {0}", newUIDsInDependencesFromLoadedSelection.Count);

            if (newUIDsInDependencesFromLoadedSelection.Count > 0)
                dependenciesOutOfDate = true;

            //if nothing new found, check each old to see if it == new
            foreach (Dependency dependencyFromSelection in dependenciesFromSelection)
            {
                Dependency dependencyFromDatabase = Dependencies.Find(dep => dep.UID.Equals(dependencyFromSelection.UID));
                if (dependencyFromDatabase == null)
                {
                    Logging.Debug(LogOptions.MethodName, "Dependency {0} was removed, continue", dependencyFromSelection.PackageName);
                    continue;
                }
                if (IsSelectionV3PackageOutOfDate(dependencyFromSelection, dependencyFromDatabase))
                {
                    Logging.Info(LogOptions.MethodName, "Dependency {0} is out of date from list of Dependencies. Setting dependenciesOutOfDate to true", dependencyFromSelection.PackageName);
                    dependenciesOutOfDate = true;
                    outOfDatePackages.Add(dependencyFromSelection);
                }
            }

            //reset database dependency calculation
            Logging.Debug(LogOptions.MethodName, "Clear dependency calculations to prevent collision from install");
            foreach (Dependency dependency in Dependencies)
                dependency.DatabasePackageLogic.Clear();

            //check if packages are out of date
            Logging.Debug(LogOptions.MethodName, "Processing packages from selection");
            foreach (SelectablePackage packageFromSelection in packagesFromSelection)
            {
                //get the package object from the parsed database list, based on UID property
                SelectablePackage packageFromDatabase = packagesFromDatabase.Find(pack => pack.UID.Equals(packageFromSelection.UID));
                if (packageFromDatabase == null)
                {
                    Logging.Debug(LogOptions.MethodName, "SelectablePackage {0} was removed (checking for out of date), continue", packageFromSelection.PackageName);
                    continue;
                }

                if (IsSelectionV3PackageOutOfDate(packageFromSelection, packageFromDatabase))
                {
                    Logging.Info(LogOptions.MethodName, "Package {0} is out of date from list of Packages. Setting packagesOutOfDate to true", packageFromSelection.PackageName);
                    packagesOutOfDate = true;
                    outOfDatePackages.Add(packageFromSelection);
                }
            }

            //check if packageName is out of date (not included for if package itself is out of date)
            foreach (SelectablePackage packageFromSelection in packagesFromSelection)
            {
                //get the package object from the parsed database list, based on UID property
                SelectablePackage packageFromDatabase = packagesFromDatabase.Find(pack => pack.UID.Equals(packageFromSelection.UID));
                if (packageFromDatabase == null)
                {
                    Logging.Debug(LogOptions.MethodName, "SelectablePackage {0} was removed (checking for package rename), continue", packageFromSelection.PackageName);
                    continue;
                }
                if (IsPackageNameOutOfDate(packageFromSelection, packageFromDatabase))
                {
                    Logging.Info(LogOptions.MethodName, "PackageName {0} is old from database's name of {1}, flagging for remap", packageFromSelection.PackageName, packageFromDatabase.PackageName);
                    packageNamesOutOfDate = true;
                }
            }

            //check if user packages are out of date
            //check if file exists and if md5 is the same
            Logging.Debug(LogOptions.MethodName, "Processing user packages for selection");
            foreach (DatabasePackage userPackage in userPackagesFromSelection)
            {
                SelectablePackage userPackageFromDatabase = UserCategory.Packages.Find(pac => pac.PackageName.Equals(userPackage.PackageName));
                if (userPackageFromDatabase == null)
                {
                    Logging.Info(LogOptions.MethodName, "User package {0} was removed, set userOutOfDate");
                    removedUserPackages.Add(userPackage);
                    userOutOfDate = true;
                    continue;
                }

                Logging.Info(LogOptions.MethodName, "Checking user package {0}", userPackage.PackageName);
                userPackageFromDatabase.Enabled = true;
                userPackageFromDatabase.Checked = true;

                //check the header as well
                if (!UserCategory.CategoryHeader.Checked)
                {
                    UserCategory.CategoryHeader.Checked = true;
                    UserCategory.TabPage.OnCheckedChanged(true);
                }

                //check crc for up to date
                if (!userPackage.CRC.Equals(FileUtils.CreateMD5Hash(userPackage.ZipFile)))
                {
                    Logging.Debug(LogOptions.MethodName, "Md5 hash values do not match, setting userOutOfDate");
                    userOutOfDate = true;
                }
            }

            //display counts of changes
            Logging.Info(LogOptions.MethodName, "Summary of package changes");
            Logging.Info(LogOptions.MethodName, "Removed packages:          {0}", removedPackages.Count);
            Logging.Info(LogOptions.MethodName, "Removed user packages:     {0}", removedUserPackages.Count);
            Logging.Info(LogOptions.MethodName, "Disabled packages:         {0}", disabledPackages.Count);
            Logging.Info(LogOptions.MethodName, "Broken structure packages: {0}", brokenStructurePackages.Count);
            Logging.Info(LogOptions.MethodName, "Out of date packages:      {0}", outOfDatePackages.Count);

            //if in some sort of auto-install mode, check if the user wants to be informed of selection issues
            int totalBrokenCount = removedPackages.Count + removedUserPackages.Count + disabledPackages.Count + brokenStructurePackages.Count + outOfDatePackages.Count;

            //save the xml document if the selection was out of date
            if (globalsOutOfDate || dependenciesOutOfDate || packagesOutOfDate || packageNamesOutOfDate || userOutOfDate)
            {
                Logging.Info(LogOptions.MethodName, "The selection file is out of date, and is being updated (write new version to disk)");
                Logging.Debug("globals={0}, dependencies={1}, packages={2}, packageNames={3}, user={4}", globalsOutOfDate, dependenciesOutOfDate, packagesOutOfDate, packageNamesOutOfDate, userOutOfDate);

                //save the document via save v3
                document = null;
                string filename = Path.GetFileNameWithoutExtension(loadPath);
                string filenameBackup = filename + "_backup.xml";
                string pathBackup = Path.Combine(Path.GetDirectoryName(loadPath), filenameBackup);

                if (File.Exists(pathBackup))
                    FileUtils.FileDelete(pathBackup, 3, 100);
                FileUtils.FileMove(loadPath, pathBackup, 3, 100);

                SaveSelectionV3(loadPath, true);
            }

            if (totalBrokenCount > 0 && (AutoInstallMode || ModpackSettings.OneClickInstall) && ModpackSettings.AutoOneclickShowWarningOnSelectionsFail)
            {
                Logging.Info(LogOptions.MethodName, "Selection issues with auto or one click enabled, with message warning enabled. Show message.");
                MessageBoxResult result = MessageBox.Show(
                    Translations.GetTranslatedString("AutoOneclickSelectionErrorsContinueBody"),
                    Translations.GetTranslatedString("AutoOneclickSelectionErrorsContinueHeader"), MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    Logging.Info("User selected stop installation");
                    selectionFileOutOfDate = false;
                    return false;
                }
            }
            else if (!silent && totalBrokenCount > 0)//only report issues if silent is false and if anything needs to be reported
            {
                SelectionFileIssuesDisplay window = new SelectionFileIssuesDisplay(this.ModpackSettings)
                {
                    Title = Translations.GetTranslatedString("selectionFileIssuesTitle"),
                    HeaderText = Translations.GetTranslatedString("selectionFileIssuesHeader"),
                    ButtonText = Translations.GetTranslatedString("close")
                };

                StringBuilder selectionMessagesBuilder = new StringBuilder();
                string seperate = Environment.NewLine;

                //disabled selections
                if (disabledPackages.Count > 0)
                {
                    selectionMessagesBuilder.AppendLine(Translations.GetTranslatedString("modDeactivated"));
                    selectionMessagesBuilder.AppendLine(string.Join(seperate, disabledPackages.Select(package => "-  " + package.CompletePath).ToArray()));
                    selectionMessagesBuilder.AppendLine();
                }

                //removed selections, db and user
                if (removedUserPackages.Count + removedPackages.Count > 0)
                {
                    selectionMessagesBuilder.AppendLine(Translations.GetTranslatedString("modsNotFoundTechnical"));
                    selectionMessagesBuilder.AppendLine(string.Join(seperate, removedPackages.Concat(removedUserPackages).Select(package => "-  " + package.CompletePath).ToArray()));
                    selectionMessagesBuilder.AppendLine();
                }

                //removed broken structure selection
                if (brokenStructurePackages.Count > 0)
                {
                    selectionMessagesBuilder.AppendLine(Translations.GetTranslatedString("modsBrokenStructure"));
                    selectionMessagesBuilder.AppendLine(string.Join(seperate, brokenStructurePackages.Select(package => "-  " + package.CompletePath).ToArray()));
                    selectionMessagesBuilder.AppendLine();
                }

                //out of date selections
                if(outOfDatePackages.Count > 0)
                {
                    selectionMessagesBuilder.AppendLine(Translations.GetTranslatedString("packagesUpdatedShouldInstall"));
                    selectionMessagesBuilder.AppendLine(string.Join(seperate, outOfDatePackages.Select(package => "-  " + package.CompletePath).ToArray()));
                    selectionMessagesBuilder.AppendLine();
                }

                window.BodyText = selectionMessagesBuilder.ToString();
                window.ShowDialog();
                window.Close();
                window = null;
            }

            selectionFileOutOfDate = globalsOutOfDate || dependenciesOutOfDate || packagesOutOfDate || userOutOfDate;
            return true;
        }

        private bool IsSelectionV3PackageOutOfDate(DatabasePackage packageFromSelection, DatabasePackage packageFromDatabase)
        {
            Logging.Info(LogOptions.MethodAndClassName, "Comparing package: {0}", packageFromDatabase.PackageName);

            Logging.Debug("Selection ZipFile: {0}", packageFromSelection.ZipFile);
            Logging.Debug("Database ZipFile:  {0}", packageFromDatabase.ZipFile);
            if (!packageFromSelection.ZipFile.Equals(packageFromDatabase.ZipFile))
            {
                Logging.Info(LogOptions.MethodAndClassName, "ZipFile is out of date");
                return true;
            }

            Logging.Debug("Selection CRC: {0}", packageFromSelection.CRC);
            Logging.Debug("Database CRC:  {0}", packageFromDatabase.CRC);
            if (!packageFromSelection.CRC.Equals(packageFromDatabase.CRC))
            {
                Logging.Info(LogOptions.MethodAndClassName, "CRC is out of date");
                return true;
            }

            Logging.Debug("Selection Version: {0}", packageFromSelection.Version);
            Logging.Debug("Database Version:  {0}", packageFromDatabase.Version);
            if (!packageFromSelection.Version.Equals(packageFromDatabase.Version))
            {
                Logging.Info(LogOptions.MethodAndClassName, "Version is out of date");
                return true;
            }

            Logging.Debug("Selection Timestamp: {0}", packageFromSelection.Timestamp);
            Logging.Debug("Database Timestamp:  {0}", packageFromDatabase.Timestamp);
            if (!packageFromSelection.Timestamp.Equals(packageFromDatabase.Timestamp))
            {
                Logging.Info(LogOptions.MethodAndClassName, "Timestamp is out of date");
                return true;
            }

            Logging.Debug("Selection Enabled: {0}", packageFromSelection.Enabled);
            Logging.Debug("Database Enabled:  {0}", packageFromDatabase.Enabled);
            if (!packageFromSelection.Enabled.Equals(packageFromDatabase.Enabled))
            {
                Logging.Info(LogOptions.MethodAndClassName, "Enabled is out of date");
                return true;
            }
            return false;
        }

        private bool IsPackageNameOutOfDate(DatabasePackage packageFromSelection, DatabasePackage packageFromDatabase)
        {
            if (!packageFromSelection.PackageName.Equals(packageFromDatabase.PackageName))
            {
                Logging.Warning("The packageName is out of date. Selection = '{0}', Database = '{1}'", packageFromSelection.PackageName, packageFromDatabase.PackageName);
                return true;
            }
            else
                return false;
        }

        private void LoadV3PropertiesFromXml(DatabasePackage package, XmlElement packageXml, string propertyName)
        {
            //get the propertyInfo of that name element
            PropertyInfo property = null;
            try
            { property = package.GetType().GetProperty(propertyName); }
            catch { }

            if (property == null)
            {
                Logging.Error("Unable to get property '{0}' from SelectablePackage object, skipping!", propertyName);
                return;
            }

            //get string value from xml
            string propertyValue = packageXml.Attributes[propertyName].InnerXml;
            if (!string.IsNullOrEmpty(propertyValue))
            {
                //add the property value
                if (!CommonUtils.SetObjectProperty(package, property, propertyValue))
                {
                    Logging.Error("Unable to set property '{0}' value from SelectablePackage object, skipping!", propertyName);
                    return;
                }
            }
        }

        private void SaveSelectionV2(string savePath, bool silent)
        {
            Logging.Info("Saving selections to " + savePath);
            //create saved config xml layout
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
#pragma warning disable CS0618 // Type or member is obsolete
                new XElement("mods", new XAttribute("ver", ApplicationConstants.ConfigFileVersion2V0),
#pragma warning restore CS0618 // Type or member is obsolete
                new XAttribute("date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                new XAttribute("timezone", TimeZoneInfo.Local.DisplayName),
                new XAttribute("dbVersion", DatabaseVersion),
                new XAttribute("dbDistro", databaseManager.DatabaseDistroToLoad.ToString())));

            //relhax mods root
            doc.Element("mods").Add(new XElement("relhaxMods"));

            //user mods root
            doc.Element("mods").Add(new XElement("userMods"));

            //do some cool xml stuff grumpel does
            var nodeRelhax = doc.Descendants("relhaxMods").FirstOrDefault();
            var nodeUserMods = doc.Descendants("userMods").FirstOrDefault();

            //check relhax Mods
            foreach (SelectablePackage package in databaseManager.GetFlatSelectablePackageList())
            {
                if (package.Checked)
                {
                    Logging.Info("Adding relhax mod " + package.PackageName);
                    //add it to the list
                    nodeRelhax.Add(new XElement("mod", package.PackageName));
                }
                else if (ModpackSettings.SaveDisabledMods && package.FlagForSelectionSave)
                {
                    Logging.Info("Adding relhax mod {0} (not checked, but flagged for save)", package.Name);
                    nodeRelhax.Add(new XElement("mod", package.PackageName));
                }
            }

            //check user mods
            foreach (SelectablePackage m in UserCategory.Packages)
            {
                if (m.Checked)
                {
                    Logging.Info("Adding user mod" + m.ZipFile);
                    //add it to the list
                    nodeUserMods.Add(new XElement("mod", m.Name));
                }
            }
            doc.Save(savePath);
            if (!silent)
            {
                MessageBox.Show(Translations.GetTranslatedString("configSaveSuccess"));
            }
        }

        private void SaveSelectionV3(string savePath, bool silent)
        {
            Logging.Info(LogOptions.MethodName, "Saving selections document to {0}", savePath);

            //create saved config xml layout
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"));

            //document root
            XElement packagesRoot = new XElement("packages",
                new XAttribute("ver", ApplicationConstants.ConfigFileVersion3V0),
                new XAttribute("date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                new XAttribute("timezone", TimeZoneInfo.Local.DisplayName),
                new XAttribute("dbVersion", DatabaseVersion),
                new XAttribute("dbDistro", databaseManager.DatabaseDistroToLoad.ToString()));

            doc.Add(packagesRoot);

            //add global root
            XElement nodeGlobal = new XElement("globalPackages");
            packagesRoot.Add(nodeGlobal);

            //add dependencies
            XElement nodeDependencies = new XElement("dependencyPackages");
            packagesRoot.Add(nodeDependencies);

            //relhax mods root
            XElement nodeRelhax = new XElement("relhaxPackages");
            packagesRoot.Add(nodeRelhax);

            //user mods root
            XElement nodeUserMods = new XElement("userPackages");
            packagesRoot.Add(nodeUserMods);

            //add global packages
            Logging.Debug("Saving global dependencies to document");
            foreach(DatabasePackage globalPackage in GlobalDependencies)
            {
                XElement xpackageGlobal = new XElement("package");
                foreach (string propName in globalPackage.AttributesToXmlParseSelectionFiles())
                {
                    SaveV3PropertiesToXmlElement(globalPackage, xpackageGlobal, propName);
                }
                nodeGlobal.Add(xpackageGlobal);
            }

            //calculate dependencies for adding
            Logging.Debug("Running dependency calculation on database");
            databaseManager.CalculateInstallLists(true, false);
            List<Dependency> dependenciesToInstall = databaseManager.DependenciesToInstall;

            Logging.Debug("Saving calculated dependencies to document");
            foreach (Dependency dependency in dependenciesToInstall)
            {
                XElement xpackageGlobal = new XElement("package");
                foreach (string propName in dependency.AttributesToXmlParseSelectionFiles())
                {
                    SaveV3PropertiesToXmlElement(dependency, xpackageGlobal, propName);
                }
                nodeDependencies.Add(xpackageGlobal);
            }

            //reset database dependency calculation
            foreach (Dependency dependency in Dependencies)
                dependency.DatabasePackageLogic.Clear();

            //check relhax Mods
            Logging.Debug("Starting selection save of Relhax packages");
            foreach (SelectablePackage package in databaseManager.GetFlatSelectablePackageList())
            {
                XElement xPackage = null;
                if (package.Checked)
                {
                    Logging.Info("Adding package {0}", package.PackageName);
                }
                else if (ModpackSettings.SaveDisabledMods && package.FlagForSelectionSave)
                {
                    Logging.Info("Adding package {0} (not checked, but flagged for save)", package.Name);
                }
                else
                {
                    continue;
                }
                xPackage = new XElement("package");

                foreach (string propName in package.AttributesToXmlParseSelectionFiles())
                {
                    SaveV3PropertiesToXmlElement(package, xPackage, propName);
                }

                //add the element to the xml container element
                nodeRelhax.Add(xPackage);
            }

            //check user mods
            Logging.Debug("Starting save of user packages");
            foreach (SelectablePackage package in UserCategory.Packages)
            {
                if (package.Checked)
                {
                    Logging.Info("Adding user package {0}", package.PackageName);
                    XElement packagee = new XElement("package", new XAttribute("name", package.Name));
                    packagee.Add(new XAttribute("crc", FileUtils.CreateMD5Hash(package.ZipFile)));
                    nodeUserMods.Add(packagee);
                }
            }

            Logging.Debug("Saving document to disk");
            if (File.Exists(savePath))
                File.Delete(savePath);
            doc.Save(savePath);
            
            Logging.Info(LogOptions.MethodName, "Selection save completed");
            if (!silent)
            {
                MessageBox.Show(Translations.GetTranslatedString("configSaveSuccess"));
            }
        }

        private void SaveV3PropertiesToXmlElement(DatabasePackage package, XElement packageXml, string propName)
        {
            //get the property
            PropertyInfo property = null;
            try
            {
                property = package.GetType().GetProperty(propName);
            }
            catch { }

            if (property == null)
            {
                Logging.Error("Unable to get property '{0}' from DatabasePackage object, skipping!", propName);
                return;
            }

            //add the attribute to the element
            packageXml.Add(new XAttribute(propName, property.GetValue(package)));
        }

        //checks for invalid structure in the selected packages
        //ex: a new mandatory option was added to a mod, but the user does not have it selected
        private List<SelectablePackage> IsValidStructure(List<Category> ParsedCategoryList)
        {
            List<SelectablePackage> brokenPackages = new List<SelectablePackage>();
            foreach (Category cat in ParsedCategoryList)
            {
                if (cat.Packages.Count > 0)
                {
                    foreach (SelectablePackage sp in cat.Packages)
                        IsValidStructure(sp, ref brokenPackages);
                }
                //then check if the header should *still* be checked
                //at this point it is assumed that the structure is valid, meanign that
                //if there is at least on package selected it should be propagated up to level 0
                //so ontly need to do this at level 0
                bool anyPackagesSelected = false;
                foreach(SelectablePackage sp in cat.Packages)
                {
                    if (sp.Enabled && sp.Checked)
                        anyPackagesSelected = true;
                }
                if (!anyPackagesSelected && cat.CategoryHeader.Checked)
                    cat.CategoryHeader.Checked = false;
            }
            return brokenPackages;
        }

        private void IsValidStructure(SelectablePackage Package, ref List<SelectablePackage> brokenPackages)
        {
            if (Package.Checked)
            {
                bool hasSingles = false;
                bool singleSelected = false;
                bool hasDD1 = false;
                bool DD1Selected = false;
                bool hasDD2 = false;
                bool DD2Selected = false;
                foreach (SelectablePackage childPackage in Package.Packages)
                {
                    if ((childPackage.Type == SelectionTypes.single1) && childPackage.Enabled)
                    {
                        hasSingles = true;
                        if (childPackage.Checked)
                            singleSelected = true;
                    }
                    else if ((childPackage.Type == SelectionTypes.single_dropdown1) && childPackage.Enabled)
                    {
                        hasDD1 = true;
                        if (childPackage.Checked)
                            DD1Selected = true;
                    }
                    else if (childPackage.Type == SelectionTypes.single_dropdown2 && childPackage.Enabled)
                    {
                        hasDD2 = true;
                        if (childPackage.Checked)
                            DD2Selected = true;
                    }
                }
                if (hasSingles && !singleSelected)
                {
                    Package.Checked = false;
                    if (!brokenPackages.Contains(Package))
                        brokenPackages.Add(Package);
                }
                if (hasDD1 && !DD1Selected)
                {
                    Package.Checked = false;
                    if (!brokenPackages.Contains(Package))
                        brokenPackages.Add(Package);
                }
                if (hasDD2 && !DD2Selected)
                {
                    Package.Checked = false;
                    if (!brokenPackages.Contains(Package))
                        brokenPackages.Add(Package);
                }
                if (Package.Checked && !Package.Parent.Checked)
                {
                    Package.Checked = false;
                    if (!brokenPackages.Contains(Package))
                        brokenPackages.Add(Package);
                }
            }
            if (Package.Packages.Count > 0)
                foreach (SelectablePackage sep in Package.Packages)
                    IsValidStructure(sep, ref brokenPackages);
        }
        #endregion

        #region Search Box Code
        private void SearchCB_KeyUp(object sender, KeyEventArgs e)
        {
            //see editor search box section for comments and log notes
            SearchCB.IsDropDownOpen = true;
            if (e.Key == Key.Down || e.Key == Key.Up)
            {
                //stop the selection from key events
                e.Handled = true;

                if (SearchCB.Items.Count > 0 && SearchCB.SelectedIndex == -1)
                {
                    SearchCB.SelectedIndex = 0;
                }
            }
            else if(e.Key == Key.Enter)
            {
                if(SearchCB.SelectedItem == null)
                {
                    Logging.Info("enter key pressed for search, but no actual package selected. ignoring");
                    return;
                }
                OnSearchCBSelectionCommitted(SearchCB.SelectedItem as RelhaxComboBoxItem);
            }
            else if (string.IsNullOrWhiteSpace(SearchCB.Text))
            {
                SearchCB.Items.Clear();
                SearchCB.IsDropDownOpen = false;
                SearchCB.SelectedIndex = -1;
            }
            else if (SearchCB.Text.Length > 1)
            {
                if (SearchCB.SelectedIndex != -1)
                {
                    TextBox textBox = (TextBox)((ComboBox)sender).Template.FindName("PART_EditableTextBox", (ComboBox)sender);
                    string temp = SearchCB.Text;
                    SearchCB.SelectedIndex = -1;
                    SearchCB.Text = temp;
                    textBox.SelectionStart = ((ComboBox)sender).Text.Length;
                    textBox.SelectionLength = 0;
                }

                //split the search into an array based on using '*' search
                List<SelectablePackage> searchComponents = new List<SelectablePackage>();
                foreach (string searchTerm in SearchCB.Text.Split('*'))
                {
                    //check if comparing with this tab only
                    if((bool)SearchThisTabOnlyCB.IsChecked)
                    {
                        TabItem selected = (TabItem)ModTabGroups.SelectedItem;
                        searchComponents.AddRange(databaseManager.GetFlatSelectablePackageList().Where(
                            term => term.NameFormatted.ToLower().Contains(searchTerm.ToLower()) && term.IsStructureVisible && term.ShowInSearchList && term.ParentCategory.TabPage.Equals(selected)));
                    }
                    else
                    {
                        //get a list of components that match the search term
                        searchComponents.AddRange(databaseManager.GetFlatSelectablePackageList().Where(
                            term => term.NameFormatted.ToLower().Contains(searchTerm.ToLower()) && term.IsStructureVisible && term.ShowInSearchList));
                    }
                }

                //remove duplicates
                searchComponents = searchComponents.Distinct().ToList();

                //clear and fill the search list again
                SearchCB.Items.Clear();
                foreach (SelectablePackage package in searchComponents)
                {
                    string formatForText = string.Format("{0} [{1}]", package.NameFormatted, package.ParentCategory.Name);
                    SearchCB.Items.Add(new RelhaxComboBoxItem(package, formatForText)
                    {
                        IsEnabled = true,
                        Content = formatForText
                    });
                }
            }
        }

        private void SearchCB_DropDownOpened(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)((ComboBox)sender).Template.FindName("PART_EditableTextBox", (ComboBox)sender);
            textBox.SelectionStart = ((ComboBox)sender).Text.Length;
            textBox.SelectionLength = 0;
        }

        private async void OnSearchCBSelectionCommitted(RelhaxComboBoxItem committedItem)
        {
            //test to make sure the UIComponent is a control (it should be, but at least a test to make sure it's not null)
            Control ctrl = null;
            if (committedItem.Package.UIComponent is Control control)
            {
                ctrl = control;
                FlashTimer.Tag = committedItem.Package;
            }
            else if (committedItem.Package.UIComponent == null && (committedItem.Package.Type == SelectionTypes.single_dropdown1 || committedItem.Package.Type == SelectionTypes.single_dropdown2))
            {
                if(committedItem.Package.Parent.UIComponent is Control ctrll)
                {
                    ctrl = ctrll;
                    FlashTimer.Tag = committedItem.Package.Parent;
                }
            }
            
            if(ctrl == null)
            {
                throw new BadMemeException("Invalid search box selection encountered");
            }

            //focus the tab first, so it is brought into view
            committedItem.Package.ParentCategory.TabPage.Focusable = true;
            committedItem.Package.ParentCategory.TabPage.Focus();

            //https://stackoverflow.com/questions/38532196/bringintoview-is-not-working
            //Note that due to the dispatcher's priority queue, the content may not be available as soon as you make changes (such as select a tab).
            //In that case, you may want to post the bring-into-view request in a lower priority:
            await Dispatcher.InvokeAsync(() =>
            {
                //need to expand the package to this item if selection is legacy
                if(ModpackSettings.ModSelectionView == SelectionView.Legacy)
                {
                    SelectablePackage package = committedItem.Package;
                    while(package.Level > -1)
                    {
                        if (!package.TreeViewItem.IsExpanded)
                            package.TreeViewItem.IsExpanded = true;
                        package = package.Parent;
                    }
                }
                ctrl.BringIntoView();
            }, DispatcherPriority.Background);

            //start the timer to show the item
            OnFlashTimerTick(null, null);
            FlashTimer.Start();
        }

        private void SearchCB_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SearchCB.IsDropDownOpen)
            {
                foreach (RelhaxComboBoxItem item in SearchCB.Items)
                {
                    if (item.IsHighlighted && item.IsMouseOver)
                    {
                        OnSearchCBSelectionCommitted(item);
                    }
                }
            }
        }
        #endregion

        #region Collapse and expand buttons
        private void CollapseAllRealButton_Click(object sender, RoutedEventArgs e)
        {
            //get the category from the tag ref
            TabItem openCategoryTab = ModTabGroups.SelectedItem as TabItem;
            Category openCategory = openCategoryTab.Tag as Category;

            //set the expanded property, it will trigger the method to collapse all but the root
            //openCategory.CategoryHeader.TreeViewItem.IsExpanded = false;

            //get a flat list to toggle
            foreach (SelectablePackage package in openCategory.GetFlatPackageList())
            {
                //toggle them
                if (package.TreeViewItem != null)
                    if (package.TreeViewItem.IsExpanded)
                        package.TreeViewItem.IsExpanded = false;
            }
        }

        private void ExpandAllRealButton_Click(object sender, RoutedEventArgs e)
        {
            //get the category from the tag ref
            TabItem openCategoryTab = ModTabGroups.SelectedItem as TabItem;
            Category openCategory = openCategoryTab.Tag as Category;

            //get a flat list to toggle
            foreach (SelectablePackage package in openCategory.GetFlatPackageList())
            {
                //toggle them
                if (package.TreeViewItem != null)
                    if (!package.TreeViewItem.IsExpanded)
                        package.TreeViewItem.IsExpanded = true;
            }
        }
        #endregion
    }
}
