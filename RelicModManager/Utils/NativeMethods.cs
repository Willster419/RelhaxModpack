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

        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();

        public static string GetLastError(bool s = false)
        {
            GetLastError();
            string errorMessage = new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message;
            return errorMessage;
        }

        public static bool MoveFileEx(string existingFileName, string newFileName, bool overwrite)
        {
            if (overwrite)
                return MoveFileExW(existingFileName, newFileName, MOVEFILE_COPY_ALLOWED | MOVEFILE_REPLACE_EXISTING | MOVEFILE_WRITE_THROUGH);
            else
                return MoveFileExW(existingFileName, newFileName, 0);
        }

        [return: MarshalAs(UnmanagedType.Bool)]                 // https://stackoverflow.com/questions/5920882/file-move-does-not-work-file-already-exists
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        // public static extern bool MoveFileEx(string existingFileName, string newFileName, int flags);
        private static extern bool MoveFileExW(string existingFileName, string newFileName, int flags);
    }
}
