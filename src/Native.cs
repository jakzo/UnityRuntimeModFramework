using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Urmf
{
    public static class Native
    {
        public enum OperatingSystem
        {
            WINDOWS,
            OSX,
            LINUX,
        }

        public static OperatingSystem Os = GetOs();

        private static OperatingSystem GetOs()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return OperatingSystem.WINDOWS;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return OperatingSystem.OSX;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return OperatingSystem.LINUX;
            throw new Exception("Operating system not supported");
        }

        public static ProcessModule FindProcessModule(
            Process process,
            Func<ProcessModule, bool> predicate
        )
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

        public static byte[] ToNativeString(string str) => Encoding.ASCII.GetBytes(str + "\0");

        public static IntPtr FindFuncAddressGetter(Process process)
        {
            switch (Os)
            {
                case OperatingSystem.WINDOWS:
                {
                    var kernel32Handle = WinApi.LoadLibrary("kernel32.dll");
                    var getProcAddressAddr = WinApi.GetProcAddress(
                        kernel32Handle,
                        "GetProcAddress"
                    );
                    var offset = getProcAddressAddr.ToInt64() - kernel32Handle.ToInt64();
                    var kernel32 =
                        FindProcessModule(
                            process,
                            module => module.ModuleName.ToLower() == "kernel32.dll"
                        ) ?? throw new Exception("kernel32 module not found");
                    return new IntPtr(kernel32.BaseAddress.ToInt64() + offset);
                }
                default:
                {
                    throw new NotImplementedException();
                }
            }
        }

        public static IntPtr GetFuncAddress(
            IntPtr allocatedAddress,
            IntPtr addressGetter,
            Process process,
            IntPtr moduleHandle,
            string name
        )
        {
            switch (Os)
            {
                case OperatingSystem.WINDOWS:
                {
                    var returnAddress = allocatedAddress;
                    var nameAddress = returnAddress + IntPtr.Size;

                    var nameBytes = ToNativeString(name);
                    WinApi.WriteProcessMemory(
                        process.Handle,
                        nameAddress,
                        nameBytes,
                        (uint)nameBytes.Length,
                        out var nameBytesWritten
                    );

                    var codeAddress = nameAddress + nameBytesWritten;
                    var byteCode = new Asm.X86_64()
                        .Sub(Asm.X86_64.Register.RSP, 0x28)
                        .Mov(Asm.X86_64.Register.RCX, moduleHandle.ToInt64())
                        .Mov(Asm.X86_64.Register.RDX, nameAddress.ToInt64())
                        .Mov(Asm.X86_64.Register.RAX, addressGetter.ToInt64())
                        .Call(Asm.X86_64.Register.RAX)
                        .MovRaxToPtrR12(returnAddress.ToInt64())
                        .Add(Asm.X86_64.Register.RSP, 0x28)
                        .Ret()
                        .GetBytes();

                    WinApi.WriteProcessMemory(
                        process.Handle,
                        codeAddress,
                        byteCode,
                        (uint)byteCode.Length,
                        out var _
                    );

                    var threadHandle = WinApi.CreateRemoteThread(
                        process.Handle,
                        IntPtr.Zero,
                        0,
                        codeAddress,
                        IntPtr.Zero,
                        0,
                        out var _
                    );
                    WinApi.WaitForSingleObject(threadHandle, WinApi.INFINITE);
                    WinApi.CloseHandle(threadHandle);

                    var resultBuffer = new byte[IntPtr.Size];

                    WinApi.ReadProcessMemory(
                        process.Handle,
                        returnAddress,
                        resultBuffer,
                        (uint)resultBuffer.Length,
                        out var _
                    );

                    return new IntPtr(BitConverter.ToInt64(resultBuffer, 0));
                }
                default:
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
