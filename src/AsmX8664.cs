using System;
using System.Collections.Generic;

namespace Urmf
{
    public class AsmX8664
    {
        public enum Register : byte
        {
            RAX = 0,
            RBX = 3,
            RCX = 1,
            RDX = 2,
            RSP = 4,
        }

        private List<byte> _byteCode = new List<byte>();

        public byte[] GetBytes() => _byteCode.ToArray();

        public AsmX8664 Mov(Register register, long value)
        {
            _byteCode.Add(0x48); // REX prefix for 64-bit operand
            _byteCode.Add((byte)(0xB8 | (byte)register)); // MOV r/m64, imm64
            _byteCode.AddRange(BitConverter.GetBytes(value));
            return this;
        }

        public AsmX8664 Add(Register register, int immediate)
        {
            _byteCode.Add(0x48);
            _byteCode.Add(0x81);
            _byteCode.Add((byte)(0xC0 | (byte)register)); // ADD operation with immediate
            _byteCode.AddRange(BitConverter.GetBytes(immediate));
            return this;
        }

        public AsmX8664 Sub(Register register, int immediate)
        {
            _byteCode.Add(0x48);
            _byteCode.Add(0x81);
            _byteCode.Add((byte)(0xE8 | (byte)register)); // SUB operation with immediate
            _byteCode.AddRange(BitConverter.GetBytes(immediate));
            return this;
        }

        public AsmX8664 Call(Register register)
        {
            _byteCode.Add(0xFF);
            _byteCode.Add((byte)(0xD0 | (byte)register)); // CALL r/m64
            return this;
        }

        public AsmX8664 MovRaxToAddress(ulong destAddress)
        {
            _byteCode.Add(0x48); // REX prefix for 64-bit operand
            _byteCode.Add(0xA3); // MOV RAX
            _byteCode.AddRange(BitConverter.GetBytes(destAddress));
            return this;
        }

        public AsmX8664 Ret()
        {
            _byteCode.Add(0xC3); // RET
            return this;
        }
    }
}
