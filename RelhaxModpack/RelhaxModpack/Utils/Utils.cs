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
using RelhaxModpack.AtlasesCreator;
using System.Drawing;
using Size = System.Drawing.Size;
using RelhaxModpack.Windows;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Web;
using RelhaxModpack.DatabaseComponents;
using System.Runtime.CompilerServices;
using RelhaxModpack.UIComponents;

namespace RelhaxModpack
{
    /// <summary>
    /// The types of text macro replacements
    /// </summary>
    public enum ReplacementTypes
    {
        /// <summary>
        /// Replacing macros with file paths
        /// </summary>
        FilePath,

        /// <summary>
        /// Replacing patch arguments of the patch object
        /// </summary>
        PatchArguementsReplace,

        /// <summary>
        /// Replacing modpack created macros (like [quote]) with the corresponding characters
        /// </summary>
        PatchFiles,

        /// <summary>
        /// Replacing literal interpretations of special characters like newline and tab with escaped versions
        /// </summary>
        TextEscape,

        /// <summary>
        /// Replacing escaped versions of special characters like newline and tab with the literal interpretations
        /// </summary>
        TextUnescape,

        /// <summary>
        /// Replacing zip path macros with absolute extraction paths
        /// </summary>
        ZipFilePath
    }

    /// <summary>
    /// Allows the old and new versions of a SelectablePackage to be saved temporarily for comparing differences between two database structures
    /// </summary>
    public struct DatabaseBeforeAfter
    {
        /// <summary>
        /// The package reference for the database before changes
        /// </summary>
        public SelectablePackage Before;

        /// <summary>
        /// The package reference for the database after changes
        /// </summary>
        public SelectablePackage After;
    }

    /// <summary>
    /// Allows the old and new versions of a DatabasePackage to be saved temporarily for comparing differences between two database structures
    /// </summary>
    public struct DatabaseBeforeAfter2
    {
        /// <summary>
        /// The package reference for the database before changes
        /// </summary>
        public DatabasePackage Before;

        /// <summary>
        /// The package reference for the database after changes
        /// </summary>
        public DatabasePackage After;
    }

    /// <summary>
    /// A structure object to contain the WoT client version and online folder version. Allows for LINQ searching
    /// </summary>
    public struct VersionInfos
    {
        /// <summary>
        /// The WoT client version e.g. 1.5.1.3
        /// </summary>
        public string WoTClientVersion;

        /// <summary>
        /// The online folder number (major game version) that contains the game zip files
        /// </summary>
        public string WoTOnlineFolderVersion;

        /// <summary>
        /// Overrides the ToString() function to display the two properties
        /// </summary>
        /// <returns>Displays the WoTClientVersion and WoTOnlineFolderVersion</returns>
        public override string ToString()
        {
            return string.Format("WoTClientVersion={0}, WoTOnlineFolderVersion={1}", WoTClientVersion, WoTOnlineFolderVersion);
        }
    }

    /// <summary>
    /// A structure used to keep a reference of a component and a dependency that it calls
    /// </summary>
    /// <remarks>This is used to determine if any packages call any dependencies who's packageName does not exist in the database</remarks>
    public struct LogicTracking
    {
        /// <summary>
        /// The database component what has dependencies
        /// </summary>
        public IComponentWithDependencies ComponentWithDependencies;

        /// <summary>
        /// The called dependency from the component
        /// </summary>
        public DatabaseLogic DatabaseLogic;
    }

    /// <summary>
    /// A structure to help with searching for inside the registry by providing a base area to start, and a string search path
    /// </summary>
    public struct RegistrySearch
    {
        /// <summary>
        /// Where to base the search in the registry (current_user, local_machiene, etc.)
        /// </summary>
        public RegistryKey Root;

        /// <summary>
        /// The absolute folder path to the desired registry entries
        /// </summary>
        public string Searchpath;
    }

    /// <summary>
    /// A utility class for static functions used in various places in the modpack
    /// </summary>
    public static class Utils
    {
        #region Statics
        /// <summary>
        /// Multiply by this value to convert milliseconds to seconds
        /// </summary>
        public const int TO_SECONDS = 1000;

        /// <summary>
        /// Multiply by this value to convert seconds to minuets
        /// </summary>
        public const int TO_MINUETS = 60;

        /// <summary>
        /// A list of file size constructs from bytes to Yotabytes
        /// </summary>
        /// <remarks>{ "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" }</remarks>
        public static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        /// <summary>
        /// Multiply by this to convert bytes to megabytes
        /// </summary>
        public const long BYTES_TO_MBYTES = 1048576;

        /// <summary>
        /// The link to the Microsoft Visual C++ dll package required by the atlas processing libraries
        /// </summary>
        public const string MSVCPLink = "https://www.microsoft.com/en-us/download/details.aspx?id=40784";

        //MACROS
        //FilePath macro
        //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/how-to-initialize-a-dictionary-with-a-collection-initializer
        //build at install time
        /// <summary>
        /// The dictionary to store filepath macros
        /// </summary>
        public static Dictionary<string, string> FilePathDict = new Dictionary<string, string>();

        /// <summary>
        /// The dictionary to store patch argument (replace) macros
        /// </summary>
        public static Dictionary<string, string> PatchArguementsReplaceDict = new Dictionary<string, string>()
        {
            {@"[sl]", @"/" }
        };

        /// <summary>
        /// The dictionary to store patch file replacement macros
        /// </summary>
        public static Dictionary<string, string> PatchFilesDict = new Dictionary<string, string>()
        {
            //add all patch file escape characters
            //key (look for), value (replaced with)
            {@"""[xvm_dollar]", @"$" },
            {@"[xvm_rbracket]""", @"}" },
            {@"[lbracket]", @"{" },
            {@"[rbracket]", @"}" },
            {@"[quote]", "\"" },
            {@"[colon]", @":" },
            {@"[dollar]", @"$" },
        };

        /// <summary>
        /// The dictionary to store escaped text characters with the literal versions
        /// </summary>
        public static Dictionary<string, string> TextUnscapeDict = new Dictionary<string, string>()
        {
            //ORDER MATTERS
            {@"\n", "\n" },
            {@"\r", "\r" },
            {@"\t", "\t" },
            //legacy compatibility (i can't believe i did this....)
            {@"newline", "\n" }
        };

        /// <summary>
        /// The dictionary to store literal versions of characters with their escaped versions
        /// </summary>
        public static Dictionary<string, string> TextEscapeDict = new Dictionary<string, string>()
        {
            //ORDER MATTERS
            {"\n", @"\n" },
            {"\r", @"\r" },
            {"\t", @"\t" }
        };

        /// <summary>
        /// Provides the ability to insert a 'null' value into json configurations
        /// </summary>
        public const string PatchJsonNullEscape = "[null]";
        #endregion

        #region Unmanaged Library stuff
        /// <summary>
        /// The manager instance of the FreeImage Library
        /// </summary>
        public static RelhaxFreeImageLibrary FreeImageLibrary = new RelhaxFreeImageLibrary();

        /// <summary>
        /// The manager instance of the Nvidia Texture Tools Library
        /// </summary>
        public static RelhaxNvTexLibrary NvTexLibrary = new RelhaxNvTexLibrary();

        /// <summary>
        /// Test the ability to load an unmanaged library
        /// </summary>
        /// <returns>True if library loaded, false otherwise</returns>
        public static bool TestLibrary(IRelhaxUnmanagedLibrary library, string name, bool unload)
        {
            Logging.Info("testing {0} library", name);
            bool libraryLoaded;
            if (!library.IsLoaded)
            {
                if (library.Load())
                {
                    Logging.Info("library loaded successfully");
                    libraryLoaded = true;
                }
                else
                {
                    Logging.Error("library failed to load");
                    libraryLoaded = false;
                }
            }
            else
            {
                Logging.Info("library already loaded");
                libraryLoaded = true;
            }

            if(unload && library.IsLoaded)
            {
                Logging.Info("unload requested and library is loaded, unloading");
                if (library.Unload())
                {
                    Logging.Info("library unloaded successfully");
                }
                else
                {
                    Logging.Error("library failed to unload library");
                    libraryLoaded = false;
                }
            }
            return libraryLoaded;
        }

        /// <summary>
        /// Test the ability to load and unload all the atlas image processing libraries
        /// </summary>
        /// <returns>True if both libraries loaded, false otherwise</returns>
        public static bool TestLoadAtlasLibraries(bool unload)
        {
            bool freeImageLoaded = TestLibrary(FreeImageLibrary, "FreeImage", true);
            bool nvttLoaded = TestLibrary(NvTexLibrary, "nvtt", true);

            if(nvttLoaded && freeImageLoaded)
            {
                Logging.Info("TestLoadAtlasLibraries(): both libraries loaded");
                return true;
            }
            else
            {
                Logging.Error("TestLoadAtlasLibraries(): failed to load one or more atlas processing libraries: freeImage={0}, nvtt={1}",
                    freeImageLoaded.ToString(), nvttLoaded.ToString());
                return false;
            }
        }

        /// <summary>
        /// Get a complete assembly name based on a matching keyword
        /// </summary>
        /// <param name="keyword">The keyword to match</param>
        /// <returns>The first matching assembly name, or null if no matches</returns>
        public static string GetAssemblyName(string keyword)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(rn => rn.Contains(keyword));
        }

        #endregion

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
            string xmlString = GetStringFromZip(Settings.ManagerInfoZipfile, "manager_version.xml");
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

        /// <summary>
        /// Get a list of all logical components in the window
        /// </summary>
        /// <param name="window">The window to get the list of</param>
        /// <param name="includeWindow">if the list should include the window itself</param>
        /// <returns>A list of type FrameowrkElement of all components</returns>
        public static List<FrameworkElement> GetAllWindowComponentsLogical(Window window, bool includeWindow)
        {
            List<FrameworkElement> windowComponents = new List<FrameworkElement>();
            if (includeWindow)
                windowComponents.Add(window);
            GetAllWindowComponentsLogical(window, windowComponents);
            return windowComponents;
        }

