// smh i wrote this framework a long time ago but lost it so i wrote it again for starlight - nick

using System;
using System.Diagnostics;

using static HackerFramework.Native;

namespace HackerFramework
{
    public class Target : IDisposable
    {
        public readonly int Handle;
        
        public readonly string Name;
        
        public readonly string FilePath;

        public readonly uint ModuleStart;
        
        public readonly uint ModuleSize;
        
        public readonly uint ModuleEnd;
        
        public static Target FromId(int procId) =>
            new(Process.GetProcessById(procId));

        public Target(Process proc)
        {
            Handle = OpenProcess(ProcessAccess.All, false, proc.Id);
            
            Name = proc.MainModule.ModuleName;
            FilePath = proc.MainModule.FileName;

            ModuleStart = (uint)proc.MainModule.BaseAddress.ToInt32();
            ModuleSize = (uint)proc.MainModule.ModuleMemorySize;
            ModuleEnd = ModuleStart + ModuleSize;
        }

        bool _disposed = false;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            CloseHandle(Handle);
        }

        ~Target() =>
            Dispose();
    }
}
