using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DzTree.VideoRecoder.Helper
{
    public class WinApiHelper
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GenerateConsoleCtrlEvent(int dwCtrlEvent, int dwProcessGroupId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeConsole();

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool AttachConsole(int dwProcessId);

        public delegate bool ControlCtrlDelegate(int CtrlType);
        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleCtrlHandler(IntPtr HandlerRoutine, bool Add);



    }
}
