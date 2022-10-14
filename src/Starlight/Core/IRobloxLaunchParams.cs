using System;
using System.Globalization;

namespace Starlight.Core
{
    public interface IRobloxLaunchParams
    {
        public string Ticket { get; set; }

        public DateTimeOffset LaunchTime { get; set; }

        public Rbx.JoinGame.JoinRequest Request { get; set; }

        public CultureInfo RobloxLocale { get; set; }

        public CultureInfo GameLocale { get; set; }
    }
}
