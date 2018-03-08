using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Linq;
using System.ComponentModel;
using System.Net;
using System.Globalization;
using Ionic.Zip;
using System.Runtime.InteropServices;
using System.Drawing;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Windows;
using System.Windows.Interop;
using IWshRuntimeLibrary;

namespace RelhaxModpack
{
    //a static utility class with usefull methods that all other forms can use if they need it
    public static class Utils
    {
        private static List<string> ParsedZips;
        private static int iMaxLogLength = 1500000; // Probably should be bigger, say 2,000,000
        private static int iTrimmedLogLength = -300000; // minimum of how much of the old log to leave
        private static object _locker = new object();

        private static Hashtable macroList;

        //logs string info to the log output
        // public static void AppendToLog(string info)
        // {
        // Logging.Manager(info);
        // }
        public static void Depricated_AppendToLog(string info)
        {
            lock (_locker)              // avoid that 2 or more threads calling the Log function and writing lines in a mess
            {
                //the method should automaticly make the file if it's not there
                string filePath = Path.Combine(System.Windows.Forms.Application.StartupPath, "RelHaxLog.txt");
                if (!System.IO.File.Exists(filePath))
                {
                    System.IO.File.AppendAllText(filePath, "");
                }
                //if the info text is containing any linefeed/carrieage return, intend the next line with 26 space char
                info = info.Replace("\n", "\n" + string.Concat(Enumerable.Repeat(" ", 26)));
                Depricated_WriteToFile(filePath, string.Format("{0:yyyy-MM-dd HH:mm:ss.fff}   {1}", DateTime.Now, info));
            }
        }
        public static void AppendToInstallLog(string info)
        {
            try
            {
                lock (_locker)              // avoid that 2 or more threads calling the Log function and writing lines in a mess
                {
                    //the method should automaticly make the file if it's not there
                    string filePath = Path.Combine(Settings.TanksLocation, "logs", "installedRelhaxFiles.log");
                    if (!System.IO.File.Exists(filePath))
                    {
                        System.IO.File.AppendAllText(filePath, "");
                        Depricated_WriteToFile(filePath, string.Format("Database Version: {0}", Settings.DatabaseVersion));
                        Depricated_WriteToFile(filePath, string.Format("/*  Date: {0:yyyy-MM-dd HH:mm:ss}  */", DateTime.Now));
                    }
                    Depricated_WriteToFile(filePath, info);
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("AppendToInstallLog", ex);
            }
        }
        // https://stackoverflow.com/questions/4741037/keeping-log-files-under-a-certain-size
        private static void Depricated_WriteToFile(string strFile, string strNewLogMessage, bool cutFile = true)
        {
            try
            {
                // bigger logfile size at testing and developing
                int multi = 1;
                if (Program.testMode) multi = 100;

                FileInfo fi = new FileInfo(strFile);

                Byte[] bytesSavedFromEndOfOldLog = null;

                if (cutFile && fi.Length > iMaxLogLength * multi) // if the log file length is already too long
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
                try
                {
                    // If the log file is less than the max length, just open it at the end to write there
                    if (!cutFile || fi.Length < iMaxLogLength * multi)
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
                finally
                {
                    fs.Dispose();
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
        public static void Depricated_DumpObjectToLog(string objectName, object n)
        {
            lock (_locker)              // avoid that 2 or more threads calling the Log function and writing lines in a mess
            {
                Logging.Manager(String.Format("----- dump of object {0} ------", objectName));
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(n))
                {
                    string name = descriptor.Name;
                    object value = descriptor.GetValue(n);
                    if (value == null)
                        value = "(null)";
                    else if (value is string && value.ToString().Trim().Equals(""))
                        value = "(string with lenght 0)";
                    Logging.Manager(string.Format("{0}={1}", name, value));
                }
                Logging.Manager("----- end of dump ------");
            }
        }

        public static void DumbObjectToLog(string objectName, object n)
        {
            DumbObjectToLog("", objectName, n);
        }

        public static void DumbObjectToLog(string text, string objectName, object n)
        {
            Logging.Manager(String.Format("{0}{1}----- dump of object {2}{3}------\n{4}\n----- end of dump ------", text, text.Equals("") ? "" : "\n", objectName, objectName.Equals("") ? "" : " ", JObject.FromObject(n).ToString()));
        }
        
        /// <summary>
        /// default logging function of exception informations, possible to expand the cxception Group with his own needed informations of the specific exception
        /// </summary>
        /// <param e=Exception>the exception object that would be catched</param>
        public static void ExceptionLog(Exception e)
        {
            Utils.ExceptionLog("", "", e);
        }
        /// <summary>
        /// default logging function of exception informations, possible to expand the cxception Group with his own needed informations of the specific exception
        /// </summary>
        /// <param msg=string>the name of the function or other unified informations to traceback the point of exception</param>
        /// <param e=Exception>the exception object that would be catched</param>
        public static void ExceptionLog(string msg, Exception e)
        {
            Utils.ExceptionLog(msg, "", e);
        }
        /// <summary>
        /// default logging function of exception informations, possible to expand the cxception Group with his own needed informations of the specific exception              https://msdn.microsoft.com/de-de/library/system.exception.data(v=vs.110).aspx
        /// </summary>
        /// <param msg=string>the name of the function or other unified informations to traceback the point of exception</param>
        /// <param info=string>more informations of the function that throw the exception</param>
        /// <param e=Exception>the exception object that would be catched</param>
        public static void ExceptionLog(string msgString, string infoString, Exception e)
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
                    WebException we = (WebException)e;
                    errorType = "WebException";
                    type = "";
                    if (we.Status == WebExceptionStatus.ProtocolError)
                    {
                        try { type = string.Format("Code: {0}\nDescription: {1}", ((HttpWebResponse)we.Response).StatusCode.Equals("") ? "(empty)" : ((HttpWebResponse)we.Response).StatusCode.ToString(), ((HttpWebResponse)we.Response).StatusDescription == null ? "(null)" : ((HttpWebResponse)we.Response).StatusDescription.Equals("") ? "(empty)" : ((HttpWebResponse)we.Response).StatusDescription.ToString()); } catch { };
                    }
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
                Logging.Manager(msg);
            }
        }
        //returns the md5 hash of the file based on the input file string location
        public static string CreateMd5Hash(string inputFile)
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

        public static bool ParseBool(string input, bool defaultValue)
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

        public static int ParseInt(string input, int defaultValue)
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

        public static float ParseFloat(string input, float defaultValue)
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

        public class CheckStorage
        {
            public string PackageName { get; set; }
            public string ZipFile { get; set; }
            public bool Dependency { get; set; }
            public int CheckDatabaseListIndex { get; set; }
            public CheckStorage() { }
        }

        public static void DuplicatesPackageName_dependencyCheck(List<Dependency> dependencyList, List<CheckStorage> checkStorageList, ref int duplicatesCounter)
        {
            foreach (Dependency d in dependencyList)
            {
                foreach (CheckStorage s in checkStorageList)
                {
                    // if both s.CheckDatabaseListIndex AND m.CheckDatabaseListIndex are equal, it is checking his own entry, so SKIP EVERY check/test
                    // if the s.dependency is FALSE, it is a single mod/config and should only exists once, if not => error/duplicate message
                    // if the s.dependency is TRUE, it is a dependecy entry and PackageName AND ZipFile must be checken if equal, if not => error/duplicate message
                    if (s.CheckDatabaseListIndex != d.CheckDatabaseListIndex && ((s.PackageName.Equals(d.PackageName) && !(s.Dependency)) || (s.Dependency && s.PackageName.Equals(d.PackageName) && !s.ZipFile.Equals(d.ZipFile))))
                    {
                        Logging.Manager(string.Format("Error: duplicate PackageName \"{0}\" found. ZipFile: \"{1}\"", s.PackageName, s.ZipFile));
                        duplicatesCounter++;
                    }
                }
            }
        }

        public static void DuplicatesPackageName_RecursiveSubConfigCheck(List<SelectablePackage> subConfigList, List<CheckStorage> checkStorageList, ref int duplicatesCounter)
        {
            foreach (SelectablePackage c in subConfigList)
            {
                foreach (CheckStorage s in checkStorageList)
                {
                    // if both s.CheckDatabaseListIndex AND m.CheckDatabaseListIndex are equal, it is checking his own entry, so SKIP EVERY check/test
                    // if the s.dependency is FALSE, it is a single mod/config and should only exists once, if not => error/duplicate message
                    // if the s.dependency is TRUE, it is a dependecy entry and PackageName AND ZipFile must be checken if equal, if not => error/duplicate message
                    if (s.CheckDatabaseListIndex != c.CheckDatabaseListIndex && ((s.PackageName.Equals(c.PackageName) && !(s.Dependency)) || (s.Dependency && s.PackageName.Equals(c.PackageName) && !s.ZipFile.Equals(c.ZipFile))))
                    {
                        Logging.Manager(string.Format("Error: duplicate PackageName \"{0}\" found. ZipFile: \"{1}\"", s.PackageName, s.ZipFile));
                        duplicatesCounter++;
                    }
                }
                if (c.Packages.Count > 0)
                {
                    DuplicatesPackageName_RecursiveSubConfigCheck(c.Packages, checkStorageList, ref duplicatesCounter);
                }
            }
        }

        public static void DuplicatesPackageName_dependencyRead(ref List<Dependency> dependencyList, ref List<CheckStorage> checkStorageList)
        {
            foreach (Dependency d in dependencyList)
            {
                CheckStorage cs = new CheckStorage();
                cs.PackageName = d.PackageName;
                cs.ZipFile = d.ZipFile;
                cs.Dependency = true;
                cs.CheckDatabaseListIndex = checkStorageList.Count;
                d.CheckDatabaseListIndex = cs.CheckDatabaseListIndex;
                checkStorageList.Add(cs);
            }
        }

        public static void DuplicatesPackageName_RecursiveSubConfigRead(ref List<SelectablePackage> subConfigList, ref List<CheckStorage> checkStorageList)
        {
            foreach (SelectablePackage c in subConfigList)
            {
                CheckStorage cs = new CheckStorage();
                cs.PackageName = c.PackageName;
                cs.ZipFile = c.ZipFile;
                cs.Dependency = false;
                cs.CheckDatabaseListIndex = checkStorageList.Count;
                c.CheckDatabaseListIndex = cs.CheckDatabaseListIndex;
                checkStorageList.Add(cs);
                if (c.Packages.Count > 0)
                {
                    DuplicatesPackageName_RecursiveSubConfigRead(ref c.Packages, ref checkStorageList);
                }
                if (c.Dependencies.Count > 0)
                {
                    //duplicatesPackageName_dependencyRead(ref c.dependencies, ref checkStorageList);
                }
            }
        }
        //checks for duplicate PackageName
        public static bool DuplicatesPackageName(List<Category> parsedCatagoryList, ref int duplicatesCounter)
        {
            //add every mod and config name to a new list
            var checkStorageList = new List<CheckStorage>();
            foreach (Category c in parsedCatagoryList)
            {
                if (c.Dependencies.Count > 0)
                {
                    //duplicatesPackageName_dependencyRead(ref c.dependencies, ref checkStorageList);
                }
                foreach (SelectablePackage m in c.Packages)
                {
                    CheckStorage cs = new CheckStorage();
                    cs.PackageName = m.PackageName;
                    cs.ZipFile = m.ZipFile;
                    cs.Dependency = false;
                    cs.CheckDatabaseListIndex = checkStorageList.Count;
                    m.CheckDatabaseListIndex = cs.CheckDatabaseListIndex;
                    checkStorageList.Add(cs);
                    if (m.Packages.Count > 0)
                    {
                        DuplicatesPackageName_RecursiveSubConfigRead(ref m.Packages, ref checkStorageList);
                    }
                    if (m.Dependencies.Count > 0)
                    {
                        //duplicatesPackageName_dependencyRead(ref m.dependencies, ref checkStorageList);
                    }
                }
            }
            //itterate through every mod name again
            foreach (Category c in parsedCatagoryList)
            {
                foreach (SelectablePackage m in c.Packages)
                {
                    foreach (var s in checkStorageList)
                    {
                        // if both s.CheckDatabaseListIndex AND m.CheckDatabaseListIndex are equal, it is checking his own entry, so SKIP EVERY check/test
                        // if the s.dependency is FALSE, it is a single mod/config and should only exists once, if not => error/duplicate message
                        // if the s.dependency is TRUE, it is a dependecy entry and PackageName AND ZipFile must be checken if equal, if not => error/duplicate message
                        if (s.CheckDatabaseListIndex != m.CheckDatabaseListIndex && ((s.PackageName.Equals(m.PackageName) && !(s.Dependency)) || (s.Dependency && s.PackageName.Equals(m.PackageName) && !(s.ZipFile.Equals(m.ZipFile)))))
                        {
                            Logging.Manager(string.Format("Error: duplicate PackageName \"{0}\" found. ZipFile: \"{1}\".", s.PackageName, s.ZipFile));
                            duplicatesCounter++;
                        }
                    }
                    if (m.Packages.Count > 0)
                    {
                        DuplicatesPackageName_RecursiveSubConfigCheck(m.Packages, checkStorageList, ref duplicatesCounter);
                    }
                    if (m.Dependencies.Count > 0)
                    {
                        //duplicatesPackageName_dependencyCheck(m.dependencies, checkStorageList, ref duplicatesCounter);
                    }
                }
                if (c.Dependencies.Count > 0)
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
        public static bool Duplicates(List<Category> parsedCatagoryList)
        {
            //add every mod name to a new list
            List<string> modNameList = new List<string>();
            foreach (Category c in parsedCatagoryList)
            {
                foreach (SelectablePackage m in c.Packages)
                {
                    modNameList.Add(m.Name);
                }
            }
            //itterate through every mod name again
            foreach (Category c in parsedCatagoryList)
            {
                foreach (SelectablePackage m in c.Packages)
                {
                    //in theory, there should only be one matching mod name
                    //between the two lists. more indicates a duplicates
                    int i = 0;
                    foreach (string s in modNameList)
                    {
                        if (s.Equals(m.Name))
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
        public static SelectablePackage LinkMod(string modName, string catagoryName, List<Category> parsedCatagoryList)
        {
            foreach (Category c in parsedCatagoryList)
            {
                foreach (SelectablePackage m in c.Packages)
                {
                    if (c.Name.Equals(catagoryName) && m.Name.Equals(modName))
                    {
                        //found it
                        return m;
                    }
                }
            }
            return null;
        }
        //returns the mod based and mod name
        public static SelectablePackage LinkMod(string modName, List<Category> parsedCatagoryList)
        {
            foreach (Category c in parsedCatagoryList)
            {
                foreach (SelectablePackage m in c.Packages)
                {
                    if (m.Name.Equals(modName))
                    {
                        //found it
                        return m;
                    }
                }
            }
            return null;
        }
        //returns the catagory based on the catagory name
        public static Category GetCatagory(string catName, List<Category> parsedCatagoryList)
        {
            foreach (Category c in parsedCatagoryList)
            {
                if (c.Name.Equals(catName)) return c;
            }
            return null;
        }
        //gets the user mod based on it's name
        public static SelectablePackage GetUserMod(string modName, List<SelectablePackage> userMods)
        {
            foreach (SelectablePackage m in userMods)
            {
                if (m.Name.Equals(modName))
                {
                    return m;
                }
            }
            return null;
        }
        //sorts a list of mods alphabetaicaly
        public static void SortModsList(List<SelectablePackage> modList)
        {
            //sortModsList
            modList.Sort(SelectablePackage.CompareMods);
        }
        //sorte a list of catagoris alphabetaicaly
        public static void SortCatagoryList(List<Category> catagoryList)
        {
            catagoryList.Sort(Category.CompareCatagories);
        }
        //sorts a list of pictures by mod or config, then name
        public static List<Media> SortPictureList(List<Media> pictureList)
        {
            //don't actually sort them anymore
            //they will not apprea in the order of which they were loaded from the xml file
            return pictureList;
        }
        //unchecks all mods from memory
        public static void ClearSelectionMemory(List<Category> parsedCatagoryList, List<SelectablePackage> UserMods)
        {
            Logging.Manager("Unchecking all mods");
            foreach (Category c in parsedCatagoryList)
            {
                if (c.CategoryHeader.Checked)
                    c.CategoryHeader.Checked = false;
                foreach (SelectablePackage m in c.Packages)
                {
                    if(m.Checked)
                        m.Checked = false;
                    //no need to clobber over UI controls, that is now done for us
                    UncheckProcessConfigs(m.Packages);
                }
            }
            if (UserMods != null)
            {
                foreach (SelectablePackage um in UserMods)
                {
                    if(um.Checked)
                        um.Checked = false;
#warning user mod
                }
            }
        }

        private static void UncheckProcessConfigs(List<SelectablePackage> configList)
        {
            foreach (SelectablePackage c in configList)
            {
                if(c.Checked)
                    c.Checked = false;
                Utils.UncheckProcessConfigs(c.Packages);
            }
        }

        public static List<string> CreateUsedFilesList(List<Category> parsedCatagoryList,
            List<Dependency> globalDependencies, List<Dependency> dependencies, List<LogicalDependency> logicalDependencies)
        {
            List<string> currentZipFiles = new List<string>();
            foreach (Dependency d in globalDependencies)
            {
                if (!d.ZipFile.Equals("") && !currentZipFiles.Contains(d.ZipFile))
                {
                    currentZipFiles.Add(d.ZipFile);
                }
            }
            foreach (Dependency d in dependencies)
            {
                if (!d.ZipFile.Equals("") && !currentZipFiles.Contains(d.ZipFile))
                {
                    currentZipFiles.Add(d.ZipFile);
                }
            }
            foreach (LogicalDependency d in logicalDependencies)
            {
                if (!d.ZipFile.Equals("") && !currentZipFiles.Contains(d.ZipFile))
                {
                    currentZipFiles.Add(d.ZipFile);
                }
            }
            foreach (Category cat in parsedCatagoryList)
            {
                foreach (SelectablePackage m in cat.Packages)
                {

                    if (!m.ZipFile.Equals("") && !currentZipFiles.Contains(m.ZipFile))
                    {
                        currentZipFiles.Add(m.ZipFile);
                    }
                    if (m.Packages.Count > 0)
                        CreateUsedFilesListParseConfigs(m.Packages, currentZipFiles, out currentZipFiles);
                }
            }
            return currentZipFiles;
        }

        public static void CreateUsedFilesListParseConfigs(List<SelectablePackage> configList, List<string> currentZipFiles, out List<string> currentZipFilesOut)
        {
            foreach (SelectablePackage c in configList)
            {
                if (!c.ZipFile.Equals("") && !currentZipFiles.Contains(c.ZipFile))
                {
                    currentZipFiles.Add(c.ZipFile);
                }
                if (c.Packages.Count > 0)
                    CreateUsedFilesListParseConfigs(c.Packages, currentZipFiles, out currentZipFiles);
            }
            currentZipFilesOut = currentZipFiles;
        }
        //moved to ModSelectionList.cs
        public static List<string> Depricated_createDownloadedOldZipsList(List<string> currentZipFiles, List<Category> parsedCatagoryList,
            List<Dependency> globalDependencies, List<Dependency> currentDependencies, List<LogicalDependency> currentLogicalDependencies)
        {
            ParsedZips = new List<string>();
            foreach (Dependency d in globalDependencies)
            {
                if (!d.ZipFile.Equals("") && !ParsedZips.Contains(d.ZipFile))
                {
                    ParsedZips.Add(d.ZipFile);
                }
            }
            foreach (Dependency d in currentDependencies)
            {
                if (!d.ZipFile.Equals("") && !ParsedZips.Contains(d.ZipFile))
                {
                    ParsedZips.Add(d.ZipFile);
                }
            }
            foreach (LogicalDependency d in currentLogicalDependencies)
            {
                if (!d.ZipFile.Equals("") && !ParsedZips.Contains(d.ZipFile))
                {
                    ParsedZips.Add(d.ZipFile);
                }
            }
            foreach (Category cat in parsedCatagoryList)
            {
                foreach (SelectablePackage m in cat.Packages)
                {

                    if (!m.ZipFile.Equals("") && !ParsedZips.Contains(m.ZipFile))
                    {
                        ParsedZips.Add(m.ZipFile);
                    }
                    if (m.Packages.Count > 0)
                        Depricated_ParseZipFileConfigs(m.Packages);
                }
            }
            //now parsedZips has every single possible ZipFile in the database
            //for each zipfile in it, remove it in currentZipFiles if it exists
            foreach (string s in ParsedZips)
            {
                if (currentZipFiles.Contains(s))
                    currentZipFiles.Remove(s);
            }
            return currentZipFiles;
        }

        public static void Depricated_ParseZipFileConfigs(List<SelectablePackage> configList)
        {
            foreach (SelectablePackage c in configList)
            {

                if (!c.ZipFile.Equals("") && !ParsedZips.Contains(c.ZipFile))
                {
                    ParsedZips.Add(c.ZipFile);
                }
                if (c.Packages.Count > 0)
                    Depricated_ParseZipFileConfigs(c.Packages);
            }
        }
        //deletes all empty directories from a given start location
        public static void ProcessDirectory(string startLocation, bool reportToLog = true)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                ProcessDirectory(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    if (reportToLog)
                        Logging.Manager(string.Format("Deleting empty directory {0}", directory));
                    Directory.Delete(directory, false);
                }
            }
        }
        //returns true if the CRC's of each file match, false otherwise
        public static bool CRCsMatch(string localFile, string remoteCRC)
        {
            if (!System.IO.File.Exists(localFile))
                return false;
            string crc = XMLUtils.GetMd5Hash(localFile);
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
                string[] pages = { "http://forum.worldoftanks.eu/index.php?/topic/623269-", "http://forum.worldoftanks.com/index.php?/topic/535868-", "http://forum.worldoftanks.eu/index.php?/topic/624499-" };
                foreach (string r in pages)
                {
                    try
                    {
                        client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                        client.DownloadString(r);
                    }
                    catch (Exception e)
                    {
                        Utils.ExceptionLog("Forum access", e);
                    }
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

        public static string GetValidFilename(String fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
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
        //https://stackoverflow.com/questions/5977445/how-to-get-windows-display-settings
        public static float GetDisplayScale(Graphics graphics)
        {
            //get the DPI setting
            float dpiX, dpiY;
            dpiX = graphics.DpiX;
            dpiY = graphics.DpiY;
            if (dpiX != dpiY)
            {
                Logging.Manager("WARNING: scale values do not equal, using x value");
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

        public static float GetScalingFactor()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();
            int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);

            float ScreenScalingFactor = (float)PhysicalScreenHeight / (float)LogicalScreenHeight;

            return ScreenScalingFactor; // 1.25 = 125%
        }
        // https://social.msdn.microsoft.com/Forums/vstudio/en-US/92a36534-0f01-4425-ab63-c5f8830d64ae/help-please-with-dotnetzip-extracting-data-form-ziped-file?forum=csharpgeneral
        public static string GetStringFromZip(string zipFilename, string archivedFilename, string password = null)
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
                Logging.Manager(string.Format("ERROR: {0} not found", zipFilename));
            }
            return textStr;
        }

        public static bool ConvertDateToLocalCultureFormat(string date, out string dateOut)
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
                    Utils.ExceptionLog("DirectoryDelete", "Filename=" + file, ex);
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
                        Utils.ExceptionLog("DirectoryDelete", "Folder=" + dir, ex);
                    }
                }
            }

            try
            {
                if (deleteTopfolder) Directory.Delete(folderPath);
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("DirectoryDelete", "Folder=" + folderPath, ex);
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

        public static long GetCurrentUniversalFiletimeTimestamp()
        {
            return DateTime.Now.ToUniversalTime().ToFileTime();
        }
        
        public static string ReplaceMacro(string text, string macro, string macrotext)
        {
            bool search = true;
            while (search)
            {
                int index = text.ToLower().IndexOf("{"+ macro.ToLower() + "}");
                if (index == -1)
                {
                    search = false;
                }
                else
                {
                    text = text.Replace(text.Substring(index, macro.Length + 2), macrotext);
                }
            }
            return text;
        }

        //builds the hashtable once rather than each time we want to use it
        public static void BuildMacroHash()
        {
            macroList = new Hashtable
            {
                { "app", Settings.TanksLocation },
                { "onlineFolder", Settings.TanksOnlineFolderVersion },
                { "versiondir", Settings.TanksVersion },
                { "appData", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) },
                { "relhax", Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) },
                { "temp", Settings.RelhaxTempFolder },
                { "desktop", Environment.GetFolderPath(Environment.SpecialFolder.Desktop) }
            };
        }
        public static string ReplaceMacro(string text)
        {
            try
            {
                foreach (DictionaryEntry macro in macroList)
                {
                    text = System.Text.RegularExpressions.Regex.Replace(text, @"{" + @macro.Key.ToString() + @"}", @macro.Value.ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
            }
            catch (Exception ex)
            {
                ExceptionLog("ReplaceMacro", string.Format("Result string: {0}", text), ex);
                DumbObjectToLog("macroList", macroList);
            }
            return text;
        }
        public static string RemoveLeadingSlash(string s)
        {
            return s.TrimStart('/').TrimStart('\\');
        }

        public static string ReplaceDirectorySeparatorChar(string s)
        {
            return s.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        public static string AddTrailingBackslashChar(string s)
        {
            return !s.Last().ToString().Equals(@"\") || !s.Last().ToString().Equals(@"/") ? s + @"\" : s;
        }

        public static string ConvertFiletimeTimestampToDate(long timestamp)
        {
            return DateTime.FromFileTime(timestamp).ToString();
        }

        /// <summary>Checks if a point is inside the possible monitor space</summary>
        /// <param name="p">The point to check</param>
        public static bool PointWithinScreen(System.Drawing.Point p)
        {
            //if eithor x or y are negative it's an invalid location
            if (p.X < 0 || p.Y < 0)
                return false;
            int totalWidth = 0, totalHeight = 0;
            foreach(Screen s in Screen.AllScreens)
            {
                totalWidth += s.Bounds.Width;
                totalHeight += s.Bounds.Height;
            }
            if (totalWidth > p.X && totalHeight > p.Y)
                return true;
            return false;
        }

        /// <summary>Checks if a point is inside the possible monitor space</summary>
        /// <param name="x">The x cordinate of the point</param>
        /// <param name="y">The y cordinate of the point</param>
        public static bool PointWithinScreen(int x, int y)
        {
            return PointWithinScreen(new System.Drawing.Point(x, y));
        }

        // https://stackoverflow.com/questions/4897655/create-shortcut-on-desktop-c-sharp
        /// <summary>Creates or removes a shortcut at the specified pathname.</summary> 
        /// <param name="shortcutTarget">The path where the original file is located.</param> 
        /// <param name="shortcutName">The filename of the shortcut to be created or removed from desktop including the (.lnk) extension.</param>
        /// <param name="create">True to create a shortcut or False to remove the shortcut.</param> 
        public static void CreateShortcut(string shortcutTarget, string shortcutName, bool create, bool log)
        {
            string modifiedName = Path.GetFileNameWithoutExtension(shortcutName) + ".lnk";
            string desktopPath = Utils.ReplaceDirectorySeparatorChar(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), modifiedName));
            if (create)
            {
                try
                {
                    if(System.IO.File.Exists(desktopPath))
                    {
                        //file exists, check if needs update
                        WshShell shell = new WshShell(); //Create a new WshShell Interface
                        IWshShortcut link = (IWshShortcut)shell.CreateShortcut(desktopPath); //Link the interface to our shortcut
                        //System.Windows.Forms.MessageBox.Show(link.TargetPath);
                        if(!shortcutTarget.Equals(link.TargetPath))
                        {
                            //needs update
                            link.TargetPath = shortcutTarget;
                            link.Save();
                        }
                        else
                        {
                            //no update needed
                        }
                    }
                    else
                    {
                        //file does not exist, needs to be created
                        IShellLink link = (IShellLink)new ShellLink();
                        // setup shortcut information
                        link.SetDescription("created by the Relhax Manager");
                        link.SetPath(@shortcutTarget);
                        link.SetIconLocation(@shortcutTarget, 0);
                        link.SetWorkingDirectory(Path.GetDirectoryName(@shortcutTarget));
                        link.SetArguments(""); //The arguments used when executing the exe
                                               // save it
                        System.Runtime.InteropServices.ComTypes.IPersistFile file = (System.Runtime.InteropServices.ComTypes.IPersistFile)link;
                        file.Save(desktopPath, false);
                    }
                    if (log)
                    {
                        // Utils.AppendToInstallLog(desktopPath);
                        // write created file with path
                        Logging.Installer(desktopPath);
                    }
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("CreateShortcut", "create: " + modifiedName, ex);
                }
            }
            else
            {
                try
                {
                    if (System.IO.File.Exists(modifiedName))
                        System.IO.File.Delete(modifiedName);
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("CreateShortcut", "delete: " + modifiedName, ex);
                }
            }
        }


        public static string Truncate(TextBox text)
        {
            return Truncate(text.Text, text.Font, text.Width);
        }

        // https://stackoverflow.com/questions/17654231/how-to-truncate-a-string-to-fit-in-a-container
        public static string Truncate(string text, Font font, int maxWidth, int direction = 0)
        {
            // Determine direction of truncation:
            // 0 = right
            // 1 = left
            // 2 = 1/2 of the Box width the first part of the string and 2/3 of the "left" "free" side, fill with [...] and the last part of the string
            if (text.Length == 0) return text;
            int i = text.Length;
            int l = 0;
            bool firstPass = false;
            string preText = "";
            string trailingText = "";
            string midText = text;
            if (direction == 0)
                trailingText = "...";
            else if (direction == 1)
                preText = "...";
            else if (direction == 2)
            {
                firstPass = true;   // only set to true, if direction typ 2 is selected
                l = text.Length / 3;
            }
            else
            {
                Logging.Manager("ERROR. Wrong Trim method called. String: " + text + " with direction: " + direction.ToString());
                return text;
            }
            //if it won't fit, account for the "..." that will be added
            if(TextRenderer.MeasureText(preText + midText + trailingText, font).Width > maxWidth)
                maxWidth = maxWidth - TextRenderer.MeasureText("...", font).Width;
            while (TextRenderer.MeasureText(preText + midText + trailingText, font).Width > maxWidth)
            {
                if (firstPass)
                {
                    preText = text.Substring(0, l) + "...";
                    midText = text.Substring(l);
                    i = midText.Length;
                    firstPass = false;
                }
                //if truncating in the middle, use the middle subtext, not the entire text
                if(direction == 2)
                {
                    midText = midText.Substring(0, --i);
                }
                else
                {
                    midText = text.Substring(0, --i);
                }
                if (i == 0) break;
            }
            return preText + midText + trailingText;
        }
        //You can implement more methods such as receiving a string with font,... and returning the truncated/trimmed version.
        
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
