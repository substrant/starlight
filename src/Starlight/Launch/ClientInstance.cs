using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HackerFramework;
using Starlight.Bootstrap;

namespace Starlight.Launch;

public class ClientInstance
{
    public readonly Process Proc;

    public readonly Target Target;

    uint _frameDelayOff;

    TaskScheduler _taskScheduler;

    long _userId;
    public Client Client;

    internal ClientInstance(Client client, Process proc)
    {
        Client = client;
        Proc = proc;
        Target = new Target(Proc);
    }

    public long GetUserId()
    {
        if (_userId != 0)
            return _userId;

        var results = Target.FindPattern(RobloxData.UserIdSignature);
        if (results.Count is 0 or > 1)
        {
            var ex = new PostLaunchException(this, "Failed to find UserId");
            throw ex;
        }

        var userIdAddr = Target.ReadPointer(results[0] + RobloxData.UserIdOffset);
        long userId;
        while ((userId = Target.ReadLong(userIdAddr)) == 0 || userId == -1)
            Thread.Sleep(100);

        _userId = userId;
        return userId;
    }

    public async Task<long> GetUserIdAsync()
    {
        return await Task.Run(GetUserId);
    }

    public TaskScheduler GetTaskScheduler()
    {
        if (_taskScheduler is not null)
            return _taskScheduler;

        var results = Target.FindPattern(RobloxData.TaskSchedulerSignature);
        if (results.Count is 0 or > 1)
        {
            var ex = new PostLaunchException(this, "Failed to find TaskScheduler");
            throw ex;
        }

        var sched = new TaskScheduler { Instance = this };
        var singletonPtr = Target.ReadPointer(results[0] + RobloxData.TaskSchedulerOffset);
        while ((sched.BaseAddress = Target.ReadPointer(singletonPtr)) == 0)
            Thread.Sleep(100);

        _taskScheduler = sched;
        return sched;
    }

    public async Task<TaskScheduler> GetTaskSchedulerAsync()
    {
        return await Task.Run(GetTaskScheduler);
    }

    public void SetFrameDelay(double delay)
    {
        var sched = GetTaskScheduler();
        if (_frameDelayOff == 0)
        {
            const double defaultDelay = 1.0d / 60.0d; // 60hz
            for (var off = sched.BaseAddress; off < sched.BaseAddress + 0x1000 - sizeof(double); off += 4)
            {
                var diff = Target.ReadDouble(off) - defaultDelay;
                if (!(Math.Abs(diff) < 0.001))
                    continue;

                _frameDelayOff = off - sched.BaseAddress;
                break;
            }

            if (_frameDelayOff == 0)
            {
                var ex = new PostLaunchException(this, "Failed to find FrameDelay");
                throw ex;
            }
        }

        sched.WriteDouble(_frameDelayOff, delay);
    }

    public async Task SetFrameDelayAsync(double delay)
    {
        await Task.Run(() => SetFrameDelay(delay));
    }

    ~ClientInstance()
    {
        Target.Dispose();
    }
}