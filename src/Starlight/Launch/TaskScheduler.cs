using HackerFramework;

namespace Starlight.Launch;

public class TaskScheduler
{
    internal uint BaseAddress;

    internal ClientInstance Instance;

    public void WriteDouble(uint offset, double value)
    {
        var addr = BaseAddress + offset;
        Instance.Target.WriteDouble(addr, value);
    }
}