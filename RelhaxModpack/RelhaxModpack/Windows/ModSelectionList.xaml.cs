using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Net;
using System.IO;
using Microsoft.Win32;
using RelhaxModpack.UIComponents;
using System.Xml.Linq;
using ComboBoxItem = RelhaxModpack.UIComponents.ComboBoxItem;
using System.Windows.Threading;

namespace RelhaxModpack.Windows
{
    #region Structs and stuff
    /// <summary>
    /// Event arguments for when the selection list is closed
    /// </summary>
    /// <remarks>See https://stackoverflow.com/questions/623451/how-can-i-make-my-own-event-in-c </remarks>
    public class SelectionListEventArgs : EventArgs
    {
        /// <summary>
        /// If the installation should be continued or if the user canceled
        /// </summary>
        public bool ContinueInstallation = false;

        /// <summary>
        /// The list of categories
        /// </summary>
        public List<Category> ParsedCategoryList;

        /// <summary>
        /// The list of dependencies
        /// </summary>
        public List<Dependency> Dependencies;

        /// <summary>
        /// The list of global dependencies
        /// </summary>
        public List<DatabasePackage> GlobalDependencies;

        /// <summary>
        /// The list of use mods
        /// </summary>
        public List<SelectablePackage> UserMods;
    }

    /// <summary>
    /// The delegate to invoke when calling back to the sender for the SelectionClosed event
    /// </summary>
    /// <param name="sender">The sender (this)</param>
    /// <param name="e">The event arguments to send to the installer (MainWindow)</param>
    public delegate void SelectionListClosedDelegate(object sender, SelectionListEventArgs e);

    /// <summary>
    /// The UI checked propagation directions
    /// </summary>
    /// <remarks>When a UI package element is checked, it needs to propagate the checked behavior up or down to prevent an invalid selection</remarks>
    public enum SelectionPropagationDirection
    {
        /// <summary>
        /// Up the higher parent levels
        /// </summary>
        PropagateUp,

        /// <summary>
        /// Down to lower child levels
        /// </summary>
        PropagateDown
    }
    #endregion

    /// <summary>
    /// Interaction logic for ModSelectionList.xaml
    /// </summary>
    public partial class ModSelectionList : RelhaxWindow
    {
        //public
        /// <summary>
        /// The list of categories
        /// </summary>
        public List<Category> ParsedCategoryList;

        /// <summary>
        /// The list of global dependencies
        /// </summary>
        public List<DatabasePackage> GlobalDependencies;

        /// <summary>
        /// The list of dependencies
        /// </summary>
        public List<Dependency> Dependencies;

        /// <summary>
        /// The event that a caller can subscribe to wait for when the selection window actually closes, with arguments for the installation
        /// </summary>
        public event SelectionListClosedDelegate OnSelectionListReturn;

        //private
        private bool continueInstallation  = false;
        private ProgressIndicator loadingProgress;
        private bool LoadingUI = false;
        private Category UserCategory = null;
        private Preview p;
        const int FLASH_TICK_INTERVAL = 250;
        const int NUM_FLASH_TICKS = 5;
        private int numTicks = 0;
        private Brush OriginalBrush = null;
        private Brush HighlightBrush = new SolidColorBrush(Colors.Blue);
        private readonly System.Windows.Forms.Timer FlashTimer = new System.Windows.Forms.Timer() { Interval=FLASH_TICK_INTERVAL };
        private XDocument Md5HashDocument;
        private DatabaseVersions databaseVersion;

        #region Boring stuff
        /// <summary>
        /// Create an instance of the ModSelectionList window
        /// </summary>
        public ModSelectionList()
        {
            InitializeComponent();
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
                Logging.Debug("Saving selection from continue button, when saveLastSelection is true");
                SaveSelection(Settings.LastInstalledConfigFilepath, true);
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
            if (p != null)
            {
                p.Close();
                p = null;
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
                ParsedCategoryList = ParsedCategoryList,
                Dependencies = Dependencies,
                GlobalDependencies = GlobalDependencies,
                UserMods = UserCategory.Packages
            });
        }

