using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Urmf
{
    public static class WinApi
    {
        public class WinApiException : Exception
        {
            public WinApiException(uint code, string message)
                : base($"Windows API failed: [{code}] {message}") { }
        }

        public static void Call(bool succeeded)
        {
            if (!succeeded)
            {
                var code = GetLastError();
                var message = GetErrorMessage(code);
                throw new WinApiException(code, message);
            }
        }

        public static uint Call(uint result, uint failureCode = 0)
        {
            if (result == failureCode)
                Call(false);
            return result;
        }

        public static IntPtr Call(IntPtr result)
        {
            if (result == IntPtr.Zero)
                Call(false);
            return result;
        }

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

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint FormatMessage(
            FormatMessageFlags dwFlags,
            IntPtr lpSource,
            uint dwMessageId,
            uint dwLanguageId,
            StringBuilder lpBuffer,
            uint nSize,
            IntPtr arguments
        );

        [Flags]
        public enum FormatMessageFlags : uint
        {
            FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100,
            FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200,
            FORMAT_MESSAGE_FROM_STRING = 0x00000400,
            FORMAT_MESSAGE_FROM_HMODULE = 0x00000800,
            FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000,
            FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000,
            FORMAT_MESSAGE_MAX_WIDTH_MASK = 0x000000FF
        }

        public static string GetErrorMessage(uint errorCode)
        {
            const uint BUFFER_SIZE = 1024;
            StringBuilder messageBuffer = new StringBuilder((int)BUFFER_SIZE);

            FormatMessage(
                FormatMessageFlags.FORMAT_MESSAGE_FROM_SYSTEM
                    | FormatMessageFlags.FORMAT_MESSAGE_IGNORE_INSERTS,
                IntPtr.Zero,
                errorCode,
                0,
                messageBuffer,
                BUFFER_SIZE,
                IntPtr.Zero
            );

            return messageBuffer.ToString().Trim();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        public enum ProcessAccess : uint
        {
            ALL = 0x1fffff,
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(
            ProcessAccess dwDesiredAccess,
            bool bInheritHandle,
            int dwProcessId
        );

        /// <summary>
        /// Remember to pass the IntPtr into CloseHandle when you're done.
        /// </summary>
        public static IntPtr OpenProcess(int processId, ProcessAccess desiredAccess)
        {
            var hProcess = OpenProcess(desiredAccess, false, processId);
            if (hProcess == IntPtr.Zero)
                throw new Exception("Failed to open process");
            return hProcess;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        public enum AllocationType : uint
        {
            MEM_COMMIT = 0x1000,
            MEM_RESERVE = 0x2000,
        }

        public enum Protection : uint
        {
            EXECUTE_READ_WRITE = 0x40,
        }

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            AllocationType flAllocationType,
            Protection flProtect
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

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            uint nSize,
            out int lpNumberOfBytesWritten
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            uint dwSize,
            out int lpNumberOfBytesRead
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            out uint lpThreadId
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        public const uint INFINITE = 0xFFFFFFFF;
        public const uint WAIT_ABANDONED = 0x00000080;
        public const uint WAIT_OBJECT_0 = 0x00000000;
        public const uint WAIT_TIMEOUT = 0x00000102;
    }
}
