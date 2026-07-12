using Content.Server.ADT.Temperature;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.ADT.Medical.BodyBags;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Storage.Components;
using Robust.Shared.Containers;

namespace Content.Server.ADT.Medical.BodyBags;

public sealed class BodyBagFireProtectionSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    private const float LavaTemperatureThreshold = 1500f;

    public override void Initialize()
    {
        SubscribeLocalEvent<FlammableComponent, GetFireProtectionEvent>(OnGetFireProtection);
        SubscribeLocalEvent<FlammableComponent, OnFireChangedEvent>(OnFireChanged);
        SubscribeLocalEvent<BodyBagGasComponent, TileFireEvent>(OnBagTileFire);
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
        {
            args.Reduce(1f);
            return;
        }
    }

    private void OnFireChanged(Entity<FlammableComponent> ent, ref OnFireChangedEvent args)
    {
        if (!args.OnFire)
            return;

        if (!IsInProtectedBag(ent))
            return;

        _flammable.Extinguish(ent, ent.Comp);
    }

    private void OnBagTileFire(Entity<BodyBagGasComponent> ent, ref TileFireEvent args)
    {
        if (HasComp<BodyBagEliteComponent>(ent))
            return;

        if (args.Temperature <= LavaTemperatureThreshold)
            return;

        var lavaDamage = new DamageSpecifier();
        lavaDamage.DamageDict.Add("Heat", 50);
        _damageable.TryChangeDamage(ent.Owner, lavaDamage);
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
