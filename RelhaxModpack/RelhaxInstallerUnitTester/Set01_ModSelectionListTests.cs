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

        //this technically applies to every test upon initialization, but it's placed here
        //https://docs.microsoft.com/en-us/previous-versions/visualstudio/visual-studio-2012/ms245572(v=vs.110)
        [AssemblyInitialize]
        public static async Task Setup(TestContext context)
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
                (Application.Current as App).InitializeComponent();
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
        public static void CleanupLogging()
        {
            //init all logs if they aren't already init
            foreach (Logfiles logfile in UnitTestHelper.AllLogFiles)
            {
                if (!Logging.IsLogDisposed(logfile))
                    Logging.DisposeLogging(logfile);
            }
        }

        [TestInitialize]
        public void Init()
        {
            modpackSettings = new ModpackSettings()
            {
                DatabaseDistroVersion = RelhaxModpack.Utilities.Enums.DatabaseVersions.Beta,
                SaveLastSelection = true
            };

            commandLineSettings = new CommandLineSettings(null)
            {

            };
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
                selectionList = new PackageSelectionList(modpackSettings, commandLineSettings, new DatabaseManager(modpackSettings, commandLineSettings) { ManagerInfoZipfile = ((App)RelhaxModpack.App.Current).ManagerInfoZipfile })
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

                selectionList.Closed += (sender, e) => selectionList.Dispatcher.InvokeShutdown();
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

            selectionList.OnSelectionListReturn += SelectionList_OnSelectionListReturn;

            Logging.Info("Selecting 100 components");
            selectionList.Dispatcher.Invoke(() =>
            {
                List<SelectablePackage> flatList = DatabaseUtils.GetFlatSelectablePackageList(selectionList.ParsedCategoryList);
                Random random = new Random();
                for (int i = 0; i < 100; i++)
                {
                    int selectIndex = random.Next(0, flatList.Count);
                    SelectablePackage package = flatList[selectIndex];
                    Logging.Info(string.Format("Index {0} selects package {1}", selectIndex, package.PackageName));
                    package.Checked = true;
                }

                //click the continue button
                List<FrameworkElement> elements = UiUtils.GetAllWindowComponentsLogical(selectionList, false);
                FrameworkElement buttonElement = elements.Find(element => element.Tag != null && element.Tag.Equals("ContinueButton"));
                Button clearSelectionsButton = buttonElement as Button;
                clearSelectionsButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            });
        }
        
        private void SelectionList_OnSelectionListReturn(object sender, SelectionListEventArgs e)
        {
            Assert.IsTrue(e.ContinueInstallation);
            Assert.IsTrue(e.GlobalDependencies.Count > 0);
            Assert.IsTrue(e.Dependencies.Count > 0);
            Assert.IsTrue(e.ParsedCategoryList.Count > 0);
        }
    }
}
