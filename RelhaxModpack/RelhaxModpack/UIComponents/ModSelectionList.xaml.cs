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
using System.Windows.Media;
using System.Xml;
using System.Net;
using System.IO;
using RelhaxModpack.UIComponents;

namespace RelhaxModpack.Windows
{
    #region structs and stuff
    public struct RelhaxProgress
    {
        public string ReportMessage;
        public int ChildProgressCurrent;
        public int ChildProgressTotal;
    }
    //https://stackoverflow.com/questions/623451/how-can-i-make-my-own-event-in-c
    public class SelectionListEventArgs : EventArgs
    {
        public bool ContinueInstallation = false;
        public List<Category> ParsedCategoryList;
    }
    public delegate void SelectionListClosedDelegate(object sender, SelectionListEventArgs e);
    #endregion
    /// <summary>
    /// Interaction logic for ModSelectionList.xaml
    /// </summary>
    public partial class ModSelectionList : RelhaxWindow
    {
        public List<Category> ParsedCategoryList;
        public List<DatabasePackage> GlobalDependencies;
        public List<Dependency> Dependencies;
        private bool continueInstallation  = false;
        private ProgressIndicator loadingProgress;
        public event SelectionListClosedDelegate OnSelectionListReturn;
        private bool LoadingConfig = false;
        private bool IgnoreSearchBoxFocus = false;
        private List<SelectablePackage> modSearchList;
        private List<SelectablePackage> userMods;

        #region Boring stuff
        public ModSelectionList()
        {
            InitializeComponent();
        }

        private void OnWindowLoadReportProgress(RelhaxProgress progress)
        {
            if (loadingProgress != null)
            {
                loadingProgress.Message = progress.ReportMessage;
                loadingProgress.ProgressValue = progress.ChildProgressCurrent;
                loadingProgress.ProgressMaximum = progress.ChildProgressTotal;
            }
        }

        private void OnContinueInstallation(object sender, RoutedEventArgs e)
        {
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
            if (OnSelectionListReturn != null)
            {
                OnSelectionListReturn(this, new SelectionListEventArgs()
                { ContinueInstallation = continueInstallation, ParsedCategoryList = ParsedCategoryList });
            }
        }
        #endregion

