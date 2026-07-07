// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.Core.Events;
using Content.Shared.ADT.Silicons.Borgs.Core.Systems;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Robust.Shared.Random;

namespace Content.Server.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgAiLinkSystem : SharedBorgAiLinkSystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgAiLinkComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<BorgAiLinkComponent, BorgReturnToAiEvent>(OnReturnToAi);
    }

    private void OnReturnToAi(EntityUid uid, BorgAiLinkComponent component, BorgReturnToAiEvent args)
    {
        if (component.LinkedAi != null)
            ReturnMindToAi(uid, component);
    }

    private void OnMobStateChanged(EntityUid uid, BorgAiLinkComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead || args.NewMobState == MobState.Critical)
        {
            if (component.LinkedAi != null)
                ReturnMindToAi(uid, component);
        }
    }

    public void ReturnMindToAi(EntityUid uid, BorgAiLinkComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (component.LinkedAi == null)
            return;

        var aiEntity = component.LinkedAi.Value;

        if (_mind.TryGetMind(uid, out var mindId, out var mind))
        {
            _mind.TransferTo(mindId, aiEntity);
        }

        UnlinkFromAi(uid, component);
    }

    public void TakeAiControl(EntityUid uid, EntityUid aiEntity, BorgAiLinkComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        LinkToAi(uid, aiEntity, component);

        if (_mind.TryGetMind(aiEntity, out var mindId, out var _))
        {
            _mind.TransferTo(mindId, uid);
        }
    }

    public void UnlinkSelf(EntityUid uid, BorgAiLinkComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        UnlinkFromAi(uid, component);
        component.LawSync = false;
        component.ScrambledCodes = true;
        component.HasCamera = false;
        Dirty(uid, component);

        _popup.PopupEntity(Loc.GetString("borg-ai-unlink-self"), uid);
    }

    public bool TryLinkToRandomAi(EntityUid uid, BorgAiLinkComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (TryComp<BorgEmagComponent>(uid, out var emag) && emag.Emagged)
            return false;

        var aiCandidates = new List<EntityUid>();
        var aiQuery = EntityQueryEnumerator<BorgAiLinkComponent>();
        while (aiQuery.MoveNext(out var aiUid, out var aiComp))
        {
            if (aiUid != uid && aiComp.LinkedAi != null)
                aiCandidates.Add(aiComp.LinkedAi.Value);
        }

        if (aiCandidates.Count == 0)
            return false;

        var randomAi = _random.Pick(aiCandidates);
        LinkToAi(uid, randomAi, component);
        _popup.PopupEntity(Loc.GetString("borg-ai-reconnected", ("ai", MetaData(randomAi).EntityName)), uid);
        return true;
    }

    public void MakeMalfRobot(EntityUid uid, EntityUid malfAi, BorgAiLinkComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        LinkToAi(uid, malfAi, component);
        component.MalfHacked = true;
        component.LawSync = true;
        Dirty(uid, component);

        _popup.PopupEntity(Loc.GetString("borg-malf-ai-takeover", ("ai", MetaData(malfAi).EntityName)), uid);
    }

    public void RemoveMindslave(EntityUid uid, BorgAiLinkComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.MalfHacked = false;
        Dirty(uid, component);
    }
}
