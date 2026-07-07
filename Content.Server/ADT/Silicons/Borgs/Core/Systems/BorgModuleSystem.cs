// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core;
using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.Core.Systems;
using Content.Shared.Storage;
using Robust.Shared.Prototypes;

namespace Content.Server.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgModuleSystem : SharedBorgModuleSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly BorgSlotSystem _slots = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public bool TryInstallModule(EntityUid uid, EntProtoId moduleId, BorgSlotComponent? slotComp = null,
        BorgComponent? borgComp = null)
    {
        if (!Resolve(uid, ref slotComp) || !Resolve(uid, ref borgComp))
            return false;

        if (!_proto.TryIndex(moduleId, out var _))
            return false;

        if (slotComp.ModuleWhitelist.Count > 0 && !slotComp.ModuleWhitelist.Contains(moduleId))
            return false;

        if (slotComp.MaxModules > 0 && GetModuleCount(uid, slotComp) >= slotComp.MaxModules)
            return false;

        var moduleUid = Spawn(moduleId, _transform.GetMapCoordinates(uid));
        var moduleComp = EnsureComp<ADTBorgModuleComponent>(moduleUid);

        PopulateItemsFromModule(uid, moduleUid, moduleComp, slotComp);
        return true;
    }

    public bool TryInstallModule(EntityUid uid, EntityUid moduleUid, BorgSlotComponent? slotComp = null,
        BorgComponent? borgComp = null)
    {
        if (!Resolve(uid, ref slotComp) || !Resolve(uid, ref borgComp))
            return false;

        if (!TryComp<ADTBorgModuleComponent>(moduleUid, out var moduleComp))
            return false;

        if (!_proto.TryIndex(moduleComp.ModuleType, out var _))
            return false;

        if (slotComp.ModuleWhitelist.Count > 0 && !slotComp.ModuleWhitelist.Contains(moduleComp.ModuleType))
            return false;

        if (slotComp.MaxModules > 0 && GetModuleCount(uid, slotComp) >= slotComp.MaxModules)
            return false;

        PopulateItemsFromModule(uid, moduleUid, moduleComp, slotComp);
        return true;
    }

    private int GetModuleCount(EntityUid uid, BorgSlotComponent component)
    {
        var count = 0;
        for (int i = 0; i < component.Count; i++)
        {
            if (component[i] != EntityUid.Invalid)
                count++;
        }
        return count;
    }

    private void PopulateItemsFromModule(EntityUid uid, EntityUid moduleUid, ADTBorgModuleComponent moduleComp, BorgSlotComponent slotComp)
    {
        var emagged = HasComp<BorgEmagComponent>(uid);
        var weaponsUnlocked = emagged && TryComp<BorgEmagComponent>(uid, out var emagComp) && emagComp.WeaponsUnlocked;

        var itemsToSpawn = new List<EntProtoId>();
        itemsToSpawn.AddRange(moduleComp.Items);

        if (emagged)
            itemsToSpawn.AddRange(moduleComp.EmagItems);

        if (weaponsUnlocked)
            itemsToSpawn.AddRange(moduleComp.OverrideItems);

        for (int i = 0; i < itemsToSpawn.Count && i < BorgSlotComponent.MaxSlots; i++)
        {
            var item = Spawn(itemsToSpawn[i], _transform.GetMapCoordinates(uid));
            slotComp[i] = item;
        }

        Dirty(uid, slotComp);

        if (TryComp<BorgComponent>(uid, out var borgComp))
        {
            borgComp.EnergyStorages.Clear();
            foreach (var storage in moduleComp.EnergyStorages)
            {
                borgComp.EnergyStorages.Add(new BorgEnergyStorageEntry
                {
                    Name = storage.Name,
                    MaxAmount = storage.MaxAmount,
                    RechargeRate = storage.RechargeRate,
                    CurrentAmount = storage.MaxAmount
                });
            }
            Dirty(uid, borgComp);
        }
    }

    public void RemoveModule(EntityUid uid, EntityUid moduleUid, BorgSlotComponent? slotComp = null)
    {
        if (!Resolve(uid, ref slotComp))
            return;

        for (int i = 0; i < slotComp.Count; i++)
        {
            slotComp[i] = EntityUid.Invalid;
        }

        Del(moduleUid);
        Dirty(uid, slotComp);

        if (TryComp<BorgComponent>(uid, out var borgComp))
        {
            borgComp.EnergyStorages.Clear();
            Dirty(uid, borgComp);
        }
    }
}
