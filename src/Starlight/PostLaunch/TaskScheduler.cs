using HackerFramework;

namespace Starlight.PostLaunch;

public class TaskScheduler
{
    internal uint BaseAddress;

    internal ClientInstance Instance;

    public void WriteDouble(uint offset, double value)
    {
        var addr = BaseAddress + offset;
        Instance.Rbx.WriteDouble(addr, value);
    }
}