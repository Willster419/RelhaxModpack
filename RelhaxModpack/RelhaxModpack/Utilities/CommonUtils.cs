using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using File = System.IO.File;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Web;
using RelhaxModpack.Database;
using System.Runtime.CompilerServices;
using RelhaxModpack.Xml;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RelhaxModpack.Utilities.Enums;
using System.Xml.Linq;
using System.Collections;
using RelhaxModpack.Common;
using RelhaxModpack.Settings;

namespace RelhaxModpack.Utilities
{
    /// <summary>
    /// A utility class for static functions used in various places in the modpack
    /// </summary>
    public static class CommonUtils
    {
        /// <summary>
        /// Multiply by this value to convert milliseconds to seconds
        /// </summary>
        public const int TO_SECONDS = 1000;

        /// <summary>
        /// Multiply by this value to convert seconds to minuets
        /// </summary>
        public const int TO_MINUTES = 60;

        /// <summary>
        /// Get a complete assembly name based on a matching keyword
        /// </summary>
        /// <param name="keyword">The keyword to match</param>
        /// <returns>The first matching assembly name, or null if no matches</returns>
        public static string GetAssemblyName(string keyword)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(rn => rn.Contains(keyword));
        }

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

            ((App)Application.Current).ManagerInfoZipfile = await GetManagerInfoZipfileAsync(overwrite);
            if(((App)Application.Current).ManagerInfoZipfile == null)
            {
                Logging.Exception("Settings.ModInfoZipfile is null");
                return null;
            }

            //get the version info string
            string xmlString = FileUtils.GetStringFromZip(((App)Application.Current).ManagerInfoZipfile, "manager_version.xml");
            if (string.IsNullOrEmpty(xmlString))
            {
                Logging.Exception("Failed to get xml string from Settings.ModInfoZipfile");
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
            if (File.Exists(ApplicationConstants.ManagerInfoDatFile))
                File.Delete(ApplicationConstants.ManagerInfoDatFile);
#pragma warning restore CS0618 // Type or member is obsolete

            //if the zipfile is not null and no overwrite, then stop
            if (((App)Application.Current).ManagerInfoZipfile != null && !overwrite)
            {
                return ((App)Application.Current).ManagerInfoZipfile;
            }
            //if zipfile is not null and we are overwriting, then dispose of the zip first
            else if (((App)Application.Current).ManagerInfoZipfile != null && overwrite)
            {
                ((App)Application.Current).ManagerInfoZipfile.Dispose();
                ((App)Application.Current).ManagerInfoZipfile = null;
            }

            using (PatientWebClient client = new PatientWebClient() { Timeout = 3 * TO_SECONDS })
            {
                try
                {
                    byte[] zipfile = await client.DownloadDataTaskAsync(ApplicationConstants.ManagerInfoURLBigmods);
                    ((App)Application.Current).CheckForUpdatesError = false;
                    return ZipFile.Read(new MemoryStream(zipfile));
                }
                catch(Exception ex)
                {
                    Logging.Exception("Failed to download managerInfo to memory stream");
                    Logging.Exception(ex.ToString());
                    ((App)Application.Current).CheckForUpdatesError = true;
                    return null;
                }
            }
        }

