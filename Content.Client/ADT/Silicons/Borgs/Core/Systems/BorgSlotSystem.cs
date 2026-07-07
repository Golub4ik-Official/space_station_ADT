using Content.Client.ADT.Silicons.Borgs.Core.UI;
using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.Core.Events;
using Content.Shared.ADT.Silicons.Borgs.Core.Systems;

namespace Content.Client.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgSlotSystem : SharedBorgSlotSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgSlotComponent, BorgSlotChangedEvent>(OnSlotChanged);
    }

    private void OnSlotChanged(EntityUid uid, BorgSlotComponent component, BorgSlotChangedEvent args)
    {
        Dirty(uid, component);
    }

    public int GetActiveSlot(EntityUid uid, BorgSlotComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return -1;

        return component.CurrentSlot;
    }

    public EntityUid? GetItemInSlot(EntityUid uid, int slotIndex, BorgSlotComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return null;

        if (slotIndex < 0 || slotIndex >= component.Count)
            return null;

        var item = component[slotIndex];
        return item == EntityUid.Invalid ? null : item;
    }

    public string GetSlotItemName(EntityUid uid, int slotIndex, BorgSlotComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return "Empty";

        if (slotIndex < 0 || slotIndex >= component.Count)
            return "Empty";

        var item = component[slotIndex];
        if (item == EntityUid.Invalid || !EntityManager.EntityExists(item))
            return "Empty";

        return MetaData(item).EntityName;
    }

    public bool IsModuleInstalled(EntityUid uid, string moduleType, BorgSlotComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        for (int i = 0; i < component.Count; i++)
        {
            var item = component[i];
            if (item == EntityUid.Invalid || !EntityManager.EntityExists(item))
                continue;

            if (TryComp<ADTBorgModuleComponent>(item, out var module)
                && module.ModuleType == moduleType)
                return true;
        }

        return false;
    }
}