        private void OnFlastTimerTick(object sender, EventArgs e)
        {
            if(!(FlashTimer.Tag is SelectablePackage))
            {
                Logging.Error("FlashTimer.Tag is not of SelectablePackage type");
                return;
            }
            SelectablePackage packageToChange = (SelectablePackage)FlashTimer.Tag;

            if(!(packageToChange.UIComponent is Control))
            {
                Logging.Error("packageToChange.UiComponent is not of Control type");
                return;
            }
            Control control = (Control)packageToChange.UIComponent;

            switch (numTicks++)
            {
                case 0:
                    //backup the current color and set the background to the flash color
                    OriginalBrush = control.Foreground;
                    control.Foreground = HighlightBrush;
                    break;
                case NUM_FLASH_TICKS:
                    //stop the timer and reset everyting
                    FlashTimer.Stop();
                    numTicks = 0;
                    control.Foreground = OriginalBrush;
                    OriginalBrush = null;
                    break;
                default:
                    //toggle the color
                    if (control.Foreground.Equals(HighlightBrush))
                    {
                        control.Foreground = OriginalBrush;
                    }
                    else if (control.Foreground.Equals(OriginalBrush))
                    {
                        control.Foreground = HighlightBrush;
                    }
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

            //show the list and hide this window
            loadingProgress.Show();
            Hide();

            //create and run async task (fire and forget style, keeps the UI thread open during the task operation)
            Logging.Info("Starting async task: " + nameof(LoadModSelectionListAsync) + "()");
            //https://blogs.msdn.microsoft.com/dotnet/2012/06/06/async-in-4-5-enabling-progress-and-cancellation-in-async-apis/
            Progress<RelhaxProgress> progressIndicator = new Progress<RelhaxProgress>();
            progressIndicator.ProgressChanged += OnWindowLoadReportProgress;
            LoadModSelectionListAsync(progressIndicator);
        }

        private Task LoadModSelectionListAsync(IProgress<RelhaxProgress> progress)
        {
            return Task.Run(() =>
            {
                //progress init setup
                RelhaxProgress loadProgress = new RelhaxProgress()
                {
                    ChildTotal = 3,
                    ChildCurrent = 1,
                    ReportMessage = Translations.GetTranslatedString("readingDatabase")
                };
                progress.Report(loadProgress);

                //save database version to temp and process if command line test mode
                databaseVersion = ModpackSettings.DatabaseDistroVersion;
                if (CommandLineSettings.TestMode)
                {
                    Logging.Info("test mode set for installation only (not saved to settings)");
                    databaseVersion = DatabaseVersions.Test;
                }

                //get the XML database loaded into a string based on database version type (from server download, from github, from testfile

                string modInfoXml = "";
                switch (databaseVersion)
                {
                    //from server download
                    case DatabaseVersions.Stable:
                        //make string
#pragma warning disable CS0618
                        string modInfoxmlURL = Settings.WotmodsDatabaseRoot + "modInfo.dat";
                        modInfoxmlURL = modInfoxmlURL.Replace("{onlineFolder}", Settings.WoTModpackOnlineFolderVersion);

                        //download latest modInfo xml
                        using(WebClient client = new WebClient())
                        using(Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(new MemoryStream(client.DownloadData(modInfoxmlURL))))
                        {
                            //extract modinfo xml string
                            modInfoXml = Utils.GetStringFromZip(zip, "modInfo.xml");
                        }
                        break;
                    //from github
                    case DatabaseVersions.Beta:
                        using (WebClient client = new WebClient())
                        {
                            //load string constant url from manager info xml
                            //V2 here
                            string rootbetaDBURL = Settings.BetaDatabaseV2FolderURL.Replace("{branch}", ModpackSettings.BetaDatabaseSelectedBranch);
                            string rootXml = rootbetaDBURL + Settings.BetaDatabaseV2RootFilename;
                            /////////

                            //V1 here
                            //aparently the #warning directive in this file causes an intellisense error with XDocuemtn for some reason
                            //#warning using V1 beta database
                            rootXml = Settings.BetaDatabaseV1URL;
//#pragma warning enable CS0618
                            /////////

                            //download the xml string into "modInfoXml"
                            client.Headers.Add("user-agent", "Mozilla / 4.0(compatible; MSIE 6.0; Windows NT 5.2;)");
                            modInfoXml = client.DownloadString(rootXml);
                        }
                        break;
                    //from testfile
                    case DatabaseVersions.Test:
                        //make string
                        if (string.IsNullOrWhiteSpace(ModpackSettings.CustomModInfoPath))
                        {
                            ModpackSettings.CustomModInfoPath = Path.Combine(Settings.ApplicationStartupPath, Settings.BetaDatabaseV2RootFilename);
                        }
                        //load modinfo xml
                        modInfoXml = File.ReadAllText(ModpackSettings.CustomModInfoPath);
                        break;
                }

                //check to make sure the xml string has xml in it
                if (string.IsNullOrWhiteSpace(modInfoXml))
                {
                    Logging.WriteToLog("Failed to read modInfoxml xml string", Logfiles.Application, LogLevel.Exception);
                    MessageBox.Show(Translations.GetTranslatedString("failedToParse") + " modInfo.xml");
                    return false;
                }

                //load the xml document into xml object
                XmlDocument modInfoDocument = XmlUtils.LoadXmlDocument(modInfoXml, XmlLoadType.FromString);
                if(modInfoDocument == null)
                {
                    Logging.Error("Failed to parse modInfoxml from xml string");
                    MessageBox.Show(Translations.GetTranslatedString("failedToParse") + " modInfo.xml");
                    return false;
                }

                //if not stable db, update WoT current version and online folder version macros from modInfoxml itself
                if (databaseVersion != DatabaseVersions.Stable)
                {
                    Settings.WoTModpackOnlineFolderVersion = XmlUtils.GetXmlStringFromXPath(modInfoDocument, "//modInfoAlpha.xml/@onlineFolder");
                    Settings.WoTClientVersion = XmlUtils.GetXmlStringFromXPath(modInfoDocument, "//modInfoAlpha.xml/@version");
                }

                //parse the modInfoXml to list in memory
                switch(databaseVersion)
                {
                    case DatabaseVersions.Stable:
                        //2.0 test
                        /*
                        Logging.Debug("getting xml string values from zip file");
                        List<string> categoriesXml = new List<string>();
                        string globalDependencyXmlString = Utils.GetStringFromZip(zipfilePath, GetXMLStringFromXPath(rootDocument, "/modInfoAlpha.xml/globalDependencies/@file"));
                        string dependenicesXmlString = Utils.GetStringFromZip(zipfilePath, GetXMLStringFromXPath(rootDocument, "/modInfoAlpha.xml/dependencies/@file"));
                        foreach (XmlNode categoryNode in GetXMLNodesFromXPath(rootDocument, "//modInfoAlpha.xml/categories/category"))
                        {
                            categoriesXml.Add(Utils.GetStringFromZip(zipfilePath, GetXMLStringFromXPath(rootDocument, categoryNode.Attributes["file"].Value)));
                        }

                        //parse into lists
                        if (!XMLUtils.ParseDatabase1V1FromStrings(globalDependencyXmlString, dependenicesXmlString,categoriesXml, GlobalDependencies, Dependencies, ParsedCategoryList))
                        {
                            Logging.WriteToLog("Failed to parse database", Logfiles.Application, LogLevel.Error);
                            MessageBox.Show(Translations.GetTranslatedString("failedToParse") + " modInfo.xml");
                            return false;
                        }
                        */
                        if (!XmlUtils.ParseDatabase(modInfoDocument, GlobalDependencies, Dependencies, ParsedCategoryList))
                        {
                            Logging.WriteToLog("Failed to parse database", Logfiles.Application, LogLevel.Error);
                            MessageBox.Show(Translations.GetTranslatedString("failedToParse") + " modInfo.xml");
                            return false;
                        }
                        break;

                    case DatabaseVersions.Beta:
                        //for 2.0
                        /*
                        string rootbetaDBURL = Settings.BetaDatabaseV2FolderURL.Replace("{branch}", ModpackSettings.BetaDatabaseSelectedBranch);

                        //download the files
                        List<string> downloadTasks = new List<string>();
                        Logging.Debug("starting downloads to globalDependnecies, dependencies, all categories");
                        using (WebClient client = new WebClient())
                        {
                            client.Headers.Add("user-agent", "Mozilla / 4.0(compatible; MSIE 6.0; Windows NT 5.2;)");
                            //global dependencies
                            downloadTasks.Add(client.DownloadString(rootbetaDBURL + XMLUtils.GetXMLStringFromXPath(modInfoDocument, "/modInfoAlpha.xml/globalDependencies/@file")));
                            //dependencies
                            downloadTasks.Add(client.DownloadString(rootbetaDBURL + XMLUtils.GetXMLStringFromXPath(modInfoDocument, "/modInfoAlpha.xml/dependencies/@file")));
                            //categories
                            foreach (XmlNode categoryNode in XMLUtils.GetXMLNodesFromXPath(modInfoDocument, "//modInfoAlpha.xml/categories/category"))
                            {
                                string categoryFileName = categoryNode.Attributes["file"].Value;
                                if(string.IsNullOrWhiteSpace(categoryFileName))
                                {
                                    BadMemeException badMeme = new BadMemeException("Failed to parse V2 database category: " + categoryNode.ToString());
                                    Logging.Exception(badMeme.ToString());
                                    throw badMeme;
                                }
                                downloadTasks.Add(client.DownloadString(rootbetaDBURL + categoryFileName));
                            }
                        }
                        List<string> categoriesXml = new List<string>();

                        Logging.Debug("tasks finished, extracting task results and sending to database string parser");

                        //parse into string
                        string globalDependencyXmlString = downloadTasks[0];
                        string dependenicesXmlString = downloadTasks[1];
                        for (int i = 2; i < downloadTasks.Count; i++)
                        {
                            categoriesXml.Add(downloadTasks[i]);
                        }

                        //parse into lists
                        if (!XMLUtils.ParseDatabase1V1FromStrings(globalDependencyXmlString, dependenicesXmlString,categoriesXml, GlobalDependencies, Dependencies, ParsedCategoryList))
                        {
                            Logging.WriteToLog("Failed to parse database", Logfiles.Application, LogLevel.Error);
                            MessageBox.Show(Translations.GetTranslatedString("failedToParse") + "database V2");
                            return false;
                        }*/
                        if (!XmlUtils.ParseDatabase(modInfoDocument, GlobalDependencies, Dependencies, ParsedCategoryList))
                        {
                            Logging.WriteToLog("Failed to parse database", Logfiles.Application, LogLevel.Error);
                            MessageBox.Show(Translations.GetTranslatedString("failedToParse") + " modInfo.xml");
                            return false;
                        }
                        break;

                    case DatabaseVersions.Test:
#warning using old database for test mode
                        if (!XmlUtils.ParseDatabase(modInfoDocument, GlobalDependencies, Dependencies, ParsedCategoryList))
                        {
                            Logging.WriteToLog("Failed to parse database", Logfiles.Application, LogLevel.Error);
                            MessageBox.Show(Translations.GetTranslatedString("failedToParse") + " modInfo.xml");
                            return false;
                        }
                        /*
                        if (!XmlUtils.ParseDatabase1V1FromFiles(Path.GetDirectoryName(ModpackSettings.CustomModInfoPath), modInfoDocument, GlobalDependencies, Dependencies, ParsedCategoryList))
                        {
                            Logging.WriteToLog("Failed to parse database", Logfiles.Application, LogLevel.Error);
                            MessageBox.Show(Translations.GetTranslatedString("failedToParse") + " modInfo.xml");
                            return false;
                        }
                        */
                        break;
                }

                //map and link all references inside the package objects for use later
                Utils.BuildLinksRefrence(ParsedCategoryList, false);
                Utils.BuildLevelPerPackage(ParsedCategoryList);
                List<DatabasePackage> flatList = Utils.GetFlatList(GlobalDependencies, Dependencies, null, ParsedCategoryList);

                //check db cache of local files in download zip folder
                loadProgress.ChildCurrent++;
                loadProgress.ReportMessage = Translations.GetTranslatedString("verifyingDownloadCache");
                progress.Report(loadProgress);

                //check if the md5 hash database file exists, if not then make it
                if (!File.Exists(Settings.MD5HashDatabaseXmlFile))
                {
                    Md5HashDocument = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("database"));
                }
                else
                {
                    Md5HashDocument = XmlUtils.LoadXDocument(Settings.MD5HashDatabaseXmlFile, XmlLoadType.FromFile);
                    if (Md5HashDocument == null)
                    {
                        Logging.Warning("Failed to load md5 hash document, creating new");
                        File.Delete(Settings.MD5HashDatabaseXmlFile);
                        Md5HashDocument = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("database"));
                    }
                }

                //make a sublist of only packages where a zipfile exists (in the database)
                List<DatabasePackage> flatListZips = flatList.Where(package => !string.IsNullOrWhiteSpace(package.ZipFile)).ToList();
                foreach (DatabasePackage package in flatListZips)
                {
                    //make path for the zipfile
                    string zipFile = Path.Combine(Settings.RelhaxDownloadsFolderPath, package.ZipFile);

                    //only look for a crc if the cache file exists
                    if (!File.Exists(zipFile))
                    {
                        //set the download flag since it doesn't exist
                        package.DownloadFlag = true;
                        continue;
                    }

                    //since file exists, report progress here
                    loadProgress.ReportMessage = string.Format("{0} {1}",
                        Translations.GetTranslatedString("verifyingDownloadCache"), package.PackageName);
                    progress.Report(loadProgress);

                    //compares the crcs of the files
                    string oldCRCFromDownloadsFolder = GetMD5Hash(zipFile);
                    if (!package.CRC.Equals(oldCRCFromDownloadsFolder))
                        package.DownloadFlag = true;
                }

                //and save the file
                Md5HashDocument.Save(Settings.MD5HashDatabaseXmlFile);

                //sort the database for UI display
                Utils.SortDatabase(ParsedCategoryList);

                //build UI
                loadProgress.ChildCurrent = 0;
                loadProgress.ReportMessage = Translations.GetTranslatedString("loadingUI");
                progress.Report(loadProgress);
                Utils.AllowUIToUpdate();

                //run UI init code
                //note that this will syncronously stop the task, and schedule on the UI thread
                //the working theory is that the reporting code should be scudeuled and therefore occur before the UI thread begins work
                Application.Current.Dispatcher.Invoke(() =>
                {
                    //initialize the categories lists
                    InitDatabaseUI(ParsedCategoryList);
                    //link everything again now that the category exists
                    Utils.BuildLinksRefrence(ParsedCategoryList, false);
                    //initialize the user mods
                    InitUsermods();
                });

                //for each category, report category progres then schedule to load it
                loadProgress.ChildTotal = ParsedCategoryList.Count;
                foreach (Category cat in ParsedCategoryList)
                {
                    //report the progress
                    loadProgress.ChildCurrent++;
                    loadProgress.ReportMessage = string.Format("{0} {1}", Translations.GetTranslatedString("loading"), cat.Name);
                    progress.Report(loadProgress);
                    Utils.AllowUIToUpdate();

                    //then schedule the UI work
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AddPackage(cat.Packages);
                    });
                    Utils.AllowUIToUpdate();
                }

