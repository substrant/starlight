using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Starlight.Apis.JoinGame;

namespace Starlight.Apis;

/// <summary>
///     Represents a Roblox web session.
/// </summary>
public partial class Session : RbxUser, IDisposable
{
    /* Clients */

    internal readonly RestClient AuthClient = new RestClient("https://auth.roblox.com/").UseNewtonsoftJson();
    internal readonly RestClient GameClient = new RestClient("https://gamejoin.roblox.com/").UseNewtonsoftJson();
    internal readonly RestClient GeneralClient = new RestClient("https://www.roblox.com/").UseNewtonsoftJson();

    /* Tokens */

    string _authToken;

    /* IDisposable implementation */

    bool _disposed;

    DateTime _xsrfLastGrabbed;
    string _xsrfToken;

    Session()
    {
    }

    /// <summary>
    ///     The token used to authenticate the session.
    ///     This token is the same thing as a <c>.ROBLOSECURITY</c> cookie.
    /// </summary>
    public string AuthToken
    {
        get => _authToken;
        set
        {
            GeneralClient.AddCookie(".ROBLOSECURITY", value, "/", ".roblox.com");
            AuthClient.AddCookie(".ROBLOSECURITY", value, "/", ".roblox.com");
            GameClient.AddCookie(".ROBLOSECURITY", value, "/", ".roblox.com");
            _authToken = value;
        }
    }

    /* Session Info */

    public override string UserId { get; protected set; }

    public override string Username { get; protected set; }

