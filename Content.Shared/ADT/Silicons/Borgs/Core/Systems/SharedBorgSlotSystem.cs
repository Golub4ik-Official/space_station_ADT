// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.Core.Events;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Systems;

public abstract class SharedBorgSlotSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<BorgSlotComponent, ComponentInit>(OnSlotInit);
    }

    private void OnSlotInit(EntityUid uid, BorgSlotComponent component, ComponentInit args)
    {
        for (int i = 0; i < BorgSlotComponent.MaxSlots; i++)
        {
            component[i] = EntityUid.Invalid;
        }
        component.CurrentSlot = 0;
    }

    public bool SetActiveSlot(EntityUid uid, int slot, BorgSlotComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (slot < 0 || slot >= BorgSlotComponent.MaxSlots)
            return false;

        if (component.CycleLocked)
            return false;

        var oldSlot = component.CurrentSlot;
        component.CurrentSlot = slot;
        Dirty(uid, component);

        var ev = new BorgSlotChangedEvent(uid, oldSlot, slot);
        RaiseLocalEvent(uid, ref ev);

        return true;
    }

    public bool CycleForward(EntityUid uid, BorgSlotComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        var next = (component.CurrentSlot + 1) % BorgSlotComponent.MaxSlots;
        return SetActiveSlot(uid, next, component);
    }

    public bool CycleBackward(EntityUid uid, BorgSlotComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        var prev = (component.CurrentSlot - 1 + BorgSlotComponent.MaxSlots) % BorgSlotComponent.MaxSlots;
        return SetActiveSlot(uid, prev, component);
    }

    public EntityUid GetActiveItem(EntityUid uid, BorgSlotComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return EntityUid.Invalid;

        if (component.CurrentSlot < 0 || component.CurrentSlot >= component.Count)
            return EntityUid.Invalid;

        return component[component.CurrentSlot];
    }

    public bool IsSlotEmpty(EntityUid uid, int slot, BorgSlotComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return true;

        if (slot < 0 || slot >= component.Count)
            return true;

        return component[slot] == EntityUid.Invalid;
    }
}
