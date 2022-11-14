using Starlight.Apis.JoinGame;
using Starlight.Misc;

// ReSharper disable once CheckNamespace
namespace Starlight.Apis;

public partial class Session
{
    /// <summary>Synchronous wrapper for <see cref="LoginAsync" />.</summary>
    public static Session Login(string authToken)
    {
        return AsyncHelpers.RunSync(() => LoginAsync(authToken));
    }

    /// <summary>Synchronous wrapper for <see cref="RedeemAsync" />.</summary>
    public static Session Redeem(string authTicket)
    {
        return AsyncHelpers.RunSync(() => RedeemAsync(authTicket));
    }

    /// <summary>Synchronous wrapper for <see cref="AuthenticateAsync" />.</summary>
    public static Session Authenticate(string authToken, AuthType authType)
    {
        return AsyncHelpers.RunSync(() => AuthenticateAsync(authToken, authType));
    }

    /// <summary>Synchronous wrapper for <see cref="GetTicketAsync" />.</summary>
    public string GetTicket()
    {
        return AsyncHelpers.RunSync(() => GetTicketAsync());
    }

    /// <summary>Synchronous wrapper for <see cref="GetXsrfTokenAsync" />.</summary>
    public string GetXsrfToken(bool bypassCache = false)
    {
        return AsyncHelpers.RunSync(() => GetXsrfTokenAsync(bypassCache));
    }

    /// <summary>Synchronous wrapper for <see cref="RequestJoinAsync" />.</summary>
    public JoinResponse RequestJoin(JoinRequest req, int maxTries = 0)
    {
        return AsyncHelpers.RunSync(() => RequestJoinAsync(req, maxTries));
    }
}