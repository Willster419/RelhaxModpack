using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using File = System.IO.File;

namespace RelhaxModpack.Shortcuts
{
    /// <summary>
    /// A utility class to handle creating of shortcuts
    /// </summary>
    public static class ShortcutUtils
    {
        #region Shortcut Utility methods
        /// <summary>
        /// Creates a shortcut on the user's desktop
        /// </summary>
        /// <param name="shortcut">The shortcut parameters</param>
        /// <param name="sb">The StringBuilder to log the path to the created file</param>
        public static void CreateShortcut(Shortcut shortcut, StringBuilder sb)
        {
            Logging.Info(shortcut.ToString());
            Logging.Info("Creating shortcut {0}", shortcut.Name);

            //build the full macro for path (target) and name (also filename)
            string target = MacroUtils.MacroReplace(shortcut.Path, ReplacementTypes.FilePath).Replace(@"/", @"\");
            string filename = string.Format("{0}.lnk", shortcut.Name);
            string shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            shortcutPath = Path.Combine(shortcutPath, filename);

            Logging.Info("Target={0}", target);
            Logging.Info("ShortcutPath={0}", shortcutPath);
            if (!File.Exists(target))
            {
                Logging.Warning(LogOptions.ClassName, "Target does not exist, skipping shortcut", target);
                return;
            }

            //target exists, but do we need to update the shortcut?
            if (File.Exists(shortcutPath))
            {
                Logging.Debug("Shortcut path exists, checking if update needed");

                //get the target path
                IShellLink link = (IShellLink)new ShellLink();
                (link as IPersistFile).Load(shortcutPath, (int)StgmConstants.STGM_READ);
                StringBuilder oldTargetSB = new StringBuilder((int)Win32Value.MAX_PATH);
                link.GetPath(oldTargetSB, oldTargetSB.Capacity, null, (int)SLGP.RAWPATH);
                string oldTarget = oldTargetSB.ToString();
                
                Logging.Debug("New target = {0}, old target = {1}", target, oldTarget);
                if (!target.Equals(oldTarget))
                {
                    //needs update
                    Logging.Debug("Updating target");
                    link.SetDescription("Created by Relhax Modpack");
                    link.SetPath(target);
                    link.SetWorkingDirectory(Path.GetDirectoryName(target));

                    //The arguments used when executing the target (none used for now)
                    link.SetArguments("");
                    (link as IPersistFile).Save(shortcutPath, false);
                }
                else
                {
                    //no update needed
                    Logging.Debug("No update needed");
                    return;
                }
                
            }
            else
            {
                Logging.Debug("Shortcut path does not exist, creating");
                IShellLink link = (IShellLink)new ShellLink();

                // setup shortcut information
                link.SetDescription("Created by Relhax Modpack");
                link.SetPath(target);
                link.SetIconLocation(target, 0);
                link.SetWorkingDirectory(Path.GetDirectoryName(target));

                //The arguments used when executing the exe (none used for now)
                link.SetArguments("");
                (link as IPersistFile).Save(shortcutPath, false);
            }

            //getting here means that the target is updated or created, so log it to the installer
            sb.AppendLine(shortcutPath);
        }
        #endregion

        #region Gross shortcut stuff
#pragma warning disable CS1591
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
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, [In, Out] WIN32_FIND_DATAW pfd, int fFlags);
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

        // needed for CreateShortcut
        [Flags]
        public enum StgmConstants
        {
            STGM_READ = 0x0,
            STGM_WRITE = 0x1,
            STGM_READWRITE = 0x2,
            STGM_SHARE_DENY_NONE = 0x40,
            STGM_SHARE_DENY_READ = 0x30,
            STGM_SHARE_DENY_WRITE = 0x20,
            STGM_SHARE_EXCLUSIVE = 0x10,
            STGM_PRIORITY = 0x40000,
            STGM_CREATE = 0x1000,
            STGM_CONVERT = 0x20000,
            STGM_FAILIFTHERE = 0x0,
            STGM_DIRECT = 0x0,
            STGM_TRANSACTED = 0x10000,
            STGM_NOSCRATCH = 0x100000,
            STGM_NOSNAPSHOT = 0x200000,
            STGM_SIMPLE = 0x8000000,
            STGM_DIRECT_SWMR = 0x400000,
            STGM_DELETEONRELEASE = 0x4000000
        }

        //https://github.com/Slashka-DK/BlitzChat/blob/d6d5a5029caa925496a4cda85867a541d2f0e7de/packages/WPF%20Shell%20Integration%20Library%20v2/Microsoft.Windows.Shell/Standard/NativeMethods.cs
        internal static class Win32Value
        {
            public const uint MAX_PATH = 260;
            public const uint INFOTIPSIZE = 1024;
            public const uint TRUE = 1;
            public const uint FALSE = 0;
            public const uint sizeof_WCHAR = 2;
            public const uint sizeof_CHAR = 1;
            public const uint sizeof_BOOL = 4;
        }

        //https://github.com/Slashka-DK/BlitzChat/blob/d6d5a5029caa925496a4cda85867a541d2f0e7de/packages/WPF%20Shell%20Integration%20Library%20v2/Microsoft.Windows.Shell/Standard/NativeMethods.cs
        [Flags]
        enum SLGP
        {
            SHORTPATH = 0x1,
            UNCPRIORITY = 0x2,
            RAWPATH = 0x4
        }

        //https://github.com/Slashka-DK/BlitzChat/blob/d6d5a5029caa925496a4cda85867a541d2f0e7de/packages/WPF%20Shell%20Integration%20Library%20v2/Microsoft.Windows.Shell/Standard/NativeMethods.csv
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        [BestFitMapping(false)]
        internal class WIN32_FIND_DATAW
        {
            public FileAttributes dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public int nFileSizeHigh;
            public int nFileSizeLow;
            public int dwReserved0;
            public int dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }
#pragma warning restore CS1591
        #endregion
    }
}
