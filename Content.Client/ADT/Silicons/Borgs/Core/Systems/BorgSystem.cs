// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.Core.Systems;

namespace Content.Client.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgSystem : SharedBorgSystem
{
    public float GetChargePercent(EntityUid uid)
    {
        if (!TryComp<BorgBatteryComponent>(uid, out var battery))
            return 0;

        if (battery.MaxCharge <= 0)
            return 0;

        return battery.Charge / battery.MaxCharge;
    }

    public string GetChargeString(EntityUid uid)
    {
        if (!TryComp<BorgBatteryComponent>(uid, out var battery))
            return "N/A";

        return $"{battery.Charge:F0}/{battery.MaxCharge:F0}";
    }

    public string GetModuleInfo(EntityUid uid)
    {
        if (!TryComp<BorgSlotComponent>(uid, out var slots))
            return "No modules";

        var active = slots.CurrentSlot + 1;
        var total = BorgSlotComponent.MaxSlots;
        return $"Slot {active}/{total}";
    }

    public string GetLawSummary(EntityUid uid)
    {
        if (!TryComp<BorgLawComponent>(uid, out var law))
            return "No laws";

        return law.LawSet;
    }
}
