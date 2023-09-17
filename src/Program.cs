using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;

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
            var getProcAddressAddr = FindGetProcAddress(process);

            foreach (ProcessModule module in process.Modules)
            {
                if (module.BaseAddress == (IntPtr)140713400008704)
                {
                    Console.WriteLine($"found {module.ModuleName} - {module.FileName}");
                }
                var moduleHandle = WinApi.GetModuleHandle(module.ModuleName);
                if (module.BaseAddress == (IntPtr)140713400008704)
                {
                    Console.WriteLine(
                        $"moduleHandle = {moduleHandle}, err = {Marshal.GetLastWin32Error()}"
                    );
                }

                // TODO:
                // - Write assembly code which calls GetProcAddress and writes the result at 0x400
                //   and reads args from 0x408 and 0x410
                // - Compile the code
                // - Make a template function which returns the compiled bytes and adds the args at
                //   0x408 and 0x410
                // - Allocate virtual memory inside the process
                // - Write the templated bytes into the memory
                // - Create a remote thread and call the function
                // -
                var result = WinApi.GetProcAddress(moduleHandle, "il2cpp_thread_attach");
                if (module.BaseAddress == (IntPtr)140713400008704)
                {
                    var err = Marshal.GetLastWin32Error();
                    Console.WriteLine(
                        $"result = {result}, err = {err}, is64 = {Environment.Is64BitProcess}"
                    );
                }
                if (result != IntPtr.Zero)
                {
                    Console.WriteLine(
                        $"{module.ModuleName} - {module.FileName}, result = {result}"
                    );
                }
            }
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
            var kernel32Handle = WinApi.LoadLibrary("kernel32.dll");
            var getProcAddressAddr = WinApi.GetProcAddress(kernel32Handle, "GetProcAddress");
            Console.WriteLine($"getProcAddressAddr = {getProcAddressAddr}");
            var offset = getProcAddressAddr.ToInt64() - kernel32Handle.ToInt64();
            var kernel32 = FindProcessModule(
                process,
                module => module.ModuleName.ToLower() == "kernel32.dll"
            );
            if (kernel32 == null)
                throw new Exception("kernel32 module not found");
            return new IntPtr(kernel32.BaseAddress.ToInt64() + offset);
        }
    }
}
