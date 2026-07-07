// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Robust.Shared.GameStates;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BorgSensorComponent : Component
{
    [DataField, AutoNetworkedField]
    public byte CurrentMode;

    [DataField]
    public int MaxMode = 3;
}

public enum BorgSensorMode : byte
{
    Off,
    Meson,
    Thermal
}
