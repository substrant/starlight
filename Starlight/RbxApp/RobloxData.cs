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
        // todo: Maybe find a less retarded signature
        public static readonly Pattern TaskSchedulerSignature = new("55 8B EC 64 A1 00 00 00 00 6A FF 68 ?? ?? ?? ?? 50 64 89 25 00 00 00 00 83 EC 14 64 A1 2C 00 00 00 8B 08 A1 ?? ?? ?? ?? 3B 81 08 00 00 00 7F 29 A1 ?? ?? ?? ?? 8B 4D F4 64 89 0D 00 00 00 00 8B E5 5D C3 8D 4D E4 E8 ?? ?? ?? ?? 68 ?? ?? ?? ?? 8D 45 E4 50 E8 ?? ?? ?? ?? 68 ?? ?? ?? ?? E8 ?? ?? ?? ?? 83 C4 04 83 3D ?? ?? ?? ?? ?? 75 C1 68");
        public const uint TaskSchedulerOffset = 49; // mov eax, dword_xxxxxxxx, instruction should appear twice for same func

        // UserId static global variable
        // string: "PlayerId=%llu\n", last instruction
        public static readonly Pattern UserIdSignature = new("FF 35 ?? ?? ?? ?? 68 ?? ?? ?? ?? 68 00 04 00 00 50 E8 ?? ?? ?? ?? 8D 8D D4 FA FF FF 83 C4 14 8D 51 01 8A 01");
        public const uint UserIdOffset = 2; // skips to rel32 of push instruction
    }
}
