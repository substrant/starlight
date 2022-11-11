using System;

namespace Starlight.SchemeLaunch;

[Flags]
internal enum ParseResultFlags
{
    PayloadExists      = 0x0,
    TicketExists       = 0x2,
    RequestExists      = 0x4,
    RequestParsed      = 0x8,
    LaunchTimeExists   = 0x10,
    LaunchTimeParsed   = 0x20,
    TrackerIdExists    = 0x40,
    TrackerIdParsed    = 0x80,
    RobloxLocaleExists = 0x100,
    RobloxLocaleParsed = 0x200,
    GameLocaleExists   = 0x400,
    GameLocaleParsed   = 0x800,
    Success = TicketExists | RequestExists | RequestParsed | LaunchTimeExists
              | LaunchTimeParsed | TrackerIdExists | TrackerIdParsed | RobloxLocaleExists
              | RobloxLocaleParsed | GameLocaleExists | GameLocaleParsed
}