    /// <summary>
    ///     Clean up and release all clients that have been allocated.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <exception cref="TaskCanceledException" />
    /// <exception cref="InvalidOperationException" />
    async Task RetrieveInfoAsync(CancellationToken token = default)
    {
        try
        {
            var res = await GeneralClient.GetJsonAsync<SessionInfo>("/my/settings/json", token);
            UserId = res?.UserId;
            Username = res?.Username;

            // Validate that the info retrieval succeeded
            if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(Username))
                throw new InvalidOperationException();
        }
        catch (JsonSerializationException)
        {
            // Roblox returns HTML content when the session is invalid, and redirects to the login page.
            // This is a workaround to catch that and throw a more descriptive exception.
            throw new InvalidOperationException();
        }
    }

    /* Authentication */

    /// <summary>
    ///     Get a <see cref="Session" /> from a <c>.ROBLOSECURITY</c> cookie.
    /// </summary>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="TaskCanceledException" />
    /// <exception cref="InvalidOperationException" />
    public static async Task<Session> LoginAsync(string authToken, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(authToken))
            throw new ArgumentNullException(nameof(authToken));

        var session = new Session { AuthToken = authToken };

        await session.RetrieveInfoAsync(token);
        return session;
    }

    /// <summary>
    ///     Get a <see cref="Session" /> from a one-time authentication ticket.
    /// </summary>
    /// <exception cref="TaskCanceledException" />
    public static async Task<Session> RedeemAsync(string authTicket, CancellationToken token = default)
    {
        var session = new Session();
        var req = new RestRequest("/v1/authentication-ticket/redeem", Method.Post)
            .AddHeader("RBXAuthenticationNegotiation", "1")
            .AddJsonBody(new { authenticationTicket = authTicket });
        await session.AuthClient.ExecuteAsync(req, token);

        await session.RetrieveInfoAsync(token);
        return session;
    }

    /// <summary>
    ///     Get a <see cref="Session" /> using authentication type <paramref name="authType" />.
    /// </summary>
    /// <exception cref="ArgumentException" />
    /// <exception cref="TaskCanceledException" />
    public static async Task<Session> AuthenticateAsync(string authToken, AuthType authType,
        CancellationToken token = default)
    {
        return authType switch
        {
            AuthType.Token => await LoginAsync(authToken, token),
            AuthType.Ticket => await RedeemAsync(authToken, token),
            _ => throw new ArgumentException("Invalid authentication type.", nameof(authType))
        };
    }

    /// <summary>
    ///     Create an authentication ticket for a one-time login.
    /// </summary>
    /// <exception cref="TaskCanceledException" />
    /// <exception cref="InvalidOperationException" />
    public async Task<string> GetTicketAsync(CancellationToken token = default)
    {
        // Get a new authentication ticket
        var req = new RestRequest("/v1/authentication-ticket", Method.Post)
            .AddHeader("X-CSRF-TOKEN", await GetXsrfTokenAsync(false, token))
            .AddHeader("Referer", "https://www.roblox.com/");
        var res = await AuthClient.ExecuteAsync(req, token);

        // Get the ticket header
        var ticketHeader = res.Headers?.FirstOrDefault(x => x.Name == "rbx-authentication-ticket");
        if (ticketHeader is null)
            throw new InvalidOperationException(
                "Failed to get authentication ticket. Is the session authenticated with a valid token?");

        return ticketHeader.Value?.ToString();
    }

    /// <summary>
    ///     <para>Get a cross-site request forgery token for use in the Roblox web API.</para>
    ///     More information on XSRF and why there's tokens for it:<br />
    ///     <see href="https://en.wikipedia.org/wiki/Cross-site_request_forgery" />.
    /// </summary>
    /// <exception cref="TaskCanceledException" />
    /// <exception cref="InvalidOperationException" />
    public async Task<string> GetXsrfTokenAsync(bool bypassCache = false, CancellationToken token = default)
    {
        // Return previous token if xsrf token is still alive
        if (!bypassCache && !string.IsNullOrEmpty(_xsrfToken) && (DateTime.Now - _xsrfLastGrabbed).TotalMinutes > 2)
            return _xsrfToken;

        // This won't log you out without a xsrf token
        var req = new RestRequest("/v2/logout", Method.Post);
        var res = await AuthClient.ExecuteAsync(req, token);

        // Get xsrf token header
        var xsrfHeader = res.Headers?.FirstOrDefault(x => x.Name?.ToLowerInvariant() == "x-csrf-token");
        if (xsrfHeader is null)
            throw new InvalidOperationException(
                "Failed to get cross-site request forgery token. Is the session authenticated with a valid token?");

        // Set token
        _xsrfToken = xsrfHeader.Value?.ToString();
        _xsrfLastGrabbed = DateTime.Now;

        return _xsrfToken;
    }

    /* Game joining */

    /// <summary>
    ///     Attempt to get a <see cref="JoinResponse" /> from a <see cref="JoinRequest" />.
    /// </summary>
    /// <exception cref="TaskCanceledException" />
    public async Task<JoinResponse> RequestJoinAsync(JoinRequest joinReq, int maxTries = 0,
        CancellationToken token = default)
    {
        var tries = -1;
        for (; !token.IsCancellationRequested;)
        {
            var reqBody = JsonConvert.SerializeObject(this, Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var req = new RestRequest(joinReq.Endpoint, Method.Post)
                .AddHeader("User-Agent", "Roblox/WinInet")
                .AddJsonBody(reqBody);
            var res = await GameClient.ExecuteAsync<JoinResponse>(req, token);

            if (res.Data is null)
                return new JoinResponse { Success = false };

            if (res.StatusCode != HttpStatusCode.OK || res.Data.Status == JoinStatus.Fail)
            {
                res.Data.Success = false;
                return res.Data;
            }

            if (res.Data.Status == JoinStatus.Retry && tries != maxTries)
            {
                await Task.Delay(2000, token); // Wait a bit.
                tries++;
                continue;
            }

            res.Data.Success = true;
            return res.Data;
        }

        throw new TaskCanceledException();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || _disposed)
            return;

        _disposed = true;
        GeneralClient.Dispose();
        AuthClient.Dispose();
        GameClient.Dispose();
    }

    class SessionInfo
    {
        [JsonProperty("UserId")] public string UserId;
        [JsonProperty("Name")] public string Username;
    }
}