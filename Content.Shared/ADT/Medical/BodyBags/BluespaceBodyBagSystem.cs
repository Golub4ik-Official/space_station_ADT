using Content.Shared.DoAfter;
using Content.Shared.Foldable;
using Content.Shared.Item;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Popups;
using Content.Shared.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;

namespace Content.Shared.ADT.Medical.BodyBags;

[Serializable, NetSerializable]
public sealed partial class BluespaceBodyBagDoAfterEvent : SimpleDoAfterEvent
{
}

public sealed class BluespaceBodyBagSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly FoldableSystem _foldable = default!;
    [Dependency] private readonly SharedEntityStorageSystem _storage = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private const float EscapeTime = 5f;

    public override void Initialize()
    {
        SubscribeLocalEvent<BluespaceBodyBagComponent, FoldAttemptEvent>(OnFoldAttempt, after: [typeof(SharedEntityStorageSystem)]);
        SubscribeLocalEvent<BluespaceBodyBagComponent, StorageAfterCloseEvent>(OnStorageChanged);
        SubscribeLocalEvent<BluespaceBodyBagComponent, StorageAfterOpenEvent>(OnStorageChanged);
        SubscribeLocalEvent<BluespaceBodyBagComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BluespaceBodyBagComponent, ContainerRelayMovementEntityEvent>(OnRelayMovement);
        SubscribeLocalEvent<BluespaceBodyBagComponent, BluespaceBodyBagDoAfterEvent>(OnEscapeDoAfter);
    }

    private void OnMapInit(Entity<BluespaceBodyBagComponent> ent, ref MapInitEvent args)
    {
        UpdateWClass(ent);
    }

    private void OnFoldAttempt(Entity<BluespaceBodyBagComponent> ent, ref FoldAttemptEvent args)
    {
        if (!TryComp<EntityStorageComponent>(ent, out var storage))
            return;

        // Reset base EntityStorage check — bluespace bag has its own fold rules
        args.Cancelled = false;

        if (storage.Open)
        {
            args.Cancelled = true;
            return;
        }

        // Prevent folding from inside the bag
        if (args.User != null)
        {
            foreach (var contained in storage.Contents.ContainedEntities)
            {
                if (contained == args.User.Value)
                {
                    args.Cancelled = true;
                    args.CancelledMessage = Loc.GetString("bluespace-body-bag-fold-from-inside");
                    return;
                }
            }
        }

        if (storage.Contents.ContainedEntities.Count > storage.Capacity * 0.5f)
        {
            args.Cancelled = true;
            args.CancelledMessage = Loc.GetString("bluespace-body-bag-too-full");
            return;
        }

        foreach (var contained in storage.Contents.ContainedEntities)
        {
            if (HasComp<BluespaceBodyBagComponent>(contained) || HasComp<EntityStorageComponent>(contained))
            {
                args.Cancelled = true;
                args.CancelledMessage = Loc.GetString("bluespace-body-bag-recursion");
                return;
            }
        }

    }

    private void OnRelayMovement(Entity<BluespaceBodyBagComponent> ent, ref ContainerRelayMovementEntityEvent args)
    {
        if (!TryComp<EntityStorageComponent>(ent, out var storage) || storage.Open)
            return;

        if (!HasComp<MobStateComponent>(args.Entity))
            return;

        if (HasComp<ActiveDoAfterComponent>(args.Entity))
            return;

        // Only need to escape if the bag is folded (unfolded bag can be opened from outside)
        if (!TryComp<FoldableComponent>(ent, out var fold) || !fold.IsFolded)
            return;

        _popup.PopupClient(Loc.GetString("bluespace-body-bag-escape-start"), ent, args.Entity);

        var doAfterArgs = new DoAfterArgs(EntityManager, args.Entity, TimeSpan.FromSeconds(EscapeTime), new BluespaceBodyBagDoAfterEvent(), ent, ent)
        {
            BreakOnDamage = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnEscapeDoAfter(Entity<BluespaceBodyBagComponent> ent, ref BluespaceBodyBagDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!TryComp<EntityStorageComponent>(ent, out var storage) || storage.Open)
            return;

        args.Handled = true;

        // 1. Eject from parent container first (before unfolding)
        var uid = ent.Owner;
        if (_container.TryGetContainingContainer((uid, null, null), out var container))
        {
            _container.RemoveEntity(container.Owner, uid);
        }

        // 2. Unfold if folded
        if (TryComp<FoldableComponent>(ent, out var foldable) && foldable.IsFolded)
            _foldable.SetFolded(ent, foldable, false);

        // 3. Open the bag
        _storage.OpenStorage(ent, storage);

        _popup.PopupClient(Loc.GetString("bluespace-body-bag-escape-success"), ent, args.User);
    }

    private void OnStorageChanged<T>(Entity<BluespaceBodyBagComponent> ent, ref T args) where T : struct
    {
        UpdateWClass(ent);
    }

    private void UpdateWClass(Entity<BluespaceBodyBagComponent> ent)
    {
        if (!TryComp<EntityStorageComponent>(ent, out var storage) || !TryComp<ItemComponent>(ent, out var item))
            return;

        if (storage.Contents.ContainedEntities.Count > 0)
            _item.SetSize(ent, "Huge", item);
        else
            _item.SetSize(ent, "Small", item);
    }
}
