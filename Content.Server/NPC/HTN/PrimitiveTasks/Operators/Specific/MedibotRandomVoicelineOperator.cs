using Content.Server.Chat.Systems;
using Content.Shared.NPC;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Server.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class MedibotRandomVoicelineOperator : HTNOperator
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private ChatSystem _chat = default!;
    private AudioSystem _audio = default!;

    /// <summary>
    /// The pack of voicelines
    /// </summary>
    [DataField(required: true)]
    public ProtoId<WeightedRandomPrototype> Voicelines { get; private set; }

    /// <summary>
    /// Whether to hide message from chat window and logs.
    /// </summary>
    [DataField]
    public bool Hidden;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);

        _chat = sysManager.GetEntitySystem<ChatSystem>();
        _audio = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<AudioSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var speaker = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);


        var weightList = _prototypeManager.Index<WeightedRandomPrototype>(Voicelines);
        var voicelineProto = weightList.Pick(_random);
        var voiceline = _prototypeManager.Index<NPCVoicelinePrototype>(voicelineProto);

        _chat.TrySendInGameICMessage(speaker, Loc.GetString(voiceline.Message), InGameICChatType.Speak, hideChat: Hidden, hideLog: Hidden);

        var uid = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        _audio.PlayPvs(voiceline.Audio, uid);
        return base.Update(blackboard, frameTime);
    }
}
