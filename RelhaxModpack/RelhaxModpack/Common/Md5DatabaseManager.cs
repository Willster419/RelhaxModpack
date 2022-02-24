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
    /// <summary>
    /// Provides a method to write and read file hash values for files in a folder, and determining if a local copy of a file is up to date based on what the server hash and/or file time is.
    /// </summary>
    public class Md5DatabaseManager
    {
        /// <summary>
        /// The root node in the xml database file.
        /// </summary>
        public const string DatabaseRootNodeName = "database";

        /// <summary>
        /// The name of the xml element for each file entry in the database file.
        /// </summary>
        public const string DatabaseFileNodeName = "file";

        /// <summary>
        /// The attribute of each file entry xml element to store the name of the file.
        /// </summary>
        public const string DatabaseFilenameAttributeName = "filename";

        /// <summary>
        /// The attribute of each file entry xml element to store the time of the file.
        /// </summary>
        public const string DatabaseFiletimeAttributeName = "filetime";

        /// <summary>
        /// The attribute of each file entry xml element to store the md5 hash of the file.
        /// </summary>
        public const string DatabaseFiletimeMd5Name = "md5";

        /// <summary>
        /// Gets if the xml document has been loaded into the database manager.
        /// </summary>
        public bool DatabaseLoaded { get; private set; } = false;

        private XDocument Md5HashDocument;

        /// <summary>
        /// Loads the xml document into the database manager.
        /// </summary>
        /// <param name="fileLocation">The location of the xml database file to load.</param>
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

        /// <summary>
        /// Saves the xml document to a path on disk.
        /// </summary>
        /// <param name="fileLocation">The location to save the xml database to.</param>
        public void SaveMd5Database(string fileLocation)
        {
            if (string.IsNullOrEmpty(fileLocation))
                throw new BadMemeException("fileLocation is empty");

            if (Md5HashDocument == null)
                throw new NullReferenceException();

            Md5HashDocument.Save(fileLocation);
        }

        /// <summary>
        /// Determines if a file entry in the database is up to date by checking the md5 hash and given file time.
        /// </summary>
        /// <param name="packageFilename">The name of the file to look up.</param>
        /// <param name="fileTime">The last modified time of the file entry to look up.</param>
        /// <returns>True if the file entry's name, time and md5 match, false otherwise.</returns>
        public bool FileEntryUpToDate(string packageFilename, DateTime fileTime)
        {
            if (string.IsNullOrEmpty(packageFilename))
                throw new NullReferenceException();

            return FileEntryUpToDate(packageFilename, Convert.ToString(fileTime.ToFileTime(), 10));
        }

        /// <summary>
        /// Determines if a file entry in the database is up to date by checking the md5 hash and given file time.
        /// </summary>
        /// <param name="packageFilename">The name of the file to look up.</param>
        /// <param name="fileTimeString">The last modified time of the file entry to look up.</param>
        /// <returns>True if the file entry's name, time and md5 match, false otherwise.</returns>
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

        /// <summary>
        /// Gets an md5 hash entry of a file based on a given file name and time.
        /// </summary>
        /// <param name="packageFilename">The name of the file to look up.</param>
        /// <param name="fileTime">The last modified time of the file entry to look up.</param>
        /// <returns>The hash of the file if it is found, or null if the entry isn't found.</returns>
        public string GetMd5HashFileEntry(string packageFilename, DateTime fileTime)
        {
            if (string.IsNullOrEmpty(packageFilename))
                throw new NullReferenceException();

            return GetMd5HashFileEntry(packageFilename, Convert.ToString(fileTime.ToFileTime(), 10));
        }

        /// <summary>
        /// Gets an md5 hash entry of a file based on a given file name and time.
        /// </summary>
        /// <param name="packageFilename">The name of the file to look up.</param>
        /// <param name="fileTimeString">The last modified time of the file entry to look up.</param>
        /// <returns>The hash of the file if it is found, or null if the entry isn't found.</returns>
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

        /// <summary>
        /// Updates a file entry in the database with the given filename, time and md5 value. If the entry is not found, then it is added.
        /// </summary>
        /// <param name="packageFilename">The name of the file to enter.</param>
        /// <param name="fileTime">The time of the file to enter.</param>
        /// <param name="md5Value">The md5 hash of the file to enter.</param>
        public void UpdateFileEntry(string packageFilename, DateTime fileTime, string md5Value)
        {
            if (string.IsNullOrEmpty(packageFilename))
                throw new NullReferenceException();

            if (string.IsNullOrEmpty(md5Value))
                throw new NullReferenceException();

            UpdateFileEntry(packageFilename, Convert.ToString(fileTime.ToFileTime(), 10), md5Value);
        }

        /// <summary>
        /// Updates a file entry in the database with the given filename, time and md5 value. If the entry is not found, then it is added.
        /// </summary>
        /// <param name="packageFilename">The name of the file to enter.</param>
        /// <param name="fileTimeString">The time of the file to enter.</param>
        /// <param name="md5Value">The md5 hash of the file to enter.</param>
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

        /// <summary>
        /// Deletes a file entry for the database by file name, if the entry exists.
        /// </summary>
        /// <param name="packageFilename">The name of the file to lookup.</param>
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

        /// <summary>
        /// Checks if a file entry exists for a given file name.
        /// </summary>
        /// <param name="packageFilename">The name of the file to lookup.</param>
        /// <returns>true if the file entry exists in the database, false if it does not.</returns>
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
