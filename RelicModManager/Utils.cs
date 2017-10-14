using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System;
using System.Linq;
using System.ComponentModel;
using System.Net;
using System.Globalization;
using System.Xml.XPath;
using Ionic.Zip;
using System.Runtime.InteropServices;
using System.Drawing;

namespace RelhaxModpack
{
    //a static utility class with usefull methods that all other forms can use if they need it
    public static class Utils
    {
        private static int numByteReads = 0;
        private static bool patchDone = false;
        private static int genericTraverse = 0;
        private static List<string> parsedZips;
        private static string xvmBootFileLoc1 = "\\res_mods\\configs\\xvm\\xvm.xc";
        private static string xvmBootFileLoc2 = "\\mods\\configs\\xvm\\xvm.xc";
        public static int totalModConfigComponents = 0;
        private static int iMaxLogLength = 1500000; // Probably should be bigger, say 2,000,000
        private static int iTrimmedLogLength = -300000; // minimum of how much of the old log to leave

        private static object _locker = new object();

        //logs string info to the log output
        public static void appendToLog(string info)
        {
            lock (_locker)              // avoid that 2 or more threads calling the Log function and writing lines in a mess
            {
                //the method should automaticly make the file if it's not there
                string filePath = Path.Combine(Application.StartupPath, "RelHaxLog.txt");
                if (!System.IO.File.Exists(filePath))
                {
                    System.IO.File.AppendAllText(filePath, "");
                }
                //if the info text is containing any linefeed/carrieage return, intend the next line with 26 space char
                info = info.Replace("\n", "\n" + string.Concat(Enumerable.Repeat(" ", 26)));
                writeToFile(filePath, string.Format("{0:yyyy-MM-dd HH:mm:ss.fff}   {1}", DateTime.Now, info));
            }
        }

        // https://stackoverflow.com/questions/4741037/keeping-log-files-under-a-certain-size
        static public void writeToFile(string strFile, string strNewLogMessage)
        {
            try
            {
                // bigger logfile size at testing and developing
                int multi = 1;
                if (Program.testMode) multi = 100;

                FileInfo fi = new FileInfo(strFile);

                Byte[] bytesSavedFromEndOfOldLog = null;

                if (fi.Length > iMaxLogLength * multi) // if the log file length is already too long
                {
                    using (BinaryReader br = new BinaryReader(System.IO.File.Open(strFile, FileMode.Open)))
                    {
                        // Seek to our required position of what you want saved.
                        br.BaseStream.Seek(iTrimmedLogLength * multi, SeekOrigin.End);

                        // Read what you want to save and hang onto it.
                        bytesSavedFromEndOfOldLog = br.ReadBytes((-1 * iTrimmedLogLength * multi));
                    }
                }

                byte[] newLine = System.Text.UTF8Encoding.UTF8.GetBytes(Environment.NewLine);

                FileStream fs = null;
                // If the log file is less than the max length, just open it at the end to write there
                if (fi.Length < iMaxLogLength * multi)
                    fs = new FileStream(strFile, FileMode.Append, FileAccess.Write, FileShare.Read);
                else // If the log file is more than the max length, just open it empty
                {
                    fs = new FileStream(strFile, FileMode.Create, FileAccess.Write, FileShare.Read);
                    // https://stackoverflow.com/questions/5266069/streamwriter-and-utf-8-byte-order-marks
                    // Creates the UTF-8 encoding with parameter "encoderShouldEmitUTF8Identifier" set to true
                    Encoding vUTF8Encoding = new UTF8Encoding(true);
                    // Gets the preamble in order to attach the BOM
                    var vPreambleByte = vUTF8Encoding.GetPreamble();
                    // Writes the preamble first
                    fs.Write(vPreambleByte, 0, vPreambleByte.Length);
                }

                using (fs)
                {
                    // If you are trimming the file length, write what you saved. 
                    if (bytesSavedFromEndOfOldLog != null)
                    {
                        Byte[] lineBreak = Encoding.UTF8.GetBytes(string.Format("### {0:yyyy-MM-dd HH:mm:ss} *** *** *** Old Log Start Position *** *** *** *** ###", DateTime.Now));
                        fs.Write(lineBreak, 0, lineBreak.Length);
                        fs.Write(newLine, 0, newLine.Length);
                        fs.Write(bytesSavedFromEndOfOldLog, 0, bytesSavedFromEndOfOldLog.Length);
                        fs.Write(newLine, 0, newLine.Length);
                    }
                    Byte[] sendBytes = Encoding.UTF8.GetBytes(strNewLogMessage);
                    // Append your last log message. 
                    fs.Write(sendBytes, 0, sendBytes.Length);
                    fs.Write(newLine, 0, newLine.Length);
                }
            }
            catch
            {
                ; // Nothing to do...
                  //writeEvent("writeToFile() Failed to write to logfile : " + ex.Message + "...", 5);
            }
        }

        /// <summary>
        /// print all information about the object to the logfile
        /// </summary>
        /// <param objectName="option">only a Name of the object as an information at the logfile</param>
        /// <param n=object>the object itself that should be printed</param>
        public static void dumpObjectToLog(string objectName, object n)
        {
            lock (_locker)              // avoid that 2 or more threads calling the Log function and writing lines in a mess
            {
                Utils.appendToLog(String.Format("----- dump of object {0} ------", objectName));
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(n))
                {
                    string name = descriptor.Name;
                    object value = descriptor.GetValue(n);
                    if (value == null)
                        value = "(null)";
                    else if (value is string && value.ToString().Trim().Equals(""))
                        value = "(string with lenght 0)";
                    Utils.appendToLog(string.Format("{0}={1}", name, value));
                }
                Utils.appendToLog("----- end of dump ------");
            }
        }

        /// <summary>
        /// default logging function of exception informations, possible to expand the cxception Group with his own needed informations of the specific exception
        /// </summary>
        /// <param e=Exception>the exception object that would be catched</param>
        public static void exceptionLog(Exception e)
        {
            Utils.exceptionLog("", "", e);
        }

        /// <summary>
        /// default logging function of exception informations, possible to expand the cxception Group with his own needed informations of the specific exception
        /// </summary>
        /// <param msg=string>the name of the function or other unified informations to traceback the point of exception</param>
        /// <param e=Exception>the exception object that would be catched</param>
        public static void exceptionLog(string msg, Exception e)
        {
            Utils.exceptionLog(msg, "", e);
        }

        /// <summary>
        /// default logging function of exception informations, possible to expand the cxception Group with his own needed informations of the specific exception              https://msdn.microsoft.com/de-de/library/system.exception.data(v=vs.110).aspx
        /// </summary>
        /// <param msg=string>the name of the function or other unified informations to traceback the point of exception</param>
        /// <param info=string>more informations of the function that throw the exception</param>
        /// <param e=Exception>the exception object that would be catched</param>
        public static void exceptionLog(string msgString, string infoString, Exception e)
        {
            // increase error Counter by every call of this function
            MainWindow.errorCounter++;
            lock (_locker)              // avoid that 2 or more threads calling the Log function and writing lines in a mess
            {
                e = e.GetBaseException();
                string errorType = "Exception";
                string info = "";
                try { info = string.Format("{0}", infoString.Equals("") || infoString == null ? "" : string.Format("Additional Info: {0}\n", infoString)); } catch { };
                string type = "";
                try { type = string.Format("Type: {0}\n", e.GetType()); } catch { };
                string exception = "";
                try { exception = string.Format("Code: {0}\n", e.ToString()); } catch { };
                string stackTrace = "";
                try { stackTrace = string.Format("StackTrace: {0}\n", e.StackTrace == null ? "(null)" : e.StackTrace.Equals("") ? "(empty)" : e.StackTrace.ToString()); } catch { };
                string message = "";
                try { message = string.Format("Message: {0}\n", e.Message == null ? "(null)" : e.Message.Equals("") ? "(empty)" : e.Message.ToString()); } catch { };
                string source = "";
                try { source = string.Format("Source: {0}\n", e.Source == null ? "(null)" : e.Source.Equals("") ? "(empty)" : e.Source.ToString()); } catch { };
                string targetSite = "";
                try { targetSite = string.Format("TargetSite: {0}\n", e.TargetSite == null ? "(null)" : e.TargetSite.Equals("") ? "(empty)" : e.TargetSite.ToString()); } catch { };
                string innerException = "";
                try { innerException = string.Format("InnerException: {0}\n", e.InnerException == null ? "(null)" : e.InnerException.Equals("") ? "(empty)" : e.InnerException.ToString()); } catch { };
                string data = "";
                try { data = string.Format("Data: {0}\n", e.Data == null ? "(null)" : e.Data.Equals("") ? "(empty)" : e.Data.ToString()); } catch { };

                if (e is WebException)
                {
                    errorType = "WebException";
                    type = "";
                }
                else if (e is IOException)
                {
                    errorType = "IOException";
                    type = "";
                }
                else if (e is UnauthorizedAccessException)
                {
                    errorType = "UnauthorizedAccessException";
                    type = "";
                }
                else if (e is ArgumentException)
                {
                    errorType = "ArgumentException";
                    innerException = "";
                    data = "";
                    type = "";
                }
                else if (e is ZipException)
                {
                    errorType = "ZipException";
                    innerException = "";
                    data = "";
                    type = "";
                }
                string msgHeader = "";
                try { msgHeader = string.Format("{0} {1}(call stack traceback)\n", errorType, msgString.Equals("") || msgString == null ? "" : string.Format(@"at ""{0}"" ", msgString)); } catch { };
                string msg = "";
                try { msg += string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}", msgHeader, info, type, exception, stackTrace, message, source, targetSite, innerException, data); } catch { };
                try { msg += "----------------------------"; } catch { };
                Utils.appendToLog(msg);
            }
        }

        public static bool IsValidXml(string xmlString)
        {
            XmlTextReader read = new XmlTextReader(xmlString);
            try
            {
                while (read.Read()) ;
                read.Close();
                return true;
            }
            catch (Exception e)
            {
                Utils.exceptionLog(e);
                read.Close();
                return false;
            }
        }
        //returns the md5 hash of the file based on the input file string location. It is searching in the database first. If not found in database or the filetime is not the same, it will create a new Hash and update the database
        public static string getMd5Hash(string inputFile)
        {
            if (Program.databaseUpdateOnline)
            {
                // if in databaseupdate mode, the online databse is downloader to get the informations of all accessable online files of this gameVersion
                try
                {
                    XDocument doc = XDocument.Load(MainWindow.onlineDatabaseXmlFile);
                    try
                    {
                        XElement element = doc.Descendants("file")
                           .Where(arg => arg.Attribute("name").Value == inputFile)
                           .Single();
                        return element.Attribute("md5").Value;
                    }
                    catch (InvalidOperationException)
                    {
                        // catch the Exception if no entry is found
                    }
                }
                catch (Exception ex)
                {
                    Utils.exceptionLog("getMd5Hash", "read from databaseupdate", ex);
                }
                return "f";
            }
            
            // check if databse exists and if not, create it
            Utils.createMd5HashDatabase();
            // get filetime from file, convert it to string with base 10
            string tempFiletime = Convert.ToString(System.IO.File.GetLastWriteTime(inputFile).ToFileTime(), 10);
            // extract filename with path
            string tempFilename = Path.GetFileName(inputFile);
            // check database for filename with filetime
            string tempHash = Utils.getMd5HashDatabase(tempFilename, tempFiletime);
            if (tempHash == "-1")   // file not found in database
            {
                // create Md5Hash from file
                tempHash = Utils.createMd5Hash(inputFile);

                if (tempHash == "-1")
                {
                    // no file found, then delete from database
                    Utils.deleteMd5HashDatabase(tempFilename);
                }
                else
                {
                    // file found. update the database with new values
                    Utils.updateMd5HashDatabase(tempFilename, tempHash, tempFiletime);
                }
                // report back the created Hash
                return tempHash;
            }
            else                    // Hash found in database
            {
                // report back the stored Hash
                return tempHash;
            }
        }

