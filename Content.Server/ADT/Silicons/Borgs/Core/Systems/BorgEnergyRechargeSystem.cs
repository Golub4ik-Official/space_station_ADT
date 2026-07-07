using Content.Shared.ADT.Silicons.Borgs.Core;
using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.PowerCell;
using Robust.Shared.Timing;

namespace Content.Server.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgEnergyRechargeSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly BorgBatterySystem _battery = default!;

    private float _accumulator;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _accumulator += frameTime;

        if (_accumulator < 1f)
            return;

        _accumulator -= 1f;

        var query = EntityQueryEnumerator<BorgComponent>();
        while (query.MoveNext(out var uid, out var borg))
        {
            TryRechargeStacks(uid, borg);
        }
    }

    private void TryRechargeStacks(EntityUid uid, BorgComponent borg)
    {
        if (!_battery.HasCharge(uid, 1))
            return;

        var anyDirty = false;
        for (int i = 0; i < borg.EnergyStorages.Count; i++)
        {
            var entry = borg.EnergyStorages[i];
            if (entry.CurrentAmount >= entry.MaxAmount)
                continue;

            if (!_battery.TryDrainPower(uid, 0.5f))
                break;

            entry.CurrentAmount = Math.Min(entry.CurrentAmount + entry.RechargeRate, entry.MaxAmount);
            borg.EnergyStorages[i] = entry;
            anyDirty = true;
        }

        if (anyDirty)
            Dirty(uid, borg);
    }
}
