using HackerFramework;

namespace Starlight.RbxApp
{
    public class TaskScheduler
    {
        internal RbxInstance Instance;

        internal uint BaseAddress;

        public void WriteDouble(uint offset, double value)
        {
            var addr = BaseAddress + offset;
            Instance.Rbx.WriteDouble(addr, value);
        }
    }
}