        //returns the md5 hash of the file based on the input file string location
        public static string createMd5Hash(string inputFile)
        {
            //first, return if the file does not exist
            if (!System.IO.File.Exists(inputFile))
                return "-1";
            MD5 md5Hash = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            var stream = System.IO.File.OpenRead(inputFile);
            byte[] data = md5Hash.ComputeHash(stream);
            stream.Close();
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();
            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public static void createMd5HashDatabase()
        {
            if (!System.IO.File.Exists(MainWindow.md5HashDatabaseXmlFile))
            {
                XDocument doc = new XDocument( new XDeclaration("1.0", "utf-8", "yes"), new XElement("database"));
                doc.Save(MainWindow.md5HashDatabaseXmlFile);
            }
        }

        // need filename and filetime to check the database
        public static string getMd5HashDatabase(string inputFile, string inputFiletime)
        {
            try
            {
                XDocument doc = XDocument.Load(MainWindow.md5HashDatabaseXmlFile);
                bool exists = doc.Descendants("file")
                       .Where(arg => arg.Attribute("filename").Value == inputFile && arg.Attribute("filetime").Value == inputFiletime)
                       .Any();
                if(exists)
                {
                    XElement element = doc.Descendants("file")
                       .Where(arg => arg.Attribute("filename").Value == inputFile && arg.Attribute("filetime").Value == inputFiletime)
                       .Single();
                    return element.Attribute("md5").Value;
                }
            }
            catch (Exception e)
            {
                Utils.exceptionLog("getMd5HashDatabase", e);
                System.IO.File.Delete(MainWindow.md5HashDatabaseXmlFile);     // delete damaged XML database
                createMd5HashDatabase();                            // create new XML database
            }
            return "-1";
        }

        public static void updateMd5HashDatabase(string inputFile, string inputMd5Hash, string inputFiletime)
        {
            try
            {
                XDocument doc = XDocument.Load(MainWindow.md5HashDatabaseXmlFile);
                try
                {
                    XElement element = doc.Descendants("file")
                       .Where(arg => arg.Attribute("filename").Value == inputFile)
                       .Single();
                    element.Attribute("filetime").Value = inputFiletime;
                    element.Attribute("md5").Value = inputMd5Hash;
                }
                catch (InvalidOperationException)
                {
                    doc.Element("database").Add(new XElement("file", new XAttribute("filename", inputFile), new XAttribute("filetime", inputFiletime), new XAttribute("md5", inputMd5Hash)));
                }
                doc.Save(MainWindow.md5HashDatabaseXmlFile);
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("updateMd5HashDatabase", ex);
            }
        }

        public static void deleteMd5HashDatabase(string inputFile)
        {
            // only for caution
            Utils.createMd5HashDatabase();

            // extract filename from path (if call with full path)
            string tempFilename = Path.GetFileName(inputFile);

            XDocument doc = XDocument.Load(MainWindow.md5HashDatabaseXmlFile);
            try
            {
                doc.Descendants("file").Where(arg => arg.Attribute("filename").Value == tempFilename).Remove();
                doc.Save(MainWindow.md5HashDatabaseXmlFile);
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }

        public static string readVersionFromModInfo(string f)
        {
            XDocument doc = XDocument.Load(f);
            try
            {
                XElement element = doc.Descendants("modInfoAlpha.xml").Single();
                return element.Attribute("version").Value;
            }
            catch (InvalidOperationException)
            {
                return "error"; // catch the Exception if no entry is found
            }
        }

        public static string readOnlineFolderFromModInfo(string f)
        {
            XDocument doc = XDocument.Load(f);
            try
            {
                XElement element = doc.Descendants("modInfoAlpha.xml").Single();
                return element.Attribute("onlineFolder").Value;
            }
            catch (InvalidOperationException)
            {
                return "error"; // catch the Exception if no entry is found
            }
        }

        public static bool parseBool(string input, bool defaultValue)
        {
            bool returnVal;
            try
            {
                returnVal = bool.Parse(input);
            }
            catch (System.FormatException)
            {
                returnVal = defaultValue;
            }
            return returnVal;
        }
        public static int parseInt(string input, int defaultValue)
        {
            int returnVal;
            try
            {
                returnVal = int.Parse(input);
            }
            catch (System.FormatException)
            {
                returnVal = defaultValue;
            }
            return returnVal;
        }
        public static float parseFloat(string input, float defaultValue)
        {
            float returnVal;
            try
            {
                returnVal = float.Parse(input, CultureInfo.InvariantCulture);
            }
            catch (System.FormatException)
            {
                returnVal = defaultValue;
            }
            return returnVal;
        }

        //parses the xml mod info into the memory database (change XML reader from XMLDocument to XDocument)
        // https://www.google.de/search?q=c%23+xdocument+get+line+number&oq=c%23+xdocument+get+line+number&aqs=chrome..69i57j69i58.11773j0j7&sourceid=chrome&ie=UTF-8
        // public static void createModStructure(string databaseURL, List<Dependency> globalDependencies, List<Dependency> dependencies, List<LogicalDependnecy> logicalDependencies, List<Category> parsedCatagoryList, List<DeveloperSelections> developerSelections = null)
        public static void createModStructure(string databaseURL, List<Dependency> globalDependencies, List<Dependency> dependencies, List<LogicalDependnecy> logicalDependencies, List<Category> parsedCatagoryList)
        {
            try
            {
                MainWindow.developerSelections.Clear();
                totalModConfigComponents = 0;
                XDocument doc = null;
                try
                {
                    if (databaseURL.ToLower().Equals(Settings.modInfoDatFile.ToLower()))
                    {
                        Utils.appendToLog("loading dat config file");
                        string xmlString = Utils.getStringFromZip(Settings.modInfoDatFile, "modInfo.xml");
                        doc = XDocument.Parse(xmlString, LoadOptions.SetLineInfo);
                        // create new developerSelections NameList
                        parseDeveloperSelections(doc);
                    }
                    else
                    {
                        Utils.appendToLog("loading local config file");
                        doc = XDocument.Load(databaseURL, LoadOptions.SetLineInfo);
                    }
                }
                catch (XmlException ex)
                {
                    Utils.appendToLog(string.Format("CRITICAL: Failed to read database: {0}\nMessage: {1}", databaseURL, ex.Message));
                    MessageBox.Show(Translations.getTranslatedString("databaseReadFailed"));
                    Application.Exit();
                    return;
                }
                catch (Exception ex)
                {
                    Utils.exceptionLog("createModStructure", string.Format("tried to access {0}", databaseURL), ex);
                    MessageBox.Show(Translations.getTranslatedString("databaseNotFound"));
                    Application.Exit();
                    return;
                }
                //add the global dependencies
                foreach (XElement dependencyNode in doc.XPathSelectElements("/modInfoAlpha.xml/globaldependencies/globaldependency"))
                {
                    string[] depNodeList = new string[] { "dependencyZipFile", "dependencyZipCRC", "startAddress", "endAddress", "dependencyenabled", "appendExtraction", "packageName", "devURL", "timestamp" };
                    Dependency d = new Dependency();
                    d.packageName = "";
                    foreach (XElement globs in dependencyNode.Elements())
                    {
                        depNodeList = depNodeList.Except(new string[] { globs.Name.ToString() }).ToArray();
                        switch (globs.Name.ToString())
                        {
                            case "dependencyZipFile":
                                d.dependencyZipFile = globs.Value;
                                break;
                            case "timestamp":
                                d.timestamp = long.Parse("0" + globs.Value);
                                break;
                            case "dependencyZipCRC":
                                d.dependencyZipCRC = globs.Value;
                                break;
                            case "startAddress":
                                d.startAddress = globs.Value;
                                break;
                            case "endAddress":
                                d.endAddress = globs.Value;
                                break;
                            case "devURL":
                                d.devURL = globs.Value;
                                break;
                            case "dependencyenabled":
                                d.enabled = Utils.parseBool(globs.Value, false);
                                break;
                            case "appendExtraction":
                                d.appendExtraction = Utils.parseBool(globs.Value, false);
                                break;
                            case "packageName":
                                d.packageName = globs.Value.Trim();
                                if (d.packageName.Equals(""))
                                {
                                    Utils.appendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => globsPend {1} (line {2})", globs.Name.ToString(), d.dependencyZipFile, ((IXmlLineInfo)globs).LineNumber));
                                    if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\" => globsPend {1}\n\nmore informations, see logfile", globs.Name.ToString(), d.dependencyZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                }
                                break;
                            default:
                                Utils.appendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => globsPend {1} (line {2})", globs.Name.ToString(), d.dependencyZipFile, ((IXmlLineInfo)globs).LineNumber));
                                if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: dependencyZipFile, dependencyZipCRC, startAddress, endAddress, dependencyenabled, packageName\n\nNode found: {0}\n\nmore informations, see logfile", globs.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                break;
                        }
                    }
                    if (d != null)
                    {
                        if (depNodeList.Length > 0) { Utils.appendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => globsPend {1} (line {2})", string.Join(",", depNodeList), d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber)); };
                        if (d.packageName.Equals("")) { string rad = Utils.RandomString(30);  d.packageName = rad; Utils.appendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                        globalDependencies.Add(d);
                    };
                }
                //add the dependencies
                foreach (XElement dependencyNode in doc.XPathSelectElements("/modInfoAlpha.xml/dependencies/dependency"))
                {
                    string[] depNodeList = new string[] { "dependencyZipFile", "dependencyZipCRC", "startAddress", "endAddress", "dependencyenabled", "appendExtraction", "packageName", "logicalDependencies", "devURL", "timestamp", "shortCuts" };
                    Dependency d = new Dependency();
                    d.packageName = "";
                    foreach (XElement globs in dependencyNode.Elements())
                    {
                        depNodeList = depNodeList.Except(new string[] { globs.Name.ToString() }).ToArray();
                        switch (globs.Name.ToString())
                        {
                            case "dependencyZipFile":
                                d.dependencyZipFile = globs.Value;
                                break;
                            case "timestamp":
                                d.timestamp = long.Parse("0" + globs.Value);
                                break;
                            case "dependencyZipCRC":
                                d.dependencyZipCRC = globs.Value;
                                break;
                            case "startAddress":
                                d.startAddress = globs.Value;
                                break;
                            case "endAddress":
                                d.endAddress = globs.Value;
                                break;
                            case "devURL":
                                d.devURL = globs.Value;
                                break;
                            case "dependencyenabled":
                                d.enabled = Utils.parseBool(globs.Value, false);
                                break;
                            case "appendExtraction":
                                d.appendExtraction = Utils.parseBool(globs.Value, false);
                                break;
                            case "packageName":
                                d.packageName = globs.Value.Trim();
                                if (d.packageName.Equals(""))
                                {
                                    Utils.appendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => globsPend {1} (line {2})", globs.Name.ToString(), d.dependencyZipFile, ((IXmlLineInfo)globs).LineNumber));
                                    if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\" => globsPend {1}\n\nmore informations, see logfile", globs.Name.ToString(), d.dependencyZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                }
                                break;
                            case "shortCuts":
                                //parse all shortCuts
                                foreach (XElement shortCutHolder in globs.Elements())
                                {
                                    ShortCut sc = new ShortCut();
                                    string[] depScNodeList = new string[] { "path", "name", "enabled" };
                                    foreach (XElement shortCutNode in shortCutHolder.Elements())
                                    {
                                        depScNodeList = depScNodeList.Except(new string[] { shortCutNode.Name.ToString() }).ToArray();
                                        switch (shortCutNode.Name.ToString())
                                        {
                                            case "path":
                                                sc.path = shortCutNode.Value;
                                                break;
                                            case "name":
                                                sc.name = shortCutNode.Value;
                                                break;
                                            case "enabled":
                                                sc.enabled = Utils.parseBool(shortCutNode.Value, false);
                                                break;
                                        }
                                    }
                                    if (sc != null)
                                    {
                                        if (depScNodeList.Length > 0) { Utils.appendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => Dep {1} (line {2})", string.Join(",", depScNodeList), d.dependencyZipFile, ((IXmlLineInfo)shortCutHolder).LineNumber)); };
                                        d.shortCuts.Add(sc);
                                    }
                                }
                                break;
                            case "logicalDependencies":
                                //parse all dependencies
                                foreach (XElement logDependencyHolder in globs.Elements())
                                {
                                    string[] logDepNodeList = new string[] { "packageName", "negateFlag" };
                                    LogicalDependnecy ld = new LogicalDependnecy();
                                    ld.packageName = "";
                                    foreach (XElement logDependencyNode in logDependencyHolder.Elements())
                                    {
                                        logDepNodeList = logDepNodeList.Except(new string[] { logDependencyNode.Name.ToString() }).ToArray();
                                        switch (logDependencyNode.Name.ToString())
                                        {
                                            case "packageName":
                                                ld.packageName = logDependencyNode.Value.Trim();
                                                if (ld.packageName.Equals(""))
                                                {
                                                    Utils.appendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => logDep {1} (line {2})", logDependencyNode.Name.ToString(), d.dependencyZipFile, ((IXmlLineInfo)logDependencyNode).LineNumber));
                                                    if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\"  => dep {1}", logDependencyNode.Name.ToString(), d.dependencyZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                }
                                                break;
                                            case "negateFlag":
                                                ld.negateFlag = Utils.parseBool(logDependencyNode.Value, true);
                                                break;
                                            default:
                                                Utils.appendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => logDep {1} (line {2})", logDependencyNode.Name.ToString(), d.dependencyZipFile, ((IXmlLineInfo)logDependencyNode).LineNumber));
                                                if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: packageName\n\nNode found: {0}\n\nmore informations, see logfile", logDependencyNode.Name.ToString())); };
                                                break;
                                        }
                                    }
                                    if (ld != null)
                                    {
                                        if (logDepNodeList.Length > 0) { Utils.appendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => logDep {1} (line {2})", string.Join(",", logDepNodeList), ld.dependencyZipFile, ((IXmlLineInfo)logDependencyHolder).LineNumber)); };
                                        if (ld.packageName.Equals("")) { string rad = Utils.RandomString(30); ld.packageName = rad; Utils.appendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                                        d.logicalDependencies.Add(ld);
                                    };
                                }
                                break;
                            default:
                                Utils.appendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => globsPend {1} (line {2})", globs.Name.ToString(), d.dependencyZipFile, ((IXmlLineInfo)globs).LineNumber));
                                if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: dependencyZipFile, dependencyZipCRC, startAddress, endAddress, dependencyenabled, packageName\n\nNode found: {0}\n\nmore informations, see logfile", globs.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                break;
                        }
                    }
                    if (d != null)
                    {
                        if (depNodeList.Length > 0) { Utils.appendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => globsPend {1} (line {2})", string.Join(",", depNodeList), d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber)); };
                        if (d.packageName.Equals("")) { string rad = Utils.RandomString(30); d.packageName = rad; Utils.appendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                        dependencies.Add(d);
                    };
                }
                //add the logicalDependencies (TODO)
                foreach (XElement dependencyNode in doc.XPathSelectElements("/modInfoAlpha.xml/logicalDependencies/logicalDependency"))
                {
                    string[] depNodeList = new string[] { "dependencyZipFile", "dependencyZipCRC", "startAddress", "endAddress", "dependencyenabled", "packageName", "devURL", "timestamp" };
                    LogicalDependnecy d = new LogicalDependnecy();
                    d.packageName = "";
                    foreach (XElement globs in dependencyNode.Elements())
                    {
                        depNodeList = depNodeList.Except(new string[] { globs.Name.ToString() }).ToArray();
                        switch (globs.Name.ToString())
                        {
                            case "dependencyZipFile":
                                d.dependencyZipFile = globs.Value;
                                break;
                            case "timestamp":
                                d.timestamp = long.Parse("0" + globs.Value);
                                break;
                            case "dependencyZipCRC":
                                d.dependencyZipCRC = globs.Value;
                                break;
                            case "startAddress":
                                d.startAddress = globs.Value;
                                break;
                            case "endAddress":
                                d.endAddress = globs.Value;
                                break;
                            case "devURL":
                                d.devURL = globs.Value;
                                break;
                            case "dependencyenabled":
                                d.enabled = Utils.parseBool(globs.Value, false);
                                break;
                            case "shortCuts":
                                //parse all shortCuts
                                foreach (XElement shortCutHolder in globs.Elements())
                                {
                                    ShortCut sc = new ShortCut();
                                    string[] logDepScNodeList = new string[] { "path", "name", "enabled" };
                                    foreach (XElement shortCutNode in shortCutHolder.Elements())
                                    {
                                        logDepScNodeList = logDepScNodeList.Except(new string[] { shortCutNode.Name.ToString() }).ToArray();
                                        switch (shortCutNode.Name.ToString())
                                        {
                                            case "path":
                                                sc.path = shortCutNode.Value;
                                                break;
                                            case "name":
                                                sc.name = shortCutNode.Value;
                                                break;
                                            case "enabled":
                                                sc.enabled = Utils.parseBool(shortCutNode.Value, false);
                                                break;
                                        }
                                    }
                                    if (sc != null)
                                    {
                                        if (logDepScNodeList.Length > 0) { Utils.appendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => logDep {1} (line {2})", string.Join(",", logDepScNodeList), d.dependencyZipFile, ((IXmlLineInfo)shortCutHolder).LineNumber)); };
                                        d.shortCuts.Add(sc);
                                    }
                                }
                                break;
                            case "packageName":
                                d.packageName = globs.Value.Trim();
                                if (d.packageName.Equals(""))
                                {
                                    Utils.appendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => logDep {1} (line {2})", globs.Name.ToString(), d.dependencyZipFile, ((IXmlLineInfo)globs).LineNumber));
                                    if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\" => logDep {1}\n\nmore informations, see logfile", globs.Name.ToString(), d.dependencyZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                }
                                break;
                            default:
                                Utils.appendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => logDep {1} (line {2})", globs.Name.ToString(), d.dependencyZipFile, ((IXmlLineInfo)globs).LineNumber));
                                if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: dependencyZipFile, dependencyZipCRC, startAddress, endAddress, dependencyenabled, packageName\n\nNode found: {0}\n\nmore informations, see logfile", globs.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                break;
                        }
                    }
                    if (d != null)
                    {
                        if (depNodeList.Length > 0) { Utils.appendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => globsPend {1} (line {2})", string.Join(",", depNodeList), d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber)); };
                        if (d.packageName.Equals("")) { string rad = Utils.RandomString(30); d.packageName = rad; Utils.appendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                        logicalDependencies.Add(d);
                    };
                }
                foreach (XElement catagoryHolder in doc.XPathSelectElements("/modInfoAlpha.xml/catagories/catagory"))
                {
                    Category cat = new Category();
                    string[] catNodeList = new string[] { "name", "selectionType", "mods", "dependencies" };
                    foreach (XElement catagoryNode in catagoryHolder.Elements())
                    {
                        catNodeList = catNodeList.Except(new string[] { catagoryNode.Name.ToString() }).ToArray();
                        switch (catagoryNode.Name.ToString())
                        {
                            case "name":
                                cat.name = catagoryNode.Value;
                                break;
                            case "selectionType":
                                cat.selectionType = catagoryNode.Value;
                                break;
                            case "mods":
                                foreach (XElement modHolder in catagoryNode.Elements())
                                {
                                    switch (modHolder.Name.ToString())
                                    {
                                        case "mod":
                                            string[] modNodeList = new string[] { "name", "version", "zipFile", "timestamp", "startAddress", "endAddress", "crc", "enabled", "visible", "packageName", "size", "description", "updateComment", "devURL", "userDatas", "pictures", "dependencies", "logicalDependencies", "configs" };
                                            Mod m = new Mod();
                                            m.packageName = "";
                                            foreach (XElement modNode in modHolder.Elements())
                                            {
                                                modNodeList = modNodeList.Except(new string[] { modNode.Name.ToString() }).ToArray();
                                                switch (modNode.Name.ToString())
                                                {
                                                    case "name":
                                                        m.name = modNode.Value;
                                                        totalModConfigComponents++;
                                                        break;
                                                    case "version":
                                                        m.version = modNode.Value;
                                                        break;
                                                    case "zipFile":
                                                        m.zipFile = modNode.Value;
                                                        break;
                                                    case "timestamp":
                                                        m.timestamp = long.Parse("0" + modNode.Value);
                                                        break;
                                                    case "startAddress":
                                                        m.startAddress = modNode.Value;
                                                        break;
                                                    case "endAddress":
                                                        m.endAddress = modNode.Value;
                                                        break;
                                                    case "crc":
                                                        m.crc = modNode.Value;
                                                        break;
                                                    case "enabled":
                                                        m.enabled = Utils.parseBool(modNode.Value, false);
                                                        break;
                                                    case "visible":
                                                        m.visible = Utils.parseBool(modNode.Value, true);
                                                        break;
                                                    case "packageName":
                                                        m.packageName = modNode.Value.Trim();
                                                        if (m.packageName.Equals(""))
                                                        {
                                                            Utils.appendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => mod {1} ({2}) (line {3})", modNode.Name.ToString(), m.name, m.zipFile, ((IXmlLineInfo)modNode).LineNumber));
                                                            if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\" => mod {1} ({2})", modNode.Name.ToString(), m.name, m.zipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                        }
                                                        break;
                                                    case "size":
                                                        // m.size = Utils.parseFloat(modNode.Value, 0.0f);
                                                        m.size = Utils.parseInt(modNode.Value, 0);
                                                        break;
                                                    case "description":
                                                        m.description = convertFromXmlSaveFormat(modNode.Value);
                                                        break;
                                                    case "updateComment":
                                                        m.updateComment = convertFromXmlSaveFormat(modNode.Value);
                                                        break;
                                                    case "devURL":
                                                        m.devURL = modNode.Value;
                                                        break;
                                                    case "userDatas":
                                                        foreach (XElement userDataNode in modNode.Elements())
                                                        {
                                                            switch (userDataNode.Name.ToString())
                                                            {
                                                                case "userData":
                                                                    string innerText = userDataNode.Value;
                                                                    if (innerText == null)
                                                                        continue;
                                                                    if (innerText.Equals(""))
                                                                        continue;
                                                                    m.userFiles.Add(innerText);
                                                                    break;
                                                                default:
                                                                    Utils.appendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => userDatas => expected node: userData (line {3})", userDataNode.Name.ToString(), m.name, m.zipFile, ((IXmlLineInfo)userDataNode).LineNumber));
                                                                    if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected node: userData\n\nNode found: {0}\n\nmore informations, see logfile", userDataNode.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                                    break;
                                                            }
                                                        }
                                                        break;
                                                    case "pictures":
                                                        //parse every picture
                                                        foreach (XElement pictureHolder in modNode.Elements())
                                                        {
                                                            Media med = new Media(null, null);
                                                            switch (pictureHolder.Name.ToString())
                                                            {
                                                                case "picture":
                                                                    foreach (XElement pictureNode in pictureHolder.Elements())
                                                                    {
                                                                        switch (pictureNode.Name.ToString())
                                                                        {
                                                                            case "URL":
                                                                                string innerText = pictureNode.Value;
                                                                                if (innerText == null)
                                                                                    continue;
                                                                                if (innerText.Equals(""))
                                                                                    continue;
                                                                                med.name = m.name;
                                                                                med.URL = innerText;
                                                                                break;
                                                                            case "type":
                                                                                int innerValue = Utils.parseInt(pictureNode.Value, 1);
                                                                                switch (innerValue) { 
                                                                                    case 1:
                                                                                        med.mediaType = MediaType.picture;
                                                                                        break;
                                                                                    case 2:
                                                                                        med.mediaType = MediaType.youtube;
                                                                                        break;
                                                                                }
                                                                                break;
                                                                            default:
                                                                                Utils.appendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => pictures => picture =>expected nodes: URL (line {3})", pictureNode.Name.ToString(), m.name, m.zipFile, ((IXmlLineInfo)pictureNode).LineNumber));
                                                                                if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: URL\n\nNode found: {0}\n\nmore informations, see logfile", pictureNode.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                                                break;
                                                                        }
                                                                    }
                                                                    break;
                                                                default:
                                                                    Utils.appendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => pictures => expected node: picture (line {3})", pictureHolder.Name, m.name, m.zipFile, ((IXmlLineInfo)pictureHolder).LineNumber));
                                                                    if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected node: picture\n\nNode found: {0}\n\nmore informations, see logfile", pictureHolder.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                                    break;
                                                            }
                                                            m.pictureList.Add(med);
                                                        }
                                                        break;
                                                    case "shortCuts":
                                                        //parse all shortCuts
                                                        foreach (XElement shortCutHolder in modNode.Elements())
                                                        {
                                                            ShortCut sc = new ShortCut();
                                                            string[] depScNodeList = new string[] { "path", "name", "enabled" };
                                                            foreach (XElement shortCutNode in shortCutHolder.Elements())
                                                            {
                                                                depScNodeList = depScNodeList.Except(new string[] { shortCutNode.Name.ToString() }).ToArray();
                                                                switch (shortCutNode.Name.ToString())
                                                                {
                                                                    case "path":
                                                                        sc.path = shortCutNode.Value;
                                                                        break;
                                                                    case "name":
                                                                        sc.name = shortCutNode.Value;
                                                                        break;
                                                                    case "enabled":
                                                                        sc.enabled = Utils.parseBool(shortCutNode.Value, false);
                                                                        break;
                                                                }
                                                            }
                                                            if (sc != null)
                                                            {
                                                                if (depScNodeList.Length > 0) { Utils.appendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => mod {1} (line {2})", string.Join(",", depScNodeList), m.zipFile, ((IXmlLineInfo)shortCutHolder).LineNumber)); };
                                                                m.shortCuts.Add(sc);
                                                            }
                                                        }
                                                        break;
                                                    case "dependencies":
                                                        //parse all dependencies
                                                        foreach (XElement dependencyHolder in modNode.Elements())
                                                        {
                                                            string[] depNodeList = new string[] { "packageName" };
                                                            Dependency d = new Dependency();
                                                            d.packageName = "";
                                                            foreach (XElement dependencyNode in dependencyHolder.Elements())
                                                            {
                                                                depNodeList = depNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
                                                                switch (dependencyNode.Name.ToString())
                                                                {
                                                                    case "packageName":
                                                                        d.packageName = dependencyNode.Value.Trim();
                                                                        if (d.packageName.Equals(""))
                                                                        {
                                                                            Utils.appendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => mod {1} ({2}) => dep {3} (line {4})", dependencyNode.Name.ToString(), m.name, m.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                                            if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\" => mod {1} ({2}) => dep {3}", dependencyNode.Name.ToString(), m.name, m.zipFile, d.dependencyZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                                        }
                                                                        break;
                                                                    default:
                                                                        Utils.appendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => dep {3} (line {4})", dependencyNode.Name.ToString(), m.name, m.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                                        if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: packageName\n\nNode found: {0}\n\nmore informations, see logfile", dependencyNode.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                                        break;
                                                                }
                                                            }
                                                            if (d != null)
                                                            {
                                                                if (depNodeList.Length > 0) { Utils.appendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => mod {1} ({2}) => dep {3} (line {4})", string.Join(",", depNodeList), m.name, m.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyHolder).LineNumber)); };
                                                                if (d.packageName.Equals("")) { string rad = Utils.RandomString(30); d.packageName = rad; Utils.appendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                                                                m.dependencies.Add(d);
                                                            };
                                                        }
                                                        break;
                                                    case "logicalDependencies":
                                                        //parse all dependencies
                                                        foreach (XElement dependencyHolder in modNode.Elements())
                                                        {
                                                            string[] depNodeList = new string[] { "packageName", "negateFlag" };
                                                            LogicalDependnecy d = new LogicalDependnecy();
                                                            d.packageName = "";
                                                            foreach (XElement dependencyNode in dependencyHolder.Elements())
                                                            {
                                                                depNodeList = depNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
                                                                switch (dependencyNode.Name.ToString())
                                                                {
                                                                    case "packageName":
                                                                        d.packageName = dependencyNode.Value.Trim();
                                                                        if (d.packageName.Equals(""))
                                                                        {
                                                                            Utils.appendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => config {1} ({2}) => dep {3} (line {4})", dependencyNode.Name.ToString(), m.name, m.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                                            if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\" => config {1} ({2}) => dep {3}", dependencyNode.Name.ToString(), m.name, m.zipFile, d.dependencyZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                                        }
                                                                        break;
                                                                    case "negateFlag":
                                                                        d.negateFlag = Utils.parseBool(dependencyNode.Value, true);
                                                                        break;
                                                                    default:
                                                                        Utils.appendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => config {1} ({2}) => dep {3} (line {4})", dependencyNode.Name.ToString(), m.name, m.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                                        if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: packageName\n\nNode found: {0}\n\nmore informations, see logfile", dependencyNode.Name.ToString())); };
                                                                        break;
                                                                }
                                                            }
                                                            if (d != null)
                                                            {
                                                                if (depNodeList.Length > 0) { Utils.appendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => config {1} ({2}) => dep {3} (line {4})", string.Join(",", depNodeList), m.name, m.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyHolder).LineNumber)); };
                                                                if (d.packageName.Equals("")) { string rad = Utils.RandomString(30); d.packageName = rad; Utils.appendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                                                                m.logicalDependencies.Add(d);
                                                            };
                                                        }
                                                        break;
                                                    case "configs":
                                                        //run the process configs method
                                                        Utils.processConfigs(modNode, m, true);
                                                        break;
                                                    default:
                                                        Utils.appendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) (line {3})", modNode.Name.ToString(), m.name, m.zipFile, ((IXmlLineInfo)modNode).LineNumber));
                                                        if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: name, version, zipFile, startAddress, endAddress, crc, enabled, packageName, size, description, updateComment, devURL, userDatas, pictures, dependencies, configs\n\nNode found: {0}\n\nmore informations, see logfile", modNode.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                        break;
                                                }
                                            }
                                            if (m != null)
                                            {
                                                if (modNodeList.Length > 0) { Utils.appendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => mod {1} ({2}) (line {3})", string.Join(",", modNodeList), m.name, m.zipFile, ((IXmlLineInfo)modHolder).LineNumber)); };
                                                if (m.packageName.Equals("")) { string rad = Utils.RandomString(30); m.packageName = rad; Utils.appendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                                                cat.mods.Add(m);
                                            };
                                            break;
                                        default:
                                            Utils.appendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => cat {1} (line {2})", modHolder.Name.ToString(), cat.name, ((IXmlLineInfo)modHolder).LineNumber));
                                            if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: mod\n\nNode found: {0}\n\nmore informations, see logfile", modHolder.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                            break;
                                    }
                                }
                                break;
                            case "dependencies":
                                //parse every dependency for that mod
                                foreach (XElement dependencyHolder in catagoryNode.Elements())
                                {
                                    string[] depNodeList = new string[] { "packageName" };
                                    Dependency d = new Dependency();
                                    d.packageName = "";
                                    foreach (XElement dependencyNode in dependencyHolder.Elements())
                                    {
                                        depNodeList = depNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
                                        switch (dependencyNode.Name.ToString())
                                        {
                                            case "packageName":
                                                d.packageName = dependencyNode.Value.Trim();
                                                if (d.packageName.Equals(""))
                                                {
                                                    Utils.appendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => cat {1} => dep {2} (line {3})", dependencyNode.Name.ToString(), cat.name, d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                    if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\" => cat {1} => dep {2}", dependencyNode.Name.ToString(), cat.name, d.dependencyZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                }
                                                break;
                                            default:
                                                Utils.appendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => cat {1} => dep {2} (line {3})", dependencyNode.Name, cat.name, d.dependencyZipFile,((IXmlLineInfo)dependencyNode).LineNumber));
                                                if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: packageName\n\nNode found: {0}\n\nmore informations, see logfile", dependencyNode.Name), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                break;
                                        }
                                    }
                                    if (d != null)
                                    {
                                        if (depNodeList.Length > 0) { Utils.appendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => cat {1} => dep {2} (line {3})", string.Join(",", depNodeList), cat.name, d.dependencyZipFile, ((IXmlLineInfo)dependencyHolder).LineNumber)); };
                                        if (d.packageName.Equals("")) { string rad = Utils.RandomString(30); d.packageName = rad; Utils.appendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                                        cat.dependencies.Add(d);
                                    };
                                }
                                break;
                            default:
                                Utils.appendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => cat {1} (line {2})", catagoryNode.Name.ToString(), cat.name, ((IXmlLineInfo)catagoryNode).LineNumber));
                                if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: name, selectionType, mods, dependencies\n\nNode found: {0}\n\nmore informations, see logfile", catagoryNode.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                break;
                        }
                    }
                    if (cat != null)
                    {
                        if (catNodeList.Length > 0) { Utils.appendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => cat {1} (line {2})", string.Join(",", catNodeList), cat.name, ((IXmlLineInfo)catagoryHolder).LineNumber)); };
                        parsedCatagoryList.Add(cat);
                    };
                }
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("createModStructure", ex);
            }
        }

        //recursivly processes the configs
        public static void processConfigs(XElement holder, Mod m, bool parentIsMod, Config con = null)
        {
            try
            {
                //parse every config for that mod
                foreach (XElement configHolder in holder.Elements())
                {
                    switch (configHolder.Name.ToString())
                    {
                        case "config":
                            string[] confNodeList = new string[] { "name", "version", "zipFile", "timestamp", "startAddress", "endAddress", "crc", "enabled", "visible", "packageName", "size", "updateComment", "description", "devURL", "type", "configs", "userDatas", "pictures", "dependencies", "logicalDependencies" };
                            Config c = new Config();
                            c.packageName = "";
                            foreach (XElement configNode in configHolder.Elements())
                            {
                                confNodeList = confNodeList.Except(new string[] { configNode.Name.ToString() }).ToArray();
                                switch (configNode.Name.ToString())
                                {
                                    case "name":
                                        c.name = configNode.Value;
                                        break;
                                    case "version":
                                        c.version = configNode.Value;
                                        break;
                                    case "zipFile":
                                        c.zipFile = configNode.Value;
                                        break;
                                    case "timestamp":
                                        c.timestamp = long.Parse("0" + configNode.Value);
                                        break;
                                    case "startAddress":
                                        c.startAddress = configNode.Value;
                                        break;
                                    case "endAddress":
                                        c.endAddress = configNode.Value;
                                        break;
                                    case "crc":
                                        c.crc = configNode.Value;
                                        break;
                                    case "enabled":
                                        c.enabled = Utils.parseBool(configNode.Value, false);
                                        break;
                                    case "visible":
                                        c.visible = Utils.parseBool(configNode.Value, true);
                                        break;
                                    case "packageName":
                                        c.packageName = configNode.Value.Trim();
                                        if (c.packageName.Equals(""))
                                        {
                                            Utils.appendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => config {1} ({2}) (line {3})", configNode.Name.ToString(), c.name, c.zipFile, ((IXmlLineInfo)configNode).LineNumber));
                                            if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\" => config {1} ({2})", configNode.Name.ToString(), c.name, c.zipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                        }
                                        break;
                                    case "size":
                                        c.size = Utils.parseInt(configNode.Value, 0);
                                        break;
                                    case "updateComment":
                                        c.updateComment = convertFromXmlSaveFormat(configNode.Value);
                                        break;
                                    case "description":
                                        c.description = convertFromXmlSaveFormat(configNode.Value);
                                        break;
                                    case "devURL":
                                        c.devURL = configNode.Value;
                                        break;
                                    case "type":
                                        c.type = configNode.Value;
                                        break;
                                    case "configs":
                                        Utils.processConfigs(configNode, m, false, c);
                                        break;
                                    case "userDatas":
                                        foreach (XElement userDataNode in configNode.Elements())
                                        {
                                            switch (userDataNode.Name.ToString())
                                            {
                                                case "userData":
                                                    string innerText = userDataNode.Value;
                                                    if (innerText == null)
                                                        continue;
                                                    if (innerText.Equals(""))
                                                        continue;
                                                    c.userFiles.Add(innerText);
                                                    break;
                                                default:
                                                    Utils.appendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => config {1} ({2}) => userDatas => expected nodes: userData (line {3})", userDataNode.Name.ToString(), c.name, c.zipFile, ((IXmlLineInfo)configNode).LineNumber));
                                                    if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: userData\n\nNode found: {0}\n\nmore informations, see logfile", userDataNode.Name.ToString())); };
                                                    break;
                                            }
                                        }
                                        break;
                                    case "pictures":
                                        //parse every picture
                                        foreach (XElement pictureHolder in configNode.Elements())
                                        {
                                            Media med = new Media(null, null);
                                            switch (pictureHolder.Name.ToString())
                                            {
                                                case "picture":
                                                    foreach (XElement pictureNode in pictureHolder.Elements())
                                                    {
                                                        switch (pictureNode.Name.ToString())
                                                        {
                                                            case "URL":
                                                                string innerText = pictureNode.Value;
                                                                if (innerText == null)
                                                                    continue;
                                                                if (innerText.Equals(""))
                                                                    continue;
                                                                med.name = c.name;
                                                                med.URL = innerText;
                                                                break;
                                                            case "type":
                                                                int innerValue = Utils.parseInt(pictureNode.Value, 1);
                                                                switch (innerValue)
                                                                {
                                                                    case 1:
                                                                        med.mediaType = MediaType.picture;
                                                                        break;
                                                                    case 2:
                                                                        med.mediaType = MediaType.youtube;
                                                                        break;
                                                                }
                                                                break;
                                                            default:
                                                                Utils.appendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => pictures => picture =>expected nodes: URL (line {3})", pictureNode.Name.ToString(), m.name, m.zipFile, ((IXmlLineInfo)pictureNode).LineNumber));
                                                                if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: URL\n\nNode found: {0}\n\nmore informations, see logfile", pictureNode.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                                break;
                                                        }
                                                    }
                                                    break;
                                                default:
                                                    Utils.appendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => pictures => expected node: picture (line {3})", pictureHolder.Name, m.name, m.zipFile, ((IXmlLineInfo)pictureHolder).LineNumber));
                                                    if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected node: picture\n\nNode found: {0}\n\nmore informations, see logfile", pictureHolder.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                    break;
                                            }
                                            c.pictureList.Add(med);
                                        }
                                        break;
                                    case "shortCuts":
                                        //parse all shortCuts
                                        foreach (XElement shortCutHolder in configNode.Elements())
                                        {
                                            ShortCut sc = new ShortCut();
                                            string[] cScNodeList = new string[] { "path", "name", "enabled" };
                                            foreach (XElement shortCutNode in shortCutHolder.Elements())
                                            {
                                                cScNodeList = cScNodeList.Except(new string[] { shortCutNode.Name.ToString() }).ToArray();
                                                switch (shortCutNode.Name.ToString())
                                                {
                                                    case "path":
                                                        sc.path = shortCutNode.Value;
                                                        break;
                                                    case "name":
                                                        sc.name = shortCutNode.Value;
                                                        break;
                                                    case "enabled":
                                                        sc.enabled = Utils.parseBool(shortCutNode.Value, false);
                                                        break;
                                                }
                                            }
                                            if (sc != null)
                                            {
                                                if (cScNodeList.Length > 0) { Utils.appendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => mod {1} (line {2})", string.Join(",", cScNodeList), c.zipFile, ((IXmlLineInfo)shortCutHolder).LineNumber)); };
                                                c.shortCuts.Add(sc);
                                            }
                                        }
                                        break;
                                    case "dependencies":
                                        //parse all dependencies
                                        foreach (XElement dependencyHolder in configNode.Elements())
                                        {
                                            string[] depNodeList = new string[] { "packageName" };
                                            Dependency d = new Dependency();
                                            d.packageName = "";
                                            foreach (XElement dependencyNode in dependencyHolder.Elements())
                                            {
                                                depNodeList = depNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
                                                switch (dependencyNode.Name.ToString())
                                                {
                                                    case "packageName":
                                                        d.packageName = dependencyNode.Value.Trim();
                                                        if (d.packageName.Equals(""))
                                                        {
                                                            Utils.appendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => config {1} ({2}) => dep {3} (line {4})", dependencyNode.Name.ToString(), c.name, c.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                            if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\" => config {1} ({2}) => dep {3}", dependencyNode.Name.ToString(), c.name, c.zipFile, d.dependencyZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                        }
                                                        break;
                                                    default:
                                                        Utils.appendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => config {1} ({2}) => dep {3} (line {4})", dependencyNode.Name.ToString(), c.name, c.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                        if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: packageName\n\nNode found: {0}\n\nmore informations, see logfile", dependencyNode.Name.ToString())); };
                                                        break;
                                                }
                                            }
                                            if (d != null)
                                            {
                                                if (depNodeList.Length > 0) { Utils.appendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => config {1} ({2}) => dep {3} (line {4})", string.Join(",", depNodeList), c.name, c.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyHolder).LineNumber)); };
                                                if (d.packageName.Equals("")) { string rad = Utils.RandomString(30); d.packageName = rad; Utils.appendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                                                c.dependencies.Add(d);
                                            };
                                        }
                                        break;
                                    case "logicalDependencies":
                                        //parse all dependencies
                                        foreach (XElement dependencyHolder in configNode.Elements())
                                        {
                                            string[] depNodeList = new string[] { "packageName", "negateFlag" };
                                            LogicalDependnecy d = new LogicalDependnecy();
                                            d.packageName = "";
                                            foreach (XElement dependencyNode in dependencyHolder.Elements())
                                            {
                                                depNodeList = depNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
                                                switch (dependencyNode.Name.ToString())
                                                {
                                                    case "packageName":
                                                        d.packageName = dependencyNode.Value.Trim();
                                                        if (d.packageName.Equals(""))
                                                        {
                                                            Utils.appendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => config {1} ({2}) => dep {3} (line {4})", dependencyNode.Name.ToString(), c.name, c.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                            if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\" => config {1} ({2}) => dep {3}", dependencyNode.Name.ToString(), c.name, c.zipFile, d.dependencyZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                        }
                                                        break;
                                                    case "negateFlag":
                                                        d.negateFlag = Utils.parseBool(dependencyNode.Value, true);
                                                        break;
                                                    default:
                                                        Utils.appendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => config {1} ({2}) => dep {3} (line {4})", dependencyNode.Name.ToString(), c.name, c.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                        if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: packageName\n\nNode found: {0}\n\nmore informations, see logfile", dependencyNode.Name.ToString())); };
                                                        break;
                                                }
                                            }
                                            if (d != null)
                                            {
                                                if (depNodeList.Length > 0) { Utils.appendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => config {1} ({2}) => dep {3} (line {4})", string.Join(",", depNodeList), c.name, c.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyHolder).LineNumber)); };
                                                if (d.packageName.Equals("")) { string rad = Utils.RandomString(30); d.packageName = rad; Utils.appendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                                                c.logicalDependencies.Add(d);
                                            };
                                        }
                                        break;
                                    default:
                                        Utils.appendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => config {1} ({2}) (line {3})", configNode.Name.ToString(), c.name, c.zipFile, ((IXmlLineInfo)configNode).LineNumber));
                                        if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: name, version, zipFile, startAddress, endAddress, crc, enabled, packageName, size, updateComment, description, devURL, type, configs, userDatas, pictures, dependencies\n\nNode found: {0}\n\nmore informations, see logfile", configNode.Name.ToString())); };
                                        break;
                                }
                            }
                            if (c != null && confNodeList.Length > 0) { Utils.appendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => config {1} ({2}) (line {3})", string.Join(",", confNodeList), c.name, c.zipFile, ((IXmlLineInfo)configHolder).LineNumber)); };
                            if (c.packageName.Equals("")) { string rad = Utils.RandomString(30); c.packageName = rad; Utils.appendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                            //attach it to eithor the config of correct level or the mod
                            if (parentIsMod)
                                m.configs.Add(c);
                            else
                                con.configs.Add(c);
                            break;
                        default:
                            Utils.appendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => expected nodes: config (line {3})", configHolder.Name.ToString(), m.name, m.zipFile, ((IXmlLineInfo)configHolder).LineNumber));
                            if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: config\n\nNode found: {0}\n\nmore informations, see logfile", configHolder.Name)); };
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("processConfigs", ex);
            }
        }

        public class CheckStorage
        {
            public string packageName { get; set; }
            public string zipFile { get; set; }
            public bool dependency { get; set; }
            public int CheckDatabaseListIndex { get; set; }
            public CheckStorage() { }
        }

        public static void duplicatesPackageName_dependencyCheck(List<Dependency> dependencyList, List<CheckStorage> checkStorageList, ref int duplicatesCounter)
        {
            foreach (Dependency d in dependencyList)
            {
                foreach (CheckStorage s in checkStorageList)
                {
                    // if both s.CheckDatabaseListIndex AND m.CheckDatabaseListIndex are equal, it is checking his own entry, so SKIP EVERY check/test
                    // if the s.dependency is FALSE, it is a single mod/config and should only exists once, if not => error/duplicate message
                    // if the s.dependency is TRUE, it is a dependecy entry and packageName AND zipFile must be checken if equal, if not => error/duplicate message
                    if (s.CheckDatabaseListIndex != d.CheckDatabaseListIndex && ((s.packageName.Equals(d.packageName) && !(s.dependency)) || (s.dependency && s.packageName.Equals(d.packageName) && !s.zipFile.Equals(d.dependencyZipFile))))
                    {
                        Utils.appendToLog(string.Format("Error: duplicate packageName \"{0}\" found. zipFile: \"{1}\"", s.packageName, s.zipFile));
                        duplicatesCounter++;
                    }
                }
            }
        }

        public static void duplicatesPackageName_RecursiveSubConfigCheck(List<Config> subConfigList, List<CheckStorage> checkStorageList, ref int duplicatesCounter)
        {
            foreach (Config c in subConfigList)
            {
                foreach (CheckStorage s in checkStorageList)
                {
                    // if both s.CheckDatabaseListIndex AND m.CheckDatabaseListIndex are equal, it is checking his own entry, so SKIP EVERY check/test
                    // if the s.dependency is FALSE, it is a single mod/config and should only exists once, if not => error/duplicate message
                    // if the s.dependency is TRUE, it is a dependecy entry and packageName AND zipFile must be checken if equal, if not => error/duplicate message
                    if (s.CheckDatabaseListIndex != c.CheckDatabaseListIndex && ((s.packageName.Equals(c.packageName) && !(s.dependency)) || (s.dependency && s.packageName.Equals(c.packageName) && !s.zipFile.Equals(c.zipFile))))
                    {
                        Utils.appendToLog(string.Format("Error: duplicate packageName \"{0}\" found. zipFile: \"{1}\"", s.packageName, s.zipFile));
                        duplicatesCounter++;
                    }
                }
                if (c.configs.Count > 0)
                {
                    duplicatesPackageName_RecursiveSubConfigCheck(c.configs, checkStorageList, ref duplicatesCounter);
                }
            }
        }

        public static void duplicatesPackageName_dependencyRead(ref List<Dependency> dependencyList, ref List<CheckStorage> checkStorageList)
        {
            foreach (Dependency d in dependencyList)
            {
                CheckStorage cs = new CheckStorage();
                cs.packageName = d.packageName;
                cs.zipFile = d.dependencyZipFile;
                cs.dependency = true;
                cs.CheckDatabaseListIndex = checkStorageList.Count;
                d.CheckDatabaseListIndex = cs.CheckDatabaseListIndex;
                checkStorageList.Add(cs);
            }
        }

        public static void duplicatesPackageName_RecursiveSubConfigRead(ref List<Config> subConfigList, ref List<CheckStorage> checkStorageList)
        {
            foreach (Config c in subConfigList)
            {
                CheckStorage cs = new CheckStorage();
                cs.packageName = c.packageName;
                cs.zipFile = c.zipFile;
                cs.dependency = false;
                cs.CheckDatabaseListIndex = checkStorageList.Count;
                c.CheckDatabaseListIndex = cs.CheckDatabaseListIndex;
                checkStorageList.Add(cs);
                if (c.configs.Count > 0)
                {
                    duplicatesPackageName_RecursiveSubConfigRead(ref c.configs, ref checkStorageList);
                }
                if (c.dependencies.Count > 0)
                {
                    //duplicatesPackageName_dependencyRead(ref c.dependencies, ref checkStorageList);
                }
            }
        }

        //checks for duplicate packageName
        public static bool duplicatesPackageName(List<Category> parsedCatagoryList, ref int duplicatesCounter)
        {
            //add every mod and config name to a new list
            var checkStorageList = new List<CheckStorage>();
            foreach (Category c in parsedCatagoryList)
            {
                if (c.dependencies.Count > 0)
                {
                    //duplicatesPackageName_dependencyRead(ref c.dependencies, ref checkStorageList);
                }
                foreach (Mod m in c.mods)
                {
                    CheckStorage cs = new CheckStorage();
                    cs.packageName = m.packageName;
                    cs.zipFile = m.zipFile;
                    cs.dependency = false;
                    cs.CheckDatabaseListIndex = checkStorageList.Count;
                    m.CheckDatabaseListIndex = cs.CheckDatabaseListIndex;
                    checkStorageList.Add(cs);
                     if (m.configs.Count > 0)
                    {
                        duplicatesPackageName_RecursiveSubConfigRead(ref m.configs, ref checkStorageList);
                    }
                    if (m.dependencies.Count > 0)
                    {
                        //duplicatesPackageName_dependencyRead(ref m.dependencies, ref checkStorageList);
                    }
                }
            }
            //itterate through every mod name again
            foreach (Category c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    foreach (var s in checkStorageList)
                    {
                        // if both s.CheckDatabaseListIndex AND m.CheckDatabaseListIndex are equal, it is checking his own entry, so SKIP EVERY check/test
                        // if the s.dependency is FALSE, it is a single mod/config and should only exists once, if not => error/duplicate message
                        // if the s.dependency is TRUE, it is a dependecy entry and packageName AND zipFile must be checken if equal, if not => error/duplicate message
                        if (s.CheckDatabaseListIndex != m.CheckDatabaseListIndex && ((s.packageName.Equals(m.packageName) && !(s.dependency)) || (s.dependency && s.packageName.Equals(m.packageName) && !(s.zipFile.Equals(m.zipFile)))))
                        { 
                            Utils.appendToLog(string.Format("Error: duplicate packageName \"{0}\" found. zipFile: \"{1}\".", s.packageName, s.zipFile));
                            duplicatesCounter++;
                        }
                    }
                    if (m.configs.Count > 0)
                    {
                        duplicatesPackageName_RecursiveSubConfigCheck(m.configs, checkStorageList, ref duplicatesCounter);
                    }
                    if (m.dependencies.Count > 0)
                    {
                        //duplicatesPackageName_dependencyCheck(m.dependencies, checkStorageList, ref duplicatesCounter);
                    }
                }
                if (c.dependencies.Count > 0)
                {
                    //duplicatesPackageName_dependencyCheck(c.dependencies, checkStorageList, ref duplicatesCounter);
                }
            }
            if (duplicatesCounter > 0)
                return true;        //duplicate detected
            else
                return false;
        }

        //checks for duplicates
        public static bool duplicates(List<Category> parsedCatagoryList)
        {
            //add every mod name to a new list
            List<string> modNameList = new List<string>();
            foreach (Category c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    modNameList.Add(m.name);
                }
            }
            //itterate through every mod name again
            foreach (Category c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    //in theory, there should only be one matching mod name
                    //between the two lists. more indicates a duplicates
                    int i = 0;
                    foreach (string s in modNameList)
                    {
                        if (s.Equals(m.name))
                            i++;
                    }
                    if (i > 1)//if there are 2 or more matching mods
                        return true;//duplicate detected
                }
            }
            //making it here means there are no duplicates
            return false;
        }
        //returns the mod based on catagory and mod name
        public static Mod linkMod(string modName, string catagoryName, List<Category> parsedCatagoryList)
        {
            foreach (Category c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    if (c.name.Equals(catagoryName) && m.name.Equals(modName))
                    {
                        //found it
                        return m;
                    }
                }
            }
            return null;
        }
        //returns the mod based and mod name
        public static Mod linkMod(string modName, List<Category> parsedCatagoryList)
        {
            foreach (Category c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    if (m.name.Equals(modName))
                    {
                        //found it
                        return m;
                    }
                }
            }
            return null;
        }
        
         //returns the catagory based on the catagory name
        public static Category getCatagory(string catName, List<Category> parsedCatagoryList)
        {
            foreach (Category c in parsedCatagoryList)
            {
                if (c.name.Equals(catName)) return c;
            }
            return null;
        }
        //gets the user mod based on it's name
        public static Mod getUserMod(string modName, List<Mod> userMods)
        {
            foreach (Mod m in userMods)
            {
                if (m.name.Equals(modName))
                {
                    return m;
                }
            }
            return null;
        }
        //sorts a list of mods alphabetaicaly
        public static void sortModsList(List<Mod> modList)
        {
            //sortModsList
            modList.Sort(Mod.CompareMods);
        }
        //sorte a list of catagoris alphabetaicaly
        public static void sortCatagoryList(List<Category> catagoryList)
        {
            catagoryList.Sort(Category.CompareCatagories);
        }
        //sorts a list of pictures by mod or config, then name
        public static List<Media> sortPictureList(List<Media> pictureList)
        {
            //don't actually sort them anymore
            //they will not apprea in the order of which they were loaded from the xml file
            return pictureList;
        }
        //method to patch a part of an xml file
        //fileLocation is relative to res_mods folder
        public static void xmlPatch(string filePath, string xpath, string mode, string search, string replace, string tanksLocation, string tanksVersion, bool testMods = false, string testXVMBootLoc = "")
        {
            if (Regex.IsMatch(filePath, "^\\\\\\\\res_mods"))
            {
                //new style patch, res_mods folder
                filePath = tanksLocation + filePath;
            }
            else if (Regex.IsMatch(filePath, "^\\\\\\\\mods"))
            {
                //new style patch, mods folder
                filePath = tanksLocation + filePath;
            }
            else if (testMods)
            {

            }
            else
            {
                //old style patch
                filePath = tanksLocation + "\\res_mods" + filePath;
            }

            if (!testMods && Regex.IsMatch(filePath, "versiondir"))
            {
                //patch versiondir out of filePath
                filePath = Regex.Replace(filePath, "versiondir", tanksVersion);
            }
            //patch xvmConfigFolderName out of fileLocation
            if (!testMods && Regex.IsMatch(filePath, "xvmConfigFolderName"))
            {
                string s = getXVMBootLoc(tanksLocation);
                if (s != null)
                    filePath = Regex.Replace(filePath, "xvmConfigFolderName", s);
            }
            else
            {
                //patch check mode, try to get boot xvm file from the xvm boot textbox
                if (testXVMBootLoc.Equals("") && Regex.IsMatch(filePath, "xvmConfigFolderName"))
                {
                    MessageBox.Show("Attempted to use variable \"xvmConfigFolderName\", but nothing in the xvm boot file location text box");
                    return;
                }
            }

            //verify the file exists...
            if (!System.IO.File.Exists(filePath))
                return;
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            //check to see if it has the header info at the top to see if we need to remove it later
            bool hadHeader = false;
            XmlDocument doc3 = new XmlDocument();
            doc3.Load(filePath);
            foreach (XmlNode node in doc3)
            {
                if (node.NodeType == XmlNodeType.XmlDeclaration)
                {
                    hadHeader = true;
                }
            }
            //determines which version of pathing will be done
            switch (mode)
            {
                case "add":
                    //check to see if it's already there
                    string[] tempp = replace.Split('/');
                    string tempPath = xpath;
                    //make the full node path
                    for (int i = 0; i < tempp.Count() - 1; i++)
                    {
                        tempPath = tempPath + "/" + tempp[i];
                    }
                    XmlNodeList currentSoundBanksAdd = doc.SelectNodes(tempPath);
                    //in each node check if the element exist with the replace innerText
                    foreach (XmlElement e in currentSoundBanksAdd)
                    {
                        string innerText = tempp[tempp.Count() - 1];
                        //remove any tabs and whitespaces first
                        innerText = Regex.Replace(innerText, @"\t", "");
                        innerText = innerText.Trim();
                        if (e.InnerText.Equals(innerText))
                            return;
                    }
                    //get to the node where to add the element
                    XmlNode reff = doc.SelectSingleNode(xpath);
                    //create node(s) to add to the element
                    string[] temp = replace.Split('/');
                    List<XmlElement> nodes = new List<XmlElement>();
                    for (int i = 0; i < temp.Count() - 1; i++)
                    {
                        XmlElement ele = doc.CreateElement(temp[i]);
                        if (i == temp.Count() - 2)
                        {
                            //last node with actual data to add
                            ele.InnerText = temp[temp.Count() - 1];
                        }
                        nodes.Add(ele);
                    }
                    //add nodes to the element in reverse for hierarchy order
                    for (int i = nodes.Count - 1; i > -1; i--)
                    {
                        if (i == 0)
                        {
                            //getting here means this is the highmost node
                            //that needto be modified
                            reff.InsertAfter(nodes[i], reff.FirstChild);
                            break;
                        }
                        XmlElement parrent = nodes[i - 1];
                        XmlElement child = nodes[i];
                        parrent.InsertAfter(child, parrent.FirstChild);
                    }
                    //save it
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                    doc.Save(filePath);
                    break;

                case "edit":
                    //check to see if it's already there
                    XmlNodeList currentSoundBanksEdit = doc.SelectNodes(xpath);
                    foreach (XmlElement e in currentSoundBanksEdit)
                    {
                        string innerText = e.InnerText;
                        innerText = Regex.Replace(innerText, "\t", "");
                        innerText = innerText.Trim();
                        if (e.InnerText.Equals(replace))
                            return;
                    }
                    //find and replace
                    XmlNodeList rel1Edit = doc.SelectNodes(xpath);
                    foreach (XmlElement eee in rel1Edit)
                    {
                        if (Regex.IsMatch(eee.InnerText, search))
                        {
                            eee.InnerText = replace;
                        }
                    }
                    //save it
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                    doc.Save(filePath);
                    break;

                case "remove":
                    //check to see if it's there
                    XmlNodeList currentSoundBanksRemove = doc.SelectNodes(xpath);
                    foreach (XmlElement e in currentSoundBanksRemove)
                    {
                        if (Regex.IsMatch(e.InnerText, search))
                        {
                            e.RemoveAll();
                        }
                    }
                    //save it
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                    doc.Save(filePath);
                    //remove empty elements
                    XDocument doc2 = XDocument.Load(filePath);
                    doc2.Descendants().Where(e => string.IsNullOrEmpty(e.Value)).Remove();
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                    doc2.Save(filePath);
                    break;
            }
            //check to see if we need to remove the header
            bool hasHeader = false;
            XmlDocument doc5 = new XmlDocument();
            doc5.Load(filePath);
            foreach (XmlNode node in doc5)
            {
                if (node.NodeType == XmlNodeType.XmlDeclaration)
                {
                    hasHeader = true;
                }
            }
            //if not had header and has header, remove header
            //if had header and has header, no change
            //if not had header and not has header, no change
            //if had header and not has header, no change
            if (!hadHeader && hasHeader)
            {
                XmlDocument doc4 = new XmlDocument();
                doc4.Load(filePath);
                foreach (XmlNode node in doc4)
                {
                    if (node.NodeType == XmlNodeType.XmlDeclaration)
                    {
                        doc4.RemoveChild(node);
                    }
                }
                doc4.Save(filePath);
            }
        }
        //method to patch a standard text or json file
        //fileLocation is relative to res_mods folder
        public static void RegxPatch(string fileLocation, string search, string replace, string tanksLocation, string tanksVersion, int lineNumber = 0, bool testMods = false, string testXVMBootLoc = "")
        {
            if (Regex.IsMatch(fileLocation, "^\\\\\\\\res_mods"))
            {
                //new style patch, res_mods folder
                fileLocation = tanksLocation + fileLocation;
            }
            else if (Regex.IsMatch(fileLocation, "^\\\\\\\\mods"))
            {
                //new style patch, mods folder
                fileLocation = tanksLocation + fileLocation;
            }
            else if (testMods)
            {

            }
            else
            {
                //old style patch
                fileLocation = tanksLocation + "\\res_mods" + fileLocation;
            }

            if (!testMods && Regex.IsMatch(fileLocation, "versiondir"))
            {
                //patch versiondir out of fileLocation
                fileLocation = Regex.Replace(fileLocation, "versiondir", tanksVersion);
            }
            //patch xvmConfigFolderName out of fileLocation
            if (!testMods && Regex.IsMatch(fileLocation, "xvmConfigFolderName"))
            {
                string s = getXVMBootLoc(tanksLocation);
                if (s != null)
                    fileLocation = Regex.Replace(fileLocation, "xvmConfigFolderName", s);
            }
            else
            {
                //patch check mode, try to get boot xvm file from the xvm boot textbox
                if (testXVMBootLoc.Equals("") && Regex.IsMatch(fileLocation, "xvmConfigFolderName"))
                {
                    MessageBox.Show("Attempted to use variable \"xvmConfigFolderName\", but nothing in the xvm boot file location text box");
                    return;
                }
            }
            
            //check that the file exists
            if (!System.IO.File.Exists(fileLocation))
                return;

            //replace all "fake escape characters" with real escape characters
            search = search.Replace(@"\n", "newline");
            search = search.Replace(@"\r", "\r");
            search = search.Replace(@"\t", "\t");

            //load file from disk...
            string file = System.IO.File.ReadAllText(fileLocation);
            //parse each line into an index array
            string[] fileParsed = file.Split('\n');
            StringBuilder sb = new StringBuilder();
            if (lineNumber == 0)
            //search entire file and replace each instance
            {
                for (int i = 0; i < fileParsed.Count(); i++)
                {
                    if (Regex.IsMatch(fileParsed[i], search))
                    {
                        fileParsed[i] = Regex.Replace(fileParsed[i], search, replace);
                        fileParsed[i] = Regex.Replace(fileParsed[i], "newline", "\n");
                    }
                    sb.Append(fileParsed[i] + "\n");
                }
            }
            else if (lineNumber == -1)
            //search entire file and string and make one giant regex replacement
            {
                //but remove newlines first
                file = Regex.Replace(file, "\n", "newline");
                try
                {
                    if (Regex.IsMatch(file, search))
                    {
                        file = Regex.Replace(file, search, replace);
                    }
                    file = Regex.Replace(file, "newline", "\n");
                    sb.Append(file);
                }
                catch (ArgumentException)
                {
                    if (testMods) MessageBox.Show("invalid regex command");
                }
            }
            else
            {
                for (int i = 0; i < fileParsed.Count(); i++)
                {
                    if (i == lineNumber - 1)
                    {
                        string value = fileParsed[i];
                        if (Regex.IsMatch(value, search))
                        {
                            fileParsed[i] = Regex.Replace(fileParsed[i], search, replace);
                        }
                    }
                    sb.Append(fileParsed[i] + "\n");
                }
            }
            //save the file back into the string and then the file
            file = sb.ToString();
            System.IO.File.WriteAllText(fileLocation, file);
        }
        //method to parse json files
        public static void jsonPatch(string jsonFile, string jsonPath, string newValue, string mode, string tanksLocation, string tanksVersion, bool testMods = false, string testXVMBootLoc = "")
        {
            //try to convert the new value to a bool or an int or double first
            bool newValueBool = false;
            int newValueInt = -69420;
            double newValueDouble = -69420.0d;
            bool useBool = false;
            bool useInt = false;
            bool useDouble = false;
            //try a bool first, only works with "true" and "false"
            try
            {
                newValueBool = bool.Parse(newValue);
                useBool = true;
                useInt = false;
                useDouble = false;
            }
            catch (FormatException)
            { }
            //try a double nixt. it will parse a double and int. at this point it could be eithor
            try
            {
                newValueDouble = double.Parse(newValue);
                useDouble = true;
            }
            catch (FormatException)
            { }
            //try an int next. if it works than turn double to false and int to true
            try
            {
                newValueInt = int.Parse(newValue);
                useInt = true;
                useDouble = false;
            }
            catch (FormatException)
            { }
            //check if it's the new structure
            if (Regex.IsMatch(jsonFile, "^\\\\\\\\res_mods"))
            {
                //new style patch, res_mods folder
                jsonFile = tanksLocation + jsonFile;
            }
            else if (Regex.IsMatch(jsonFile, "^\\\\\\\\mods"))
            {
                //new style patch, mods folder
                jsonFile = tanksLocation + jsonFile;
            }
            else if (testMods)
            {

            }
            else
            {
                //old style patch
                jsonFile = tanksLocation + "\\res_mods" + jsonFile;
            }

            //patch versiondir out of fileLocation
            if (!testMods && Regex.IsMatch(jsonFile, "versiondir"))
            {
                jsonFile = Regex.Replace(jsonFile, "versiondir", tanksVersion);
            }
            //patch xvmConfigFolderName out of fileLocation
            if (!testMods && Regex.IsMatch(jsonFile, "xvmConfigFolderName"))
            {
                string s = getXVMBootLoc(tanksLocation);
                if (s != null)
                    jsonFile = Regex.Replace(jsonFile, "xvmConfigFolderName", s);
            }
            else
            {
                //patch check mode, try to get boot xvm file from the xvm boot textbox
                if (testXVMBootLoc.Equals("") && Regex.IsMatch(jsonFile, "xvmConfigFolderName"))
                {
                    MessageBox.Show("Attempted to use variable \"xvmConfigFolderName\", but nothing in the xvm boot file location text box");
                    return;
                }
            }

            //check that the file exists
            if (!System.IO.File.Exists(jsonFile))
                return;

            //load file from disk...
            string file = System.IO.File.ReadAllText(jsonFile);
            //save the "$" lines
            List<StringSave> ssList = new List<StringSave>();
            StringBuilder backTogether = new StringBuilder();
            string[] removeComments = file.Split('\n');
            for (int i = 0; i < removeComments.Count(); i++)
            {
                string temp = removeComments[i];
                //determine if it has (had) a comma at the end of the string
                bool hadComma = false;
                bool modified = false;
                if (Regex.IsMatch(temp, @",[ \/\w\t\r\n()]*$"))
                    hadComma = true;

                //determine if it is a illegal refrence in jarray or jobject
                StringSave ss = new StringSave();
                if (Regex.IsMatch(temp, @"^[ \t]*\"".*\"" *: *\$\{ *\"".*\""\ *}"))
                {
                    modified = true;
                    //jobject
                    ss.name = temp.Split('"')[1];
                    ss.value = temp.Split('$')[1];
                    ssList.Add(ss);
                    temp = "\"" + ss.name + "\"" + ": -69420";
                }
                else if (Regex.IsMatch(temp, @"^[ \t]*\$ *\{ *\"".*\"" *\}"))
                {
                    modified = true;
                    //jarray
                    string comment = "//\"comment_Willster419\"";
                    temp = comment + temp;
                    ss.name = temp.Split('"')[1];
                    ss.value = temp.Split('$')[1];
                    ssList.Add(ss);
                    temp = "-42069";
                }
                else if (Regex.IsMatch(temp, @"^[ \t]*""\$ref"" *: *{.*}"))
                {
                    modified = true;
                    //jobject
                    ss.name = temp.Split('"')[1];
                    ss.value = temp.Split('{')[1];
                    ssList.Add(ss);
                    temp = "\"" + "willster419_refReplace" + "\"" + ": -6969";
                }
                if (hadComma && modified)
                    temp = temp + ",";
                backTogether.Append(temp + "\n");
            }
            file = backTogether.ToString();
            JsonLoadSettings settings = new JsonLoadSettings();
            settings.CommentHandling = CommentHandling.Ignore;
            JObject root = null;
            //load json for editing
            try
            {
                root = JObject.Parse(file, settings);
            }
            catch (JsonReaderException j)
            {
                Utils.appendToLog(string.Format("ERROR: Failed to patch {0}",jsonFile));
                if (Program.testMode)
                {
                    //in test mode this is worthy of an EXCEPTION
                    throw new JsonReaderException(j.Message);
                }
            }
            //if it failed to parse show the message (above) and pull out
            if (root == null)
                return;
            if (mode == null || mode.Equals("") || mode.Equals("edit") || mode.Equals("arrayEdit"))
            {
                //the actual patch method
                JValue newObject = (JValue)root.SelectToken(jsonPath);
                //JValue newObject = null;
                //pull out if it failed to get the selection
                if (newObject == null)
                {
                    Utils.appendToLog(string.Format("WARNING: path {0} not found for {1}", jsonPath, Path.GetFileName(jsonFile)));
                }
                else if (useBool)
                {
                    newObject.Value = newValueBool;
                }
                else if (useInt)
                {
                    newObject.Value = newValueInt;
                }
                else if (useDouble)
                {
                    newObject.Value = newValueDouble;
                }
                else //string
                {
                    newObject.Value = newValue;
                }
            }
            else if (mode.Equals("remove"))
            {
                //TODO
            }
            else if (mode.Equals("arrayRemove"))
            {
                //TODO
            }
            else if (mode.Equals("add"))
            {
                //TODO
            }
            else if (mode.Equals("arrayAdd"))
            {
                //TODO
            }
            else
            {
                Utils.appendToLog(string.Format("ERROR: Unknown json patch mode, {0}", mode));
            }
            StringBuilder rebuilder = new StringBuilder();
            string[] putBackDollas = root.ToString().Split('\n');
            for (int i = 0; i < putBackDollas.Count(); i++)
            {
                string temp = putBackDollas[i];
                if (Regex.IsMatch(temp, "-69420"))//look for the temp value
                {
                    //array of string save and text file are in sync, so when one is found,
                    //take it from index 0 and remove from the list at index 0
                    temp = "\"" + ssList[0].name + "\"" + ": $" + ssList[0].value;
                    putBackDollas[i] = temp;//update the text file file
                    ssList.RemoveAt(0);//remove the entry from the list of entries to fix/replace
                }
                else if (Regex.IsMatch(temp, "-42069"))
                {
                    temp = "$" + ssList[0].value;
                    putBackDollas[i] = temp;
                    ssList.RemoveAt(0);
                }
                else if (Regex.IsMatch(temp, "willster419_refReplace"))
                {
                    temp = "\"" + ssList[0].name + "\"" + ": {" + ssList[0].value;
                    putBackDollas[i] = temp;
                    ssList.RemoveAt(0);
                }
                rebuilder.Append(putBackDollas[i] + "\n");
            }
            if (ssList.Count != 0)
                Utils.appendToLog(string.Format("There was an error with patching the file {0}, with extra refrences", jsonFile));
            System.IO.File.WriteAllText(jsonFile, rebuilder.ToString());
        }
        public static void pmodPatch(string bootFile, string xvmPath, string search, string newValue, string mode, string tanksLocation, string tanksVersion, bool testMods = false, string testXVMBootLoc = "")
        {
            numByteReads = 0;
            patchDone = false;
            genericTraverse = 0;
            //check if it's the new structure
            if (Regex.IsMatch(bootFile, "^\\\\\\\\res_mods"))
            {
                //new style patch, res_mods folder
                bootFile = tanksLocation + bootFile;
            }
            else if (Regex.IsMatch(bootFile, "^\\\\\\\\mods"))
            {
                //new style patch, mods folder
                bootFile = tanksLocation + bootFile;
            }
            else if (testMods)
            {

            }
            else
            {
                //old style patch
                bootFile = tanksLocation + "\\res_mods" + bootFile;
            }

            //patch versiondir out of fileLocation
            if (!testMods && Regex.IsMatch(bootFile, "versiondir"))
            {
                bootFile = Regex.Replace(bootFile, "versiondir", tanksVersion);
            }

            //patch "newline" out of the replace text
            newValue = Regex.Replace(newValue, "newline", "\n");

            //check that the file exists
            if (!System.IO.File.Exists(bootFile))
                return;
            //break down the path into an array
            string[] pathArrayy = xvmPath.Split('.');
            List<string> pathArray = new List<string>();
            //convert it to a List cause it has more features
            foreach (string s in pathArrayy)
                pathArray.Add(s);
            //load the file from disk
            numByteReads = 0;
            readInside(pathArray, bootFile, newValue, search, mode, xvmPath);
        }

        public static void xvmPatch(string bootFile, string xvmPath, string search, string newValue, string mode, string tanksLocation, string tanksVersion, bool testMods = false, string testXVMBootLoc = "")
        {
            numByteReads = 0;
            patchDone = false;
            genericTraverse = 0;
            //check if it's the new structure
            if (Regex.IsMatch(bootFile, "^\\\\\\\\res_mods"))
            {
                //new style patch, res_mods folder
                bootFile = tanksLocation + bootFile;
            }
            else if (Regex.IsMatch(bootFile, "^\\\\\\\\mods"))
            {
                //new style patch, mods folder
                bootFile = tanksLocation + bootFile;
            }
            else if (testMods)
            {

            }
            else
            {
                //old style patch
                bootFile = tanksLocation + "\\res_mods" + bootFile;
            }

            //patch versiondir out of fileLocation
            if (!testMods && Regex.IsMatch(bootFile, "versiondir"))
            {
                bootFile = Regex.Replace(bootFile, "versiondir", tanksVersion);
            }
            //patch xvmConfigFolderName out of fileLocation
            if (!testMods && Regex.IsMatch(bootFile, "xvmConfigFolderName"))
            {
                string s = getXVMBootLoc(tanksLocation);
                if (s != null)
                    bootFile = Regex.Replace(bootFile, "xvmConfigFolderName", s);
            }
            else
            {
                //patch check mode, try to get boot xvm file from the xvm boot textbox
                if (testXVMBootLoc.Equals("") && Regex.IsMatch(bootFile, "xvmConfigFolderName"))
                {
                    MessageBox.Show("Attempted to use variable \"xvmConfigFolderName\", but nothing in the xvm boot file location text box");
                    return;
                }
            }

            //patch "newline" out of the replace text
            newValue = Regex.Replace(newValue, "newline", "\n");

            //check that the file exists
            if (!System.IO.File.Exists(bootFile))
                return;
            //break down the path into an array
            string[] pathArrayy = xvmPath.Split('.');
            List<string> pathArray = new List<string>();
            //convert it to a List cause it has more features
            foreach (string s in pathArrayy)
                pathArray.Add(s);
            //create the stringBuilder to rewrite the file
            StringBuilder sb = new StringBuilder();
            //load the file from disk

            //read untill *start of string*${
            numByteReads = 0;
            string fileContents = System.IO.File.ReadAllText(bootFile);
            fileContents = Regex.Replace(fileContents, @"\/\*.*\*\/", "", RegexOptions.Singleline);
            string[] removeComments = fileContents.Split('\n');
            StringBuilder bootBuilder = new StringBuilder();
            foreach (string s in removeComments)
            {
                if (Regex.IsMatch(s, @"\/\/.*$"))
                    continue;
                bootBuilder.Append(s + "\n");
            }
            fileContents = bootBuilder.ToString();
            readUntill(fileContents, sb, @"^[ \t]*\$[ \t]*{[ \t]*""");
            //now read untill the next quote for the temp path
            string filePath = readUntill(fileContents, sb, "\"");
            //flip the folder path things
            filePath = Regex.Replace(filePath, "/", "\\");
            //remove the last one
            filePath = filePath.Substring(0, filePath.Length - 1);
            filePath = filePath.Trim();
            readInside(pathArray, Path.GetDirectoryName(bootFile) + "\\" + filePath, newValue, search, mode, xvmPath);
        }

        //getting into this means that we've started reading a new config file, maybe patch this one?
        private static void readInside(List<string> pathArray, string newFilePath, string replaceValue, string search, string mode, string origXvmPath)
        {
            numByteReads = 0;
            //create the (new) stringBuilder to rewrite the file
            StringBuilder sb = new StringBuilder();
            bool isToEnd = false;
            //load the file from disk
            string fileContents = System.IO.File.ReadAllText(newFilePath);
            while (pathArray.Count != 0)
            {
                string regex = "";
                bool isArrayIndex = false;
                //check if the patharray has array index in it
                if (Regex.IsMatch(pathArray[0], @"\["))
                {
                    regex = @"[ \t]*""" + pathArray[0].Split('[')[0] + "\"[ \t]*:";
                    isArrayIndex = true;
                }
                else
                {
                    regex = @"[ \t]*""" + pathArray[0] + "\"[ \t]*:";
                }
                //read untill the value we want
                if (readUntill(fileContents, sb, regex) == "null")
                {
                    Utils.appendToLog(string.Format("ERROR: Path not found: {0}", origXvmPath));
                    return;
                }
                //determine if the this value is actually a file refrence
                string refrenceTest = peekUntill(fileContents, @"[\[,}\]]$");
                if (Regex.IsMatch(refrenceTest, @"\${[ \t]*""") && !isArrayIndex)
                {
                    parseRefrence1(pathArray, newFilePath, replaceValue, search, mode, origXvmPath, fileContents, sb);
                }
                //determine if it is the other type of refrence
                string refrenceTest2 = peekUntill(fileContents, @"[\[,}\]]$");
                if (Regex.IsMatch(refrenceTest2, @"""\$ref"":") && !isArrayIndex)
                {
                    parseRefrence2(pathArray, newFilePath, replaceValue, search, mode, origXvmPath, fileContents, sb);
                }
                //determine if it is an array
                isToEnd = false;
                if (isArrayIndex)
                {
                    int indexToReadTo = 0;
                    //boolean flag for it you want to get to the end of the jarray
                    isToEnd = false;
                    //split the array into an array lol
                    if (readUntill(fileContents, sb, @"\[") == "null")
                    {
                        Utils.appendToLog(string.Format("ERROR: Path not found: {0}", origXvmPath));
                        return;
                    }
                    string arrayContents = peekUntill(fileContents, @"\]");
                    //split the array based on "},"
                    List<string> carray = split(arrayContents, @"[,}]$");
                    //if it is an index, just use it
                    if (Regex.IsMatch(pathArray[0], @"\[\d\]+"))
                    {
                        string splitter = pathArray[0].Split('[')[1];
                        splitter = splitter.Split(']')[0];
                        //also repair the path
                        pathArray[0] = Regex.Replace(pathArray[0], "endIndex", "");
                        indexToReadTo = int.Parse(splitter);
                        if (indexToReadTo < 0 || indexToReadTo >= carray.Count())
                        {
                            //error and abort
                            Utils.appendToLog("invalid index: " + pathArray[0]);
                            return;
                        }
                    }
                    else if (Regex.IsMatch(pathArray[0], @"\[-1\]+"))
                    {
                        //-1 keyword for the add array method
                        if (!mode.Equals("array_add"))
                        {
                            Utils.appendToLog("To use -1 keyword, must be in array_add mode!");
                            return;
                        }
                        //set the flag and reset the values
                        indexToReadTo = carray.Count - 1;
                        isToEnd = true;
                    }
                    //if it is a search, 
                    else
                    {
                        //grab the regex search out of it an repair the array
                        //figure out how broken (how many "." are part of regex
                        string borken1 = "";
                        foreach (string s in pathArray)
                            borken1 = borken1 + s + ".";
                        genericTraverse = 0;
                        readUntillGeneric(borken1, @"\[");
                        genericTraverse = 0;
                        string anotherString2 = readUntillGeneric(borken1, @"\]endIndex");
                        genericTraverse = 0;
                        pathArray[0] = anotherString2;
                        //figure out how many indexes got rekt
                        string[] anotherTemp = pathArray[0].Split('.');
                        int numIndexesLost = anotherTemp.Count() - 1;
                        for (int i = 1; i != numIndexesLost + 1; i++)
                        {
                            pathArray.RemoveAt(1);
                        }
                        string[] vals = pathArray[0].Split('[');
                        int origLength = vals[0].Length;
                        origLength++;
                        string splitter = pathArray[0];
                        splitter = splitter.Substring(origLength);
                        splitter = splitter.Substring(0, splitter.Length - 9);
                        //search each index for the regex match
                        //for loop it and check at the end if it went all the way through
                        for (indexToReadTo = 0; indexToReadTo < carray.Count(); indexToReadTo++)
                        {
                            string carrayVal = carray[indexToReadTo];
                            if (Regex.IsMatch(carrayVal, splitter))
                            {
                                break;
                            }
                        }
                        if (indexToReadTo < 0 || indexToReadTo >= carray.Count())
                        {
                            //error and abort
                            Utils.appendToLog(string.Format("invalid index: {0}", pathArray[0]));
                            return;
                        }
                    }
                    //ok now we have a valid index to actually make the change in the requested index
                    string advanceTo = "";
                    for (int i = 0; i < indexToReadTo; i++)
                    {
                        advanceTo = advanceTo + carray[i];
                    }
                    if (isToEnd)
                    {
                        advanceTo = advanceTo + carray[carray.Count - 1];
                    }
                    //get it to right before the desired index starts
                    readUntill(fileContents, sb, advanceTo.Count());
                    //determine if the this value is actually a file refrence
                    string refrenceTest5 = peekUntill(fileContents, @"[,}\]]$");
                    if (Regex.IsMatch(refrenceTest5, @"\${[ \t]*"""))
                    {
                        if (mode.Equals("add") || mode.Equals("edit"))
                            parseRefrence1(pathArray, newFilePath, replaceValue, search, mode, origXvmPath, fileContents, sb);
                    }
                }
                //we found it so remove it from the path
                if (pathArray.Count != 0)
                    pathArray.RemoveAt(0);
            }
            //split off into the cases for different xvm modification types
            if (!patchDone)
            {
                patchDone = true;
                switch (mode)
                {
                    default:
                        //do nothing
                        Utils.appendToLog(string.Format("Invalid mode: {0} for xvm patch {1}", mode, origXvmPath));
                        break;
                    case "edit":
                        xvmEdit(fileContents, sb, newFilePath, replaceValue, search);
                        break;
                    case "add":
                        xvmAdd(fileContents, sb, newFilePath, replaceValue, search);
                        break;
                    case "remove":
                        //TODO
                        break;
                    case "array_clear":
                        xvmArrayClear(fileContents, sb, newFilePath, replaceValue, search);
                        break;
                    case "array_add":
                        xvmArrayAdd(fileContents, sb, newFilePath, replaceValue, search, isToEnd);
                        break;
                    case "array_edit":
                        xvmArrayEdit(fileContents, sb, newFilePath, replaceValue, search);
                        break;
                    case "array_remove":
                        xvmArrayRemove(fileContents, sb, newFilePath, replaceValue, search);
                        break;
                }
            }
        }
        //parse the xvm style refrence
        private static void parseRefrence1(List<string> pathArray, string newFilePath, string replaceValue, string search, string mode, string origXvmPath, string fileContents, StringBuilder sb)
        {
            //it's a refrence, move it to the next file and readInsideEdit (yes recursion)
            if (readUntill(fileContents, sb, @"\${[ \t]*""") == "null")
            {
                Utils.appendToLog(string.Format("ERROR: Path not found: {0}", origXvmPath));
                return;
            }
            //now read untill the next quote for the temp path
            string filePath = readUntill(fileContents, sb, "\"");
            //check to see if it is only a refrence withen the same file
            string test = peekUntill(fileContents, "}");
            if (Regex.IsMatch(test, ":"))
            {
                //new file refrence
                //read untill the next quote
                readUntill(fileContents, sb, "\"");
                string tempPath = readUntill(fileContents, sb, "\"");
                tempPath = tempPath.Substring(0, tempPath.Length - 1);
                string[] newPathSplit = tempPath.Split('.');
                pathArray[0] = newPathSplit[0];
                for (int i = 1; i < newPathSplit.Count(); i++)
                    pathArray.Insert(1, newPathSplit[i]);

                //flip the folder path things
                filePath = Regex.Replace(filePath, "/", "\\");
                //remove the last one
                filePath = filePath.Substring(0, filePath.Length - 1);
                filePath = filePath.Trim();
                readInside(pathArray, Path.GetDirectoryName(newFilePath) + "\\" + filePath, replaceValue, search, mode, origXvmPath);
            }
            else if (Regex.IsMatch(filePath, @"\.(json|xc)"))
            {
                //new file
                string tempPath = filePath.Substring(0, filePath.Length - 1);
                pathArray.RemoveAt(0);
                readInside(pathArray, Path.GetDirectoryName(newFilePath) + "\\" + tempPath, replaceValue, search, mode, origXvmPath);
            }
            else
            {
                //same file refrence
                //EXCEPT it could eithor be a new file refrence
                //3 types of refrences
                //"file.json"
                //"path.within.file
                //"file.json":"path.to.new.ting"
                string[] newPathSplit = filePath.Split('.');
                pathArray[0] = newPathSplit[0];
                for (int i = 1; i < newPathSplit.Count(); i++)
                    pathArray.Insert(1, newPathSplit[i]);
                readInside(pathArray, newFilePath, replaceValue, search, mode, origXvmPath);
            }

        }
        //parse the ref style refrence
        private static void parseRefrence2(List<string> pathArray, string newFilePath, string replaceValue, string search, string mode, string origXvmPath, string fileContents, StringBuilder sb)
        {
            //ref style refrence
            if (readUntill(fileContents, sb, @"""\$ref"":") == "null")
            {
                Utils.appendToLog(string.Format("ERROR: Path not found: {0}", origXvmPath));
                return;
            }
            readUntill(fileContents, sb, ":");
            readUntill(fileContents, sb, "\"");
            string filePath = readUntill(fileContents, sb, "\"");
            filePath = Regex.Replace(filePath, "/", "\\");
            filePath = filePath.Substring(0, filePath.Length - 1);
            filePath = filePath.Trim();
            readInside(pathArray, Path.GetDirectoryName(newFilePath) + "\\" + filePath, replaceValue, search, mode, origXvmPath);
            //readUntill(fileContents, sb, ":");
        }
        private static void xvmEdit(string fileContents, StringBuilder sb, string newFilePath, string replaceValue, string search)
        {
            bool modified = false;
            //this is the actual value we want to change
            //get past all the boring stuff
            string toReplace = readUntill(fileContents, sb, @"[,}\]]", false);
            //actually replace the value
            //check if it's a comma type (not last) or curley bracket (last)
            string replaced = "";
            if (Regex.IsMatch(toReplace, @",$"))
            {
                if (Regex.IsMatch(toReplace, search + ",$"))
                {
                    replaced = " " + replaceValue + ",";
                    modified = true;
                    patchDone = true;
                }
            }
            else
            {
                if (Regex.IsMatch(toReplace, search))
                {
                    replaced = replaceValue + "\n";
                    modified = true;
                    patchDone = true;
                }
            }
            sb.Append(replaced);
            readUntillEnd(fileContents, sb);

            if (modified)
            {
                System.IO.File.Delete(newFilePath);
                System.IO.File.WriteAllText(newFilePath, sb.ToString());
            }

        }
        //adding a new entry. adds to the top of the parent entry
        private static void xvmAdd(string fileContents, StringBuilder sb, string newFilePath, string replaceValue, string search)
        {
            bool modified = false;
            string replaced = "";
            //read to the end of that line
            //read for (white space) (anything but white space)
            string temp = readUntill(fileContents, sb, @"\s*\S+\s");
            //back up one
            numByteReads--;
            sb.Remove(sb.Length - 1, 1);
            //check if the last one was a comma
            //if not then add it
            //determine if it stopped in groups of eithor ([ , {) or
            char c = fileContents[numByteReads - 1];
            if (c.Equals(','))
            {
                replaced = "\n" + replaceValue + ",";
            }
            else if (c.Equals('{'))
            {
                replaced = "\n" + replaceValue + ",";
            }
            else
            {
                replaced = ",\n" + replaceValue;
            }
            //append it in the sb
            sb.Append(replaced);
            readUntillEnd(fileContents, sb);
            modified = true;
            patchDone = true;
            if (modified)
            {
                System.IO.File.Delete(newFilePath);
                System.IO.File.WriteAllText(newFilePath, sb.ToString());
            }
        }
        //clearing out an array
        private static void xvmArrayClear(string fileContents, StringBuilder sb, string newFilePath, string replaceValue, string search)
        {
            //advance reader to start of the array
            readUntill(fileContents, sb, @"\[");
            //advance reader to end of array, not saving sb
            readUntill(fileContents, sb, @"\]", false);
            sb.Append("]");
            bool modified = false;
            readUntillEnd(fileContents, sb);
            modified = true;
            patchDone = true;
            if (modified)
            {
                System.IO.File.Delete(newFilePath);
                System.IO.File.WriteAllText(newFilePath, sb.ToString());
            }
        }
        //adding a new entry
        private static void xvmArrayAdd(string fileContents, StringBuilder sb, string newFilePath, string replaceValue, string search, bool isToEnd = false)
        {
            bool modified = false;
            string replaced = "";
            //all it needs to do is add it
            //unless it's to end
            if (!isToEnd)
            {
                replaced = replaced + replaceValue + ",";
            }
            else
            {
                //need to not add the comma
                replaced = replaced + replaceValue;
                //need to also go back to add the comma to the stringbuilder
                string temp = peekBehindUntill(fileContents, @"[},]");
                //temp = temp.Trim();
                sb.Remove(sb.ToString().Count() - (temp.Count() - 1), temp.Count() - 1);
                string toApeend = temp.Substring(0, 1);
                sb.Append(toApeend);
                //temp.Remove(0, 1);
                sb.Append(",");
                sb.Append(temp.Substring(1, temp.Length - 2));
            }
            //append it in the sb
            sb.Append(replaced);
            if (isToEnd)
                sb.Append("\n");
            readUntillEnd(fileContents, sb);
            modified = true;
            patchDone = true;
            if (modified)
            {
                System.IO.File.Delete(newFilePath);
                System.IO.File.WriteAllText(newFilePath, sb.ToString());
            }
        }
        //editing an existing entry
        private static void xvmArrayEdit(string fileContents, StringBuilder sb, string newFilePath, string replaceValue, string search)
        {
            bool modified = false;
            bool hadComma = false;
            //move past it and save int input
            string editCheck = readUntill(fileContents, sb, @"[,\]]", false);
            if (Regex.IsMatch(editCheck, search))
            {
                if (Regex.IsMatch(editCheck, @",$"))
                    hadComma = true;
                editCheck = " " + replaceValue;
                if (hadComma)
                    editCheck = editCheck + ",";
                //append it in the sb
                sb.Append(editCheck);
                readUntillEnd(fileContents, sb);
                modified = true;
                patchDone = true;
            }
            if (modified)
            {
                System.IO.File.Delete(newFilePath);
                System.IO.File.WriteAllText(newFilePath, sb.ToString());
            }
        }
        //removing an existing entry
        private static void xvmArrayRemove(string fileContents, StringBuilder sb, string newFilePath, string replaceValue, string search)
        {
            bool modified = false;
            bool lastItem = false;
            //move past it and save int input
            string editCheck = readUntill(fileContents, sb, @"[,\]]", false);
            if (Regex.IsMatch(editCheck, @"\]$"))
            {
                lastItem = true;
                //remove last comma from sb index
                sb.Remove(sb.Length - 1, 1);
            }
            if (Regex.IsMatch(editCheck, search))
            {
                editCheck = Regex.Replace(editCheck, search, "");
                if (lastItem)
                {
                    editCheck = editCheck + "]";
                }
                //append it in the sb
                sb.Append(editCheck);
                readUntillEnd(fileContents, sb);
                modified = true;
                patchDone = true;
            }
            if (modified)
            {
                System.IO.File.Delete(newFilePath);
                System.IO.File.WriteAllText(newFilePath, sb.ToString());
            }
        }
        //advances the reader untill a certain set of characters are found
        private static string readUntill(string fileContents, StringBuilder sb, string stopAt, bool save = true)
        {
            string readValues = "";
            string temp = readValues;
            while (!Regex.IsMatch(temp, stopAt, RegexOptions.Multiline))
            {
                int numChars = fileContents.Count();
                if (numByteReads >= numChars)
                {
                    return "null";
                }
                char c = fileContents[numByteReads];
                numByteReads++;
                readValues = readValues + c;
                if (save)
                    sb.Append(c);
                if (readValues.Length > stopAt.Length)
                {
                    temp = readValues.Substring(readValues.Length - stopAt.Length, stopAt.Length);
                }
                else
                {
                    temp = readValues;
                }
            }
            return readValues;
        }
        //generic readAhead method of above
        private static string readUntillGeneric(string s, string stopAt)
        {
            //genericTraverse = 0;
            string readValues = "";
            while (!Regex.IsMatch(readValues, stopAt, RegexOptions.Multiline))
            {
                if (genericTraverse >= s.Count())
                    return readValues;
                char c = s[genericTraverse];
                genericTraverse++;
                readValues = readValues + c;
            }
            return readValues;
        }
        //advances the reader x ammout of characters
        private static string readUntill(string fileContents, StringBuilder sb, int stopAt, bool save = true)
        {
            string readValues = "";
            int session = 0;
            while (session != stopAt)
            {
                char c = fileContents[numByteReads];
                numByteReads++;
                session++;
                readValues = readValues + c;
                if (save)
                    sb.Append(c);
            }
            return readValues;
        }
        //advances the reader untill the end
        private static string readUntillEnd(string fileContents, StringBuilder sb)
        {
            string readValues = "";
            while (numByteReads < fileContents.Length)
            {
                char c = fileContents[numByteReads];
                numByteReads++;
                readValues = readValues + c;
                sb.Append(c);
            }
            return readValues;
        }
        //looks ahead untill a certain set of characters are found
        private static string peekUntill(string fileContents, string stopAt)
        {
            string readValues = "";
            int numPeeks = 0;
            while (!Regex.IsMatch(readValues, stopAt, RegexOptions.Multiline))
            {
                if (numByteReads > fileContents.Count())
                {
                    numByteReads = numByteReads + numPeeks;
                    return readValues;
                }
                char c = fileContents[numByteReads];
                numByteReads++;
                readValues = readValues + c;
                numPeeks++;
            }
            numByteReads = numByteReads - numPeeks;
            return readValues;
        }
        //looks behind untill a certain set of characters are found
        private static string peekBehindUntill(string fileContents, string stopAt)
        {
            string readValues = "";
            int numPeeks = 0;
            while (!Regex.IsMatch(readValues, stopAt, RegexOptions.Multiline))
            {
                if (numByteReads == 0)
                {
                    numByteReads = numByteReads + numPeeks;
                    return readValues;
                }
                char c = fileContents[numByteReads];
                numByteReads--;
                readValues = c + readValues;
                numPeeks++;
            }
            numByteReads = numByteReads + numPeeks;
            return readValues;
        }
        //looks ahead untill a certain amount of characters are found
        private static string peekUntill(string fileContents, int stopAt)
        {
            string readValues = "";
            int numPeeks = 0;
            while (numPeeks != stopAt)
            {
                if (numByteReads > fileContents.Count())
                {
                    numByteReads = numByteReads - numPeeks;
                    return readValues;
                }
                char c = fileContents[numByteReads];
                numByteReads++;
                readValues = readValues + c;
                numPeeks++;
            }
            numByteReads = numByteReads - numPeeks;
            return readValues;
        }
        //splits text into an array based on a regex match
        private static List<string> split(string stringToSplit, string regexSplitCommand)
        {
            List<string> temp = new List<string>();
            int saveNumReadBytes = 0;
            string readValues = "";
            while (saveNumReadBytes <= stringToSplit.Count())
            {
                int startBracketCount = 0;
                while (true)
                {
                    if (saveNumReadBytes >= stringToSplit.Count())
                    {
                        readValues = readValues.Substring(0, readValues.Length - 1);
                        temp.Add(readValues);
                        return temp;
                    }
                    char c = stringToSplit[saveNumReadBytes];
                    if (c.Equals('{'))
                        startBracketCount++;
                    else if (c.Equals('}'))
                        startBracketCount--;
                    else if (c.Equals(','))
                    {
                        if (startBracketCount == 0)
                        {
                            saveNumReadBytes++;
                            readValues = readValues + c;
                            break;
                        }
                    }
                    saveNumReadBytes++;
                    readValues = readValues + c;
                }
                temp.Add(readValues);
                readValues = "";
            }
            return temp;
        }
        //returns the folder(s) to get to the xvm config folder directory
        public static string getXVMBootLoc(string tanksLocation, string customBootFileLoc = null, bool writeToLog = true)
        {
            string bootFile = tanksLocation + xvmBootFileLoc1;
            if (customBootFileLoc != null)
                bootFile = customBootFileLoc;
            if (!System.IO.File.Exists(bootFile))
            {
                if (writeToLog)
                    appendToLog(string.Format("ERROR: xvm config boot file does not exist at {0}, checking {1}", xvmBootFileLoc1, xvmBootFileLoc2));
                else
                    appendToLog(string.Format("NOTICE: default run, xvm config boot file does not exist at {0}, checking {1}", xvmBootFileLoc1, xvmBootFileLoc2));
                bootFile = xvmBootFileLoc2;
                if (!System.IO.File.Exists(bootFile))
                {
                    if(writeToLog)
                        appendToLog(string.Format("ERROR: xvm config boot file does not exist at {0}, aborting patch", xvmBootFileLoc2));
                    else
                        appendToLog(string.Format("NOTICE: default run, xvm config boot file does not exist at {0}, user did not install xvm", xvmBootFileLoc2));
                    return null;
                }
            }
            appendToLog("xvm boot file located to parse");
            string fileContents = System.IO.File.ReadAllText(bootFile);
            //patch block comments out
            fileContents = Regex.Replace(fileContents, @"\/\*.*\*\/", "", RegexOptions.Singleline);
            //patch single line comments out
            string[] removeComments = fileContents.Split('\n');
            StringBuilder bootBuilder = new StringBuilder();
            foreach (string s in removeComments)
            {
                if (Regex.IsMatch(s, @"\/\/.*$"))
                    continue;
                bootBuilder.Append(s + "\n");
            }
            fileContents = bootBuilder.ToString();
            //read to the actual file path
            genericTraverse = 0;
            readUntillGeneric(fileContents, @"^[ \t]*\$[ \t]*{[ \t]*""");
            //now read untill the next quote for the temp path
            string filePath = readUntillGeneric(fileContents, "\"");
            //flip the folder path things
            filePath = Regex.Replace(filePath, "/", "\\");
            //remove the last one
            filePath = filePath.Substring(0, filePath.Length - 1);
            filePath = filePath.Trim();
            genericTraverse = 0;
            string theNewPath = Path.GetDirectoryName(filePath);
            return theNewPath;
        }
        //unchecks all mods from memory
        public static void clearSelectionMemory_org(List<Category> parsedCatagoryList, List<Mod> UserMods)
        {
            Utils.appendToLog("Unchecking all mods");
            foreach (Category c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    m.Checked = false;
                    Utils.uncheckProcessConfigs_org(m.configs);
                }
            }
            if(UserMods != null)
            {
                foreach(Mod m in UserMods)
                {
                    m.Checked = false;
                }
            }
        }

        private static void uncheckProcessConfigs_org(List<Config> configList)
        {
            foreach (Config cc in configList)
            {
                cc.Checked = false;
                Utils.uncheckProcessConfigs_org(cc.configs);
            }
        }

        //unchecks all mods from memory
        public static void clearSelectionMemory(List<Category> parsedCatagoryList, List<Mod> UserMods)
        {
            Utils.appendToLog("Unchecking all mods");
            foreach (Category c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    m.Checked = false;
                    if (m.modFormCheckBox is ModFormCheckBox)
                    {
                        ModFormCheckBox mfcb = (ModFormCheckBox)m.modFormCheckBox;
                        mfcb.Checked = false;
                        mfcb.Parent.BackColor = Settings.getBackColor();
                    }
                    else if (m.modFormCheckBox is ModWPFCheckBox)
                    {
                        ModWPFCheckBox mfCB2 = (ModWPFCheckBox)m.modFormCheckBox;
                        mfCB2.IsChecked = false;
                    }
                    Utils.uncheckProcessConfigs(m.configs);
                }
            }
            if (UserMods != null)
            {
                foreach (Mod um in UserMods)
                {
                    um.Checked = false;
                    if (um.modFormCheckBox != null)
                    {
                        ModFormCheckBox mfcb = (ModFormCheckBox)um.modFormCheckBox;
                        mfcb.Checked = false;
                    }
                }
            }
        }

        private static void uncheckProcessConfigs(List<Config> configList)
        {
            foreach (Config c in configList)
            {
                c.Checked = false;
                if (c.configUIComponent is ConfigFormCheckBox)
                {
                    ConfigFormCheckBox CBTemp = (ConfigFormCheckBox)c.configUIComponent;
                    CBTemp.Checked = false;
                    CBTemp.Parent.BackColor = Settings.getBackColor();
                }
                else if (c.configUIComponent is ConfigFormComboBox)
                {
                    ConfigFormComboBox CBTemp = (ConfigFormComboBox)c.configUIComponent;
                    if (CBTemp.SelectedIndex != 0)
                        CBTemp.SelectedIndex = 0;
                    CBTemp.Parent.BackColor = Settings.getBackColor();
                }
                else if (c.configUIComponent is ConfigFormRadioButton)
                {
                    ConfigFormRadioButton CBTemp = (ConfigFormRadioButton)c.configUIComponent;
                    CBTemp.Checked = false;
                    CBTemp.Parent.BackColor = Settings.getBackColor();
                }
                else if (c.configUIComponent is ConfigWPFCheckBox)
                {
                    ConfigWPFCheckBox CBTemp = (ConfigWPFCheckBox)c.configUIComponent;
                    CBTemp.IsChecked = false;
                }
                else if (c.configUIComponent is ConfigWPFComboBox)
                {
                    //do nothing...
                }
                else if (c.configUIComponent is ConfigWPFRadioButton)
                {
                    ConfigWPFRadioButton CBTemp = (ConfigWPFRadioButton)c.configUIComponent;
                    CBTemp.IsChecked = false;
                }
                Utils.uncheckProcessConfigs(c.configs);
            }
        }
        
        //saves the currently checked configs and mods
        public static void saveConfig(bool fromButton, string fileToConvert, List<Category> parsedCatagoryList, List<Mod> userMods)
        {
            //dialog box to ask where to save the config to
            System.Windows.Forms.SaveFileDialog saveLocation = new System.Windows.Forms.SaveFileDialog();
            saveLocation.AddExtension = true;
            saveLocation.DefaultExt = ".xml";
            saveLocation.Filter = "*.xml|*.xml";
            saveLocation.InitialDirectory = Path.Combine(Application.StartupPath, "RelHaxUserConfigs");
            saveLocation.Title = Translations.getTranslatedString("selectWhereToSave");
            if (fromButton)
            {
                if (saveLocation.ShowDialog().Equals(DialogResult.Cancel))
                {
                    //cancel
                    return;
                }
            }
            string savePath = saveLocation.FileName;
            if (Settings.saveLastConfig && !fromButton && fileToConvert == null)
            {
                savePath = Path.Combine(Application.StartupPath, "RelHaxUserConfigs", "lastInstalledConfig.xml");
                Utils.appendToLog(string.Format("Save last config checked, saving to {0}", savePath));
            }
            else if (!fromButton && !(fileToConvert == null))
            {
                savePath = fileToConvert;
                Utils.appendToLog(string.Format("convert saved config file \"{0}\" to format {1}", savePath, Settings.configFileVersion));
            }

            //create saved config xml layout
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("mods", new XAttribute("ver", Settings.configFileVersion), new XAttribute("date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))));

            //relhax mods root
            doc.Element("mods").Add(new XElement("relhaxMods"));
            //user mods root
            doc.Element("mods").Add(new XElement("userMods"));

            var nodeRelhax = doc.Descendants("relhaxMods").FirstOrDefault();
            //check every mod
            foreach (Category c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    if (m.Checked)
                    {
                        //add it to the list
                        nodeRelhax.Add(new XElement("mod", m.packageName));
                        if (m.configs.Count > 0)
                        {
                            Utils.saveProcessConfigs(ref doc, m.configs);
                        }
                    }
                }
            }

            var nodeUserMods = doc.Descendants("userMods").FirstOrDefault();
            //check user mods
            foreach (Mod m in userMods)
            {
                if (m.Checked)
                {
                    //add it to the list
                    nodeUserMods.Add(new XElement("mod", m.name));
                }
            }
            doc.Save(savePath);
            if (fromButton)
            {
                MessageBox.Show(Translations.getTranslatedString("configSaveSuccess"));
            }
        }

        private static void saveProcessConfigs(ref XDocument doc, List<Config> configList)
        {
            var node = doc.Descendants("relhaxMods").FirstOrDefault();
            foreach (Config cc in configList)
            {
                if (cc.Checked)
                {
                    //add the config to the list
                    node.Add(new XElement("mod", cc.packageName));
                    if (cc.configs.Count > 0)
                    {
                        Utils.saveProcessConfigs(ref doc, cc.configs);
                    }
                }
            }
        }

        public static void loadConfig(bool fromButton, string filePath, List<Category> parsedCatagoryList, List<Mod> userMods)
        {
            //uncheck everythihng in memory first
            Utils.clearSelectionMemory(parsedCatagoryList, userMods);
            XmlDocument doc = new XmlDocument();
            string[] filePathSplit = filePath.Split(',');
            if (filePathSplit.Count() > 1)
            {
                string xmlString = Utils.getStringFromZip(filePathSplit[0], filePathSplit[1]);
                doc.LoadXml(xmlString);
            }
            else
            {
                doc.Load(filePath);
            }
            //check config file version
            XmlNode xmlNode = doc.SelectSingleNode("//mods");
            string ver = "";
            // check if attribut exists and if TRUE, get the value
            if (xmlNode.Attributes != null && xmlNode.Attributes["ver"] != null)
            {
                ver = xmlNode.Attributes["ver"].Value;
            }
            if (ver.Equals("2.0"))      //the file is version v2.0, so go "loadConfigV2" (packageName depended)
            {
                loadConfigV2(filePath, parsedCatagoryList, userMods);
            }
            else // file is still version v1.0 (name dependend)
            {
                loadConfigV1(fromButton, filePath, parsedCatagoryList, userMods);
            }
        }

        //loads a saved config from xml and parses it into the memory database
        public static void loadConfigV1(bool fromButton, string filePath, List<Category> parsedCatagoryList, List<Mod> userMods)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            Utils.appendToLog("Loading mod selections v1.0 from " + filePath);
            //get a list of mods
            XmlNodeList xmlModList = doc.SelectNodes("//mods/relhaxMods/mod");
            foreach (XmlNode n in xmlModList)
            {
                //gets the inside of each mod
                //also store each config that needsto be enabled
                Mod m = new Mod();
                foreach (XmlNode nn in n.ChildNodes)
                {
                    switch (nn.Name)
                    {
                        case "name":
                            m = Utils.linkMod(nn.InnerText, parsedCatagoryList);
                            if ((m != null) && (!m.visible))
                                return;
                            if (m == null)
                            {
                                Utils.appendToLog(string.Format("WARNING: mod \"{0}\" not found", nn.InnerText));
                                MessageBox.Show(string.Format(Translations.getTranslatedString("modNotFound"), nn.InnerText));
                                continue;
                            }
                            if (m.enabled)
                            {
                                m.Checked = true;
                                if (m.modFormCheckBox != null)
                                {
                                    if (m.modFormCheckBox is ModFormCheckBox)
                                    {
                                        ModFormCheckBox mfcb = (ModFormCheckBox)m.modFormCheckBox;
                                        mfcb.Checked = true;
                                        if(!Settings.disableColorChange)
                                            mfcb.Parent.BackColor = System.Drawing.Color.BlanchedAlmond;
                                    }
                                    else if (m.modFormCheckBox is ModWPFCheckBox)
                                    {
                                        ModWPFCheckBox mfCB2 = (ModWPFCheckBox)m.modFormCheckBox;
                                        mfCB2.IsChecked = true;
                                    }
                                }
                                Utils.appendToLog(string.Format("Checking mod {0}", m.name));
                            }
                            else
                            {
                                //uncheck
                                if (m.modFormCheckBox != null)
                                {
                                    if (m.modFormCheckBox is ModFormCheckBox)
                                    {
                                        ModFormCheckBox mfcb = (ModFormCheckBox)m.modFormCheckBox;
                                        mfcb.Checked = false;
                                        mfcb.Parent.BackColor = Settings.getBackColor();
                                    }
                                    else if (m.modFormCheckBox is ModWPFCheckBox)
                                    {
                                        ModWPFCheckBox mfCB2 = (ModWPFCheckBox)m.modFormCheckBox;
                                        mfCB2.IsChecked = false;
                                    }
                                }
                            }
                            break;
                        case "configs":
                            Utils.loadProcessConfigsV1(nn, m, true);
                            break;
                        //compatibility in case it's a super legacy with subConfigs
                        case "subConfigs":
                            Utils.loadProcessConfigsV1(nn, m, true);
                            break;
                    }
                }
            }
            //user mods
            XmlNodeList xmlUserModList = doc.SelectNodes("//mods/userMods/mod");
            foreach (XmlNode n in xmlUserModList)
            {
                //gets the inside of each user mod
                Mod m = new Mod();
                foreach (XmlNode nn in n.ChildNodes)
                {
                    switch (nn.Name)
                    {
                        case "name":
                            m = Utils.getUserMod(nn.InnerText, userMods);
                            if (m != null)
                            {
                                string filename = m.name + ".zip";
                                if (System.IO.File.Exists(Path.Combine(Application.StartupPath, "RelHaxUserMods", filename)))
                                {
                                    m.Checked = true;
                                    if (m.modFormCheckBox != null)
                                    {
                                        ModFormCheckBox mfcb = (ModFormCheckBox)m.modFormCheckBox;
                                        mfcb.Checked = true;
                                    }
                                    Utils.appendToLog(string.Format("checking user mod {0}", m.zipFile));
                                }
                            }
                            break;
                    }
                }
            }
            Utils.appendToLog("Finished loading mod selections v1.0");
            if (fromButton)
            {
                DialogResult result = MessageBox.Show(Translations.getTranslatedString("oldSavedConfigFile"), Translations.getTranslatedString("information"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    // create Path to UserConfigs Backup
                    string backupFolder = Path.Combine(Application.StartupPath, "RelHaxUserConfigs", "Backup");
                    // create Backup folder at UserConfigs
                    System.IO.Directory.CreateDirectory(backupFolder);
                    // exctrat filename to create a new filename with backup date and time
                    string filename = Path.GetFileNameWithoutExtension(filePath);
                    string fileextention = Path.GetExtension(filePath);
                    // create target path
                    string targetFilePath = Path.Combine(backupFolder, string.Format("{0}_{1:yyyy-MM-dd-HH-mm-ss}{2}", filename, DateTime.Now, fileextention));
                    // move file to new location now
                    try
                    {
                        System.IO.File.Move(filePath, targetFilePath);
                    }
                    catch (Exception ex)
                    {
                        Utils.exceptionLog("loadConfigV1", string.Format("sourceFile: {0}\ntargetFile: {1}", filePath, targetFilePath), ex);
                    }
                    // create saved config file with new format
                    saveConfig(false, filePath, parsedCatagoryList, userMods);
                }
            }
        }

        //loads a saved config from xml and parses it into the memory database
        public static void loadConfigV2(string filePath, List<Category> parsedCatagoryList, List<Mod> userMods)
        {
            Utils.appendToLog(string.Format("Loading mod selections v2.0 from {0}", filePath));
            List<string> savedConfigList = new List<string>();
            XPathDocument doc;
            string[] filePathSplit = filePath.Split(',');
            if (filePathSplit.Count() > 1)
            {
                // go here, if the config file selected is a developerSelection config and stored at the modInfo.dat file
                Utils.appendToLog("parsing developerSelection file: " + filePath);
                string xmlString = getStringFromZip(filePathSplit[0], filePathSplit[1]);
                StringReader rdr = new StringReader(xmlString);
                doc = new XPathDocument(rdr);
            }
            else
            {
                Utils.appendToLog("parsing config file: " + filePath);
                doc = new XPathDocument(filePath);
            }

            foreach (var mod in doc.CreateNavigator().Select("//relhaxMods/mod"))
            {
                savedConfigList.Add(mod.ToString());
            }
            foreach (Category c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    if (m.visible)
                    {
                        if (savedConfigList.Contains(m.packageName))
                        {
                            savedConfigList.Remove(m.packageName);
                            if (!m.enabled)
                            {
                                MessageBox.Show(string.Format(Translations.getTranslatedString("modDeactivated"), m.name), Translations.getTranslatedString("information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                m.Checked = true;
                                if (m.modFormCheckBox != null)
                                {
                                    if (m.modFormCheckBox is ModFormCheckBox)
                                    {
                                        ModFormCheckBox mfcb = (ModFormCheckBox)m.modFormCheckBox;
                                        mfcb.Checked = true;
                                        if (!Settings.disableColorChange)
                                            mfcb.Parent.BackColor = System.Drawing.Color.BlanchedAlmond;
                                    }
                                    else if (m.modFormCheckBox is ModWPFCheckBox)
                                    {
                                        ModWPFCheckBox mfCB2 = (ModWPFCheckBox)m.modFormCheckBox;
                                        mfCB2.IsChecked = true;
                                    }
                                }
                                Utils.appendToLog(string.Format("Checking mod {0}", m.name));
                            }
                        }
                        else
                        {
                            //uncheck
                            if (m.modFormCheckBox != null)
                            {
                                if (m.modFormCheckBox is ModFormCheckBox)
                                {
                                    ModFormCheckBox mfcb = (ModFormCheckBox)m.modFormCheckBox;
                                    mfcb.Checked = false;
                                    mfcb.Parent.BackColor = Settings.getBackColor();
                                }
                                else if (m.modFormCheckBox is ModWPFCheckBox)
                                {
                                    ModWPFCheckBox mfCB2 = (ModWPFCheckBox)m.modFormCheckBox;
                                    mfCB2.IsChecked = false;
                                }
                            }
                        }
                        if (m.configs.Count > 0)
                        {
                            loadProcessConfigsV2(m.name, m.configs, ref savedConfigList);
                        }
                    }
                }
            }
            List<string> savedUserConfigList = new List<string>();
            foreach (var userMod in doc.CreateNavigator().Select("//userMods/mod"))
            {
                savedUserConfigList.Add(userMod.ToString());
            }
            foreach (Mod um in userMods)
            {
                if (savedUserConfigList.Contains(um.name))
                {
                    string filename = um.name + ".zip";
                    if (System.IO.File.Exists(Path.Combine(Application.StartupPath, "RelHaxUserMods", filename)))
                    {
                        //it will be done in the UI code
                        um.Checked = true;
                        if (um.modFormCheckBox != null)
                        {
                            ModFormCheckBox mfcb = (ModFormCheckBox)um.modFormCheckBox;
                            mfcb.Checked = true;
                        }
                        Utils.appendToLog(string.Format("Checking user mod {0}", um.zipFile));
                    }
                }
            }
            if (savedConfigList.Count > 0)
            {
                string modsNotFoundList = "";
                foreach (var s in savedConfigList)
                {
                    modsNotFoundList += "\n" + s;
                }
                MessageBox.Show(string.Format(Translations.getTranslatedString("modsNotFoundTechnical"), modsNotFoundList), Translations.getTranslatedString("information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            Utils.appendToLog("Finished loading mod selections v2.0");
        }

        private static void loadProcessConfigsV1(XmlNode holder, Mod m, bool parentIsMod, Config con = null)
        {
            foreach (XmlNode nnn in holder.ChildNodes)
            {
                if (parentIsMod)
                {
                    if (m == null)
                    {
                        continue;
                    }
                }
                else
                {
                    if (con == null)
                    {
                        continue;
                    }
                }
                Config c = new Config();
                foreach (XmlNode nnnn in nnn.ChildNodes)
                {
                    switch (nnnn.Name)
                    {
                        case "name":
                            if (parentIsMod)
                            {
                                c = m.getConfig(nnnn.InnerText);
                                if ((c != null) && (!c.visible))
                                    return;
                            }
                            else
                            {
                                c = con.getSubConfig(nnnn.InnerText);
                                if ((c != null) && (!c.visible))
                                    return;
                            }
                            if (c == null)
                            {
                                Utils.appendToLog(string.Format("WARNING: config \"{0}\" not found for mod/config \"{1}\"", nnnn.InnerText, holder.InnerText));
                                MessageBox.Show(string.Format(Translations.getTranslatedString("configNotFound"), nnnn.InnerText, holder.InnerText));
                                continue;
                            }
                            if (c.enabled)
                            {
                                c.Checked = true;
                                if (c.configUIComponent != null)
                                {
                                    if (c.configUIComponent is ConfigFormCheckBox)
                                    {
                                        ConfigFormCheckBox CBTemp = (ConfigFormCheckBox)c.configUIComponent;
                                        CBTemp.Checked = true;
                                        if (!Settings.disableColorChange)
                                            CBTemp.Parent.BackColor = System.Drawing.Color.BlanchedAlmond;
                                    }
                                    else if (c.configUIComponent is ConfigFormComboBox)
                                    {
                                        ConfigFormComboBox CBTemp = (ConfigFormComboBox)c.configUIComponent;
                                        foreach (Object o in CBTemp.Items)
                                        {
                                            if (o is ComboBoxItem)
                                            {
                                                ComboBoxItem tempCBI = (ComboBoxItem)o;
                                                if (tempCBI.config.packageName.Equals(c.packageName))
                                                {
                                                    CBTemp.SelectedItem = o;
                                                    break;
                                                }
                                            }
                                        }
                                        if (!Settings.disableColorChange)
                                            CBTemp.Parent.BackColor = System.Drawing.Color.BlanchedAlmond;
                                    }
                                    else if (c.configUIComponent is ConfigFormRadioButton)
                                    {
                                        ConfigFormRadioButton CBTemp = (ConfigFormRadioButton)c.configUIComponent;
                                        CBTemp.Checked = true;
                                        if (!Settings.disableColorChange)
                                            CBTemp.Parent.BackColor = System.Drawing.Color.BlanchedAlmond;
                                    }
                                    else if (c.configUIComponent is ConfigWPFCheckBox)
                                    {
                                        ConfigWPFCheckBox CBTemp = (ConfigWPFCheckBox)c.configUIComponent;
                                        CBTemp.IsChecked = true;
                                    }
                                    else if (c.configUIComponent is ConfigWPFComboBox)
                                    {
                                        ConfigWPFComboBox CBTemp = (ConfigWPFComboBox)c.configUIComponent;
                                        foreach (Object o in CBTemp.Items)
                                        {
                                            if (o is ComboBoxItem)
                                            {
                                                ComboBoxItem tempCBI = (ComboBoxItem)o;
                                                if (tempCBI.config.packageName.Equals(c.packageName))
                                                {
                                                    CBTemp.SelectedItem = o;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else if (c.configUIComponent is ConfigWPFRadioButton)
                                    {
                                        ConfigWPFRadioButton CBTemp = (ConfigWPFRadioButton)c.configUIComponent;
                                        CBTemp.IsChecked = true;
                                    }
                                }
                                Utils.appendToLog(string.Format("Checking mod {0}", c.name));
                            }
                            else
                            {
                                if (c.configUIComponent != null)
                                {
                                    if (c.configUIComponent is ConfigFormCheckBox)
                                    {
                                        ConfigFormCheckBox CBTemp = (ConfigFormCheckBox)c.configUIComponent;
                                        CBTemp.Checked = false;
                                        CBTemp.Parent.BackColor = Settings.getBackColor();
                                    }
                                    else if (c.configUIComponent is ConfigFormComboBox)
                                    {
                                        ConfigFormComboBox CBTemp = (ConfigFormComboBox)c.configUIComponent;
                                        CBTemp.Parent.BackColor = Settings.getBackColor();
                                    }
                                    else if (c.configUIComponent is ConfigFormRadioButton)
                                    {
                                        ConfigFormRadioButton CBTemp = (ConfigFormRadioButton)c.configUIComponent;
                                        CBTemp.Checked = false;
                                        CBTemp.Parent.BackColor = Settings.getBackColor();
                                    }
                                    else if (c.configUIComponent is ConfigWPFCheckBox)
                                    {
                                        ConfigWPFCheckBox CBTemp = (ConfigWPFCheckBox)c.configUIComponent;
                                        CBTemp.IsChecked = false;
                                    }
                                    else if (c.configUIComponent is ConfigWPFComboBox)
                                    {
                                        //do nothing...
                                    }
                                    else if (c.configUIComponent is ConfigWPFRadioButton)
                                    {
                                        ConfigWPFRadioButton CBTemp = (ConfigWPFRadioButton)c.configUIComponent;
                                        CBTemp.IsChecked = false;
                                    }
                                }
                            }
                            break;
                        case "configs":
                            Utils.loadProcessConfigsV1(nnnn, m, false, c);
                            break;
                        case "subConfigs":
                            Utils.loadProcessConfigsV1(nnnn, m, false, c);
                            break;
                    }
                }
            }
        }

        private static void loadProcessConfigsV2(string parentName, List<Config> configList, ref List<string> savedConfigList)
        {
            bool shouldBeBA = false;
            Panel panelRef = null;
            foreach (Config c in configList)
            {
                if (c.visible)
                {
                    if (savedConfigList.Contains(c.packageName))
                    {
                        savedConfigList.Remove(c.packageName);
                        if (!c.enabled)
                        {
                            MessageBox.Show(string.Format(Translations.getTranslatedString("configDeactivated"), c.name, parentName), Translations.getTranslatedString("information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            c.Checked = true;
                            if (c.configUIComponent != null)
                            {
                                if (c.configUIComponent is ConfigFormCheckBox)
                                {
                                    ConfigFormCheckBox CBTemp = (ConfigFormCheckBox)c.configUIComponent;
                                    CBTemp.Checked = true;
                                    shouldBeBA = true;
                                    if (CBTemp.Parent is Panel)
                                        panelRef = (Panel)CBTemp.Parent;
                                }
                                else if (c.configUIComponent is ConfigFormComboBox)
                                {
                                    ConfigFormComboBox CBTemp = (ConfigFormComboBox)c.configUIComponent;
                                    foreach (Object o in CBTemp.Items)
                                    {
                                        if (o is ComboBoxItem)
                                        {
                                            ComboBoxItem tempCBI = (ComboBoxItem)o;
                                            if (tempCBI.config.packageName.Equals(c.packageName))
                                            {
                                                CBTemp.SelectedItem = o;
                                                break;
                                            }
                                        }
                                    }
                                    shouldBeBA = true;
                                    if (CBTemp.Parent is Panel)
                                        panelRef = (Panel)CBTemp.Parent;
                                }
                                else if (c.configUIComponent is ConfigFormRadioButton)
                                {
                                    ConfigFormRadioButton CBTemp = (ConfigFormRadioButton)c.configUIComponent;
                                    CBTemp.Checked = true;
                                    shouldBeBA = true;
                                    if (CBTemp.Parent is Panel)
                                        panelRef = (Panel)CBTemp.Parent;
                                }
                                else if (c.configUIComponent is ConfigWPFCheckBox)
                                {
                                    ConfigWPFCheckBox CBTemp = (ConfigWPFCheckBox)c.configUIComponent;
                                    CBTemp.IsChecked = true;
                                }
                                else if (c.configUIComponent is ConfigWPFComboBox)
                                {
                                    ConfigWPFComboBox CBTemp = (ConfigWPFComboBox)c.configUIComponent;
                                    foreach (Object o in CBTemp.Items)
                                    {
                                        if (o is ComboBoxItem)
                                        {
                                            ComboBoxItem tempCBI = (ComboBoxItem)o;
                                            if (tempCBI.config.packageName.Equals(c.packageName))
                                            {
                                                CBTemp.SelectedItem = o;
                                                break;
                                            }
                                        }
                                    }
                                }
                                else if (c.configUIComponent is ConfigWPFRadioButton)
                                {
                                    ConfigWPFRadioButton CBTemp = (ConfigWPFRadioButton)c.configUIComponent;
                                    CBTemp.IsChecked = true;
                                }
                            }
                            Utils.appendToLog(string.Format("Checking mod {0}", c.name));
                        }
                    }
                    else
                    {
                        if (c.configUIComponent != null)
                        {
                            if (c.configUIComponent is ConfigFormCheckBox)
                            {
                                ConfigFormCheckBox CBTemp = (ConfigFormCheckBox)c.configUIComponent;
                                CBTemp.Checked = false;
                                CBTemp.Parent.BackColor = Settings.getBackColor();
                            }
                            else if (c.configUIComponent is ConfigFormComboBox)
                            {
                                ConfigFormComboBox CBTemp = (ConfigFormComboBox)c.configUIComponent;
                                CBTemp.Parent.BackColor = Settings.getBackColor();
                            }
                            else if (c.configUIComponent is ConfigFormRadioButton)
                            {
                                ConfigFormRadioButton CBTemp = (ConfigFormRadioButton)c.configUIComponent;
                                CBTemp.Checked = false;
                                CBTemp.Parent.BackColor = Settings.getBackColor();
                            }
                            else if (c.configUIComponent is ConfigWPFCheckBox)
                            {
                                ConfigWPFCheckBox CBTemp = (ConfigWPFCheckBox)c.configUIComponent;
                                CBTemp.IsChecked = false;
                            }
                            else if (c.configUIComponent is ConfigWPFComboBox)
                            {
                                //do nothing...
                            }
                            else if (c.configUIComponent is ConfigWPFRadioButton)
                            {
                                ConfigWPFRadioButton CBTemp = (ConfigWPFRadioButton)c.configUIComponent;
                                CBTemp.IsChecked = false;
                            }
                        }
                    }
                    if (c.configs.Count > 0)
                    {
                        loadProcessConfigsV2(c.name, c.configs, ref savedConfigList);
                    }
                }
            }
            if(shouldBeBA && panelRef != null)
            {
                if (!Settings.disableColorChange)
                    panelRef.BackColor = System.Drawing.Color.BlanchedAlmond;
            }
        }

        public static List<string> createUsedFilesList(List<Category> parsedCatagoryList,
            List<Dependency> globalDependencies, List<Dependency> dependencies, List<LogicalDependnecy> logicalDependencies)
        {
            List<string> currentZipFiles = new List<string>();
            foreach (Dependency d in globalDependencies)
            {
                if (!d.dependencyZipFile.Equals("") && !currentZipFiles.Contains(d.dependencyZipFile))
                {
                    currentZipFiles.Add(d.dependencyZipFile);
                }
            }
            foreach (Dependency d in dependencies)
            {
                if (!d.dependencyZipFile.Equals("") && !currentZipFiles.Contains(d.dependencyZipFile))
                {
                    currentZipFiles.Add(d.dependencyZipFile);
                }
            }
            foreach (LogicalDependnecy d in logicalDependencies)
            {
                if (!d.dependencyZipFile.Equals("") && !currentZipFiles.Contains(d.dependencyZipFile))
                {
                    currentZipFiles.Add(d.dependencyZipFile);
                }
            }
            foreach (Category cat in parsedCatagoryList)
            {
                foreach (Mod m in cat.mods)
                {

                    if (!m.zipFile.Equals("") && !currentZipFiles.Contains(m.zipFile))
                    {
                        currentZipFiles.Add(m.zipFile);
                    }
                    if (m.configs.Count > 0)
                        createUsedFilesListParseConfigs(m.configs, currentZipFiles, out currentZipFiles);
                }
            }
            return currentZipFiles;
        }

        public static void createUsedFilesListParseConfigs(List<Config> configList, List<string> currentZipFiles, out List<string> currentZipFilesOut)
        {
            foreach (Config c in configList)
            {

                if (!c.zipFile.Equals("") && !currentZipFiles.Contains(c.zipFile))
                {
                    currentZipFiles.Add(c.zipFile);
                }
                if (c.configs.Count > 0)
                    createUsedFilesListParseConfigs(c.configs, currentZipFiles, out currentZipFiles);
            }
            currentZipFilesOut = currentZipFiles;
        }

        //moved to ModSelectionList.cs
        public static List<string> depricated_createDownloadedOldZipsList(List<string> currentZipFiles, List<Category> parsedCatagoryList,
            List<Dependency> globalDependencies, List<Dependency> currentDependencies, List<LogicalDependnecy> currentLogicalDependencies)  
        {
            parsedZips = new List<string>();
            foreach (Dependency d in globalDependencies)
            {
                if (!d.dependencyZipFile.Equals("") && !parsedZips.Contains(d.dependencyZipFile))
                {
                    parsedZips.Add(d.dependencyZipFile);
                }
            }
            foreach (Dependency d in currentDependencies)
            {
                if (!d.dependencyZipFile.Equals("") && !parsedZips.Contains(d.dependencyZipFile))
                {
                    parsedZips.Add(d.dependencyZipFile);
                }
            }
            foreach (LogicalDependnecy d in currentLogicalDependencies)
            {
                if (!d.dependencyZipFile.Equals("") && !parsedZips.Contains(d.dependencyZipFile))
                {
                    parsedZips.Add(d.dependencyZipFile);
                }
            }
            foreach (Category cat in parsedCatagoryList)
            {
                foreach (Mod m in cat.mods)
                {
                    
                    if (!m.zipFile.Equals("") && !parsedZips.Contains(m.zipFile))
                    {
                        parsedZips.Add(m.zipFile);
                    }
                    if (m.configs.Count > 0)
                        depricated_parseZipFileConfigs(m.configs);
                }
            }
            //now parsedZips has every single possible zipFile in the database
            //for each zipfile in it, remove it in currentZipFiles if it exists
            foreach (string s in parsedZips)
            {
                if(currentZipFiles.Contains(s))
                    currentZipFiles.Remove(s);
            }
            return currentZipFiles;
        }

        public static void depricated_parseZipFileConfigs(List<Config> configList)
        {
            foreach (Config c in configList)
            {
                
                if (!c.zipFile.Equals("") && !parsedZips.Contains(c.zipFile))
                {
                    parsedZips.Add(c.zipFile);
                }
                if (c.configs.Count > 0)
                    depricated_parseZipFileConfigs(c.configs);
            }
        }

        private static void parseDeveloperSelections(XDocument doc)
        {
            DeveloperSelections d;
            var xMembers = from members in doc.Descendants("selections").Elements() select members;
            foreach (XElement x in xMembers)
            {
                d = new DeveloperSelections();
                d.internalName = x.Value;
                d.displayName = x.Attribute("displayName").Value;
                d.date = x.Attribute("date").Value;
                MainWindow.developerSelections.Add(d);
            }
        }

        //deletes all empty directories from a given start location
        public static void processDirectory(string startLocation, bool reportToLog = true)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                processDirectory(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    if(reportToLog)
                        Utils.appendToLog(string.Format("Deleting empty directory {0}", directory));
                    Directory.Delete(directory, false);
                }
            }
        }
        //returns true if the CRC's of each file match, false otherwise
        public static bool CRCsMatch(string localFile, string remoteCRC)
        {
            if (!System.IO.File.Exists(localFile))
                return false;
            string crc = Utils.getMd5Hash(localFile);
            if (crc.Equals(remoteCRC))
                return true;
            return false;
        }

        //Downloads the forum page. Totally not stat padding
        public static void TotallyNotStatPaddingForumPageViewCount()
        {
            using (BackgroundWorker worker = new BackgroundWorker())
            {
                worker.DoWork += worker_TotallyNotStatPaddingForumPageViewCount;
                worker.RunWorkerAsync();
            }
        }

        //Downloads the forum page. Totally not stat padding
        public static void worker_TotallyNotStatPaddingForumPageViewCount(object sender, DoWorkEventArgs args)
        {
            //create a new downloader to download the modpack forum page on a new thread
            using (WebClient client = new WebClient())
            {
                try
                {
                    client.DownloadString("http://forum.worldoftanks.eu/index.php?/topic/623269-");
                    client.DownloadString("http://forum.worldoftanks.com/index.php?/topic/535868-");
                    client.DownloadString("http://forum.worldoftanks.eu/index.php?/topic/624499-");
                }
                catch (Exception e)
                {
                    Utils.exceptionLog("Tried to access one of the forum URL's", e);
                }
            }
        }

        // https://stackoverflow.com/questions/14488796/does-net-provide-an-easy-way-convert-bytes-to-kb-mb-gb-etc
        static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static string SizeSuffix(Int64 value, int decimalPlaces = 1, bool sizeSuffix = false)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { if (sizeSuffix) return "0.0 bytes"; else return "0.0"; }
            if (value < 1000) { if (sizeSuffix) return string.Format("{0:n" + decimalPlaces + "} {1}", 0.1, SizeSuffixes[1]); else return string.Format("{0:n" + decimalPlaces + "}", 0.1); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            if (sizeSuffix)
                return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[mag]);
            else
                return string.Format("{0:n" + decimalPlaces + "}", adjustedSize);
        }

        public static string getValidFilename(String fileName)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
               fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        private static Random random = new Random();
        /// <summary>
        /// https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings-in-c
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        //convert string with CR and/or LF from Xml save format
        private static string convertFromXmlSaveFormat(string s)
        {
            return s.TrimEnd().Replace("@","\n").Replace(@"\r", "\r").Replace(@"\t", "\t").Replace(@"\n", "\n").Replace(@"&#92;", @"\");
        }

        //convert string with CR and/or LF to Xml save format
        private static string convertToXmlSaveFormat(string s)
        {
            return s.TrimEnd().Replace(@"\", @"&#92;").Replace("\r", @"\r").Replace("\t", @"\t").Replace("\n", @"\n");
        }

        //saves the mod database
        public static void SaveDatabase(string saveLocation, string gameVersion, string onlineFolderVersion, List<Dependency> globalDependencies, List<Dependency> dependencies, List<LogicalDependnecy> logicalDependencies, List<Category> parsedCatagoryList)
        {
            XmlDocument doc = new XmlDocument();
            //database root modInfo.xml
            XmlElement root = doc.CreateElement("modInfoAlpha.xml");
            root.SetAttribute("version", gameVersion);
            root.SetAttribute("onlineFolder", onlineFolderVersion);
            doc.AppendChild(root);
            //global dependencies
            XmlElement globalDependenciesXml = doc.CreateElement("globaldependencies");
            foreach (Dependency d in globalDependencies)
            {
                //declare dependency root
                XmlElement globalDependencyRoot = doc.CreateElement("globaldependency");
                //make dependency
                XmlElement globalDepZipFile = doc.CreateElement("dependencyZipFile");
                if (!d.dependencyZipFile.Trim().Equals(""))
                    globalDepZipFile.InnerText = d.dependencyZipFile.Trim();
                globalDependencyRoot.AppendChild(globalDepZipFile);
                XmlElement globalDepTimestamp = doc.CreateElement("timestamp");
                if (d.timestamp != 0)
                    globalDepTimestamp.InnerText = d.timestamp.ToString();
                globalDependencyRoot.AppendChild(globalDepTimestamp);
                XmlElement globalDepStartAddress = doc.CreateElement("startAddress");
                if (!d.startAddress.Trim().Equals(""))
                    globalDepStartAddress.InnerText = d.startAddress.Trim();
                globalDependencyRoot.AppendChild(globalDepStartAddress);
                XmlElement globalDepEndAddress = doc.CreateElement("endAddress");
                if (!d.endAddress.Trim().Equals(""))
                    globalDepEndAddress.InnerText = d.endAddress.Trim();
                globalDependencyRoot.AppendChild(globalDepEndAddress);
                XmlElement globalDepURL = doc.CreateElement("devURL");
                if (!d.devURL.Trim().Equals(""))
                    globalDepURL.InnerText = d.devURL.Trim();
                globalDependencyRoot.AppendChild(globalDepURL);
                XmlElement globalDepCRC = doc.CreateElement("dependencyZipCRC");
                if (!d.dependencyZipCRC.Trim().Equals(""))
                    globalDepCRC.InnerText = d.dependencyZipCRC.Trim();
                globalDependencyRoot.AppendChild(globalDepCRC);
                XmlElement globalDepEnabled = doc.CreateElement("dependencyenabled");
                if (!d.enabled.ToString().Trim().Equals(""))
                    globalDepEnabled.InnerText = "" + d.enabled;
                globalDependencyRoot.AppendChild(globalDepEnabled);
                XmlElement globalDepAppendExtraction = doc.CreateElement("appendExtraction");
                if (!d.appendExtraction.ToString().Trim().Equals(""))
                    globalDepAppendExtraction.InnerText = "" + d.appendExtraction;
                globalDependencyRoot.AppendChild(globalDepAppendExtraction);
                XmlElement globalDepPackageName = doc.CreateElement("packageName");
                if (!d.packageName.Trim().Equals(""))
                    globalDepPackageName.InnerText = d.packageName.Trim();
                globalDependencyRoot.AppendChild(globalDepPackageName);
                //attach dependency root
                globalDependenciesXml.AppendChild(globalDependencyRoot);
            }
            root.AppendChild(globalDependenciesXml);
            //dependencies
            XmlElement DependenciesXml = doc.CreateElement("dependencies");
            foreach (Dependency d in dependencies)
            {
                //declare dependency root
                XmlElement dependencyRoot = doc.CreateElement("dependency");
                //make dependency
                XmlElement depZipFile = doc.CreateElement("dependencyZipFile");
                if (!d.dependencyZipFile.Trim().Equals(""))
                    depZipFile.InnerText = d.dependencyZipFile.Trim();
                dependencyRoot.AppendChild(depZipFile);
                XmlElement depTimestamp = doc.CreateElement("timestamp");
                if (d.timestamp != 0)
                    depTimestamp.InnerText = d.timestamp.ToString();
                dependencyRoot.AppendChild(depTimestamp);
                XmlElement depStartAddress = doc.CreateElement("startAddress");
                if (!d.startAddress.Trim().Equals(""))
                    depStartAddress.InnerText = d.startAddress.Trim();
                dependencyRoot.AppendChild(depStartAddress);
                XmlElement depEndAddress = doc.CreateElement("endAddress");
                if (!d.endAddress.Trim().Equals(""))
                    depEndAddress.InnerText = d.endAddress.Trim();
                dependencyRoot.AppendChild(depEndAddress);
                XmlElement depdevURL = doc.CreateElement("devURL");
                if (!d.devURL.Trim().Equals(""))
                    depdevURL.InnerText = d.devURL.Trim();
                dependencyRoot.AppendChild(depdevURL);
                XmlElement depCRC = doc.CreateElement("dependencyZipCRC");
                if (!d.dependencyZipCRC.Trim().Equals(""))
                    depCRC.InnerText = d.dependencyZipCRC.Trim();
                dependencyRoot.AppendChild(depCRC);
                XmlElement depEnabled = doc.CreateElement("dependencyenabled");
                if (!d.enabled.ToString().Trim().Equals(""))
                    depEnabled.InnerText = "" + d.enabled;
                dependencyRoot.AppendChild(depEnabled);
                XmlElement depAppendExtraction = doc.CreateElement("appendExtraction");
                if (!d.appendExtraction.ToString().Trim().Equals(""))
                    depAppendExtraction.InnerText = "" + d.appendExtraction;
                dependencyRoot.AppendChild(depAppendExtraction);
                XmlElement depPackageName = doc.CreateElement("packageName");
                if (!d.packageName.Trim().Equals(""))
                    depPackageName.InnerText = d.packageName.Trim();
                dependencyRoot.AppendChild(depPackageName);
                //logicalDependencies for the configs
                XmlElement depLogicalDependencies = doc.CreateElement("logicalDependencies");
                foreach (LogicalDependnecy ld in d.logicalDependencies)
                {
                    //declare logicalDependency root
                    XmlElement LogicalDependencyRoot = doc.CreateElement("logicalDependency");
                    //make logicalDependency
                    XmlElement LogicalDependencyPackageName = doc.CreateElement("packageName");
                    if (!ld.packageName.Trim().Equals(""))
                        LogicalDependencyPackageName.InnerText = ld.packageName.Trim();
                    LogicalDependencyRoot.AppendChild(LogicalDependencyPackageName);
                    XmlElement LogicalDependencyNegateFlag = doc.CreateElement("negateFlag");
                    if (!ld.negateFlag.ToString().Trim().Equals(""))
                        LogicalDependencyNegateFlag.InnerText = "" + ld.negateFlag;
                    LogicalDependencyRoot.AppendChild(LogicalDependencyNegateFlag);
                    //attach logicalDependency root
                    depLogicalDependencies.AppendChild(LogicalDependencyRoot);
                }
                dependencyRoot.AppendChild(depLogicalDependencies);
                XmlElement shortCuts = doc.CreateElement("shortCuts");
                foreach (ShortCut sc in d.shortCuts)
                {
                    //declare ShortCut root
                    XmlElement ShortCutRoot = doc.CreateElement("shortCut");
                    //make ShortCut
                    XmlElement ShortCutPath = doc.CreateElement("path");
                    if (!sc.path.Trim().Equals(""))
                        ShortCutPath.InnerText = sc.path.Trim();
                    ShortCutRoot.AppendChild(ShortCutPath);
                    XmlElement ShortCutName = doc.CreateElement("name");
                    if (!sc.name.Trim().Equals(""))
                        ShortCutName.InnerText = sc.name.Trim();
                    ShortCutRoot.AppendChild(ShortCutName);
                    XmlElement ShortCutEnabled = doc.CreateElement("enabled");
                    ShortCutEnabled.InnerText = sc.enabled.ToString().Trim();
                    ShortCutRoot.AppendChild(ShortCutEnabled);
                    shortCuts.AppendChild(ShortCutRoot);
                }
                //attach ShortCuts to root
                dependencyRoot.AppendChild(shortCuts);
                DependenciesXml.AppendChild(dependencyRoot);
            }
            root.AppendChild(DependenciesXml);
            //dependencies
            XmlElement logicalDependenciesXml = doc.CreateElement("logicalDependencies");
            foreach (LogicalDependnecy d in logicalDependencies)
            {
                //declare dependency root
                XmlElement logicalDependencyRoot = doc.CreateElement("logicalDependency");
                //make dependency
                XmlElement logicalDepZipFile = doc.CreateElement("dependencyZipFile");
                if (!d.dependencyZipFile.Trim().Equals(""))
                    logicalDepZipFile.InnerText = d.dependencyZipFile.Trim();
                logicalDependencyRoot.AppendChild(logicalDepZipFile);
                XmlElement logicalDepTimestamp = doc.CreateElement("timestamp");
                if (d.timestamp != 0)
                    logicalDepTimestamp.InnerText = d.timestamp.ToString();
                logicalDependencyRoot.AppendChild(logicalDepTimestamp);
                XmlElement logicalDepStartAddress = doc.CreateElement("startAddress");
                if (!d.startAddress.Trim().Equals(""))
                    logicalDepStartAddress.InnerText = d.startAddress.Trim();
                logicalDependencyRoot.AppendChild(logicalDepStartAddress);
                XmlElement logicalDepEndAddress = doc.CreateElement("endAddress");
                if (!d.endAddress.Trim().Equals(""))
                    logicalDepEndAddress.InnerText = d.endAddress.Trim();
                logicalDependencyRoot.AppendChild(logicalDepEndAddress);
                XmlElement logicalDepdevURL = doc.CreateElement("devURL");
                if (!d.devURL.Trim().Equals(""))
                    logicalDepdevURL.InnerText = d.devURL.Trim();
                logicalDependencyRoot.AppendChild(logicalDepdevURL);
                XmlElement logicalDepCRC = doc.CreateElement("dependencyZipCRC");
                if (!d.dependencyZipCRC.Trim().Equals(""))
                    logicalDepCRC.InnerText = d.dependencyZipCRC.Trim();
                logicalDependencyRoot.AppendChild(logicalDepCRC);
                XmlElement logicalDepEnabled = doc.CreateElement("dependencyenabled");
                if (!d.enabled.ToString().Trim().Equals(""))
                    logicalDepEnabled.InnerText = "" + d.enabled;
                logicalDependencyRoot.AppendChild(logicalDepEnabled);
                XmlElement logicalDepPackageName = doc.CreateElement("packageName");
                if (!d.packageName.Trim().Equals(""))
                    logicalDepPackageName.InnerText = d.packageName.Trim();
                logicalDependencyRoot.AppendChild(logicalDepPackageName);
                XmlElement shortCuts = doc.CreateElement("shortCuts");
                foreach (ShortCut sc in d.shortCuts)
                {
                    //declare ShortCut root
                    XmlElement ShortCutRoot = doc.CreateElement("shortCut");
                    //make ShortCut
                    XmlElement ShortCutPath = doc.CreateElement("path");
                    if (!sc.path.Trim().Equals(""))
                        ShortCutPath.InnerText = sc.path.Trim();
                    ShortCutRoot.AppendChild(ShortCutPath);
                    XmlElement ShortCutName = doc.CreateElement("name");
                    if (!sc.name.Trim().Equals(""))
                        ShortCutName.InnerText = sc.name.Trim();
                    ShortCutRoot.AppendChild(ShortCutName);
                    XmlElement ShortCutEnabled = doc.CreateElement("enabled");
                    ShortCutEnabled.InnerText = sc.enabled.ToString().Trim();
                    ShortCutRoot.AppendChild(ShortCutEnabled);
                    shortCuts.AppendChild(ShortCutRoot);
                }
                //attach ShortCuts to root
                logicalDependencyRoot.AppendChild(shortCuts);
                //attach dependency root
                logicalDependenciesXml.AppendChild(logicalDependencyRoot);
            }
            root.AppendChild(logicalDependenciesXml);
            //catagories
            XmlElement catagoriesHolder = doc.CreateElement("catagories");
            foreach (Category c in parsedCatagoryList)
            {
                //catagory root
                XmlElement catagoryRoot = doc.CreateElement("catagory");
                //make catagory
                XmlElement catagoryName = doc.CreateElement("name");
                if (!c.name.Trim().Equals(""))
                    catagoryName.InnerText = c.name.Trim();
                catagoryRoot.AppendChild(catagoryName);
                XmlElement catagorySelectionType = doc.CreateElement("selectionType");
                if (!c.selectionType.Trim().Equals(""))
                    catagorySelectionType.InnerText = c.selectionType;
                catagoryRoot.AppendChild(catagorySelectionType);
                //dependencies for catagory
                XmlElement catagoryDependencies = doc.CreateElement("dependencies");
                foreach (Dependency d in c.dependencies)
                {
                    //declare dependency root
                    XmlElement DependencyRoot = doc.CreateElement("dependency");
                    XmlElement DepPackageName = doc.CreateElement("packageName");
                    if (!d.packageName.Trim().Equals(""))
                        DepPackageName.InnerText = d.packageName.Trim();
                    DependencyRoot.AppendChild(DepPackageName);
                    //attach dependency root
                    catagoryDependencies.AppendChild(DependencyRoot);
                }
                catagoryRoot.AppendChild(catagoryDependencies);
                //mods for catagory
                XmlElement modsHolder = doc.CreateElement("mods");
                foreach (Mod m in c.mods)
                {
                    //add it to the list
                    XmlElement modRoot = doc.CreateElement("mod");
                    XmlElement modName = doc.CreateElement("name");
                    if (!m.name.Trim().Equals(""))
                        modName.InnerText = m.name.Trim();
                    modRoot.AppendChild(modName);
                    XmlElement modVersion = doc.CreateElement("version");
                    if (!m.version.Trim().Equals(""))
                        modVersion.InnerText = m.version.Trim();
                    modRoot.AppendChild(modVersion);
                    XmlElement modZipFile = doc.CreateElement("zipFile");
                    if (!m.zipFile.Trim().Equals(""))
                        modZipFile.InnerText = m.zipFile.Trim();
                    modRoot.AppendChild(modZipFile);
                    XmlElement modTimestamp = doc.CreateElement("timestamp");
                    if (m.timestamp != 0)
                        modTimestamp.InnerText = m.timestamp.ToString();
                    modRoot.AppendChild(modTimestamp);
                    XmlElement modStartAddress = doc.CreateElement("startAddress");
                    if (!m.startAddress.Trim().Equals(""))
                        modStartAddress.InnerText = m.startAddress.Trim();
                    modRoot.AppendChild(modStartAddress);
                    XmlElement modEndAddress = doc.CreateElement("endAddress");
                    if (!m.endAddress.Trim().Equals(""))
                        modEndAddress.InnerText = m.endAddress.Trim();
                    modRoot.AppendChild(modEndAddress);
                    XmlElement modZipCRC = doc.CreateElement("crc");
                    if (!m.crc.Trim().Equals(""))
                        modZipCRC.InnerText = m.crc.Trim();
                    modRoot.AppendChild(modZipCRC);
                    XmlElement modEnabled = doc.CreateElement("enabled");
                    if (!m.enabled.ToString().Trim().Equals(""))
                        modEnabled.InnerText = "" + m.enabled;
                    modRoot.AppendChild(modEnabled);
                    XmlElement modVisible = doc.CreateElement("visible");
                    if (!m.visible.ToString().Trim().Equals(""))
                        modVisible.InnerText = "" + m.visible;
                    modRoot.AppendChild(modVisible);
                    XmlElement modPackageName = doc.CreateElement("packageName");
                    if (!m.packageName.Trim().Equals(""))
                        modPackageName.InnerText = m.packageName.Trim();
                    modRoot.AppendChild(modPackageName);
                    XmlElement modZipSize = doc.CreateElement("size");
                    if (!m.size.ToString().Trim().Equals(""))
                        modZipSize.InnerText = "" + m.size;
                    modRoot.AppendChild(modZipSize);
                    XmlElement modUpdateComment = doc.CreateElement("updateComment");
                    if (!m.updateComment.Trim().Equals(""))
                        modUpdateComment.InnerText = convertToXmlSaveFormat(m.updateComment);
                    modRoot.AppendChild(modUpdateComment);
                    XmlElement modDescription = doc.CreateElement("description");
                    if (!m.description.Trim().Equals(""))
                        modDescription.InnerText = convertToXmlSaveFormat(m.description);
                    modRoot.AppendChild(modDescription);
                    XmlElement modDevURL = doc.CreateElement("devURL");
                    if (!m.devURL.Trim().Equals(""))
                        modDevURL.InnerText = m.devURL.Trim();
                    modRoot.AppendChild(modDevURL);
                    //datas for the mods
                    XmlElement modDatas = doc.CreateElement("userDatas");
                    foreach (string s in m.userFiles)
                    {
                        XmlElement userData = doc.CreateElement("userData");
                        if (!s.Trim().Equals(""))
                            userData.InnerText = s.Trim();
                        modDatas.AppendChild(userData);
                    }
                    modRoot.AppendChild(modDatas);
                    //pictures for the mods
                    XmlElement modPictures = doc.CreateElement("pictures");
                    foreach (Media p in m.pictureList)
                    {
                        XmlElement pictureRoot = doc.CreateElement("picture");
                        XmlElement pictureType = doc.CreateElement("type");
                        XmlElement pictureURL = doc.CreateElement("URL");
                        if (!p.URL.Trim().Equals(""))
                            pictureURL.InnerText = p.URL.Trim();
                        if (!p.mediaType.ToString().Trim().Equals(""))
                            pictureType.InnerText = "" + (int)p.mediaType;
                        pictureRoot.AppendChild(pictureType);
                        pictureRoot.AppendChild(pictureURL);
                        modPictures.AppendChild(pictureRoot);
                    }
                    modRoot.AppendChild(modPictures);
                    //configs for the mods
                    XmlElement configsHolder = doc.CreateElement("configs");
                    //if statement here
                    if (m.configs.Count > 0)
                        saveDatabaseConfigLevel(doc, configsHolder, m.configs);
                    modRoot.AppendChild(configsHolder);
                    XmlElement modDependencies = doc.CreateElement("dependencies");
                    foreach (Dependency d in m.dependencies)
                    {
                        //declare dependency root
                        XmlElement DependencyRoot = doc.CreateElement("dependency");
                        //make dependency
                        XmlElement DepPackageName = doc.CreateElement("packageName");
                        if (!d.packageName.Trim().Equals(""))
                            DepPackageName.InnerText = d.packageName.Trim();
                        DependencyRoot.AppendChild(DepPackageName);
                        //attach dependency root
                        modDependencies.AppendChild(DependencyRoot);
                    }
                    modRoot.AppendChild(modDependencies);
                    //logicalDependencies for the configs
                    XmlElement modLogicalDependencies = doc.CreateElement("logicalDependencies");
                    foreach (LogicalDependnecy ld in m.logicalDependencies)
                    {
                        //declare logicalDependency root
                        XmlElement LogicalDependencyRoot = doc.CreateElement("logicalDependency");
                        //make logicalDependency
                        XmlElement LogicalDependencyPackageName = doc.CreateElement("packageName");
                        if (!ld.packageName.Trim().Equals(""))
                            LogicalDependencyPackageName.InnerText = ld.packageName.Trim();
                        LogicalDependencyRoot.AppendChild(LogicalDependencyPackageName);
                        XmlElement LogicalDependencyNegateFlag = doc.CreateElement("negateFlag");
                        if (!ld.negateFlag.ToString().Trim().Equals(""))
                            LogicalDependencyNegateFlag.InnerText = "" + ld.negateFlag;
                        LogicalDependencyRoot.AppendChild(LogicalDependencyNegateFlag);
                        //attach logicalDependency root
                        modLogicalDependencies.AppendChild(LogicalDependencyRoot);
                    }
                    modRoot.AppendChild(modLogicalDependencies);
                    XmlElement shortCuts = doc.CreateElement("shortCuts");
                    foreach (ShortCut sc in m.shortCuts)
                    {
                        //declare ShortCut root
                        XmlElement ShortCutRoot = doc.CreateElement("shortCut");
                        //make ShortCut
                        XmlElement ShortCutPath = doc.CreateElement("path");
                        if (!sc.path.Trim().Equals(""))
                            ShortCutPath.InnerText = sc.path.Trim();
                        ShortCutRoot.AppendChild(ShortCutPath);
                        XmlElement ShortCutName = doc.CreateElement("name");
                        if (!sc.name.Trim().Equals(""))
                            ShortCutName.InnerText = sc.name.Trim();
                        ShortCutRoot.AppendChild(ShortCutName);
                        XmlElement ShortCutEnabled = doc.CreateElement("enabled");
                        ShortCutEnabled.InnerText = sc.enabled.ToString().Trim();
                        ShortCutRoot.AppendChild(ShortCutEnabled);
                        shortCuts.AppendChild(ShortCutRoot);
                    }
                    //attach ShortCuts to root
                    modRoot.AppendChild(shortCuts);
                    modsHolder.AppendChild(modRoot);
                }
                catagoryRoot.AppendChild(modsHolder);
                //append catagory
                catagoriesHolder.AppendChild(catagoryRoot);
            }
            root.AppendChild(catagoriesHolder);
            
            // Create an XML declaration.
            XmlDeclaration xmldecl;
            xmldecl = doc.CreateXmlDeclaration("1.0", null, null);
            xmldecl.Encoding = "UTF-8";
            xmldecl.Standalone = "yes";

            // Add the new node to the document.
            XmlElement cDoc = doc.DocumentElement;
            doc.InsertBefore(xmldecl, cDoc);

            // save database file
            doc.Save(saveLocation);
        }
        private static void saveDatabaseConfigLevel(XmlDocument doc, XmlElement configsHolder, List<Config> configsList)
        {
            foreach (Config cc in configsList)
            {
                //add the config to the list
                XmlElement configRoot = doc.CreateElement("config");
                configsHolder.AppendChild(configRoot);
                XmlElement configName = doc.CreateElement("name");
                if (!cc.name.Trim().Equals(""))
                    configName.InnerText = cc.name.Trim();
                configRoot.AppendChild(configName);
                XmlElement configVersion = doc.CreateElement("version");
                if (!cc.version.Trim().Equals(""))
                    configVersion.InnerText = cc.version.Trim();
                configRoot.AppendChild(configVersion);
                XmlElement configZipFile = doc.CreateElement("zipFile");
                if (!cc.zipFile.Trim().Equals(""))
                    configZipFile.InnerText = cc.zipFile.Trim();
                configRoot.AppendChild(configZipFile);
                XmlElement configTimestamp = doc.CreateElement("timestamp");
                if (cc.timestamp != 0)
                    configTimestamp.InnerText = cc.timestamp.ToString();
                configRoot.AppendChild(configTimestamp);
                XmlElement configStartAddress = doc.CreateElement("startAddress");
                if (!cc.startAddress.Trim().Equals(""))
                    configStartAddress.InnerText = cc.startAddress.Trim();
                configRoot.AppendChild(configStartAddress);
                XmlElement configEndAddress = doc.CreateElement("endAddress");
                if (!cc.endAddress.Trim().Equals(""))
                    configEndAddress.InnerText = cc.endAddress.Trim();
                configRoot.AppendChild(configEndAddress);
                XmlElement configZipCRC = doc.CreateElement("crc");
                if (!cc.crc.Trim().Equals(""))
                    configZipCRC.InnerText = cc.crc.Trim();
                configRoot.AppendChild(configZipCRC);
                XmlElement configEnabled = doc.CreateElement("enabled");
                if (!cc.enabled.ToString().Trim().Equals(""))
                    configEnabled.InnerText = "" + cc.enabled;
                configRoot.AppendChild(configEnabled);
                XmlElement configVisible = doc.CreateElement("visible");
                if (!cc.visible.ToString().Trim().Equals(""))
                    configVisible.InnerText = "" + cc.visible;
                configRoot.AppendChild(configVisible);
                XmlElement configPackageName = doc.CreateElement("packageName");
                if (!cc.packageName.Trim().Equals(""))
                    configPackageName.InnerText = cc.packageName.Trim();
                configRoot.AppendChild(configPackageName);
                XmlElement configSize = doc.CreateElement("size");
                if (!cc.size.ToString().Trim().Equals(""))
                    configSize.InnerText = "" + cc.size;
                configRoot.AppendChild(configSize);
                XmlElement configComment = doc.CreateElement("updateComment");
                if (!cc.updateComment.Trim().Equals(""))
                    configComment.InnerText = convertToXmlSaveFormat(cc.updateComment);
                configRoot.AppendChild(configComment);
                XmlElement configDescription = doc.CreateElement("description");
                if (!cc.description.Trim().Equals(""))
                    configDescription.InnerText = convertToXmlSaveFormat(cc.description);
                configRoot.AppendChild(configDescription);
                XmlElement configDevURL = doc.CreateElement("devURL");
                if (!cc.devURL.Trim().Equals(""))
                    configDevURL.InnerText = cc.devURL.Trim();
                configRoot.AppendChild(configDevURL);
                XmlElement configType = doc.CreateElement("type");
                if (!cc.type.ToString().Trim().Equals(""))
                    configType.InnerText = cc.type;
                configRoot.AppendChild(configType);
                //datas for the mods
                XmlElement configDatas = doc.CreateElement("userDatas");
                foreach (string s in cc.userFiles)
                {
                    XmlElement userData = doc.CreateElement("userData");
                    if (!s.Trim().Equals(""))
                        userData.InnerText = s.Trim();
                    configDatas.AppendChild(userData);
                }
                configRoot.AppendChild(configDatas);
                //pictures for the configs
                XmlElement configPictures = doc.CreateElement("pictures");
                foreach (Media p in cc.pictureList)
                {
                    XmlElement pictureRoot = doc.CreateElement("picture");
                    XmlElement pictureType = doc.CreateElement("type");
                    XmlElement pictureURL = doc.CreateElement("URL");
                    if (!p.URL.Trim().Equals(""))
                        pictureURL.InnerText = p.URL.Trim();
                    if (!p.mediaType.ToString().Trim().Equals(""))
                        pictureType.InnerText = "" + (int)p.mediaType;
                    pictureRoot.AppendChild(pictureType);
                    pictureRoot.AppendChild(pictureURL);
                    configPictures.AppendChild(pictureRoot);
                }
                configRoot.AppendChild(configPictures);
                //configs for the configs (meta)
                XmlElement configsHolderSub = doc.CreateElement("configs");
                //if statement here
                if (cc.configs.Count > 0)
                    saveDatabaseConfigLevel(doc, configsHolderSub, cc.configs);
                configRoot.AppendChild(configsHolderSub);
                //dependencies for the configs
                XmlElement catDependencies = doc.CreateElement("dependencies");
                foreach (Dependency d in cc.dependencies)
                {
                    //declare dependency root
                    XmlElement DependencyRoot = doc.CreateElement("dependency");
                    //make dependency
                    XmlElement DepPackageName = doc.CreateElement("packageName");
                    if (!d.packageName.Trim().Equals(""))
                        DepPackageName.InnerText = d.packageName.Trim();
                    DependencyRoot.AppendChild(DepPackageName);
                    //attach dependency root
                    catDependencies.AppendChild(DependencyRoot);
                }
                configRoot.AppendChild(catDependencies);
                //logicalDependencies for the configs
                XmlElement conLogicalDependencies = doc.CreateElement("logicalDependencies");
                foreach (LogicalDependnecy ld in cc.logicalDependencies)
                {
                    //declare logicalDependency root
                    XmlElement LogicalDependencyRoot = doc.CreateElement("logicalDependency");
                    //make logicalDependency
                    XmlElement LogicalDependencyPackageName = doc.CreateElement("packageName");
                    if (!ld.packageName.Trim().Equals(""))
                        LogicalDependencyPackageName.InnerText = ld.packageName.Trim();
                    LogicalDependencyRoot.AppendChild(LogicalDependencyPackageName);
                    XmlElement LogicalDependencyNegateFlag = doc.CreateElement("negateFlag");
                    if (!ld.negateFlag.ToString().Trim().Equals(""))
                        LogicalDependencyNegateFlag.InnerText = "" + ld.negateFlag;
                    LogicalDependencyRoot.AppendChild(LogicalDependencyNegateFlag);
                    //attach logicalDependency root
                    conLogicalDependencies.AppendChild(LogicalDependencyRoot);
                }
                configRoot.AppendChild(conLogicalDependencies);
                XmlElement shortCuts = doc.CreateElement("shortCuts");
                foreach (ShortCut sc in cc.shortCuts)
                {
                    //declare ShortCut root
                    XmlElement ShortCutRoot = doc.CreateElement("shortCut");
                    //make ShortCut
                    XmlElement ShortCutPath = doc.CreateElement("path");
                    if (!sc.path.Trim().Equals(""))
                        ShortCutPath.InnerText = sc.path.Trim();
                    ShortCutRoot.AppendChild(ShortCutPath);
                    XmlElement ShortCutName = doc.CreateElement("name");
                    if (!sc.name.Trim().Equals(""))
                        ShortCutName.InnerText = sc.name.Trim();
                    ShortCutRoot.AppendChild(ShortCutName);
                    XmlElement ShortCutEnabled = doc.CreateElement("enabled");
                    ShortCutEnabled.InnerText = sc.enabled.ToString().Trim();
                    ShortCutRoot.AppendChild(ShortCutEnabled);
                    shortCuts.AppendChild(ShortCutRoot);
                }
                //attach ShortCuts to root
                configRoot.AppendChild(shortCuts);
                configsHolder.AppendChild(configRoot);
            }
        }

        //https://stackoverflow.com/questions/5977445/how-to-get-windows-display-settings
        public static float GetDisplayScale(Graphics graphics)
        {
            //get the DPI setting
            float dpiX, dpiY;
            dpiX = graphics.DpiX;
            dpiY = graphics.DpiY;
            if(dpiX != dpiY)
            {
                Utils.appendToLog("WARNING: scale values do not equal, using x value");
            }
            return dpiX / 96;
        }

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        public enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117,

            // http://pinvoke.net/default.aspx/gdi32/GetDeviceCaps.html
        }

        public static float getScalingFactor()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();
            int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);

            float ScreenScalingFactor = (float)PhysicalScreenHeight / (float)LogicalScreenHeight;

            return ScreenScalingFactor; // 1.25 = 125%
        }

        // https://social.msdn.microsoft.com/Forums/vstudio/en-US/92a36534-0f01-4425-ab63-c5f8830d64ae/help-please-with-dotnetzip-extracting-data-form-ziped-file?forum=csharpgeneral
        public static string getStringFromZip(string zipFilename, string archivedFilename, string password = null)
        {
            MemoryStream ms = new MemoryStream();
            string textStr = "";
            if (System.IO.File.Exists(zipFilename))
            {
                using (ZipFile zip = ZipFile.Read(zipFilename))
                {
                    ZipEntry e = zip[archivedFilename];
                    if (password != null)
                    {
                        e.ExtractWithPassword(ms, password);
                    }
                    else
                    {
                        e.Extract(ms);
                    }
                    StreamReader sr = new StreamReader(ms);
                    ms.Position = 0;
                    textStr = sr.ReadToEnd();
                }
            } 
            else
            {
                Utils.appendToLog(string.Format("ERROR: {0} not found", zipFilename));
            }
            return textStr;
        }

        public static bool convertDateToLocalCultureFormat(string date, out string dateOut)
        {
            DateTimeFormatInfo myDTFI = new CultureInfo("en-US").DateTimeFormat;
            dateOut = date;
            string[] mask = new string[] { "dd.MM.yyyy  h:mm:ss,ff", "dd.MM.yyyy HH:mm:ss,ff", "YYYY-MM-DD  h:mm:ss", "YYYY-MM-DD HH:mm:ss", "YYYY-MM-DD HH:mm:ss.ff", "YYYY-MM-DD  h:mm:ss.ff", "MM/DD/YYYY  h:mm:ss.ff",
                "MM/DD/YYYY HH:mm:ss.ff", "ddd MM/DD/YYYY  h:mm:ss.ff", "ddd MM/DD/YYYY HH:mm:ss.ff","ddd M/d/yyyy h:mm:ss.ff","ddd M/d/yyyy H:mm:ss.ff", "yyyy-MM-dd HH:mm:ss"};
            foreach (var m in mask)
            {
                if (DateTime.TryParseExact(date, m, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.AllowInnerWhite | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite, out DateTime dateValue))
                {
                    dateOut = dateValue.ToString();
                    return true;
                }
            }
            return false;
        }

        public static void DirectoryDelete(string folderPath, bool doSubfolder = false, bool deleteTopfolder = false)
        {
            foreach (string file in Directory.GetFiles(folderPath))
            {
                try
                {
                    System.IO.File.Delete(file);
                }
                catch (Exception ex)
                {
                    Utils.exceptionLog("DirectoryDelete", "Filename=" + file, ex);
                }
            }

            if (doSubfolder)
            {
                foreach (string dir in Directory.GetDirectories(folderPath))
                {
                    try
                    {
                        DirectoryDelete(dir, doSubfolder, true);
                    }
                    catch (Exception ex)
                    {
                        Utils.exceptionLog("DirectoryDelete", "Folder=" + dir, ex);
                    }
                }
            }

            try
            {
                if (deleteTopfolder) Directory.Delete(folderPath);
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("DirectoryDelete", "Folder=" + folderPath, ex);
            }
        }

        // https://stackoverflow.com/questions/30494/compare-version-identifiers
        /// <summary>
        /// Compare versions of form "1,2,3,4" or "1.2.3.4". Throws FormatException
        /// in case of invalid version. See function comments for more informations and samples.
        /// </summary>
        /// <param name="strA">the first version</param>
        /// <param name="strB">the second version</param>
        /// <returns>less than zero if strA is less than strB, equal to zero if
        /// strA equals strB, and greater than zero if strA is greater than strB
        /// Samples:
        /// 1.0.0.0     | 1.0.0.1 = -1
        /// 1.0.0.1     | 1.0.0.0 =  1
        /// 1.0.0.0     | 1.0.0.0 =  0
        /// 1, 0.0.0    | 1.0.0.0 =  0
        /// 9, 5, 1, 44 | 3.4.5.6 =  1
        /// 1, 5, 1, 44 | 3.4.5.6 = -1
        /// 6,5,4,3     | 6.5.4.3 =  0</returns>
        public static int CompareVersions(String strA, String strB)
        {
            Version vA = new Version(strA.Replace(",", "."));
            Version vB = new Version(strB.Replace(",", "."));

            return vA.CompareTo(vB);
        }

        public static long getCurrentUniversalFiletimeTimestamp()
        {
            return DateTime.Now.ToUniversalTime().ToFileTime();
        }

        public static string convertFiletimeTimestampToDate(long timestamp)
        {
            return DateTime.FromFileTime(timestamp).ToString();
        }

        // https://stackoverflow.com/questions/4897655/create-shortcut-on-desktop-c-sharp
        /// <summary>Creates or removes a shortcut at the specified pathname.</summary> 
        /// <param name="shortcutTarget">The path where the original file is located.</param> 
        /// <param name="shortcutName">The filename of the shortcut to be created or removed from desktop including the (.lnk) extension.</param>
        /// <param name="create">True to create a shortcut or False to remove the shortcut.</param> 
        public static void CreateShortcut(string shortcutTarget, string shortcutName, bool create, bool log, StreamWriter sw)
        {
            string modifiedName = Path.GetFileNameWithoutExtension(shortcutName) + ".lnk";
            if (create)
            {
                try
                {
                    IShellLink link = (IShellLink)new ShellLink();

                    // setup shortcut information
                    link.SetDescription("created by the Relhax Manager");
                    link.SetPath(@shortcutTarget);
                    link.SetIconLocation(@shortcutTarget, 0);
                    link.SetWorkingDirectory(Path.GetDirectoryName(@shortcutTarget));
                    link.SetArguments(""); //The arguments used when executing the exe
                    // save it
                    System.Runtime.InteropServices.ComTypes.IPersistFile file = (System.Runtime.InteropServices.ComTypes.IPersistFile)link;
                    string desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), modifiedName);
                    if (log)
                        sw.WriteLine(desktopPath);
                    file.Save(desktopPath, false);
                }
                catch (Exception ex)
                {
                    Utils.exceptionLog("CreateShortcut", "create: "+ modifiedName, ex);
                }
            }
            else
            {
                try
                {
                    if (File.Exists(modifiedName))
                        File.Delete(modifiedName);
                }
                catch (Exception ex)
                {
                    Utils.exceptionLog("CreateShortcut", "delete: " + modifiedName, ex);
                }
            }
        }
    }

    // needed for CreateShortcut
    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    internal class ShellLink
    {
    }

    // needed for CreateShortcut
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    internal interface IShellLink
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
        void Resolve(IntPtr hwnd, int fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }
}
