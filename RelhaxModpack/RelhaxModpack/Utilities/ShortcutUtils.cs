using IWshRuntimeLibrary;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using File = System.IO.File;

namespace RelhaxModpack.Utilities
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
            Logging.Info("target={0}", target);
            Logging.Info("shortcutPath={0}", shortcutPath);
            if (!File.Exists(target))
            {
                Logging.Warning("target does not exist, skipping shortcut", target);
                return;
            }
            if (File.Exists(shortcutPath))
            {
                Logging.Debug("shortcut path exists, checking if update needed");
                WshShell shell = new WshShell();
                IWshShortcut link = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                Logging.Debug("new target = {0}, old target = {1}", target, link.TargetPath);
                if (!target.Equals(link.TargetPath))
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
