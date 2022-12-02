using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Starlight.Apis;
using Starlight.Apis.JoinGame;
using Starlight.Bootstrap;
using Starlight.Misc;

namespace Starlight.Launch;

/// <summary>
///     Contains methods for the <c>roblox-player</c> scheme.
/// </summary>
public static class Scheme {
    /// <summary>
    ///     Parse a Roblox launch scheme payload into <see cref="LaunchParams" />.
    /// </summary>
    public static LaunchParams Parse(string payload) {
        LaunchParams info = new();
        var result = ParseResultFlags.PayloadExists;

        Dictionary<string, string> args;

        try {
            var split = HttpUtility.UrlDecode(payload).Split(' ');

            args = split.Select(t => t.Split(':'))
                .ToDictionary(pair => pair[0], pair => string.Join(":", pair.Skip(1)));
        }
        catch (Exception) {
            return null;
        }

        if (args.TryGetValue("gameinfo", out var ticket)) {
            result |= ParseResultFlags.TicketExists;
            info.AuthStr = ticket;
            info.AuthType = AuthType.Ticket;
        }

        if (args.TryGetValue("placelauncherurl", out var launchUrl)) {
            result |= ParseResultFlags.RequestExists;

            if (Uri.TryCreate(launchUrl, UriKind.Absolute, out var launchUri)) {
                result |= ParseResultFlags.RequestParsed;
                info.Request = JoinRequest.FromUri(launchUri);
            }
        }

        if (args.TryGetValue("launchtime", out var launchTimeStr)) {
            result |= ParseResultFlags.LaunchTimeExists;

            if (long.TryParse(launchTimeStr, out var launchTime)) {
                result |= ParseResultFlags.LaunchTimeParsed;
                info.LaunchTime = DateTimeOffset.FromUnixTimeMilliseconds(launchTime);
            }
        }

        if (args.TryGetValue("browsertrackerid", out var trackerIdStr)) {
            result |= ParseResultFlags.TrackerIdExists;

            if (long.TryParse(trackerIdStr, out var trackerId)) {
                result |= ParseResultFlags.TrackerIdParsed;
                info.Request.BrowserTrackerId = trackerId;
            }
        }

        if (args.TryGetValue("robloxLocale", out var rbxLocaleStr)) {
            result |= ParseResultFlags.RobloxLocaleExists;

            if (Utility.TryGetCultureInfo(rbxLocaleStr, out var rbxLocale)) {
                result |= ParseResultFlags.RobloxLocaleParsed;
                info.RobloxLocale = rbxLocale;
            }
        }

        // ReSharper disable InvertIf
        if (args.TryGetValue("gameLocale", out var gameLocaleStr)) {
            result |= ParseResultFlags.GameLocaleExists;

            if (Utility.TryGetCultureInfo(gameLocaleStr, out var gameLocale)) {
                result |= ParseResultFlags.GameLocaleParsed;
                info.GameLocale = gameLocale;
            }
        }
        // ReSharper enable InvertIf

        return result.HasFlag(ParseResultFlags.Success) ? info : null;
    }

    /// <summary>
    ///     Hook the `roblox-player` scheme for the given <see cref="Client" />.
    /// </summary>
    public static void Hook(Client client) {
        Bootstrapper.RegisterClass(client);
    }

    /// <summary>
    ///     Unhook the `roblox-player` scheme.
    /// </summary>
    public static void Unhook() {
        Bootstrapper.UnregisterClass();
    }
}