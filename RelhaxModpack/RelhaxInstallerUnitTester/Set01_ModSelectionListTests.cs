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

namespace RelhaxInstallerUnitTester
{
    [TestClass]
    public class Set01_ModSelectionListTests : UnitTestLogBase
    {
        //declaring these objects as static will allow them to exist throughout the test
        //exists in all methods
        private static ModSelectionList SelectionList = null;
        private static List<DatabasePackage> GlobalDependencies = null;
        private static List<Dependency> Dependencies = null;
        private static List<Category> ParsedCategoryList = null;
        private static Logfile log = null;
        private static App app = null;

        [TestMethod]
        public void Test01_LoadModpackSettingsTest()
        {
            bool settingsLoaded = Settings.LoadSettings(Settings.ModpackSettingsFileName, typeof(ModpackSettings), null, null);
            if (File.Exists(Settings.ModpackSettingsFileName))
                Assert.IsTrue(settingsLoaded);

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
            foreach(string folderPath in Settings.FoldersToCheck)
            {
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
            }

            //get the managerInfo document
            Settings.ManagerInfoZipfile = await CommonUtils.GetManagerInfoZipfileAsync(true);
        }

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
                SelectionList = new ModSelectionList()
                {
                    ApplyColorSettings = false,
                    ApplyScaling = false,
                    ApplyToolTips = true,
                    AutoInstallMode = false,
                    LocalizeWindow = true,
                    OriginalHeight = 720.0,
                    OriginalWidth = 1280.0,
                    LastSupportedWoTClientVersion = "1.10.0.2",
                    //the lists are newed in the application
                    GlobalDependencies = Set01_ModSelectionListTests.GlobalDependencies,
                    Dependencies = Set01_ModSelectionListTests.Dependencies,
                    ParsedCategoryList = Set01_ModSelectionListTests.ParsedCategoryList
                };

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

            List<FrameworkElement> elements = null;
            FrameworkElement buttonElement = null;

            SelectionList.Dispatcher.Invoke(() =>
            {
                elements = UiUtils.GetAllWindowComponentsLogical(SelectionList, false);
                buttonElement = elements.Find(element => element.Tag != null && element.Tag.Equals("CancelButton"));
                Button clearSelectionsButton = buttonElement as Button;
                clearSelectionsButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            });
        }

        [ClassInitialize]
        public static void InitClass(TestContext ctx)
        {
            string[] fullyQualifiedTestNameSplit = ctx.FullyQualifiedTestClassName.Split('.');
            //throw exception if it fails to create the log file
            log = new Logfile(fullyQualifiedTestNameSplit[fullyQualifiedTestNameSplit.Length-1], Logging.ApplicationLogfileTimestamp);

            log.Init();

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
