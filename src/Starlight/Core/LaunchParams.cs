using Starlight.Rbx.JoinGame;
using System;
using System.Globalization;
using System.Text;

namespace Starlight.Core
{
    public class LaunchParams : IRobloxLaunchParams, IStarlightLaunchParams
    {
        public int FpsCap { get; set; }

        public bool Headless { get; set; }

        public bool Spoof { get; set; }
        
        public string Hash { get; set; }
        
        public string Ticket { get; set; }

        public DateTimeOffset LaunchTime { get; set; } = DateTime.Now;

        public string Resolution { get; set; }

        JoinRequest _request;
        public JoinRequest Request
        {
            get => _request;
            set
            {
                if (TrackerId is not null)
                    value.BrowserTrackerId = TrackerId;
                _request = value;
            }
        }

        public long? TrackerId
        {
            get => Request?.BrowserTrackerId;
            set => Request.BrowserTrackerId = value;
        }

        public CultureInfo RobloxLocale { get; set; } = CultureInfo.CurrentCulture;

        public CultureInfo GameLocale { get; set; } = CultureInfo.CurrentCulture;

        public void Merge<T>(T args)
        {
            foreach (var member in typeof(T).GetProperties())
            {
                var value = member.GetValue(args);
                member.SetValue(this, value);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("Ticket=\"REDACTED-FOR-PRIVACY\";"); // you're welcome :)
            foreach (var prop in GetType().GetProperties())
            {
                if (!prop.CanRead || prop.Name is "Ticket" or "Request")
                    continue;

                sb.Append(prop.Name + $"=\"{prop.GetValue(this)}\";");
            }

            return sb.ToString();
        }
    }
}
