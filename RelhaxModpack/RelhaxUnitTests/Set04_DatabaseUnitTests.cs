using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RelhaxModpack;
using RelhaxModpack.Common;
using RelhaxModpack.Database;
using RelhaxModpack.Settings;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Xml;

namespace RelhaxUnitTests
{
    [TestClass]
    public class Set04_DatabaseUnitTests
    {
        const string databaseSaveFolder = "DatabaseSaveTest";

        DatabaseManager databaseManager;

        [TestInitialize]
        public void Setup()
        {
            databaseManager = new DatabaseManager(new ModpackSettings(), new CommandLineSettings(null));

            CleanDatabaseSaveFolder(true);
        }

        [TestCleanup]
        public void Cleanup()
        {
            CleanDatabaseSaveFolder(false);
        }

        private void CleanDatabaseSaveFolder(bool createNew)
        {
            if (Directory.Exists(databaseSaveFolder))
                Directory.Delete(databaseSaveFolder, true);

            if (createNew)
                Directory.CreateDirectory(databaseSaveFolder);
        }

        [TestMethod]
        public void Test01_TagSerialaizationTest()
        {
            string tags = "Patch,Script,Atlas,ImagePNG";
            int tagsCount = tags.Split(',').Count();

            if (CommonUtils.SetObjectValue(typeof(PackageTagsList), tags, out object tagsObject))
            {
                PackageTagsList tagsObjectClass = (PackageTagsList)tagsObject;
                Assert.IsNotNull(tagsObjectClass);
                Assert.IsTrue(tagsObjectClass.Count == tagsCount);
            }
            else
                Assert.Fail("CommonUtils failed to setObjectValue, Test04");
        }

        [TestMethod]
        public async Task Test02_LoadStableDatabaseTest()
        {
            databaseManager.ModpackSettings.DatabaseDistroVersion = DatabaseVersions.Stable;
            Assert.IsTrue(await databaseManager.LoadDatabaseAsync() == DatabaseLoadFailCode.None);
            TestDatabaseEntries();
        }

        [TestMethod]
        public async Task Test03_LoadBetaDatabaseTest()
        {
            databaseManager.ModpackSettings.DatabaseDistroVersion = DatabaseVersions.Beta;
            databaseManager.ModpackSettings.BetaDatabaseSelectedBranch = "master";
            Assert.IsTrue(await databaseManager.LoadDatabaseAsync() == DatabaseLoadFailCode.None);
            TestDatabaseEntries();
        }

        [TestMethod]
        public async Task Test04_SaveDatabaseFromBetaTest()
        {
            databaseManager.ModpackSettings.DatabaseDistroVersion = DatabaseVersions.Beta;
            databaseManager.ModpackSettings.BetaDatabaseSelectedBranch = "master";
            Assert.IsTrue(await databaseManager.LoadDatabaseAsync() == DatabaseLoadFailCode.None);
            databaseManager.SaveDatabase(databaseSaveFolder);

            //testing part
            string rootDatabasePath = Path.Combine(databaseSaveFolder, ApplicationConstants.BetaDatabaseV2RootFilename);
            Assert.IsTrue(File.Exists(rootDatabasePath));
            XmlDocument loadDoc = XmlUtils.LoadXmlDocument(rootDatabasePath, XmlLoadType.FromFile);
            Assert.IsNotNull(loadDoc);

            List<string> allCategoriesXml = await databaseManager.GetBetaDatabase1V1FilesListAsync();
            foreach (string path in allCategoriesXml)
            {
                string endPath = path.Split('/').Last();
                endPath = Path.Combine(databaseSaveFolder, endPath);
                Assert.IsTrue(File.Exists(endPath));
                loadDoc = XmlUtils.LoadXmlDocument(endPath, XmlLoadType.FromFile);
                Assert.IsNotNull(loadDoc);
            }
        }

        [TestMethod]
        public async Task Test05_LoadTestDatabaseFromStableTest()
        {
            //load stable, save it to disk, then load it back as custom
            databaseManager.ModpackSettings.DatabaseDistroVersion = DatabaseVersions.Stable;
            Assert.IsTrue(await databaseManager.LoadDatabaseAsync() == DatabaseLoadFailCode.None);
            databaseManager.SaveDatabase(databaseSaveFolder);

            Assert.IsTrue(await databaseManager.LoadDatabaseTestAsync(Path.Combine(databaseSaveFolder, ApplicationConstants.BetaDatabaseV2RootFilename)) == DatabaseLoadFailCode.None);
            TestDatabaseEntries();
        }
        
        private void TestDatabaseEntries()
        {
            Assert.IsNotNull(databaseManager);

            //test flat list function (all packages)
            List<DatabasePackage> allPackages = databaseManager.GetFlatList();
            Assert.IsNotNull(allPackages);
            Assert.IsFalse(allPackages.Count == 0);

            foreach (DatabasePackage package in allPackages)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(package.CompletePackageNamePath));
                Assert.IsFalse(string.IsNullOrWhiteSpace(package.CompletePath));
                Assert.IsFalse(string.IsNullOrWhiteSpace(package.CompleteUIDPath));
                Assert.IsFalse(string.IsNullOrWhiteSpace(package.ComponentInternalName));
                Assert.IsFalse(string.IsNullOrWhiteSpace(package.PackageName));
                Assert.IsFalse(string.IsNullOrWhiteSpace(package.UID));
            }

            //we're not technically checking these functions, but at least they should be able to run without throwing an exception, right?
            databaseManager.ProcessDatabase();
            databaseManager.CalculateInstallLists(true, false);

            //test flat list function (selectable packages)
            List<SelectablePackage> selectablePackages = databaseManager.GetFlatSelectablePackageList();
            Assert.IsNotNull(selectablePackages);

            //test duplicate finder functions
            List<string> duplicatesPackageNames = databaseManager.CheckForDuplicatePackageNamesStringsList();
            List<DatabasePackage> duplicatesUID = databaseManager.CheckForDuplicateUIDsPackageList();
            Assert.IsTrue(duplicatesPackageNames.Count == 0);
            Assert.IsTrue(duplicatesUID.Count == 0);
        }
    }
}