        #region UI INIT STUFF
        private async void OnWindowLoad(object sender, RoutedEventArgs e)
        {
            LoadingConfig = true;
            //init the lists
            ParsedCategoryList = new List<Category>();
            GlobalDependencies = new List<DatabasePackage>();
            Dependencies = new List<Dependency>();
            modSearchList = new List<SelectablePackage>();
            //create and show loading window
            loadingProgress = new ProgressIndicator()
            {
                ProgressMaximum = 8,
                ProgressMinimum = 0,
                Message = Translations.GetTranslatedString("loading")
            };
            loadingProgress.Show();
            this.Hide();
            //create and run async task
            try
            {
                Logging.WriteToLog("Starting async task: " + nameof(ActuallyLoadModSelectionListAsync) + "()");
                //https://blogs.msdn.microsoft.com/dotnet/2012/06/06/async-in-4-5-enabling-progress-and-cancellation-in-async-apis/
                Progress<RelhaxProgress> progressIndicator = new Progress<RelhaxProgress>(OnWindowLoadReportProgress);
                bool result = await ActuallyLoadModSelectionListAsync(progressIndicator);
                if (!result)
                    throw new BadMemeException("Result was false reeeeeee!!");
            }
            catch (Exception ex)
            {
                Logging.WriteToLog("Failed to load ModSelectionList window\n" + ex.ToString(), Logfiles.Application, LogLevel.Exception);
                MessageBox.Show(Translations.GetTranslatedString("failedToLoadSelectionList"),
                    Translations.GetTranslatedString("critical"));
                loadingProgress.Close();
                loadingProgress = null;
                this.Close();
                return;
            }
            loadingProgress.Close();
            loadingProgress = null;
            LoadingConfig = false;
            //this.WindowState = WindowState.Normal;
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        

        private async Task<bool> ActuallyLoadModSelectionListAsync(IProgress<RelhaxProgress> progress)
        {
            RelhaxProgress loadProgress = new RelhaxProgress()
            {
                ChildProgressTotal = 4,
                ChildProgressCurrent = 1,
                ReportMessage = Translations.GetTranslatedString("downloadingDatabase")
            };
            progress.Report(loadProgress);
            //download online modInfo into xml file
            XmlDocument modInfoDocument = new XmlDocument();
            string modInfoXml = "";
            //get is based on different types of database mode
            switch(ModpackSettings.DatabaseDistroVersion)
            {
                case DatabaseVersions.Stable:
                    //make string
                    string modInfoxmlURL = Settings.DefaultStartAddress + "modInfo.dat";
                    modInfoxmlURL = modInfoxmlURL.Replace("{onlineFolder}", Settings.WoTModpackOnlineFolderVersion);
                    //download dat file
                    string tempDownloadLocation = Path.Combine(Settings.RelhaxTempFolder, "modInfo.dat");
                    using (WebClient client = new WebClient())
                    {
                        try
                        {
                            await client.DownloadFileTaskAsync(modInfoxmlURL, tempDownloadLocation);
                        }
                        catch (Exception ex)
                        {
                            Logging.WriteToLog("Failed to download managerInfo.dat from " + modInfoxmlURL + "\n" + ex.ToString(),
                                Logfiles.Application, LogLevel.Exception);
                            return false;
                        }
                    }
                    //extract modinfo xml string
                    modInfoXml = Utils.GetStringFromZip(tempDownloadLocation, "modInfo.xml");
                    break;
                case DatabaseVersions.Beta:
                    //load string constant url from manager info xml
                    string managerInfoXml = Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "manager_version.xml");
                    if (string.IsNullOrWhiteSpace(managerInfoXml))
                    {
                        Logging.WriteToLog("Failed to parse manager_version.xml from string from zipfile", Logfiles.Application, LogLevel.Exception);
                        MessageBox.Show(Translations.GetTranslatedString("failedToParse") + " manager_version.xml");
                        return false;
                    }
                    //get download URL of static beta database location
                    string downloadURL = XMLUtils.GetXMLStringFromXPath(managerInfoXml, "//version/database_beta_url", "manager_version.xml");
                    if(string.IsNullOrWhiteSpace(downloadURL))
                    {
                        Logging.WriteToLog("Failed to get xpath value //version/database_beta_url from manager_version.xml",
                            Logfiles.Application, LogLevel.Exception);
                        return false;
                    }
                    //download document from string
                    using (WebClient client = new WebClient())
                    {
                        modInfoXml = await client.DownloadStringTaskAsync(downloadURL);
                    }
                    break;
                case DatabaseVersions.Test:
                    //make string
                    string modInfoFilePath = ModpackSettings.CustomModInfoPath;
                    if(string.IsNullOrWhiteSpace(modInfoFilePath))
                    {
                        modInfoFilePath = System.IO.Path.Combine(Settings.ApplicationStartupPath, "modInfo.xml");
                    }
                    //load modinfo xml
                    if (System.IO.File.Exists(modInfoFilePath))
                        modInfoXml = System.IO.File.ReadAllText(modInfoFilePath);
                    else
                    {
                        Logging.WriteToLog("modInfo.xml does not exist at " + modInfoFilePath, Logfiles.Application, LogLevel.Error);
                        return false;
                    }
                    break;
            }
            if (string.IsNullOrWhiteSpace(modInfoXml))
            {
                Logging.WriteToLog("Failed to read modInfoxml xml string", Logfiles.Application, LogLevel.Exception);
                MessageBox.Show(Translations.GetTranslatedString("failedToParse") + " modInfo.xml");
                return false;
            }
            try
            {
                modInfoDocument.LoadXml(modInfoXml);
            }
            catch (XmlException ex)
            {
                Logging.WriteToLog("Failed to parse modInfoxml from xml string\n" + ex.ToString(), Logfiles.Application, LogLevel.Exception);
                MessageBox.Show(Translations.GetTranslatedString("failedToParse") + " modInfo.xml");
                return false;
            }
            //if not stable db, update current version and online folder version from modInfoxml itself
            if(ModpackSettings.DatabaseDistroVersion != DatabaseVersions.Stable)
            {
                Settings.WoTModpackOnlineFolderVersion = XMLUtils.GetXMLStringFromXPath(modInfoDocument, "//modInfoAlpha.xml@onlineFolder");
                Settings.WoTClientVersion = XMLUtils.GetXMLStringFromXPath(modInfoDocument, "//modInfoAlpha.xml@version");
            }
            //parse the modInfoXml to list in memory
            loadProgress.ChildProgressCurrent++;
            loadProgress.ReportMessage = Translations.GetTranslatedString("parsingDatabase");
            progress.Report(loadProgress);
            if(!XMLUtils.ParseDatabase(modInfoDocument,GlobalDependencies,Dependencies,ParsedCategoryList))
            {
                Logging.WriteToLog("Failed to parse database",Logfiles.Application,LogLevel.Error);
                MessageBox.Show(Translations.GetTranslatedString("failedToParse") + " modInfo.xml");
                return false;
            }
            Utils.BuildLinksRefrence(ParsedCategoryList);
            Utils.BuildLevelPerPackage(ParsedCategoryList);
            List<DatabasePackage> flatList = Utils.GetFlatList(GlobalDependencies, Dependencies, null, ParsedCategoryList);
            //check db cache of local files
            loadProgress.ChildProgressCurrent++;
            loadProgress.ReportMessage = Translations.GetTranslatedString("verifyingDownloadCache");
            progress.Report(loadProgress);
            //the below does not work yet TODO: CHECK?
            List<DatabasePackage> flatListZips = flatList.Where(package => !string.IsNullOrWhiteSpace(package.ZipFile)).ToList();
            foreach(DatabasePackage package in flatListZips)
            {
                string zipFile = Path.Combine(Settings.RelhaxDownloadsFolder, package.ZipFile);
                //only look for a crc if the cache file exists
                if (!File.Exists(zipFile))
                    continue;
                string name = package.PackageName;
                if(package is SelectablePackage sp)
                {
                    name = sp.NameFormatted;
                }
                loadProgress.ReportMessage=string.Format(Translations.GetTranslatedString("loading") + " " + name);
                progress.Report(loadProgress);
                string oldCRCFromDownloadsFolder = await Utils.CreateMD5HashAsync(Path.Combine(Settings.RelhaxDownloadsFolder, package.ZipFile));
                if (!package.CRC.Equals(oldCRCFromDownloadsFolder))
                    package.DownloadFlag = true;
            }
            //build UI
            loadProgress.ChildProgressCurrent = 0;
            loadProgress.ReportMessage = Translations.GetTranslatedString("loadingUI");
            progress.Report(loadProgress);
            //initialize the categories lists
            BuildUIInit(ref progress, ref loadProgress, ParsedCategoryList);
            //link everything again now that the category exists
            Utils.BuildLinksRefrence(ParsedCategoryList);
            //initialize the user mods
            BuildUserMods();
            //add the packages for each category
            loadProgress.ChildProgressTotal = Utils.GetFlatList(null, null, null, ParsedCategoryList).Count;
            loadProgress.ChildProgressTotal += userMods.Count;
            foreach(Category cat in ParsedCategoryList)
            {
                AddPackage(ref progress, ref loadProgress, cat.Packages);
            }
            //add the user mods
            AddUserMods();
            //finish loading
            //update the text on the list
            TanksPath.Text = string.Format(Translations.GetTranslatedString("installingTo"), Settings.WoTDirectory);
            TanksVersionLabel.Text = string.Format(Translations.GetTranslatedString("installingAsWoT"), Settings.WoTClientVersion);
            //determind if the collapse and expand buttons should be visible
            switch(ModpackSettings.ModSelectionView)
            {
                case SelectionView.DefaultV2:
                    CollapseAllButton.IsEnabled = false;
                    CollapseAllButton.Visibility = Visibility.Hidden;
                    ExpandAllButton.IsEnabled = false;
                    ExpandAllButton.Visibility = Visibility.Hidden;
                    break;
                case SelectionView.Legacy:
                    CollapseAllButton.IsEnabled = true;
                    CollapseAllButton.Visibility = Visibility.Visible;
                    ExpandAllButton.IsEnabled = true;
                    ExpandAllButton.Visibility = Visibility.Visible;
                    break;
            }
            //deal with ceate used files??
            //save database hash?
            //if mods sync
            //else if auto install
            //else if saveLastConfig
            //else {load default checked}
            return true;
        }

