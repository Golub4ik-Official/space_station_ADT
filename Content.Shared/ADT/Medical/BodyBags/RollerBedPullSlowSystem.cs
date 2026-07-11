using Content.Shared.Buckle.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Systems;

namespace Content.Shared.ADT.Medical.BodyBags;

public sealed class RollerBedPullSlowSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _modifier = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RollerBedPullSlowComponent, StrappedEvent>(OnStrapped);
        SubscribeLocalEvent<RollerBedPullSlowComponent, UnstrappedEvent>(OnUnstrapped);
    }

    private void OnStrapped(Entity<RollerBedPullSlowComponent> ent, ref StrappedEvent args)
    {
        RefreshPullerSpeed(ent);
    }

    private void OnUnstrapped(Entity<RollerBedPullSlowComponent> ent, ref UnstrappedEvent args)
    {
        RefreshPullerSpeed(ent);
    }

    private void RefreshPullerSpeed(EntityUid uid)
    {
        if (!TryComp<PullableComponent>(uid, out var pullable) || pullable.Puller == null)
            return;

        _modifier.RefreshMovementSpeedModifiers(pullable.Puller.Value);
    }
}
