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
                needsToCreateDatabase = Md5HashDocument == null;
                Logging.Debug(LogOptions.ClassName, "Database load status: {0}", !needsToCreateDatabase);
            }

            if (needsToCreateDatabase)
            {
                Logging.Debug(LogOptions.ClassName, "Creating new database file");
                Md5HashDocument = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("database"));
            }
        }

        public bool FileEntryUpToDate(string packageFilename, DateTime fileTime)
        {
            return FileEntryUpToDate(packageFilename, Convert.ToString(fileTime.ToFileTime(), 10));
        }

        public bool FileEntryUpToDate(string packageFilename, string fileTimeString)
        {
            //get the last remaining entry and check if the entry is up to date
            //FirstOrDefault returns null for reference types
            //https://developerpublish.com/default-keyword-in-c/
            XElement element = Md5HashDocument.Descendants("file").FirstOrDefault(arg => arg.Attribute("filename").Value.Equals(packageFilename) && arg.Attribute("filetime").Value.Equals(fileTimeString));

            //check if the return was null, it means that the entry wasn't found and therefore isn't up to date
            if (element == null)
            {
                return false;
            }

            string md5EntryTimeValue = element.Attribute("filetime").Value;
            Logging.Debug(LogOptions.ClassName, "md5EntryTimeValue: {0}, fileTimeString: {1}", md5EntryTimeValue, fileTimeString);
            return md5EntryTimeValue.Equals(fileTimeString);
        }

        public void UpdateFileEntry(string packageFilename, DateTime fileTime, string md5Value)
        {
            UpdateFileEntry(packageFilename, Convert.ToString(fileTime.ToFileTime(), 10), md5Value);
        }

        public void UpdateFileEntry(string packageFilename, string fileTimeString, string md5Value)
        {
            XElement element = Md5HashDocument.Descendants("file").FirstOrDefault(arg => arg.Attribute("filename").Value.Equals(packageFilename) && arg.Attribute("filetime").Value.Equals(fileTimeString));

            //check if the return was null, it means that the entry wasn't found and therefore isn't up to date
            if (element == null)
            {
                Logging.Info(LogOptions.MethodAndClassName, "XElement search returned null, no entry exists, adding one");
                Md5HashDocument.Element("database").Add(new XElement("file", new XAttribute("filename", packageFilename), new XAttribute("filetime", fileTimeString), new XAttribute("md5", md5Value)));
                Logging.Debug(LogOptions.MethodAndClassName, "XElement added. filename = {0}, filetime = {1}, md5 = {2}", packageFilename, fileTimeString, md5Value);
            }
            else
            {
                element.Attribute("filetime").Value = fileTimeString;
                element.Attribute("md5").Value = md5Value;
                Logging.Debug(LogOptions.MethodAndClassName, "XElement updated. filename = {0}, filetime = {1}, md5 = {2}", packageFilename, fileTimeString, md5Value);
            }
        }

        public void DeleteFileEntry(string packageFilename)
        {
            int matches = Md5HashDocument.Descendants("file").Where(arg => arg.Attribute("filename").Value.Equals(packageFilename)).Count();

            while (matches > 0)
            {
                Md5HashDocument.Descendants("file").First(arg => arg.Attribute("filename").Value.Equals(packageFilename)).Remove();
                matches--;
            }
        }
    }
}
