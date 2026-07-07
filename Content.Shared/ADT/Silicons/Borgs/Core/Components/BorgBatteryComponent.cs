// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class BorgBatteryComponent : Component
{
    [DataField]
    public string CellContainerId = "cell_slot";

    [DataField, AutoNetworkedField]
    public EntityUid? CellUid;

    [DataField, AutoNetworkedField]
    public float Charge;

    [DataField, AutoNetworkedField]
    public float MaxCharge;

    [DataField, AutoNetworkedField]
    public bool IsCharging;

    [DataField]
    public float LowPowerThreshold = 0.1f;

    [DataField]
    public float DrainPerSecond = 10f;
}
