using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Urmf
{
    class UnityGameContext
    {
        public Process Process;
        public ProcessModule GameAssembly;
        public bool IsIl2cpp;
        public Dictionary<string, UnityNative.Func> FuncList;
    }

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
            var ctx = Init(process) ?? throw new Exception("Failed to find game assembly");
            if (ctx.IsIl2cpp)
            {
                Console.WriteLine($"mono_domain_get = {ctx.FuncList["mono_domain_get"]}");
            }
            else
            {
                throw new NotImplementedException();
            }
            Console.WriteLine($"gameAssembly = {ctx?.GameAssembly.ModuleName ?? "(none)"}");
        }

        static UnityGameContext Init(Process process)
        {
            var nativeAddressGetter = Native.FindFuncAddressGetter(process);
            foreach (ProcessModule module in process.Modules)
            {
                const uint PAGE_SIZE = 4096;
                var allocatedAddress = WinApi.VirtualAllocEx(
                    process.Handle,
                    IntPtr.Zero,
                    PAGE_SIZE,
                    WinApi.AllocationType.MEM_COMMIT | WinApi.AllocationType.MEM_RESERVE,
                    WinApi.Protection.EXECUTE_READ_WRITE
                );

                try
                {
                    Func<string, IntPtr> getAddress = name =>
                        Native.GetFuncAddress(
                            allocatedAddress,
                            nativeAddressGetter,
                            process,
                            module.BaseAddress,
                            name
                        );

                    var isIl2cppAssembly = getAddress("il2cpp_thread_attach") != IntPtr.Zero;
                    if (isIl2cppAssembly)
                    {
                        return new UnityGameContext()
                        {
                            Process = process,
                            GameAssembly = module,
                            IsIl2cpp = true,
                            FuncList = UnityNative.BuildFuncList(getAddress, true),
                        };
                    }

                    var isMonoAssembly = getAddress("mono_thread_attach") != IntPtr.Zero;
                    if (isMonoAssembly)
                    {
                        return new UnityGameContext()
                        {
                            Process = process,
                            GameAssembly = module,
                            IsIl2cpp = false,
                            FuncList = UnityNative.BuildFuncList(getAddress, false),
                        };
                    }
                }
                finally
                {
                    WinApi.VirtualFreeEx(
                        process.Handle,
                        allocatedAddress,
                        0,
                        WinApi.FreeType.MEM_RELEASE
                    );
                }
            }
            return null;
        }
    }
}
