namespace Starlight.Apis.JoinGame;

/// <summary>
///     The status of a <see cref="JoinResponse" />.
/// </summary>
public enum JoinStatus {
    /// <summary>
    ///     Request failed/rejected.
    /// </summary>
    Fail,

    /// <summary>
    ///     Request acknowledged, you should standby for a server to be available or retry.
    /// </summary>
    Retry,

    /// <summary>
    ///     Request accepted.
    /// </summary>
    Success,

    /// <summary>
    ///     Request acknowledged, but the game is full.
    /// </summary>
    FullGame,

    /// <summary>
    ///     <para>The user left the game before you could join them.</para>
    ///     <strong>Note:</strong> Applies when following a user.
    /// </summary>
    UserLeft
}