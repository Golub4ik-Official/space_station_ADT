// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core.Components;

namespace Content.Server.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgSelfRepairSystem : EntitySystem
{
    [Dependency] private readonly BorgBatterySystem _battery = default!;
    [Dependency] private readonly BorgComponentPartSystem _parts = default!;

    private readonly HashSet<EntityUid> _repairingBorgs = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgSelfRepairComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<BorgSelfRepairComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnInit(EntityUid uid, BorgSelfRepairComponent component, ComponentInit args)
    {
        if (component.Enabled)
            _repairingBorgs.Add(uid);
    }

    private void OnShutdown(EntityUid uid, BorgSelfRepairComponent component, ComponentShutdown args)
    {
        _repairingBorgs.Remove(uid);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var uid in _repairingBorgs)
        {
            if (!TryComp<BorgSelfRepairComponent>(uid, out var repair) || !repair.Enabled)
                continue;

            repair.Accumulator += frameTime;
            if (repair.Accumulator < repair.TickInterval)
                continue;

            repair.Accumulator -= repair.TickInterval;

            if (!_battery.TryDrainPower(uid, 5))
                continue;

            var partQuery = EntityQueryEnumerator<BorgComponentPartComponent>();
            while (partQuery.MoveNext(out var partUid, out var part))
            {
                if (!part.Installed || part.Broken)
                    continue;

                var amount = repair.RepairAmountPerSecond * repair.TickInterval;
                _parts.RepairBrute(partUid, amount);
                _parts.RepairBurn(partUid, amount);
            }

            Dirty(uid, repair);
        }
    }
}