        /// <summary>
        /// Compares if the current application version is the same as the version checked from online
        /// </summary>
        /// <param name="currentVersion">The string representation of the latest modpack application version</param>
        /// <param name="applicationVersion">Control if the update check will use the beta or stable distribution channel</param>
        /// <returns>True if the manager string versions are the same, false otherwise</returns>
        /// <remarks>IsManagerUptoDate will return false if it fails to get the latest managerInfo zip file</remarks>
        public static async Task<bool> IsManagerUptoDate(string currentVersion, ApplicationVersions applicationVersion)
        {
            //actually compare the build of the application of the requested distribution channel
            XmlDocument doc = await GetManagerInfoDocumentAsync(false);
            if (doc == null)
            {
                Logging.Error("failed to get manager online version");
                return false;
            }

            string applicationOnlineVersion = (applicationVersion == ApplicationVersions.Stable) ?
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

        /// <summary>
        /// Gets the list of branches that currently exist for a github repository using it's web api.
        /// </summary>
        /// <param name="githubApiUrl">The repository api url to use to get the list of branches from.</param>
        /// <returns>The list of branches that currently exist for the given repository</returns>
        public static async Task<List<string>> GetListOfGithubRepoBranchesAsync(string githubApiUrl)
        {
            //declare objects to use
            string jsonText = string.Empty;

            //check if we're windows 7 to enable TLS options needed by github
            CheckAndEnableTLS();

            //get the list of branches
            using (PatientWebClient client = new PatientWebClient() { Timeout = 30 * TO_SECONDS })
            {
                
                try
                {
                    Logging.Debug("[GetListOfGithubRepoBranchesAsync]: downloading branch list as json from github API");
                    client.Headers.Add("user-agent", "Mozilla / 4.0(compatible; MSIE 6.0; Windows NT 5.2;)");
                    jsonText = await client.DownloadStringTaskAsync(githubApiUrl);
                }
                catch (WebException wex)
                {
                    Logging.Exception(wex.ToString());
                }
            }

            //parse from json to list
            return ParseBranchesJsonToList(jsonText);
        }

        private static List<string> ParseBranchesJsonToList(string jsonText)
        {
            JArray root = null;
            List<string> branches = new List<string>
            {
                "master"
            };

            if (!string.IsNullOrWhiteSpace(jsonText))
            {
                try
                {
                    Logging.Debug("[ParseBranchesJsonToList]: parsing json branches");
                    root = JArray.Parse(jsonText);
                }
                catch (JsonException jex)
                {
                    Logging.Exception(jex.ToString());
                }
                if (root != null)
                {
                    //parse the string into a json array object
                    foreach (JObject branch in root.Children())
                    {
                        JValue value = (JValue)branch["name"];
                        string branchName = value.Value.ToString();
                        Logging.Debug("[ParseBranchesJsonToList]: Adding branch {0}", branchName);
                        if (!branches.Contains(branchName))
                            branches.Add(branchName);
                    }
                }
            }

            return branches;
        }

        /// <summary>
        /// Check if the version of windows this is running on is windows 7 (NT 6.1), and if it is enable TLS versions 1.1 and 1.2.
        /// </summary>
        /// <remarks>This is needed for many websites that no longer support TLS 1.0 and 1.1. TLS 1.1 and 1.2 are not enabled by default when an application is running on windows 7.</remarks>
        public static void CheckAndEnableTLS()
        {
            //if windows 7, enable TLS 1.1 and 1.2
            //https://stackoverflow.com/questions/47017973/could-not-establish-secure-channel-for-ssl-tls-c-sharp-web-service-client
            //https://docs.microsoft.com/en-us/dotnet/api/system.net.servicepointmanager.securityprotocol?view=netframework-4.8#System_Net_ServicePointManager_SecurityProtocol
            //https://docs.microsoft.com/en-us/dotnet/framework/network-programming/tls
            if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1)
            {
                Logging.Debug(LogOptions.MethodName, "Windows 7 detected, enabling TLS 1.1 and 1.2");
                System.Net.ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Ssl3 |
                    SecurityProtocolType.Tls |
                    SecurityProtocolType.Tls11 |
                    SecurityProtocolType.Tls12;
            }
        }

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
            return RandomString(ApplicationConstants.NumberUIDCharacters, ApplicationConstants.UIDCharacters);
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

