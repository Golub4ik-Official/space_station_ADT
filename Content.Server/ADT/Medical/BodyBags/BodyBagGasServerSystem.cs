using Content.Shared.ADT.Medical.BodyBags;
using Content.Shared.Atmos;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.StatusEffectNew;
using Content.Shared.Storage.Components;
using Robust.Shared.Timing;

namespace Content.Server.ADT.Medical.BodyBags;

public sealed class BodyBagGasServerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    private const float N2ODelay = 0.2f;
    private const float BreathableRegenInterval = 5f;
    private const float MedicalHealRegenInterval = 1f;
    private const float SedativeSleepRegenInterval = 1f;

    private const float N2ORatio = 0.79f;

    private readonly HashSet<EntityUid> _pendingN2OFill = new();
    private readonly HashSet<EntityUid> _breathableBags = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<BodyBagGasComponent, StorageAfterCloseEvent>(OnStorageClosed);
        SubscribeLocalEvent<BodyBagGasComponent, EntityTerminatingEvent>(OnEntityTerminating);
    }

    private void OnEntityTerminating(Entity<BodyBagGasComponent> ent, ref EntityTerminatingEvent args)
    {
        _pendingN2OFill.Remove(ent);
        _breathableBags.Remove(ent);
    }

    public override void Update(float frameTime)
    {
        var time = _timing.CurTime;

        if (_pendingN2OFill.Count > 0)
            ProcessN2OFills(time);

        if (_breathableBags.Count > 0)
            ProcessBreathableBags(frameTime);
    }

    private void OnStorageClosed(Entity<BodyBagGasComponent> ent, ref StorageAfterCloseEvent args)
    {
        if (ent.Comp.FillMode == BodyBagGasFillMode.None)
            return;

        if (!TryComp<EntityStorageComponent>(ent, out var storage))
            return;

        if (storage.Air == null)
            return;

        if (ent.Comp.FillMode == BodyBagGasFillMode.N2O)
        {
            _pendingN2OFill.Add(ent);
            return;
        }

        FillGas(ent, storage);
        _breathableBags.Add(ent);
    }

    private void ProcessN2OFills(TimeSpan curTime)
    {
        var completed = new HashSet<EntityUid>();

        foreach (var uid in _pendingN2OFill)
        {
            if (!TryComp<BodyBagGasComponent>(uid, out var gasComp))
            {
                completed.Add(uid);
                continue;
            }

            if (gasComp.N2OFillTime == null)
            {
                gasComp.N2OFillTime = curTime + TimeSpan.FromSeconds(N2ODelay);
                continue;
            }

            if (curTime < gasComp.N2OFillTime.Value)
                continue;

            if (TryComp<EntityStorageComponent>(uid, out var storage) && storage.Air != null)
                FillGas((uid, gasComp), storage);

            completed.Add(uid);
        }

        foreach (var uid in completed)
            _pendingN2OFill.Remove(uid);
    }

    private void ProcessBreathableBags(float frameTime)
    {
        var toRemove = new HashSet<EntityUid>();

        foreach (var uid in _breathableBags)
        {
            if (!TryComp<BodyBagGasComponent>(uid, out var gasComp)
                || (gasComp.FillMode != BodyBagGasFillMode.Breathable
                    && gasComp.FillMode != BodyBagGasFillMode.MedicalHeal
                    && gasComp.FillMode != BodyBagGasFillMode.SedativeSleep))
            {
                toRemove.Add(uid);
                continue;
            }

            if (!TryComp<EntityStorageComponent>(uid, out var storage))
            {
                toRemove.Add(uid);
                continue;
            }

            if (storage.Open || storage.Air == null)
            {
                toRemove.Add(uid);
                continue;
            }

            EntityUid? occupant = null;
            foreach (var contained in storage.Contents.ContainedEntities)
            {
                if (HasComp<MobStateComponent>(contained))
                {
                    occupant = contained;
                    break;
                }
            }

            if (occupant == null)
            {
                gasComp.TimeUntilNextRegen = 0f;
                continue;
            }

            var interval = gasComp.FillMode switch
            {
                BodyBagGasFillMode.MedicalHeal => MedicalHealRegenInterval,
                BodyBagGasFillMode.SedativeSleep => SedativeSleepRegenInterval,
                _ => BreathableRegenInterval
            };

            gasComp.TimeUntilNextRegen -= frameTime;
            if (gasComp.TimeUntilNextRegen <= 0f)
            {
                FillGas((uid, gasComp), storage);
                gasComp.TimeUntilNextRegen = interval;

                switch (gasComp.FillMode)
                {
                    case BodyBagGasFillMode.MedicalHeal:
                        ApplyMedicalHeal(occupant.Value);
                        break;
                    case BodyBagGasFillMode.SedativeSleep:
                        ApplySedativeSleep(occupant.Value);
                        break;
                }
            }
        }

        foreach (var uid in toRemove)
            _breathableBags.Remove(uid);
    }

    private void ApplyMedicalHeal(EntityUid occupant)
    {
        var heal = new DamageSpecifier();
        heal.DamageDict.Add("Blunt", -0.08);
        heal.DamageDict.Add("Slash", -0.08);
        heal.DamageDict.Add("Piercing", -0.08);
        heal.DamageDict.Add("Heat", -0.08);
        heal.DamageDict.Add("Shock", -0.08);
        heal.DamageDict.Add("Poison", -0.10);
        heal.DamageDict.Add("Asphyxiation", -1.00);
        _damageable.TryChangeDamage((occupant, null), heal, true, false);
    }

    private void ApplySedativeSleep(EntityUid occupant)
    {
        _statusEffects.TryAddStatusEffectDuration(occupant, SleepingSystem.StatusEffectForcedSleeping, TimeSpan.FromSeconds(3));
    }

    private void FillGas(Entity<BodyBagGasComponent> ent, EntityStorageComponent storage)
    {
        var moles = (Atmospherics.OneAtmosphere * storage.Air.Volume) / (Atmospherics.R * Atmospherics.T20C);

        storage.Air.Clear();
        storage.Air.Temperature = Atmospherics.T20C;

        switch (ent.Comp.FillMode)
        {
            case BodyBagGasFillMode.N2O:
                storage.Air.SetMoles(Gas.Oxygen, moles * Atmospherics.OxygenStandard);
                storage.Air.SetMoles(Gas.NitrousOxide, moles * N2ORatio);
                break;

            case BodyBagGasFillMode.Breathable:
            case BodyBagGasFillMode.MedicalHeal:
            case BodyBagGasFillMode.SedativeSleep:
                storage.Air.SetMoles(Gas.Oxygen, moles * Atmospherics.OxygenStandard);
                storage.Air.SetMoles(Gas.Nitrogen, moles * Atmospherics.NitrogenStandard);
                break;
        }
    }
}
