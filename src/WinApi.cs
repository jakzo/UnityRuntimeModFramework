using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Urmf
{
    public static class WinApi
    {
        private const uint HR_SUCCESS = 0x80070000;

        private static void CheckError(bool succeeded)
        {
            if (!succeeded)
            {
                var hr = Marshal.GetHRForLastWin32Error();
                if ((uint)hr != HR_SUCCESS)
                    Marshal.ThrowExceptionForHR(hr);
            }
        }

        private static uint CheckError(uint result, uint failureCode = 0)
        {
            if (result == failureCode)
                CheckError(false);
            return result;
        }

        private static IntPtr CheckError(IntPtr result)
        {
            if (result == IntPtr.Zero)
                CheckError(false);
            return result;
        }

        [DllImport("kernel32.dll", EntryPoint = "LoadLibrary", SetLastError = true)]
        private static extern IntPtr LoadLibraryInternal(string lpFileName);

        public static IntPtr LoadLibrary(string lpFileName) =>
            CheckError(LoadLibraryInternal(lpFileName));

        public enum ProcessAccess : uint
        {
            ALL = 0x1fffff,
        }

        [DllImport("kernel32.dll", EntryPoint = "OpenProcess", SetLastError = true)]
        private static extern IntPtr OpenProcessInternal(
            ProcessAccess dwDesiredAccess,
            bool bInheritHandle,
            int dwProcessId
        );

        public static IntPtr OpenProcess(
            ProcessAccess dwDesiredAccess,
            bool bInheritHandle,
            int dwProcessId
        ) => CheckError(OpenProcessInternal(dwDesiredAccess, bInheritHandle, dwProcessId));

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
        private static extern bool CloseHandleInternal(IntPtr hObject);

        public static void CloseHandle(IntPtr hObject) => CheckError(CloseHandleInternal(hObject));

        [Flags]
        public enum AllocationType : uint
        {
            MEM_COMMIT = 0x1000,
            MEM_RESERVE = 0x2000,
        }

        [Flags]
        public enum Protection : uint
        {
            EXECUTE_READ_WRITE = 0x40,
        }

        [DllImport(
            "kernel32.dll",
            EntryPoint = "VirtualAllocEx",
            SetLastError = true,
            ExactSpelling = true
        )]
        private static extern IntPtr VirtualAllocExInternal(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            AllocationType flAllocationType,
            Protection flProtect
        );

        public static IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            AllocationType flAllocationType,
            Protection flProtect
        ) =>
            CheckError(
                VirtualAllocExInternal(hProcess, lpAddress, dwSize, flAllocationType, flProtect)
            );

        [Flags]
        public enum FreeType : uint
        {
            MEM_DECOMMIT = 0x4000,
            MEM_RELEASE = 0x8000
        }

        [DllImport("kernel32.dll", EntryPoint = "VirtualFreeEx", SetLastError = true)]
        private static extern bool VirtualFreeExInternal(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            FreeType dwFreeType
        );

        public static void VirtualFreeEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            FreeType dwFreeType
        ) => CheckError(VirtualFreeExInternal(hProcess, lpAddress, dwSize, dwFreeType));

        [DllImport(
            "kernel32.dll",
            EntryPoint = "GetProcAddress",
            CharSet = CharSet.Ansi,
            ExactSpelling = true,
            SetLastError = true
        )]
        private static extern IntPtr GetProcAddressInternal(IntPtr hModule, string procName);

        public static IntPtr GetProcAddress(IntPtr hModule, string procName) =>
            CheckError(GetProcAddressInternal(hModule, procName));

        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandle", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandleInternal(string lpModuleName);

        public static IntPtr GetModuleHandle(string lpModuleName) =>
            CheckError(GetModuleHandleInternal(lpModuleName));

        [DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory", SetLastError = true)]
        private static extern bool WriteProcessMemoryInternal(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            uint nSize,
            out int lpNumberOfBytesWritten
        );

        public static void WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            uint nSize,
            out int lpNumberOfBytesWritten
        ) =>
            CheckError(
                WriteProcessMemoryInternal(
                    hProcess,
                    lpBaseAddress,
                    lpBuffer,
                    nSize,
                    out lpNumberOfBytesWritten
                )
            );

        [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory", SetLastError = true)]
        private static extern bool ReadProcessMemoryInternal(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            uint dwSize,
            out int lpNumberOfBytesRead
        );

        public static void ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            uint dwSize,
            out int lpNumberOfBytesRead
        ) =>
            CheckError(
                ReadProcessMemoryInternal(
                    hProcess,
                    lpBaseAddress,
                    lpBuffer,
                    dwSize,
                    out lpNumberOfBytesRead
                )
            );

        [DllImport("kernel32.dll", EntryPoint = "CreateRemoteThread", SetLastError = true)]
        private static extern IntPtr CreateRemoteThreadInternal(
            IntPtr hProcess,
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            out uint lpThreadId
        );

        public static IntPtr CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            out uint lpThreadId
        ) =>
            CheckError(
                CreateRemoteThreadInternal(
                    hProcess,
                    lpThreadAttributes,
                    dwStackSize,
                    lpStartAddress,
                    lpParameter,
                    dwCreationFlags,
                    out lpThreadId
                )
            );

        [DllImport("kernel32.dll", EntryPoint = "WaitForSingleObject", SetLastError = true)]
        private static extern uint WaitForSingleObjectInternal(IntPtr hHandle, uint dwMilliseconds);

        public static uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds) =>
            CheckError(WaitForSingleObjectInternal(hHandle, dwMilliseconds), 0xFFFFFFFF);

        public const uint INFINITE = 0xFFFFFFFF;
        public const uint WAIT_ABANDONED = 0x00000080;
        public const uint WAIT_OBJECT_0 = 0x00000000;
        public const uint WAIT_TIMEOUT = 0x00000102;
    }
}
