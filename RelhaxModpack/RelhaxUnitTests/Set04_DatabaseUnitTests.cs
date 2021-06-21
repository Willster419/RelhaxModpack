using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RelhaxModpack;
using RelhaxModpack.Common;
using RelhaxModpack.Database;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Xml;

namespace RelhaxUnitTests
{
    [TestClass]
    public class Set04_DatabaseUnitTests : UnitTestLogBase
    {
        private static string LatestSupportedWoTVersion = string.Empty;
        private static List<DatabasePackage> GlobalDependenciesForSave = new List<DatabasePackage>();
        private static List<Dependency> DependenciesForSave = new List<Dependency>();
        private static List<Category> ParsedCategoryListForSave = new List<Category>();
        /*
        [TestMethod]
        public void Test01_GetLatestSupportWoTVersionTest()
        {
            XmlDocument rootDocument = DatabaseUtils.GetBetaDatabaseRoot1V1Document(ApplicationConstants.BetaDatabaseV2FolderURLEscaped.Replace(@"{branch}", "master"), "master");
            LatestSupportedWoTVersion = XmlUtils.GetXmlStringFromXPath(rootDocument, ApplicationConstants.DatabaseOnlineVersionXpath);
            Assert.IsFalse(string.IsNullOrEmpty(LatestSupportedWoTVersion));
        }

        [TestMethod]
        public void Test02_LoadStableDatabaseTest()
        {
            List<Category> ParsedCategoryList = new List<Category>();
            List<DatabasePackage> GlobalDependencies = new List<DatabasePackage>();
            List<Dependency> Dependencies = new List<Dependency>();

            string modInfoXml = string.Empty;
            Ionic.Zip.ZipFile zipfile = null;
            XmlDocument modInfoDocument = null;
            List<string> categoriesXml = new List<string>();

            string modInfoxmlURL = ApplicationConstants.BigmodsDatabaseRootEscaped.Replace(@"{dbVersion}", LatestSupportedWoTVersion) + "modInfo.dat";

            using (WebClient client = new WebClient())
            {
                //save zip file into memory for later
                zipfile = Ionic.Zip.ZipFile.Read(new MemoryStream(client.DownloadData(modInfoxmlURL)));

                //extract modinfo xml string
                modInfoXml = FileUtils.GetStringFromZip(zipfile, "database.xml");
            }

            modInfoDocument = XmlUtils.LoadXmlDocument(modInfoXml, XmlLoadType.FromString);

            string globalDependencyFilename = XmlUtils.GetXmlStringFromXPath(modInfoDocument, "/modInfoAlpha.xml/globalDependencies/@file");
            string globalDependencyXmlString = FileUtils.GetStringFromZip(zipfile, globalDependencyFilename);

            string dependencyFilename = XmlUtils.GetXmlStringFromXPath(modInfoDocument, "/modInfoAlpha.xml/dependencies/@file");
            string dependenicesXmlString = FileUtils.GetStringFromZip(zipfile, dependencyFilename);

            foreach (XmlNode categoryNode in XmlUtils.GetXmlNodesFromXPath(modInfoDocument, "//modInfoAlpha.xml/categories/category"))
            {
                string categoryFilename = categoryNode.Attributes["file"].Value;
                categoriesXml.Add(FileUtils.GetStringFromZip(zipfile, categoryFilename));
            }
            zipfile.Dispose();
            zipfile = null;

            DatabaseUtils.ParseDatabase1V1FromStrings(globalDependencyXmlString, dependenicesXmlString, categoriesXml, GlobalDependencies, Dependencies, ParsedCategoryList);

            DatabaseUtils.BuildLinksRefrence(ParsedCategoryList, false);
            DatabaseUtils.BuildLevelPerPackage(ParsedCategoryList);

            TestDatabaseEntries(GlobalDependencies, Dependencies, ParsedCategoryList, false);
        }

        [TestMethod]
        public void Test03_LoadBetaDatabaseTest()
        {
            List<Category> ParsedCategoryList = new List<Category>();
            List<DatabasePackage> GlobalDependencies = new List<DatabasePackage>();
            List<Dependency> Dependencies = new List<Dependency>();

            string modInfoXml = string.Empty;

            //load string constant url from manager info xml
            string rootXml = ApplicationConstants.BetaDatabaseV2FolderURLEscaped.Replace(@"{branch}", "master") + ApplicationConstants.BetaDatabaseV2RootFilename;

            //download the xml string into "modInfoXml"
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("user-agent", "Mozilla / 4.0(compatible; MSIE 6.0; Windows NT 5.2;)");
                modInfoXml = client.DownloadString(rootXml);
            }

            List<string> downloadURLs = DatabaseUtils.GetBetaDatabase1V1FilesList(ApplicationConstants.BetaDatabaseV2FolderURLEscaped.Replace(@"{branch}", "master"), "master");

            string[] downloadStrings = CommonUtils.DownloadStringsFromUrls(downloadURLs);

            //parse into strings
            string globalDependencyXmlStringBeta = downloadStrings[0];
            string dependenicesXmlStringBeta = downloadStrings[1];

            List<string> categoriesXmlBeta = new List<string>();
            for (int i = 2; i < downloadURLs.Count; i++)
            {
                categoriesXmlBeta.Add(downloadStrings[i]);
            }

            //parse into lists
            DatabaseUtils.ParseDatabase1V1FromStrings(globalDependencyXmlStringBeta, dependenicesXmlStringBeta, categoriesXmlBeta, GlobalDependencies, Dependencies, ParsedCategoryList);

            DatabaseUtils.BuildLinksRefrence(ParsedCategoryList, false);
            DatabaseUtils.BuildLevelPerPackage(ParsedCategoryList);

            TestDatabaseEntries(GlobalDependencies, Dependencies, ParsedCategoryList, true);

            GlobalDependenciesForSave = GlobalDependencies;
            DependenciesForSave = Dependencies;
            ParsedCategoryListForSave = ParsedCategoryList;
        }

        [TestMethod]
        public void Test04_TagSerialaizationTest()
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
        public void Test05_SaveDatabaseTest()
        {
            string databaseSavePath = "DatabaseSaveTest";

            if (Directory.Exists(databaseSavePath))
                Directory.Delete(databaseSavePath, true);

            Directory.CreateDirectory(databaseSavePath);

            XmlDocument rootDoc = DatabaseUtils.GetBetaDatabaseRoot1V1Document(ApplicationConstants.BetaDatabaseV2FolderURLEscaped.Replace(@"{branch}", "master"), "master");
            Assert.IsNotNull(rootDoc);

            List<string> allCategoriesXml = DatabaseUtils.GetBetaDatabase1V1FilesList(rootDoc, ApplicationConstants.BetaDatabaseV2FolderURLEscaped.Replace(@"{branch}", "master"));

            DatabaseUtils.SaveDatabase1V1(databaseSavePath, rootDoc, GlobalDependenciesForSave, DependenciesForSave, ParsedCategoryListForSave);

            Assert.IsTrue(File.Exists(Path.Combine(databaseSavePath, ApplicationConstants.BetaDatabaseV2RootFilename)));
            XmlDocument loadDoc = XmlUtils.LoadXmlDocument(Path.Combine(databaseSavePath, ApplicationConstants.BetaDatabaseV2RootFilename), XmlLoadType.FromFile);
            Assert.IsNotNull(loadDoc);

            foreach (string path in allCategoriesXml)
            {
                string endPath = path.Split('/').Last();
                Assert.IsTrue(File.Exists(Path.Combine(databaseSavePath, endPath)));
                loadDoc = XmlUtils.LoadXmlDocument(Path.Combine(databaseSavePath, endPath), XmlLoadType.FromFile);
                Assert.IsNotNull(loadDoc);
            }

            if (Directory.Exists(databaseSavePath))
                Directory.Delete(databaseSavePath, true);
        }
        */
        private void TestDatabaseEntries(List<DatabasePackage> GlobalDependencies, List<Dependency> Dependencies, List<Category> ParsedCategoryList, bool checkDuplicates)
        {
            List<DatabasePackage> allPackages = DatabaseUtils.GetFlatList(GlobalDependencies, Dependencies, ParsedCategoryList);
            Assert.IsNotNull(allPackages);

            foreach (DatabasePackage package in allPackages)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(package.CompletePackageNamePath));
                Assert.IsFalse(string.IsNullOrWhiteSpace(package.CompletePath));
                Assert.IsFalse(string.IsNullOrWhiteSpace(package.CompleteUIDPath));
                Assert.IsFalse(string.IsNullOrWhiteSpace(package.ComponentInternalName));
                Assert.IsFalse(string.IsNullOrWhiteSpace(package.PackageName));
                Assert.IsFalse(string.IsNullOrWhiteSpace(package.UID));
            }

            List<SelectablePackage> selectablePackages = DatabaseUtils.GetFlatSelectablePackageList(ParsedCategoryList);
            Assert.IsNotNull(selectablePackages);

            if (checkDuplicates)
            {
                List<string> duplicatesPackageNames = null;
                List<DatabasePackage> duplicatesUID = null;

                duplicatesPackageNames = DatabaseUtils.CheckForDuplicates(GlobalDependencies, Dependencies, ParsedCategoryList);
                duplicatesUID = DatabaseUtils.CheckForDuplicateUIDsPackageList(GlobalDependencies, Dependencies, ParsedCategoryList);

                Assert.IsTrue(duplicatesPackageNames.Count == 0);
                Assert.IsTrue(duplicatesUID.Count == 0);
            }
        }
    }
}
