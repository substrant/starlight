using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Win32;
using Starlight.Apis;
using Starlight.Apis.JoinGame;
using Starlight.Bootstrap;
using Starlight.Launch;
using Starlight.Misc;
using Starlight.PostLaunch;

namespace Starlight.SchemeLaunch;

public class Scheme
{
    /// <summary>
    ///     Parse a Roblox launch scheme payload into <see cref="LaunchParams" />
    /// </summary>
    /// <param name="payload">The raw payload to use.</param>
    /// <returns>A <see cref="LaunchParams" /> class representing the deserialized payload.</returns>
    public static LaunchParams Parse(string payload)
    {
        LaunchParams info = new();
        var result = ParseResultFlags.PayloadExists;

        Dictionary<string, string> args;
        try
        {
            var split = HttpUtility.UrlDecode(payload).Split(' ');
            args = split.Select(t => t.Split(':')).ToDictionary(pair => pair[0], pair => string.Join(":", pair.Skip(1)));
        }
        catch (Exception ex)
        {
            //Logger.Out("Failed to parse payload", Level.Warn, ex);
            return null;
        }

        if (args.TryGetValue("gameinfo", out var ticket))
        {
            result |= ParseResultFlags.TicketExists;
            info.AuthStr = ticket;
            info.AuthType = AuthType.Ticket;
        }
        else
        {
            //Logger.Out("'gameinfo' doesn't exist", Level.Warn);
        }

        if (args.TryGetValue("placelauncherurl", out var launchUrl))
        {
            result |= ParseResultFlags.RequestExists;
            if (Uri.TryCreate(launchUrl, UriKind.Absolute, out var launchUri))
            {
                result |= ParseResultFlags.RequestParsed;
                info.Request = new JoinRequest(launchUri);
            }
            else
            {
                //Logger.Out("'placelauncherurl' couldn't be parsed", Level.Warn);
            }
        }
        else
        {
            //Logger.Out("'placelauncherurl' doesn't exist", Level.Warn);
        }

        if (args.TryGetValue("launchtime", out var launchTimeStr))
        {
            result |= ParseResultFlags.LaunchTimeExists;
            if (long.TryParse(launchTimeStr, out var launchTime))
            {
                result |= ParseResultFlags.LaunchTimeParsed;
                info.LaunchTime = DateTimeOffset.FromUnixTimeMilliseconds(launchTime);
            }
            else
            {
                //Logger.Out("'launchtime' couldn't be parsed", Level.Warn);
            }
        }
        else
        {
            //Logger.Out("'launchtime' doesn't exist", Level.Warn);
        }

        if (args.TryGetValue("browsertrackerid", out var trackerIdStr))
        {
            result |= ParseResultFlags.TrackerIdExists;
            if (long.TryParse(trackerIdStr, out var trackerId))
            {
                result |= ParseResultFlags.TrackerIdParsed;
                info.Request.BrowserTrackerId = trackerId;
            }
            else
            {
                //Logger.Out("'browsertrackerid' couldn't be parsed", Level.Warn);
            }
        }
        else
        {
            //Logger.Out("'browsertrackerid' doesn't exist", Level.Warn);
        }

        if (args.TryGetValue("robloxLocale", out var rbxLocaleStr))
        {
            result |= ParseResultFlags.RobloxLocaleExists;
            if (Utility.TryGetCultureInfo(rbxLocaleStr, out var rbxLocale))
            {
                result |= ParseResultFlags.RobloxLocaleParsed;
                info.RobloxLocale = rbxLocale;
            }
            else
            {
                //Logger.Out("'robloxLocale' couldn't be parsed", Level.Warn);
            }
        }
        else
        {
            //Logger.Out("'browsertrackerid' doesn't exist", Level.Warn);
        }

        if (args.TryGetValue("gameLocale", out var gameLocaleStr))
        {
            result |= ParseResultFlags.GameLocaleExists;
            if (Utility.TryGetCultureInfo(gameLocaleStr, out var gameLocale))
            {
                result |= ParseResultFlags.GameLocaleParsed;
                info.GameLocale = gameLocale;
            }
            else
            {
                //Logger.Out("'gameLocale' couldn't be parsed", Level.Warn);
            }
        }
        else
        {
            //Logger.Out("'gameLocale' doesn't exist", Level.Warn);
        }

        return result.HasFlag(ParseResultFlags.Success) ? info : null;
    }
    
    public static void Hook()
    {
        //var latestClient = 
        //Bootstrapper.RegisterClass();
    }
    
    public static void Unhook()
    {
        var client = Bootstrapper.QueryClientDesperate();
        if (client is not null)
        {
            //Logger.Out($"Registering client version{client.VersionHash}", Level.Info);
            Bootstrapper.RegisterClass(client);
            Bootstrapper.RegisterClient(client);
        }
        else
        {
            //Logger.Out("Unregistering client", Level.Info);
            Bootstrapper.UnregisterClass();
            Bootstrapper.UnregisterClient();
        }
    }
}