using Content.Shared.ADT.Body.Events;
using Content.Shared.Atmos;
using Content.Shared.Body.Events;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Metabolism;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.ADT.Medical.BodyBags;

public sealed class StasisBodyBagSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedEntityStorageSystem _storage = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _modifier = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MetabolizerSystem _metabolizer = default!;

    private static readonly ProtoId<DamageTypePrototype> BluntDamageType = "Blunt";

    public override void Initialize()
    {
        SubscribeLocalEvent<StasisBodyBagComponent, StorageAfterCloseEvent>(OnClosed);
        SubscribeLocalEvent<StasisBodyBagComponent, StorageAfterOpenEvent>(OnOpened);
        SubscribeLocalEvent<StasisBodyBagComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<StasisBodyBagComponent, StasisBodyBagActiveEvent>(OnStasisActive);
        SubscribeLocalEvent<StasisBodyBagOccupantComponent, GetMetabolicMultiplierEvent>(OnGetMetabolicMultiplier);
        SubscribeLocalEvent<StasisBodyBagOccupantComponent, ThermalRegulationAttemptEvent>(OnThermalRegulationAttempt);
    }

    private void OnExamined(Entity<StasisBodyBagComponent> ent, ref ExaminedEvent args)
    {
        var integrityPercent = 100f - (ent.Comp.DamageTaken / ent.Comp.MaxIntegrity * 100);

        if (integrityPercent > 75)
        {
            args.PushMarkup(Loc.GetString("stasis-body-bag-examine-good"));
            return;
        }

        if (integrityPercent > 50)
        {
            args.PushMarkup(Loc.GetString("stasis-body-bag-examine-worn"));
            return;
        }

        if (integrityPercent > 25)
        {
            args.PushMarkup(Loc.GetString("stasis-body-bag-examine-moderately-worn"));
            return;
        }

        args.PushMarkup(Loc.GetString("stasis-body-bag-examine-falling-apart"));
    }

    private void OnClosed(Entity<StasisBodyBagComponent> ent, ref StorageAfterCloseEvent args)
    {
        var wasStasisApplied = ent.Comp.StasisApplied;

        ent.Comp.SecondsActive = 0f;
        ent.Comp.FireExtinguished = false;
        ent.Comp.StasisApplied = false;

        TryComp<EntityStorageComponent>(ent, out var storage);

        if (storage?.Air != null)
        {
            storage.Air.Temperature = Atmospherics.T0C - 60f;
        }

        if (wasStasisApplied && storage != null)
        {
            foreach (var contained in storage.Contents.ContainedEntities)
            {
                EnsureComp<StasisBodyBagOccupantComponent>(contained);
                UpdateMetabolicMultiplier(contained);
            }
        }
    }

    private void OnOpened(Entity<StasisBodyBagComponent> ent, ref StorageAfterOpenEvent args)
    {
        ent.Comp.SecondsActive = 0f;
        ent.Comp.FireExtinguished = false;
        ent.Comp.StasisApplied = false;

        if (TryComp<EntityStorageComponent>(ent, out var storage))
        {
            foreach (var contained in storage.Contents.ContainedEntities)
            {
                RemComp<StasisBodyBagOccupantComponent>(contained);
                UpdateMetabolicMultiplier(contained);
            }
        }
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<StasisBodyBagComponent, EntityStorageComponent>();
        while (query.MoveNext(out var uid, out var stasis, out var storage))
        {
            if (storage.Open)
            {
                stasis.SecondsActive = 0f;
                stasis.FireExtinguished = false;
                stasis.StasisApplied = false;
                continue;
            }

            var hasOccupant = false;
            foreach (var contained in storage.Contents.ContainedEntities)
            {
                if (!HasComp<DamageableComponent>(contained))
                    continue;

                hasOccupant = true;
                stasis.SecondsActive += frameTime;

                if (stasis.SecondsActive >= stasis.FireExtinguishDelay && !stasis.FireExtinguished)
                {
                    stasis.FireExtinguished = true;
                    var fireEv = new StasisBodyBagFireExtinguishEvent(contained);
                    RaiseLocalEvent(uid, ref fireEv);
                }

                if (stasis.SecondsActive >= stasis.StasisDelay && !stasis.StasisApplied)
                {
                    stasis.StasisApplied = true;
                    var stasisEv = new StasisBodyBagActiveEvent(contained);
                    RaiseLocalEvent(uid, ref stasisEv);
                }
            }

            if (!hasOccupant)
            {
                stasis.SecondsActive = 0f;
                continue;
            }

            if (stasis.SecondsActive >= stasis.StasisDelay)
            {
                stasis.DamageTaken += stasis.DamagePerSecond * frameTime;

                var damage = new DamageSpecifier(_proto.Index(BluntDamageType), stasis.DamagePerSecond * frameTime);
                _damageable.TryChangeDamage(uid, damage, ignoreResistances: true);
            }
        }
    }

    private void OnStasisActive(Entity<StasisBodyBagComponent> ent, ref StasisBodyBagActiveEvent args)
    {
        EnsureComp<StasisBodyBagOccupantComponent>(args.Occupant);
        UpdateMetabolicMultiplier(args.Occupant);

        if (TryComp<MovementSpeedModifierComponent>(args.Occupant, out var modifier))
            _modifier.RefreshMovementSpeedModifiers(args.Occupant);
    }

    private void OnGetMetabolicMultiplier(Entity<StasisBodyBagOccupantComponent> ent, ref GetMetabolicMultiplierEvent args)
    {
        args.Multiplier *= 10f;
    }

    private void OnThermalRegulationAttempt(Entity<StasisBodyBagOccupantComponent> ent, ref ThermalRegulationAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void UpdateMetabolicMultiplier(EntityUid uid)
    {
        _metabolizer.UpdateMetabolicMultiplier(uid);
    }
}

[ByRefEvent]
public readonly record struct StasisBodyBagActiveEvent(EntityUid Occupant);

[ByRefEvent]
public readonly record struct StasisBodyBagFireExtinguishEvent(EntityUid Occupant);
