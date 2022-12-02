using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Starlight.Apis;
using Starlight.Apis.JoinGame;
using Starlight.Misc;

namespace Starlight.Launch;

/// <summary>
///     Represents parameters for launching Roblox.
/// </summary>
public partial class LaunchParams {
    CultureInfo _gameLocale;
    CultureInfo _robloxLocale;

    /// <summary>
    ///     The authentication string to be used.<br />
    ///     Use <see cref="AuthType" /> to set the method of authentication.
    /// </summary>
    public string AuthStr;

    /// <summary>
    ///     The authentication method to use when launching.
    /// </summary>
    public AuthType AuthType = AuthType.Ticket;

    /// <summary>
    ///     The unix time when the launch was requested.
    /// </summary>
    public DateTimeOffset LaunchTime = DateTimeOffset.Now;

    /// <summary>
    ///     The join request to use.
    /// </summary>
    public JoinRequest Request;

    /// <summary>
    ///     The locale to use in the game.
    /// </summary>
    public CultureInfo GameLocale {
        get => _gameLocale ?? CultureInfo.CurrentCulture;
        set => _gameLocale = value;
    }

    /// <summary>
    ///     The locale to use in Roblox.
    /// </summary>
    public CultureInfo RobloxLocale {
        get => _robloxLocale ?? CultureInfo.CurrentCulture;
        set => _robloxLocale = value;
    }

    /// <summary>
    ///     Get the parameters used in the CLI to launch Roblox.
    /// </summary>
    public async Task<string> GetCliParamsAsync() {
        // Runtime check
        if (AuthStr is null || Request is null)
            throw new InvalidOperationException("AuthStr and Request must be set before calling GetCliParams.");

        // Log in if a token was provided
        var authToken = AuthStr;

        if (AuthType == AuthType.Token) {
            var session = await Session.LoginAsync(authToken);
            authToken = await session.GetTicketAsync();
        }

        // Build the parameters
        var str = new StringBuilder("--app");
        str.Append(" -t " + authToken);
        str.Append(" -j " + Request);
        str.Append(" -b " + Request.BrowserTrackerId);
        str.Append(" --launchtime=" + LaunchTime.ToUnixTimeSeconds());
        str.Append(" --rloc " + Utility.GetLocaleName(RobloxLocale));
        str.Append(" --gloc " + Utility.GetLocaleName(GameLocale));

        // Return the parameters
        return str.ToString();
    }

    /// <summary>
    ///     Get the URI used to launch Roblox through the `roblox-player` scheme.
    /// </summary>
    public async Task<string> GetLaunchUriAsync() {
        // Runtime check
        if (AuthStr is null || Request is null)
            throw new InvalidOperationException("AuthStr and Request must be set before calling GetLaunchUri.");

        // Log in if a token was provided
        var authToken = AuthStr;

        if (AuthType == AuthType.Token) {
            var session = await Session.LoginAsync(authToken);
            authToken = await session.GetTicketAsync();
        }

        // Build the request
        var uri = new StringBuilder("1+launchmode:play");
        uri.Append("+gameinfo:" + authToken);
        uri.Append("+launchtime:" + LaunchTime.ToUnixTimeMilliseconds());
        uri.Append("+placelauncherurl:" + HttpUtility.UrlEncode(Request.ToString()));
        uri.Append("+browsertrackerid:" + Request.BrowserTrackerId);
        uri.Append("+robloxLocale:" + Utility.GetLocaleName(RobloxLocale));
        uri.Append("+gameLocale:" + Utility.GetLocaleName(GameLocale));

        return "roblox-player:" + uri;
    }
}