        private void BuildUserMods()
        {
            //get a list of all zip files in the folder
            string[] zipFilesUserMods = Directory.GetFiles(Settings.RelhaxUserModsFolder, @"*.zip");
            userMods = new List<SelectablePackage>();
            foreach (string s in zipFilesUserMods)
            {
                SelectablePackage sp = new SelectablePackage
                {
                    ZipFile = s,
                    Name = Path.GetFileNameWithoutExtension(s),
                    Enabled = true,
                    Level = 0
                };
                //circular refrence because
                sp.Parent = sp.TopParent = sp;
                userMods.Add(sp);
            }
        }

        private void AddUserMods()
        {
            StackPanel userStackPanel = new StackPanel();
            TabItem userTab = new TabItem()
            {
                Name = "UserMods",
                Header = Translations.GetTranslatedString("userMods"),
            };
            userTab.Content = userStackPanel;
            ModTabGroups.Items.Add(userTab);
            foreach(SelectablePackage package in userMods)
            {
                RelhaxWPFCheckBox userMod = new RelhaxWPFCheckBox()
                {
                    Package = package,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    //FONT/BACKGROUND TODO
                    IsChecked = false,
                    IsEnabled = true
                };
                //EVENT TODO
                userStackPanel.Children.Add(userMod);
            }
        }