        /// <summary>
        /// Get a list of all logical components in the window
        /// </summary>
        /// <param name="rootElement">The element to get the list of logical items from</param>
        /// <param name="addRoot">If this rootElement should be added to the list</param>
        /// <returns></returns>
        public static List<FrameworkElement> GetAllWindowComponentsLogical(FrameworkElement rootElement, bool addRoot)
        {
            List<FrameworkElement> components = new List<FrameworkElement>();
            if (addRoot)
                components.Add(rootElement);
            GetAllWindowComponentsLogical(rootElement, components);
            return components;
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
        //Gets any logical components that are not currently shown (like elements behind a tab)
        private static void GetAllWindowComponentsLogical(FrameworkElement v, List<FrameworkElement> allWindowComponents)
        {
            //NOTE: v has been added
            //have to use var here cause i got NO CLUE what type it is #niceMeme
            if(v == null)
            {
                Logging.Error("parameter \"v\" is null, skipping");
                return;
            }
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

        /// <summary>Checks if a point is inside the possible monitor space</summary>
        /// <param name="x">The x coordinate of the point</param>
        /// <param name="y">The y coordinate of the point</param>
        public static bool PointWithinScreen(int x, int y)
        {
            return PointWithinScreen(new System.Drawing.Point(x, y));
        }

        /// <summary>Checks if a point is inside the possible monitor space</summary>
        /// <param name="p">The point to check</param>
        public static bool PointWithinScreen(System.Drawing.Point p)
        {
            //if either x or y are negative it's an invalid location
            if (p.X < 0 || p.Y < 0)
                return false;
            int totalWidth = 0, totalHeight = 0;
            foreach (System.Windows.Forms.Screen s in System.Windows.Forms.Screen.AllScreens)
            {
                totalWidth += s.Bounds.Width;
                totalHeight += s.Bounds.Height;
            }
            if (totalWidth > p.X && totalHeight > p.Y)
                return true;
            return false;
        }


        /// <summary>
        /// Injects a Dispatcher frame followed by an idle backgrouned operation to allow for the UI to update during an intensive operation on the UI thread
        /// </summary>
        /// <remarks>See https://stackoverflow.com/questions/37787388/how-to-force-a-ui-update-during-a-lengthy-task-on-the-ui-thread 
        /// <para>and https://stackoverflow.com/questions/2329978/the-calling-thread-must-be-sta-because-many-ui-components-require-this</para></remarks>
        public static void AllowUIToUpdate()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)
            {
                frame.Continue = false;
                return null;
            }), null);

            Dispatcher.PushFrame(frame);
            //EDIT:
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                          new Action(delegate { }));
        }

        /// <summary>
        /// Applies vector based application scaling to the specified window
        /// </summary>
        /// <param name="window">The window to apply scaling to</param>
        /// <param name="scaleValue">The amount of scaling, in a multiplication factor, to apply to the window from</param>
        public static void ApplyApplicationScale(Window window, double scaleValue)
        {
            //input filtering
            if(scaleValue < Settings.MinimumDisplayScale)
            {
                Logging.Warning("scale size of {0} is to small, setting to 1", scaleValue.ToString("N"));
                scaleValue = Settings.MinimumDisplayScale;
            }
            if (scaleValue > Settings.MaximumDisplayScale)
            {
                Logging.Warning("scale size of {0} is to large, setting to 3", scaleValue.ToString("N"));
                scaleValue = Settings.MaximumDisplayScale;
            }

            //scale internals
            (window.Content as FrameworkElement).LayoutTransform = new ScaleTransform(scaleValue, scaleValue, 0, 0);

            //scale window itself
            if (window is MainWindow mw)
            {
                mw.Width = mw.OriginalWidth * scaleValue;
                mw.Height = mw.OriginalHeight * scaleValue;
            }
            else if (window is RelhaxWindow rw)
            {
                rw.Width = rw.OriginalWidth * scaleValue;
                rw.Height = rw.OriginalHeight * scaleValue;
            }
            else
                throw new BadMemeException("you should probably make me a RelhaxWindow if you want to use this feature");
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
            //return if arg is null or empty
            if (string.IsNullOrWhiteSpace(inputFile))
                return "-1";

            //return if the file does not exist
            if (!File.Exists(inputFile))
                return "-1";

            FileStream stream = null;
            string result = string.Empty;
            using (stream = File.OpenRead(inputFile))
            {
                result = CreateMD5Hash(stream);
            }

            if(result.Equals("-1"))
            {
                Logging.Error("Failed to check MD5 of file " + inputFile);
            }
            return result;
        }

        /// <summary>
        /// Creates an MD5 hash calculation from and stream object
        /// </summary>
        /// <param name="stream">The stream object to calculate from</param>
        /// <returns>The MD5 calculated hash</returns>
        /// <exception cref="ArgumentNullException"/>
        public static string CreateMD5Hash(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            //Create a new Stringbuilder to collect the bytes
            StringBuilder sBuilder = new StringBuilder();
            MD5 md5Hash;
            try
            {
                using (md5Hash = MD5.Create())
                {
                    //Convert the input string to a byte array and compute the hash
                    byte[] data = md5Hash.ComputeHash(stream);
                    stream.Close();

                    //Loop through each byte of the hashed data 
                    //and format each one as a hexadecimal string.
                    for (int i = 0; i < data.Length; i++)
                    {
                        sBuilder.Append(data[i].ToString("x2"));
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
                return "-1";
            }

            //Return the hexadecimal string.
            return sBuilder.ToString();
        }

        /// <summary>
        /// Creates an MD5 hash calculation of the input file
        /// </summary>
        /// <param name="inputFile">The path to the file to calculate</param>
        /// <returns></returns>
        public static async Task<string> CreateMD5HashAsync(string inputFile)
        {
            //https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming
            //Task taskA = Task.Run( () => Console.WriteLine("Hello from taskA."));
            //https://stackoverflow.com/questions/38423472/what-is-the-difference-between-task-run-and-task-factory-startnew
            /*
                in the .NET Framework 4.5 Developer Preview, we’ve introduced the new Task.Run method. This in no way obsoletes Task.Factory.StartNew,
                but rather should simply be thought of as a quick way to use Task.Factory.StartNew without needing to specify a bunch of parameters.
                It’s a shortcut. In fact, Task.Run is actually implemented in terms of the same logic used for Task.Factory.StartNew, just passing in
                some default parameters. When you pass an Action to Task.Run:

                'Task.Run(someAction);'

                it's exactly equivalent to:

                'Task.Factory.StartNew(someAction, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);'
             */
            return await Task.Run(() => CreateMD5Hash(inputFile));
        }

        /// <summary>
        /// Creates an MD5 hash calculation from and stream object
        /// </summary>
        /// <param name="stream">The stream object to calculate from</param>
        /// <returns>The MD5 calculated hash</returns>
        /// <exception cref="ArgumentNullException"/>
        public static async Task<string> CreateMD5HashAsync(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return await Task.Run(() => CreateMD5Hash(stream));
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
                Logging.Error("Zip file {0} not found", zipFilename);
                return null;
            }
            using (ZipFile zip = ZipFile.Read(zipFilename))
                return GetStringFromZip(zip, archivedFilename, password);
        }

        /// <summary>
        /// Gets the string contents of a text based file inside a zip file
        /// </summary>
        /// <param name="zip">The zipfile to extract the entry from</param>
        /// <param name="archivedFilename">The archive path to the entry</param>
        /// <param name="password">The password to use when extracting the entry. Leave blank for no password</param>
        /// <returns></returns>
        public static string GetStringFromZip(ZipFile zip, string archivedFilename, string password = "")
        {
            //make sure the entry exists in the stream first
            if(!zip.ContainsEntry(archivedFilename))
            {
                Logging.Error("entry {0} does not exist in given zip file", archivedFilename);
                return null;
            }

            using (MemoryStream ms = new MemoryStream() { Position = 0 })
            using (StreamReader sr = new StreamReader(ms))
            {
                ZipEntry e = zip[archivedFilename];

                //if a password is provided, then use it for extraction
                if (!string.IsNullOrWhiteSpace(password))
                    e.ExtractWithPassword(ms, password);
                else
                    e.Extract(ms);

                //read stream
                ms.Position = 0;
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// Deletes any empty directories from a given path
        /// </summary>
        /// <param name="startLocation">The location to start from. Includes deleting empty directories from this point</param>
        /// <param name="recursive">Toggle to check inside the starting location for empty folders</param>
        /// <param name="numRetrys">The number of times the method should retry after receiving an exception</param>
        /// <param name="timeout">The time to wait between retries</param>
        /// <returns>True if the operation completed successfully, false otherwise</returns>
        public static bool ProcessEmptyDirectories(string startLocation, bool recursive, uint numRetrys = 3, uint timeout = 100)
        {
            //if the root does not exist then stop now
            if (!Directory.Exists(startLocation))
            {
                Logging.Warning("start location {0} does not exist, skipping", startLocation);
                return true;
            }

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

            uint retryCounter = 0;
            if (recursive)
            {
                //get the list of all directories inside it, no need to recursively process
                List<string> directories = DirectorySearch(startLocation, SearchOption.AllDirectories,false).ToList().Where(direct => Directory.Exists(direct)).ToList();

                //sort and reverse the list to make longer paths on top to simulate recursively deleting from all the way down to up
                directories.Sort();
                directories.Reverse();

                //now can delete for each folder
                foreach (string directory in directories)
                {
                    retryCounter = 0;
                    while (retryCounter < numRetrys)
                    {
                        try
                        {
                            if (Directory.GetFiles(directory).Length == 0 && Directory.GetDirectories(directory).Length == 0)
                            {
                                Logging.Debug("Deleting empty directory {0}", directory);
                                Directory.Delete(directory, false);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logging.Warning("failed to delete {0}, retryCount={1}, message:\n{2}", directory, retryCounter, ex.Message);
                            retryCounter++;
                            System.Threading.Thread.Sleep((int)timeout);
                            if (retryCounter == numRetrys)
                            {
                                Logging.Error("retries = counter, fully failed to delete directory {0}", directory);
                                return false;
                            }
                        }
                    }
                }
            }

            //and process the root
            retryCounter = 0;
            while (retryCounter < numRetrys)
            {
                try
                {
                    if (Directory.GetFiles(startLocation).Length == 0 && Directory.GetDirectories(startLocation).Length == 0)
                    {
                        Logging.Debug("Deleting empty directory {0}", startLocation);
                        Directory.Delete(startLocation, false);
                    }
                    retryCounter = numRetrys;
                }
                catch (Exception ex)
                {
                    Logging.Warning("failed to delete {0}, retryCount={1}, message:\n{2}", startLocation, retryCounter, ex.Message);
                    retryCounter++;
                    System.Threading.Thread.Sleep((int)timeout);
                    if (retryCounter == numRetrys)
                    {
                        Logging.Error("retries = counter, fully failed to delete directory {0}", startLocation);
                        return false;
                    }
                }
            }
            return true;
        }
        
        /// <summary>
        /// Calculates and returns the size magnitude of the file (kilo, mega, giga...)
        /// </summary>
        /// <param name="value">The file size in bytes</param>
        /// <param name="decimalPlaces">The number of decimal places to maintain in the result</param>
        /// <param name="sizeSuffix">If it should return the byte symbol with the size amount (KB, MB, etc.)</param>
        /// <param name="ignoreSizeWarningIf0">If set to true, the application log will not show values about the passed in value for size calculation being 0. 
        /// File of 0 size, for example.</param>
        /// <returns>The string representation to decimalPlaces of the file size optionally with the bytes parameter</returns>
        public static string SizeSuffix(ulong value, uint decimalPlaces = 1, bool sizeSuffix = false, bool ignoreSizeWarningIf0 = false)
        {
            if (value == 0)
            {
                if(!ignoreSizeWarningIf0)
                    Logging.Warning("SizeSuffix value is 0 (is this the intent?)");
                if (sizeSuffix)
                    return "0.0 bytes";
                else
                    return "0.0";
            }

            if (value < 1000)
            {
                if (sizeSuffix)
                    return string.Format("{0:n" + decimalPlaces + "} {1}", 0.1, SizeSuffixes[1]);
                else
                    return string.Format("{0:n" + decimalPlaces + "}", 0.1);
            }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, (int)decimalPlaces) >= 1000)
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
        /// Gets the size of the file in bytes
        /// </summary>
        /// <param name="filepath">The string path to the file</param>
        /// <returns>The size of the file in bytes</returns>
        /// <remarks>This is a wrapper for the FileInfo.Length property</remarks>
        public static long GetFilesize(string filepath)
        {
            //https://stackoverflow.com/questions/1380839/how-do-you-get-the-file-size-in-c
            return new FileInfo(filepath).Length;
        }

        /// <summary>
        /// Checks if a filename has invalid characters and replaces them with underscores
        /// </summary>
        /// <param name="fileName">The filename to replace characters from</param>
        /// <returns>The filename with valid characters</returns>
        public static string GetValidFilename(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        /// <summary>
        /// Attempts to move a file from source to destination with numRetrys, with a file timeout of timeout
        /// </summary>
        /// <param name="source">The source file to move</param>
        /// <param name="destination">The destination of the file to move</param>
        /// <param name="numRetrys">The number of fail retries if it failes to move the file</param>
        /// <param name="timeout">The timeout, in milliseconds, to wait between faliures</param>
        /// <returns>True if the file was moved, false otherwise</returns>
        /// <remarks>This method does NOT work to move a file across physical drives. 
        /// This method does NOT check if the destination file already exists.</remarks>
        public static bool FileMove(string source, string destination, uint numRetrys = 3, uint timeout = 100)
        {
            bool overallSuccess = true;
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
            uint retryCounter = 0;
            while (retryCounter < numRetrys)
            {
                try
                {
                    File.Move(source, destination);
                    retryCounter = numRetrys;
                }
                catch (Exception ex)
                {
                    Logging.WriteToLog(string.Format("move file {0} -> {1}, retryCount={2}, message:\n{3}", source, destination,retryCounter, ex.Message),
                        Logfiles.Application, LogLevel.Warning);
                    retryCounter++;
                    System.Threading.Thread.Sleep((int)timeout);
                    if (retryCounter == numRetrys)
                    {
                        Logging.Error("retries = counter, fully failed to move file {0} -> {1}", source, destination);
                        overallSuccess = false;
                    }
                }
            }
            return overallSuccess;
        }

        /// <summary>
        /// Tries to delete a file from the given path
        /// </summary>
        /// <param name="file">The file to delete</param>
        /// <param name="numRetrys">The number of retires if an exception is encountered</param>
        /// <param name="timeout">The number of milliseconds between retries</param>
        /// <returns>True is the file operation was successful, false otherwise</returns>
        public static bool FileDelete(string file, uint numRetrys = 3, uint timeout = 100)
        {
            bool overallSuccess = true;
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
            uint retryCounter = 0;
            while (retryCounter < numRetrys)
            {
                try
                {
                    File.Delete(file);
                    retryCounter = numRetrys;
                }
                catch (Exception ex)
                {
                    Logging.WriteToLog(string.Format("failed to delete {0}, retryCount={1}, message:\n{2}", file, retryCounter, ex.Message),
                        Logfiles.Application, LogLevel.Warning);
                    retryCounter++;
                    System.Threading.Thread.Sleep((int)timeout);
                    if (retryCounter == numRetrys)
                    {
                        Logging.Error("retries = counter, fully failed to delete file {0}", file);
                        overallSuccess = false;
                    }
                }
            }
            return overallSuccess;
        }

        /// <summary>
        /// Deletes files in a directory
        /// </summary>
        /// <param name="folderPath">The directory to delete files in</param>
        /// <param name="deleteSubfolders">Toggle if the method should recursively look inside directory</param>
        /// <param name="deleteRoot">Toggle if the method should delete the folderPath directory</param>
        /// <param name="numRetrys">The number of retires to delete a file entry before failing</param>
        /// <param name="timeout">The time in milliseconds between retries</param>
        /// <param name="pattern">The pattern of files to search for in a directory</param>
        /// <returns>True if the complete operation was a success, false otherwise</returns>
        public static bool DirectoryDelete(string folderPath, bool deleteSubfolders, bool deleteRoot = true, uint numRetrys = 3, uint timeout = 100, string pattern = "*")
        {
            bool overallSuccess = true;
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
            uint retryCounter;
            foreach (string file in Directory.GetFiles(folderPath,pattern,SearchOption.TopDirectoryOnly))
            {
                retryCounter = 0;
                while (retryCounter < numRetrys)
                {
                    try
                    {
                        File.Delete(file);
                        retryCounter = numRetrys;
                    }
                    catch(Exception ex)
                    {
                        Logging.WriteToLog(string.Format("failed to delete {0}, retryCount={1}, message:\n{2}", file, retryCounter, ex.Message),
                            Logfiles.Application,LogLevel.Warning);
                        retryCounter++;
                        System.Threading.Thread.Sleep((int)timeout);
                        if(retryCounter == numRetrys)
                        {
                            Logging.Error("retries = counter, fully failed to delete file {0}",file);
                            overallSuccess = false;
                        }
                    }
                }
            }
            //if deleting the sub directories
            if (deleteSubfolders)
            {
                foreach (string dir in Directory.GetDirectories(folderPath,pattern,SearchOption.TopDirectoryOnly))
                {
                    if (!DirectoryDelete(dir, deleteSubfolders, true, numRetrys, timeout))
                        overallSuccess = false;
                }
            }
            //delete the folder as well (if requested)
            if(deleteRoot)
            {
                retryCounter = 0;
                while (retryCounter < numRetrys)
                {
                    try
                    {
                        Directory.Delete(folderPath);
                        retryCounter = numRetrys;
                    }
                    catch (Exception ex)
                    {
                        Logging.WriteToLog(string.Format("failed to delete {0} (empty folder), retryCount={1}, message:\n{2}", folderPath, retryCounter, ex.Message),
                            Logfiles.Application, LogLevel.Error);
                        retryCounter++;
                        System.Threading.Thread.Sleep((int)timeout);
                        if (retryCounter == numRetrys)
                        {
                            Logging.Debug("retries = counter, fully failed to delete file {0}", folderPath);
                            overallSuccess = false;
                        }
                    }
                }
            }
            return overallSuccess;
        }

        /// <summary>
        /// Async wrapper around DirectoryDelete() method. Deletes files in a directory
        /// </summary>
        /// <param name="folderPath">The directory to delete files in</param>
        /// <param name="deleteSubfolders">Toggle if the method should recursively look inside directory</param>
        /// <param name="deleteRoot">Toggle if the method should delete the folderPath directory</param>
        /// <param name="numRetrys">The number of retires to delete a file entry before failing</param>
        /// <param name="timeout">The time in milliseconds between retries</param>
        /// <param name="pattern">The pattern of files to search for in a directory</param>
        /// <returns>True if the complete operation was a success, false otherwise</returns>
        public static async Task DirectoryDeleteAsync(string folderPath, bool deleteSubfolders, bool deleteRoot = true, uint numRetrys = 3, uint timeout = 100, string pattern = "*")
        {
            //Task taskA = Task.Run( () => Console.WriteLine("Hello from taskA."));
            await Task.Run(() => DirectoryDelete(folderPath, deleteSubfolders, deleteRoot, numRetrys, timeout, pattern));
        }

        /// <summary>
        /// Move a directory and its files to a new location. Works across drive letters.
        /// </summary>
        /// <param name="source">The source path of the directory to move from</param>
        /// <param name="destination">The destination path of the directory to move to</param>
        /// <param name="recursive">Toggle if the sub-folders and files should be moved as well</param>
        /// <param name="numRetrys">The number of retires to delete a file entry before failing</param>
        /// <param name="timeout">The time in milliseconds between retries</param>
        /// <param name="pattern">The pattern of files to search for in a directory</param>
        /// <remarks>The DirectoryMove method works across drive letters and other physical separate drives because it deletes and re-creates folders rather then trying to move them</remarks>
        public static void DirectoryMove(string source, string destination, bool recursive, uint numRetrys = 3, uint timeout = 100, string pattern = "*")
        {
            //make the destination if it does not already exist
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            //DirectoryMove works by getting a directory list of all directories in the source to create,
            //then making the directories, moving the files, and then deleting the old directories
            List<string> directoreisToCreate = Directory.GetDirectories(source, pattern,
                recursive? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();

            //create them at the target
            foreach(string fullPath in directoreisToCreate)
            {
                //trim out the base path so we only have the new path left
                string partPath = fullPath.Substring(source.Length+1);
                string newPath = Path.Combine(destination, partPath);
                if(!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }
            }

            //move the files over
            List<string> filesToMove = Directory.GetFiles(source, pattern,
                recursive? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            foreach(string file in filesToMove)
            {
                string partPath = file.Substring(source.Length + 1);
                string newPath = Path.Combine(destination, partPath);
                File.Move(file, newPath);
            }

            //delete all the other old empty source directories
            directoreisToCreate.Sort();
            directoreisToCreate.Reverse();
            foreach(string fullPath in directoreisToCreate)
            {
                if(Directory.Exists(fullPath))
                {
                    if (Directory.GetFiles(fullPath,"*",SearchOption.AllDirectories).Count() > 0)
                        throw new BadMemeException("waaaaaaa?");
                    Directory.Delete(fullPath);
                }
            }
        }

        /// <summary>
        /// Copy a directory and its files to a new location
        /// </summary>
        /// <param name="source">The source path of the directory to move from</param>
        /// <param name="destination">The destination path of the directory to move to</param>
        /// <param name="recursive">Toggle if the sub-folders and files should be moved as well</param>
        /// <param name="numRetrys">The number of retires to delete a file entry before failing</param>
        /// <param name="timeout">The time in milliseconds between retries</param>
        /// <param name="pattern">The pattern of files to search for in a directory</param>
        public static void DirectoryCopy(string source, string destination, bool recursive, uint numRetrys = 3, uint timeout = 100, string pattern = "*")
        {
            List<string> directoreisToCreate = Directory.GetDirectories(source, pattern,
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            //create them at the target
            foreach (string s in directoreisToCreate)
            {
                if (!Directory.Exists(Path.Combine(destination, s)))
                {
                    Directory.CreateDirectory(Path.Combine(destination, s));
                }
            }
            //copy the files over
            List<string> filesToMove = Directory.GetFiles(source, pattern,
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            foreach (string file in filesToMove)
            {
                File.Copy(Path.Combine(source, file), Path.Combine(destination, file));
            }
        }

        /// <summary>
        /// Return a list of files from a directory
        /// </summary>
        /// <param name="directoryPath">The directory to search for files</param>
        /// <param name="option">Specifies to search this top directory or subdirectories to the Directory.GetFiles() method</param>
        /// <param name="includeDirectoryRoot">Toggle if the directoryPath should be included in the list of files</param>
        /// <param name="searchPattern">The search pattern for finding files in a directory</param>
        /// <param name="numRetrys">The number of retires to delete a file entry before failing</param>
        /// <param name="timeout">The time in milliseconds between retries</param>
        /// <param name="applyFolderProperties">Toggle if the "Normal" file property as assigned to these files at the same time</param>
        /// <returns>The list of files if the search operation was successful, otherwise null</returns>
        public static string[] DirectorySearch(string directoryPath, SearchOption option, bool includeDirectoryRoot, string searchPattern = "*",
            uint timeout = 5, uint numRetrys = 3, bool applyFolderProperties = true)
        {
            //filter input
            if(numRetrys == 0)
            {
                Logging.Warning("numRetrys needs to be larger than 0! setting to 1");
                numRetrys++;
            }
            //loop for how many times to try (in case the OS herped a derp, for example)
            while(numRetrys > 0)
            {
                //if a timout is requested, then sleep the thread
                if (timeout > 0)
                    System.Threading.Thread.Sleep((int)timeout);
                //put it in a try catch block
                try
                {
                    if(!Directory.Exists(directoryPath))
                    {
                        Logging.WriteToLog(string.Format("Path {0} does not exist!", directoryPath), Logfiles.Application, LogLevel.Warning);
                        return null;
                    }
                    if (applyFolderProperties)
                        File.SetAttributes(directoryPath, FileAttributes.Directory);
                    //add the directory path itself to the search
                    List<string> files = Directory.GetFiles(directoryPath, searchPattern, option).ToList();
                    if(includeDirectoryRoot)
                        files.Insert(0, directoryPath);
                    return files.ToArray();
                }
                catch (Exception e)
                {
                    //decreate the number of times we will retry to get the files
                    numRetrys--;
                    if(numRetrys == 0)
                    {
                        //give up; report it and move on
                        Logging.WriteToLog(string.Format("Failed to get files fo directory {0}\n{1}", Path.GetFullPath(directoryPath), e.ToString()),
                            Logfiles.Application, LogLevel.Exception);
                        return null;
                    }
                    else
                    {
                        Logging.WriteToLog(string.Format("Failed to get files for direcotry {0}\nThis is attempt {1} of 0",
                            Path.GetFullPath(directoryPath), numRetrys), Logfiles.Application, LogLevel.Warning);
                    }
                }
            }
            Logging.WriteToLog("Code shuld not reach this point: Utils.DirectorySearch()", Logfiles.Application, LogLevel.Error);
            return null;
        }

        /// <summary>
        /// Applies the "Normal" file attribute to a file
        /// </summary>
        /// <param name="file">The file to apply normal attributes to</param>
        public static void ApplyNormalFileProperties(string file)
        {
            //check to make sure it's eithor a file or folder
            if (!File.Exists(file) && !Directory.Exists(file))
            {
                Logging.WriteToLog("file/folder does not exist " + file, Logfiles.Application, LogLevel.Error);
                return;
            }
            try
            {
                FileAttributes attribute = File.GetAttributes(file);
                if (attribute != FileAttributes.Normal)
                {
                    Logging.WriteToLog(string.Format("file {0} has FileAttribute {1}, setting to FileAttributes.Normal",
                        file, attribute.ToString()), Logfiles.Application, LogLevel.Debug);
                    File.SetAttributes(file, FileAttributes.Normal);
                }
            }
            catch (Exception e)
            {
                Logging.WriteToLog("Failed to apply normal attribute\n" + e.ToString(), Logfiles.Application, LogLevel.Exception);
                return;
            }
        }

        /// <summary>
        /// Removes the directory character and Wots 'win32' and/or 'win64' directories if it exists in the string
        /// </summary>
        /// <param name="wotPath">The path to the WoT exe</param>
        /// <returns>The absolute directory path to the World_of_Tanks folder</returns>
        /// <remarks>This is for in case the user specifies the WoT exe inside the win32 and/or win64 folders</remarks>
        public static string RemoveWoT32bit64bitPathIfExists(string wotPath)
        {
            return wotPath.Replace(Settings.WoT32bitFolderWithSlash, string.Empty).Replace(Settings.WoT64bitFolderWithSlash, string.Empty);
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

        #region Database Utils
        /// <summary>
        /// Checks for any duplicate UID entries inside the provided lists
        /// </summary>
        /// <param name="globalDependencies">The list of global dependencies</param>
        /// <param name="dependencies">The list of dependencies</param>
        /// <param name="parsedCategoryList">The list of categories</param>
        /// <returns>A list of packages with duplicate UIDs, or an empty list if no duplicates</returns>
        public static List<DatabasePackage> CheckForDuplicateUIDsPackageList(List<DatabasePackage> globalDependencies, List<Dependency> dependencies, List<Category> parsedCategoryList)
        {
            List<DatabasePackage> duplicatesList = new List<DatabasePackage>();
            List<DatabasePackage> flatList = GetFlatList(globalDependencies, dependencies, null, parsedCategoryList);
            foreach (DatabasePackage package in flatList)
            {
                List<DatabasePackage> packagesWithMatchingUID = flatList.FindAll(item => item.UID.Equals(package.UID));
                //by default it will at least match itself
                if (packagesWithMatchingUID.Count > 1)
                    duplicatesList.Add(package);
            }
            return duplicatesList;
        }

        /// <summary>
        /// Checks for any duplicate UID entries inside the provided lists
        /// </summary>
        /// <param name="globalDependencies">The list of global dependencies</param>
        /// <param name="dependencies">The list of dependencies</param>
        /// <param name="parsedCategoryList">The list of categories</param>
        /// <returns>A list of duplicate UIDs, or an empty list if no duplicates</returns>
        public static List<string> CheckForDuplicateUIDsStringsList(List<DatabasePackage> globalDependencies, List<Dependency> dependencies, List<Category> parsedCategoryList)
        {
            return CheckForDuplicateUIDsPackageList(globalDependencies, dependencies, parsedCategoryList).Select(package => package.UID).ToList();
        }

        /// <summary>
        /// Checks for any duplicate PackageName entries inside the provided lists
        /// </summary>
        /// <param name="globalDependencies">The list of global dependencies</param>
        /// <param name="dependencies">The list of dependencies</param>
        /// <param name="parsedCategoryList">The list of categories</param>
        /// <returns>A list of duplicate packages, or an empty list if no duplicates</returns>
        public static List<string> CheckForDuplicates(List<DatabasePackage> globalDependencies, List<Dependency> dependencies, List<Category> parsedCategoryList)
        {
            List<string> duplicatesList = new List<string>();
            List<DatabasePackage> flatList = GetFlatList(globalDependencies, dependencies, null, parsedCategoryList);
            foreach(DatabasePackage package in flatList)
            {
                List<DatabasePackage> packagesWithPackagename = flatList.Where(item => item.PackageName.Equals(package.PackageName)).ToList();
                if (packagesWithPackagename.Count > 1)
                    duplicatesList.Add(package.PackageName);
            }
            return duplicatesList;
        }

        /// <summary>
        /// Checks if a packageName exists within a list of packages
        /// </summary>
        /// <param name="packagesToCheckWith">The list of packages to check inside</param>
        /// <param name="nameToCheck">The PackageName parameter to check</param>
        /// <returns>True if the nameToCheck exists in the list, false otherwise</returns>
        public static bool IsDuplicateName(List<DatabasePackage> packagesToCheckWith, string nameToCheck)
        {
            foreach(DatabasePackage package in packagesToCheckWith)
            {
                if (package.PackageName.Equals(nameToCheck))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Sorts the packages inside each Category object
        /// </summary>
        /// <param name="parsedCategoryList">The list of categories to sort</param>
        public static void SortDatabase(List<Category> parsedCategoryList)
        {
            //the first level of packages are always sorted
            foreach(Category cat in parsedCategoryList)
            {
                SortDatabase(cat.Packages);
            }
        }

        /// <summary>
        /// Sorts a list of packages
        /// </summary>
        /// <param name="packages">The list of packages to sort</param>
        /// <param name="recursive">If the list should recursively sort</param>
        private static void SortDatabase(List<SelectablePackage> packages, bool recursive = true)
        {
            //sorts packages in alphabetical order
            packages.Sort(SelectablePackage.CompareModsName);
            if (recursive)
            {
                //if set in the database, child elements can be sorted as well
                foreach (SelectablePackage child in packages)
                {
                    if (child.SortChildPackages)
                    {
                        Logging.Debug("Sorting packages of package {0}", child.PackageName);
                        SortDatabase(child.Packages);
                    }
                }
            }
        }

        /// <summary>
        /// Links all the references (like parent, etc) for each class object making it possible to traverse the list tree in memory
        /// </summary>
        /// <param name="ParsedCategoryList">The List of categories</param>
        /// <param name="buildFakeParents">If the header parent SelectablePackage objects should be built as well</param>
        public static void BuildLinksRefrence(List<Category> ParsedCategoryList, bool buildFakeParents)
        {
            foreach (Category cat in ParsedCategoryList)
            {
                if(buildFakeParents)
                {
                    cat.CategoryHeader = new SelectablePackage()
                    {
                        Name = string.Format("----------[{0}]----------", cat.Name),
                        TabIndex = cat.TabPage,
                        ParentCategory = cat,
                        Type = SelectionTypes.multi,
                        Visible = true,
                        Enabled = true,
                        Level = -1,
                        PackageName = string.Format("Category_{0}_Header", cat.Name.Replace(' ', '_')),
                        Packages = cat.Packages
                    };
                }
                foreach (SelectablePackage sp in cat.Packages)
                {
                    BuildLinksRefrence(sp, cat, cat.CategoryHeader);
                }
            }
        }

        /// <summary>
        /// Links all the references (like parent, etc) for each class object making it possible to traverse the list tree in memory
        /// </summary>
        /// <param name="sp">The package to perform linking on</param>
        /// <param name="cat">The category that the SelectablePackagesp belongs to</param>
        /// <param name="parent">The tree parent of sp</param>
        private static void BuildLinksRefrence(SelectablePackage sp, Category cat, SelectablePackage parent)
        {
            sp.Parent = parent;
            sp.TopParent = cat.CategoryHeader;
            sp.ParentCategory = cat;
            if (sp.Packages.Count > 0)
            {
                foreach (SelectablePackage sp2 in sp.Packages)
                {
                    BuildLinksRefrence(sp2, cat, sp);
                }
            }
        }

        /// <summary>
        /// Assigns the level parameter to the packages based on how recursively deep they are in the package sub lists
        /// </summary>
        /// <param name="ParsedCategoryList">The list of assign package values to</param>
        /// <param name="startingLevel">The starting level to assign the level parameter</param>
        public static void BuildLevelPerPackage(List<Category> ParsedCategoryList, int startingLevel = 0)
        {
            //root level direct form category is 0
            foreach (Category cat in ParsedCategoryList)
            {
                foreach (SelectablePackage package in cat.Packages)
                {
                    package.Level = startingLevel;
                    if (package.Packages.Count > 0)
                        //increase the level BEFORE it calls the method
                        BuildLevelPerPackage(package.Packages, startingLevel+ 1);
                }
            }
        }

        /// <summary>
        /// Assigns the level parameter to the packages based on how recursively deep they are in the package sub lists
        /// </summary>
        /// <param name="packages">The list of package values to</param>
        /// <param name="level">The level to assign the level parameter</param>
        private static void BuildLevelPerPackage(List<SelectablePackage> packages, int level)
        {
            foreach (SelectablePackage package in packages)
            {
                package.Level = level;
                if (package.Packages.Count > 0)
                    //increase the level BEFORE it calls the method
                    BuildLevelPerPackage(package.Packages, level+1);
            }
        }

        /// <summary>
        /// Links the databasePackage objects with dependencies objects to have those objects link references to the parent and the dependency object
        /// </summary>
        /// <param name="componentsWithDependencies">List of all DatabasePackage objects that have dependencies</param>
        /// <param name="dependencies">List of all Dependencies that exist in the database</param>
        public static void BuildDependencyPackageRefrences(List<Category> componentsWithDependencies, List<Dependency> dependencies)
        {
            List<IComponentWithDependencies> componentsWithDependencies_ = new List<IComponentWithDependencies>();

            //get all categories where at least one dependency exists
            componentsWithDependencies_.AddRange(componentsWithDependencies.Where(cat => cat.Dependencies.Count > 0));

            //get all packages and dependencies where at least one dependency exists
            componentsWithDependencies_.AddRange(GetFlatList(null, dependencies, null, componentsWithDependencies).OfType<IComponentWithDependencies>().Where(component => component.Dependencies.Count > 0).ToList());

            foreach (IComponentWithDependencies componentWithDependencies in componentsWithDependencies_)
            {
                foreach(DatabaseLogic logic in componentWithDependencies.Dependencies)
                {
                    logic.ParentPackageRefrence = componentWithDependencies;
                    logic.DependencyPackageRefrence = dependencies.Find(dependency => dependency.PackageName.Equals(logic.PackageName));
                    if(logic.DependencyPackageRefrence == null)
                    {
                        Logging.Error("DatabaseLogic component from package {0} was unable to link to dependency {1} (does the dependency not exist or bad reference?)", componentWithDependencies.ComponentInternalName, logic.PackageName);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates which packages and dependencies are dependent on other dependencies and if each dependency that is selected for install is enabled for installation
        /// </summary>
        /// <param name="dependencies">The list of dependencies</param>
        /// <param name="parsedCategoryList">The list of Categories</param>
        /// <param name="suppressSomeLogging">Flag for it some of the more verbose logging should be suppressed</param>
        /// <returns>A list of calculated dependencies to install</returns>
        public static List<Dependency> CalculateDependencies(List<Dependency> dependencies, List<Category> parsedCategoryList, bool suppressSomeLogging)
        {
            //flat list is packages
            List<SelectablePackage> flatListSelect = GetFlatSelectablePackageList(parsedCategoryList);

            //1- build the list of calling mods that need it
            List<Dependency> dependenciesToInstall = new List<Dependency>();

            //create list to track all database dependency references
            List<LogicTracking> refrencedDependencies = new List<LogicTracking>();

            Logging.Debug("Starting step 1 of 4 in dependency calculation: adding from categories");
            foreach (Category category in parsedCategoryList)
            {
                foreach (DatabaseLogic logic in category.Dependencies)
                {
                    refrencedDependencies.Add(new LogicTracking
                    {
                        DatabaseLogic = logic,
                        ComponentWithDependencies = category
                    });
                    foreach (Dependency dependency in dependencies)
                    {
                        if (logic.PackageName.Equals(dependency.PackageName))
                        {
                            if(!suppressSomeLogging)
                                Logging.Debug("Category \"{0}\" logic entry added to dependency \"{1}\" of logic type \"{2}\", NotFlag value of \"{3}\"",
                                category.Name, dependency.PackageName, logic.Logic, logic.NotFlag);
                            dependency.DatabasePackageLogic.Add(new DatabaseLogic()
                            {
                                PackageName = category.Name,
                                WillBeInstalled = category.AnyPackagesChecked(),
                                Logic = logic.Logic,
                                NotFlag = logic.NotFlag
                            });

                            //log that the categories dependency reference was linked properly
                            logic.RefrenceLinked = true;
                        }
                    }
                }
            }
            Logging.Debug("Step 1 complete");

            Logging.Debug("Starting step 2 of 4 in dependency calculation: adding from selectable packages that use each dependency");
            foreach(SelectablePackage package in flatListSelect)
            {
                //got though each logic property. if the package called is this dependency, then add it to it's list
                foreach (DatabaseLogic logic in package.Dependencies)
                {
                    refrencedDependencies.Add(new LogicTracking
                    {
                        DatabaseLogic = logic,
                        ComponentWithDependencies = package
                    });
                    foreach (Dependency dependency in dependencies)
                    {
                        if (logic.PackageName.Equals(dependency.PackageName))
                        {
                            if (!suppressSomeLogging)
                                Logging.Debug("SelectablePackage \"{0}\" logic entry added to dependency \"{1}\" of logic type \"{2}\", NotFlag value of \"{3}\"",
                                package.PackageName, dependency.PackageName, logic.Logic, logic.NotFlag);
                            dependency.DatabasePackageLogic.Add(new DatabaseLogic()
                            {
                                //set PackageName to the selectablepackage package name so later we know where this logic entry came from
                                PackageName = package.PackageName,
                                WillBeInstalled = package.Checked,
                                Logic = logic.Logic,
                                NotFlag = logic.NotFlag
                            });

                            //log that the categories dependency reference was linked properly
                            logic.RefrenceLinked = true;
                        }
                    }
                }
            }
            Logging.Debug("Step 2 complete");


            Logging.Debug("Starting step 3 of 4 in dependency calculation: adding dependencies that use each dependency");
            //for each dependency go through each dependency's package logic and if it's called then add it
            foreach(Dependency processingDependency in dependencies)
            {
                foreach (DatabaseLogic logic in processingDependency.Dependencies)
                {
                    refrencedDependencies.Add(new LogicTracking
                    {
                        DatabaseLogic = logic,
                        ComponentWithDependencies = processingDependency
                    });
                    foreach (Dependency dependency in dependencies)
                    {
                        if (processingDependency.PackageName.Equals(dependency.PackageName))
                            continue;
                        if (logic.PackageName.Equals(dependency.PackageName))
                        {
                            if (!suppressSomeLogging)
                                Logging.Debug("Dependency \"{0}\" logic entry added to dependency \"{1}\" of logic type \"{2}\", NotFlag value of \"{3}\"",
                                processingDependency.PackageName, dependency.PackageName, logic.Logic, logic.NotFlag);
                            dependency.DatabasePackageLogic.Add(new DatabaseLogic()
                            {
                                PackageName = processingDependency.PackageName,
                                //by default, dependences that are dependent on dependencies start as false until proven needed
                                WillBeInstalled = false,
                                Logic = logic.Logic,
                                NotFlag = logic.NotFlag
                            });

                            //log that the categories dependency reference was linked properly
                            logic.RefrenceLinked = true;
                        }
                    }
                }
            }
            Logging.Debug("Step 3 complete");

            //3a - check if any dependency references were never matched
            //like if a category references dependency the_dependency_packageName, but that package does not exist
            refrencedDependencies = refrencedDependencies.Where((refrence) => !refrence.DatabaseLogic.RefrenceLinked).ToList();
            Logging.Debug("Broken dependency references count: {0}", refrencedDependencies.Count);
            if(refrencedDependencies.Count > 0)
            {
                Logging.Error("The following packages call references to dependencies that do not exist:");
                foreach(LogicTracking logicTracking in refrencedDependencies)
                {
                    Logging.Error("Package: {0} => broken reference: {1}",
                        logicTracking.ComponentWithDependencies.ComponentInternalName, logicTracking.DatabaseLogic.PackageName);
                }
            }

            //4 - run calculations IN DEPENDENCY LIST ORDER FROM TOP DOWN
            List<Dependency> notProcessedDependnecies = new List<Dependency>(dependencies);
            Logging.Debug("Starting step 4 of 4 in dependency calculation: calculating dependencies from top down (perspective to list)");
            int calcNumber = 1;
            foreach (Dependency dependency in dependencies)
            {
                //first check if this dependency is referencing a dependency that has not yet been processed
                //if so then note it in the log
                if (!suppressSomeLogging)
                    Logging.Debug(string.Empty);
                if (!suppressSomeLogging)
                    Logging.Debug("Calculating if dependency {0} will be installed, {1} of {2}", dependency.PackageName, calcNumber++, dependencies.Count);

                foreach(DatabaseLogic login in dependency.DatabasePackageLogic)
                {
                    List<Dependency> matches = notProcessedDependnecies.Where(dep => login.PackageName.Equals(dep.PackageName)).ToList();
                    if(matches.Count > 0)
                    {
                        string errorMessage = string.Format("Dependency {0} is referenced by the dependency {1} which has not yet been processed! " +
                            "This will lead to logic errors in database calculation! Tip: this dependency ({0}) should be BELOW ({1}) in the" +
                            "list of dependencies in the editor. Order matters!",dependency.PackageName, login.PackageName);
                        Logging.Error(errorMessage);
                        if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                            MessageBox.Show(errorMessage);
                    }
                }

                //two types of logics - OR and AND (with NOT flags)
                //each can be calculated separately
                List<DatabaseLogic> localOR = dependency.DatabasePackageLogic.Where(logic => logic.Logic == Logic.OR).ToList();
                List<DatabaseLogic> logicalAND = dependency.DatabasePackageLogic.Where(logic => logic.Logic == Logic.AND).ToList();

                //debug logging
                if (!suppressSomeLogging)
                    Logging.Debug("Logical OR count: {0}", localOR.Count);
                if (!suppressSomeLogging)
                    Logging.Debug("Logical AND count: {0}", logicalAND.Count);

                //if there are no logical ands, then only do ors, vise versa
                bool ORsPass = localOR.Count > 0? false: true;
                bool ANDSPass = logicalAND.Count > 0? false:true;

                //if ors and ands are both true already, then something's broken
                if(ORsPass && ANDSPass)
                {
                    Logging.Warning("Logic ORs and ANDs already pass for dependency package {0} (nothing uses it?)", dependency.PackageName);
                    if (!suppressSomeLogging)
                        Logging.Debug("Skip calculation logic and remove from not processed list");

                    //remove it from list of not processed dependencies
                    notProcessedDependnecies.RemoveAt(0);
                    continue;
                }

                //calc the ORs first
                if (!suppressSomeLogging)
                    Logging.Debug("Processing OR logic");
                foreach(DatabaseLogic orLogic in localOR)
                {
                    //OR logic - if any mod/dependency is checked, then it's installed and can stop there
                    //because only one of them needs to be true
                    //same case goes for negatives - if mod is NOT checked and negateFlag
                    if (!orLogic.WillBeInstalled)
                    {
                        if (!suppressSomeLogging)
                            Logging.Debug("Skipping logic check of package {0} because it is not set for installation!", orLogic.PackageName);
                        continue;
                    }
                    else
                    {
                        if (!orLogic.NotFlag)
                        {
                            if (!suppressSomeLogging)
                                Logging.Debug("Package {0}, checked={1}, notFlag={2}, is checked and notFlag is false (package must be checked), sets orLogic to pass!", orLogic.PackageName, orLogic.WillBeInstalled, orLogic.NotFlag);
                            ORsPass = true;
                            break;
                        }
                        else if (orLogic.NotFlag)
                        {
                            if (!suppressSomeLogging)
                                Logging.Debug("Package {0}, checked={1}, notFlag={2}, is NOT checked and notFlag is true (package must NOT be checked), sets orLogic to pass!", orLogic.PackageName, orLogic.WillBeInstalled, orLogic.NotFlag);
                            ORsPass = true;
                            break;
                        }
                        else
                        {
                            if (!suppressSomeLogging)
                                Logging.Debug("Package {0}, checked={1}, notFlag={2}, does not set orLogic to pass!", orLogic.PackageName, orLogic.WillBeInstalled, orLogic.NotFlag);
                        }
                    }
                }

                //now calc the ands
                if (!suppressSomeLogging)
                    Logging.Debug("Processing AND logic");
                foreach(DatabaseLogic andLogic in logicalAND)
                {
                    if (andLogic.WillBeInstalled && !andLogic.NotFlag)
                    {
                        if (!suppressSomeLogging)
                            Logging.Debug("Package {0}, checked={1}, notFlag={2}, is checked and notFlag is false (package must be checked), correct AND logic, continue", andLogic.PackageName, andLogic.WillBeInstalled, andLogic.NotFlag);
                        ANDSPass = true;
                    }
                    else if (!andLogic.WillBeInstalled && andLogic.NotFlag)
                    {
                        if (!suppressSomeLogging)
                            Logging.Debug("Package {0}, checked={1}, notFlag={2}, is NOT checked and notFlag is true (package must NOT be checked), correct AND logic, continue", andLogic.PackageName, andLogic.WillBeInstalled, andLogic.NotFlag);
                        ANDSPass = true;
                    }
                    else
                    {
                        if (!suppressSomeLogging)
                            Logging.Debug("Package {0}, checked={1}, notFlag={2}, incorrect AND logic, set ANDSPass=false and stop processing!", andLogic.PackageName, andLogic.WillBeInstalled, andLogic.NotFlag);
                        ANDSPass = false;
                        break;
                    }
                }

                string final = string.Format("Final result for dependency {0}: AND={1}, OR={2}", dependency.PackageName, ANDSPass, ORsPass);
                if(ANDSPass && ORsPass)
                {
                    if (suppressSomeLogging)
                        Logging.Info(LogOptions.MethodAndClassName, "Dependency {0} WILL be installed!", dependency.PackageName);
                    else
                        Logging.Debug("{0} (AND and OR) = TRUE, dependency WILL be installed!", final);
                    dependenciesToInstall.Add(dependency);
                }
                else
                {
                    if (!suppressSomeLogging)
                        Logging.Debug("{0} (AND and OR) = FALSE, dependency WILL NOT be installed!", final);
                }

                if (dependency.DatabasePackageLogic.Count > 0 && (ANDSPass && ORsPass))
                {
                    if (!suppressSomeLogging)
                        Logging.Debug("Updating future references (like logicalDependnecies) for if dependency was checked");
                    //update any dependencies that use it
                    foreach (DatabaseLogic callingLogic in dependency.Dependencies)
                    {
                        //get the dependency (if it is a dependency) that called this dependency
                        List<Dependency> found = dependencies.Where(dep => dep.PackageName.Equals(callingLogic.PackageName)).ToList();

                        if (found.Count > 0)
                        {
                            Dependency refrenced = found[0];
                            //now get the logic entry that references the original calculated dependency
                            List<DatabaseLogic> foundLogic = refrenced.DatabasePackageLogic.Where(logic => logic.PackageName.Equals(dependency.PackageName)).ToList();
                            if (foundLogic.Count > 0)
                            {
                                Logging.Debug("Logic reference entry for dependency {0} updated to {1}", refrenced.PackageName, ANDSPass && ORsPass);
                                foundLogic[0].WillBeInstalled = ANDSPass && ORsPass;
                            }
                            else
                            {
                                Logging.Error("Found logics count is 0 for updating references");
                            }
                        }
                        else
                        {
                            Logging.Error("Found count is 0 for updating references");
                        }
                    }
                }

                //remove it from list of not processed dependencies
                notProcessedDependnecies.RemoveAt(0);
            }

            Logging.Debug("Step 4 complete");
            return dependenciesToInstall;
        }

        /// <summary>
        /// Creates an array of DatabasePackage lists sorted by Installation groups i.e. list in array index 0 is packages of install group 0
        /// </summary>
        /// <param name="packagesToInstall"></param>
        /// <returns>The array of DatabasePackage lists</returns>
        public static List<DatabasePackage>[] CreateOrderedInstallList(List<DatabasePackage> packagesToInstall)
        {
            //get the max number of defined groups
            int maxGrops = packagesToInstall.Select(max => max.InstallGroupWithOffset).Max();

            //make the list to return
            //make it maxGroups +1 because group 4 exists, but making a array of 4 is 0-3
            List<DatabasePackage>[] orderedList = new List<DatabasePackage>[maxGrops+1];

            //new up the lists
            for (int i = 0; i < orderedList.Count(); i++)
                orderedList[i] = new List<DatabasePackage>();

            foreach(DatabasePackage package in packagesToInstall)
            {
                orderedList[package.InstallGroupWithOffset].Add(package);
            }
            return orderedList;
        }

        /// <summary>
        /// Clears all selections in the given lists by setting the checked properties to false
        /// </summary>
        /// <param name="ParsedCategoryList">The list of Categories</param>
        public static void ClearSelections(List<Category> ParsedCategoryList)
        {
            foreach (SelectablePackage package in GetFlatList(null, null, null, ParsedCategoryList))
            {
                if(ModpackSettings.SaveDisabledMods && package.FlagForSelectionSave)
                {
                    Logging.Debug("SaveDisabledMods=True and package {0} FlagForSelectionSave is high, setting to low", package.Name);
                    package.FlagForSelectionSave = false;
                }
                package.Checked = false;
            }
            foreach (Category category in ParsedCategoryList)
                if (category.CategoryHeader != null && category.CategoryHeader.Checked)
                    category.CategoryHeader.Checked = false;
        }

        /// <summary>
        /// Gets the maximum InstallGroup number from a list of Packages
        /// </summary>
        /// <param name="listToCheck">The list of DatabasePackages</param>
        /// <returns>The maximum InstallGroup number</returns>
        public static int GetMaxInstallGroupNumber(List<DatabasePackage> listToCheck)
        {
            return listToCheck.Max(ma => ma.InstallGroup);
        }

        /// <summary>
        /// Gets the maximum InstallGroup number from a list of Packages factoring in the offset that a category may apply to it
        /// </summary>
        /// <param name="listToCheck">The list of DatabasePackages</param>
        /// <returns>The maximum InstallGroup number</returns>
        public static int GetMaxInstallGroupNumberWithOffset(List<DatabasePackage> listToCheck)
        {
            return listToCheck.Max(ma => ma.InstallGroupWithOffset);
        }

        /// <summary>
        /// Gets the maximum PatchGroup number from a list of Packages
        /// </summary>
        /// <param name="listToCheck">The list of DatabasePackages</param>
        /// <returns>The maximum PatchGroup number</returns>
        public static int GetMaxPatchGroupNumber(List<DatabasePackage> listToCheck)
        {
            return listToCheck.Max(ma => ma.PatchGroup);
        }

        /// <summary>
        /// Attempts to set a property value of a class or structure object instance with the string valueToSet
        /// </summary>
        /// <param name="componentWithProperty">The class or structure object instance to have property set</param>
        /// <param name="propertyInfoFromComponent">The property information/metadata of the property to set on the object</param>
        /// <param name="valueToSet">The string version of the value to set</param>
        /// <returns>False if the value could not be set, true otherwise</returns>
        public static bool SetObjectProperty(object componentWithProperty, PropertyInfo propertyInfoFromComponent, string valueToSet)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(propertyInfoFromComponent.PropertyType);
                propertyInfoFromComponent.SetValue(componentWithProperty, converter.ConvertFrom(valueToSet));
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
                var converter = TypeDescriptor.GetConverter(objectType);
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
        /// <param name="componentWithID">The database component with the list property, for example SelectablePackage</param>
        /// <param name="listPropertyInfo">the property metadata/info about the list property, for example Medias</param>
        /// <param name="xmlListItems">The xml element holder for the property object types, for example Medias element holder</param>
        public static void SetListEntries(IDatabaseComponent componentWithID, PropertyInfo listPropertyInfo, IEnumerable<XElement> xmlListItems)
        {
            //get the list interfaced component
            IList listProperty = listPropertyInfo.GetValue(componentWithID) as IList;

            //we now have the empty list, now get type of list it is
            //https://stackoverflow.com/questions/34211815/how-to-get-the-underlying-type-of-an-ilist-item
            Type listObjectType = listProperty.GetType().GetInterfaces().Where(i => i.IsGenericType && i.GenericTypeArguments.Length == 1)
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
                //if it's just like a string or something then just load that
                if(listObjectType.IsValueType)
                {
                    if(SetObjectValue(listObjectType,listElement.Value,out object newObject))
                    {
                        listProperty.Add(newObject);
                        continue;
                    }
                }

                //make sure object type is properly implemented into serialization system
                if (!(Activator.CreateInstance(listObjectType) is IXmlSerializable listEntry))
                    throw new BadMemeException("Type of this list is not of IXmlSerializable");

                //assign missing attributes if not done already
                if (missingAttributes == null)
                    missingAttributes = new List<string>(listEntry.PropertiesForSerializationAttributes());

                foreach (XAttribute listEntryAttribute in listElement.Attributes())
                {
                    if (!listEntry.PropertiesForSerializationAttributes().Contains(listEntryAttribute.Name.LocalName))
                    {
                        unknownAttributes.Add(listEntryAttribute.Name.LocalName);
                        continue;
                    }

                    PropertyInfo property = listObjectType.GetProperty(listEntryAttribute.Name.LocalName);

                    //check if attribute exists in class object
                    if(property == null)
                    {
                        Logging.Error("Property (xml attribute) {0} exists in array for serialization, but not in class design!, ", listEntryAttribute.Name.LocalName);
                        Logging.Error("Package: {0}, line: {1}", componentWithID.ComponentInternalName, ((IXmlLineInfo)listElement).LineNumber);
                        continue;
                    }

                    //remove from list of potential missing mandatory elements
                    missingAttributes.Remove(listEntryAttribute.Name.LocalName);

                    if(!SetObjectProperty(listEntry, property, listEntryAttribute.Value))
                    {
                        Logging.Error("Failed to set property {0} for element in IList", property.Name);
                        Logging.Error("Package: {0}, line: {1}", componentWithID.ComponentInternalName, ((IXmlLineInfo)listElement).LineNumber);
                    }
                }

                foreach(XElement listEntryElement in listElement.Elements())
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
                        Logging.Error("Package: {0}, line: {1}", componentWithID.ComponentInternalName, ((IXmlLineInfo)listEntryElement).LineNumber);
                        continue;
                    }

                    //no missing elements (elements are optional)

                    if (!SetObjectProperty(listEntry, property, listEntryElement.Value))
                    {
                        Logging.Error("Failed to set property {0} for element in IList", property.Name);
                        Logging.Error("Package: {0}, line: {1}", componentWithID.ComponentInternalName, ((IXmlLineInfo)listEntryElement).LineNumber);
                    }
                }

                //logging unknown and missings
                foreach (string missingAttribute in missingAttributes)
                {
                    Logging.Error("Missing xml attribute: {0}, package: {1}, line: {2}", missingAttribute, componentWithID.ComponentInternalName, ((IXmlLineInfo)listElement).LineNumber);
                }
                foreach (string unknownAttribute in unknownAttributes)
                {
                    Logging.Error("Missing xml attribute: {0}, package: {1}, line: {2}", unknownAttribute, componentWithID.ComponentInternalName, ((IXmlLineInfo)listElement).LineNumber);
                }
                foreach (string unknownElement in unknownElements)
                {
                    Logging.Error("Unknown xml element: {0}, package: {1}, line: {2}", unknownElement, componentWithID.ComponentInternalName, ((IXmlLineInfo)listElement).LineNumber);
                }

                listProperty.Add(listEntry);
            }
        }

        public static List<DatabaseLogic> GetAllPackageDependencies(SelectablePackage package)
        {
            List<DatabaseLogic> dependencies = new List<DatabaseLogic>();

            if (package.Dependencies.Count > 0)
            {
                foreach (DatabaseLogic logic in package.Dependencies)
                {
                    dependencies.Add(logic);
                    Dependency dep = logic.DependencyPackageRefrence as Dependency;
                    if (dep.Dependencies.Count > 0)
                        GetAllPackageDependencies(dep, dependencies);
                }
            }

            if (package.ParentCategory.Dependencies.Count > 0)
            {
                foreach (DatabaseLogic logic in package.ParentCategory.Dependencies)
                {
                    if(!dependencies.Contains(logic))
                        dependencies.Add(logic);
                    Dependency dep = logic.DependencyPackageRefrence as Dependency;
                    if (dep.Dependencies.Count > 0)
                        GetAllPackageDependencies(dep, dependencies);
                }
            }

            return dependencies;
        }

        private static void GetAllPackageDependencies(Dependency dependency, List<DatabaseLogic> dependencies)
        {
            if (dependency.Dependencies.Count == 0)
                return;

            foreach(DatabaseLogic logic in dependency.Dependencies)
            {
                dependencies.Add(logic);
                Dependency dep = logic.DependencyPackageRefrence as Dependency;
                if (dep.Dependencies.Count != 0)
                    GetAllPackageDependencies(dep, dependencies);
            }
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

                string processStartFilepathCorrected = RemoveWoT32bit64bitPathIfExists(processStartFilepath);
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

        #region Kernel import p/invoke stuff
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
        #endregion

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
        /// Returns a flat list of the given recursive lists, in the order that the parameters are stated
        /// </summary>
        /// <param name="globalDependnecies">The list of global dependences</param>
        /// <param name="dependencies">The list of dependencies</param>
        /// <param name="logicalDependencies">The list of logical dependencies</param>
        /// <param name="parsedCategoryList">The list of Categories</param>
        /// <returns>The flat list</returns>
        /// <remarks>In the case of Categories, the flat list has the sub-level packages added at the level of the parent</remarks>
        public static List<DatabasePackage> GetFlatList(List<DatabasePackage> globalDependnecies = null, List<Dependency> dependencies = null,
            List<Dependency> logicalDependencies = null, List<Category> parsedCategoryList = null)
        {
            if (globalDependnecies == null && dependencies == null && logicalDependencies == null && parsedCategoryList == null)
                return null;
            List<DatabasePackage> flatList = new List<DatabasePackage>();
            if (globalDependnecies != null)
                flatList.AddRange(globalDependnecies);
            if (dependencies != null)
                flatList.AddRange(dependencies);
            if (logicalDependencies != null)
                flatList.AddRange(logicalDependencies);
            if (parsedCategoryList != null)
                foreach (Category cat in parsedCategoryList)
                    flatList.AddRange(cat.GetFlatPackageList());
            return flatList;
        }

        /// <summary>
        /// Returns a flat list of the given recursive lists, in the order that the parameters are stated
        /// </summary>
        /// <param name="parsedCategoryList">The list of Categories</param>
        /// <returns>The flat list</returns>
        /// <remarks>In the case of Categories, the flat list has the sub-level packages added at the level of the parent</remarks>
        public static List<SelectablePackage> GetFlatSelectablePackageList(List<Category> parsedCategoryList)
        {
            if (parsedCategoryList == null)
                return null;
            List<SelectablePackage> flatList = new List<SelectablePackage>();
            foreach (Category cat in parsedCategoryList)
                flatList.AddRange(cat.GetFlatPackageList());
            return flatList;
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

        #region Macro Utils
        /// <summary>
        /// Builds the Filepath macro dictionary with settings that should be parsed from the Settings class
        /// </summary>
        public static void BuildFilepathMacroList()
        {
            if (FilePathDict == null)
                throw new BadMemeException("REEEEEEEEEE");
            FilePathDict.Clear();
            //add macro versions first then regular versions
            FilePathDict.Add(@"{versiondir}", Settings.WoTClientVersion);
            FilePathDict.Add(@"{tanksversion}", Settings.WoTClientVersion);
            FilePathDict.Add(@"{tanksonlinefolderversion}", Settings.WoTModpackOnlineFolderVersion);
            FilePathDict.Add(@"{appdata}", Settings.AppDataFolder);
            FilePathDict.Add(@"{appData}", Settings.AppDataFolder);
            FilePathDict.Add(@"{app}", Settings.WoTDirectory);
            FilePathDict.Add(@"versiondir", Settings.WoTClientVersion);
        }

        /// <summary>
        /// Performs a replacement of macros using the specified macro replace operation
        /// </summary>
        /// <param name="inputString">The string to replace the macros of</param>
        /// <param name="type">The type of macro replace operation</param>
        /// <returns>The replaced string</returns>
        public static string MacroReplace(string inputString, ReplacementTypes type)
        {
            //itterate through each entry depending on the dictionary. if the key is contained in the string, replace it
            //use a switch to get which dictionary reaplce we will use
            Dictionary<string, string> dictionary = null;
            switch (type)
            {
                case ReplacementTypes.FilePath:
                    dictionary = FilePathDict;
                    break;
                case ReplacementTypes.PatchArguementsReplace:
                    dictionary = PatchArguementsReplaceDict;
                    break;
                case ReplacementTypes.PatchFiles:
                    dictionary = PatchFilesDict;
                    break;
                case ReplacementTypes.TextEscape:
                    dictionary = TextEscapeDict;
                    break;
                case ReplacementTypes.TextUnescape:
                    dictionary = TextUnscapeDict;
                    break;
            }
            if (dictionary == null)
            {
                Logging.Error("macro replace dictionary is null! type={0}", type.ToString());
                return inputString;
            }
            for (int i = 0; i < dictionary.Count; i++)
            {
                string key = dictionary.ElementAt(i).Key;
                string replace = dictionary.ElementAt(i).Value;
                //https://stackoverflow.com/questions/444798/case-insensitive-containsstring
                //it's an option, not actually used here cause it would be a lot of work to implement
                //could also try regex, may be easlier to ignore case, but then might have to make it an option
                //so for now, no
                if (inputString.Contains(key))
                    inputString = inputString.Replace(key, replace);
            }
            return inputString;
        }
        #endregion

        #region Tanks Install Auto/Manuel Search Code
        /// <summary>
        /// Checks the registry to get the latest location of where WoT is installed
        /// </summary>
        /// <returns>True if operation success</returns>
        public static string AutoFindWoTDirectory()
        {
            List<string> searchPathWoT = new List<string>();
            RegistryKey result = null;

            //check replay link locations (the last game instance the user opened)
            //key is null, value is path
            RegistrySearch[] registryEntriesGroup1 = new RegistrySearch[]
            {
                new RegistrySearch(){Root = Registry.LocalMachine, Searchpath = @"SOFTWARE\Classes\.wotreplay\shell\open\command"},
                new RegistrySearch(){Root = Registry.CurrentUser, Searchpath = @"Software\Classes\.wotreplay\shell\open\command"}
            };

            foreach (RegistrySearch searchPath in registryEntriesGroup1)
            {
                Logging.Debug("Searching in registry root {0} with path {1}", searchPath.Root.Name, searchPath.Searchpath);
                result = GetRegistryKeys(searchPath);
                if (result != null)
                {
                    foreach (string valueInKey in result.GetValueNames())
                    {
                        string possiblePath = result.GetValue(valueInKey) as string;
                        if (!string.IsNullOrWhiteSpace(possiblePath) && possiblePath.ToLower().Contains("worldoftanks.exe"))
                        {
                            //trim front
                            possiblePath = possiblePath.Substring(1);
                            //trim end
                            possiblePath = possiblePath.Substring(0, possiblePath.Length - 6);
                            Logging.Debug("Possible path found: {0}", possiblePath);
                            searchPathWoT.Add(possiblePath);
                        }
                    }
                    result.Dispose();
                    result = null;
                }
            }

            //key is WoT path, don't care about value
            RegistrySearch[] registryEntriesGroup2 = new RegistrySearch[]
            {
                new RegistrySearch(){Root = Registry.CurrentUser, Searchpath = @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache"},
                new RegistrySearch(){Root = Registry.CurrentUser, Searchpath = @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Compatibility Assistant\Store"}
            };

            foreach (RegistrySearch searchPath in registryEntriesGroup2)
            {
                Logging.Debug("Searching in registry root {0} with path {1}", searchPath.Root.Name, searchPath.Searchpath);
                result = GetRegistryKeys(searchPath);
                if (result != null)
                {
                    foreach(string possiblePath in result.GetValueNames())
                    {
                        if(!string.IsNullOrWhiteSpace(possiblePath) && possiblePath.ToLower().Contains("worldoftanks.exe"))
                        {
                            Logging.Debug("Possible path found: {0}", possiblePath);
                            searchPathWoT.Add(possiblePath);
                        }
                    }
                    result.Dispose();
                    result = null;
                }
            }

            foreach (string path in searchPathWoT)
            {
                string potentialResult = path;
                //if it has win32 or win64, filter it out
                if (potentialResult.Contains(Settings.WoT32bitFolderWithSlash) || potentialResult.Contains(Settings.WoT64bitFolderWithSlash))
                {
                    potentialResult = potentialResult.Replace(Settings.WoT32bitFolderWithSlash, string.Empty).Replace(Settings.WoT64bitFolderWithSlash, string.Empty);
                }
                if (File.Exists(potentialResult))
                {
                    Logging.Info("Valid game path found: {0}", potentialResult);
                    return potentialResult;
                }
            }
            //return false if nothing found
            return null;
        }

        /// <summary>
        /// Gets all registry keys that exist in the given search base and path
        /// </summary>
        /// <param name="search">The RegistrySearch structure to specify where to search and where to base the search</param>
        /// <returns>The RegistryKey object of the folder in registry, or null if the search failed</returns>
        public static RegistryKey GetRegistryKeys(RegistrySearch search)
        {
            try
            {
                return search.Root.OpenSubKey(search.Searchpath);
            }
            catch (Exception ex)
            {
                Logging.Exception("Failed to get registry entry");
                Logging.Exception(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Finds the location of the Wargaming Game center installation directory from the registry
        /// </summary>
        /// <returns>The location of wgc.exe if found, else null</returns>
        public static string AutoFindWgcDirectory()
        {
            string wgcRegistryKeyLoc = @"Software\Classes\wgc\shell\open\command";
            Logging.Debug("Searching registry ({0}) for wgc location",wgcRegistryKeyLoc);
            //search for the location of the game center from the registry
            RegistryKey wgcKey = GetRegistryKeys(new RegistrySearch() { Root = Registry.CurrentUser, Searchpath = wgcRegistryKeyLoc });
            string actualLocation = null;
            if (wgcKey != null)
            {
                Logging.Debug("Not null key, checking results");
                foreach (string valueInKey in wgcKey.GetValueNames())
                {
                    string wgcPath = wgcKey.GetValue(valueInKey) as string;
                    Logging.Debug("Parsing result name '{0}' with value '{1}'", valueInKey, wgcPath);
                    if (!string.IsNullOrWhiteSpace(wgcPath) && wgcPath.ToLower().Contains("wgc.exe"))
                    {
                        //trim front
                        wgcPath = wgcPath.Substring(1);
                        //trim end
                        wgcPath = wgcPath.Substring(0, wgcPath.Length - 6);
                        Logging.Debug("Parsed to new value of '{0}', checking if file exists");
                        if (File.Exists(wgcPath))
                        {
                            Logging.Debug("Exists, use this for wgc start");
                            actualLocation = wgcPath;
                            break;
                        }
                        else
                        {
                            Logging.Debug("Not exist, continue to search");
                        }
                    }
                }
            }
            return actualLocation;
        }
        #endregion

        #region Install Utils
        /// <summary>
        /// Creates a shortcut on the user's desktop
        /// </summary>
        /// <param name="shortcut">The shortcut parameters</param>
        /// <param name="sb">The StringBuilder to log the path to the created file</param>
        public static void CreateShortcut(Shortcut shortcut, StringBuilder sb)
        {
            Logging.Info(shortcut.ToString());
            Logging.Info("Creating shortcut {0}",shortcut.Name);
            //build the full macro for path (target) and name (also filename)
            string target = MacroReplace(shortcut.Path, ReplacementTypes.FilePath).Replace(@"/",@"\");
            string filename = string.Format("{0}.lnk", shortcut.Name);
            string shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            shortcutPath = Path.Combine(shortcutPath, filename);
            Logging.Info("target={0}", target);
            Logging.Info("shortcutPath={0}", shortcutPath);
            if(!File.Exists(target))
            {
                Logging.Warning("target does not exist, skipping shortcut", target);
                return;
            }
            if(File.Exists(shortcutPath))
            {
                Logging.Debug("shortcut path exists, checking if update needed");
                WshShell shell = new WshShell();
                IWshShortcut link = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                Logging.Debug("new target = {0}, old target = {1}", target, link.TargetPath);
                if(!target.Equals(link.TargetPath))
                {
                    //needs update
                    Logging.Debug("updating target");
                    link.TargetPath = target;
                    link.Save();
                }
                else
                {
                    //no update needed
                    Logging.Debug("no update needed");
                    return;
                }
            }
            else
            {
                Logging.Debug("shortcut path does not exist, creating");
                IShellLink link = (IShellLink)new ShellLink();
                // setup shortcut information
                link.SetDescription("created by the Relhax Manager");
                link.SetPath(target);
                link.SetIconLocation(target, 0);
                link.SetWorkingDirectory(Path.GetDirectoryName(target));
                //The arguments used when executing the exe (none used for now)
                link.SetArguments("");
                System.Runtime.InteropServices.ComTypes.IPersistFile file = (System.Runtime.InteropServices.ComTypes.IPersistFile)link;
                file.Save(shortcutPath, false);
            }
            //getting here means that the target is updated or created, so log it to the installer
            sb.AppendLine(shortcutPath);
        }

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
                Utils.DirectoryDelete(AppPathTempFolder, true);

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
                    if (!Utils.FileMove(Path.Combine(Settings.AppDataFolder, file), Path.Combine(AppPathTempFolder, file)))
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
                    Utils.DirectoryMove(Path.Combine(Settings.AppDataFolder, folder), Path.Combine(AppPathTempFolder, folder), true);
                }
                else
                {
                    Logging.Info("Folder does not exist in step clearCache: {0}", folder);
                }
            }

            //now delete the temp folder
            Logging.WriteToLog("Starting clearing cache step 2 of 3: actually clearing cache", Logfiles.Application, LogLevel.Debug);
            Utils.DirectoryDelete(Settings.AppDataFolder, true);

            //then put the above files back
            Logging.WriteToLog("Starting clearing cache step 3 of 3: restoring old files", Logfiles.Application, LogLevel.Debug);
            Directory.CreateDirectory(Settings.AppDataFolder);
            foreach (string file in fileNames)
            {
                Logging.WriteToLog("Processing cache file/folder to move: " + file, Logfiles.Application, LogLevel.Debug);
                if (File.Exists(Path.Combine(AppPathTempFolder, file)))
                {
                    if (!Utils.FileMove(Path.Combine(AppPathTempFolder, file), Path.Combine(Settings.AppDataFolder, file)))
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
                Utils.DirectoryDelete(Path.Combine(AppPathTempFolder, folderNames[0], xvmFolderToDelete), true);
            if (File.Exists(Path.Combine(AppPathTempFolder, folderNames[1], pmodCacheFileToDelete)))
                Utils.FileDelete(Path.Combine(AppPathTempFolder, folderNames[1], pmodCacheFileToDelete));

            foreach (string folder in folderNames)
            {
                if (Directory.Exists(Path.Combine(AppPathTempFolder, folder)))
                {
                    Utils.DirectoryMove(Path.Combine(AppPathTempFolder, folder), Path.Combine(Settings.AppDataFolder, folder), true);
                }
                else
                {
                    Logging.Info("Folder does not exist in step clearCache: {0}", folder);
                }
            }
            return true;
        }
        #endregion

        #region FTP methods
        /// <summary>
        /// Create an FTP folder
        /// </summary>
        /// <param name="addressWithDirectory">The complete path to the folder to create</param>
        /// <param name="credentials">The FTP server credentials</param>
        public static void FTPMakeFolder(string addressWithDirectory, ICredentials credentials)
        {
            WebRequest folderRequest = WebRequest.Create(addressWithDirectory);
            folderRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
            folderRequest.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse)folderRequest.GetResponse())
            { }
        }

        /// <summary>
        /// Create an FTP folder
        /// </summary>
        /// <param name="addressWithDirectory">The complete path to the folder to create</param>
        /// <param name="credentials">The FTP server credentials</param>
        public static async Task FTPMakeFolderAsync(string addressWithDirectory, ICredentials credentials)
        {
            WebRequest folderRequest = WebRequest.Create(addressWithDirectory);
            folderRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
            folderRequest.Credentials = credentials;
            using (FtpWebResponse webResponse = (FtpWebResponse)await folderRequest.GetResponseAsync())
            { }
        }

        /// <summary>
        /// Get a list of files currently in an FTP folder
        /// </summary>
        /// <param name="address">The complete path to the FTP folder</param>
        /// <param name="credentials">The FTP server credentials</param>
        /// <returns>The list of files on the server, as well as the current directory "." and parent directory ".." characters</returns>
        public static string[] FTPListFilesFolders(string address, ICredentials credentials)
        {
            WebRequest folderRequest = WebRequest.Create(address);
            folderRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            folderRequest.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse)folderRequest.GetResponse())
            {
                Stream responseStream = response.GetResponseStream();
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string temp = reader.ReadToEnd();
                    return temp.Split(new[] { "\r\n" }, StringSplitOptions.None);
                }
            }
        }

        /// <summary>
        /// Get a list of files currently in an FTP folder
        /// </summary>
        /// <param name="address">The complete path to the FTP folder</param>
        /// <param name="credentials">The FTP server credentials</param>
        /// <returns>The list of files on the server, as well as the current directory "." and parent directory ".." characters</returns>
        public static async Task<string[]> FTPListFilesFoldersAsync(string address, ICredentials credentials)
        {
            WebRequest folderRequest = WebRequest.Create(address);
            folderRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            folderRequest.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse)await folderRequest.GetResponseAsync())
            {
                Stream responseStream = response.GetResponseStream();
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string temp = reader.ReadToEnd();
                    return temp.Split(new[] { "\r\n" }, StringSplitOptions.None);
                }
            }
        }

        /// <summary>
        /// Delete a file on an FTP server
        /// </summary>
        /// <param name="address">The complete path to the FTP file to delete</param>
        /// <param name="credentials">The FTP server credentials</param>
        public static void FTPDeleteFile(string address, ICredentials credentials)
        {
            WebRequest folderRequest = WebRequest.Create(address);
            folderRequest.Method = WebRequestMethods.Ftp.DeleteFile;
            folderRequest.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse)folderRequest.GetResponse())
            { }
        }

        /// <summary>
        /// Delete a file on an FTP server
        /// </summary>
        /// <param name="address">The complete path to the FTP file to delete</param>
        /// <param name="credentials">The FTP server credentials</param>
        public static async Task FTPDeleteFileAsync(string address, ICredentials credentials)
        {
            WebRequest folderRequest = WebRequest.Create(address);
            folderRequest.Method = WebRequestMethods.Ftp.DeleteFile;
            folderRequest.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse)await folderRequest.GetResponseAsync())
            { }
        }

        /// <summary>
        /// Get a file size of an FTP file
        /// </summary>
        /// <param name="address">The complete path to the FTP file</param>
        /// <param name="credentials">The FTP server credentials</param>
        /// <returns>The size of the file in bytes</returns>
        public static long FTPGetFilesize(string address, ICredentials credentials)
        {
            long result = -1;
            // Get the object used to communicate with the server.
            //https://stackoverflow.com/questions/4591059/download-file-from-ftp-with-progress-totalbytestoreceive-is-always-1
            WebRequest request = WebRequest.Create(address);
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            request.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                //Stream responseStream = response.GetResponseStream();
                result = response.ContentLength;
            }
            return result;
        }

        /// <summary>
        /// Get a file size of an FTP file
        /// </summary>
        /// <param name="address">The complete path to the FTP file</param>
        /// <param name="credentials">The FTP server credentials</param>
        /// <returns>The size of the file in bytes</returns>
        public static async Task<long> FTPGetFilesizeAsync(string address, ICredentials credentials)
        {
            long result = -1;
            // Get the object used to communicate with the server.
            //https://stackoverflow.com/questions/4591059/download-file-from-ftp-with-progress-totalbytestoreceive-is-always-1
            WebRequest request = WebRequest.Create(address);
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            request.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
            {
                //Stream responseStream = response.GetResponseStream();
                result = response.ContentLength;
            }
            return result;
        }
        #endregion

        #region Gross shortcut stuff
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
        #endregion
    }
}
