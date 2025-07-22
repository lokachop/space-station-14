using Robust.Shared.Player;

namespace Content.Server.Arcade.TestGame;

[RegisterComponent]
public sealed partial class TestGameArcadeComponent : Component
{
    /// <summary>
    /// The currently active session of NT-BG.
    /// </summary>
    public TestGame? Game = null;

    /// <summary>
    /// The player currently playing the active session of NT-BG.
    /// </summary>
    public EntityUid? Player = null;

    /// <summary>
    /// The players currently viewing (but not playing) the active session of NT-BG.
    /// </summary>
    public readonly List<EntityUid> Spectators = new();
}
