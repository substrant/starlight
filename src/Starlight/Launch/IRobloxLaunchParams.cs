using System;
using System.Globalization;
using Starlight.Apis.JoinGame;

namespace Starlight.Launch;

public interface IRobloxLaunchParams
{
    public string Ticket { get; set; }

    public DateTimeOffset LaunchTime { get; set; }

    public JoinRequest Request { get; set; }

    public CultureInfo RobloxLocale { get; set; }

    public CultureInfo GameLocale { get; set; }
}