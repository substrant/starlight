namespace Starlight.Apis.JoinGame;

/// <summary>
///     The method of joining to use.
/// </summary>
public enum JoinType {
    /// <summary>
    ///     Join a server that Roblox has selected for you.
    /// </summary>
    Auto,

    /// <summary>
    ///     Join a specific server.
    /// </summary>
    Specific,

    /// <summary>
    ///     Join a private or reserved server.
    /// </summary>
    Private
}