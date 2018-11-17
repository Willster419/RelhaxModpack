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
    /// <summary>
    /// Interaction logic for ModSelectionList.xaml
    /// </summary>
    public partial class ModSelectionList : RelhaxWindow
    {
        private SolidColorBrush SelectedColor = new SolidColorBrush(Colors.BlanchedAlmond);
        private SolidColorBrush NotSelectedColor = new SolidColorBrush(Colors.White);
        private SolidColorBrush SelectedTextColor = SystemColors.ControlTextBrush;
        private SolidColorBrush NotSelectedTextColor = SystemColors.ControlTextBrush;
        public List<Category> ParsedCategoryList;
        public List<DatabasePackage> GlobalDependencies;
        public List<Dependency> Dependencies;
        private bool continueInstallation  = false;
        private ProgressIndicator loadingProgress;
        public event SelectionListClosedDelegate OnSelectionListReturn;
        private bool Loading = false;

        public ModSelectionList()
        {
            InitializeComponent();
        }

        private async void OnWindowLoad(object sender, RoutedEventArgs e)
        {
            Loading = true;
            //init the lists
            ParsedCategoryList = new List<Category>();
            GlobalDependencies = new List<DatabasePackage>();
            Dependencies = new List<Dependency>();
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
            Loading = false;
            this.Show();
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
            //the below does not work yet
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
            loadProgress.ChildProgressCurrent++;
            loadProgress.ReportMessage = Translations.GetTranslatedString("loadingUI");
            progress.Report(loadProgress);
            //initialize the categories lists
            BuildUIInit(progress, loadProgress, ParsedCategoryList);
            //add the packages for each category
            foreach(Category cat in ParsedCategoryList)
            {
                AddPackage(progress, loadProgress, cat.Packages);
            }
            return true;
        }

        private void BuildUserMods()
        {
            //stub
        }

        private void BuildUIInit(IProgress<RelhaxProgress> progress, RelhaxProgress loadProgress, List<Category> parsedCategoryList)
        {
            //one time init of stuff goes here (init the tabGroup would have been nice if needed here)
            foreach(Category cat in parsedCategoryList)
            {
                //build per cateogry tab here
                //like all the UI stuff and linking internally
                //make the tab page
                cat.TabPage = new TabItem()
                {
                    //Background TODO
                    Name = cat.Name
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
                //just in case
                if(ModTabGroups.Items.Count > 0)
                    ModTabGroups.Items.Clear();
                switch(ModpackSettings.ModSelectionView)
                {
                    case SelectionView.Legacy:
                        cat.CategoryHeader.TreeView = new TreeView();
                        cat.CategoryHeader.ChildStackPanel = new StackPanel();
                        cat.CategoryHeader.ChildBorder = new Border()
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = ModpackSettings.EnableBordersLegacyView? new Thickness(1) : new Thickness(0),
                            Child = cat.CategoryHeader.ChildStackPanel,
                            Margin = new Thickness(-25, 0, 0, 0)
                        };
                        //i know i don't need to do this but i'm doing it anyways
                        if (cat.CategoryHeader.TreeView.Items.Count > 0)
                            cat.CategoryHeader.TreeView.Items.Clear();
                        cat.CategoryHeader.TreeViewItem.Items.Add(cat.CategoryHeader.ChildBorder);
                        cat.CategoryHeader.TreeViewItem.IsExpanded = true;
                        //TODO MOUSE DOWN
                        //TODO BACKGROUND?
                        IPackageUIComponent categoryTop = new RelhaxWPFCheckBox()
                        {
                            Package = cat.CategoryHeader,
                            Content = cat.CategoryHeader.NameFormatted,
                            //forground TODO
                        };
                        //TODO ON WPF COMPONENT CLICK
                        cat.CategoryHeader.UIComponent = cat.CategoryHeader.ParentUIComponent = cat.CategoryHeader.TopParentUIComponent = categoryTop;
                        cat.CategoryHeader.Packages = cat.Packages;
                        cat.CategoryHeader.TreeViewItem.Header = cat.CategoryHeader.UIComponent;
                        break;
                    case SelectionView.DefaultV2:
                        cat.CategoryHeader.ParentStackPanel = new StackPanel();
                        cat.CategoryHeader.ParentBorder = new Border()
                        {
                            //background TODO
                            Child = cat.CategoryHeader.ParentStackPanel,
                            Padding = new Thickness(2)
                        };
                        //put parent stack panel into parent border
                        //cat tabpage add top border (parentBorder)
                        //COLOR UI BACKGROUND TODO
                        //ON WPF TODO
                        //create checkbox for inside selecteionlist
                        RelhaxWPFCheckBox cb2 = new RelhaxWPFCheckBox()
                        {
                            Package = cat.CategoryHeader,
                            Content = cat.CategoryHeader.NameFormatted,
                            //Foreground = Settings.GetTextColorWPF(),//TODO
                            HorizontalAlignment = HorizontalAlignment.Left
                        };
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

        private void AddPackage(IProgress<RelhaxProgress> progress, RelhaxProgress loadProgress, List<SelectablePackage> packages)
        {
            foreach(SelectablePackage package in packages)
            {
                //do all the package UI building here
                //should NOT have to do any of the memory linking since that was all done for us above in a utility #likeABoss


                //howerver
                if(package.Packages.Count > 0)
                    AddPackage(progress,loadProgress, package.Packages);
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

        private void RelhaxWindow_Closed(object sender, EventArgs e)
        {
            if(OnSelectionListReturn != null)
            {
                OnSelectionListReturn(this, new SelectionListEventArgs()
                { ContinueInstallation = continueInstallation, ParsedCategoryList = ParsedCategoryList });
            }
        }
    }
}
