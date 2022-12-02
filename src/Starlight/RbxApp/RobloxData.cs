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
        public static readonly Pattern TssCallRefSignature = new("55 8B EC 83 E4 F8 83 EC 08 E8 ?? ?? ?? ?? 8D 0C 24");
        public static readonly Pattern TssPtrRefSignature = new("A1 ?? ?? ?? ?? 8B 4D F4"); // push eax, dword ptr [tssObjectPtr]
        public const uint TssCallOffset = 9;

        // UserId static global variable
        // string: "PlayerId=%llu\n", last instruction
        public static readonly Pattern UserIdSignature = new("FF 35 ?? ?? ?? ?? 68 ?? ?? ?? ?? 68 00 04 00 00 50 E8 ?? ?? ?? ?? 8D 8D D4 FA FF FF 83 C4 14 8D 51 01 8A 01");
        public const uint UserIdOffset = 2; // skips to rel32 of push instruction
    }
}
