using System.Collections.Generic;

namespace HackerFramework
{
    public static class Disasm
    {
        // Determines if the given address initializes a stack frame
        public static bool PrologueAt(this Target target, uint addr)
        {
            var bytes = target.ReadBytes(addr, 3);
            
            if (bytes[2] != 0x8B) // Not a "mov r32, r32" instruction
                return false;
            
            return
                (bytes[0] == 0x55 && bytes[2] == 0xEC) || // push ebp; mov ebp, esp
                (bytes[0] == 0x53 && bytes[2] == 0xDC) || // push ebx; mov ebx, esp
                (bytes[0] == 0x56 && bytes[2] == 0xF4);   // push esi; mov esi, esp
        }

        /*
        * A cdecl function will let the callee clean up the stack, meaning there will be a "ret" instruction
        * Generally, an stdcall function will clean up the stack, hence the "ret imm16" instruction, although
        * that can be mutated to a lot of things, such as popping into junk registers or adding onto esp,
        * which I won't take into account here.
        */

        // Determines if the given address cleans up a stack frame
        public static bool EpilogueAt(this Target target, uint addr)
        {
            var bytes = target.ReadBytes(addr - 1, 3);

            if (bytes[0] != 0x5D && bytes[0] != 0xC9) // Not a "pop ebp" or "leave" instruction
                return false;

            if (bytes[1] == 0xC3) // ret
                return true;
            else if (bytes[1] == 0xC2) // ret imm16
            {
                var imm = target.ReadUShort(addr + 1);
                return imm % 4 == 0; // Stack is aligned to 4 bytes, so the imm reg must be a multiple of 4.
            }
            
            return false;
        }

        public static uint GetPrologue(this Target target, uint addr) =>
            target.PrologueAt(addr) ? addr : target.LastPrologue(addr);

        public static uint GetEpilogue(this Target target, uint addr) =>
            target.EpilogueAt(addr) ? addr : target.NextEpilogue(addr);

        public static uint NextPrologue(this Target target, uint addr)
        {
            if (target.PrologueAt(addr))
                addr += 16;

            if (addr % 16 == 0)
                addr += 16 - (addr % 16);

            while (!target.PrologueAt(addr))
                addr += 16;

            return addr;
        }

        public static uint LastPrologue(this Target target, uint addr)
        {
            if (target.PrologueAt(addr))
                addr -= 16;

            if (addr % 16 != 0)
                addr -= (addr % 16);

            while (!target.PrologueAt(addr))
                addr -= 16;

            return addr;
        }
        
        public static uint NextEpilogue(this Target target, uint addr)
        {
            if (target.EpilogueAt(addr))
                addr++;

            while (!target.EpilogueAt(addr))
                addr++;

            return addr;
        }

        public static uint LastEpilogue(this Target target, uint addr)
        {
            if (target.EpilogueAt(addr))
                addr--;

            while (!target.EpilogueAt(addr))
                addr--;
            
            return addr;
        }

        public static uint Follow(this Target target, uint addr) =>
            addr + 5 + target.ReadPointer(addr + 1);

        // Return the followed address of the call or zero if it's invalid
        public static uint CallAt(this Target target, uint addr)
        {
            var iAt = target.ReadByte(addr);
            if (iAt != 0xE8) // Not a "call" instruction
                return 0;

            var dest = target.Follow(addr);
            if (dest % 16 != 0) // Check if the call's dest is aligned
                return 0;

            if (dest >= target.ModuleStart && dest <= target.ModuleEnd) // Check if the call's dest is within the same module
                return dest;

            return 0;
        }

        public static IReadOnlyList<uint> GetCalls(this Target target, uint addr, int max)
        {
            List<uint> calls = new();

            var end = target.GetEpilogue(addr) - 5;
            for (var at = addr; at < end || calls.Count == max; at++)
            {
                var rel = target.CallAt(at);
                if (rel != 0)
                    calls.Add(at);
            }

            return calls;
        }

        public static uint NextCall(this Target target, uint addr, bool follow = false)
        {
            while (target.CallAt(addr) == 0)
                addr++;

            if (follow)
                return target.CallAt(addr);

            return addr;
        }

        public static uint LastCall(this Target target, uint addr, bool follow = false)
        {
            while (target.CallAt(addr) == 0)
                addr--;

            if (follow)
                return target.CallAt(addr);

            return addr;
        }

        public static uint NextNCalls(this Target target, uint addr, int n)
        {
            for (var i = 0; i < n; i++)
                addr = target.NextCall(addr);

            return addr;
        }

        public static uint LastNCalls(this Target target, uint addr, int n)
        {
            for (var i = 0; i < n; i++)
                addr = target.LastCall(addr);

            return addr;
        }
    }
}
