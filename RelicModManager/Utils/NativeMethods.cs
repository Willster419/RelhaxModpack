using System;
using System.Runtime.InteropServices;

namespace RelhaxModpack
{
    public static class NativeMethods                         // https://stackoverflow.com/questions/254197/how-can-i-get-the-active-screen-dimensions
    {
        public const Int32 MONITOR_DEFAULTTOPRIMERTY = 0x00000001;
        public const Int32 MONITOR_DEFAULTTONEAREST = 0x00000002;

        public const Int32 MOVEFILE_COPY_ALLOWED = 0x02;
        public const Int32 MOVEFILE_DELAY_UNTIL_REBOOT = 0x04;
        public const Int32 MOVEFILE_FAIL_IF_NOT_TRACKABLE = 0x20;
        public const Int32 MOVEFILE_REPLACE_EXISTING = 0x01;
        public const Int32 MOVEFILE_WRITE_THROUGH = 0x08;

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr handle, Int32 flags);

        [DllImport("user32.dll")]
        public static extern Boolean GetMonitorInfo(IntPtr hMonitor, NativeMonitorInfo lpmi);

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct NativeRectangle
        {
            public Int32 Left;
            public Int32 Top;
            public Int32 Right;
            public Int32 Bottom;


            public NativeRectangle(Int32 left, Int32 top, Int32 right, Int32 bottom)
            {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public sealed class NativeMonitorInfo
        {
            public Int32 Size = Marshal.SizeOf(typeof(NativeMonitorInfo));
            public NativeRectangle Monitor;
            public NativeRectangle Work;
            public Int32 Flags;
        }

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();

        public static string GetLastError(bool s = false)
        {
            GetLastError();
            string errorMessage = new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message;
            return errorMessage;
        }

        public static bool MoveFileEx(string existingFileName, string newFileName, bool overwrite, bool waitTillFinished = true)
        {
            return MoveFileEx(existingFileName, newFileName, MOVEFILE_COPY_ALLOWED | (overwrite ? MOVEFILE_REPLACE_EXISTING : 0) | (waitTillFinished ? MOVEFILE_WRITE_THROUGH : 0));
        }

        [return: MarshalAs(UnmanagedType.Bool)]                 // https://stackoverflow.com/questions/5920882/file-move-does-not-work-file-already-exists
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool MoveFileEx(string existingFileName, string newFileName, int flags);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, BestFitMapping = false, SetLastError = true, EntryPoint = "LoadLibrary")]
        public static extern IntPtr WinLoadLibrary(String fileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, String procName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool GetDiskFreeSpace(string lpRootPathName,
           out uint lpSectorsPerCluster,
           out uint lpBytesPerSector,
           out uint lpNumberOfFreeClusters,
           out uint lpTotalNumberOfClusters);
    }
}
