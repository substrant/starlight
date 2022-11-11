using HackerFramework;
using log4net;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Starlight.RbxApp
{
    public class RbxInstance
    {
        // ReSharper disable once PossibleNullReferenceException
        internal static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public readonly Process Proc;

        public readonly Target Rbx;

        long _userId;
        public long GetUserId()
        {
            if (_userId != 0)
                return _userId;
            
            var results = Rbx.FindPattern(RobloxData.UserIdSignature);
            if (results.Count is 0 or > 1)
            {
                var ex = new AppModException("Failed to find UserId.");
                Log.Fatal("UserId signature scan failed. Signature update required.", ex);
                throw ex;
            }

            var userIdAddr = Rbx.ReadPointer(results[0] + RobloxData.UserIdOffset);
            long userId;
            while ((userId = Rbx.ReadLong(userIdAddr)) == 0 || userId == -1)
                Thread.Sleep(100);

            _userId = userId;
            return userId;
        }

        public async Task<long> GetUserIdAsync() =>
            await Task.Run(GetUserId);

        TaskScheduler _taskScheduler;
        public TaskScheduler GetTaskScheduler()
        {
            if (_taskScheduler is not null)
                return _taskScheduler;
            
            var results = Rbx.FindPattern(RobloxData.TssCallRefSignature);
            if (results.Count is 0 or > 1)
            {
                var ex = new AppModException("Failed to find TaskScheduler reference.");
                Log.Fatal("TaskScheduler reference signature scan failed. Signature update required.", ex);
                throw ex;
            }
            var tssFunction = Rbx.CallAt(results[0] + RobloxData.TssCallOffset);

            results = Rbx.FindPattern(RobloxData.TssPtrRefSignature, new VirtualRange<uint>(tssFunction, tssFunction + 0x100));
            if (results.Count == 0)
            {
                var ex = new AppModException("Failed to find TaskScheduler pointer.");
                Log.Fatal("TaskScheduler pointer reference signature scan failed. Signature update required.", ex);
                throw ex;
            }
            var singletonPtr = Rbx.ReadPointer(results[0] + 1); // A1 ?? ?? ?? ??

            var sched = new TaskScheduler { Instance = this };
            while ((sched.BaseAddress = Rbx.ReadPointer(singletonPtr)) == 0)
                Thread.Sleep(100);
            
            _taskScheduler = sched;
            return sched;
        }

        public async Task<TaskScheduler> GetTaskSchedulerAsync() =>
            await Task.Run(GetTaskScheduler);

        uint _frameDelayOff;
        public void SetFrameDelay(double delay)
        {
            var sched = GetTaskScheduler();
            if (_frameDelayOff == 0)
            {
                const double defaultDelay = 1.0d / 60.0d; // 60hz
                for (var off = sched.BaseAddress; off < sched.BaseAddress + 0x1000 - sizeof(double); off += 4)
                {
                    var diff = Rbx.ReadDouble(off) - defaultDelay;
                    if (!(Math.Abs(diff) < 0.001))
                        continue;

                    _frameDelayOff = off - sched.BaseAddress;
                    break;
                }

                if (_frameDelayOff == 0)
                {
                    var ex = new AppModException("Failed to find FrameDelay.");
                    Log.Fatal("FrameDelay offset scan failed. Possibly an invalid TaskScheduler object. Signature update MAY be required.", ex);
                    throw ex;
                }
            }
            
            sched.WriteDouble(_frameDelayOff, delay);
        }

        public async Task SetFrameDelayAsync(double delay) =>
            await Task.Run(() => SetFrameDelay(delay));

        public RbxInstance(int procId)
        {
            Proc = Process.GetProcessById(procId);
            if (Proc is null || Proc.HasExited)
            {
                var ex = new AppModException("Failed to get Roblox process.");
                Log.Fatal("Roblox prematurely exited.", ex);
                throw ex;
            }
            Rbx = new Target(Proc);
        }

        ~RbxInstance()
        {
            Rbx.Dispose();
        }
    }
}
