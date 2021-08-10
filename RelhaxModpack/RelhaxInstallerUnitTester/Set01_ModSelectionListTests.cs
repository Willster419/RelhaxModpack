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

namespace RelhaxInstallerUnitTester
{
    [TestClass]
    public class Set01_ModSelectionListTests
    {
        //declaring these objects as static will allow them to exist throughout the test
        //exists in all methods
        private static PackageSelectionList SelectionList = null;
        private static List<DatabasePackage> GlobalDependencies = null;
        private static List<Dependency> Dependencies = null;
        private static List<Category> ParsedCategoryList = null;
        private static Logfile log = null;
        private static App app = null;
        private static ModpackSettings ModpackSettings = new ModpackSettings();
        private static SettingsParser SettingsParser = new SettingsParser();

        //this technically applies to every test upon initialization, but it's placed here
        //https://docs.microsoft.com/en-us/previous-versions/visualstudio/visual-studio-2012/ms245572(v=vs.110)
        [AssemblyInitialize]
        public static void SetupLogging(TestContext context)
        {
            foreach (string logfile in UnitTestHelper.ListOfLogfilenames)
            {
                if (File.Exists(logfile))
                    File.Delete(logfile);
            }

            //if the log file isn't already open, then create it
            if (!Logging.IsLogOpen(Logfiles.Application))
                //init with the default name (pass in null to get default), or if no default, the name of the enumeration and ".log"
                //throw exception if it fails to create the log file
                if (!Logging.Init(Logfiles.Application, true, false))
                    throw new BadMemeException("Failed to create a log file");
        }

        [AssemblyCleanup]
        public static void CleanupLogging()
        {
            //init all logs if they aren't already init
            foreach (Logfiles logfile in UnitTestHelper.AllLogFiles)
            {
                if (!Logging.IsLogDisposed(logfile))
                    Logging.DisposeLogging(logfile);
            }
        }

        [TestMethod]
        public void Test01_LoadModpackSettingsTest()
        {
            Assert.IsTrue(File.Exists(ModpackSettings.SettingsFilename));
            SettingsParser.LoadSettings(ModpackSettings);

            GlobalDependencies = new List<DatabasePackage>();
        }

        [TestMethod]
        public async Task Test02_LoadMiscForSelectionListTest()
        {
            log.Write("Loading translations for windows");
            Translations.LoadTranslations();
            Assert.IsTrue(Translations.TranslationsLoaded);

            //set properties outside of the selection list
            log.Write("Setting all required misc settings");
            ModpackSettings.DatabaseDistroVersion = RelhaxModpack.Utilities.Enums.DatabaseVersions.Beta;
            ModpackSettings.SaveLastSelection = true;

            //ensure folder structure exists
            log.Write("Ensuring folder structure exists");
            foreach(string folderPath in ApplicationConstants.FoldersToCheck)
            {
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
            }

            //get the managerInfo document
            ((App)RelhaxModpack.App.Current).ManagerInfoZipfile = await CommonUtils.GetManagerInfoZipfileAsync(true);
        }
        /*
        [TestMethod]
        public async Task Test03_LoadModSelectionListTest()
        {
            app.InitializeComponent();
            log.Write("Creating a ModSelectionList Window");

            //create the window and run it on its own thread and dispatcher
            //this can avoid problems with the unit test dispatcher not running the window the way it should
            //it's still an STA thread so WPF can use it as a UI thread
            //https://www.c-sharpcorner.com/uploadfile/suchit_84/creating-wpf-windows-on-dedicated-threads/
            Thread thread = new Thread(() =>
            {
                SelectionList = new PackageSelectionList(ModpackSettings, null)
                {
                    ApplyColorSettings = false, //not cross-thread safe
                    ApplyScaling = false,
                    ApplyToolTips = true,
                    AutoInstallMode = false,
                    LocalizeWindow = true,
                    OriginalHeight = 720.0,
                    OriginalWidth = 1280.0,
                    LastSupportedWoTClientVersionFromMainWindow = "1.10.0.2",
                    //the lists are newed in the application
                    GlobalDependencies = Set01_ModSelectionListTests.GlobalDependencies,
                    Dependencies = Set01_ModSelectionListTests.Dependencies,
                    ParsedCategoryList = Set01_ModSelectionListTests.ParsedCategoryList,
                    WotClientVersionFromMainWindow = "TODO",
                    DatabaseVersionFromMainWindow = "TODO",
                    WoTDirectoryFromMainWindow = "TODO"
                };
                throw new BadMemeException("Finish plox");

                SelectionList.Closed += (sender, e) => SelectionList.Dispatcher.InvokeShutdown();
                SelectionList.WindowState = WindowState.Normal;
                SelectionList.Show();

                //start the windows message pump
                Dispatcher.Run();

            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            while (SelectionList == null)
                await Task.Delay(100);
            
            while (!SelectionList.LoadingUI)
                await Task.Delay(100);

            while (SelectionList.LoadingUI)
                await Task.Delay(1000);
        }
        
        [TestMethod]
        public void Test04_CreateRandomSelectionListTest()
        {
            SelectionList.OnSelectionListReturn += SelectionList_OnSelectionListReturn;

            log.Write("Selecting 100 components");
            SelectionList.Dispatcher.Invoke(() =>
            {
                List <SelectablePackage> flatList = DatabaseUtils.GetFlatSelectablePackageList(SelectionList.ParsedCategoryList);
                Random random = new Random();
                for (int i = 0; i < 100; i++)
                {
                    int selectIndex = random.Next(0, flatList.Count);
                    SelectablePackage package = flatList[selectIndex];
                    log.Write(string.Format("Index {0} selects package {1}", selectIndex, package.PackageName));
                    package.Checked = true;
                }

                //click the continue button
                List<FrameworkElement> elements = UiUtils.GetAllWindowComponentsLogical(SelectionList, false);
                FrameworkElement buttonElement = elements.Find(element => element.Tag != null && element.Tag.Equals("ContinueButton"));
                Button clearSelectionsButton = buttonElement as Button;
                clearSelectionsButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            });
        }
        */
        private void SelectionList_OnSelectionListReturn(object sender, SelectionListEventArgs e)
        {
            Assert.IsTrue(e.ContinueInstallation);
            Assert.IsTrue(e.GlobalDependencies.Count > 0);
            Assert.IsTrue(e.Dependencies.Count > 0);
            Assert.IsTrue(e.ParsedCategoryList.Count > 0);
        }

        [ClassInitialize]
        public static void InitClass(TestContext ctx)
        {
            string[] fullyQualifiedTestNameSplit = ctx.FullyQualifiedTestClassName.Split('.');
            //throw exception if it fails to create the log file
            log = new Logfile(fullyQualifiedTestNameSplit[fullyQualifiedTestNameSplit.Length-1], Logging.ApplicationLogfileTimestamp, true);

            log.Init(false);

            if (!log.CanWrite)
                throw new BadMemeException("Can't write to the logfile");

            //init the modpack app to load resources required by the unit tested windows
            //https://stackoverflow.com/a/20834469/3128017
            //https://stackoverflow.com/a/39841167/3128017
            if (Application.Current == null)
            {
                new App { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                (Application.Current as App).InitializeComponent();
                app = Application.Current as App;
            }
        }
    }
}
