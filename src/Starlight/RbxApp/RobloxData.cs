using HackerFramework;

namespace Starlight.RbxApp
{
    // The signatures and offsets stored here may need to be
    // updated if Roblox chooses to change their code, which
    // is highly unlikely to happen.

    internal class RobloxData
    {
        // TaskScheduler::singleton
        // string: "Load ClientAppSettings", last four calls
        public static readonly Pattern TssCallRefSignature = new("55 8B EC 83 EC 10 56 E8 ?? ?? ?? ?? 8B F0 8D 45 F0");
        public static readonly Pattern TssPtrRefSignature = new("A1 ?? ?? ?? ?? 8B 4D F4"); // push eax, dword ptr [tssObjectPtr]
        public const uint TssCallOffset = 7;

        // UserId static global variable
        // string: "PlayerId=%llu\n", last instruction
        public static readonly Pattern UserIdSignature = new("FF 35 ?? ?? ?? ?? 68 ?? ?? ?? ?? 68 00 04 00 00 50 E8 ?? ?? ?? ?? 8D 8D D4 FA FF FF 83 C4 14 8D 51 01 8A 01");
        public const uint UserIdOffset = 2; // skips to rel32 of push instruction
    }
}
