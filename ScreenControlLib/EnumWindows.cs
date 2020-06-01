using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static WindowsApi.User32;

namespace ScreenControlLib
{
    /// <summary>
    ///  
    /// </summary>
    /// <remarks>
    ///  References:
    ///  https://stackoverflow.com/questions/4082770/control-the-mouse-cursor-using-c-sharp
    ///  https://stackoverflow.com/questions/3047375/simulating-key-press-c-sharp
    ///  https://stackoverflow.com/questions/34347387/switch-between-windows-of-a-program
    ///  https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
    ///  https://stackoverflow.com/questions/7732633/postmessage-wm-keydown-send-multiply-keys
    ///  http://pinvoke.net/default.aspx/user32.sendinput
    /// </remarks>
    public static class EnumWindows
    {
        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            List<IntPtr> pointers = GCHandle.FromIntPtr(pointer).Target as List<IntPtr>;
            pointers.Add(handle);
            return true;
        }

        private static List<IntPtr> GetAllWindows()
        {
            var enumCallback = new EnumWindowsProc(EnumWindow);
            List<IntPtr> AllWindowPtrs = new List<IntPtr>();
            var listHandle = GCHandle.Alloc(AllWindowPtrs);
            try
            {
                EnumWindows(enumCallback, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return AllWindowPtrs;
        }

        public static string GetTitle(IntPtr handle)
        {
            var length = GetWindowTextLength(handle);
            var sb = new StringBuilder(length + 1);
            GetWindowText(handle, sb, sb.Capacity);
            return sb.ToString();
        }

        public static IEnumerable<IntPtr> GetAllWindowsContaining(string containedTitle)
        {
            var allWindows = GetAllWindows();
            foreach (var ptr in allWindows)
            {
                if (GetTitle(ptr).Contains(containedTitle) == true)
                {
                    yield return ptr;
                }
            }
        }

        public static IEnumerable<IntPtr> GetAllMainWindowsByProcessName(string processName)
            => Process.GetProcessesByName(processName).Select(x => x.MainWindowHandle);
    }
}
