using Ionic.Zip;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace RelhaxModpack
{
    /// <summary>
    /// A utility class for static functions used in various places in the modpack
    /// </summary>
    public static class Utils
    {
        #region Statics
        public static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public const long BYTES_TO_MBYTES = 1048576;
        //MACROS TODO
        #endregion

        #region Application Utils
        /// <summary>
        /// Return the entire assembely version
        /// </summary>
        /// <returns>The entire assembely version string (major, minor, build, revision)</returns>
        public static string GetApplicationVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        /// <summary>
        /// Return the date and time in EN-US form, the time that the application was built
        /// </summary>
        /// <returns>the application build date and time in EN-US form</returns>
        public static string GetCompileTime()
        {
            return CiInfo.BuildTag + " (EN-US date format)";
        }
        #endregion

        #region Window Utils
        /// <summary>
        /// Get a list of all visual components in the window
        /// </summary>
        /// <param name="window">The window to get the list of</param>
        /// <param name="includeWindow">if the list should include the window itself</param>
        /// <returns>A list of type FrameowrkElement of all components</returns>
        public static List<FrameworkElement> GetAllWindowComponentsVisual(Window window, bool includeWindow)
        {
            //https://stackoverflow.com/questions/874380/wpf-how-do-i-loop-through-the-all-controls-in-a-window
            List<FrameworkElement> windowComponents = new List<FrameworkElement>();
            if (includeWindow)
                windowComponents.Add(window);
            if (VisualTreeHelper.GetChildrenCount(window) > 0)
                GetAllWindowComponentsVisual(window, windowComponents);
            return windowComponents;
        }
        public static List<FrameworkElement> GetAllWindowComponentsLogical(Window window, bool includeWindow)
        {
            List<FrameworkElement> windowComponents = new List<FrameworkElement>();
            if (includeWindow)
                windowComponents.Add(window);
            GetAllWindowComponentsLogical(window, windowComponents);
            return windowComponents;
        }
        //A recursive method for navigating the visual tree
        private static void GetAllWindowComponentsVisual(FrameworkElement v, List<FrameworkElement> allWindowComponents)
        {
            int ChildrenComponents = VisualTreeHelper.GetChildrenCount(v);
            for (int i = 0; i < ChildrenComponents; i++)
            {
                DependencyObject dep = VisualTreeHelper.GetChild(v, i);
                if(!(dep is FrameworkElement))
                {
                    continue;
                }
                FrameworkElement subV = (FrameworkElement)VisualTreeHelper.GetChild(v, i);
                allWindowComponents.Add(subV);
                if (subV is TabControl tabControl)
                {
                    foreach(FrameworkElement tabVisual in tabControl.Items)
                    {
                        allWindowComponents.Add(tabVisual);
                        GetAllWindowComponentsLogical(tabVisual, allWindowComponents);
                    }
                }
                int childrenCount = VisualTreeHelper.GetChildrenCount(subV);
                if (childrenCount > 0)
                    GetAllWindowComponentsVisual(subV, allWindowComponents);
            }
        }
        //Gets any logical components that are not currently shown (like elemnts behind a tab)
        private static void GetAllWindowComponentsLogical(FrameworkElement v, List<FrameworkElement> allWindowComponents)
        {
            //NOTE: v has been added
            //have to use var here cause i got NO CLUE what type it is #niceMeme
            var children = LogicalTreeHelper.GetChildren(v);
            //Type temp = children.GetType();
            foreach (var child in children)
            {
                //Type temp2 = child.GetType();
                if (child is FrameworkElement childVisual)
                {
                    allWindowComponents.Add(childVisual);
                    GetAllWindowComponentsLogical(childVisual, allWindowComponents);
                }
            }
        }
        #endregion

        #region File Utilities
        /// <summary>
        /// Creates an MD5 hash calculation of the input file
        /// </summary>
        /// <param name="inputFile">The path to the file to calculate</param>
        /// <returns></returns>
        public static string CreateMD5Hash(string inputFile)
        {
            if (string.IsNullOrWhiteSpace(inputFile))
                return "-1";
            //first, return if the file does not exist
            if (!System.IO.File.Exists(inputFile))
                return "-1";
            //Convert the input string to a byte array and compute the hash
            StringBuilder sBuilder;
            using (MD5 md5Hash = MD5.Create())
            using (var stream = System.IO.File.OpenRead(inputFile))
            {
                byte[] data = md5Hash.ComputeHash(stream);
                stream.Close();
                //Create a new Stringbuilder to collect the bytes
                sBuilder = new StringBuilder();
                //Loop through each byte of the hashed data 
                //and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
            }
            //Return the hexadecimal string.
            return sBuilder.ToString();
        }
        /// <summary>
        /// Gets a zip file entry in the form of a string
        /// </summary>
        /// <param name="zipFilename">The path to the file in the zip</param>
        /// <param name="archivedFilename">the path to the zip file</param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string GetStringFromZip(string zipFilename, string archivedFilename, string password = "")
        {
            // https://social.msdn.microsoft.com/Forums/vstudio/en-US/92a36534-0f01-4425-ab63-c5f8830d64ae/help-please-with-dotnetzip-extracting-data-form-ziped-file?forum=csharpgeneral
            if(!File.Exists(zipFilename))
            {
                Logging.WriteToLog(string.Format("ERROR: {0} not found", zipFilename));
                return null;
            }
            string textStr = "";
            using (ZipFile zip = ZipFile.Read(zipFilename))
            using (MemoryStream ms = new MemoryStream() { Position=0 })
            using (StreamReader sr = new StreamReader(ms))
            {
                ZipEntry e = zip[archivedFilename];
                if (!string.IsNullOrWhiteSpace(password))
                    e.ExtractWithPassword(ms, password);
                else
                    e.Extract(ms);
                ms.Position = 0;
                textStr = sr.ReadToEnd();
            }
            return textStr;
        }
        /// <summary>
        /// deletes all empty directories from a given path
        /// </summary>
        /// <param name="startLocation">the path to start in</param>
        public static void ProcessDirectory(string startLocation)
        {
            if (!Directory.Exists(startLocation))
                return;
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                ProcessDirectory(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Logging.WriteToLog(string.Format("Deleting empty directory {0}", directory),Logfiles.Application, LogLevel.Debug);
                    Directory.Delete(directory, false);
                }
            }
        }
        //TODO
        public static string SizeSuffix(long value, int decimalPlaces = 1, bool sizeSuffix = false)
        {
            // https://stackoverflow.com/questions/14488796/does-net-provide-an-easy-way-convert-bytes-to-kb-mb-gb-etc
            if (value < 0) { return "-" + SizeSuffix(-value); }
            return SizeSuffix((ulong)value, decimalPlaces, sizeSuffix);
        }
        //TODO
        private static string SizeSuffix(ulong value, int decimalPlaces = 1, bool sizeSuffix = false)
        {
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
        /// <summary>
        /// Checks if a filename has invalid characters and replaces them with underscores
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetValidFilename(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }
        /// <summary>
        /// Deletes files in a directory
        /// </summary>
        /// <param name="folderPath">The path to delete files from</param>
        /// <param name="deleteSubfolders">set to true to delete files recursivly inside each subdirectory</param>
        /// <param name="numRetrys">The number of times the method should retry to delete a file</param>
        /// <param name="timeout">The ammount of time in milliseconds to wait before trying again to delete files</param>
        public static void DirectoryDelete(string folderPath, bool deleteSubfolders, int numRetrys, int timeout)
        {
            //check to make sure the number of retries is between 1 and 10
            if (numRetrys < 1)
            {
                Logging.WriteToLog(string.Format("numRetrys is invalid (below 1), setting to 1 (numRetryes={0})", numRetrys),
                    Logfiles.Application, LogLevel.Warning);
                numRetrys = 1;
            }
            if (numRetrys > 10)
            {
                Logging.WriteToLog(string.Format("numRetrys is invalid (above 10), setting to 10 (numRetryes={0})", numRetrys),
                    Logfiles.Application, LogLevel.Warning);
                numRetrys = 10;
            }
            int retryCounter = 0;
            foreach (string file in Directory.GetFiles(folderPath))
            {
                while(retryCounter < numRetrys)
                {
                    try
                    {
                        File.Delete(file);
                        retryCounter = numRetrys;
                    }
                    catch(Exception ex)
                    {
                        Logging.WriteToLog(string.Format("failed to delete {0}, retryCount={1}, message:\n{2}", file, retryCounter, ex.Message),
                            Logfiles.Application,LogLevel.Error);
                        retryCounter++;
                        System.Threading.Thread.Sleep(timeout);
                    }
                }
            }
            //if deleting the sub directories
            if (deleteSubfolders)
            {
                foreach (string dir in Directory.GetDirectories(folderPath))
                {
                    DirectoryDelete(dir, deleteSubfolders, numRetrys,timeout);
                }
            }
        }
        #endregion

        #region Data type from string processing/parsing
        /// <summary>
        /// Try to parse a boolean value based on string input
        /// </summary>
        /// <param name="input">the string to try to parse</param>
        /// <param name="defaultValue">the default value to use if parsing fails</param>
        /// <returns>The bool value of the ipnut string, or the default value if parsing failes</returns>
        public static bool ParseBool(string input, bool defaultValue)
        {
            if (bool.TryParse(input, out bool result))
                return result;
            else return defaultValue;
        }
        /// <summary>
        /// Try to parse an intiger value based on string input
        /// </summary>
        /// <param name="input">the string to try to parse</param>
        /// <param name="defaultValue">the default value to use if parsing fails</param>
        /// <returns>The int value of the ipnut string, or the default value if parsing failes</returns>
        public static int ParseInt(string input, int defaultValue)
        {
            if (int.TryParse(input, out int result))
                return result;
            else return defaultValue;
        }
        /// <summary>
        /// Try to parse a float value based on string input
        /// </summary>
        /// <param name="input">the string to try to parse</param>
        /// <param name="defaultValue">the default value to use if parsing fails</param>
        /// <returns>The float value of the ipnut string, or the default value if parsing failes</returns>
        public static float ParseFloat(string input, float defaultValue)
        {
            if (float.TryParse(input,NumberStyles.Float,CultureInfo.InvariantCulture,out float result))
                return result;
            else return defaultValue;
        }
        #endregion

        #region Duplicates checking

        #endregion

        #region Mods list sorting

        #endregion

        #region Generic utils
        /// <summary>
        /// https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings-in-c
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
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
        /// <summary>
        /// Gets the current time in the form of universal time
        /// </summary>
        /// <returns>the universal time of now</returns>
        public static long GetCurrentUniversalFiletimeTimestamp()
        {
            return DateTime.Now.ToUniversalTime().ToFileTime();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static string ConvertFiletimeTimestampToDate(long timestamp)
        {
            return DateTime.FromFileTime(timestamp).ToString();
        }
        //MACROS TODO
        /// <summary>
        /// Encode a plain text string into base64 UTF8 encoding
        /// </summary>
        /// <param name="plainText">The plain text string</param>
        /// <returns>The UTF8 base64 encoded version</returns>
        public static string Base64Encode(string plainText)
        {
            //https://stackoverflow.com/questions/11743160/how-do-i-encode-and-decode-a-base64-string
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
        /// <summary>
        /// Decode a base64 UTF8 encoded string into plain text
        /// </summary>
        /// <param name="base64EncodedData">The base64 stirng</param>
        /// <returns>The plain text version</returns>
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static bool IsProcessRunning(string processName, string pathToMatch = "")
        {
            //check if path of exe is the same as the one we're looking at
            //first check to make sure wot path is legit
            bool checkWithPath = true;
            if(string.IsNullOrEmpty(pathToMatch))
            {
                Logging.WriteToLog(nameof(pathToMatch) + "Is empty, cannot check for direct path, only checking for num processes",
                    Logfiles.Application, LogLevel.Error);
                checkWithPath = false;
            }
            //get list of running instances of WoT
            //TO GET PROCESS NAME: Process.GetCurrentProcess().ProcessName
            Process[] processes = Process.GetProcessesByName(processName);
            //check if three are any at all
            if(processes.Length == 0)
            {
                return false;
            }
            //first check if the number is 1 or less, if so stop here
            else if (processes.Length == 1)
                return false;
            //if not checking for path, we don't know if is the direct path, 
            else if (!checkWithPath)
                return true;
            else
            {
                foreach (Process p in processes)
                {
                    if (pathToMatch.Equals(Path.GetDirectoryName(p.StartInfo.FileName)))
                    {
                        Logging.WriteToLog(string.Format("Matched process name {0} to path {1}", p.ProcessName, pathToMatch),
                            Logfiles.Application, LogLevel.Debug);
                    }
                }
            }
            return false;
        }
        public static void BuildMacroList()
        {
            //TODO
        }

        #endregion

        #region Selections parsing
        public static void ParseDeveloperSelections()
        {
            //run php script to create xml string (async)
        }
        private static void OnDeveloperSelectionParsed()//TO PUT IN MODSELECTINLSIT
        {
            //get nodecollection of developerSelections
              //display name
              //list of mods
            //make UI nodes radiobuttons in stackpanel
              //text = displayname
              //tag = list<string> packagesToSelect
        }
        private static void OnDeveloperSelectionSelect()//TO PUT IN MODSELECTIONLIST
        {
            //parseSelection (radioButton name, radioButton tag)
        }
        public static void ParseUserSelection(string filePath)
        {
            //load xml string
            //get version ID
            string versionID = "";
            switch(versionID)
            {
                case "2.0":
                  //parse via 2.0 method
                  break;
                default:
                  //unknown or not supported
                  break;
            }
        }
        public static void ParseUserSelectionV2(XmlDocument doc)
        {
            //make list of stirng for packages to select
        }
        public static void ParseSelection()
        {
            //will take category view and packagelistToSelect<string>
            //for each category load packages recursivly
        }
        public static void VerifySelection()
        {
            //verify selections and remove and rouge selections and report them
        }
        #endregion

        #region Tanks Install Auto/Manuel Search Code
        //checks the registry to get the location of where WoT is installed
        public static bool AutoFindWoTDirectory(ref string WoTRoot)
        {
            List<string> searchPathWoT = new List<string>();
            string[] registryPathArray = new string[] { };

            // here we need the value for the searchlist
            // check replay link
            registryPathArray = new string[] { @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\.wotreplay\shell\open\command", @"HKEY_CURRENT_USER\Software\Classes\.wotreplay\shell\open\command" };
            foreach (string regEntry in registryPathArray)
            {
                // get values from from registry
                object obj = Registry.GetValue(regEntry, "", -1);
                // if it is not "null", it is containing possible a string
                if (obj != null)
                {
                    try
                    {
                        // add the thing to the checklist, but remove the Quotation Marks in front of the string and the trailing -> " "%1"
                        searchPathWoT.Add(((string)obj).Substring(1).Substring(0, ((string)obj).Length - 7));
                    }
                    catch
                    { } // only exception catching
                }
            }

            // here we need the value for the searchlist
            string regPath = @"HKEY_CURRENT_USER\Software\Wargaming.net\Launcher\Apps\wot";
            RegistryKey subKeyHandle = Registry.CurrentUser.OpenSubKey(regPath.Replace(@"HKEY_CURRENT_USER\", ""));
            if (subKeyHandle != null)
            {
                // get the value names at the reg Key one by one
                foreach (string valueName in subKeyHandle.GetValueNames())
                {
                    // read the value from the regPath
                    object obj = Registry.GetValue(regPath, valueName, -1);
                    if (obj != null)
                    {
                        try
                        {
                            // we did get only a path to used WoT folders, so add the game name to the path and add it to the checklist
                            searchPathWoT.Add(Path.Combine((string)obj, "WorldOfTanks.exe"));
                        }
                        catch
                        { } // only exception catching
                    }
                }
            }

            // here we need the value name for the searchlist
            registryPathArray = new string[] { @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache", @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Compatibility Assistant\Store" };
            foreach (string p in registryPathArray)
            {
                // set the handle to the registry key
                subKeyHandle = Registry.CurrentUser.OpenSubKey(p);
                if (subKeyHandle == null) continue;            // subKeyHandle == null not existsting
                // parse all value names of the registry key abouve
                foreach (string valueName in subKeyHandle.GetValueNames())
                {
                    try
                    {
                        // if the lower string "worldoftanks.exe" is contained => match !!
                        if (valueName.ToLower().Contains("Worldoftanks.exe".ToLower()))
                        {
                            // remove (replace it with "") the attachment ".ApplicationCompany" or ".FriendlyAppName" in the string and add the string to the searchlist
                            searchPathWoT.Add(valueName.Replace(".ApplicationCompany", "").Replace(".FriendlyAppName", ""));
                        }
                    }
                    catch
                    { } // only exception catching
                }
            }

            // this searchlist is long, maybe 30-40 entries (system depended), but the best possibility to find a currently installed WoT game.
            foreach (string path in searchPathWoT)
            {
                if (File.Exists(path))
                {
                    Logging.WriteToLog(string.Format("valid game path found: {0}", path));
                    // write the path to the central value holder
                    WoTRoot = path;
                    // return the path
                    return true;
                }
            }
            //return false if nothing found
            return false;
        }
        #endregion
    }
}
