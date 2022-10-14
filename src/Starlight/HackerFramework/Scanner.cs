using System;
using System.Collections.Generic;

using static HackerFramework.Native;

namespace HackerFramework
{
    public class Pattern
    {
        public readonly int Length;
        
        public readonly byte[] Data;
        
        public readonly bool[] Mask;

        internal bool Compare(ref byte[] buffer, ref int idx)
        {
            for (var i = 0; i < Length; i++)
            {
                if (Mask[i] || buffer[idx + i] == Data[i])
                    continue;

                return false;
            }

            return true;
        }

        public Pattern(byte[] pattern, bool[] mask)
        {
            if (pattern.Length != mask.Length)
                throw new Exception("Pattern and mask must be the same length");

            Data = pattern;
            Mask = mask;
            Length = pattern.Length;
        }

        public Pattern(string patternStr) // "AA BB ?? ?? ?? ?? CC DD"
        {
            var parts = patternStr.Split(' ');
            var pattern = new byte[parts.Length];
            var mask = new bool[parts.Length];

            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "??" || parts[i] == "?")
                {
                    mask[i] = true;
                    continue;
                }
                
                pattern[i] = Convert.ToByte(parts[i], 16);
            }

            Data = pattern;
            Mask = mask;
            Length = pattern.Length;
        }

        public Pattern(string raw, string maskStr) // "\xAA\xBB\x00\x00\x00\x00\xCC\xDD", "xx????xx"
        {
            if (raw.Length != maskStr.Length)
                throw new Exception("Pattern and mask must be the same length");

            var pattern = new byte[raw.Length];
            var mask = new bool[raw.Length];
            
            for (var i = 0; i < raw.Length; i++)
            {
                if (maskStr[i] != '?')
                    pattern[i] = (byte)raw[i];
                else
                    mask[i] = true;
            }

            Data = pattern;
            Mask = mask;
            Length = pattern.Length;
        }
    }
    
    public static class Scanner
    {
        // scan for a pattern in memory
        public static IReadOnlyList<uint> FindPattern(this Target target, Pattern pattern, VirtualRange<uint> range = null)
        {
            List<uint> results = new();
            range ??= new(target.ModuleStart, target.ModuleEnd);

            MemoryBasicInformation mbi;
            for (var at = range.Min; at < range.Max; at += (uint)mbi.RegionSize)
            {
                VirtualQueryEx(target.Handle, at, out mbi, 0x2C); // smh 0x2C is sizeof(MemoryBasicInformation) but C# cries about some unsafe crap

                if ((mbi.State & (uint)AllocationType.Commit) == 0) // Make sure the memory is committed
                    continue;

                if ((mbi.Protect & (uint)MemoryProtection.NoAccess) != 0 ||
                    (mbi.Protect & (uint)MemoryProtection.NoCache) != 0 ||
                    (mbi.Protect & (uint)MemoryProtection.Guard) != 0) // Make sure the memory can be accessed
                    continue;
                
                var buffer = target.ReadBytes(at, mbi.RegionSize); // todo: unugly
                for (var i = 0; i < mbi.RegionSize; i++)
                {
                    if (pattern.Compare(ref buffer, ref i))
                        results.Add(at + (uint)i);
                }
            }

            return results;
        }
    }
}
