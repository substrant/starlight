/* 
 *  Launcher.cs
 *  Author: RealNickk
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

using static Starlight.Native;

namespace Starlight
{
	public class LaunchParams
	{
		public string Token;
		public DateTimeOffset LaunchTime;
		public string LaunchUrl;
		public long TrackerId;
		public CultureInfo RobloxLocale;
		public CultureInfo GameLocale;
	}

	public class Launcher
    {
		public static LaunchParams ParsePayload(string szPayload)
		{
			LaunchParams launchParams = new LaunchParams();
			Dictionary<string, string> args = Utility.ParsePayload(szPayload);
            bool b = true;

			// String parsing
            b &= args.TryGetValue("gameinfo", out launchParams.Token);
			b &= args.TryGetValue("placelauncherurl", out launchParams.LaunchUrl);
			b &= args.TryGetValue("launchtime", out string szLaunchTime);
			b &= args.TryGetValue("browsertrackerid", out string szTrackerId);
			b &= args.TryGetValue("robloxLocale", out string szRobloxLocale);
			b &= args.TryGetValue("gameLocale", out string szGameLocale);
			if (!b)
				throw new Exception("Failed to parse launch payload.");
			
			// Serialization of launch time
			b &= long.TryParse(szLaunchTime, out long nLaunchTime);
			if (!b)
				throw new Exception("Failed to parse launch payload.");
			launchParams.LaunchTime = DateTimeOffset.FromUnixTimeMilliseconds(nLaunchTime);

			// Serialization of tracker id and locales
			b &= long.TryParse(szTrackerId, out launchParams.TrackerId);
			b &= Utility.TryGetCultureInfo(szRobloxLocale, out launchParams.RobloxLocale);
			b &= Utility.TryGetCultureInfo(szGameLocale, out launchParams.GameLocale);

			// Last error check
			if (!b)
				throw new Exception("Failed to parse launch payload.");

			return launchParams;
		}

		public static void EvadeTracking(LaunchParams info)
		{
			info.TrackerId = Utility.SecureRandomInteger();
			info.LaunchUrl = Regex.Replace(info.LaunchUrl, @"browserTrackerId=\d+", $"browserTrackerId={info.TrackerId}");
			info.LaunchTime = (DateTimeOffset)DateTime.Now;
		}

		public static bool OpenRoblox(string szBinPath, LaunchParams info, out PROCESS_INFORMATION procInfo)
        {
			STARTUPINFO startInfo = new();
			return CreateProcess(
				Path.GetFullPath(szBinPath),
				$"--play -a https://www.roblox.com/Login/Negotiate.ashx -t {info.Token} -j {info.LaunchUrl} -b {info.TrackerId} --launchtime={info.LaunchTime.ToUnixTimeMilliseconds()} --rloc {info.RobloxLocale.Name} --gloc {info.GameLocale.Name}",
				IntPtr.Zero,
				IntPtr.Zero,
				false,
				ProcessCreationFlags.CREATE_SUSPENDED, // start suspended so you can modify roblox before anything runs,
				IntPtr.Zero,
				null,
				ref startInfo,
				out procInfo
			);
		}
	}
}
