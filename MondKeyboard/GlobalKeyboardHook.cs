using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MondKeyboard
{
    class GlobalKeyboardHook
    {
        private IntPtr _hookHandle = IntPtr.Zero;
        private KeyboardHookProc _hookProc;

        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyUp;

        public GlobalKeyboardHook()
        {
            var hInstance = LoadLibrary("user32.dll");

            _hookProc = HookProc;
            _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, hInstance, 0);
        }

        ~GlobalKeyboardHook()
        {
            UnhookWindowsHookEx(_hookHandle);
        }

        private int HookProc(int code, int wParam, ref KeyboardHookStruct lParam)
        {
            if (code >= 0)
            {
                var key = (Keys)lParam.vkCode;
                var args = new KeyEventArgs(key);

                if ((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) && (KeyDown != null))
                {
                    KeyDown(this, args);
                }
                else if ((wParam == WM_KEYUP || wParam == WM_SYSKEYUP) && (KeyUp != null))
                {
                    KeyUp(this, args);
                }

                if (args.Handled)
                    return 1;
            }

            return CallNextHookEx(_hookHandle, code, wParam, ref lParam);
        }

        #region DLL Imports

        private delegate int KeyboardHookProc(int code, int wParam, ref KeyboardHookStruct lParam);

        public struct KeyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHookProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref KeyboardHookStruct lParam);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        #endregion
    }
}
