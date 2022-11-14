using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Starlight.Apis;
using Starlight.Apis.JoinGame;

namespace Starlight.Launch;

/// <summary>
///     Represents parameters for launching Roblox.
/// </summary>
public class LaunchParams
{
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
    ///     The locale to use in the game.
    /// </summary>
    public CultureInfo GameLocale;

    /// <summary>
    ///     The unix time when the launch was requested.
    /// </summary>
    public DateTimeOffset? LaunchTime = DateTimeOffset.Now;

    /// <summary>
    ///     The join request to use.
    /// </summary>
    public JoinRequest Request;

    /// <summary>
    ///     The locale to use in Roblox.
    /// </summary>
    public CultureInfo RobloxLocale;

    /// <summary>
    ///     Get the parameters used in the CLI to launch Roblox.
    /// </summary>
    public async Task<string> GetCliParamsAsync()
    {
        // Runtime check
        if (AuthStr is null || Request is null)
            throw new InvalidOperationException("AuthStr and Request must be set before calling ToCliArguments.");

        // Log in if a token was provided
        var authToken = AuthStr;
        if (AuthType == AuthType.Token)
        {
            var session = await Session.LoginAsync(authToken);
            authToken = await session.GetTicketAsync();
        }

        // Build the parameters
        var str = new StringBuilder("--app");
        str.Append(" -t " + authToken);
        str.Append(" -j \"" + Request);
        str.Append("\" -b " + Request.BrowserTrackerId);
        str.Append(" --launchtime=" + DateTimeOffset.Now.ToUnixTimeSeconds());
        str.Append(" --rloc " + (RobloxLocale ?? CultureInfo.CurrentCulture).Name.Replace('-', '_').Split('/')[0]
            .ToLowerInvariant());
        str.Append(" --gloc " + (GameLocale ?? CultureInfo.CurrentCulture).Name.Replace('-', '_').Split('/')[0]
            .ToLowerInvariant());

        // Return the parameters
        return str.ToString();
    }
}