using Newtonsoft.Json;
using Starlight.Launch;
using Starlight.PostLaunch;

namespace Starlight.Launcher;

internal class LaunchParamsConfig : IStarlightLaunchParams
{
    [JsonProperty("saveLog")] public bool SaveLog { get; set; }

    [JsonProperty("verbose")] public bool Verbose { get; set; }

    [JsonProperty("attachMethod")] public AttachMethod AttachMethod { get; set; }

    [JsonProperty("fpsCap")] public int FpsCap { get; set; }

    [JsonProperty("resolution")] public string Resolution { get; set; }

    [JsonProperty("headless")] public bool Headless { get; set; }

    [JsonProperty("spoof")] public bool Spoof { get; set; }

    [JsonProperty("hash")] public string Hash { get; set; }
}