                //perform any final loading to do
                loadProgress.ReportMessage = Translations.GetTranslatedString("loadingUI");
                progress.Report(loadProgress);
                Utils.AllowUIToUpdate();

                //then schedule the UI work
                Application.Current.Dispatcher.Invoke(() =>
                {
                    //add the user mods
                    AddUserMods();

                    //update the text on the list
                    InstallingTo.Text = string.Format(Translations.GetTranslatedString("InstallingTo"), Settings.WoTDirectory);
                    InstallingAsWoTVersion.Text = string.Format(Translations.GetTranslatedString("InstallingAsWoTVersion"), Settings.WoTClientVersion);

                    //determind if the collapse and expand buttons should be visible
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

                    //process loading selections after loading UI
                    XmlDocument SelectionsDocument = null;
                    bool shouldLoadSomething = false;
                    bool loadSuccess = false;
                    if (ModpackSettings.AutoInstall || ModpackSettings.OneClickInstall)
                    {
                        //check that the file exists before trying to load it
                        if(File.Exists(ModpackSettings.AutoOneclickSelectionFilePath))
                        {
                            //load the custom selection file
                            SelectionsDocument = XmlUtils.LoadXmlDocument(ModpackSettings.AutoOneclickSelectionFilePath, XmlLoadType.FromFile);
                            shouldLoadSomething = true;
                        }
                        else
                        {
                            Logging.Warning("AutoInstall or OneClickInstall is true, but the file selection path does not exist");
                            Logging.Warning(ModpackSettings.AutoOneclickSelectionFilePath);
                            MessageBox.Show(Translations.GetTranslatedString("configLoadFailed"));
                        }
                    }
                    //else check and load the use selection from auto launch command line
                    else if (!string.IsNullOrEmpty(CommandLineSettings.AutoInstallFileName))
                    {
                        SelectionsDocument = XmlUtils.LoadXmlDocument(Path.Combine(Settings.RelhaxUserSelectionsFolderPath, CommandLineSettings.AutoInstallFileName), XmlLoadType.FromFile);
                        shouldLoadSomething = true;
                    }
                    else if (ModpackSettings.SaveLastSelection)
                    {
                        if (!File.Exists(Settings.LastInstalledConfigFilepath))
                        {
                            Logging.Warning("LastInstalledConfigFile does not exist, loading as first time with check default mods");
                            SelectionsDocument = XmlUtils.LoadXmlDocument(Utils.GetStringFromZip(Settings.ManagerInfoZipfile, Settings.DefaultCheckedSelectionfile), XmlLoadType.FromString);
                            shouldLoadSomething = true;
                        }
                        else
                        {
                            SelectionsDocument = XmlUtils.LoadXmlDocument(Settings.LastInstalledConfigFilepath, XmlLoadType.FromFile);
                            shouldLoadSomething = true;
                        }
                    }
                    else
                    {
                        //load default checked mods
                        SelectionsDocument = XmlUtils.LoadXmlDocument(Utils.GetStringFromZip(Settings.ManagerInfoZipfile, Settings.DefaultCheckedSelectionfile), XmlLoadType.FromString);
                        shouldLoadSomething = true;
                    }

                    //check if errors and if should load something
                    if(shouldLoadSomething)
                    {
                        if(SelectionsDocument != null)
                        {
                            loadSuccess = LoadSelection(SelectionsDocument, true);
                        }
                        else
                        {
                            Logging.Error("Failed to load SelectionsDocument, AutoInstall={0}, OneClickInstall={1}, DatabaseDistro={2}, SaveSelection={3}",
                            ModpackSettings.AutoInstall, ModpackSettings.OneClickInstall, databaseVersion, ModpackSettings.SaveLastSelection);
                            Logging.Error("Failed to load SelectionsDocument, AutoSelectionFilePath={0}", ModpackSettings.AutoOneclickSelectionFilePath);
                        }
                    }

                    //like hook up the flashing timer
                    FlashTimer.Tick += OnFlastTimerTick;

                    //set the selection window width, height
                    Width = ModpackSettings.ModSelectionWidth;
                    Height = ModpackSettings.ModSelectionHeight;

                    //close the loading window and show this one
                    loadingProgress.Close();
                    loadingProgress = null;

                    //set the loading flag back to false
                    LoadingUI = false;

                    //set the UI color to null so it's grabbed first time
                    UISettings.NotSelectedTabColor = null;

                    //set tabs UI coloring
                    ModTabGroups_SelectionChanged(null, null);

                    //if auto install or one-click install, don't show the UI
                    if (ModpackSettings.AutoInstall || ModpackSettings.OneClickInstall || !string.IsNullOrEmpty(CommandLineSettings.AutoInstallFileName))
                    {
                        OnSelectionListReturn(this, new SelectionListEventArgs()
                        {
                            ContinueInstallation = loadSuccess,
                            ParsedCategoryList = ParsedCategoryList,
                            Dependencies = Dependencies,
                            GlobalDependencies = GlobalDependencies,
                            UserMods = UserCategory.Packages
                        });
                    }
                    else
                    {
                        //show the UI for selection list, and if should be fullscreen or not
                        Show();
                        WindowState = ModpackSettings.ModSelectionFullscreen ? WindowState.Maximized : WindowState.Normal;
                    }
                });
                return true;
            }).ContinueWith((t) =>
            {
                //https://stackoverflow.com/questions/32067034/how-to-handle-task-run-exception
                if(t.IsFaulted)
                {
                    Logging.Exception(t.Exception.ToString());
                    MessageBox.Show(Translations.GetTranslatedString("failedToLoadSelectionList"),
                        Translations.GetTranslatedString("critical"));
                    loadingProgress.Close();
                    loadingProgress = null;
                    this.Close();
                }
            });
        }

        private void InitUsermods()
        {
            //get a list of all zip files in the folder
            string[] zipFilesUserMods = Utils.DirectorySearch(Settings.RelhaxUserModsFolderPath, SearchOption.TopDirectoryOnly, false, @"*.zip", 5, 3, true);

            //init database components
            UserCategory = new Category();
            foreach (string s in zipFilesUserMods)
            {
                SelectablePackage sp = new SelectablePackage
                {
                    ZipFile = s,
                    Name = Path.GetFileNameWithoutExtension(s),
                    Enabled = true,
                    Level = 0,
                    PatchGroup = 9,
                    InstallGroup = 9,
                    DownloadFlag = false
                };
                //circular reference because
                sp.Parent = sp.TopParent = sp;
                UserCategory.Packages.Add(sp);
            }
        }

        private void AddUserMods()
        {
            StackPanel userStackPanel = new StackPanel();
            TabItem userTab = new TabItem()
            {
                Name = "UserMods",
                Header = Translations.GetTranslatedString("userMods"),
                Style = (Style)Application.Current.Resources["RelhaxSelectionListTabItemStyle"]
            };
            userTab.Resources.Add("TabItemHeaderSelectedBackground", UISettings.CurrentTheme.SelectionListActiveTabHeaderBackgroundColor.Brush);
            userTab.RequestBringIntoView += OnUserModsTabSelected;
            userTab.Content = userStackPanel;
            ModTabGroups.Items.Add(userTab);
            UserCategory.TabPage = userTab;

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
                    Content = package.NameDisplay,
                    PopularModVisability = Visibility.Hidden,
                    GreyAreaVisability = Visibility.Hidden,
                    Foreground = UISettings.CurrentTheme.SelectionListNotSelectedTextColor.Brush
                };
                package.UIComponent = userMod;
                userMod.Click += OnUserPackageClick;
                userStackPanel.Children.Add(userMod);
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
                //build per cateogry tab here
                //like all the UI stuff and linking internally
                //make the tab page
                cat.TabPage = new TabItem()
                {
                    Header = cat.Name,
                    //HorizontalAlignment = HorizontalAlignment.Left,
                    //VerticalAlignment = VerticalAlignment.Center,
                    //MinWidth = 50,
                    //MaxWidth = 150,
                    //Width = 0
                    Tag = cat,
                    Style = (Style)Application.Current.Resources["RelhaxSelectionListTabItemStyle"]
                };

                //add brush resource
                cat.TabPage.Resources.Add("TabItemHeaderSelectedBackground", UISettings.CurrentTheme.SelectionListActiveTabHeaderBackgroundColor.Brush);

                //make and attach the category header
                cat.CategoryHeader = new SelectablePackage()
                {
                    Name = string.Format("----------[{0}]----------", cat.Name),
                    TabIndex = cat.TabPage,
                    ParentCategory = cat,
                    Type = SelectionTypes.multi,
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
                        cat.CategoryHeader.TreeViewItem = new StretchingTreeViewItem()
                        {
                            Background = System.Windows.Media.Brushes.Transparent,
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                            HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch,
                            IsExpanded = true
                        };
                        cat.CategoryHeader.RelhaxWPFComboBoxList = new RelhaxWPFComboBox[2];
                        cat.CategoryHeader.TreeView = new StretchingTreeView()
                        {
                            Background = Brushes.Transparent,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            HorizontalContentAlignment = HorizontalAlignment.Stretch
                        };
                        cat.CategoryHeader.TreeView.MouseDown += Lsl_MouseDown;
                        cat.CategoryHeader.ChildStackPanel = new StackPanel();
                        cat.CategoryHeader.ChildBorder = new Border()
                        {
                            BorderBrush = UISettings.CurrentTheme.SelectionListBorderColor.Brush,
                            BorderThickness = ModpackSettings.EnableBordersLegacyView? new Thickness(1) : new Thickness(0),
                            Child = cat.CategoryHeader.ChildStackPanel,
                            Margin = new Thickness(-25, 0, 0, 0),
                            Background = UISettings.CurrentTheme.SelectionListNotSelectedPanelColor.Brush
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
                            Package = cat.CategoryHeader,
                            Content = cat.CategoryHeader.NameFormatted,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Foreground = UISettings.CurrentTheme.SelectionListNotSelectedTextColor.Brush,
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
                        cat.CategoryHeader.RelhaxWPFComboBoxList = new RelhaxWPFComboBox[2];
                        cat.CategoryHeader.ContentControl = new ContentControl();
                        cat.CategoryHeader.ParentStackPanel = new StackPanel();
                        cat.CategoryHeader.ParentBorder = new Border()
                        {
                            Child = cat.CategoryHeader.ParentStackPanel,
                            Padding = new Thickness(2),
                            Background = UISettings.CurrentTheme.SelectionListNotSelectedPanelColor.Brush,
                        };
                        cat.CategoryHeader.ScrollViewer = new ScrollViewer()
                        {
                            Content = cat.CategoryHeader.ParentBorder
                        };
                        //tab page -> scrollViewer -> Border -> stackPanel
                        cat.TabPage.Content = cat.CategoryHeader.ScrollViewer;
                        //create checkbox for inside selecteionlist
                        RelhaxWPFCheckBox cb2 = new RelhaxWPFCheckBox()
                        {
                            Package = cat.CategoryHeader,
                            Content = cat.CategoryHeader.NameFormatted,
                            Foreground = UISettings.CurrentTheme.SelectionListNotSelectedTextColor.Brush,
                            HorizontalAlignment = HorizontalAlignment.Left
                        };
                        cb2.Click += OnWPFComponentCheck;
                        //set it's parent and top parent to itself
                        cat.CategoryHeader.UIComponent = cat.CategoryHeader.ParentUIComponent = cat.CategoryHeader.TopParentUIComponent = cb2;
                        //create and link the child borderand stackpanel
                        cat.CategoryHeader.ChildStackPanel = new StackPanel();
                        cat.CategoryHeader.ChildBorder = new Border()
                        {
                            BorderBrush = UISettings.CurrentTheme.SelectionListBorderColor.Brush,
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

                //init some required UI components for all selectablePackages inside it
                foreach(SelectablePackage package in cat.GetFlatPackageList())
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
                                Background = System.Windows.Media.Brushes.Transparent,
                                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch
                            };
                            break;
                    }
                }
            }
        }

        private void AddPackage(List<SelectablePackage> packages)
        {
            foreach(SelectablePackage package in packages)
            {
                //link the parent panels and border to childs
                package.ParentBorder = package.Parent.ChildBorder;
                package.ParentStackPanel = package.Parent.ChildStackPanel;

                //check if we actually want to add it. if the program isn't forcing them to be enabled
                //and the mod reports being disabled, then don't add it to the UI
                //the counter needs to still be kept up to date with the list (the whole list includes invisible mods!)
                if (!ModpackSettings.ForceVisible && !package.Visible)
                    continue;
                //ok now actuallt load the UI stuff
                //parse command line stuff. if we're forcinfg it to be enabled or visable
                if (ModpackSettings.ForceVisible && !package.IsStructureVisible)
                    package.Visible = true;
                if (ModpackSettings.ForceEnabled && !package.IsStructureEnabled)
                    package.Enabled = true;
                
                //special code for the borders and stackpanels
                //if the child container for sub options hsa yet to be made AND there are sub options, make it
                if(package.ChildBorder == null && package.Packages.Count > 0)
                {
                    package.ChildStackPanel = new StackPanel();
                    package.ChildBorder = new Border()
                    {
                        BorderBrush = BorderBrush = UISettings.CurrentTheme.SelectionListBorderColor.Brush,
                        BorderThickness = ModpackSettings.EnableBordersDefaultV2View ? new Thickness(1) : new Thickness(0),
                        Child = package.ChildStackPanel,
                        Background = UISettings.CurrentTheme.SelectionListNotSelectedPanelColor.Brush
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
                    case SelectionTypes.single1:
                        package.UIComponent = new RelhaxWPFRadioButton()
                        {
                            ToolTip = package.ToolTipString,
                            Package = package,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            HorizontalContentAlignment = HorizontalAlignment.Left,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            Content = package.NameDisplay,
                            IsEnabled = package.IsStructureEnabled,
                            PopularModVisability = package.PopularMod? Visibility.Visible : Visibility.Hidden,
                            GreyAreaVisability = package.GreyAreaMod? Visibility.Visible : Visibility.Hidden,
                            Foreground = BorderBrush = UISettings.CurrentTheme.SelectionListNotSelectedTextColor.Brush,
                            IsChecked = false
                        };
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
                            ToolTip = package.ToolTipString,
                            Package = package,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            HorizontalContentAlignment = HorizontalAlignment.Left,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            Content = package.NameDisplay,
                            IsEnabled = package.IsStructureEnabled,
                            IsChecked = false,
                            PopularModVisability = package.PopularMod ? Visibility.Visible : Visibility.Hidden,
                            GreyAreaVisability = package.GreyAreaMod ? Visibility.Visible : Visibility.Hidden,
                            Foreground = BorderBrush = UISettings.CurrentTheme.SelectionListNotSelectedTextColor.Brush,
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
                //howerver
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
                    //FONT?
                    MinWidth = 100,
                    MaxWidth = 420,//yes, really
                    HorizontalAlignment = HorizontalAlignment.Left,
                    AddedToList = false
                };
            }
            ComboBoxItem cbi = new ComboBoxItem(package, package.NameDisplay)
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
                package.Parent.RelhaxWPFComboBoxList[boxIndex].SelectionChanged += OnSingleDDPackageClick;
                package.Parent.RelhaxWPFComboBoxList[boxIndex].Handler = OnSingleDDPackageClick;
                package.Parent.RelhaxWPFComboBoxList[boxIndex].DropDownClosed += DropDownSelectSelfFix;
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
        //generic handler to disable the auto check like in forms, but for WPF
        void OnWPFComponentCheck(object sender, RoutedEventArgs e)
        {
            if (LoadingUI)
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
        //special fix for when the combobox is showing selectedIndex item 0 (first item)
        //(AND it's not actually clicked, just showing it), and user selects that one
        //this takes care of event not previously firing to select it in memory
        //https://stackoverflow.com/questions/25763954/event-when-combobox-is-selected
        private void DropDownSelectSelfFix(object sender, EventArgs e)
        {
            if (LoadingUI)
                return;
            SelectablePackage spc = null;
            if (sender is RelhaxWPFComboBox cb2)
            {
                //get the UI cbi struct and the internal SeletablePackage
                ComboBoxItem cbi = (ComboBoxItem)cb2.SelectedItem;
                spc = cbi.Package;
                //only enable the package if the structure leading to this package is enabled
                if(cb2.SelectedIndex == 0 && spc.IsStructureEnabled && !spc.Checked)
                {
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
                    PropagateChecked(spc, SelectionPropagationDirection.PropagateUp);
                }
            }
        }

        //when a single/single1 mod is selected
        void OnSinglePackageClick(object sender, EventArgs e)
        {
            if (LoadingUI)
                return;

            IPackageUIComponent ipc = (IPackageUIComponent)sender;
            SelectablePackage spc = ipc.Package;

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
        void OnSingleDDPackageClick(object sender, EventArgs e)
        {
            if (LoadingUI)
                return;

            SelectablePackage spc = null;

            if (sender is RelhaxWPFComboBox cb2)
            {
                //don't change the selection if the user did not want to change the option
                if (!cb2.IsDropDownOpen)
                    return;
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
            PropagateChecked(spc, SelectionPropagationDirection.PropagateUp);
        }

        void OnUserPackageClick(object sender, EventArgs e)
        {
            if (LoadingUI)
                return;

            IPackageUIComponent ipc = (IPackageUIComponent)sender;
            SelectablePackage spc = ipc.Package;

            if (!spc.Checked)
                spc.Checked = true;
            else
                spc.Checked = false;
        }

        //when a multi mod is selected
        void OnMultiPackageClick(object sender, EventArgs e)
        {
            if (LoadingUI)
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
                //if it's a user check box end here
                //DISABLED FOR NOW
                //if (ipc is RelhaxUserCheckBox)
                   // return;

                //down
                PropagateChecked(spc, SelectionPropagationDirection.PropagateDown);
                //up
                PropagateChecked(spc, SelectionPropagationDirection.PropagateUp);
            }
            else if (spc.Checked)
            {
                //uncheck it and propagate change
                spc.Checked = false;

                //if (ipc is RelhaxUserCheckBox)
                   // return;
                PropagateDownNotChecked(spc);
            }
        }

        //propagates the change back up the selection tree
        //can be sent from any component
        //true = up, false = down
        void PropagateChecked(SelectablePackage spc, SelectionPropagationDirection direction)
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
        void PropagateUpNotChecked(SelectablePackage spc)
        {
            if (spc.Level == -1)
                return;
            //if nothing checked at this level, uncheck the parent and propagate up not checked again
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

        private void ModTabGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LoadingUI)
                return;
            if (ParsedCategoryList == null)
                return;

            List<Category> listWithUserCat = new List<Category>();
            listWithUserCat.AddRange(ParsedCategoryList);
            listWithUserCat.Add(UserCategory);

            foreach (Category category in listWithUserCat)
            {
                TabItem TabIndex = category.TabPage;
                //if the color is not saved yet, then save what the default currently is
                if (UISettings.NotSelectedTabColor == null)
                {
                    //windows 10 uses a linear gradient brush (at least mine does)
                    //windows 7 in classic theme uses a solid color brush
                    if (UISettings.CurrentTheme.Equals(Themes.Default))
                    {
                        UISettings.NotSelectedTabColor = TabIndex.Background;
                    }
                    else
                    {
                        UISettings.NotSelectedTabColor = UISettings.CurrentTheme.SelectionListNotActiveHasNoSelectionsBackgroundColor.Brush;
                    }
                }

                //3 possible conditions:
                // if (active){ }
                // else
                // {
                //  if (has selections) { }
                //  else{ }
                // }

                if (TabIndex.IsSelected)
                {
                    //brush is set in tab resources when created as trigger
                    TabIndex.Foreground = UISettings.CurrentTheme.SelectionListActiveTabHeaderTextColor.Brush;
                }
                else
                {
                    if (category.AnyPackagesChecked())
                    {
                        TabIndex.Background = UISettings.CurrentTheme.SelectionListNotActiveHasSelectionsBackgroundColor.Brush;
                        TabIndex.Foreground = UISettings.CurrentTheme.SelectionListNotActiveHasSelectionsTextColor.Brush;
                    }
                    else
                    {
                        TabIndex.Background = UISettings.NotSelectedTabColor;
                        TabIndex.Foreground = UISettings.CurrentTheme.SelectionListNotActiveHasNoSelectionsTextColor.Brush;
                    }
                }
            }
        }
        #endregion

        #region Preview Code
        //generic hander for when any mouse button is clicked for MouseDown Events
        void Generic_MouseDown(object sender, EventArgs e)
        {
            if (LoadingUI)
                return;

            if (e is MouseEventArgs m)
                if (m.RightButton != MouseButtonState.Pressed)
                    return;

            SelectablePackage spc = null;
            if (sender is IPackageUIComponent packageSender)
            {
                spc = packageSender.Package;
            }
            else if (sender is RelhaxWPFComboBox comboboxSender)
            {
                //check to see if a specific item is highlighted
                //if so, it means that the user wants to preview a specific version
                //if not, then the user clicked on the combobox as a whole, so show all items in the box
                foreach (ComboBoxItem itemInBox in comboboxSender.Items)
                {
                    if (itemInBox.IsHighlighted && itemInBox.IsEnabled)
                    {
                        spc = itemInBox.Package;
                        break;
                    }
                }
                if (spc == null)
                {
                    //make a new temporary package with a custom preview items list
                    //get a temp known good package, doesn't matter what cause we want the parent
                    ComboBoxItem cbi = (ComboBoxItem)comboboxSender.Items[0];
                    //parent of item in combobox is header
                    SelectablePackage parentPackage = cbi.Package.Parent;
                    spc = new SelectablePackage()
                    {
                        PackageName = parentPackage.PackageName,
                        Name = string.Format("{0}: {1}", Translations.GetTranslatedString("dropDownItemsInside"), parentPackage.NameFormatted),
                        Version = parentPackage.Version,
                        Description = parentPackage.Description,
                        UpdateComment = parentPackage.UpdateComment
                    };
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

            if (spc.DevURL == null)
                spc.DevURL = string.Empty;

            if (p != null)
            {
                p.Close();
                p = null;
            }

            p = new Preview()
            {
                Package = spc
            };
            p.Show();
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
                    Generic_MouseDown(pkg, null);
                }
            }
        }
        #endregion

        #region Selection stuff
        private void OnSaveSelectionClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog selectSavePath = new SaveFileDialog()
            {
                InitialDirectory = Settings.RelhaxUserSelectionsFolderPath,
                AddExtension = true,
                Filter = "XML files|*.xml",
                ValidateNames = true,
                Title = Translations.GetTranslatedString("SelectSelectionFileToSave")
            };
            if((bool)selectSavePath.ShowDialog())
                SaveSelection(selectSavePath.FileName,false);
        }

        private void OnLoadSelectionClick(object sender, RoutedEventArgs e)
        {
            DeveloperSelectionsViewer selections = new DeveloperSelectionsViewer() { };
            selections.OnDeveloperSelectionsClosed += OnDeveloperSelectionsExit;
            selections.ShowDialog();
        }

        private async void OnDeveloperSelectionsExit(object sender, DevleoperSelectionsClosedEWventArgs e)
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
                    InitialDirectory = Settings.RelhaxUserSelectionsFolderPath,
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
                    LoadSelection(doc,false);
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
                        xmlString = await client.DownloadStringTaskAsync(Settings.SelectionsRoot + e.FileToLoad);
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
                LoadSelection(doc,false);
            }
        }

        private void OnClearSelectionsClick(object sender, RoutedEventArgs e)
        {
            Logging.Info("Clearing selections");
            //clear in lists
            Utils.ClearSelections(ParsedCategoryList);
            Utils.ClearSelections(new List<Category>() { UserCategory});
            //update selection list UI
            ModTabGroups_SelectionChanged(null, null);
            Logging.Info("Selections cleared");
            MessageBox.Show(Translations.GetTranslatedString("selectionsCleared"));
        }

        private bool LoadSelection(XmlDocument document, bool silent)
        {
            //get the string version of the document, determine what to do from there
            string selectionVersion;
            //attribute example: "//root/element/@attribute"
            selectionVersion = XmlUtils.GetXmlStringFromXPath(document, "//mods/@ver");
            Logging.Debug("SelectionVersion={0}", selectionVersion);
            switch(selectionVersion)
            {
                case "2.0":
                    return LoadSelectionV2(document, silent);

                default:
                    //log we don't know wtf it is
                    Logging.Warning("Unknown selection version: " + selectionVersion + ", aborting");
                    if(!silent)
                        MessageBox.Show(string.Format(Translations.GetTranslatedString("unknownselectionFileFormat"),selectionVersion));
                    return false;
            }
        }

        private bool LoadSelectionV2(XmlDocument document, bool silent)
        {
            //first uncheck everyting
            Utils.ClearSelections(ParsedCategoryList);

            //get a list of all the mods currently in the selection
            XmlNodeList xmlSelections = document.SelectNodes("//mods/relhaxMods/mod");
            XmlNodeList xmluserSelections = document.SelectNodes("//mods/userMods/mod");

            //logging
            Logging.Debug("xmlSelections count: {0}", xmlSelections.Count);
            Logging.Debug("xmluserSelections count: {0}", xmluserSelections.Count);

            //save a list string of all the packagenames in the list for later
            List<string> stringSelections = new List<string>();
            List<string> stringUserSelections = new List<string>();
            List<string> disabledMods = new List<string>();
            List<string> disabledStructureMods = new List<string>();

            foreach(XmlNode node in xmlSelections)
                stringSelections.Add(node.InnerText);
            foreach(XmlNode node in xmluserSelections)
                stringUserSelections.Add(node.InnerText);

            //check the mods in the actual list if it's in the list
            foreach(SelectablePackage package in Utils.GetFlatList(null,null,null,ParsedCategoryList))
            {
                //also check to only "check" the mod if it is visible OR if the command line settings to force visiable all compoents
                if(stringSelections.Contains(package.PackageName) && (package.Visible || ModpackSettings.ForceVisible))
                {
                    stringSelections.Remove(package.PackageName);
                    //also check if the mod only if it's enabled OR is command line settings force enabled
                    if(package.Enabled || ModpackSettings.ForceEnabled)
                    {
                        package.Checked = true;
                        Logging.Info(string.Format("Checking package {0}",package.CompletePath));
                    }
                    else
                    {
                        if(ModpackSettings.SaveDisabledMods)
                        {
                            Logging.Debug("SaveDisabledMods=True, flagging disabled mod {0} for future selection later",package.Name);
                            package.FlagForSelectionSave = true;
                        }
                        disabledMods.Add(package.CompletePath);
                        Logging.Info(string.Format("\"{0}\" is a disabled mod", package.CompletePath));
                    }
                    //if it's the top level, check the category header
                    if (package.Level == 0 && !package.ParentCategory.CategoryHeader.Checked)
                    {
                        package.ParentCategory.CategoryHeader.Checked = true;
                        Logging.Info("Checking top header " + package.ParentCategory.CategoryHeader.NameFormatted);
                    }
                }
            }
            //do the same as above but for user mods
            foreach(SelectablePackage package in UserCategory.Packages)
            {
                if(stringUserSelections.Contains(Path.GetFileNameWithoutExtension(package.ZipFile)) && File.Exists(Path.Combine(Settings.RelhaxUserModsFolderPath,package.ZipFile)))
                {
                    Logging.Info(string.Format("Checking User Mod {0}",package.ZipFile));
                    package.Enabled = true;
                    package.Checked = true;
                    stringUserSelections.Remove(Path.GetFileNameWithoutExtension(package.ZipFile));
                }
            }
            //now check for the correct structure of mods
            List<SelectablePackage> brokenMods = IsValidStructure(ParsedCategoryList);
            Logging.Info("Broken mods structure count: " + brokenMods.Count);

            //
            int totalBrokenCount = disabledMods.Count + brokenMods.Count + stringSelections.Count + stringUserSelections.Count;
            if (totalBrokenCount > 0 && (ModpackSettings.AutoInstall || ModpackSettings.OneClickInstall) && ModpackSettings.AutoOneclickShowWarningOnSelectionsFail)
            {
                Logging.Info("Selection issues with auto or one click enabled, with message warning enabled. Show message.");
                MessageBoxResult  result = MessageBox.Show(
                    Translations.GetTranslatedString("AutoOneclickSelectionErrorsContinueHeader"),
                    Translations.GetTranslatedString("AutoOneclickSelectionErrorsContinueBody"), MessageBoxButton.YesNo);
                if(result == MessageBoxResult.No)
                {
                    Logging.Info("User selected stop installation");
                    return false;
                }
            }

            //only report issues if silent is false. true means its doing something like auto selections or
            else if(!silent)
            {
                Logging.Info("Informing user of {0} disabled selections, {1} broken selections, {2} removed selections, {3} removed user selections",
                disabledMods.Count, brokenMods.Count, stringSelections.Count, stringUserSelections.Count);
                if(disabledMods.Count > 0)
                {
                    //disabled selections
                    MessageBox.Show(string.Format("{0}: {1}{2}",
                        Translations.GetTranslatedString("modDeactivated"), Environment.NewLine, string.Join(Environment.NewLine, disabledMods)));
                }
                if(stringSelections.Count > 0)
                {
                    //removed selections
                    MessageBox.Show(string.Format("{0}: {1}{2}",
                        Translations.GetTranslatedString("modsNotFoundTechnical"), Environment.NewLine, string.Join(Environment.NewLine, stringSelections)));
                }
                if(stringUserSelections.Count > 0)
                {
                    //removed user selections
                    MessageBox.Show(string.Format("{0}: {1}{2}",
                        Translations.GetTranslatedString("modsNotFoundTechnical"), Environment.NewLine, string.Join(Environment.NewLine, stringUserSelections)));
                }
                if(disabledStructureMods.Count > 0)
                {
                    //removed structure user selections
                    MessageBox.Show(string.Format("{0}: {1}{2}",
                        Translations.GetTranslatedString("modsBrokenStructure"), Environment.NewLine, string.Join(Environment.NewLine, disabledStructureMods)));
                }
            }
            return true;
        }

        private void SaveSelection(string savePath, bool silent)
        {
            Logging.Info("Saving selections to " + savePath);
            //create saved config xml layout
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("mods", new XAttribute("ver", Settings.ConfigFileVersion), new XAttribute("date",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), new XAttribute("timezone", TimeZoneInfo.Local.DisplayName),
                new XAttribute("dbVersion", Settings.DatabaseVersion), new XAttribute("dbDistro", databaseVersion.ToString())));

            //relhax mods root
            doc.Element("mods").Add(new XElement("relhaxMods"));
            //user mods root
            doc.Element("mods").Add(new XElement("userMods"));

            //do some cool xml stuff grumpel does
            var nodeRelhax = doc.Descendants("relhaxMods").FirstOrDefault();
            var nodeUserMods = doc.Descendants("userMods").FirstOrDefault();

            //check relhax Mods
            foreach (SelectablePackage package in Utils.GetFlatList(null,null,null,ParsedCategoryList))
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
                    Logging.Info("adding user mod" + m.ZipFile);
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
        //checks for invalid structure in the selected packages
        //ex: a new mandatory option was added to a mod, but the user does not have it selected
        private List<SelectablePackage> IsValidStructure(List<Category> ParsedCategoryList)
        {
            List<SelectablePackage>  brokenPackages = new List<SelectablePackage>();
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

        private void SearchCB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //https://stackoverflow.com/questions/17250650/wpf-combobox-auto-highlighting-on-first-letter-input
            SearchCB.IsDropDownOpen = true;
            //https://stackoverflow.com/questions/17250650/wpf-combobox-auto-highlighting-on-first-letter-input
            /*
            var textbox = (TextBox)SearchCB.Template.FindName("PART_EditableTextBox", SearchCB);
            if (textbox != null && textbox.SelectionLength > 0)
            {
                textbox.Select(textbox.SelectionLength, 0);
            }
            */
        }

        private void SearchCB_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up)
            {
                //stop the selection from key events!!!
                //https://www.codeproject.com/questions/183259/how-to-prevent-selecteditem-change-on-up-and-down (second answer)
                e.Handled = true;
                SearchCB.IsDropDownOpen = true;
            }
            else if(e.Key == Key.Enter)
            {
                if(SearchCB.SelectedItem == null)
                {
                    Logging.Info("enter key pressed for search, but no actual package selected. ignoring");
                    return;
                }
                OnSearchCBSelectionCommitted(SearchCB.SelectedItem as ComboBoxItem);
            }
            //check if length 0 or whitespace
            else if (string.IsNullOrWhiteSpace(SearchCB.Text))
            {
                SearchCB.Items.Clear();
                SearchCB.IsDropDownOpen = false;
                SearchCB.SelectedIndex = -1;
            }
            //check if length 1
            /*
            else if(SearchCB.Text.Count() == 1)
            {

            }
            */
            //actually search
            else
            {
                //split the search into an array based on using '*' search
                List<SelectablePackage> searchComponents = new List<SelectablePackage>();
                foreach (string searchTerm in SearchCB.Text.Split('*'))
                {
                    //check if comparing with this tab only
                    if((bool)SearchThisTabOnlyCB.IsChecked)
                    {
                        TabItem selected = (TabItem)ModTabGroups.SelectedItem;
                        searchComponents.AddRange(Utils.GetFlatSelectablePackageList(ParsedCategoryList).Where(
                            term => term.NameFormatted.ToLower().Contains(searchTerm.ToLower()) && term.Visible && term.ParentCategory.TabPage.Equals(selected)));
                    }
                    else
                    {
                        //get a list of components that match the search term
                        searchComponents.AddRange(Utils.GetFlatSelectablePackageList(ParsedCategoryList).Where(
                            term => term.NameFormatted.ToLower().Contains(searchTerm.ToLower()) && term.Visible));
                    }
                }
                //assuming it maintains the order it previously had i.e. removing only when need to...
                searchComponents = searchComponents.Distinct().ToList();
                //clear and fill the search list again
                SearchCB.Items.Clear();
                foreach (SelectablePackage package in searchComponents)
                {
                    string formatForText = string.Format("{0} [{1}]", package.NameFormatted, package.ParentCategory.Name);
                    SearchCB.Items.Add(new ComboBoxItem(package, formatForText)
                    {
                        IsEnabled = true,
                        Content = formatForText
                    });
                }
                SearchCB.IsDropDownOpen = true;
            }
        }

        private async void OnSearchCBSelectionCommitted(ComboBoxItem committedItem)
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
            OnFlastTimerTick(null, null);
            FlashTimer.Start();
        }

        private void SearchCB_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SearchCB.IsDropDownOpen)
            {
                foreach (ComboBoxItem item in SearchCB.Items)
                {
                    if (item.IsHighlighted && item.IsMouseOver)
                    {
                        OnSearchCBSelectionCommitted(item);
                    }
                }
            }
        }
        #endregion

        #region MD5 hash code
        private string GetMD5Hash(string inputFile)
        {
            string hash;
            //get filetime from file, convert it to string with base 10
            string filetime = Convert.ToString(File.GetLastWriteTime(inputFile).ToFileTime(), 10);
            //extract filename with path
            string filename = Path.GetFileName(inputFile);
            //check database for filename with filetime
            hash = GetMd5HashDatabase(filename, filetime);
            if (hash == "-1")   //file not found in database
            {
                //create Md5Hash from file
                hash = Utils.CreateMD5Hash(inputFile);

                if (hash == "-1")
                {
                    //no file found, then delete from database
                    DeleteMd5HashDatabase(filename);
                }
                else
                {
                    //file found. update the database with new values
                    UpdateMd5HashDatabase(filename, hash, filetime);
                }
                //report back the created Hash
                return hash;
            }
            //Hash found in database
            else
            {
                //report back the stored Hash
                return hash;
            }
        }
        // need filename and filetime to check the database
        private string GetMd5HashDatabase(string inputFile, string inputFiletime)
        {
            bool exists = Md5HashDocument.Descendants("file")
                       .Where(arg => arg.Attribute("filename").Value.Equals(inputFile) && arg.Attribute("filetime").Value.Equals(inputFiletime))
                       .Any();
            if (exists)
            {
                XElement element = Md5HashDocument.Descendants("file")
                   .Where(arg => arg.Attribute("filename").Value.Equals(inputFile) && arg.Attribute("filetime").Value.Equals(inputFiletime))
                   .Single();
                return element.Attribute("md5").Value;
            }
            return "-1";
        }

        private void UpdateMd5HashDatabase(string inputFile, string inputMd5Hash, string inputFiletime)
        {
            bool exists = Md5HashDocument.Descendants("file")
                       .Where(arg => arg.Attribute("filename").Value.Equals(inputFile))
                       .Any();
            if (exists)
            {
                XElement element = Md5HashDocument.Descendants("file")
                   .Where(arg => arg.Attribute("filename").Value.Equals(inputFile))
                   .Single();
                element.Attribute("filetime").Value = inputFiletime;
                element.Attribute("md5").Value = inputMd5Hash;
            }
            else
            {
                Md5HashDocument.Element("database").Add(new XElement("file", new XAttribute("filename", inputFile), new XAttribute("filetime", inputFiletime), new XAttribute("md5", inputMd5Hash)));
            }
        }

        private void DeleteMd5HashDatabase(string inputFile)
        {
            // extract filename from path (if call with full path)
            string fileName = Path.GetFileName(inputFile);
            bool exists = Md5HashDocument.Descendants("file")
                       .Where(arg => arg.Attribute("filename").Value.Equals(inputFile))
                       .Any();
            if (exists)
                Md5HashDocument.Descendants("file").Where(arg => arg.Attribute("filename").Value.Equals(inputFile)).Remove();
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
