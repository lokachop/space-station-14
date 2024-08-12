using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Objectives;

/// <summary>
/// Stores the localized string and audio clip for a voiceline
/// </summary>
[Prototype("npcVoiceline")]
public sealed partial class NPCVoicelinePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;
    [DataField] public string Message { get; private set; } = string.Empty;
    [DataField] public SoundSpecifier Audio { get; private set; } = default!;
}
