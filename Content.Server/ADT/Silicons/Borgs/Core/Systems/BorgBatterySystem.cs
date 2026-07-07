// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.Power;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;
using Robust.Shared.Containers;

namespace Content.Server.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgBatterySystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly SharedBatterySystem _battery = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgBatteryComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BorgBatteryComponent, EntInsertedIntoContainerMessage>(OnCellInserted);
        SubscribeLocalEvent<BorgBatteryComponent, EntRemovedFromContainerMessage>(OnCellRemoved);
    }

    private void OnMapInit(EntityUid uid, BorgBatteryComponent component, MapInitEvent args)
    {
        var container = _container.EnsureContainer<ContainerSlot>(uid, component.CellContainerId);

        if (container.ContainedEntity == null)
        {
            component.CellUid = null;
            component.Charge = 0;
            component.MaxCharge = 0;
        }
        else
        {
            UpdateChargeFromCell(uid, component);
        }

        Dirty(uid, component);
    }

    private void OnCellInserted(EntityUid uid, BorgBatteryComponent component, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != component.CellContainerId)
            return;

        component.CellUid = args.Entity;
        UpdateChargeFromCell(uid, component);
        Dirty(uid, component);
    }

    private void OnCellRemoved(EntityUid uid, BorgBatteryComponent component, EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != component.CellContainerId)
            return;

        component.CellUid = null;
        component.Charge = 0;
        component.MaxCharge = 0;
        Dirty(uid, component);
    }

    public void UpdateChargeFromCell(EntityUid uid, BorgBatteryComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (component.CellUid == null)
        {
            component.Charge = 0;
            component.MaxCharge = 0;
            return;
        }

        if (_powerCell.TryGetBatteryFromSlot(uid, out var battery))
        {
            var bEntity = battery.Value.AsNullable();
            component.Charge = _battery.GetCharge(bEntity);
            component.MaxCharge = battery.Value.Comp.MaxCharge;
        }
    }

    public bool TryDrainPower(EntityUid uid, float amount, BorgBatteryComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (component.CellUid == null)
            return false;

        if (_powerCell.TryGetBatteryFromSlot(uid, out var battery))
        {
            var bEntity = battery.Value.AsNullable();
            return _battery.TryUseCharge(bEntity, amount);
        }

        return false;
    }

    public bool HasCharge(EntityUid uid, float threshold = 0, BorgBatteryComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        return component.Charge > threshold;
    }

    public bool IsLowPower(EntityUid uid, BorgBatteryComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return true;

        if (component.MaxCharge <= 0)
            return true;

        return component.Charge / component.MaxCharge < component.LowPowerThreshold;
    }
}
