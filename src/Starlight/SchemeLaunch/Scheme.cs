using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using log4net;
using Microsoft.Win32;
using Starlight.Apis.JoinGame;
using Starlight.Bootstrap;
using Starlight.Except;
using Starlight.Launch;
using Starlight.Misc;
using Starlight.PostLaunch;

namespace Starlight.SchemeLaunch;

public class Scheme
{
    // ReSharper disable once PossibleNullReferenceException
    internal static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    internal static IReadOnlyDictionary<string, string> ParseRaw(string payload)
    {
        try
        {
            var split = HttpUtility.UrlDecode(payload).Split(' ');
            return split.Select(t => t.Split(':'))
                .ToDictionary(pair => pair[0], pair => string.Join(":", pair.Skip(1)));
        }
        catch
        {
            Log.Error("Failed to parse raw payload data.");
            return null;
        }
    }

    /// <summary>
    ///     Parse a Roblox launch scheme payload into <see cref="LaunchParams" />
    /// </summary>
    /// <param name="rawArgs">The raw payload to use.</param>
    /// <returns>A <see cref="LaunchParams" /> class representing the deserialized payload.</returns>
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

    /// <summary>
    ///     Registers the Starlight scheme handler.
    /// </summary>
    /// <param name="launcherBin">The binary path to launch.</param>
    /// <param name="options">Any command line options to provide.</param>
    /// <returns>A boolean determining whether or not the function succeeded.</returns>
    public static bool Hook(string launcherBin, string options = "")
    {
        try
        {
            using var registryKey =
                Registry.CurrentUser.CreateSubKey("Software\\Classes\\roblox-player\\shell\\open\\command");
            registryKey?.SetValue(string.Empty, $"\"{launcherBin}\" {options}%1", RegistryValueKind.String);
            return true;
        }
        catch
        {
            Log.Error("Failed to hook scheme.");
            return false;
        }
    }

    /// <summary>
    ///     Unhook the scheme that Roblox uses to launch.
    /// </summary>
    /// <returns>A boolean determining whether or not the function succeeded.</returns>
    public static bool Unhook()
    {
        try
        {
            var rbxBin = Bootstrapper.GetClients()[0].Player;
            using var registryKey =
                Registry.CurrentUser.CreateSubKey("Software\\Classes\\roblox-player\\shell\\open\\command");
            registryKey?.SetValue(string.Empty, $"\"{rbxBin}\" %1", RegistryValueKind.String);
            return true;
        }
        catch (ClientNotFoundException)
        {
            Registry.CurrentUser.DeleteSubKeyTree("Software\\Classes\\roblox-player\\shell");
            return true;
        }
        catch
        {
            Log.Error("Failed to unhook scheme.");
            return false;
        }
    }
}