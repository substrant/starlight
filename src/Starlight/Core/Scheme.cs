using log4net;
using Microsoft.Win32;
using Starlight.Misc;
using Starlight.Rbx.JoinGame;
using Starlight.RbxApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Starlight.Core
{
    [Flags]
    enum ParseResultFlags
    {
        TicketExists       = 0x0,
        RequestExists      = 0x2,
        RequestParsed      = 0x4,
        LaunchTimeExists   = 0x8,
        LaunchTimeParsed   = 0x10,
        TrackerIdExists    = 0x20,
        TrackerIdParsed    = 0x40,
        RobloxLocaleExists = 0x80,
        RobloxLocaleParsed = 0x100,
        GameLocaleExists   = 0x200,
        GameLocaleParsed   = 0x300,
        Success = TicketExists | RequestExists | RequestParsed | LaunchTimeExists
            | LaunchTimeParsed | TrackerIdExists | TrackerIdParsed | RobloxLocaleExists
            | RobloxLocaleParsed | GameLocaleExists | GameLocaleParsed
    }

    public class Scheme
    {
        // ReSharper disable once PossibleNullReferenceException
        internal static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        internal static IReadOnlyDictionary<string, string> ParseRaw(string payload)
        {
            try
            {
                var split = HttpUtility.UrlDecode(payload).Split(' ');
                return split.Select(t => t.Split(':')).ToDictionary(pair => pair[0], pair => string.Join(":", pair.Skip(1)));
            }
            catch
            {
                Log.Error("Failed to parse raw payload data.");
                return null;
            }
        }

        public static LaunchParams Parse(string rawArgs)
        {
            LaunchParams info = new();
            var args = ParseRaw(rawArgs);
            ParseResultFlags result = 0;
            
            if (args.TryGetValue("gameinfo", out var ticket))
            {
                result |= ParseResultFlags.TicketExists;
                info.Ticket = ticket;
            }

            if (args.TryGetValue("placelauncherurl", out var launchUrl))
            {
                result |= ParseResultFlags.RequestExists;
                if (Uri.TryCreate(launchUrl, UriKind.Absolute, out var launchUri))
                {
                    result |= ParseResultFlags.RequestParsed;
                    info.Request = new JoinRequest(launchUri);
                }
            }

            if (args.TryGetValue("launchtime", out var launchTimeStr))
            {
                result |= ParseResultFlags.LaunchTimeExists;
                if (long.TryParse(launchTimeStr, out var launchTime))
                {
                    result |= ParseResultFlags.LaunchTimeParsed;
                    info.LaunchTime = DateTimeOffset.FromUnixTimeMilliseconds(launchTime);
                }
            }

            if (args.TryGetValue("browsertrackerid", out var trackerIdStr))
            {
                result |= ParseResultFlags.TrackerIdExists;
                if (long.TryParse(trackerIdStr, out var trackerId))
                {
                    result |= ParseResultFlags.TrackerIdParsed;
                    info.TrackerId = trackerId;
                }
            }

            if (args.TryGetValue("robloxLocale", out var rbxLocaleStr))
            {
                result |= ParseResultFlags.RobloxLocaleExists;
                if (Utility.TryGetCultureInfo(rbxLocaleStr, out var rbxLocale))
                {
                    result |= ParseResultFlags.RobloxLocaleParsed;
                    info.RobloxLocale = rbxLocale;
                }
            }

            if (args.TryGetValue("gameLocale", out var gameLocaleStr))
            {
                result |= ParseResultFlags.GameLocaleExists;
                if (Utility.TryGetCultureInfo(gameLocaleStr, out var gameLocale))
                {
                    result |= ParseResultFlags.GameLocaleParsed;
                    info.GameLocale = gameLocale;
                }
            }

            if (result.HasFlag(ParseResultFlags.Success))
                return info;

            Log.Error($"Failed to parse scheme payload. Parse result: {result}");
            return null;
        }

        public static async Task<RbxInstance> LaunchAsync(string args, IStarlightLaunchParams extras = null)
        {
            var parsed = Parse(args);

            if (parsed == null)
            {
                var ex = new SchemeParseException("Failed to parse scheme payload.");
                Log.Fatal("Failed to deserialize payload into LaunchParams.", ex);
                throw ex;
            }
            
            return await Launcher.LaunchAsync(parsed, extras);
        }

        public static RbxInstance Launch(string args, IStarlightLaunchParams extras = null) =>
            LaunchAsync(args, extras).Result;

        public static bool Hook(string launcherBin, string options = "")
        {
            try
            {
                using var registryKey = Registry.CurrentUser.CreateSubKey("Software\\Classes\\roblox-player\\shell\\open\\command");
                registryKey?.SetValue(string.Empty, $"\"{launcherBin}\" {options}%1", RegistryValueKind.String);
                return true;
            }
            catch
            {
                Log.Error("Failed to hook scheme.");
                return false;
            }
        }

        public static bool Unhook()
        {
            var clients = Bootstrapper.GetClients();
            if (clients.Count < 1)
            {
                Bootstrapper.RemoveShortcuts();
                return true;
            }

            var rbxBin = clients.First().LauncherPath;
            try
            {
                using var registryKey = Registry.CurrentUser.CreateSubKey("Software\\Classes\\roblox-player\\shell\\open\\command");
                registryKey?.SetValue(string.Empty, $"\"{rbxBin}\" %1", RegistryValueKind.String);
                return true;
            }
            catch
            {
                Log.Error("Failed to unhook scheme.");
                return false;
            }
        }
    }
}
