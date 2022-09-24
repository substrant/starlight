using System;
using System.Threading;

using static Starlight.Shared;
using HackerFramework;

namespace Starlight.RBX
{
    internal class TaskScheduler
    {
        // TaskScheduler::singleton
        // string: "Load ClientAppSettings", last four calls
        readonly static Pattern SingletonSig = new("55 8B EC 64 A1 00 00 00 00 6A FF 68 ?? ?? ?? ?? 50 64 89 25 00 00 00 00 83 EC 14 64 A1 2C 00 00 00 8B 08 A1 ?? ?? ?? ?? 3B 81 08 00 00 00 7F 29 A1 ?? ?? ?? ?? 8B 4D F4 64 89 0D 00 00 00 00 8B E5 5D C3 8D 4D E4 E8 ?? ?? ?? ?? 68 ?? ?? ?? ?? 8D 45 E4 50 E8 ?? ?? ?? ?? 68 ?? ?? ?? ?? E8 ?? ?? ?? ?? 83 C4 04 83 3D ?? ?? ?? ?? ?? 75 C1 68");
        readonly static uint SingletonPtrOff = 49; // mov eax, dword_xxxxxxxx, should appear twice

        static uint SingletonPtr = 0u;

        static uint FrameDelay = 0u;
        public double FramesPerSecond
        {
            get => Rbx.ReadDouble(BaseAddress + FrameDelay) * 60;
            set => Rbx.WriteDouble(BaseAddress + FrameDelay, 1 / value);
        }
        
        readonly uint BaseAddress;

        public TaskScheduler(uint addr)
        {
            BaseAddress = addr;
        }
        
        static void Initialize()
        {
            uint taskScheduler;

            // Get singleton function
            var results = Rbx.FindPattern(SingletonSig);
            if (results.Count == 0 || results.Count > 1)
                throw new Exception("TaskScheduler::singleton signature update required.");
            
            // Get singleton object pointer
            SingletonPtr = Rbx.ReadPointer(results[0] + SingletonPtrOff);

            // Wait if needed
            while ((taskScheduler = Rbx.ReadPointer(SingletonPtr)) == 0)
                Thread.Sleep(100);

            /* Dump offsets */

            // FrameDelay
            // Basically scans the TaskScheduler object for a 60'th (1/60th) of a second as a double value, not good method but works :cOol:
            const double delay = 1.0 / 60.0; // 60fps lock
            for (uint i = taskScheduler; i < taskScheduler + 0x1000 - sizeof(double); i += 4)
            {
                double diff = Rbx.ReadDouble(i) - delay;
                if (Math.Abs(diff) < 0.001)
                {
                    FrameDelay = i - taskScheduler;
                    break;
                }
            }

            if (FrameDelay == 0)
                throw new Exception("Failed to find FrameDelay offset.");
        }

        public static TaskScheduler Singleton()
        {
            if (FrameDelay == 0) // Initialize if not yet
                Initialize();
            return new(Rbx.ReadPointer(SingletonPtr));
        }
    }
}
