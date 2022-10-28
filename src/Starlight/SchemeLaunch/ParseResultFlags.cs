using System;

namespace Starlight.SchemeLaunch;

[Flags]
internal enum ParseResultFlags
{
    TicketExists = 0x0,
    RequestExists = 0x2,
    RequestParsed = 0x4,
    LaunchTimeExists = 0x8,
    LaunchTimeParsed = 0x10,
    TrackerIdExists = 0x20,
    TrackerIdParsed = 0x40,
    RobloxLocaleExists = 0x80,
    RobloxLocaleParsed = 0x100,
    GameLocaleExists = 0x200,
    GameLocaleParsed = 0x300,

    Success = TicketExists | RequestExists | RequestParsed | LaunchTimeExists
              | LaunchTimeParsed | TrackerIdExists | TrackerIdParsed | RobloxLocaleExists
              | RobloxLocaleParsed | GameLocaleExists | GameLocaleParsed
}