using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Starlight.Apis;

public enum AuthType
{
    Token,
    Ticket
}

public class Session
{
    internal readonly RestClient AuthClient = new("https://auth.roblox.com/");

    internal readonly RestClient GameClient = new("https://gamejoin.roblox.com/");
    /* Clients */

    internal readonly RestClient GeneralClient = new("https://www.roblox.com/");

    /* Tokens */

    string _authToken;
    DateTime _xsrfLastGrabbed;

    string _xsrfToken;

    Session()
    {
    }

    protected string AuthToken
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

    public string UserId { get; protected set; }

    public string Username { get; protected set; }

    public async Task<bool> ValidateAsync()
    {
        var req = new RestRequest("/v2/logout", Method.Post);
        var res = await AuthClient.ExecuteAsync(req);

        return
            res.StatusCode !=
            HttpStatusCode.Unauthorized; // Forbidden means no xsrf token, unauthorized means no authentication token
    }

    public bool Validate()
    {
        return ValidateAsync().Result;
    }

    public string GetXsrfToken(bool ignoreAliveCheck = false)
    {
        if (!ignoreAliveCheck && string.IsNullOrEmpty(_xsrfToken) &&
            (DateTime.Now - _xsrfLastGrabbed).TotalMinutes < 2) // Return previous token if session is still alive
            return _xsrfToken;

        var req = new RestRequest("/v2/logout", Method.Post); // won't log you out without a xsrf token
        var res = AuthClient.Execute(req);

        var xsrfHeader = res.Headers?.FirstOrDefault(x => x.Name == "x-csrf-token");
        if (xsrfHeader is null)
            throw new Exception("Failed to get xsrf token");

        _xsrfToken = xsrfHeader.Value?.ToString();
        _xsrfLastGrabbed = DateTime.Now;

        return _xsrfToken;
    }

    public async Task<string> GetXsrfTokenAsync(bool ignoreAliveCheck = false)
    {
        return await Task.Run(() => GetXsrfToken(ignoreAliveCheck));
    }

    void RetrieveInfo()
    {
        var req = new RestRequest("/my/settings/json");
        var res = GeneralClient.Execute(req);

        if (res.Content == null)
            return;

        var body = JObject.Parse(res.Content);
        UserId = body["UserId"]?.ToString();
        Username = body["Name"]?.ToString();
    }

    /* Session Management */

    public static Session Login(string authString, AuthType type)
    {
        Session session;
        switch (type)
        {
            case AuthType.Token:
                session = new Session { AuthToken = authString };
                break;
            case AuthType.Ticket:
                session = new Session();
                session.RedeemTicket(authString);
                break;
            default:
                throw new NotImplementedException();
        }

        if (!session.Validate())
            return null;

        session.RetrieveInfo();

        return session;
    }

    public static async Task<Session> LoginAsync(string authToken, AuthType type)
    {
        return await Task.Run(() => Login(authToken, type));
    }

    /* Tickets */

    public string GetTicket()
    {
        var req = new RestRequest("/v1/authentication-ticket", Method.Post)
            .AddHeader("X-CSRF-TOKEN", GetXsrfToken())
            .AddHeader("Referer", "https://www.roblox.com/"); // hehehe

        var res = AuthClient.Execute(req);

        var ticketHeader = res.Headers?.FirstOrDefault(x => x.Name == "rbx-authentication-ticket");
        if (ticketHeader is null)
            throw new Exception("Failed to get auth ticket");

        return ticketHeader.Value?.ToString();
    }

    public async Task<string> GetTicketAsync()
    {
        return await Task.Run(GetTicket);
    }

    public void RedeemTicket(string ticket)
    {
        var req = new RestRequest("/v1/authentication-ticket/redeem", Method.Post)
            .AddHeader("RBXAuthenticationNegotiation", "haha yes") // lmao
            .AddJsonBody(new { authenticationTicket = ticket });

        var res = AuthClient.Execute(req);

        if (res.StatusCode != HttpStatusCode.OK)
            throw new NotImplementedException();
    }

    public async Task RedeemTicketAsync(string ticket)
    {
        await Task.Run(() => RedeemTicket(ticket));
    }

    ~Session()
    {
        GeneralClient.Dispose();

        AuthClient.Dispose();

        GameClient.Dispose();
    }
}