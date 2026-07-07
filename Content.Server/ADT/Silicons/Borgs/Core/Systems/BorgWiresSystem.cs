// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.Core.Systems;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Content.Shared.StationAi;
using Content.Shared.Stunnable;

namespace Content.Server.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgWiresSystem : SharedBorgWiresSystem
{
    [Dependency] private readonly BorgSystem _borg = default!;
    [Dependency] private readonly BorgLawSystem _law = default!;
    [Dependency] private readonly BorgAiLinkSystem _aiLink = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedStationAiSystem _stationAi = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public void ProcessWireAction(EntityUid uid, BorgWireType wireType, BorgWireAction action, BorgWiresComponent? wires = null)
    {
        if (!Resolve(uid, ref wires))
            return;

        switch (wireType)
        {
            case BorgWireType.AiControl:
                HandleAiControlWire(uid, action);
                break;
            case BorgWireType.Camera:
                HandleCameraWire(uid, action);
                break;
            case BorgWireType.LawCheck:
                HandleLawCheckWire(uid, action);
                break;
            case BorgWireType.Locked:
                HandleLockedWire(uid, action);
                break;
        }
    }

    private void HandleAiControlWire(EntityUid uid, BorgWireAction action)
    {
        switch (action)
        {
            case BorgWireAction.Cut:
                // SS13: Disconnect from AI entirely
                if (TryComp<BorgAiLinkComponent>(uid, out var aiLink))
                    _aiLink.UnlinkFromAi(uid, aiLink);
                _popup.PopupEntity(Loc.GetString("borg-wire-ai-control-cut"), uid);
                break;
            case BorgWireAction.Mend:
                // SS13: No effect on mend (only lawupdate is handled by LAWCHECK wire)
                _popup.PopupEntity(Loc.GetString("borg-wire-ai-control-mend"), uid);
                break;
            case BorgWireAction.Pulse:
                // SS13: Reconnect to random active AI (if not emagged)
                if (TryComp<BorgAiLinkComponent>(uid, out var aiComp))
                    _aiLink.TryLinkToRandomAi(uid, aiComp);
                break;
        }
    }

    private void HandleCameraWire(EntityUid uid, BorgWireAction action)
    {
        switch (action)
        {
            case BorgWireAction.Cut:
                // SS13: Disable camera + kick observers
                if (TryComp<StationAiVisionComponent>(uid, out var vision))
                    _stationAi.SetVisionEnabled((uid, vision), false);
                if (TryComp<BorgAiLinkComponent>(uid, out var aiLink))
                    aiLink.HasCamera = false;
                _popup.PopupEntity(Loc.GetString("borg-wire-camera-cut"), uid);
                break;
            case BorgWireAction.Mend:
                // SS13: Enable camera
                if (TryComp<StationAiVisionComponent>(uid, out var visionMend))
                    _stationAi.SetVisionEnabled((uid, visionMend), true);
                if (TryComp<BorgAiLinkComponent>(uid, out var aiMend))
                    aiMend.HasCamera = true;
                break;
            case BorgWireAction.Pulse:
                // SS13: Kick observers, loud focus
                _stun.TryKnockdown(uid, TimeSpan.FromSeconds(1));
                _popup.PopupEntity(Loc.GetString("borg-wire-camera-pulse"), uid);
                break;
        }
    }

    private void HandleLawCheckWire(EntityUid uid, BorgWireAction action)
    {
        switch (action)
        {
            case BorgWireAction.Cut:
                // SS13: ENABLE lawupdate (shows laws), sync with AI
                _law.SetLawSync(uid, true);
                _popup.PopupEntity(Loc.GetString("borg-wire-law-check-cut"), uid);
                break;
            case BorgWireAction.Mend:
                // SS13: Enable lawupdate (if not emagged)
                if (!TryComp<BorgEmagComponent>(uid, out var emag) || !emag.Emagged)
                    _law.SetLawSync(uid, true);
                break;
            case BorgWireAction.Pulse:
                // SS13: No effect
                break;
        }
    }

    private void HandleLockedWire(EntityUid uid, BorgWireAction action)
    {
        switch (action)
        {
            case BorgWireAction.Cut:
                // SS13: Enable lockdown permanently while wire is cut
                if (TryComp<BorgComponent>(uid, out var borgCut))
                    _borg.SetLocked(uid, true);
                break;
            case BorgWireAction.Mend:
                // SS13: No effect
                break;
            case BorgWireAction.Pulse:
                // SS13: Toggle lockcharge
                if (TryComp<BorgComponent>(uid, out var borg))
                    _borg.SetLocked(uid, !borg.Locked);
                break;
        }
    }
}
