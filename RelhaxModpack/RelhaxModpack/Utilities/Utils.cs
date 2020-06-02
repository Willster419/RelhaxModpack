using Ionic.Zip;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using IWshRuntimeLibrary;
using File = System.IO.File;
using System.Windows.Threading;
using RelhaxModpack.Atlases;
using System.Drawing;
using Size = System.Drawing.Size;
using RelhaxModpack.Windows;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Web;
using RelhaxModpack.Database;
using System.Runtime.CompilerServices;
using RelhaxModpack.UI;
using RelhaxModpack.Xml;
using RelhaxModpack.Utilities;

namespace RelhaxModpack
{

    /// <summary>
    /// A utility class for static functions used in various places in the modpack
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Multiply by this value to convert milliseconds to seconds
        /// </summary>
        public const int TO_SECONDS = 1000;

        /// <summary>
        /// Multiply by this value to convert seconds to minuets
        /// </summary>
        public const int TO_MINUETS = 60;

        /// <summary>
        /// Get a complete assembly name based on a matching keyword
        /// </summary>
        /// <param name="keyword">The keyword to match</param>
        /// <returns>The first matching assembly name, or null if no matches</returns>
        public static string GetAssemblyName(string keyword)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(rn => rn.Contains(keyword));
        }

        #region Application Utils
        /// <summary>
        /// Return the entire assembly version
        /// </summary>
        /// <returns>The entire assembly version string (major, minor, build, revision)</returns>
        public static string GetApplicationVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// Return the date and time in EN-US form, the time that the application was built
        /// </summary>
        /// <returns>the application build date and time in EN-US form</returns>
        public static string GetCompileTime()
        {
            return CiInfo.BuildTag + " (EN-US date format)";
        }

        /// <summary>
        /// Get the XmlDocument object of the managerInfo zip file
        /// </summary>
        /// <param name="overwrite">If the managerInfo zip file should be force refreshed</param>
        /// <returns>An xmlDocument object of manager_version.xml</returns>
        public static async Task<XmlDocument> GetManagerInfoDocumentAsync(bool overwrite)
        {

            Settings.ManagerInfoZipfile = await GetManagerInfoZipfileAsync(overwrite);
            if(Settings.ManagerInfoZipfile == null)
            {
                Logging.Exception("Settings.ModInfoZipfile is null");
                return null;
            }

            //get the version info string
            string xmlString = FileUtils.GetStringFromZip(Settings.ManagerInfoZipfile, "manager_version.xml");
            if (string.IsNullOrEmpty(xmlString))
            {
                Logging.Exception("Failed to get xml string from Settings.ModInfoZipfile");
                Application.Current.Shutdown();
                return null;
            }

            return XmlUtils.LoadXmlDocument(xmlString, XmlLoadType.FromString);
        }

        /// <summary>
        /// Download and store the latest managerInfo zip file
        /// </summary>
        /// <param name="overwrite">Set to true to force a download of the latest version</param>
        /// <returns>The mangerInfo zip file in the Ionic.Zipfile object</returns>
        public static async Task<ZipFile> GetManagerInfoZipfileAsync(bool overwrite)
        {
            //first delete the old file if it exists, just to check
#pragma warning disable CS0618 // Type or member is obsolete
            if (File.Exists(Settings.ManagerInfoDatFile))
                File.Delete(Settings.ManagerInfoDatFile);
#pragma warning restore CS0618 // Type or member is obsolete

            //if the zipfile is not null and no overwrite, then stop
            if (Settings.ManagerInfoZipfile != null && !overwrite)
            {
                return Settings.ManagerInfoZipfile;
            }
            //if zipfile is not null and we are overwriting, then dispose of the zip first
            else if (Settings.ManagerInfoZipfile != null && overwrite)
            {
                Settings.ManagerInfoZipfile.Dispose();
                Settings.ManagerInfoZipfile = null;
            }

            using (WebClient client = new WebClient())
            {
                try
                {
                    byte[] zipfile = await client.DownloadDataTaskAsync(Settings.ManagerInfoURLBigmods);
                    return ZipFile.Read(new MemoryStream(zipfile));
                }
                catch(Exception ex)
                {
                    Logging.Exception("Failed to download managerInfo to memory stream");
                    Logging.Exception(ex.ToString());
                    return null;
                }
            }
        }

        /// <summary>
        /// Compares if the current application version is the same as the version checked from online
        /// </summary>
        /// <param name="currentVersion">The string representation of the latest modpack application version</param>
        /// <returns>True if the manager string versions are the same, false otherwise</returns>
        /// <remarks>IsManagerUptoDate will return false if it fails to get the latest managerInfo zip file</remarks>
        public static async Task<bool> IsManagerUptoDate(string currentVersion)
        {
            //actually compare the build of the application of the requested distribution channel
            XmlDocument doc = await GetManagerInfoDocumentAsync(false);
            if (doc == null)
            {
                Logging.Error("failed to get manager online version");
                return false;
            }

            string applicationOnlineVersion = (ModpackSettings.ApplicationDistroVersion == ApplicationVersions.Stable) ?
                XmlUtils.GetXmlStringFromXPath(doc, "//version/relhax_v2_stable").Trim() ://stable
                XmlUtils.GetXmlStringFromXPath(doc, "//version/relhax_v2_beta").Trim();//beta

            Logging.Info("Current build is {0} online build is {1}", currentVersion, applicationOnlineVersion);

            //check if versions are equal
            //return currentVersion.Equals(applicationOnlineVersion);
            //currentVersion = strA, applicationOnline=strB
            //if currentVersion >  applicationOnline, probably testing, ok
            //if currentVersion == applicationOnline, same version, ok
            //if currentVersion <  applicationOnline, update available, not ok
            //when strA < strB, it returns -1
            bool outOfDate = (CompareVersions(currentVersion, applicationOnlineVersion) == -1);
            return !outOfDate;
        }
        #endregion

        #region Data type from string processing/parsing
        /// <summary>
        /// Try to parse a boolean value based on string input
        /// </summary>
        /// <param name="input">The string to try to parse</param>
        /// <param name="defaultValue">The default value to use if parsing fails</param>
        /// <returns>The bool value of the input string, or the default value if parsing fails</returns>
        public static bool ParseBool(string input, bool defaultValue)
        {
            if (bool.TryParse(input, out bool result))
                return result;
            else return defaultValue;
        }

        /// <summary>
        /// Try to parse a boolean value based on string input
        /// </summary>
        /// <param name="input">The string to try to parse</param>
        /// <param name="result">The result value</param>
        /// <param name="defaultValue">The default value for result, if parse fails</param>
        /// <returns>Returns if the TryParse() worked</returns>
        public static bool ParseBool(string input, out bool result, bool defaultValue = false)
        {
            if (bool.TryParse(input, out result))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Try to parse an integer value based on string input
        /// </summary>
        /// <param name="input">The string to try to parse</param>
        /// <param name="defaultValue">The default value to use if parsing fails</param>
        /// <returns>The int value of the input string, or the default value if parsing fails</returns>
        public static int ParseInt(string input, int defaultValue)
        {
            if (int.TryParse(input, out int result))
                return result;
            else return defaultValue;
        }

        /// <summary>
        /// Try to parse an integer value based on string input
        /// </summary>
        /// <param name="input">The string to try to parse</param>
        /// <param name="result">The result value</param>
        /// <param name="defaultValue">The default value for result, if parse fails</param>
        /// <returns>Returns if the TryParse() worked</returns>
        public static bool ParseInt(string input, out int result, int defaultValue = 0)
        {
            if (int.TryParse(input, out result))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Try to parse a float value based on string input
        /// </summary>
        /// <param name="input">The string to try to parse</param>
        /// <param name="defaultValue">The default value to use if parsing fails</param>
        /// <returns>The float value of the input string, or the default value if parsing fails</returns>
        public static float ParseFloat(string input, float defaultValue)
        {
            if (float.TryParse(input,NumberStyles.Float,CultureInfo.InvariantCulture,out float result))
                return result;
            else return defaultValue;
        }

        /// <summary>
        /// Try to parse a float value based on string input
        /// </summary>
        /// <param name="input">The string to try to parse</param>
        /// <param name="result">The result value</param>
        /// <param name="defaultValue">The default value for result, if parse fails</param>
        /// <returns>Returns if the TryParse() worked</returns>
        public static bool ParseFloat(string input, out float result, float defaultValue = 0)
        {
            if (float.TryParse(input, NumberStyles.Float,CultureInfo.InvariantCulture,out result))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Try to parse a long value based on string input
        /// </summary>
        /// <param name="input">The string to try to parse</param>
        /// <param name="defaultValue">The default value to use if parsing fails</param>
        /// <returns>The float value of the input string, or the default value if parsing fails</returns>
        public static long ParseLong(string input, long defaultValue)
        {
            if (long.TryParse(input, out long result))
                return result;
            else return defaultValue;
        }

        /// <summary>
        /// Try to parse a long value based on string input
        /// </summary>
        /// <param name="input">The string to try to parse</param>
        /// <param name="result">The result value</param>
        /// <param name="defaultValue">The default value for result, if parse fails</param>
        /// <returns>Returns if the TryParse() worked</returns>
        public static bool ParseLong(string input, out long result, long defaultValue = 0)
        {
            if (long.TryParse(input, out result))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Try to parse an unsigned long value based on string input
        /// </summary>
        /// <param name="input">The string to try to parse</param>
        /// <param name="defaultValue">The default value to use if parsing fails</param>
        /// <returns>The float value of the input string, or the default value if parsing fails</returns>
        public static ulong ParseuLong(string input, ulong defaultValue)
        {
            if (ulong.TryParse(input, out ulong result))
                return result;
            else return defaultValue;
        }

        /// <summary>
        /// Try to parse an unsigned long value based on string input
        /// </summary>
        /// <param name="input">The string to try to parse</param>
        /// <param name="result">The result value</param>
        /// <param name="defaultValue">The default value for result, if parse fails</param>
        /// <returns>Returns if the TryParse() worked</returns>
        public static bool ParseuLong(string input, out ulong result, ulong defaultValue = 0)
        {
            if (ulong.TryParse(input, out result))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Tries to parse an enumeration of a given type
        /// </summary>
        /// <typeparam name="TEnum">The type of enumeration to parse as</typeparam>
        /// <param name="input">The input string to parse</param>
        /// <param name="defaultValue">The default value if the enumeration parse fails</param>
        /// <returns>The parsed or default enumeration value</returns>
        /// <remarks>see https://stackoverflow.com/questions/10685794/how-to-use-generic-tryparse-with-enum </remarks>
        public static TEnum ParseEnum<TEnum>(string input, TEnum defaultValue)
            where TEnum : struct, IConvertible
        {
            if (Enum.TryParse(input, true, out TEnum result))
                return result;
            else return defaultValue;
        }
        #endregion

        #region Generic Utils
        /// <summary>
        /// Get all xml strings for the V2 database file format from the selected beta database github branch
        /// </summary>
        /// <returns>all xml files in string form of the V2 database</returns>
        public static string GetBetaDatabase1V1ForStringCompare(bool loadMode)
        {
            List<string> downloadURLs = XmlUtils.GetBetaDatabase1V1FilesList();

            string[] downloadStrings = null;

            if (loadMode)
            {
                Task t = Task.Run(() => { downloadStrings = Utils.DownloadStringsFromUrls(downloadURLs); } );

                while(!t.IsCompleted)
                {
                    Thread.Sleep(100);
                }
            }
            else
            {
                downloadStrings = Utils.DownloadStringsFromUrls(downloadURLs);
            }

            return string.Join(string.Empty, downloadStrings);
        }

        /// <summary>
        /// Get all xml strings for the V2 database file format from the selected beta database github branch
        /// </summary>
        /// <returns>all xml files in string form of the V2 database</returns>
        public async static Task<string> GetBetaDatabase1V1ForStringCompareAsync()
        {
            return await Task<string>.Run(() => GetBetaDatabase1V1ForStringCompare(false));
        }

        /// <summary>
        /// Downloads an array of strings from a list of download URLs all at the same time
        /// </summary>
        /// <param name="downloadURLs">the list of string URLs to download</param>
        /// <returns>An array of downloaded strings, or empty for each string that failed to download</returns>
        public static string[] DownloadStringsFromUrls(List<string> downloadURLs)
        {
            string[] downloadData = new string[downloadURLs.Count];

            //create arrays
            WebClient[] downloadClients = new WebClient[downloadURLs.Count];
            Task<string>[] downloadTasks = new Task<string>[downloadURLs.Count];

            //setup downloads
            Logging.Debug("[DownloadStringsFromUrls]: Starting async download tasks");
            for (int i = 0; i < downloadURLs.Count; i++)
            {
                downloadClients[i] = new WebClient();
                downloadClients[i].Headers.Add("user-agent", "Mozilla / 4.0(compatible; MSIE 6.0; Windows NT 5.2;)");
                downloadTasks[i] = downloadClients[i].DownloadStringTaskAsync(downloadURLs[i]);
            }

            //wait
            Task.WaitAll(downloadTasks);

            //parse into strings
            Logging.Debug("[DownloadStringsFromUrls]: Tasks finished, extracting task results");
            for (int i = 0; i < downloadURLs.Count; i++)
            {
                downloadData[i] = downloadTasks[i].Result;
            }

            return downloadData;
        }

        /// <summary>
        /// Downloads an array of strings from a list of download URLs all at the same time
        /// </summary>
        /// <param name="downloadURLs">the list of string URLs to download</param>
        /// <returns>An array of downloaded strings, or empty for each string that failed to download</returns>
        public async static Task<string[]> DownloadStringsFromUrlsAsync(List<string> downloadURLs)
        {
            return await Task<string[]>.Run(() => DownloadStringsFromUrls(downloadURLs));
        }

        /// <summary>
        /// Converts a Bitmap object to a BitmapImage object
        /// </summary>
        /// <param name="bitmap">The Bitmap object to convert</param>
        /// <returns>The BitmapImage object</returns>
        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            //https://stackoverflow.com/questions/22499407/how-to-display-a-bitmap-in-a-wpf-image
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }

        /// <summary>
        /// Creates a string of random characters
        /// </summary>
        /// <param name="length">The number of characters to create the random string</param>
        /// <param name="chars">The list of characters to use for making the random string</param>
        /// <returns>The random string</returns>
        /// <remarks>See https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings-in-c </remarks>
        public static string RandomString(int length, string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Generates a Unique IDentifier for a package using the constant defined number of string and character selections
        /// </summary>
        /// <returns>a Unique IDentifier for a package</returns>
        public static string GenerateUID()
        {
            return RandomString(Settings.NumberUIDCharacters, Settings.UIDCharacters);
        }

        /// <summary>
        /// Generates a Unique IDentifier for a package using the constant defined number of string and character selections
        /// while verifying that it's unique against a given list
        /// </summary>
        /// <param name="allPackages">A list of packages to test to make sure the UID is unique</param>
        /// <returns>A guaranteed unique ID that does not exist in the list</returns>
        public static string GenerateUID(List<DatabasePackage> allPackages)
        {
            string UID = GenerateUID();
            while (allPackages.Find(package => package.UID.Equals(UID)) != null)
            {
                UID = GenerateUID();
            }
            return UID;
        }

        /// <summary>
        /// Compare versions of form "1,2,3,4" or "1.2.3.4". Throws FormatException
        /// in case of invalid version. See function comments for more informations and samples.
        /// </summary>
        /// <param name="strA">the first version</param>
        /// <param name="strB">the second version</param>
        /// <returns>less than zero if strA is less than strB, equal to zero if
        /// strA equals strB, and greater than zero if strA is greater than strB</returns>
        /// <remarks>
        /// See https://stackoverflow.com/questions/30494/compare-version-identifiers
        /// Samples:
        /// strA        | strB
        /// 1.0.0.0     | 1.0.0.1 = -1
        /// 1.0.0.1     | 1.0.0.0 =  1
        /// 1.0.0.0     | 1.0.0.0 =  0
        /// 1, 0.0.0    | 1.0.0.0 =  0
        /// 9, 5, 1, 44 | 3.4.5.6 =  1
        /// 1, 5, 1, 44 | 3.4.5.6 = -1
        /// 6,5,4,3     | 6.5.4.3 =  0
        /// </remarks>
        public static int CompareVersions(string strA, string strB)
        {
            try
            {
                Version vA = new Version(strA.Replace(",", "."));
                Version vB = new Version(strB.Replace(",", "."));

                return vA.CompareTo(vB);
            }
            catch(Exception ex)
            {
                Logging.Exception("failed to parse versions in CompareVersions, vA=strA={0}, vB=strB={1}", strA, strB);
                Logging.Exception(ex.ToString());
                //assume out of date
                return -1;
            }
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
        /// Converts a timestamp value to a string representation
        /// </summary>
        /// <param name="timestamp">The timestamp to convert</param>
        /// <returns>The string representation of the timestamp</returns>
        public static string ConvertFiletimeTimestampToDate(long timestamp)
        {
            if (timestamp > 0)
                return DateTime.FromFileTime(timestamp).ToString();
            else
                return "(none)";
        }

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

        /// <summary>
        /// Checks if a process is running on the system
        /// </summary>
        /// <param name="processName">The name of the process</param>
        /// <param name="pathToMatch">(Optional) The directory that the process is running from</param>
        /// <returns>The Process object that matches, or null if no matches</returns>
        public static Process GetProcess(string processName, string pathToMatch = "")
        {
            //check if path of exe is the same as the one we're looking at
            //first check to make sure wot path is legit
            if (string.IsNullOrEmpty(pathToMatch))
            {
                Logging.Info("[GetProcess()]: PathToMatch is empty, only checking for instance count > 0");
            }

            //get list of running instances of WoT
            Process[] processes = Process.GetProcessesByName(processName);

            //check if three are any at all
            if (processes.Length == 0)
            {
                return null;
            }

            //if not checking for path, we don't know which instance, only that there is one
            //so return
            Logging.Debug("[GetProcess()]: Process name to match: '{0}'. Matching entries: {1}", processName, processes.Length.ToString());
            if (string.IsNullOrEmpty(pathToMatch) || processes.Length == 1)
            {
                Logging.Debug("[GetProcess()]: Processes.length = {0} and/or pathToMatch is empty, returning first entry", processes.Length.ToString());
                return processes[0];
            }

            //else try to match the path
            foreach (Process p in processes)
            {
                string processStartFilepath = string.Empty;

                //get path of process start file
                //https://stackoverflow.com/questions/5497064/how-to-get-the-full-path-of-running-process
                Logging.Debug("[GetProcess()]: Is this 64bit process? {0}", Environment.Is64BitProcess);
                if(Environment.Is64BitProcess)
                {
                    try
                    {
                        ProcessModule module = p.MainModule;
                        processStartFilepath = Path.GetDirectoryName(module.FileName);
                    }
                    catch(Win32Exception ex)
                    {
                        Logging.Error("[GetProcess()]: Failed to get process main module filename, but reported to be 64bit process!");
                        Logging.Error(ex.ToString());
                    }
                }

                if(string.IsNullOrEmpty(processStartFilepath))
                {
                    Logging.Debug("[GetProcess()]: ProcessStartFilepath is still empty, attempt alternate method");
                    //http://stackoverflow.com/questions/3399819/access-denied-while-getting-process-path/3654195#3654195
                    try
                    {
                        string processStartFilename = GetExecutablePathAboveVista(p.Id);
                        Logging.Debug("[GetProcess()]: GetExecutablePathAboveVista() returned '{0}'", processStartFilename);
                        processStartFilepath = Path.GetDirectoryName(processStartFilename);
                    }
                    catch (Win32Exception ex)
                    {
                        Logging.Error("[GetProcess()]: Failed to get process path using alternate method, return first listing");
                        Logging.Error(ex.ToString());
                        return processes[0];
                    }
                }

                string processStartFilepathCorrected = FileUtils.RemoveWoT32bit64bitPathIfExists(processStartFilepath);
                Logging.Debug("[GetProcess()]: Checking if path process {0} matching with path {1}", processStartFilepathCorrected, pathToMatch);
                if (pathToMatch.Equals(processStartFilepathCorrected))
                {
                    Logging.Debug("[GetProcess()]: Process name matched");
                    return p;
                }
                Logging.Debug("[GetProcess()]: Never matched path processes (count={0}) matching with path {1}", processes.Length.ToString(), pathToMatch);
            }
            return null;
        }

        [DllImport("kernel32.dll")]
        private static extern bool QueryFullProcessImageName(IntPtr hprocess, int dwFlags, StringBuilder lpExeName, out int size);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hHandle);

        /// <summary>
        /// An enumeration of desired access rights to ask for information when opening a process's info
        /// </summary>
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            //http://www.pinvoke.net/default.aspx/kernel32.openprocess
            /// <summary>
            /// All process info
            /// </summary>
            All = 0x001F0FFF,
            /// <summary>
            /// Terminate the process
            /// </summary>
            Terminate = 0x00000001,
            /// <summary>
            /// Create a thread from the process
            /// </summary>
            CreateThread = 0x00000002,
            /// <summary>
            /// View the process's virtual memory operations
            /// </summary>
            VirtualMemoryOperation = 0x00000008,
            /// <summary>
            /// View the process's virtual memory reads
            /// </summary>
            VirtualMemoryRead = 0x00000010,
            /// <summary>
            /// View the process's virtual memoty writes
            /// </summary>
            VirtualMemoryWrite = 0x00000020,
            /// <summary>
            /// Ability to create a duplicate process handle
            /// </summary>
            DuplicateHandle = 0x00000040,
            /// <summary>
            /// Ability for process to create processes
            /// </summary>
            CreateProcess = 0x000000080,
            /// <summary>
            /// Set quotas on the process
            /// </summary>
            SetQuota = 0x00000100,
            /// <summary>
            /// Set information about the process
            /// </summary>
            SetInformation = 0x00000200,
            /// <summary>
            /// Query information about the process
            /// </summary>
            QueryInformation = 0x00000400,
            /// <summary>
            /// Query information about the process that does not require administrator rights
            /// </summary>
            QueryLimitedInformation = 0x00001000,
            /// <summary>
            /// Synchronize rights
            /// </summary>
            Synchronize = 0x00100000
        }

        /// <summary>
        /// Gets the path to the application, including exe filename, based on the process ID
        /// </summary>
        /// <param name="ProcessId">The process ID from Process object</param>
        /// <returns>The path to the process's exe</returns>
        /// <remarks>This can throw a Win32Exception if the method fails.
        /// It uses kernel32.dll p/invoke methods to perform the operation.
        /// Does not work below windows vista.</remarks>
        public static string GetExecutablePathAboveVista(int ProcessId)
        {
            var buffer = new StringBuilder(1024);
            IntPtr hprocess = OpenProcess(ProcessAccessFlags.QueryLimitedInformation,
                                          false, ProcessId);
            if (hprocess != IntPtr.Zero)
            {
                try
                {
                    int size = buffer.Capacity;
                    if (QueryFullProcessImageName(hprocess, 0, buffer, out size))
                    {
                        return buffer.ToString();
                    }
                }
                finally
                {
                    CloseHandle(hprocess);
                }
            }
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// Wrapper for IsProcessRunning() to return boolean type
        /// </summary>
        /// <param name="processName">The name of the process</param>
        /// <param name="pathToMatch">(Optional) The directory that the process is running from</param>
        /// <returns>True if match, false otherwise</returns>
        public static bool IsProcessRunning(string processName, string pathToMatch = "")
        {
            return GetProcess(processName, pathToMatch) == null ? false : true;
        }

        /// <summary>
        /// Start a process
        /// </summary>
        /// <param name="startInfo">The ProcessStartInfo parameters object</param>
        /// <returns>True if process start was successful, false otherwise</returns>
        public static bool StartProcess(ProcessStartInfo startInfo)
        {
            try
            {
                Process.Start(startInfo);
                return true;
            }
            catch (Exception ex)
            {
                Logging.Exception("Failed to start process");
                Logging.Exception(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Start a process
        /// </summary>
        /// <param name="command">The entire command as a string style commandline</param>
        /// <returns>True if process start was successful, false otherwise</returns>
        public static bool StartProcess(string command)
        {
            try
            {
                Process.Start(command);
                return true;
            }
            catch (Exception ex)
            {
                Logging.Exception("Failed to start process");
                Logging.Exception(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Opens the selected text in Google translate web page
        /// </summary>
        /// <param name="message">The text to translate</param>
        /// <returns></returns>
        public static bool OpenInGoogleTranslate(string message)
        {
            //sample:
            //https://translate.google.com/#view=home&op=translate&sl=en&tl=de&text=test

            //replace percent
            message = message.Replace(@"%", @"percent");
            message = message.Replace(@"&", @"and");

            //google translate has a limit of 5000 characters
            if (message.Length > 4999)
            {
                message = message.Substring(0, 4999);
            }
            string textToSend = HttpUtility.UrlPathEncode(message);

            //replace comma: %2C
            textToSend = textToSend.Replace(@",", @"%2C");

            //remove colon escapes and slash escapes
            string completeTemplate = string.Format("https://translate.google.com/#view=home&op=translate&sl=en&tl={0}&text={1}",
                Translations.GetTranslatedString("GoogleTranslateLanguageKey"), textToSend);
            return StartProcess(completeTemplate);
        }

        /// <summary>
        /// Tests if an input string is null or empty
        /// </summary>
        /// <param name="stringToTest">The string to test</param>
        /// <param name="emptyNullReturn">The emptyNullReturn value if the string is null or empty, stringToTest otherwise</param>
        /// <returns></returns>
        public static string EmptyNullStringCheck(string stringToTest, string emptyNullReturn = "(null)")
        {
            return string.IsNullOrEmpty(stringToTest) ? emptyNullReturn : stringToTest;
        }

        /// <summary>
        /// Gets the name of the method above this
        /// </summary>
        /// <returns>The name of the calling method on this method</returns>
        /// <remarks>This is mostly used for in logging, to log the name of the method
        /// See https://stackoverflow.com/a/2652481/3128017 </remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetExecutingMethodName()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(2);
            return sf.GetMethod().Name;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetExecutingClassName()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(2);
            return sf.GetMethod().DeclaringType.Name;
        }
        #endregion

        #region Install Utils
        /// <summary>
        /// Copies a file from one path or in an archive to a destination
        /// </summary>
        /// <param name="package">The zip archive to extract the file from</param>
        /// <param name="sourceCompletePath">The complete path to the file. Could be a path on disk, or a path in a zip archive</param>
        /// <param name="destinationCompletePath">The complete path to copy the destination file to</param>
        public static void Unpack(string package, string sourceCompletePath, string destinationCompletePath)
        {
            string destinationFilename = Path.GetFileName(destinationCompletePath);
            string destinationDirectory = Path.GetDirectoryName(destinationCompletePath);

            //if the package entry is empty, then it's just a file copy
            if (string.IsNullOrWhiteSpace(package))
            {
                if (File.Exists(sourceCompletePath))
                    File.Copy(sourceCompletePath, destinationCompletePath);
                Logging.Info("file copied");
            }
            else
            {
                if (!File.Exists(package))
                {
                    Logging.Error("packagefile does not exist, skipping");
                    return;
                }
                using (ZipFile zip = new ZipFile(package))
                {
                    //get the files that match the specified path from the Xml entry
                    string zipPath = sourceCompletePath.Replace(@"\", @"/");
                    ZipEntry[] matchingEntries = zip.Where(zipp => zipp.FileName.Equals(zipPath)).ToArray();
                    Logging.Debug("matching zip entries: {0}", matchingEntries.Count());
                    if (matchingEntries.Count() > 0)
                    {
                        foreach (ZipEntry entry in matchingEntries)
                        {
                            //change the name to the destination
                            entry.FileName = destinationFilename;

                            //extract to disk and log
                            entry.Extract(destinationDirectory, ExtractExistingFileAction.DoNotOverwrite);
                            Logging.Info("entry extracted: {0}", destinationFilename);
                        }
                    }
                    else
                        Logging.Warning("no matching zip entries for file: {0}", zipPath);
                }
            }
        }

        /// <summary>
        /// Clear the WoT appdata cache folder
        /// </summary>
        /// <returns>True if clearing operation was sucessfull, false otherwise</returns>
        public static bool ClearCache()
        {
            //make sure that the app data folder exists
            //if it does not, then it does not need to run this
            if (!Directory.Exists(Settings.AppDataFolder))
            {
                Logging.Info("Appdata folder does not exist, creating");
                Directory.CreateDirectory(Settings.AppDataFolder);
                return true;
            }
            Logging.Info("Appdata folder exists, backing up user settings and clearing cache");

            //make the temp folder if it does not already exist
            string AppPathTempFolder = Path.Combine(Settings.RelhaxTempFolderPath, "AppDataBackup");
            //delete if possibly from previous install
            if (Directory.Exists(AppPathTempFolder))
                FileUtils.DirectoryDelete(AppPathTempFolder, true);

            //and make the folder at the end
            Directory.CreateDirectory(AppPathTempFolder);

            //backup files and folders that should be kept that aren't cache
            string[] fileNames = { "preferences.xml", "preferences_ct.xml", "modsettings.dat" };
            string[] folderNames = { "xvm", "pmod" };
            string pmodCacheFileToDelete = "cache.dat";
            string xvmFolderToDelete = "cache";

            //check if the directories are files or folders
            //if files they can move directly
            //if folders they have to be re-created on the destination and files moved manually
            Logging.WriteToLog("Starting clearing cache step 1 of 3: backing up old files", Logfiles.Application, LogLevel.Debug);
            foreach (string file in fileNames)
            {
                Logging.WriteToLog("Processing cache file/folder to move: " + file, Logfiles.Application, LogLevel.Debug);
                if (File.Exists(Path.Combine(Settings.AppDataFolder, file)))
                {
                    if (!FileUtils.FileMove(Path.Combine(Settings.AppDataFolder, file), Path.Combine(AppPathTempFolder, file)))
                    {
                        Logging.Error("Failed to move file for clear cache");
                        return false;
                    }
                }
                else
                {
                    Logging.Info("File does not exist in step clearCache: {0}", file);
                }
            }

            foreach (string folder in folderNames)
            {
                if (Directory.Exists(Path.Combine(Settings.AppDataFolder, folder)))
                {
                    FileUtils.DirectoryMove(Path.Combine(Settings.AppDataFolder, folder), Path.Combine(AppPathTempFolder, folder), true);
                }
                else
                {
                    Logging.Info("Folder does not exist in step clearCache: {0}", folder);
                }
            }

            //now delete the temp folder
            Logging.WriteToLog("Starting clearing cache step 2 of 3: actually clearing cache", Logfiles.Application, LogLevel.Debug);
            FileUtils.DirectoryDelete(Settings.AppDataFolder, true);

            //then put the above files back
            Logging.WriteToLog("Starting clearing cache step 3 of 3: restoring old files", Logfiles.Application, LogLevel.Debug);
            Directory.CreateDirectory(Settings.AppDataFolder);
            foreach (string file in fileNames)
            {
                Logging.WriteToLog("Processing cache file/folder to move: " + file, Logfiles.Application, LogLevel.Debug);
                if (File.Exists(Path.Combine(AppPathTempFolder, file)))
                {
                    if (!FileUtils.FileMove(Path.Combine(AppPathTempFolder, file), Path.Combine(Settings.AppDataFolder, file)))
                    {
                        Logging.Error("Failed to move file for clear cache");
                        return false;
                    }
                }
                else
                {
                    Logging.Info("File does not exist in step clearCache: {0}", file);
                }
            }

            //delete extra xvm cache folder and pmod cache file
            if (Directory.Exists(Path.Combine(AppPathTempFolder, folderNames[0], xvmFolderToDelete)))
                FileUtils.DirectoryDelete(Path.Combine(AppPathTempFolder, folderNames[0], xvmFolderToDelete), true);
            if (File.Exists(Path.Combine(AppPathTempFolder, folderNames[1], pmodCacheFileToDelete)))
                FileUtils.FileDelete(Path.Combine(AppPathTempFolder, folderNames[1], pmodCacheFileToDelete));

            foreach (string folder in folderNames)
            {
                if (Directory.Exists(Path.Combine(AppPathTempFolder, folder)))
                {
                    FileUtils.DirectoryMove(Path.Combine(AppPathTempFolder, folder), Path.Combine(Settings.AppDataFolder, folder), true);
                }
                else
                {
                    Logging.Info("Folder does not exist in step clearCache: {0}", folder);
                }
            }
            return true;
        }
        #endregion
    }
}
