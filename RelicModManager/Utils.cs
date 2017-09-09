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
                if (!File.Exists(filePath))
                {
                    File.AppendAllText(filePath, "");
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
                    using (BinaryReader br = new BinaryReader(File.Open(strFile, FileMode.Open)))
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
                    switch (value)
                    {
                        case null:
                            value = "(null)";
                            break;
                        case "":
                            value = "(string with lenght 0)";
                            break;
                        default:
                            break;
                    }
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
                try { msg += string.Format(@"{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}", msgHeader, info, type, exception, stackTrace, message, source, targetSite, innerException, data); } catch { };
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
            if (Program.databaseupdate)
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
            string tempFiletime = Convert.ToString(File.GetLastWriteTime(inputFile).ToFileTime(), 10);
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
            if (!File.Exists(inputFile))
                return "-1";
            MD5 md5Hash = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            var stream = File.OpenRead(inputFile);
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
            if (!File.Exists(MainWindow.md5HashDatabaseXmlFile))
            {
                XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("database")
                );
                doc.Save(MainWindow.md5HashDatabaseXmlFile);
            }
        }

        // need filename and filetime to check the database
        public static string getMd5HashDatabase(string inputFile, string inputFiletime)
        {
            try
            {
                XDocument doc = XDocument.Load(MainWindow.md5HashDatabaseXmlFile);
                try
                {
                    XElement element = doc.Descendants("file")
                       .Where(arg => arg.Attribute("filename").Value == inputFile && arg.Attribute("filetime").Value == inputFiletime)
                       .Single();
                    return element.Attribute("md5").Value;
                }
                catch (InvalidOperationException)
                {
                    // catch the Exception if no entry is found
                }
            }
            catch (Exception e)
            {
                Utils.exceptionLog("getMd5HashDatabase", e);
                File.Delete(MainWindow.md5HashDatabaseXmlFile);     // delete damaged XML database
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
                    doc.Element("database").Add(new XElement("file",
                        new XAttribute("filename", inputFile),
                        new XAttribute("filetime", inputFiletime),
                        new XAttribute("md5", inputMd5Hash)));
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
                doc.Descendants("file")
                   .Where(arg => arg.Attribute("filename").Value == tempFilename)
                    .Remove();
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
                // .Where(arg => arg.Attribute("filename").Value == inputFile && arg.Attribute("filetime").Value == inputFiletime)

                return element.Attribute("version").Value;
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

        //parses the xml mod info into the memory database (change XML reader to from XMLDocument to XDocument)
        // https://www.google.de/search?q=c%23+xdocument+get+line+number&oq=c%23+xdocument+get+line+number&aqs=chrome..69i57j69i58.11773j0j7&sourceid=chrome&ie=UTF-8
        public static void createModStructure(string databaseURL, List<Dependency> globalDependencies, List<Dependency> dependencies, List<LogicalDependnecy> logicalDependencies, List<Category> parsedCatagoryList)
        {
            try
            {
                totalModConfigComponents = 0;
                XDocument doc = null;
                try
                {
                    doc = XDocument.Load(databaseURL, LoadOptions.SetLineInfo);
                }
                catch (XmlException ex)
                {
                    Utils.appendToLog("CRITICAL: Failed to read database: " + databaseURL + "\nMessage: " + ex.Message);
                    MessageBox.Show(Translations.getTranslatedString("databaseReadFailed") + "\n\nsee Logfile for detailed info");
                    Application.Exit();
                    return;
                }
                catch (Exception ex)
                {
                    Utils.exceptionLog("createModStructure_new", "tried to access " + databaseURL, ex);
                    MessageBox.Show(Translations.getTranslatedString("databaseNotFound"));
                    Application.Exit();
                    return;
                }
                //add the global dependencies
                foreach (XElement dependencyNode in doc.XPathSelectElements("/modInfoAlpha.xml/globaldependencies/globaldependency"))
                {
                    string[] depNodeList = new string[] { "dependencyZipFile", "dependencyZipCRC", "startAddress", "endAddress", "dependencyenabled", "packageName" };
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
                            case "dependencyZipCRC":
                                d.dependencyZipCRC = globs.Value;
                                break;
                            case "startAddress":
                                d.startAddress = globs.Value;
                                break;
                            case "endAddress":
                                d.endAddress = globs.Value;
                                break;
                            case "dependencyenabled":
                                d.enabled = Utils.parseBool(globs.Value, false);
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
                        if (depNodeList.Length > 0) { Utils.appendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => globsPend {1} (line {2})", string.Join(",", depNodeList), d.dependencyZipFile, ((IXmlLineInfo)d).LineNumber)); };
                        if (d.packageName.Equals("")) { string rad = Utils.RandomString(30);  d.packageName = rad; Utils.appendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                        globalDependencies.Add(d);
                    };
                }
                //add the dependencies
                foreach (XElement dependencyNode in doc.XPathSelectElements("/modInfoAlpha.xml/dependencies/dependency"))
                {
                    string[] depNodeList = new string[] { "dependencyZipFile", "dependencyZipCRC", "startAddress", "endAddress", "dependencyenabled", "packageName" };
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
                            case "dependencyZipCRC":
                                d.dependencyZipCRC = globs.Value;
                                break;
                            case "startAddress":
                                d.startAddress = globs.Value;
                                break;
                            case "endAddress":
                                d.endAddress = globs.Value;
                                break;
                            case "dependencyenabled":
                                d.enabled = Utils.parseBool(globs.Value, false);
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
                        if (depNodeList.Length > 0) { Utils.appendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => globsPend {1} (line {2})", string.Join(",", depNodeList), d.dependencyZipFile, ((IXmlLineInfo)d).LineNumber)); };
                        if (d.packageName.Equals("")) { string rad = Utils.RandomString(30); d.packageName = rad; Utils.appendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                        dependencies.Add(d);
                    };
                }
                //add the dependencies
                foreach (XElement dependencyNode in doc.XPathSelectElements("/modInfoAlpha.xml/logicalDependencies/logicalDependency"))
                {
                    string[] depNodeList = new string[] { "dependencyZipFile", "dependencyZipCRC", "startAddress", "endAddress", "dependencyenabled", "packageName" };
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
                            case "dependencyZipCRC":
                                d.dependencyZipCRC = globs.Value;
                                break;
                            case "startAddress":
                                d.startAddress = globs.Value;
                                break;
                            case "endAddress":
                                d.endAddress = globs.Value;
                                break;
                            case "dependencyenabled":
                                d.enabled = Utils.parseBool(globs.Value, false);
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
                        if (depNodeList.Length > 0) { Utils.appendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => globsPend {1} (line {2})", string.Join(",", depNodeList), d.dependencyZipFile, ((IXmlLineInfo)d).LineNumber)); };
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
                                            string[] modNodeList = new string[] { "name", "version", "zipFile", "startAddress", "endAddress", "crc", "enabled", "visible", "packageName", "size", "description", "updateComment", "devURL", "userDatas", "pictures", "dependencies", "configs" };
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
                                                        m.size = Utils.parseFloat(modNode.Value, 0.0f);
                                                        break;
                                                    case "description":
                                                        m.description = modNode.Value;
                                                        break;
                                                    case "updateComment":
                                                        m.updateComment = modNode.Value;
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
                Utils.exceptionLog("createModStructure_new", ex);
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
                            string[] confNodeList = new string[] { "name", "version", "zipFile", "startAddress", "endAddress", "crc", "enabled", "visible", "packageName", "size", "updateComment", "description", "devURL", "type", "configs", "userDatas", "pictures", "dependencies" };
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
                                        c.size = Utils.parseFloat(configNode.Value, 0.0f);
                                        break;
                                    case "updateComment":
                                        c.updateComment = configNode.Value;
                                        break;
                                    case "description":
                                        c.description = configNode.Value;
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
                                    case "dependencies":
                                        //parse all dependencies
                                        foreach (XElement dependencyHolder in configNode.Elements())
                                        {
                                            string[] depNodeList = new string[] { "packageName" };
                                            Dependency d = new Dependency();
                                            d.packageName = "";
                                            foreach (XElement dependencyNode in dependencyHolder.Elements())
                                            {
                                                depNodeList = confNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
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
                Utils.exceptionLog("processConfigs_new", ex);
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
            if (!File.Exists(filePath))
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
                    if (File.Exists(filePath)) File.Delete(filePath);
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
                    if (File.Exists(filePath)) File.Delete(filePath);
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
                    if (File.Exists(filePath)) File.Delete(filePath);
                    doc.Save(filePath);
                    //remove empty elements
                    XDocument doc2 = XDocument.Load(filePath);
                    doc2.Descendants().Where(e => string.IsNullOrEmpty(e.Value)).Remove();
                    if (File.Exists(filePath)) File.Delete(filePath);
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
            if (!File.Exists(fileLocation))
                return;

            //replace all "fake escape characters" with real escape characters
            search = search.Replace(@"\n", "newline");
            search = search.Replace(@"\r", "\r");
            search = search.Replace(@"\t", "\t");

            //load file from disk...
            string file = File.ReadAllText(fileLocation);
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
            File.WriteAllText(fileLocation, file);
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
            if (!File.Exists(jsonFile))
                return;

            //load file from disk...
            string file = File.ReadAllText(jsonFile);
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
                Utils.appendToLog("ERROR: Failed to patch " + jsonFile);
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
                    Utils.appendToLog("ERROR: path " + jsonPath + " not found for " + Path.GetFileName(jsonFile));
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
                Utils.appendToLog("ERROR: Unknown json patch mode, " + mode);
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
                Utils.appendToLog("There was an error with patching the file " + jsonFile + ", with extra refrences");
            File.WriteAllText(jsonFile, rebuilder.ToString());
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
            if (!File.Exists(bootFile))
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
            if (!File.Exists(bootFile))
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
            string fileContents = File.ReadAllText(bootFile);
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
            string fileContents = File.ReadAllText(newFilePath);
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
                    Utils.appendToLog("ERROR: Path not found: " + origXvmPath);
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
                        Utils.appendToLog("ERROR: Path not found: " + origXvmPath);
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
                            Utils.appendToLog("invalid index: " + pathArray[0]);
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
                        Utils.appendToLog("Invalid mode: " + mode + " for xvm patch " + origXvmPath);
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
                Utils.appendToLog("ERROR: Path not found: " + origXvmPath);
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
                Utils.appendToLog("ERROR: Path not found: " + origXvmPath);
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
                File.Delete(newFilePath);
                File.WriteAllText(newFilePath, sb.ToString());
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
                File.Delete(newFilePath);
                File.WriteAllText(newFilePath, sb.ToString());
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
                File.Delete(newFilePath);
                File.WriteAllText(newFilePath, sb.ToString());
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
                File.Delete(newFilePath);
                File.WriteAllText(newFilePath, sb.ToString());
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
                File.Delete(newFilePath);
                File.WriteAllText(newFilePath, sb.ToString());
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
                File.Delete(newFilePath);
                File.WriteAllText(newFilePath, sb.ToString());
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
            if (!File.Exists(bootFile))
            {
                if(writeToLog)
                    appendToLog("ERROR: xvm config boot file does not exist at " + xvmBootFileLoc1 + ", checking " + xvmBootFileLoc2);
                else
                    appendToLog("NOTICE: default run, xvm config boot file does not exist at " + xvmBootFileLoc1 + ", checking " + xvmBootFileLoc2);
                bootFile = xvmBootFileLoc2;
                if (!File.Exists(bootFile))
                {
                    if(writeToLog)
                        appendToLog("ERROR: xvm config boot file does not exist at " + xvmBootFileLoc2 + ", aborting patch");
                    else
                        appendToLog("NOTICE: default run, xvm config boot file does not exist at " + xvmBootFileLoc2 + ", user did not install xvm");
                    return null;
                }
            }
            appendToLog("xvm boot file located to parse");
            string fileContents = File.ReadAllText(bootFile);
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
        public static void clearSelectionMemory(List<Category> parsedCatagoryList)
        {
            Utils.appendToLog("Unchecking all mods");
            foreach (Category c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    m.Checked = false;
                    Utils.uncheckProcessConfigs(m.configs);
                }
            }
        }
        private static void uncheckProcessConfigs(List<Config> configList)
        {
            foreach (Config cc in configList)
            {
                cc.Checked = false;
                Utils.uncheckProcessConfigs(cc.configs);
            }
        }
        //saves the currently checked configs and mods
        public static void saveConfig_old(bool fromButton, List<Category> parsedCatagoryList, List<Mod> userMods)
        {
            //dialog box to ask where to save the config to
            System.Windows.Forms.SaveFileDialog saveLocation = new System.Windows.Forms.SaveFileDialog();
            saveLocation.AddExtension = true;
            saveLocation.DefaultExt = ".xml";
            saveLocation.Filter = "*.xml|*.xml";
            saveLocation.InitialDirectory = Application.StartupPath + "\\RelHaxUserConfigs";
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
            if (Settings.saveLastConfig && !fromButton)
            {
                savePath = Application.StartupPath + "\\RelHaxUserConfigs\\lastInstalledConfig.xml";
                Utils.appendToLog("Save last config checked, saving to " + savePath);
            }
            //XmlDocument save time!
            XmlDocument doc = new XmlDocument();
            //mods root
            XmlElement modsHolderBase = doc.CreateElement("mods");
            doc.AppendChild(modsHolderBase);
            //add configfile version as an attribute (changes to this save format are very likely)
            modsHolderBase.SetAttribute("ver", "1.0");
            modsHolderBase.SetAttribute("date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //relhax mods root
            XmlElement modsHolder = doc.CreateElement("relhaxMods");
            modsHolderBase.AppendChild(modsHolder);
            //user mods root
            XmlElement userModsHolder = doc.CreateElement("userMods");
            modsHolderBase.AppendChild(userModsHolder);
            //check every mod
            foreach (Category c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    if (m.Checked)
                    {
                        //add it to the list
                        XmlElement mod = doc.CreateElement("mod");
                        modsHolder.AppendChild(mod);
                        XmlElement modName = doc.CreateElement("name");
                        modName.InnerText = m.name;
                        mod.AppendChild(modName);
                        if (m.configs.Count > 0)
                        {
                            XmlElement configsHolder = doc.CreateElement("configs");
                            Utils.saveProcessConfigs_old(doc, m.configs, configsHolder);
                            mod.AppendChild(configsHolder);
                        }
                    }
                }
            }
            //check user mods
            foreach (Mod m in userMods)
            {
                if (m.Checked)
                {
                    //add it to the list
                    XmlElement mod = doc.CreateElement("mod");
                    modsHolder.AppendChild(mod);
                    XmlElement modName = doc.CreateElement("name");
                    modName.InnerText = m.name;
                    mod.AppendChild(modName);
                    userModsHolder.AppendChild(mod);
                }
            }
            doc.Save(savePath);
            if (fromButton)
            {
                MessageBox.Show(Translations.getTranslatedString("configSaveSuccess"));
            }
        }
        private static void saveProcessConfigs_old(XmlDocument doc, List<Config> configList, XmlElement configsHolder)
        {
            foreach (Config cc in configList)
            {
                XmlElement config = null;
                if (cc.Checked)
                {
                    //add the config to the list
                    config = doc.CreateElement("config");
                    configsHolder.AppendChild(config);
                    XmlElement configName = doc.CreateElement("name");
                    configName.InnerText = cc.name;
                    config.AppendChild(configName);

                    if (cc.configs.Count > 0)
                    {
                        XmlElement configsHolderSub = doc.CreateElement("configs");
                        Utils.saveProcessConfigs_old(doc, cc.configs, configsHolderSub);
                        config.AppendChild(configsHolderSub);
                    }
                }
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
            Utils.clearSelectionMemory(parsedCatagoryList);
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
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
                                Utils.appendToLog("WARNING: mod \"" + nn.InnerText + "\" not found");
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
                                        mfcb.Parent.BackColor = System.Drawing.Color.BlanchedAlmond;
                                    }
                                    else if (m.modFormCheckBox is ModWPFCheckBox)
                                    {
                                        ModWPFCheckBox mfCB2 = (ModWPFCheckBox)m.modFormCheckBox;
                                        mfCB2.IsChecked = true;
                                    }
                                }
                                Utils.appendToLog("Checking mod " + m.name);
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
                                if (File.Exists(Application.StartupPath + "\\RelHaxUserMods\\" + filename))
                                {
                                    m.Checked = true;
                                    Utils.appendToLog("checking user mod " + m.zipFile);
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
                        File.Move(filePath, targetFilePath);
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
            Utils.appendToLog("Loading mod selections v2.0 from " + filePath);
            List<string> savedConfigList = new List<string>();
            var doc = new XPathDocument(filePath);
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
                                        mfcb.Parent.BackColor = System.Drawing.Color.BlanchedAlmond;
                                    }
                                    else if (m.modFormCheckBox is ModWPFCheckBox)
                                    {
                                        ModWPFCheckBox mfCB2 = (ModWPFCheckBox)m.modFormCheckBox;
                                        mfCB2.IsChecked = true;
                                    }
                                }
                                Utils.appendToLog("Checking mod " + m.name);
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
                    if (File.Exists(Path.Combine(Application.StartupPath, "RelHaxUserMods", filename)))
                    {
                        um.Checked = true;
                        Utils.appendToLog("Checking user mod " + um.zipFile);
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
                                Utils.appendToLog("WARNING: config \"" + nnnn.InnerText + "\" not found for mod/config \"" + holder.InnerText + "\"");
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
                                        CBTemp.Parent.BackColor = System.Drawing.Color.BlanchedAlmond;
                                    }
                                    else if (c.configUIComponent is ConfigFormRadioButton)
                                    {
                                        ConfigFormRadioButton CBTemp = (ConfigFormRadioButton)c.configUIComponent;
                                        CBTemp.Checked = true;
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
                                Utils.appendToLog("Checking mod " + c.name);
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
                            Utils.appendToLog("Checking mod " + c.name);
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
                panelRef.BackColor = System.Drawing.Color.BlanchedAlmond;
            }
        }

        public static List<string> createDownloadedOldZipsList(List<string> currentZipFiles, List<Category> parsedCatagoryList, List<Dependency> globalDependencies)
        {
            parsedZips = new List<string>();
            foreach (Dependency d in globalDependencies)
            {
                if (!d.dependencyZipFile.Equals("") && !parsedZips.Contains(d.dependencyZipFile))
                {
                    parsedZips.Add(d.dependencyZipFile);
                }
            }
            foreach (Category cat in parsedCatagoryList)
            {
                foreach (Dependency d in cat.dependencies)
                {
                    if (!d.dependencyZipFile.Equals("") && !parsedZips.Contains(d.dependencyZipFile))
                    {
                        parsedZips.Add(d.dependencyZipFile);
                    }
                }
                foreach (Mod m in cat.mods)
                {
                    foreach (Dependency d in m.dependencies)
                    {
                        if (!d.dependencyZipFile.Equals("") && !parsedZips.Contains(d.dependencyZipFile))
                        {
                            parsedZips.Add(d.dependencyZipFile);
                        }
                    }
                    if (!m.zipFile.Equals("") && !parsedZips.Contains(m.zipFile))
                    {
                        parsedZips.Add(m.zipFile);
                    }
                    if (m.configs.Count > 0)
                        parseZipFileConfigs(m.configs);
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
        public static void parseZipFileConfigs(List<Config> configList)
        {
            foreach (Config c in configList)
            {
                foreach (Dependency d in c.dependencies)
                {
                    if (!d.dependencyZipFile.Equals("") && !parsedZips.Contains(d.dependencyZipFile))
                    {
                        parsedZips.Add(d.dependencyZipFile);
                    }
                }
                if (!c.zipFile.Equals("") && !parsedZips.Contains(c.zipFile))
                {
                    parsedZips.Add(c.zipFile);
                }
                if (c.configs.Count > 0)
                    parseZipFileConfigs(c.configs);
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
                        Utils.appendToLog("Deleting empty directory " + directory);
                    Directory.Delete(directory, false);
                }
            }
        }
        //returns true if the CRC's of each file match, false otherwise
        public static bool CRCsMatch(string localFile, string remoteCRC)
        {
            if (!File.Exists(localFile))
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
        static readonly string[] SizeSuffixes =
                   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return "0.0 bytes"; }
            if (value < 1000) { return string.Format("{0:n" + decimalPlaces + "} {1}", 0.1, SizeSuffixes[1]); }

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

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
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
    }
}
