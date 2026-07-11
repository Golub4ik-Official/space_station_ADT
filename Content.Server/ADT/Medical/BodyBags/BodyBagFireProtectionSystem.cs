using Content.Server.ADT.Temperature;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.ADT.Medical.BodyBags;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Storage.Components;
using Robust.Shared.Containers;

namespace Content.Server.ADT.Medical.BodyBags;

public sealed class BodyBagFireProtectionSystem : EntitySystem
{
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<FlammableComponent, GetFireProtectionEvent>(OnGetFireProtection);
        SubscribeLocalEvent<FlammableComponent, OnFireChangedEvent>(OnFireChanged);
    }

    private void OnGetFireProtection(Entity<FlammableComponent> ent, ref GetFireProtectionEvent args)
    {
        if (IsInProtectedBag(ent))
        {
            args.Reduce(1f);
            return;
        }

        // Elite body bags (BodyBagEnvironmentalNanotresen) are fire-immune
        if (HasComp<BodyBagEliteComponent>(ent))
            args.Reduce(1f);
    }

    private void OnFireChanged(Entity<FlammableComponent> ent, ref OnFireChangedEvent args)
    {
        if (!args.OnFire)
            return;

        if (!IsInProtectedBag(ent))
            return;

        _flammable.Extinguish(ent, ent.Comp);
    }

    private bool IsInProtectedBag(EntityUid uid)
    {
        Entity<TransformComponent?, MetaDataComponent?> containerCheckEnt = (uid, null, null);
        if (!_container.TryGetContainingContainer(containerCheckEnt, out var container))
            return false;

        var containerEntity = container.Owner;
        if (!HasComp<BodyBagGasComponent>(containerEntity))
            return false;

        if (!TryComp<EntityStorageComponent>(containerEntity, out var storage) || storage.Open)
            return false;

        return true;
    }
}
