using Content.Shared.Arcade;
using Robust.Server.GameObjects;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server.Arcade.TestGame;

public sealed partial class TestGame
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    private readonly ArcadeSystem _arcadeSystem;
    private readonly UserInterfaceSystem _uiSystem;

    /// <summary>
    /// What entity is currently hosting this game.
    /// </summary>
    private readonly EntityUid _owner = default!;

    public TestGame(EntityUid owner)
    {
        IoCManager.InjectDependencies(this);
        _arcadeSystem = _entityManager.System<ArcadeSystem>();
        _uiSystem = _entityManager.System<UserInterfaceSystem>();

        _owner = owner;
    }
}