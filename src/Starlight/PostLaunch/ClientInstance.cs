using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HackerFramework;
using log4net;
using Starlight.Except;
using sxlib;
using sxlib.Specialized;

namespace Starlight.PostLaunch;

public class ClientInstance
{
    // ReSharper disable once PossibleNullReferenceException
    internal static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    static SxLibOffscreen _libSx;

    public readonly Process Proc;

    public readonly Target Rbx;

    uint _frameDelayOff;

    TaskScheduler _taskScheduler;

    long _userId;

    public ClientInstance(int procId)
    {
        Proc = Process.GetProcessById(procId);
        if (Proc is null || Proc.HasExited)
        {
            var ex = new PrematureCloseException();
            Log.Fatal("Roblox prematurely exited.", ex);
            throw ex;
        }

        Rbx = new Target(Proc);
    }

    public long GetUserId()
    {
        if (_userId != 0)
            return _userId;

        var results = Rbx.FindPattern(RobloxData.UserIdSignature);
        if (results.Count is 0 or > 1)
        {
            var ex = new PostLaunchException("Failed to find UserId.");
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

    public async Task<long> GetUserIdAsync()
    {
        return await Task.Run(GetUserId);
    }

    public TaskScheduler GetTaskScheduler()
    {
        if (_taskScheduler is not null)
            return _taskScheduler;

        var results = Rbx.FindPattern(RobloxData.TaskSchedulerSignature);
        if (results.Count is 0 or > 1)
        {
            var ex = new PostLaunchException("Failed to find TaskScheduler.");
            Log.Fatal("TaskScheduler signature scan failed. Signature update required.", ex);
            throw ex;
        }

        var sched = new TaskScheduler { Instance = this };
        var singletonPtr = Rbx.ReadPointer(results[0] + RobloxData.TaskSchedulerOffset);
        while ((sched.BaseAddress = Rbx.ReadPointer(singletonPtr)) == 0)
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
                var diff = Rbx.ReadDouble(off) - defaultDelay;
                if (!(Math.Abs(diff) < 0.001))
                    continue;

                _frameDelayOff = off - sched.BaseAddress;
                break;
            }

            if (_frameDelayOff == 0)
            {
                var ex = new PostLaunchException("Failed to find FrameDelay.");
                Log.Fatal(
                    "FrameDelay offset scan failed. Possibly an invalid TaskScheduler object. Signature update MAY be required.",
                    ex);
                throw ex;
            }
        }

        sched.WriteDouble(_frameDelayOff, delay);
    }

    public async Task SetFrameDelayAsync(double delay)
    {
        await Task.Run(() => SetFrameDelay(delay));
    }

    static void SynInit()
    {
        if (_libSx is not null)
            return;

        var sxPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..");
        PostLaunchException ex;

        if (!File.Exists(Path.Combine(sxPath, "Synapse Launcher.exe")))
        {
            ex = new PostLaunchException("Could not attach; Synapse X directory not found.");
            Log.Fatal("Could not attach; Synapse X directory not found.", ex);
            throw ex;
        }

        _libSx = SxLib.InitializeOffscreen(sxPath);

        using var wh = new ManualResetEvent(false);
        _libSx.LoadEvent += (e, _) =>
        {
            switch (e)
            {
                case SxLibBase.SynLoadEvents.READY:
                    wh.Set();
                    break;
                case SxLibBase.SynLoadEvents.UNKNOWN:
                    throw new PostLaunchException("Unknown Synapse error.");
                case SxLibBase.SynLoadEvents.NOT_LOGGED_IN:
                    throw new PostLaunchException("Not logged into Synapse.");
                case SxLibBase.SynLoadEvents.NOT_UPDATED:
                    throw new PostLaunchException("Synapse has not updated yet.");
                case SxLibBase.SynLoadEvents.FAILED_TO_VERIFY:
                    throw new PostLaunchException("Couldn't verify Synapse ownership.");
                case SxLibBase.SynLoadEvents.FAILED_TO_DOWNLOAD:
                    throw new PostLaunchException("Failed to download Synapse DLLs.");
                case SxLibBase.SynLoadEvents.UNAUTHORIZED_HWID:
                    throw new PostLaunchException("You are not authorized to use Synapse.");
                case SxLibBase.SynLoadEvents.ALREADY_EXISTING_WL:
                    throw new PostLaunchException("what");
                case SxLibBase.SynLoadEvents.NOT_ENOUGH_TIME:
                    throw new PostLaunchException("Synapse initialization timed out.");
                case SxLibBase.SynLoadEvents.CHECKING_WL:
                    break;
                case SxLibBase.SynLoadEvents.CHANGING_WL:
                    break;
                case SxLibBase.SynLoadEvents.DOWNLOADING_DATA:
                    break;
                case SxLibBase.SynLoadEvents.CHECKING_DATA:
                    break;
                case SxLibBase.SynLoadEvents.DOWNLOADING_DLLS:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e), e, null);
            }
        };

        _libSx.Load();
        if (wh.WaitOne(TimeSpan.FromSeconds(60)))
            return;

        ex = new PostLaunchException("Synapse X init timed out.");
        Log.Fatal("Synapse X init timed out.", ex);
        throw ex;
    }

    static void SynAttach()
    {
        SynInit();
        if (_libSx.Attach())
            return;

        var ex = new PostLaunchException("Synapse X failed to attach.");
        Log.Fatal("Synapse X failed to attach.", ex);
        throw ex;
    }

    public void Attach(AttachMethod method)
    {
        switch (method)
        {
            case AttachMethod.Synapse:
                SynAttach();
                break;
            case AttachMethod.None:
            default:
                throw new NotImplementedException();
        }
    }

    public async Task AttachAsync(AttachMethod method) =>
        await Task.Run(() => Attach(method));

    ~ClientInstance()
    {
        Rbx.Dispose();
    }
}