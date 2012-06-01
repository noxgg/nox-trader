using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace noxiousET.src.helpers
{
    static class ProcessKiller
    {

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public static int killProcess(string processName)
        {
            try
            {
                Process[] proc = Process.GetProcessesByName(processName);
                foreach (Process p in proc)
                {
                    p.Kill();
                }
            }
            catch
            {
                return 1;
            }
            return 0;
        }
        public static int killProcessByHandle(IntPtr handle)
        {
            try
            {
                uint activePID;
                GetWindowThreadProcessId(handle, out activePID);
                Process nukeIt = Process.GetProcessById((int)activePID);
                nukeIt.Kill();
                Thread.Sleep(1000);
                return 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}
