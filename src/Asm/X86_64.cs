using System;
using System.Collections.Generic;

namespace Urmf.Asm
{
    public class X86_64
    {
        public enum Register : byte
        {
            RAX = 0,
            RBX = 3,
            RCX = 1,
            RDX = 2,
            RSP = 4,
            RBP = 5,
            RSI = 6,
            RDI = 7,
            R8 = 8,
            R9 = 9,
            R10 = 10,
            R11 = 11,
            R12 = 12,
            R13 = 13,
            R14 = 14,
            R15 = 15,
        }

        private List<byte> _byteCode = new List<byte>();

        public byte[] GetBytes() => _byteCode.ToArray();

        private void AddRexByte(Register dest = Register.RAX, Register source = Register.RAX)
        {
            _byteCode.Add(
                (byte)(0x48 | ((byte)source >= 8 ? 0b100 : 0) | ((byte)dest >= 8 ? 1 : 0))
            );
        }

        public X86_64 Mov(Register register, long value)
        {
            AddRexByte(register);
            _byteCode.Add((byte)(0xB8 | (byte)((byte)register & 0xFF))); // MOV r/m64, imm64
            _byteCode.AddRange(BitConverter.GetBytes(value));
            return this;
        }

        public X86_64 Add(Register register, int immediate)
        {
            AddRexByte(register);
            _byteCode.Add(0x81);
            _byteCode.Add((byte)(0xC0 | (byte)register)); // ADD operation with immediate
            _byteCode.AddRange(BitConverter.GetBytes(immediate));
            return this;
        }

        public X86_64 Sub(Register register, int immediate)
        {
            AddRexByte(register);
            _byteCode.Add(0x81);
            _byteCode.Add((byte)(0xE8 | (byte)register)); // SUB operation with immediate
            _byteCode.AddRange(BitConverter.GetBytes(immediate));
            return this;
        }

        public X86_64 Call(Register register)
        {
            _byteCode.Add(0xFF);
            _byteCode.Add((byte)(0xD0 | (byte)register)); // CALL r/m64
            return this;
        }

        public X86_64 MovRaxToAddress(long destAddress)
        {
            AddRexByte();
            _byteCode.Add(0xA3); // MOV RAX
            _byteCode.AddRange(BitConverter.GetBytes(destAddress));
            return this;
        }

        // public X86_64 MovPtr(Register dest, Register source)
        // {
        //     AddRexByte(dest, source);
        //     _byteCode.Add(0x89); // MOV r/m64, r64
        //     _byteCode.Add((byte)(((byte)source << 3) | 0b100));
        //     _byteCode.Add(0x24);
        //     return this;
        // }

        public X86_64 MovRaxToPtrR12(long destAddress)
        {
            Mov(Register.R12, (long)destAddress);
            AddRexByte(Register.R12);
            _byteCode.Add(0x89); // MOV r/m64, r64
            _byteCode.Add(0x04);
            _byteCode.Add(0x24);
            return this;
        }

        public X86_64 Ret()
        {
            _byteCode.Add(0xC3); // RET
            return this;
        }
    }
}
