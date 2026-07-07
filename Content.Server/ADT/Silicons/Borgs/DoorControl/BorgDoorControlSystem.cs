// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.Access.Systems;
using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.DoorControl;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Electrocution;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Network;

namespace Content.Server.ADT.Silicons.Borgs.DoorControl;

/// <summary>
/// Handles borg door-control keybind actions sent from the client.
/// Validates that the sender is a borg, the target is a reachable door,
/// and then performs the requested action through the existing door systems.
/// </summary>
public sealed class BorgDoorControlSystem : EntitySystem
{
    [Dependency] private readonly SharedAirlockSystem _airlocks = default!;
    [Dependency] private readonly SharedDoorSystem _doors = default!;
    [Dependency] private readonly SharedElectrocutionSystem _electrify = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly AccessReaderSystem _access = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;

    public override void Initialize()
    {
        SubscribeNetworkEvent<BorgDoorControlEvent>(OnDoorControl);
    }

    private void OnDoorControl(BorgDoorControlEvent ev, EntitySessionEventArgs args)
    {
        var user = args.SenderSession.AttachedEntity;
        if (user == null)
            return;

        // Accept both the ADT-rework borg and the vanilla chassis borg.
        if (!HasComp<BorgComponent>(user.Value) && !HasComp<BorgChassisComponent>(user.Value))
            return;

        if (!TryGetEntity(ev.Target, out var target))
            return;

        var targetUid = target.Value;

        // Borgs must be physically next to the door (unlike AI which uses cameras).
        if (!_interaction.InRangeUnobstructed(user.Value, targetUid))
        {
            _popup.PopupCursor(Loc.GetString("borg-door-control-out-of-range"), args.SenderSession);
            return;
        }

        switch (ev.Action)
        {
            case BorgDoorAction.ToggleBolt:
                HandleToggleBolt(user.Value, targetUid);
                break;
            case BorgDoorAction.ToggleElectrified:
                HandleToggleElectrified(user.Value, targetUid);
                break;
            case BorgDoorAction.ToggleEmergencyAccess:
                HandleToggleEmergencyAccess(user.Value, targetUid);
                break;
            case BorgDoorAction.ToggleDoor:
                HandleToggleDoor(user.Value, targetUid);
                break;
        }
    }

    private void HandleToggleBolt(EntityUid user, EntityUid target)
    {
        if (!TryComp<DoorBoltComponent>(target, out var bolts))
        {
            _popup.PopupCursor(Loc.GetString("borg-door-control-not-supported"), user);
            return;
        }

        if (bolts.BoltWireCut || !_power.IsPowered(target))
        {
            _popup.PopupCursor(Loc.GetString("borg-door-control-not-responding"), user);
            return;
        }

        if (!_access.IsAllowed(user, target))
        {
            _popup.PopupCursor(Loc.GetString("borg-door-control-no-access"), user);
            return;
        }

        var desired = !bolts.BoltsDown;
        _doors.TrySetBoltDown((target, bolts), desired, user, predicted: false);
        _adminLogger.Add(LogType.Action,
            $"{user} set bolt status on {target} to [{desired}] using the borg door-control keybind.");
    }

    private void HandleToggleElectrified(EntityUid user, EntityUid target)
    {
        if (!TryComp<ElectrifiedComponent>(target, out var electrified))
        {
            _popup.PopupCursor(Loc.GetString("borg-door-control-not-supported"), user);
            return;
        }

        if (electrified.IsWireCut || !_power.IsPowered(target))
        {
            _popup.PopupCursor(Loc.GetString("borg-door-control-not-responding"), user);
            return;
        }

        if (!_access.IsAllowed(user, target))
        {
            _popup.PopupCursor(Loc.GetString("borg-door-control-no-access"), user);
            return;
        }

        var desired = !electrified.Enabled;
        _electrify.SetElectrified((target, electrified), desired);
        _adminLogger.Add(LogType.Action,
            $"{user} set electrified status on {target} to [{desired}] using the borg door-control keybind.");
    }

    private void HandleToggleEmergencyAccess(EntityUid user, EntityUid target)
    {
        if (!TryComp<AirlockComponent>(target, out var airlock))
        {
            _popup.PopupCursor(Loc.GetString("borg-door-control-not-supported"), user);
            return;
        }

        if (!_power.IsPowered(target))
        {
            _popup.PopupCursor(Loc.GetString("borg-door-control-not-responding"), user);
            return;
        }

        if (!_access.IsAllowed(user, target))
        {
            _popup.PopupCursor(Loc.GetString("borg-door-control-no-access"), user);
            return;
        }

        var desired = !airlock.EmergencyAccess;
        _airlocks.SetEmergencyAccess((target, airlock), desired, user, predicted: false);
        _adminLogger.Add(LogType.Action,
            $"{user} set emergency access on {target} to [{desired}] using the borg door-control keybind.");
    }

    private void HandleToggleDoor(EntityUid user, EntityUid target)
    {
        if (!TryComp<DoorComponent>(target, out var door))
        {
            _popup.PopupCursor(Loc.GetString("borg-door-control-not-supported"), user);
            return;
        }

        // Shutters / blast doors have clickOpen = false and no access reader wiring
        // through this path, so a direct toggle is always allowed for borgs in range.
        _doors.TryToggleDoor(target, door, user);
        _adminLogger.Add(LogType.Action,
            $"{user} toggled door {target} using the borg door-control keybind.");
    }
}
