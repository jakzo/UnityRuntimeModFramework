using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;

namespace Urmf
{
    class Program
    {
        static void Main(string[] args)
        {
            // var processId = WinApi.GetPidByName("SonsOfTheForest");
            // Console.WriteLine($"processId = {processId}");
            // var processHandle = WinApi.OpenProcess(processId, WinApi.ProcessAccess.ALL);
            // Console.WriteLine($"processHandle = {processHandle}");
            // var closeResult = WinApi.CloseHandle(processHandle);
            // Console.WriteLine($"closeResult = {closeResult}");

            var process = Process.GetProcessesByName("SonsOfTheForest")[0];
            var gameAssembly = FindGameAssembly(process);
            Console.WriteLine($"gameAssembly = {gameAssembly?.ModuleName ?? "(none)"}");
        }

        static ProcessModule FindProcessModule(Process process, Func<ProcessModule, bool> predicate)
        {
            foreach (ProcessModule module in process.Modules)
            {
                if (predicate(module))
                {
                    return module;
                }
            }
            return null;
        }

        static IntPtr FindGetProcAddress(Process process)
        {
            var kernel32Handle = WinApi.Call(WinApi.LoadLibrary("kernel32.dll"));
            var getProcAddressAddr = WinApi.Call(
                WinApi.GetProcAddress(kernel32Handle, "GetProcAddress")
            );
            var offset = getProcAddressAddr.ToInt64() - kernel32Handle.ToInt64();
            var kernel32 = FindProcessModule(
                process,
                module => module.ModuleName.ToLower() == "kernel32.dll"
            );
            if (kernel32 == null)
                throw new Exception("kernel32 module not found");
            return new IntPtr(kernel32.BaseAddress.ToInt64() + offset);
        }

        static ProcessModule FindGameAssembly(Process process)
        {
            var getProcAddressAddr = FindGetProcAddress(process);
            foreach (ProcessModule module in process.Modules)
            {
                var moduleHandle = WinApi.GetModuleHandle(module.ModuleName);
                if (moduleHandle == IntPtr.Zero)
                    continue;
                var result = GetProcAddress(
                    getProcAddressAddr,
                    process,
                    moduleHandle,
                    "il2cpp_thread_attach"
                );
                if (result != IntPtr.Zero)
                    return module;
                break;
            }
            return null;
        }

        static IntPtr GetProcAddress(
            IntPtr getProcAddressAddr,
            Process process,
            IntPtr moduleHandle,
            string name
        )
        {
            const uint PAGE_SIZE = 4096;
            var allocatedAddress = WinApi.Call(
                WinApi.VirtualAllocEx(
                    process.Handle,
                    IntPtr.Zero,
                    PAGE_SIZE,
                    WinApi.AllocationType.MEM_COMMIT | WinApi.AllocationType.MEM_RESERVE,
                    WinApi.Protection.EXECUTE_READ_WRITE
                )
            );

            try
            {
                var returnAddress = allocatedAddress + 0x500;
                var nameAddress = returnAddress + IntPtr.Size;

                var nameBytes = Encoding.ASCII.GetBytes(name + "\0"); // null-terminated
                WinApi.Call(
                    WinApi.WriteProcessMemory(
                        process.Handle,
                        nameAddress,
                        nameBytes,
                        (uint)nameBytes.Length,
                        out var _
                    )
                );

                var byteCode = new AsmX8664()
                    .Sub(AsmX8664.Register.RSP, 0x20)
                    .Mov(AsmX8664.Register.RAX, getProcAddressAddr.ToInt64())
                    .Mov(AsmX8664.Register.RCX, moduleHandle.ToInt64())
                    .Mov(AsmX8664.Register.RDX, nameAddress.ToInt64())
                    .Call(AsmX8664.Register.RAX)
                    .MovRaxToAddress((ulong)returnAddress.ToInt64())
                    .Add(AsmX8664.Register.RSP, 0x20)
                    .Ret()
                    .GetBytes();

                // Console.WriteLine(getProcAddressAddr.ToInt64().ToString("X8"));
                // Console.WriteLine(moduleHandle.ToInt64().ToString("X8"));
                // Console.WriteLine(nameAddress.ToInt64().ToString("X8"));
                // Console.WriteLine(returnAddress.ToInt64().ToString("X8"));
                // Console.WriteLine(string.Join(", ", byteCode.Select(b => b.ToString("X2"))));

                WinApi.Call(
                    WinApi.WriteProcessMemory(
                        process.Handle,
                        allocatedAddress,
                        byteCode,
                        (uint)byteCode.Length,
                        out var _
                    )
                );

                var threadHandle = WinApi.Call(
                    WinApi.CreateRemoteThread(
                        process.Handle,
                        IntPtr.Zero,
                        0,
                        allocatedAddress,
                        IntPtr.Zero,
                        0,
                        out var _
                    )
                );
                WinApi.Call(WinApi.WaitForSingleObject(threadHandle, WinApi.INFINITE), 0xFFFFFFFF);
                WinApi.Call(WinApi.CloseHandle(threadHandle));

                var resultBuffer = new byte[IntPtr.Size];
                WinApi.Call(
                    WinApi.ReadProcessMemory(
                        process.Handle,
                        returnAddress,
                        resultBuffer,
                        (uint)resultBuffer.Length,
                        out var _
                    )
                );

                return new IntPtr(BitConverter.ToInt64(resultBuffer, 0));
            }
            finally
            {
                WinApi.Call(
                    WinApi.VirtualFreeEx(
                        process.Handle,
                        allocatedAddress,
                        PAGE_SIZE,
                        WinApi.FreeType.MEM_RELEASE
                    )
                );
            }
        }
    }
}
