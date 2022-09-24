using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.Win32;

namespace Starlight
{
    public class Scheme
    {
        internal static IReadOnlyDictionary<string, string> ParseRaw(string payload)
        {
            Dictionary<string, string> dict = new();
            var split = HttpUtility.UrlDecode(payload).Split(' ');

            for (int i = 0; i < split.Length; i++)
            {
                var pair = split[i].Split(':');
                dict.Add(pair[0], string.Join(":", pair.Skip(1)));
            }

            return dict;
        }

        // Sorry that this is hell.
        internal static LaunchParams Parse(string rawArgs)
        {
            LaunchParams info = new();
            var args = ParseRaw(rawArgs);
            bool b = true; // Success

            if (b &= args.TryGetValue("gameinfo", out string token))
                info.Token = token;

            if (b &= args.TryGetValue("placelauncherurl", out string launchUrl))
                info.LaunchUrl = launchUrl;
            
            if (b &= args.TryGetValue("launchtime", out string launchTimeStr))
                if (b &= long.TryParse(launchTimeStr, out long launchTime))
                    info.LaunchTime = DateTimeOffset.FromUnixTimeMilliseconds(launchTime);

            if (b &= args.TryGetValue("browsertrackerid", out string trackerIdStr))
                if (b &= long.TryParse(trackerIdStr, out long trackerId))
                    info.TrackerId = trackerId;
            
            if ((b &= args.TryGetValue("robloxLocale", out string rbxLocaleStr)) &&
                (b &= args.TryGetValue("gameLocale", out string gameLocaleStr)))
            {
                if (b &= Utility.TryGetCultureInfo(rbxLocaleStr, out CultureInfo rbxLocale))
                    info.RobloxLocale = rbxLocale;

                if (b &= Utility.TryGetCultureInfo(gameLocaleStr, out CultureInfo gameLocale))
                    info.GameLocale = gameLocale;
            }

            if (!b)
                return null;
            
            return info;
        }

        public static bool Launch(string args, IStarlightLaunchParams extras = null)
        {
            var parsed = Parse(args);
            
            if (parsed == null)
                return false;
            else if (extras != null)
                parsed.Derive(extras);
            
            return Launcher.Launch(parsed);
        }

        public static bool Hook(string launcherBin, string options = "")
        {
            try
            {
                using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Classes\\roblox-player\\shell\\open\\command");
                registryKey.SetValue(string.Empty, $"\"{launcherBin}\" {options}%1", RegistryValueKind.String);
                return true;
            }
            catch
            {
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

            var rbxBin = Path.Combine(clients.First().Directory, "RobloxPlayerLauncher.exe");
            try
            {
                using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Classes\\roblox-player\\shell\\open\\command");
                registryKey.SetValue(string.Empty, $"\"{rbxBin}\" %1", RegistryValueKind.String);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
