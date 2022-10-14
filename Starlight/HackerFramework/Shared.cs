namespace HackerFramework
{
    public class VirtualRange<T> where T : unmanaged
    {
        public readonly T Min;
        public readonly T Max;

        public VirtualRange(T min, T max)
        {
            Min = min;
            Max = max;
        }
    }
}
