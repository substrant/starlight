using System;

using static HackerFramework.Native;

namespace HackerFramework
{
    public static class Memory
    {
        public static uint Rebase(this Target target, uint addr, uint bAddr = 0) =>
            target.ModuleStart + addr - bAddr;

        public static uint Unbase(this Target target, uint addr, uint bAddr = 0) =>
            addr -  target.ModuleStart + bAddr;

        /* Generic */

        public static uint Allocate(this Target target, int size, MemoryProtection protect = MemoryProtection.ReadWriteExecute) =>
            VirtualAllocEx(target.Handle, 0, size, AllocationType.Commit | AllocationType.Reserve, protect);

        public static bool Free(this Target target, uint addr) =>
            VirtualFreeEx(target.Handle, addr, 0, AllocationType.Release);

        public static MemoryProtection Protect(this Target target, uint address, int size, MemoryProtection protection)
        {
            VirtualProtectEx(target.Handle, address, size, protection, out var old);
            return old;
        }

        /* int8 */

        public static byte[] ReadBytes(this Target target, uint addr, int size)
        {
            var buf = new byte[size];
            ReadProcessMemory(target.Handle, addr, buf, size, out _);
            return buf;
        }

        public static byte ReadByte(this Target target, uint addr) =>
            ReadBytes(target, addr, 1)[0];

        public static void WriteBytes(this Target target, uint addr, byte[] value) =>
            WriteProcessMemory(target.Handle, addr, value, value.Length, out _);

        public static void WriteByte(this Target target, uint addr, byte value) =>
            WriteBytes(target, addr, new[] { value });

        /* int16 */

        public static ushort ReadUShort(this Target target, uint addr) =>
           BitConverter.ToUInt16(ReadBytes(target, addr, 2), 0);

        public static short ReadShort(this Target target, uint addr) =>
            BitConverter.ToInt16(ReadBytes(target, addr, 2), 0);

        public static void WriteUShort(this Target target, uint addr, ushort value) =>
            WriteBytes(target, addr, BitConverter.GetBytes(value));

        public static void WriteShort(this Target target, uint addr, short value) =>
            WriteBytes(target, addr, BitConverter.GetBytes(value));

        /* int32 */

        public static uint ReadPointer(this Target target, uint addr) => // remember: x86 process
           BitConverter.ToUInt32(ReadBytes(target, addr, 4), 0);

        public static uint ReadUInt(this Target target, uint addr) =>
           BitConverter.ToUInt32(ReadBytes(target, addr, 4), 0);

        public static int ReadInt(this Target target, uint addr) =>
           BitConverter.ToInt32(ReadBytes(target, addr, 4), 0);

        public static void WriteUInt(this Target target, uint addr, uint value) =>
            WriteBytes(target, addr, BitConverter.GetBytes(value));

        public static void WriteInt(this Target target, uint addr, int value) =>
            WriteBytes(target, addr, BitConverter.GetBytes(value));

        /* float */

        public static float ReadFloat(this Target target, uint addr) =>
            BitConverter.ToSingle(ReadBytes(target, addr, 4), 0);

        public static void WriteFloat(this Target target, uint addr, float value) =>
            WriteBytes(target, addr, BitConverter.GetBytes(value));

        /* int64 */

        public static ulong ReadULong(this Target target, uint addr) =>
          BitConverter.ToUInt64(ReadBytes(target, addr, 8), 0);

        public static long ReadLong(this Target target, uint addr) =>
           BitConverter.ToInt64(ReadBytes(target, addr, 8), 0);
        
        public static void WriteULong(this Target target, uint addr, long value) =>
            WriteBytes(target, addr, BitConverter.GetBytes(value));

        public static void WriteLong(this Target target, uint addr, long value) =>
            WriteBytes(target, addr, BitConverter.GetBytes(value));

        /* double */

        public static double ReadDouble(this Target target, uint addr) =>
            BitConverter.ToDouble(ReadBytes(target, addr, 8), 0);

        public static void WriteDouble(this Target target, uint addr, double value) =>
            WriteBytes(target, addr, BitConverter.GetBytes(value));
    }
}
