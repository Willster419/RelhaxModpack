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

namespace RelhaxInstallerUnitTester
{
    [TestClass]
    public class Set01_ModSelectionListTests : UnitTestLogBase
    {
        [TestMethod]
        public void Test01_LoadModpackSettingsTest()
        {
            InstallerUnitTestHelper.InstallerHelperLog = UnitTestHelper.CreateLogfile();
            Assert.IsNotNull(InstallerUnitTestHelper.InstallerHelperLog);
            Assert.IsTrue(InstallerUnitTestHelper.InstallerHelperLog.CanWrite);

            bool settingsLoaded = Settings.LoadSettings(Settings.ModpackSettingsFileName, typeof(ModpackSettings), null, null);
            if (File.Exists(Settings.ModpackSettingsFileName))
                Assert.IsTrue(settingsLoaded);
        }

        [TestMethod]
        public void Test02_LoadTranslationsForSelectionListTest()
        {
            InstallerUnitTestHelper.InstallerHelperLog.Write("Loading translations for windows");
            Translations.LoadTranslations();
        }

        [TestMethod]
        public async Task Test03_LoadModSelectionListTest()
        {
            InstallerUnitTestHelper.InstallerHelperLog.Write("Creating a ModSelectionList Window");
            InstallerUnitTestHelper.InstallerHelperList = new ModSelectionList()
            {
                ApplyColorSettings = false,
                ApplyScaling = false,
                ApplyToolTips = true,
                AutoInstallMode = false,
                LocalizeWindow = true,
                OriginalHeight = 720.0,
                OriginalWidth = 1280.0,
                LastSupportedWoTClientVersion = "1.10.0.1",
                //the lists are newed in the application
                GlobalDependencies = InstallerUnitTestHelper.GlobalDependencies,
                Dependencies = InstallerUnitTestHelper.Dependencies,
                ParsedCategoryList = InstallerUnitTestHelper.ParsedCategoryList
            };

            InstallerUnitTestHelper.InstallerHelperList.Show();

            await Task.Delay(10000);

            List<FrameworkElement> elements = UiUtils.GetAllWindowComponentsLogical(InstallerUnitTestHelper.InstallerHelperList, false);

            FrameworkElement buttonElement = elements.Find(element => element.Tag != null && element.Tag.Equals("ClearSelectionsButton"));

            Button clearSelectionsButton = buttonElement as Button;
            clearSelectionsButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
    }
}
