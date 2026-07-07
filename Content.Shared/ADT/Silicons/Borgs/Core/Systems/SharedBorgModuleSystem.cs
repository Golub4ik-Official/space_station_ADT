// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core.Components;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Systems;

public abstract class SharedBorgModuleSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<ADTBorgModuleComponent, ComponentInit>(OnModuleInit);
    }

    private void OnModuleInit(EntityUid uid, ADTBorgModuleComponent component, ComponentInit args)
    {
        Dirty(uid, component);
    }

    public void SetModuleType(EntityUid uid, string moduleType, ADTBorgModuleComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.ModuleType = moduleType;
        Dirty(uid, component);
    }

    public void SetEnergyStorageAmount(EntityUid uid, int storageIndex, float amount, ADTBorgModuleComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (storageIndex < 0 || storageIndex >= component.EnergyStorages.Count)
            return;

        var entry = component.EnergyStorages[storageIndex];
        entry.CurrentAmount = Math.Clamp(amount, 0, entry.MaxAmount);
        component.EnergyStorages[storageIndex] = entry;
        Dirty(uid, component);
    }

    public bool TryUseEnergy(EntityUid uid, string storageName, float amount, ADTBorgModuleComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        for (int i = 0; i < component.EnergyStorages.Count; i++)
        {
            var entry = component.EnergyStorages[i];
            if (entry.Name != storageName)
                continue;

            if (entry.CurrentAmount < amount)
                return false;

            entry.CurrentAmount -= amount;
            component.EnergyStorages[i] = entry;
            Dirty(uid, component);
            return true;
        }

        return false;
    }

    public float GetEnergyAmount(EntityUid uid, string storageName, ADTBorgModuleComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return 0;

        foreach (var entry in component.EnergyStorages)
        {
            if (entry.Name == storageName)
                return entry.CurrentAmount;
        }

        return 0;
    }
}
