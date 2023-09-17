using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Urmf
{
    public static class WinApi
    {
        public static int GetPidByName(string processName)
        {
            foreach (var process in Process.GetProcesses())
            {
                if (process.ProcessName == processName)
                {
                    return process.Id;
                }
            }
            throw new Exception($"Process not found with name: {processName}");
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        public enum ProcessAccess
        {
            ALL = 0x1fffff,
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(
            uint dwDesiredAccess,
            bool bInheritHandle,
            int dwProcessId
        );

        /// <summary>
        /// Remember to pass the IntPtr into CloseHandle when you're done.
        /// </summary>
        public static IntPtr OpenProcess(int processId, ProcessAccess desiredAccess)
        {
            var hProcess = OpenProcess((uint)desiredAccess, false, processId);
            if (hProcess == IntPtr.Zero)
                throw new Exception("Failed to open process");
            return hProcess;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        public enum AllocationType
        {
            MEM_COMMIT = 0x1000,
            MEM_RESERVE = 0x2000,
        }

        public enum Protection
        {
            READ_WRITE = 64,
        }

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            uint flAllocationType,
            uint flProtect
        );

        [DllImport(
            "kernel32.dll",
            CharSet = CharSet.Ansi,
            ExactSpelling = true,
            SetLastError = true
        )]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        // ---

        private const uint LIST_MODULES_ALL = 0x03;

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool EnumProcessModulesEx(
            IntPtr hProcess,
            [Out] IntPtr[] lphModule,
            uint cb,
            out uint lpcbNeeded,
            uint dwFilterFlag
        );

        public static IntPtr[] GetModules(IntPtr hProcess)
        {
            uint cb = (uint)(IntPtr.Size * 1024);
            IntPtr[] modules = new IntPtr[1024];

            if (EnumProcessModulesEx(hProcess, modules, cb, out uint cbNeeded, LIST_MODULES_ALL))
            {
                if (cb < cbNeeded)
                {
                    cb = cbNeeded;
                    modules = new IntPtr[cb / (uint)IntPtr.Size];
                    if (EnumProcessModulesEx(hProcess, modules, cb, out cbNeeded, LIST_MODULES_ALL))
                    {
                        return modules;
                    }
                }
                else
                {
                    return modules;
                }
            }
            return null;
        }
    }
}