        /// <summary>
        /// Gets the name of the class above this
        /// </summary>
        /// <returns>The name of the calling class on this method call</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetExecutingClassName()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(2);
            return sf.GetMethod().DeclaringType.Name;
        }

        /// <summary>
        /// Clear the WoT appdata cache folder
        /// </summary>
        /// <returns>True if clearing operation was sucessfull, false otherwise</returns>
        public static bool ClearCache()
        {
            //make sure that the app data folder exists
            //if it does not, then it does not need to run this
            if (!Directory.Exists(ApplicationConstants.AppDataFolder))
            {
                Logging.Info("Appdata folder does not exist, creating");
                Directory.CreateDirectory(ApplicationConstants.AppDataFolder);
                return true;
            }
            Logging.Info("Appdata folder exists, backing up user settings and clearing cache");

            //make the temp folder if it does not already exist
            string AppPathTempFolder = Path.Combine(ApplicationConstants.RelhaxTempFolderPath, "AppDataBackup");
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
                if (File.Exists(Path.Combine(ApplicationConstants.AppDataFolder, file)))
                {
                    if (!FileUtils.FileMove(Path.Combine(ApplicationConstants.AppDataFolder, file), Path.Combine(AppPathTempFolder, file)))
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
                if (Directory.Exists(Path.Combine(ApplicationConstants.AppDataFolder, folder)))
                {
                    FileUtils.DirectoryMove(Path.Combine(ApplicationConstants.AppDataFolder, folder), Path.Combine(AppPathTempFolder, folder), true);
                }
                else
                {
                    Logging.Info("Folder does not exist in step clearCache: {0}", folder);
                }
            }

            //now delete the temp folder
            Logging.WriteToLog("Starting clearing cache step 2 of 3: actually clearing cache", Logfiles.Application, LogLevel.Debug);
            FileUtils.DirectoryDelete(ApplicationConstants.AppDataFolder, true);

            //then put the above files back
            Logging.WriteToLog("Starting clearing cache step 3 of 3: restoring old files", Logfiles.Application, LogLevel.Debug);
            Directory.CreateDirectory(ApplicationConstants.AppDataFolder);
            foreach (string file in fileNames)
            {
                Logging.WriteToLog("Processing cache file/folder to move: " + file, Logfiles.Application, LogLevel.Debug);
                if (File.Exists(Path.Combine(AppPathTempFolder, file)))
                {
                    if (!FileUtils.FileMove(Path.Combine(AppPathTempFolder, file), Path.Combine(ApplicationConstants.AppDataFolder, file)))
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
                    FileUtils.DirectoryMove(Path.Combine(AppPathTempFolder, folder), Path.Combine(ApplicationConstants.AppDataFolder, folder), true);
                }
                else
                {
                    Logging.Info("Folder does not exist in step clearCache: {0}", folder);
                }
            }
            return true;
        }

        /// <summary>
        /// Attempts to set a property value of a class or structure object instance with the string valueToSet
        /// </summary>
        /// <param name="objectToSetValueOn">The class or structure object instance to have property set</param>
        /// <param name="propertyInfoOfObject">The property information/metadata of the property to set on the object</param>
        /// <param name="valueToSet">The string version of the value to set</param>
        /// <returns>False if the value could not be set, true otherwise</returns>
        public static bool SetObjectProperty(object objectToSetValueOn, PropertyInfo propertyInfoOfObject, string valueToSet)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(propertyInfoOfObject.PropertyType);
                propertyInfoOfObject.SetValue(objectToSetValueOn, converter.ConvertFrom(valueToSet));
                return true;
            }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Attempts to set a list's value at a given index position.
        /// </summary>
        /// <param name="list">The list object to set value in.</param>
        /// <param name="index">The position in the list to try to set the value.</param>
        /// <param name="typeToSet">The type of the value to set.</param>
        /// <param name="valueToSet">The value to try to set.</param>
        /// <returns>False if the value could not be set, true otherwise</returns>
        public static bool SetListIndexValueType(IList list, int index, Type typeToSet, string valueToSet)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(typeToSet);
                object convertedObject = converter.ConvertFrom(valueToSet);
                if (index < list.Count)
                    list[index] = convertedObject;
                else
                    list.Add(convertedObject);
                return true;
            }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Attempts to set a field value of a class or structure object instance with the string valueToSet
        /// </summary>
        /// <param name="objectToSetValueOn">The class or structure object instance to have property set</param>
        /// <param name="fieldInfoOfObject">The field information/metadata of the field to set on the object</param>
        /// <param name="valueToSet">The string version of the value to set</param>
        /// <returns>False if the value could not be set, true otherwise</returns>
        public static bool SetObjectField(object objectToSetValueOn, FieldInfo fieldInfoOfObject, string valueToSet)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(fieldInfoOfObject.FieldType);
                fieldInfoOfObject.SetValue(objectToSetValueOn, converter.ConvertFrom(valueToSet));
                return true;
            }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Attempts to create an instance of a value type object and set it's value based on valueToSet for objectType
        /// </summary>
        /// <param name="objectType">The type of value object to create</param>
        /// <param name="valueToSet">The string version of the value to set</param>
        /// <param name="newObject">The new value type object created</param>
        /// <returns>False if the value could not be set, true otherwise</returns>
        public static bool SetObjectValue(Type objectType, string valueToSet, out object newObject)
        {
            try
            {
                newObject = Activator.CreateInstance(objectType);
                TypeConverter converter = TypeDescriptor.GetConverter(objectType);
                newObject = converter.ConvertFrom(valueToSet);
                return true;
            }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
                newObject = null;
                return false;
            }
        }

        /// <summary>
        /// Creates all database entries in a list property, parsing each list entry object by xmlListItems
        /// </summary>
        /// <param name="listProperty">A generic representation of a initialized, empty list.</param>
        /// <param name="componentWithIdInternalName">The ID or internal name of the component that contains that list that we are attempting to set values on, used for debugging.</param>
        /// <param name="xmlListItems">The xml element holder for the property object types, for example Medias element holder.</param>
        /// <param name="customTypeAttributeName">If using custom typing, the name of the xml attribute to use for lookup of the type of the value to set.</param>
        /// <param name="typeMapper">If using custom typing, a dictionary to map potential string results to type values of the value to create.</param>
        public static bool SetListEntries(IList listProperty, string componentWithIdInternalName, IEnumerable<XElement> xmlListItems, string customTypeAttributeName = null, Dictionary<string, Type> typeMapper = null)
        {
            bool errorOccured = false;
            bool customTyping = !(string.IsNullOrEmpty(customTypeAttributeName));

            //we now have the empty list, now get type of list it is, unless we have a dictionary to map it
            Type listObjectType = null;
            //https://stackoverflow.com/questions/34211815/how-to-get-the-underlying-type-of-an-ilist-item
            if (!customTyping) listObjectType = listProperty.GetType().GetInterfaces().Where(i => i.IsGenericType && i.GenericTypeArguments.Length == 1)
                .FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GenericTypeArguments[0];

            //create the tracking lists for unknown and missing elements out here as null for first time init later
            List<string> missingAttributes = null;
            List<string> unknownAttributes = new List<string>();
            List<string> unknownElements = new List<string>();

            //so now we have the xml container (xmlListItems), and the internal memory container (listProperty)
            //now for each xml element, get the value information and set it
            //if it originates from the 
            foreach (XElement listElement in xmlListItems)
            {
                //if we're doing custom typing, then get the type based on the attribute name
                if (customTyping)
                {
                    XAttribute typeResultAttribute = listElement.Attribute(customTypeAttributeName);
                    if (typeResultAttribute == null)
                    {
                        Logging.Error("The custom attribute {0} was missing from the xml element", customTypeAttributeName);
                        errorOccured = true;
                        continue;
                    }

                    string typeResult = typeResultAttribute.Value;
                    if (string.IsNullOrEmpty(typeResult))
                    {
                        Logging.Error("typeResult is null or empty - invalid attribute name", customTypeAttributeName);
                        errorOccured = true;
                        continue;
                    }

                    if (!typeMapper.ContainsKey(typeResult))
                    {
                        Logging.Error("typeResult {0} does not exist in dictionary", typeResult);
                        errorOccured = true;
                        continue;
                    }
                    listObjectType = typeMapper[typeResult];
                }

                //if it's just like a string or something then just load that
                if (listObjectType.IsValueType)
                {
                    if (SetObjectValue(listObjectType, listElement.Value, out object newObject))
                    {
                        listProperty.Add(newObject);
                        continue;
                    }
                }
                else if (listObjectType.Equals(typeof(string)))
                {
                    listProperty.Add(listElement.Value);
                }

                object listEntryObject = Activator.CreateInstance(listObjectType);

                //make sure object type is properly implemented into serialization system
                if (!(listEntryObject is IXmlSerializable listEntry))
                    throw new BadMemeException("Type of this list is not of IXmlSerializable");

                //assign missing attributes if not done already
                if (missingAttributes == null)
                    missingAttributes = new List<string>(listEntry.PropertiesForSerializationAttributes());

                foreach (XAttribute listEntryAttribute in listElement.Attributes())
                {
                    if (!listEntry.PropertiesForSerializationAttributes().Contains(listEntryAttribute.Name.LocalName))
                    {
                        //if the 'unknown' is the custom type for mapping we added, we know what it is and thus don't add it as an unknown
                        if (!(customTyping && listEntryAttribute.Name.LocalName.Equals(customTypeAttributeName)))
                        {
                            unknownAttributes.Add(listEntryAttribute.Name.LocalName);
                        }
                        continue;
                    }

                    PropertyInfo property = listObjectType.GetProperty(listEntryAttribute.Name.LocalName);

                    //check if attribute exists in class object
                    if (property == null)
                    {
                        Logging.Error("Property (xml attribute) {0} exists in array for serialization, but not in class design!, ", listEntryAttribute.Name.LocalName);
                        Logging.Error("Component: {0}, ID: {1}, line: {2}", componentWithIdInternalName, listEntry.ToString(), ((IXmlLineInfo)listElement).LineNumber);
                        errorOccured = true;
                        continue;
                    }

                    //remove from list of potential missing mandatory elements
                    missingAttributes.Remove(listEntryAttribute.Name.LocalName);

                    if (!SetObjectProperty(listEntry, property, listEntryAttribute.Value))
                    {
                        Logging.Error("Failed to set property {0} for element in IList", property.Name);
                        Logging.Error("Component: {0}, ID: {1}, line: {2}", componentWithIdInternalName, listEntry.ToString(), ((IXmlLineInfo)listElement).LineNumber);
                        errorOccured = true;
                    }
                }

                foreach (XElement listEntryElement in listElement.Elements())
                {
                    if (!listEntry.PropertiesForSerializationElements().Contains(listEntryElement.Name.LocalName))
                    {
                        unknownElements.Add(listEntryElement.Name.LocalName);
                        continue;
                    }

                    PropertyInfo property = listObjectType.GetProperty(listEntryElement.Name.LocalName);

                    if (property == null)
                    {
                        Logging.Error("Property (xml element) {0} exists in array for serialization, but not in class design!, ", listEntryElement.Name.LocalName);
                        Logging.Error("Component: {0}, ID: {1}, line: {2}", componentWithIdInternalName, listEntry.ToString(), ((IXmlLineInfo)listElement).LineNumber);
                        errorOccured = true;
                        continue;
                    }

                    //no missing elements (elements are optional)

                    if (!SetObjectProperty(listEntry, property, listEntryElement.Value))
                    {
                        Logging.Error("Failed to set property {0} for element in IList", property.Name);
                        Logging.Error("Component: {0}, ID: {1}, line: {2}", componentWithIdInternalName, listEntry.ToString(), ((IXmlLineInfo)listElement).LineNumber);
                        errorOccured = true;
                    }
                }

                //logging unknown and missings
                foreach (string missingAttribute in missingAttributes)
                {
                    Logging.Error("Missing xml attribute: {0}, Component: {1}, ID: {2}, line: {3}", missingAttribute, componentWithIdInternalName, listEntry.ToString(), ((IXmlLineInfo)listElement).LineNumber);
                    errorOccured = true;
                }
                foreach (string unknownAttribute in unknownAttributes)
                {
                    Logging.Error("Unknown xml attribute: {0}, Component: {1}, ID: {2}, line: {3}", unknownAttribute, componentWithIdInternalName, listEntry.ToString(), ((IXmlLineInfo)listElement).LineNumber);
                    errorOccured = true;
                }
                foreach (string unknownElement in unknownElements)
                {
                    Logging.Error("Unknown xml element: {0}, Component: {1}, ID: {2}, line: {3}", unknownElement, componentWithIdInternalName, listEntry.ToString(), ((IXmlLineInfo)listElement).LineNumber);
                    errorOccured = true;
                }

                listProperty.Add(listEntry);
            }

            return !errorOccured;
        }

        /// <summary>
        /// Creates all database entries in a list property, parsing each list entry object by xmlListItems
        /// </summary>
        /// <param name="databasePackageObject">The database package object with the list property, for example SelectablePackage</param>
        /// <param name="listPropertyInfo">The property metadata/info about the list property, for example Medias</param>
        /// <param name="xmlListItems">The xml element holder for the property object types, for example Medias element holder</param>
        /// <param name="customTypeAttributeName">If using custom typing, the name of the xml attribute to use for lookup of the type of the value to set.</param>
        /// <param name="typeMapper">If using custom typing, a dictionary to map potential string results to type values of the value to create.</param>
        public static bool SetListEntries(IComponentWithID databasePackageObject, PropertyInfo listPropertyInfo, IEnumerable<XElement> xmlListItems, string customTypeAttributeName = null, Dictionary<string, Type> typeMapper = null)
        {
            bool customTyping = !(string.IsNullOrEmpty(customTypeAttributeName));
            if (customTyping && typeMapper == null)
                throw new NullReferenceException(nameof(typeMapper) + " is null");
            if (databasePackageObject == null || listPropertyInfo == null || xmlListItems == null)
                throw new NullReferenceException(string.Format("{0}: null = {1}, {2}: null = {3}, {4}: null = {5}",
                    nameof(databasePackageObject), databasePackageObject == null, nameof(listPropertyInfo), listPropertyInfo == null, nameof(xmlListItems), xmlListItems == null));

            //get the list interfaced component
            IList listProperty = listPropertyInfo.GetValue(databasePackageObject) as IList;

            return SetListEntries(listProperty, databasePackageObject.ComponentInternalName, xmlListItems, customTypeAttributeName, typeMapper);
        }

        /// <summary>
        /// Gets a list of all types that can exist in an enumeration
        /// </summary>
        /// <typeparam name="T">The type of the enumeration to get a list of</typeparam>
        /// <returns>A list of all enumeration values of that type</returns>
        /// <remarks>See https://stackoverflow.com/a/801058/3128017 </remarks>
        public static List<T> GetEnumList<T>()
        {
            T[] array = (T[])Enum.GetValues(typeof(T));
            List<T> list = new List<T>(array);
            return list;
        }
    }
}
