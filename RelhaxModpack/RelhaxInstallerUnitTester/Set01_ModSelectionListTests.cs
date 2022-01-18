using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RelhaxUnitTests;
using RelhaxModpack;
using RelhaxModpack.Windows;
using System.Collections;
using System.Collections.Generic;
using RelhaxModpack.Database;
using System.Threading.Tasks;
using System.Windows.Controls;
using RelhaxModpack.UI;
using System.Windows;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using RelhaxModpack.Utilities;
using RelhaxModpack.Settings;
using RelhaxModpack.Common;
using RelhaxModpack.Utilities.ClassEventArgs;
using RelhaxModpack.Utilities.Enums;
using System.Xml;
using RelhaxModpack.Xml;

namespace RelhaxInstallerUnitTester
{
    [TestClass]
    public class Set01_ModSelectionListTests
    {
        //declaring these objects as static will allow them to exist throughout the test
        //exists in all methods
        ModpackSettings modpackSettings;
        CommandLineSettings commandLineSettings;
        SelectionListEventArgs args;
        DatabaseManager databaseManager;

        //this technically applies to every test upon initialization, but it's placed here
        //https://docs.microsoft.com/en-us/previous-versions/visualstudio/visual-studio-2012/ms245572(v=vs.110)
        [AssemblyInitialize]
        public static async Task Init(TestContext context)
        {
            foreach (string logfile in UnitTestHelper.ListOfLogfilenames)
            {
                if (File.Exists(logfile))
                    File.Delete(logfile);
            }

            //init the modpack app to load resources required by the unit tested windows
            //https://stackoverflow.com/a/20834469/3128017
            //https://stackoverflow.com/a/39841167/3128017

            if (Application.Current == null)
            {
                new App { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            }

            //if the log file isn't already open, then create it
            if (!Logging.IsLogOpen(Logfiles.Application))
                //init with the default name (pass in null to get default), or if no default, the name of the enumeration and ".log"
                //throw exception if it fails to create the log file
                if (!Logging.Init(Logfiles.Application, true, false))
                    throw new BadMemeException("Failed to create a log file");

            //ensure folder structure exists
            Logging.Info("Ensuring folder structure exists");
            foreach (string folderPath in ApplicationConstants.FoldersToCheck)
            {
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
            }

            //get the managerInfo document
            ((App)RelhaxModpack.App.Current).ManagerInfoZipfile = await CommonUtils.GetManagerInfoZipfileAsync(true);

            Logging.Info("Loading translations for windows");
            Translations.LoadTranslations();
            Assert.IsTrue(Translations.TranslationsLoaded);
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            //init all logs if they aren't already init
            foreach (Logfiles logfile in UnitTestHelper.AllLogFiles)
            {
                if (!Logging.IsLogDisposed(logfile))
                    Logging.DisposeLogging(logfile);
            }

            Application.Current.Shutdown();
        }

        [TestInitialize]
        public void Init()
        {
            modpackSettings = new ModpackSettings()
            {
                DatabaseDistroVersion = DatabaseVersions.Beta,
                SaveLastSelection = true
            };

            commandLineSettings = new CommandLineSettings(null)
            {

            };

            databaseManager = new DatabaseManager(modpackSettings, commandLineSettings) { ManagerInfoZipfile = ((App)RelhaxModpack.App.Current).ManagerInfoZipfile };
        }
        
        [TestMethod]
        public async Task Test01_LoadModSelectionListTest()
        {
            PackageSelectionList selectionList = null;
            Logging.Info("Creating a ModSelectionList Window");

            //create the window and run it on its own thread and dispatcher
            //this can avoid problems with the unit test dispatcher not running the window the way it should
            //it's still an STA thread so WPF can use it as a UI thread
            //https://www.c-sharpcorner.com/uploadfile/suchit_84/creating-wpf-windows-on-dedicated-threads/
            Thread thread = new Thread(() =>
            {
                selectionList = new PackageSelectionList(modpackSettings, commandLineSettings, databaseManager)
                {
                    ApplyColorSettings = false, //not cross-thread safe
                    ApplyScaling = false,
                    ApplyToolTips = true,
                    AutoInstallMode = false,
                    LocalizeWindow = true,
                    OriginalHeight = 720.0,
                    OriginalWidth = 1280.0,

                    //WotClientVersionFromMainWindow is for UI display only
                    WotClientVersionFromMainWindow = "TESTING",

                    //WoTDirectoryFromMainWindow is for UI display only
                    WoTDirectoryFromMainWindow = "TESTING",

                    //DatabaseVersionFromMainWindow is for UI display and when saving a selection
                    DatabaseVersionFromMainWindow = "TESTING"
                };

                selectionList.Closed += (sender, e) => selectionList.Dispatcher.BeginInvokeShutdown(DispatcherPriority.ApplicationIdle);
                selectionList.WindowState = WindowState.Normal;
                selectionList.Show();

                //start the windows message pump
                Dispatcher.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            //wait for selection list to finish loading
            while (selectionList == null)
                await Task.Delay(100);
            
            while (!selectionList.LoadingUI)
                await Task.Delay(100);

            while (selectionList.LoadingUI)
                await Task.Delay(1000);

            selectionList.OnSelectionListReturn += (sender, e) => args = e;

            Logging.Info("Selecting 100 components");
            selectionList.Dispatcher.Invoke(() =>
            {
                List<SelectablePackage> flatListRandomSelection = DatabaseUtils.GetFlatSelectablePackageList(selectionList.ParsedCategoryList);
                flatListRandomSelection = flatListRandomSelection.FindAll(package => package.Enabled);
                Random random = new Random();
                for (int i = 0; i < 100; i++)
                {
                    int selectIndex = random.Next(0, flatListRandomSelection.Count);
                    SelectablePackage package = flatListRandomSelection[selectIndex];
                    Logging.Info("Index {0} selects package {1}", selectIndex, package.PackageName);
                    package.Checked = true;
                }

                //click the continue button
                List<FrameworkElement> elements = UiUtils.GetAllWindowComponentsLogical(selectionList, false);
                FrameworkElement buttonElement = elements.Find(element => element.Tag != null && element.Tag.Equals("ContinueButton"));
                Button clearSelectionsButton = buttonElement as Button;
                clearSelectionsButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            });

            //run some base tests on the return args
            Assert.IsTrue(args.ContinueInstallation);
            Assert.IsTrue(args.GlobalDependencies.Count > 0);
            Assert.IsTrue(args.Dependencies.Count > 0);
            Assert.IsTrue(args.ParsedCategoryList.Count > 0);

            //below this message is copy-modify-paste from the MainWindow's install and OnBeginInstall methods. Some of this should be moved into some sort of re-usable implementation. TODO
            //setup for install
            string wotExeFilepath = RegistryUtils.AutoFindWoTDirectoryFirst();
            string wotExeFolderpath = Path.GetDirectoryName(wotExeFilepath);

            //get version folders to install as
            string versionXml = Path.Combine(wotExeFolderpath, ApplicationConstants.WoTVersionXml);
            string versionTemp = XmlUtils.GetXmlStringFromXPath(versionXml, ApplicationConstants.WoTVersionXmlXpath);
            string WoTClientVersion = versionTemp.Split('#')[0].Trim().Substring(2).Trim();
            Logging.Info("Detected client version: {0}", WoTClientVersion);

            //build macro hash for install
            MacroUtils.BuildFilepathMacroList(WoTClientVersion, databaseManager.WoTOnlineFolderVersion, wotExeFolderpath);

            //perform dependency calculations
            List<Dependency> dependneciesToInstall = new List<Dependency>(DatabaseUtils.CalculateDependencies(selectionList.Dependencies, selectionList.ParsedCategoryList, false, false));

            //create install list
            List<DatabasePackage> packagesToInstall = DatabaseUtils.CreateListOfPackagesWithZipFilesToInstall(selectionList.GlobalDependencies, selectionList.Dependencies, selectionList.ParsedCategoryList);

            //user mod calculation
            List<SelectablePackage> userModsToInstall = args.UserMods.FindAll(mod => mod.Checked);

            //while we're at it let's make a list of packages that need to be downloaded
            List<DatabasePackage> packagesToDownload = packagesToInstall.FindAll(pack => pack.DownloadFlag);

            //and check if we need to actually install anything
            List<SelectablePackage> selectablePackagesToInstall = DatabaseUtils.CreateListOfSelectablePackagesToInstall(selectionList.ParsedCategoryList);
            if (selectablePackagesToInstall.Count == 0 && userModsToInstall.Count == 0)
            {
                Assert.Fail("No packages to install");
            }

            //perform list install order calculations
            List<DatabasePackage>[] orderedPackagesToInstall = DatabaseUtils.CreateOrderedInstallList(packagesToInstall);

            //first, if we have downloads to do, then start processing them
            CancellationToken nullToken;
            if (packagesToDownload.Count > 0)
            {
                DownloadManager downloadManager = new DownloadManager()
                {
                    CancellationToken = nullToken,
                    RetryCount = 3,
                    DownloadLocationBase = ApplicationConstants.RelhaxDownloadsFolderPath,
                    UrlBase = ApplicationConstants.DownloadMirrors[selectionList.ModpackSettings.DownloadMirror].Replace("{onlineFolder}", databaseManager.WoTOnlineFolderVersion)
                };

                Logging.Info("Download while install = false and packages to download, processing downloads with await");
                Progress<RelhaxDownloadProgress> downloadProgress = new Progress<RelhaxDownloadProgress>();
                downloadManager.Progress = downloadProgress;
                await downloadManager.DownloadPackagesAsync(packagesToDownload);
                downloadManager.Dispose();
            }
            else
                Logging.Info("No packages to download, continue");

            InstallEngine installEngine = new InstallEngine(selectionList.ModpackSettings, selectionList.CommandLineSettings)
            {
                OrderedPackagesToInstall = orderedPackagesToInstall,
                PackagesToInstallWithZipfile = packagesToInstall,
                ParsedCategoryList = args.ParsedCategoryList,
                Dependencies = args.Dependencies,
                GlobalDependencies = args.GlobalDependencies,
                UserPackagesToInstall = userModsToInstall,
                CancellationToken = nullToken,
                DownloadingPackages = (packagesToDownload.Count > 0),
                DisableTriggersForInstall = true,
                DatabaseVersion = "TESTING",
                WoTDirectory = wotExeFolderpath,
                WoTClientVersion = WoTClientVersion
            };

            Progress<RelhaxInstallerProgress> progress = new Progress<RelhaxInstallerProgress>();
            RelhaxInstallFinishedEventArgs results = await installEngine.RunInstallationAsync(progress);
            Logging.Debug("Installation has finished");
            Assert.IsTrue(results.ExitCode == InstallerExitCodes.Success);
            installEngine.Dispose();
            installEngine = null;
        }
    }
}
