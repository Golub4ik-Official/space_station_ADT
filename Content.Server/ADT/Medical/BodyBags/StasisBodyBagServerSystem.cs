using Content.Server.Atmos.EntitySystems;
using Content.Shared.ADT.Medical.BodyBags;
using Content.Shared.Atmos.Components;

namespace Content.Server.ADT.Medical.BodyBags;

public sealed class StasisBodyBagServerSystem : EntitySystem
{
    [Dependency] private readonly FlammableSystem _flammable = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<StasisBodyBagComponent, StasisBodyBagFireExtinguishEvent>(OnFireExtinguish);
    }

    private void OnFireExtinguish(Entity<StasisBodyBagComponent> ent, ref StasisBodyBagFireExtinguishEvent args)
    {
        if (!TryComp<FlammableComponent>(args.Occupant, out var flammable) || !flammable.OnFire)
            return;

        _flammable.Extinguish(args.Occupant, flammable);
    }
}
