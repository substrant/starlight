namespace Starlight.Core
{
    public interface IStarlightLaunchParams
    {
        public int FpsCap { get; set; }

        public bool Headless { get; set; }

        public bool Spoof { get; set; }

        public string Hash { get; set; }

        public string Resolution { get; set; }
    }
}
