using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.DoAfter;
using Content.Shared.Foldable;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Physics;
using Content.Shared.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Containers;

namespace Content.Shared.ADT.Medical.BodyBags;

/// <summary>
/// Позволяет размещать сложенные body bag на каталках (stretcher)
/// через do-after → drop → unfold → buckle.
/// </summary>
public sealed class BodyBagStretcherSystem : EntitySystem
{
    [Dependency] private readonly SharedBuckleSystem _buckle = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly FoldableSystem _foldable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyBagStretcherComponent, InteractUsingEvent>(OnInteractUsing, before: [typeof(SharedInteractionSystem)]);
        SubscribeLocalEvent<BodyBagStretcherComponent, BodyBagStretcherDoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<FoldableComponent, FoldAttemptEvent>(OnBodyBagFoldAttempt);
        SubscribeLocalEvent<BuckleComponent, FoldAttemptEvent>(OnBuckleFoldAttempt);

        SubscribeLocalEvent<DeployFoldableComponent, ContainerGettingInsertedAttemptEvent>(OnDeployableInsertAttempt);
    }

    private void OnDeployableInsertAttempt(Entity<DeployFoldableComponent> ent, ref ContainerGettingInsertedAttemptEvent args)
    {
        if (!TryComp<FoldableComponent>(ent, out var foldable))
            return;

        if (!foldable.IsFolded)
            args.Cancel();
    }

    private void OnInteractUsing(Entity<BodyBagStretcherComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        var used = args.Used;

        if (!TryComp<FoldableComponent>(used, out var fold) || !fold.IsFolded)
            return;

        if (!HasComp<EntityStorageComponent>(used))
            return;

        var doAfterTime = 2f;
        if (TryComp<StrapComponent>(ent, out var strap))
            doAfterTime = strap.BuckleDoafterTime;

        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, doAfterTime, new BodyBagStretcherDoAfterEvent(), ent, used, ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            AttemptFrequency = AttemptFrequency.EveryTick,
            NeedHand = true,
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
            return;

        args.Handled = true;
    }

    private void OnDoAfter(Entity<BodyBagStretcherComponent> ent, ref BodyBagStretcherDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Used == null || args.Target == null)
            return;

        var used = args.Target.Value;
        var stretcher = args.Used.Value;

        if (!TryComp<FoldableComponent>(used, out var fold) || !fold.IsFolded)
            return;

        if (!HasComp<EntityStorageComponent>(used))
            return;

        var xform = Transform(stretcher);

        if (!_hands.TryDrop(args.User, used, targetDropLocation: xform.Coordinates))
            return;

        if (!_foldable.TrySetFolded(used, fold, false, args.User))
        {
            _hands.TryPickup(args.User, used);
            return;
        }

        if (TryComp<BuckleComponent>(used, out var buckleComp))
            _buckle.TryBuckle(used, args.User, stretcher, buckleComp);

        args.Handled = true;
    }

    private void OnBodyBagFoldAttempt(Entity<FoldableComponent> ent, ref FoldAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!HasComp<EntityStorageComponent>(ent))
            return;

        // Allow unfolding (e.g. when placing on stretcher)
        if (ent.Comp.IsFolded)
            return;

        var xform = Transform(ent);
        foreach (var stretcher in _lookup.GetEntitiesInRange(xform.Coordinates, 0.5f, LookupFlags.Uncontained))
        {
            if (HasComp<BodyBagStretcherComponent>(stretcher))
            {
                args.Cancelled = true;
                return;
            }
        }
    }

    private void OnBuckleFoldAttempt(Entity<BuckleComponent> ent, ref FoldAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (ent.Comp.Buckled)
            args.Cancelled = true;
    }
}