        private void BuildUIInit(ref IProgress<RelhaxProgress> progress, ref RelhaxProgress loadProgress, List<Category> parsedCategoryList)
        {
            //one time init of stuff goes here (init the tabGroup would have been nice if needed here)
            //just in case
            if (ModTabGroups.Items.Count > 0)
                ModTabGroups.Items.Clear();
            foreach (Category cat in parsedCategoryList)
            {
                //build per cateogry tab here
                //like all the UI stuff and linking internally
                //make the tab page
                cat.TabPage = new TabItem()
                {
                    //Background TODO
                    Header = cat.Name,
                    //HorizontalAlignment = HorizontalAlignment.Left,
                    //VerticalAlignment = VerticalAlignment.Center,
                    //MinWidth = 50,
                    //MaxWidth = 150,
                    //Width = 0
                    
                };
                //Sorts the mods
                Utils.SortModsList(cat.Packages);
                //make and attach the category header
                cat.CategoryHeader = new SelectablePackage()
                {
                    Name = string.Format("----------[{0}]----------", cat.Name),
                    TabIndex = cat.TabPage,
                    ParentCategory = cat,
                    Type = "multi",
                    Visible = true,
                    Enabled = true,
                    Level = -1,
                    PackageName = string.Format("Category_{0}_Header",cat.Name.Replace(' ','_'))
                };
                //creates a refrence to itself
                cat.CategoryHeader.Parent = cat.CategoryHeader;
                cat.CategoryHeader.TopParent = cat.CategoryHeader;
                switch(ModpackSettings.ModSelectionView)
                {
                    case SelectionView.Legacy:
                        cat.CategoryHeader.TreeView = new TreeView();
                        cat.CategoryHeader.TreeView.MouseDown += Lsl_MouseDown;
                        cat.CategoryHeader.ChildStackPanel = new StackPanel();
                        cat.CategoryHeader.ChildBorder = new Border()
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = ModpackSettings.EnableBordersLegacyView? new Thickness(1) : new Thickness(0),
                            Child = cat.CategoryHeader.ChildStackPanel,
                            Margin = new Thickness(-25, 0, 0, 0)
                        };
                        if (cat.CategoryHeader.TreeView.Items.Count > 0)
                            cat.CategoryHeader.TreeView.Items.Clear();
                        cat.CategoryHeader.TreeViewItem.Items.Add(cat.CategoryHeader.ChildBorder);
                        cat.CategoryHeader.TreeViewItem.IsExpanded = true;
                        //TODO BACKGROUND
                        RelhaxWPFCheckBox box = new RelhaxWPFCheckBox()
                        {
                            Package = cat.CategoryHeader,
                            Content = cat.CategoryHeader.NameFormatted,
                            //forground TODO
                        };
                        cat.CategoryHeader.UIComponent = box;
                        box.Click += OnWPFComponentCheck;
                        cat.CategoryHeader.ParentUIComponent = cat.CategoryHeader.TopParentUIComponent = cat.CategoryHeader.UIComponent;
                        cat.CategoryHeader.TreeViewItem.Header = cat.CategoryHeader.UIComponent;
                        cat.CategoryHeader.TreeView.Items.Add(cat.CategoryHeader.TreeViewItem);
                        cat.TabPage.Content = cat.CategoryHeader.TreeView;
                        cat.CategoryHeader.Packages = cat.Packages;
                        break;
                    case SelectionView.DefaultV2:
                        cat.CategoryHeader.ParentStackPanel = new StackPanel();
                        cat.CategoryHeader.ParentBorder = new Border()
                        {
                            //background TODO
                            Child = cat.CategoryHeader.ParentStackPanel,
                            Padding = new Thickness(2)
                        };
                        cat.CategoryHeader.ScrollViewer = new ScrollViewer()
                        {
                            //BACKROUND TODO
                            Content = cat.CategoryHeader.ParentBorder
                        };
                        //tab page -> scrollViewer -> Border -> stackPanel
                        cat.TabPage.Content = cat.CategoryHeader.ScrollViewer;
                        //COLOR UI BACKGROUND TODO
                        //create checkbox for inside selecteionlist
                        RelhaxWPFCheckBox cb2 = new RelhaxWPFCheckBox()
                        {
                            Package = cat.CategoryHeader,
                            Content = cat.CategoryHeader.NameFormatted,
                            //Foreground = Settings.GetTextColorWPF(),//TODO
                            HorizontalAlignment = HorizontalAlignment.Left
                        };
                        cb2.Click += OnWPFComponentCheck;
                        //set it's parent and top parent to itself
                        cat.CategoryHeader.UIComponent = cat.CategoryHeader.ParentUIComponent = cat.CategoryHeader.TopParentUIComponent = cb2;
                        //create and link the child borderand stackpanel
                        cat.CategoryHeader.ChildStackPanel = new StackPanel();
                        cat.CategoryHeader.ChildBorder = new Border()
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = ModpackSettings.EnableBordersDefaultV2View? new Thickness(1) : new Thickness(0),
                            Child = cat.CategoryHeader.ChildStackPanel,
                            Padding = new Thickness(15,0,0,0)
                        };
                        //add the category header item to the stack panel
                        cat.CategoryHeader.ParentStackPanel.Children.Add((Control)cat.CategoryHeader.UIComponent);
                        //add the child border to the parent stack panel
                        cat.CategoryHeader.ParentStackPanel.Children.Add(cat.CategoryHeader.ChildBorder);
                        cat.CategoryHeader.Packages = cat.Packages;
                        break;
                }
                ModTabGroups.Items.Add(cat.TabPage);
            }
        }

        private void AddPackage(ref IProgress<RelhaxProgress> progress, ref RelhaxProgress loadProgress, List<SelectablePackage> packages)
        {
            foreach(SelectablePackage package in packages)
            {
                //do all the package UI building here
                //should NOT have to do any of the memory linking since that was all done for us above in a utility #likeABoss
                //but first check if we actually want to add it. if the program isn't forcing them to be enabled
                //and the mod reports being disabled, then don't add it to the UI
                //the counter needs to still be kept up to date with the list (the whole list includes invisible mods!)
                loadProgress.ChildProgressCurrent++;
                if (!CommandLineSettings.ForceVisible && !package.Visible)
                    continue;
                //now that we are actually adding it, report some progress
                loadProgress.ReportMessage = string.Format("{0} {1}", Translations.GetTranslatedString("loading"), package.NameFormatted);
                progress.Report(loadProgress);
                //ok now actuallt load the UI stuff
                //parse command line stuff. if we're forcinfg it to be enabled or visable
                if (CommandLineSettings.ForceVisible && !package.IsStructureVisible)
                    package.Visible = true;
                if (CommandLineSettings.ForceEnabled && !package.IsStructureEnabled)
                    package.Enabled = true;
                //add the package to the search list if the package parameter specifies it to be added
                if (package.ShowInSearchList)
                    modSearchList.Add(package);
                //link the parent panels and border to childs
                package.ParentBorder = package.Parent.ChildBorder;
                package.ParentStackPanel = package.Parent.ChildStackPanel;
                //special code for the borders and stackpanels
                //if the child container for sub options hsa yet to be made AND there are sub options, make it
                if(package.ChildBorder == null && package.Packages.Count > 0)
                {
                    package.ChildStackPanel = new StackPanel();
                    package.ChildBorder = new Border()
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = ModpackSettings.EnableBordersDefaultV2View ? new Thickness(1) : new Thickness(0),
                        Child = package.ChildStackPanel,
                        //background TODO
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
                switch(package.Type)
                {
                    case "single":
                    case "single1":
                        package.UIComponent = new RelhaxWPFRadioButton()
                        {
                            ToolTip = package.ToolTipString,
                            Package = package,
                            //FONT? TODO
                            //TODO: DOES HORIZONAL BREAK LEGACY??
                            HorizontalAlignment = HorizontalAlignment.Left,
                            HorizontalContentAlignment = HorizontalAlignment.Left,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            Content = package.NameDisplay,
                            IsEnabled = package.IsStructureEnabled,
                            //the UI building code ONLY deals with BUILDING the UI, not loading configuration options!!
                            //so make it false and later when loading selection it will mark it
                            //BACKGROUND FORGROUND TODO
                            IsChecked = false
                        };
                        break;
                    case "single_dropdown":
                    case "single_dropdown1":
                        DoComboboxStuff(package, 0);
                        break;
                    case "single_dropdown2":
                        DoComboboxStuff(package, 1);
                        break;
                    case "multi":
                        package.UIComponent = new RelhaxWPFCheckBox()
                        {
                            ToolTip = package.ToolTipString,
                            Package = package,
                            //FONT? TODO
                            HorizontalAlignment = HorizontalAlignment.Left,
                            HorizontalContentAlignment = HorizontalAlignment.Left,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            Content = package.NameDisplay,
                            IsEnabled = package.IsStructureEnabled,
                            IsChecked = false
                            //BACKGROUND FORGROUND TODO
                        };
                        break;
                }
                //filters out the null UIComponents like if dropdown
                if(package.UIComponent != null)
                {
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
                            //expand the tree view item
                            package.TreeViewItem.IsExpanded = true;
                            //and add the treeviewitem to the stackpanel
                            package.Parent.ChildStackPanel.Children.Add(package.TreeViewItem);
                            break;
                    }
                }
                //howerver
                if (package.Packages.Count > 0)
                {
                    if(ModpackSettings.ModSelectionView == SelectionView.DefaultV2)
                    {
                        //if there are child packages, they will be in the child border
                        //so add the child border to the parent (where this package is) stackpanel
                        package.ParentStackPanel.Children.Add(package.ChildBorder);
                    }
                    AddPackage(ref progress, ref loadProgress, package.Packages);
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
                    Name = "notAddedYet",
                    IsEnabled = false,
                    //FONT?
                    MinWidth = 100,
                    //TODO: BELOW OK IN LEGACY?
                    MaxWidth = 420,//yes, really
                    HorizontalAlignment = HorizontalAlignment.Left
                };
            }
            ComboBoxItem cbi = new ComboBoxItem(package, package.NameDisplay)
            {
                IsEnabled = package.IsStructureEnabled,
            };
            package.Parent.RelhaxWPFComboBoxList[boxIndex].Items.Add(cbi);
            if (package.Parent.RelhaxWPFComboBoxList[boxIndex].Name.Equals("notAddedYet"))
            {
                //lol add it
                package.Parent.RelhaxWPFComboBoxList[boxIndex].Name = "added";
                package.Parent.RelhaxWPFComboBoxList[boxIndex].PreviewMouseRightButtonDown += Generic_MouseDown;
                package.Parent.RelhaxWPFComboBoxList[boxIndex].SelectionChanged += OnSingleDDPackageClick;
                package.Parent.RelhaxWPFComboBoxList[boxIndex].handler = OnSingleDDPackageClick;
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

        #region UI Interaction
        //generic handler to disable the auto check like in forms, but for WPF
        void OnWPFComponentCheck(object sender, RoutedEventArgs e)
        {
            if (LoadingConfig)
                return;
            if (sender is RelhaxWPFCheckBox cb)
            {
                if ((bool)cb.IsChecked)
                    cb.IsChecked = false;
                else if (!(bool)cb.IsChecked)
                    cb.IsChecked = true;
                OnMultiPackageClick(sender, e);
            }
            else if (sender is RelhaxWPFRadioButton rb)
            {
                if ((bool)rb.IsChecked)
                    rb.IsChecked = false;
                else if (!(bool)rb.IsChecked)
                    rb.IsChecked = true;
                OnSinglePackageClick(sender, e);
            }
        }

        //when a single/single1 mod is selected
        void OnSinglePackageClick(object sender, EventArgs e)
        {
            if (LoadingConfig || IgnoreSearchBoxFocus)
                return;
            IPackageUIComponent ipc = (IPackageUIComponent)sender;
            SelectablePackage spc = ipc.Package;
            if (!spc.IsStructureEnabled)
                return;
            //uncheck all packages at this level that are single
            foreach (SelectablePackage childPackage in spc.Parent.Packages)
            {
                if ((childPackage.Type.Equals("single") || childPackage.Type.Equals("single1")) && childPackage.Enabled)
                {
                    if (childPackage.Equals(spc))
                        continue;
                    childPackage.Checked = false;
                    PropagateDownNotChecked(childPackage);
                }
            }
            //check the acutal package
            spc.Checked = true;
            //down
            PropagateChecked(spc, false);
            //up
            PropagateChecked(spc, true);
        }

        //when a single_dropdown mod is selected
        void OnSingleDDPackageClick(object sender, EventArgs e)
        {
            if (LoadingConfig || IgnoreSearchBoxFocus)
                return;
            IPackageUIComponent ipc = (IPackageUIComponent)sender;
            SelectablePackage spc = null;
            if (ipc is RelhaxWPFComboBox cb2)
            {
                ComboBoxItem cbi = (ComboBoxItem)cb2.SelectedItem;
                spc = cbi.Package;
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
                    childPackage.Checked = false;
                }
            }
            //verify selected is actually checked
            if (!spc.Checked)
                spc.Checked = true;
            //dropdown packages only need to propagate up when selected...
            PropagateChecked(spc, true);
        }

        //when a multi mod is selected
        void OnMultiPackageClick(object sender, EventArgs e)
        {
            if (LoadingConfig || IgnoreSearchBoxFocus)
                return;
            IPackageUIComponent ipc = (IPackageUIComponent)sender;
            SelectablePackage spc = ipc.Package;
            if (!spc.IsStructureEnabled)
                return;
            //can be enabled
            if (!spc.Checked)
            {
                //check it and propagate change
                spc.Checked = true;
                //if it's a user checkbox end here
                //DISABLED FOR NOW
                //if (ipc is RelhaxUserCheckBox)
                   // return;
                //down
                PropagateChecked(spc, false);
                //up
                PropagateChecked(spc, true);
            }
            else if (spc.Checked)
            {
                //uncheck it and propagate change
                spc.Checked = false;
                //if (ipc is RelhaxUserCheckBox)
                   // return;
                //up then down
                //PropagateUpNotChecked(spc);
                PropagateDownNotChecked(spc);
            }
        }

        //propagates the change back up the selection tree
        //can be sent from any component
        //true = up, false = down
        void PropagateChecked(SelectablePackage spc, bool upDown)
        {
            //the parent of the package we just checked
            SelectablePackage parent = null;
            //if we're going up the tree, set the package to it's parent
            //else use itself
            if (upDown)
                parent = spc.Parent;
            else
                parent = spc;
            //first of all, check itself (if not checked already)
            parent.Checked = true;
            //for each type of requried single selection, check if the package has them, and if any are enabled
            bool hasSingles = false;
            bool singleSelected = false;
            bool hasDD1 = false;
            bool DD1Selected = false;
            bool hasDD2 = false;
            bool DD2Selected = false;
            foreach (SelectablePackage childPackage in parent.Packages)
            {
                //if the pacakge is enabled and it is of single type
                if ((childPackage.Type.Equals("single") || childPackage.Type.Equals("single1")) && childPackage.Enabled)
                {
                    //then this package does have single type packages
                    hasSingles = true;
                    //if it's checked, set that bool as well
                    if (childPackage.Checked)
                        singleSelected = true;
                }
                //same idea as above
                else if ((childPackage.Type.Equals("single_dropdown") || childPackage.Type.Equals("single_dropdown1")) && childPackage.Enabled)
                {
                    hasDD1 = true;
                    if (childPackage.Checked)
                        DD1Selected = true;
                }
                else if (childPackage.Type.Equals("single_dropdown2") && childPackage.Enabled)
                {
                    hasDD2 = true;
                    if (childPackage.Checked)
                        DD2Selected = true;
                }
            }
            //if going up, will only ever see radiobuttons (not dropDown)
            //check if this package is of single type, if it is then we need to unselect all other packages of this level
            if (upDown && (parent.Type.Equals("single") || parent.Type.Equals("single1")))
            {
                foreach (SelectablePackage childPackage in parent.Parent.Packages)
                {
                    if ((childPackage.Type.Equals("single") || childPackage.Type.Equals("single1")) && childPackage.Enabled)
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
                    if ((childPackage.Type.Equals("single") || childPackage.Type.Equals("single1")) && childPackage.Enabled)
                    {
                        childPackage.Checked = true;
                        PropagateChecked(childPackage, false);
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
                    if ((childPackage.Type.Equals("single_dropdown") || childPackage.Type.Equals("single_dropdown1")) && childPackage.Enabled)
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
                    if (childPackage.Type.Equals("single_dropdown2") && childPackage.Enabled)
                    {
                        childPackage.Checked = true;
                        break;
                        //no need to propagate, dropdown has no children
                    }
                }
            }
            if (upDown)
                if (parent.Level >= 0)
                    //recursivly propagate the change back up the selection list
                    PropagateChecked(parent, true);
        }

        //propagates the change back up the selection tree
        //NOTE: the only component that can propagate up for a not checked is a multi
        void PropagateUpNotChecked(SelectablePackage spc)
        {
            if (spc.Level == -1)
                return;
            //if nothing cheched at this level, uncheck the parent and propagate up not checked agailn
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

        //propagaetes the change down the selection tree
        void PropagateDownNotChecked(SelectablePackage spc)
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
        #endregion

        #region Preview Code
        //generic hander for when any mouse button is clicked for MouseDown Events
        void Generic_MouseDown(object sender, EventArgs e)
        {
            if (LoadingConfig)
                return;
            if (e is MouseEventArgs m)
                if (m.RightButton != MouseButtonState.Pressed)
                    return;
            if (sender is IPackageUIComponent ipc)
            {
                SelectablePackage spc = ipc.Package;
                if (ipc is RelhaxWPFComboBox cb2)
                {
                    ComboBoxItem cbi = (ComboBoxItem)cb2.SelectedItem;
                    spc = cbi.Package;
                }
                if (spc.DevURL == null)
                    spc.DevURL = "";
                //PREVIEW GENERATION DISABELD
                /*
                if (p != null)
                {
                    p.Close();
                    p.Dispose();
                    p = null;
                    GC.Collect();
                }
                p = new Preview()
                {
                    LastUpdated = LastUpdated,
                    DBO = spc,
                    Medias = spc.PictureList
                };
                p.Show();
                */
            }
        }

        //Handler for allowing right click of disabled mods (WPF)
        private void Lsl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (LoadingConfig)
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
                    Generic_MouseDown(pkg, null);
                }
            }
        }
        #endregion

        #region Selection stuff
        private void OnSaveSelectionClick(object sender, RoutedEventArgs e)
        {

        }

        private void OnLoadSelectionClick(object sender, RoutedEventArgs e)
        {

        }

        private void OnClearSelectionsClick(object sender, RoutedEventArgs e)
        {
            Logging.WriteToLog("Clearing selections");
            foreach (Category category in ParsedCategoryList)
                ClearSelections(category.Packages);
            Logging.WriteToLog("Selections cleared");
            MessageBox.Show(Translations.GetTranslatedString("selectionsCleared"));
        }

        private void ClearSelections(List<SelectablePackage> packages)
        {
            foreach(SelectablePackage package in packages)
            {
                if (package.Packages.Count > 0)
                    ClearSelections(package.Packages);
                package.Checked = false;
            }
        }

        private void LoadSelection(XmlDocument document, List<SelectablePackage> parsedCategoryList)
        {
            //get the string version of the document, determine what to do from there
            string selectionVersion = "";
            //attribute example: "//root/element/@attribute"
            selectionVersion = XMLUtils.GetXMLStringFromXPath(document, "//mods@ver");//TODO: CHECK THIS
            switch(selectionVersion)
            {
                case "2.0":
                    LoadSelectionV2(document, parsedCategoryList);
                break;

                default:
                    //log we don't know wtf it is
                    Logging.WriteToLog("Unknown selection version: " + selectionVersion + ", aborting");
                    MessageBox.Show(string.Format(Translations.GetTranslatedString("unknownselectionFileFormat"),selectionVersion));
                    return;
                break;
            }
        }

        private void LoadSelectionV2(XmlDocument document, List<SelectablePackage> parsedCategoryList)
        {

        }

        private void SaveSelection()
        {

        }
        #endregion

    }
}
