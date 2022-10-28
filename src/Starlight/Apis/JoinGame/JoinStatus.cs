namespace Starlight.Apis.JoinGame;

public enum JoinStatus
{
    Fail, // Request failed/rejected.
    Retry, // Request acknowledged, either standby for a server to be available or just retry.
    Success, // Request accepted.
    FullGame, // Request acknowledged, but the game is full.
    UserLeft // When joining another user: the user left the game before you could join. I dislike this.
}