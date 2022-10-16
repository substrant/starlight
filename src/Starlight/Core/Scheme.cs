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
            var b = true; // Success

            // ReSharper disable AssignmentInConditionalExpression
            if (b &= args.TryGetValue("gameinfo", out var ticket))
                info.Ticket = ticket;

            if (b &= args.TryGetValue("placelauncherurl", out var launchUrl))
                if (b &= Uri.TryCreate(launchUrl, UriKind.Absolute, out var launchUri))
                    info.Request = new JoinRequest(launchUri);

            if (b &= args.TryGetValue("launchtime", out var launchTimeStr))
                if (b &= long.TryParse(launchTimeStr, out var launchTime))
                    info.LaunchTime = DateTimeOffset.FromUnixTimeMilliseconds(launchTime);

            if (b &= args.TryGetValue("browsertrackerid", out var trackerIdStr))
                if (b &= long.TryParse(trackerIdStr, out var trackerId))
                    info.TrackerId = trackerId;

            if (b &= args.TryGetValue("robloxLocale", out var rbxLocaleStr))
                if (Utility.TryGetCultureInfo(rbxLocaleStr, out var rbxLocale))
                    info.RobloxLocale = rbxLocale;

            if (b &= args.TryGetValue("gameLocale", out var gameLocaleStr))
                if (Utility.TryGetCultureInfo(gameLocaleStr, out var gameLocale))
                    info.GameLocale = gameLocale;
            // ReSharper enable AssignmentInConditionalExpression

            if (b)
                return info;

            Log.Error($"Failed to parse scheme payload. Dump: {info}");
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
