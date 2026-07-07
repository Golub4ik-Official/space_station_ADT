// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.Core.Events;
using Content.Shared.ADT.Silicons.Borgs.Core.Systems;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Storage;
using Robust.Shared.Containers;

namespace Content.Server.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgSlotSystem : SharedBorgSlotSystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgSlotComponent, BorgSlotChangedEvent>(OnSlotChangedServer);
    }

    private void OnSlotChangedServer(EntityUid uid, BorgSlotComponent component, BorgSlotChangedEvent args)
    {
        Dirty(uid, component);
        DeactivateCurrentItem(uid, args.OldSlot, component);
        ActivateItemInSlot(uid, args.NewSlot, component);
    }

    private void DeactivateCurrentItem(EntityUid uid, int slotIndex, BorgSlotComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (slotIndex < 0 || slotIndex >= component.Count)
            return;

        var item = component[slotIndex];
        if (item == EntityUid.Invalid)
            return;

        _container.RemoveEntity(uid, item);
        _transform.DropNextTo(item, uid);
    }

    private void ActivateItemInSlot(EntityUid uid, int slotIndex, BorgSlotComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (slotIndex < 0 || slotIndex >= component.Count)
            return;

        var item = component[slotIndex];
        if (item == EntityUid.Invalid)
            return;

        if (_hands.TryPickup(uid, item))
            return;

        _container.Insert(item, _container.EnsureContainer<ContainerSlot>(uid, $"borg_slot_{slotIndex}"));
    }

    public bool SetSlotItem(EntityUid uid, int slotIndex, EntityUid item, BorgSlotComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (slotIndex < 0 || slotIndex >= BorgSlotComponent.MaxSlots)
            return false;

        component[slotIndex] = item;
        Dirty(uid, component);
        return true;
    }

    public void ClearSlot(EntityUid uid, int slotIndex, BorgSlotComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (slotIndex < 0 || slotIndex >= component.Count)
            return;

        component[slotIndex] = EntityUid.Invalid;
        Dirty(uid, component);
    }
}
