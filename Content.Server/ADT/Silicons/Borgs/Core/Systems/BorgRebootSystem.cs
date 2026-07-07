// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.StatusEffect;
using Robust.Shared.Timing;

namespace Content.Server.ADT.Silicons.Borgs.Core.Systems;

[ByRefEvent]
public readonly record struct BorgRebootEvent(EntityUid Borg);

public sealed class BorgRebootSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly BorgBatterySystem _battery = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgRebootComponent, ComponentInit>(OnRebootInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BorgRebootComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (!component.IsRebooting)
                continue;

            if (_timing.CurTime.TotalSeconds >= component.RebootTime)
            {
                FinishReboot(uid, component);
            }
        }
    }

    private void OnRebootInit(EntityUid uid, BorgRebootComponent component, ComponentInit args)
    {
        component.HasPowerSource = _battery.HasCharge(uid, 0);
    }

    public void StartReboot(EntityUid uid, BorgRebootComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (component.IsRebooting)
            return;

        if (!component.HasPowerSource && !_battery.HasCharge(uid, 0))
        {
            KillBorg(uid);
            return;
        }

        component.IsRebooting = true;
        component.RebootTime = (float)_timing.CurTime.TotalSeconds + component.RebootDuration;
        Dirty(uid, component);
    }

    public void FinishReboot(EntityUid uid, BorgRebootComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!component.IsRebooting)
            return;

        component.IsRebooting = false;
        component.RebootTime = 0;

        var ev = new BorgRebootEvent(uid);
        RaiseLocalEvent(uid, ref ev);

        _status.TryRemoveStatusEffect(uid, "Stun");
        Dirty(uid, component);
    }

    public void CancelReboot(EntityUid uid, BorgRebootComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.IsRebooting = false;
        component.RebootTime = 0;
        Dirty(uid, component);
    }

    private void KillBorg(EntityUid uid)
    {
        _status.TryRemoveAllStatusEffects(uid);
    }
}
