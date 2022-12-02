namespace Starlight.Apis;

/// <summary>
///     The authentication method to use.
/// </summary>
public enum AuthType {
    /// <summary>
    ///     Authenticate using an authentication token (<c>.ROBLOSECURITY</c> cookie).
    /// </summary>
    Token,

    /// <summary>
    ///     Authenticate using a one-time authentication ticket.
    /// </summary>
    Ticket
}