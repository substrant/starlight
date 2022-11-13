using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Starlight.Apis;
using Starlight.Apis.JoinGame;
using Starlight.Misc;

namespace Starlight.Launch;

public class LaunchParams
{
    public string AuthStr;

    public AuthType AuthType = AuthType.Ticket;

    public CultureInfo GameLocale;

    public DateTimeOffset? LaunchTime = DateTimeOffset.Now;
    public JoinRequest Request;

    public CultureInfo RobloxLocale;

    public async Task<string> GetCliParamsAsync()
    {
        // Runtime check
        if (AuthStr is null || Request is null)
            throw new InvalidOperationException("AuthStr and Request must be set before calling ToCliArguments.");

        // Log in if a token was provided
        var authToken = AuthStr;
        if (AuthType == AuthType.Token)
        {
            var session = await Session.LoginAsync(authToken, AuthType.Token);
            authToken = await session.GetTicketAsync();
        }

        // Build the parameters
        // AFTER VERSION-d780cbcde4ab4f52:
        // OLD: --play -a https://auth.roblox.com/v1/authentication-ticket/redeem {args}
        // NEW: "FullPathToRobloxPlayerBeta.exe" --app {args}
        var str = new StringBuilder("--app");
        str.Append(" -t " + authToken);
        str.Append(" -j \"" + Request);
        str.Append("\" -b " + Request.BrowserTrackerId);
        str.Append(" --launchtime=" + DateTimeOffset.Now.ToUnixTimeSeconds());
        str.Append(" --rloc " + (RobloxLocale ?? CultureInfo.CurrentCulture).Name.Replace('-', '_').Split('/')[0].ToLowerInvariant());
        str.Append(" --gloc " + (GameLocale ?? CultureInfo.CurrentCulture).Name.Replace('-', '_').Split('/')[0].ToLowerInvariant());

        // Return the parameters
        return str.ToString();
    }
}