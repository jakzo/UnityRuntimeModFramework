using System;
using System.Linq;

namespace Urmf
{
    public static class Asm
    {
        public static byte[] Mov(ulong address) => new byte[] { 0x48, 0xA1 };

        public static byte[] GetProcAddress(
            ulong getAddressProc,
            ulong hModule,
            ulong procName,
            ulong result
        )
        {
            // Convert the ulong values to byte arrays
            byte[] getAddressProcBytes = BitConverter.GetBytes(getAddressProc);
            byte[] hModuleBytes = BitConverter.GetBytes(hModule);
            byte[] procNameBytes = BitConverter.GetBytes(procName);
            byte[] resultBytes = BitConverter.GetBytes(result);

            return new byte[]
            {
                // mov rax, [getAddressProc]
                0x48,
                0xA1,
                getAddressProcBytes[0],
                getAddressProcBytes[1],
                getAddressProcBytes[2],
                getAddressProcBytes[3],
                getAddressProcBytes[4],
                getAddressProcBytes[5],
                getAddressProcBytes[6],
                getAddressProcBytes[7],
                // mov rdx, [hModule]
                0x48,
                0xA3,
                hModuleBytes[0],
                hModuleBytes[1],
                hModuleBytes[2],
                hModuleBytes[3],
                hModuleBytes[4],
                hModuleBytes[5],
                hModuleBytes[6],
                hModuleBytes[7],
                // mov rcx, [procName]
                0x48,
                0x8B,
                0x0D,
                procNameBytes[0],
                procNameBytes[1],
                procNameBytes[2],
                procNameBytes[3],
                procNameBytes[4],
                procNameBytes[5],
                procNameBytes[6],
                // call rax
                0xFF,
                0xD0,
                // mov [result], rax
                0x48,
                0xA3,
                resultBytes[0],
                resultBytes[1],
                resultBytes[2],
                resultBytes[3],
                resultBytes[4],
                resultBytes[5],
                resultBytes[6],
                resultBytes[7],
                // ret
                0xC3
            };
        }
    }
}
