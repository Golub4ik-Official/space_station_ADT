using Content.Shared.ADT.Medical.BodyBags;
using Content.Shared.EntityTable;
using Content.Shared.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Prototypes;

namespace Content.Server.ADT.Medical.BodyBags;

/// <summary>
/// Спавнит тело и случайный лут в мешке LostCrew при первом открытии
/// или при инициализации карты (если <see cref="LostCrewBodyBagComponent.SpawnBodyOnMapInit"/>).
/// </summary>
public sealed class LostCrewBodyBagSystem : EntitySystem
{
    [Dependency] private readonly SharedEntityStorageSystem _storage = default!;
    [Dependency] private readonly EntityTableSystem _entityTable = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<LostCrewBodyBagComponent, StorageAfterOpenEvent>(OnOpened);
        SubscribeLocalEvent<LostCrewBodyBagComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<LostCrewBodyBagComponent> ent, ref MapInitEvent args)
    {
        if (!ent.Comp.SpawnBodyOnMapInit)
            return;

        SpawnBodyAndLoot(ent);
    }

    private void OnOpened(Entity<LostCrewBodyBagComponent> ent, ref StorageAfterOpenEvent args)
    {
        if (ent.Comp.BodySpawned)
            return;

        SpawnBodyAndLoot(ent);
    }

    private void SpawnBodyAndLoot(Entity<LostCrewBodyBagComponent> ent)
    {
        var bag = ent.Owner;

        if (!TryComp<EntityStorageComponent>(bag, out var storage))
            return;

        ent.Comp.BodySpawned = true;
        Dirty(bag, ent.Comp);

        // Spawn random humanoid corpse inside the bag
        var corpse = EntityManager.SpawnEntity(ent.Comp.CorpsePrototype, Transform(bag).Coordinates);
        _storage.Insert(corpse, bag);

        // Spawn random loot items
        SpawnLootItems(ent, bag, storage);
    }

    private void SpawnLootItems(Entity<LostCrewBodyBagComponent> ent, EntityUid bag, EntityStorageComponent storage)
    {
        if (!_prototype.TryIndex<EntityTablePrototype>(ent.Comp.LootTable, out var table))
            return;

        var spawns = _entityTable.GetSpawns(table);
        foreach (var proto in spawns)
        {
            var item = EntityManager.SpawnEntity(proto, Transform(bag).Coordinates);
            _storage.Insert(item, bag);
        }
    }
}
