namespace Starlight.Apis;

public abstract class RbxUser {
    /// <summary>
    ///     The user's ID.
    /// </summary>
    public abstract string UserId { get; protected set; }

    /// <summary>
    ///     The user's username.
    /// </summary>
    public abstract string Username { get; protected set; }
}