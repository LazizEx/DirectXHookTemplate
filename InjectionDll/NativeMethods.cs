using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InjectionDll
{
    [System.Security.SuppressUnmanagedCodeSecurity()]
    internal sealed class NativeMethods
    {
        // Use DllImport to import the Win32 MessageBox function.
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

        public static int MessageBox(string text, string caption = "Message")
        {
            return MessageBox(Process.GetCurrentProcess().MainWindowHandle, text, caption, 0);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        public const UInt32 WM_CLOSE = 0x0010;

        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();
    }
}
