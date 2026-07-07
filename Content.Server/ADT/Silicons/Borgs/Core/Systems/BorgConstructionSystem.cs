using System.Linq;
using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.Core.Systems;
using Content.Shared.Flash.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Stacks;
using Content.Shared.Tag;
using Content.Shared.Tools.Systems;
using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgConstructionSystem : EntitySystem
{
    [Dependency] private readonly SharedToolSystem _tool = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly BorgBatterySystem _battery = default!;
    [Dependency] private readonly BorgSlotSystem _slots = default!;
    [Dependency] private readonly BorgSystem _borg = default!;
    [Dependency] private readonly SharedBorgWiresSystem _wires = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly BorgLawSystem _law = default!;

    private const string BrainContainerId = "_brain";
    private const string PartsContainerId = "borg_parts";
    private const string CellContainerId = "cell_slot";

    private static readonly ProtoId<TagPrototype>[] LimbTags =
    [
        "BorgLArm", "BorgRArm", "BorgLLeg", "BorgRLeg", "BorgHead", "BorgTorso"
    ];

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgConstructionComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<BorgConstructionComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, BorgConstructionComponent component, MapInitEvent args)
    {
        if (HasComp<BorgComponent>(uid))
            component.Stage = ConstructionStage.Complete;
    }

    private void OnInteractUsing(EntityUid uid, BorgConstructionComponent component, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (component.Stage == ConstructionStage.Complete)
        {
            TryDeconstruct(uid, args);
            return;
        }

        TryConstruct(uid, component, args);
    }

    private void TryConstruct(EntityUid uid, BorgConstructionComponent component, InteractUsingEvent args)
    {
        if (TryDepositMaterial(uid, component, args))
            return;

        switch (component.Stage)
        {
            case ConstructionStage.None:
                AdvanceToFrame(uid, component, args);
                break;
            case ConstructionStage.Frame:
                TryInstallLimb(uid, component, args);
                break;
            case ConstructionStage.LimbsInstalled:
                TryInstallChest(uid, component, args);
                break;
            case ConstructionStage.ChestInstalled:
                TryInstallHead(uid, component, args);
                break;
            case ConstructionStage.HeadInstalled:
            case ConstructionStage.Configuration:
                TryConfigure(uid, component, args);
                break;
            case ConstructionStage.MMIInserted:
                FinalizeConstruction(uid, component, args);
                break;
        }
    }

    private bool TryDepositMaterial(EntityUid uid, BorgConstructionComponent component, InteractUsingEvent args)
    {
        if (TryComp<StackComponent>(args.Used, out var stack))
        {
            if (stack.StackTypeId == "Steel")
            {
                ConsumeSteel(uid, component, args, stack);
                return true;
            }
            if (stack.StackTypeId == "Cable")
            {
                ConsumeCable(uid, component, args, stack);
                return true;
            }
        }

        if (_tag.HasTag(args.Used, "CableCoil") && !HasComp<StackComponent>(args.Used))
        {
            _popup.PopupEntity(Loc.GetString("borg-material-cable-need-stack"), uid, args.User);
            return true;
        }

        if (HasComp<FlashComponent>(args.Used))
        {
            ConsumeFlash(uid, component, args);
            return true;
        }

        return false;
    }

    private void ConsumeSteel(EntityUid uid, BorgConstructionComponent component, InteractUsingEvent args, StackComponent stack)
    {
        var inHand = stack.Count;
        if (inHand <= 0)
            return;

        _stack.SetCount((args.Used, stack), 0);
        component.SteelDeposited += inHand;
        Dirty(uid, component);
        _popup.PopupEntity(Loc.GetString("borg-material-steel-deposited", ("amount", inHand), ("total", component.SteelDeposited)), uid, args.User);
        args.Handled = true;
    }

    private void ConsumeCable(EntityUid uid, BorgConstructionComponent component, InteractUsingEvent args, StackComponent stack)
    {
        if (component.CableInserted)
        {
            _popup.PopupEntity(Loc.GetString("borg-material-cable-already"), uid, args.User);
            return;
        }

        _stack.SetCount((args.Used, stack), 0);
        component.CableInserted = true;
        Dirty(uid, component);
        _popup.PopupEntity(Loc.GetString("borg-material-cable-installed"), uid, args.User);
        args.Handled = true;
    }

    private void ConsumeFlash(EntityUid uid, BorgConstructionComponent component, InteractUsingEvent args)
    {
        if (component.FlashesInserted >= 2)
        {
            _popup.PopupEntity(Loc.GetString("borg-material-flash-already"), uid, args.User);
            return;
        }

        QueueDel(args.Used);
        component.FlashesInserted++;
        Dirty(uid, component);
        _popup.PopupEntity(Loc.GetString("borg-material-flash-installed", ("count", component.FlashesInserted)), uid, args.User);
        args.Handled = true;
    }

    private void AdvanceToFrame(EntityUid uid, BorgConstructionComponent component, InteractUsingEvent args)
    {
        if (!_tool.HasQuality(args.Used, "Screwing"))
            return;

        component.Stage = ConstructionStage.Frame;
        Dirty(uid, component);
        args.Handled = true;
    }

    private void TryInstallLimb(EntityUid uid, BorgConstructionComponent component, InteractUsingEvent args)
    {
        foreach (var limbTag in LimbTags)
        {
            if (!_tag.HasTag(args.Used, limbTag))
                continue;

            var tagStr = limbTag.ToString();
            var steelCost = tagStr switch
            {
                "BorgLArm" or "BorgRArm" or "BorgLLeg" or "BorgRLeg" => 10,
                "BorgTorso" => 50,
                "BorgHead" => 50,
                _ => 0
            };

            if (component.SteelDeposited < steelCost)
            {
                _popup.PopupEntity(Loc.GetString("borg-material-need-steel",
                    ("need", steelCost),
                    ("have", component.SteelDeposited)), uid, args.User);
                return;
            }

            component.SteelDeposited -= steelCost;
            Dirty(uid, component);

            var container = _container.EnsureContainer<Container>(uid, PartsContainerId);
            _container.Insert(args.Used, container);
            _popup.PopupEntity(Loc.GetString("borg-construction-part-installed"), uid, args.User);
            args.Handled = true;

            var limbContainer = _container.EnsureContainer<Container>(uid, PartsContainerId);
            if (limbContainer.ContainedEntities.Count >= 6)
            {
                component.Stage = ConstructionStage.LimbsInstalled;
                Dirty(uid, component);
            }
            return;
        }

        _popup.PopupEntity(Loc.GetString("borg-construction-invalid-part"), uid, args.User);
    }

    private void TryInstallChest(EntityUid uid, BorgConstructionComponent component, InteractUsingEvent args)
    {
        if (!_tool.HasQuality(args.Used, "Screwing"))
            return;

        if (!component.CableInserted)
        {
            _popup.PopupEntity(Loc.GetString("borg-material-need-cable"), uid, args.User);
            return;
        }

        component.Stage = ConstructionStage.ChestInstalled;
        Dirty(uid, component);
        _popup.PopupEntity(Loc.GetString("borg-construction-chest-installed"), uid, args.User);
        args.Handled = true;
    }

    private void TryInstallHead(EntityUid uid, BorgConstructionComponent component, InteractUsingEvent args)
    {
        if (!_tool.HasQuality(args.Used, "Screwing"))
            return;

        if (component.FlashesInserted < 2)
        {
            _popup.PopupEntity(Loc.GetString("borg-material-need-flash", ("need", 2 - component.FlashesInserted)), uid, args.User);
            return;
        }

        component.Stage = ConstructionStage.HeadInstalled;
        Dirty(uid, component);
        _popup.PopupEntity(Loc.GetString("borg-construction-head-installed"), uid, args.User);
        args.Handled = true;
    }

    private void TryConfigure(EntityUid uid, BorgConstructionComponent component, InteractUsingEvent args)
    {
        // Multitool: cycle through configuration mode and toggle settings
        if (_tool.HasQuality(args.Used, "Multitool"))
        {
            if (component.Stage == ConstructionStage.HeadInstalled)
                component.Stage = ConstructionStage.Configuration;

            if (component.LawSyncEnabled && component.AiSyncEnabled && component.LocomotionEnabled && !component.PanelLocked)
            {
                component.LawSyncEnabled = false;
                _popup.PopupEntity(Loc.GetString("borg-config-law-sync-off"), uid, args.User);
            }
            else if (!component.LawSyncEnabled && component.AiSyncEnabled && component.LocomotionEnabled && !component.PanelLocked)
            {
                component.LawSyncEnabled = true;
                component.AiSyncEnabled = false;
                _popup.PopupEntity(Loc.GetString("borg-config-ai-sync-off"), uid, args.User);
            }
            else if (component.LawSyncEnabled && !component.AiSyncEnabled && component.LocomotionEnabled && !component.PanelLocked)
            {
                component.AiSyncEnabled = true;
                component.LocomotionEnabled = false;
                _popup.PopupEntity(Loc.GetString("borg-config-locomotion-off"), uid, args.User);
            }
            else if (component.LawSyncEnabled && component.AiSyncEnabled && !component.LocomotionEnabled && !component.PanelLocked)
            {
                component.LocomotionEnabled = true;
                component.PanelLocked = true;
                _popup.PopupEntity(Loc.GetString("borg-config-panel-locked"), uid, args.User);
            }
            else
            {
                component.LawSyncEnabled = true;
                component.AiSyncEnabled = true;
                component.LocomotionEnabled = true;
                component.PanelLocked = false;
                _popup.PopupEntity(Loc.GetString("borg-config-reset"), uid, args.User);
            }

            Dirty(uid, component);
            args.Handled = true;
            return;
        }

        if (_tool.HasQuality(args.Used, "Writing") && EntityManager.TryGetComponent(args.Used, out MetaDataComponent? usedMeta))
        {
            var penName = usedMeta.EntityName;
            if (penName != null && penName.Length > 0)
            {
                component.ConfigName = penName;
                _metaData.SetEntityName(uid, penName);
                Dirty(uid, component);
                _popup.PopupEntity(Loc.GetString("borg-config-name-set", ("name", penName)), uid, args.User);
                args.Handled = true;
                return;
            }
        }

        if (_tool.HasQuality(args.Used, "Screwing"))
        {
            if (component.Stage == ConstructionStage.Configuration)
            {
                _popup.PopupEntity(Loc.GetString("borg-config-applied",
                    ("lawSync", component.LawSyncEnabled),
                    ("aiSync", component.AiSyncEnabled),
                    ("locomotion", component.LocomotionEnabled),
                    ("panelLock", component.PanelLocked)), uid, args.User);
            }
            return;
        }

        if (HasComp<MMIComponent>(args.Used) || HasComp<BorgBrainComponent>(args.Used))
        {
            TryInsertMMI(uid, component, args);
        }
    }

    private void TryInsertMMI(EntityUid uid, BorgConstructionComponent component, InteractUsingEvent args)
    {
        if (!HasComp<MMIComponent>(args.Used) && !HasComp<BorgBrainComponent>(args.Used))
            return;

        var container = _container.EnsureContainer<ContainerSlot>(uid, BrainContainerId);
        if (container.ContainedEntity != null)
        {
            _popup.PopupEntity(Loc.GetString("borg-construction-brain-slot-occupied"), uid, args.User);
            return;
        }

        _container.Insert(args.Used, container);
        var borgComp = EnsureComp<BorgComponent>(uid);
        borgComp.BrainEntity = args.Used;
        component.Stage = ConstructionStage.MMIInserted;
        Dirty(uid, component);

        if (HasComp<SyndicateMMIComponent>(args.Used))
        {
            _law.ApplySyndicateOverride(uid, args.User);
            _popup.PopupEntity(Loc.GetString("borg-construction-syndicate-mmi-inserted"), uid, args.User);
        }

        var brainName = MetaData(args.Used).EntityName;
        _popup.PopupEntity(Loc.GetString("borg-construction-brain-inserted", ("name", brainName)), uid, args.User);
        args.Handled = true;
    }

    private void FinalizeConstruction(EntityUid uid, BorgConstructionComponent component, InteractUsingEvent args)
    {
        if (!_tool.HasQuality(args.Used, "Screwing"))
            return;

        component.Stage = ConstructionStage.Complete;
        var borgComp = EnsureComp<BorgComponent>(uid);
        borgComp.IsActive = true;
        Dirty(uid, component);
        Dirty(uid, borgComp);
        _popup.PopupEntity(Loc.GetString("borg-construction-complete"), uid, args.User);
        args.Handled = true;
    }

    private void TryDeconstruct(EntityUid uid, InteractUsingEvent args)
    {
        if (!TryComp<BorgComponent>(uid, out var borg))
            return;

        if (!borg.MaintenancePanelOpen)
        {
            if (_tool.HasQuality(args.Used, "Screwing"))
            {
                _borg.SetActive(uid, !borg.MaintenancePanelOpen);
                borg.MaintenancePanelOpen = !borg.MaintenancePanelOpen;
                Dirty(uid, borg);
                args.Handled = true;
            }
            return;
        }

        if (_tool.HasQuality(args.Used, "Prying") && _battery.HasCharge(uid, 0))
        {
            RemoveBattery(uid);
            _popup.PopupEntity(Loc.GetString("borg-decon-battery-removed"), uid, args.User);
            args.Handled = true;
            return;
        }

        if (_tool.HasQuality(args.Used, "Screwing") && !_battery.HasCharge(uid, 0))
        {
            if (TryComp<BorgWiresComponent>(uid, out var wires))
            {
                wires.PanelOpen = !wires.PanelOpen;
                Dirty(uid, wires);
            }
            _popup.PopupEntity(Loc.GetString("borg-decon-wires-exposed"), uid, args.User);
            args.Handled = true;
            return;
        }

        if (_tool.HasQuality(args.Used, "Prying"))
        {
            if (TryComp<BorgWiresComponent>(uid, out var wires) && AreAllWiresCut(wires))
            {
                EjectMMI(uid, borg);
                _popup.PopupEntity(Loc.GetString("borg-decon-mmi-removed"), uid, args.User);
                args.Handled = true;
            }
            return;
        }
    }

    private static bool AreAllWiresCut(BorgWiresComponent wires)
    {
        return wires.Wires.Count > 0 && wires.Wires.All(w => w.IsCut);
    }

    private void RemoveBattery(EntityUid uid)
    {
        if (_container.TryGetContainer(uid, CellContainerId, out var container))
        {
            foreach (var ent in container.ContainedEntities)
            {
                _container.Remove(ent, container);
                _transform.DropNextTo(ent, uid);
            }
        }
    }

    private void EjectMMI(EntityUid uid, BorgComponent borg)
    {
        if (borg.BrainEntity != null && _container.TryGetContainer(uid, BrainContainerId, out var container))
        {
            _container.Remove(borg.BrainEntity.Value, container);
            _transform.DropNextTo(borg.BrainEntity.Value, uid);
            borg.BrainEntity = null;
            Dirty(uid, borg);
        }

        SpawnPartsFromContainer(uid);
        QueueDel(uid);
    }

    private void SpawnPartsFromContainer(EntityUid uid)
    {
        if (!_container.TryGetContainer(uid, PartsContainerId, out var container))
            return;

        foreach (var part in container.ContainedEntities.ToArray())
        {
            _container.Remove(part, container);
            _transform.DropNextTo(part, uid);
        }

        var coords = _transform.GetMapCoordinates(uid);
        Spawn("CyborgEndoskeleton", coords);
    }

    public void SetConstructionStage(EntityUid uid, ConstructionStage stage, BorgConstructionComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.Stage = stage;
        Dirty(uid, component);
    }
}
