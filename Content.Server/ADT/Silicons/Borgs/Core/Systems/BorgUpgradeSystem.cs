// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.Interaction;
using Robust.Shared.Containers;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.PowerCell;
using Content.Shared.Throwing;

namespace Content.Server.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgUpgradeSystem : EntitySystem
{
    [Dependency] private readonly BorgBatterySystem _battery = default!;
    [Dependency] private readonly BorgSlotSystem _slots = default!;
    [Dependency] private readonly BorgLawSystem _law = default!;
    [Dependency] private readonly BorgRebootSystem _reboot = default!;
    [Dependency] private readonly BorgComponentPartSystem _parts = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly MetaDataSystem _metaDataSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgUpgradeComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(EntityUid uid, BorgUpgradeComponent component, AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target == null)
            return;

        var target = args.Target.Value;

        if (!HasComp<BorgComponent>(target))
            return;

        if (component.Installed)
        {
            args.Handled = true;
            return;
        }

        if (!string.IsNullOrEmpty(component.ModuleRequired))
        {
            if (!HasComp<ADTBorgModuleComponent>(target))
                return;

            if (Comp<ADTBorgModuleComponent>(target).ModuleType != component.UpgradeType)
                return;
        }

        ApplyUpgrade(target, uid, component);
        args.Handled = true;
    }

    private void ApplyUpgrade(EntityUid target, EntityUid upgradeUid, BorgUpgradeComponent upgrade)
    {
        switch (upgrade.UpgradeType)
        {
            case "Reset":
                HandleReset(target);
                break;
            case "Rename":
                HandleRename(target, upgradeUid);
                break;
            case "Restart":
                HandleRestart(target);
                break;
            case "Thrusters":
                HandleThrusters(target);
                break;
            case "SelfRepair":
                HandleSelfRepair(target);
                break;
            case "VTEC":
                HandleVTEC(target);
                break;
            case "DisablerCooler":
                HandleDisablerCooler(target);
                break;
            case "DiamondDrill":
                HandleDiamondDrill(target);
                break;
            case "SatchelOfHolding":
                HandleSatchelOfHolding(target);
                break;
            case "Lavaproof":
                HandleLavaproof(target);
                break;
            case "RCD":
                HandleRCD(target);
                break;
            case "RPED":
                HandleRPED(target);
                break;
            case "FloorBuffer":
                HandleFloorBuffer(target);
                break;
            case "BluespaceTrashBag":
                HandleBluespaceTrashBag(target);
                break;
            case "RSFExecutive":
                HandleRSFExecutive(target);
                break;
            case "WeaponsUnlock":
                HandleWeaponsUnlock(target);
                break;
        }

        upgrade.Installed = true;
        Dirty(upgradeUid, upgrade);

        Del(upgradeUid);
    }

    private void HandleReset(EntityUid target)
    {
        var partQuery = EntityQueryEnumerator<BorgComponentPartComponent>();
        while (partQuery.MoveNext(out var partUid, out var part))
        {
            part.BruteDamage = 0;
            part.BurnDamage = 0;
            part.Broken = false;
            part.Installed = true;
            Dirty(partUid, part);
        }

        if (TryComp<BorgSlotComponent>(target, out var slots))
        {
            for (var i = 0; i < slots.Count; i++)
            {
                var item = slots[i];
                if (item != EntityUid.Invalid)
                {
                    Del(item);
                }
                slots[i] = EntityUid.Invalid;
            }
            Dirty(target, slots);
        }

        if (TryComp<BorgLawComponent>(target, out var law) && !_law.TryResetToDefault(target, law))
            return;

        RemComp<BorgSelfRepairComponent>(target);
        RemComp<BorgEmagComponent>(target);

        if (TryComp<MovementSpeedModifierComponent>(target, out var move))
        {
            _movement.ChangeBaseSpeed(target,
                MovementSpeedModifierComponent.DefaultBaseWalkSpeed,
                MovementSpeedModifierComponent.DefaultBaseSprintSpeed,
                MovementSpeedModifierComponent.DefaultAcceleration, move);
        }

        RemComp<MovementIgnoreGravityComponent>(target);
    }

    private void HandleRestart(EntityUid target)
    {
        _reboot.StartReboot(target);
    }

    private void HandleThrusters(EntityUid target)
    {
        var thrusters = EnsureComp<MovementIgnoreGravityComponent>(target);
        thrusters.Weightless = true;
        Dirty(target, thrusters);
    }

    private void HandleSelfRepair(EntityUid target)
    {
        var repair = EnsureComp<BorgSelfRepairComponent>(target);
        repair.Enabled = true;
        Dirty(target, repair);
    }

    private void HandleVTEC(EntityUid target)
    {
        if (TryComp<MovementSpeedModifierComponent>(target, out var move))
        {
            var newWalk = move.BaseWalkSpeed * 1.5f;
            var newSprint = move.BaseSprintSpeed * 1.5f;
            _movement.ChangeBaseSpeed(target, newWalk, newSprint, move.BaseAcceleration, move);
        }
    }

    private void HandleRename(EntityUid target, EntityUid upgradeUid)
    {
        var meta = Comp<MetaDataComponent>(target);
        var newName = meta.EntityName + " [upgraded]";
        _metaDataSystem.SetEntityName(target, newName);
        _popup.PopupEntity(Loc.GetString("borg-upgrade-rename-applied"), target, target);
    }

    private void HandleDisablerCooler(EntityUid target)
    {
        _popup.PopupEntity(Loc.GetString("borg-upgrade-disabler-cooler"), target, target);
    }

    private void HandleDiamondDrill(EntityUid target)
    {
        TryReplaceSlotItem(target, "MiningDrill", "DiamondDrill");
        _popup.PopupEntity(Loc.GetString("borg-upgrade-diamond-drill"), target, target);
    }

    private void HandleSatchelOfHolding(EntityUid target)
    {
        TryReplaceSlotItem(target, "OreBag", "SatchelOfHolding");
        _popup.PopupEntity(Loc.GetString("borg-upgrade-satchel-holding"), target, target);
    }

    private void HandleLavaproof(EntityUid target)
    {
        _popup.PopupEntity(Loc.GetString("borg-upgrade-lavaproof"), target, target);
    }

    private void HandleRCD(EntityUid target)
    {
        if (TryComp<BorgSlotComponent>(target, out var slots))
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == EntityUid.Invalid)
                {
                    var rcd = Spawn("RCD", _transform.GetMapCoordinates(target));
                    slots[i] = rcd;
                    Dirty(target, slots);
                    break;
                }
            }
        }
        _popup.PopupEntity(Loc.GetString("borg-upgrade-rcd"), target, target);
    }

    private void HandleRPED(EntityUid target)
    {
        if (TryComp<BorgSlotComponent>(target, out var slots))
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == EntityUid.Invalid)
                {
                    var rped = Spawn("RPED", _transform.GetMapCoordinates(target));
                    slots[i] = rped;
                    Dirty(target, slots);
                    break;
                }
            }
        }
        _popup.PopupEntity(Loc.GetString("borg-upgrade-rped"), target, target);
    }

    private void HandleFloorBuffer(EntityUid target)
    {
        _popup.PopupEntity(Loc.GetString("borg-upgrade-floor-buffer"), target, target);
    }

    private void HandleBluespaceTrashBag(EntityUid target)
    {
        TryReplaceSlotItem(target, "TrashBag", "BluespaceTrashBag");
        _popup.PopupEntity(Loc.GetString("borg-upgrade-bluespace-bag"), target, target);
    }

    private void HandleRSFExecutive(EntityUid target)
    {
        _popup.PopupEntity(Loc.GetString("borg-upgrade-rsf-executive"), target, target);
    }

    private void TryReplaceSlotItem(EntityUid target, string oldItem, string newItem)
    {
        if (!TryComp<BorgSlotComponent>(target, out var slots))
            return;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == EntityUid.Invalid)
                continue;

            if (MetaData(slots[i]).EntityPrototype?.ID == oldItem)
            {
                Del(slots[i]);
                var replacement = Spawn(newItem, _transform.GetMapCoordinates(target));
                slots[i] = replacement;
                Dirty(target, slots);
                return;
            }
        }
    }

    private void HandleWeaponsUnlock(EntityUid target)
    {
        var emag = EnsureComp<BorgEmagComponent>(target);
        emag.WeaponsUnlocked = true;
        Dirty(target, emag);
    }
}
