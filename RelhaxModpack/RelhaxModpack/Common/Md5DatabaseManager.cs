using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RelhaxModpack.Common
{
    public class Md5DatabaseManager
    {
        public const string DatabaseRootNodeName = "database";

        public const string DatabaseFileNodeName = "file";

        public const string DatabaseFilenameAttributeName = "filename";

        public const string DatabaseFiletimeAttributeName = "filetime";

        public const string DatabaseFiletimeMd5Name = "md5";

        public bool DatabaseLoaded { get; private set; } = false;

        private XDocument Md5HashDocument;

        public void LoadMd5Database(string fileLocation)
        {
            if (string.IsNullOrEmpty(fileLocation))
                throw new BadMemeException("fileLocation is empty");

            bool needsToCreateDatabase = !File.Exists(fileLocation);
            Logging.Debug(LogOptions.ClassName, "Md5 file exists: {0}", !needsToCreateDatabase);
            if (!needsToCreateDatabase)
            {
                //try to load it
                Logging.Debug(LogOptions.ClassName, "Attempt to load database file");
                Md5HashDocument = XmlUtils.LoadXDocument(fileLocation, XmlLoadType.FromFile);

                //check if it was able to load  the database
                needsToCreateDatabase = (Md5HashDocument == null);
                Logging.Debug(LogOptions.ClassName, "Database load status: {0}", !needsToCreateDatabase);
            }

            if (needsToCreateDatabase)
            {
                Logging.Debug(LogOptions.ClassName, "Creating new database file");
                Md5HashDocument = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement(DatabaseRootNodeName));
            }
            DatabaseLoaded = true;
        }

        public void SaveMd5Database(string fileLocation)
        {
            if (string.IsNullOrEmpty(fileLocation))
                throw new BadMemeException("fileLocation is empty");

            if (Md5HashDocument == null)
                throw new NullReferenceException();

            Md5HashDocument.Save(fileLocation);
        }

        public bool FileEntryUpToDate(string packageFilename, DateTime fileTime)
        {
            if (string.IsNullOrEmpty(packageFilename))
                throw new NullReferenceException();

            return FileEntryUpToDate(packageFilename, Convert.ToString(fileTime.ToFileTime(), 10));
        }

        public bool FileEntryUpToDate(string packageFilename, string fileTimeString)
        {
            if (string.IsNullOrEmpty(packageFilename))
                throw new NullReferenceException();

            if (string.IsNullOrEmpty(fileTimeString))
                throw new NullReferenceException();

            XElement element = GetFileEntryFromXmlDocument(packageFilename, fileTimeString);

            //check if the return was null, it means that the entry wasn't found and therefore isn't up to date
            if (element == null)
            {
                return false;
            }

            string md5EntryTimeValue = element.Attribute(DatabaseFiletimeAttributeName).Value;
            Logging.Debug(LogOptions.ClassName, "packageFilename: {0}, md5EntryTimeValue: {1}, fileTimeString: {2}", packageFilename, md5EntryTimeValue, fileTimeString);
            return md5EntryTimeValue.Equals(fileTimeString);
        }

        public string GetMd5HashFileEntry(string packageFilename, DateTime fileTime)
        {
            if (string.IsNullOrEmpty(packageFilename))
                throw new NullReferenceException();

            return GetMd5HashFileEntry(packageFilename, Convert.ToString(fileTime.ToFileTime(), 10));
        }

        public string GetMd5HashFileEntry(string packageFilename, string fileTimeString)
        {
            if (string.IsNullOrEmpty(packageFilename))
                throw new NullReferenceException();

            if (string.IsNullOrEmpty(fileTimeString))
                throw new NullReferenceException();

            XElement element = GetFileEntryFromXmlDocument(packageFilename, fileTimeString);

            //check if the return was null, it means that the entry wasn't found and therefore isn't up to date
            if (element == null)
            {
                return null;
            }

            return element.Attribute(DatabaseFiletimeMd5Name).Value;
        }

        public void UpdateFileEntry(string packageFilename, DateTime fileTime, string md5Value)
        {
            if (string.IsNullOrEmpty(packageFilename))
                throw new NullReferenceException();

            if (string.IsNullOrEmpty(md5Value))
                throw new NullReferenceException();

            UpdateFileEntry(packageFilename, Convert.ToString(fileTime.ToFileTime(), 10), md5Value);
        }

        public void UpdateFileEntry(string packageFilename, string fileTimeString, string md5Value)
        {
            if (string.IsNullOrEmpty(packageFilename))
                throw new NullReferenceException();

            if (string.IsNullOrEmpty(md5Value))
                throw new NullReferenceException();

            if (string.IsNullOrEmpty(fileTimeString))
                throw new NullReferenceException();

            XElement element = GetFileEntryFromXmlDocument(packageFilename, fileTimeString);

            //check if the return was null, it means that the entry wasn't found and therefore isn't up to date
            if (element == null)
            {
                Md5HashDocument.Element(DatabaseRootNodeName).Add(new XElement(DatabaseFileNodeName, new XAttribute(DatabaseFilenameAttributeName, packageFilename), new XAttribute(DatabaseFiletimeAttributeName, fileTimeString), new XAttribute(DatabaseFiletimeMd5Name, md5Value)));
                Logging.Debug(LogOptions.MethodAndClassName, "XElement added. filename = {0}, filetime = {1}, md5 = {2}", packageFilename, fileTimeString, md5Value);
            }
            else
            {
                element.Attribute(DatabaseFiletimeAttributeName).Value = fileTimeString;
                element.Attribute(DatabaseFiletimeMd5Name).Value = md5Value;
                Logging.Debug(LogOptions.MethodAndClassName, "XElement updated. filename = {0}, filetime = {1}, md5 = {2}", packageFilename, fileTimeString, md5Value);
            }
        }

        public void DeleteFileEntry(string packageFilename)
        {
            if (string.IsNullOrEmpty(packageFilename))
                throw new NullReferenceException();

            int matches = Md5HashDocument.Descendants(DatabaseFileNodeName).Where(arg => arg.Attribute(DatabaseFilenameAttributeName).Value.Equals(packageFilename)).Count();

            while (matches > 0)
            {
                Md5HashDocument.Descendants(DatabaseFileNodeName).First(arg => arg.Attribute(DatabaseFilenameAttributeName).Value.Equals(packageFilename)).Remove();
                matches--;
            }
        }

        public bool FileEntryWithoutTimeExists(string packageFilename)
        {
            return Md5HashDocument.Descendants(DatabaseFileNodeName).FirstOrDefault(arg => arg.Attribute(DatabaseFilenameAttributeName).Value.Equals(packageFilename)) != null;
        }

        private XElement GetFileEntryFromXmlDocument(string packageFilename, string fileTimeString)
        {
            //get the last remaining entry and check if the entry is up to date
            //FirstOrDefault returns null for reference types
            //https://developerpublish.com/default-keyword-in-c/
            if (Md5HashDocument == null)
                return null;
            else
                return Md5HashDocument.Descendants(DatabaseFileNodeName).FirstOrDefault(arg => arg.Attribute(DatabaseFilenameAttributeName).Value.Equals(packageFilename) && arg.Attribute(DatabaseFiletimeAttributeName).Value.Equals(fileTimeString));
        }
    }
}
