using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RelhaxModpack.Common
{
    internal delegate T2 GenericDelegate<T1, T2>(T1 param);
    internal delegate T3 GenericDelegate<T1, T2, T3>(ref T1 param1, ref T2 param2);

    internal static class WindowsInterop
    {
        #region << Variable Declarations >>

        private const Int32 WM_COMMAND = 0x0111;
        private const Int32 WM_INITDIALOG = 0x0110;

        private static IntPtr _pWH_CALLWNDPROCRET = IntPtr.Zero;

        private static HookProcedureDelegate _WH_CALLWNDPROCRET_PROC =
            new HookProcedureDelegate(WindowsInterop.WH_CALLWNDPROCRET_PROC);

        internal static event GenericDelegate<Boolean, Boolean> SecurityAlertDialogWillBeShown;
        internal static event GenericDelegate<String, String, Boolean> ConnectToDialogWillBeShown;

        #endregion

        #region << DllImports >>

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(HookType hooktype, HookProcedureDelegate callback, IntPtr hMod, UInt32 dwThreadId);

        [DllImport("user32.dll")]
        private static extern IntPtr UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern Int32 CallNextHookEx(IntPtr hhk, Int32 nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern Int32 GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern Int32 GetWindowText(IntPtr hWnd, StringBuilder text, Int32 maxLength);

        [DllImport("user32.dll")]
        private static extern Boolean SetWindowText(IntPtr hWnd, String lpString);

        [DllImport("user32.dll")]
        private static extern Int32 GetClassName(IntPtr hWnd, StringBuilder lpClassName, Int32 nMaxCount);

        [DllImport("user32.dll")]
        private static extern Boolean EnumChildWindows(IntPtr hWndParent, EnumerateWindowDelegate callback, IntPtr data);

        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetDlgCtrlID(IntPtr hwndCtl);

        #endregion

        #region << Managed Structures/Enums >>

        // Hook Types
        private enum HookType
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CWPRETSTRUCT
        {
            public IntPtr lResult;
            public IntPtr lParam;
            public IntPtr wParam;
            public UInt32 message;
            public IntPtr hwnd;
        };

        #endregion

        #region << Managed Delegates For Unmanaged Functions >>

        // Delegate for a WH_ hook procedure
        private delegate Int32 HookProcedureDelegate(Int32 iCode, IntPtr pWParam, IntPtr pLParam);

        // Delegate for the EnumChildWindows method
        private delegate Boolean EnumerateWindowDelegate(IntPtr pHwnd, IntPtr pParam);

        #endregion

        #region << Internal Static Functions >>

        // Add a Hook into the CALLWNDPROCRET notification chain
        internal static void Hook()
        {
            if (WindowsInterop._pWH_CALLWNDPROCRET == IntPtr.Zero)
            {
                WindowsInterop._pWH_CALLWNDPROCRET = SetWindowsHookEx(
                    HookType.WH_CALLWNDPROCRET,
                        WindowsInterop._WH_CALLWNDPROCRET_PROC,
                            IntPtr.Zero,
                                (uint)AppDomain.GetCurrentThreadId());

                // NB: Visual Studio will likely be upset about 
                // the use of the Obsolete 'GetCurrentThreadId()' function
                // however, unless the app is running on fibres 
                // it seems to do the job quite nicely.
            }
        }

        // Remove the Hook into the CALLWNDPROCRET notification chain
        internal static void Unhook()
        {
            if (WindowsInterop._pWH_CALLWNDPROCRET != IntPtr.Zero)
            {
                UnhookWindowsHookEx(WindowsInterop._pWH_CALLWNDPROCRET);
            }
        }

        #endregion

        #region << Private Parts >>

        // Hook proceedure called by the OS when a message has been processed by the target Window
        private static Int32 WH_CALLWNDPROCRET_PROC(Int32 iCode, IntPtr pWParam, IntPtr pLParam)
        {
            if (iCode < 0)
                return CallNextHookEx(WindowsInterop._pWH_CALLWNDPROCRET, iCode, pWParam, pLParam);

            CWPRETSTRUCT cwp = (CWPRETSTRUCT)
                Marshal.PtrToStructure(pLParam, typeof(CWPRETSTRUCT));

            if (cwp.message == WM_INITDIALOG)
            {
                // A dialog was initialised, find out what sort it was via it's Caption text
                Int32 iLength = GetWindowTextLength(cwp.hwnd);
                StringBuilder sb = new StringBuilder(iLength + 1);

                GetWindowText(cwp.hwnd, sb, sb.Capacity);
                if (StringConstants.DialogCaptionSecurityAlert.Equals(sb.ToString(),
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    // A "Security Alert" dialog was initialised, now need 
                    //
                    // a)   To know what type it is - e.g. is it an SSL related one or a switching b/w 
                    //      secure/non-secure modes etc one - and,
                    //
                    // b)   A handle to the 'Yes' button so a user-click can be simulated on it
                    //
                    Boolean blnIsSslDialog = true; // assumed true for now
                    IntPtr pYesButtonHwnd = IntPtr.Zero;

                    // Check out further properties of the dialog
                    foreach (IntPtr pChildOfDialog in WindowsInterop.listChildWindows(cwp.hwnd))
                    {
                        // Go through all of the child controls on the dialog and see what they reveal via their text
                        iLength = GetWindowTextLength(pChildOfDialog);
                        if (iLength > 0)
                        {
                            StringBuilder sbProbe = new StringBuilder(iLength + 1);
                            GetWindowText(pChildOfDialog, sbProbe, sbProbe.Capacity);

                            if (StringConstants.DialogTextSecureToNonSecureWarning.Equals(sbProbe.ToString(),
                                    StringComparison.InvariantCultureIgnoreCase) ||
                                        StringConstants.DialogTextNonSecureToSecureWarning.Equals(sbProbe.ToString(),
                                            StringComparison.InvariantCultureIgnoreCase))
                            {
                                // Ok, the text says something about toggling secure/non-secure or vice-versa so,
                                blnIsSslDialog = false;
                            }

                            if (StringConstants.ButtonTextYes.Equals(sbProbe.ToString(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                // Hey, this one says 'Yes', so cache a pointer to it
                                pYesButtonHwnd = pChildOfDialog;

                                // NB: Could also confim it is a button we're pointing at, at this point, and
                                // not a label or something by using the Win32 API's GetClassName() function (see below for e.g.),
                                // but in this case it is already known that a button is the only thing saying 'Yes' 
                                // on this particular dialog
                            }
                        }
                    }

                    // Ok, some sort of "Security Alert" dialog was initialised, fire a message 
                    // to anyone who's listening i.e. In this case, ask the event receiver if 
                    // they want to ignore it
                    if (SecurityAlertDialogWillBeShown != null)
                    {
                        if (SecurityAlertDialogWillBeShown(blnIsSslDialog) && pYesButtonHwnd != IntPtr.Zero)
                        {
                            // Alert the dialog's message-pump that the 'Yes' button was 'Pressed'
                            Int32 ctrlId = GetDlgCtrlID(pYesButtonHwnd);
                            SendMessage(cwp.hwnd, WM_COMMAND, new IntPtr(ctrlId), pYesButtonHwnd);

                            // Block any further processing of the WM_INITDIALOG message by anyone else
                            // This is important: by doing this, the dialog never shows up on the screen
                            return 1;
                        }
                    }
                }
                else if (sb.ToString().StartsWith(StringConstants.DialogCaptionConnectTo))
                {
                    // 'Connect to ...' style dialog shown

                    IntPtr pComboHwnd = IntPtr.Zero;
                    IntPtr pEditHwnd = IntPtr.Zero;
                    IntPtr pOkButtonHwnd = IntPtr.Zero;

                    foreach (IntPtr pChildOfDialog in WindowsInterop.listChildWindows(cwp.hwnd))
                    {
                        // Go through all of the child controls on the dialog and see what they reveal via their type/text
                        StringBuilder sbProbe = new StringBuilder(255);
                        if (GetClassName(pChildOfDialog, sbProbe, sbProbe.Capacity) != 0 &&
                            !String.IsNullOrEmpty(sbProbe.ToString()))
                        {
                            if (sbProbe.ToString().Equals("SysCredential"))
                            {
                                // This control is actually a set of controls called a "SysCredential" 
                                // (as determined via Spy++), so cache it's child controls that are of 
                                // type "ComboBoxEx32" and "Edit"
                                foreach (IntPtr pChildOfSysCredential in WindowsInterop.listChildWindows(pChildOfDialog))
                                {
                                    StringBuilder sbProbe2 = new StringBuilder(255);
                                    if (GetClassName(pChildOfSysCredential, sbProbe2, sbProbe2.Capacity) != 0 &&
                                        !String.IsNullOrEmpty(sbProbe2.ToString()))
                                    {
                                        if (StringConstants.WindowTypeCombo.Equals(sbProbe2.ToString(),
                                            StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            // Hey, here's the Combo
                                            pComboHwnd = pChildOfSysCredential;
                                        }

                                        if (StringConstants.WindowTypeEdit.Equals(sbProbe2.ToString(),
                                            StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            // Hey, here's *an* Edit box.
                                            //
                                            // This will happen several times as there is an Edit box at the bottom of 
                                            // the "ComboBoxEx" heirarchy as well, but luckily the last one encountered 
                                            // is actually the required one 
                                            pEditHwnd = pChildOfSysCredential;
                                        }
                                    }
                                }
                            }

                            if (StringConstants.WindowTypeButton.Equals(sbProbe.ToString(),
                                StringComparison.InvariantCultureIgnoreCase))
                            {
                                // This control is a Button, does it have text, if so what does it say
                                iLength = GetWindowTextLength(pChildOfDialog);
                                if (iLength > 0)
                                {
                                    StringBuilder sbText = new StringBuilder(iLength + 1);
                                    GetWindowText(pChildOfDialog, sbText, sbText.Capacity);
                                    if (StringConstants.ButtonTextOk.Equals(sbText.ToString(),
                                        StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        // Hey, this one says 'OK', so cache a pointer to it
                                        pOkButtonHwnd = pChildOfDialog;
                                    }
                                }
                            }
                        }
                    }

                    // Ok, a "Connect to" dialog was initialiased, fire a message to anyone who's interested
                    // i.e. Ask the event receiver if they want to automate the dialog by providing 
                    // a username and password with which to populate the dialog
                    if (ConnectToDialogWillBeShown != null)
                    {
                        String sUsername = null;
                        String sPassword = null;

                        if (ConnectToDialogWillBeShown(ref sUsername, ref sPassword) &&
                            sUsername != null && sPassword != null && pOkButtonHwnd != IntPtr.Zero &&
                                pComboHwnd != IntPtr.Zero && pEditHwnd != IntPtr.Zero)
                        {
                            // Yep, they do

                            // Put the username and password into the boxes
                            SetWindowText(pComboHwnd, sUsername);
                            SetWindowText(pEditHwnd, sPassword);

                            // Alert the dialog's message-pump that the 'OK' button was 'Pressed'
                            Int32 ctrlId = GetDlgCtrlID(pOkButtonHwnd);
                            SendMessage(cwp.hwnd, WM_COMMAND, new IntPtr(ctrlId), pOkButtonHwnd);

                            // Block further processing of the WM_INITDIALOG message by anyone else
                            // This is important: by doing this, the dialog never shows up on the screen
                            return 1;
                        }
                    }
                }
            }

            // Call the next hook in the chain
            return CallNextHookEx(WindowsInterop._pWH_CALLWNDPROCRET, iCode, pWParam, pLParam);
        }

        // Populate a list of all the child windows of a given parent 
        // (Uses the Win32 API's EnumChildWindows() function)
        private static List<IntPtr> listChildWindows(IntPtr p)
        {
            List<IntPtr> lstChildWindows = new List<IntPtr>();
            GCHandle gchChildWindows = GCHandle.Alloc(lstChildWindows);
            try
            {
                WindowsInterop.EnumChildWindows(p,
                    new EnumerateWindowDelegate(WindowsInterop.enumWindowsCallback),
                        GCHandle.ToIntPtr(gchChildWindows));
            }
            finally
            {
                if (gchChildWindows.IsAllocated)
                    gchChildWindows.Free();
            }

            return lstChildWindows;
        }

        // Callback method called when EnumChildWindows is enumerating windows.
        // (Called by the Win32API EnumChildWindows function)
        private static bool enumWindowsCallback(IntPtr hwnd, IntPtr p)
        {
            GCHandle gch = GCHandle.FromIntPtr(p);
            if (gch != null)
            {
                List<IntPtr> lstChildWindows = gch.Target as List<IntPtr>;
                if (lstChildWindows == null)
                {
                    // This should NEVER happen
                    throw new InvalidCastException(StringConstants.GCHandleTargetIsNotExpectedType);
                }

                lstChildWindows.Add(hwnd);
            }

            // Continue processing down the parent-child chain
            return true;
        }


        #endregion
    }
}
