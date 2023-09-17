using System;
using System.Diagnostics;

namespace Urmf
{
    public static class Patch
    {
        private const uint PAGE_SIZE = 4096;

        public static void RunAsmInProcess(Process process)
        {
            var address = WinApi.VirtualAllocEx(
                process.Handle,
                IntPtr.Zero,
                PAGE_SIZE,
                (uint)(WinApi.AllocationType.MEM_COMMIT | WinApi.AllocationType.MEM_RESERVE),
                (uint)WinApi.Protection.READ_WRITE
            );
        }
    }
}
