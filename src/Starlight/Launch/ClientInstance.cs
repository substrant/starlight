using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HackerFramework;
using Starlight.Bootstrap;

namespace Starlight.Launch;

/// <summary>
///     Represents a running instance of Roblox.
/// </summary>
public class ClientInstance
{
    /// <summary>
    ///     The process of the running instance.
    /// </summary>
    public readonly Process Proc;

    /// <summary>
    ///     The target of the running instance (HackerFramework).
    /// </summary>
    public readonly Target Target;

    uint _frameDelayOff;
    TaskScheduler _taskScheduler;
    long _userId;

    /// <summary>
    ///     The client of the instance.
    /// </summary>
    public Client Client;

    internal ClientInstance(Client client, Process proc)
    {
        Client = client;
        Proc = proc;
        Target = new Target(Proc);
    }

    /// <summary>
    ///     Get the playing user's ID.
    /// </summary>
    public async Task<long> GetUserIdAsync()
    {
        if (_userId != 0)
            return _userId;

        var results = Target.FindPattern(RobloxData.UserIdSignature);
        if (results.Count is 0 or > 1)
            throw new NotImplementedException();

        var userIdAddr = Target.ReadPointer(results[0] + RobloxData.UserIdOffset);
        long userId;
        while ((userId = Target.ReadLong(userIdAddr)) == 0 || userId == -1)
            await Task.Delay(100);

        _userId = userId;
        return userId;
    }

    /// <summary>
    ///     Get the TaskScheduler.
    /// </summary>
    public async Task<TaskScheduler> GetTaskSchedulerAsync()
    {
        if (_taskScheduler is not null)
            return _taskScheduler;

        var results = Target.FindPattern(RobloxData.TaskSchedulerSignature);
        if (results.Count is 0 or > 1)
            throw new NotImplementedException();

        var sched = new TaskScheduler { Instance = this };
        var singletonPtr = Target.ReadPointer(results[0] + RobloxData.TaskSchedulerOffset);
        while ((sched.BaseAddress = Target.ReadPointer(singletonPtr)) == 0)
            await Task.Delay(100);

        _taskScheduler = sched;
        return sched;
    }

    /// <summary>
    ///     <para>Set the TaskScheduler frame delay.</para>
    ///     <strong>Note:</strong> The delay is in hertz, not frames per second.
    /// </summary>
    public async Task SetFrameDelayAsync(double delay)
    {
        var sched = await GetTaskSchedulerAsync();
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
                throw new NotImplementedException();
        }

        sched.WriteDouble(_frameDelayOff, delay);
    }

    ~ClientInstance()
    {
        Target.Dispose();
    }
}