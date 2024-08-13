using Content.Server.Advertise.Components;
using Content.Server.Chat.Systems;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Shared.NPC;
using Robust.Server.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Advertise.EntitySystems;

public sealed class VoicelineAdvertiseSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly AudioSystem _audio = default!;


    /// <summary>
    /// The maximum amount of time between checking if advertisements should be displayed
    /// </summary>
    private readonly TimeSpan _maximumNextCheckDuration = TimeSpan.FromSeconds(15);

    /// <summary>
    /// The next time the game will check if advertisements should be displayed
    /// </summary>
    private TimeSpan _nextCheckTime = TimeSpan.MinValue;

    public override void Initialize()
    {
        SubscribeLocalEvent<VoicelineAdvertiseComponent, MapInitEvent>(OnMapInit);

        _nextCheckTime = TimeSpan.MinValue;
    }

    private void OnMapInit(EntityUid uid, VoicelineAdvertiseComponent advert, MapInitEvent args)
    {
        var prewarm = advert.Prewarm;
        RandomizeNextAdvertTime(advert, prewarm);
        _nextCheckTime = MathHelper.Min(advert.NextAdvertisementTime, _nextCheckTime);
    }

    private void RandomizeNextAdvertTime(VoicelineAdvertiseComponent advert, bool prewarm = false)
    {
        var minDuration = prewarm ? 0 : Math.Max(1, advert.MinimumWait);
        var maxDuration = Math.Max(minDuration, advert.MaximumWait);
        var waitDuration = TimeSpan.FromSeconds(_random.Next(minDuration, maxDuration));

        advert.NextAdvertisementTime = _gameTiming.CurTime + waitDuration;
    }

    public void SayAdvertisement(EntityUid uid, VoicelineAdvertiseComponent? advert = null)
    {
        if (!Resolve(uid, ref advert))
            return;

        var attemptEvent = new AttemptVoicelineAdvertiseEvent(uid);
        RaiseLocalEvent(uid, ref attemptEvent);
        if (attemptEvent.Cancelled)
            return;

        var weightList = _prototypeManager.Index<WeightedRandomPrototype>(advert.Voicelines);
        var voicelineProto = weightList.Pick(_random);
        var voiceline = _prototypeManager.Index<NPCVoicelinePrototype>(voicelineProto);

        _chat.TrySendInGameICMessage(uid, Loc.GetString(voiceline.Message), InGameICChatType.Speak, hideChat: true);
        _audio.PlayPvs(voiceline.Audio, uid);
    }

    public override void Update(float frameTime)
    {
        var currentGameTime = _gameTiming.CurTime;
        if (_nextCheckTime > currentGameTime)
            return;

        // _nextCheckTime starts at TimeSpan.MinValue, so this has to SET the value, not just increment it.
        _nextCheckTime = currentGameTime + _maximumNextCheckDuration;

        var query = EntityQueryEnumerator<VoicelineAdvertiseComponent>();
        while (query.MoveNext(out var uid, out var advert))
        {
            if (currentGameTime > advert.NextAdvertisementTime)
            {
                SayAdvertisement(uid, advert);
                // The timer is always refreshed when it expires, to prevent mass advertising (ex: all the vending machines have no power, and get it back at the same time).
                RandomizeNextAdvertTime(advert);
            }
            _nextCheckTime = MathHelper.Min(advert.NextAdvertisementTime, _nextCheckTime);
        }
    }
}

[ByRefEvent]
public record struct AttemptVoicelineAdvertiseEvent(EntityUid? Advertiser)
{
    public bool Cancelled = false;
}
