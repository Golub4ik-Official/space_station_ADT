// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Robust.Shared.GameStates;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class BorgRebootComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool IsRebooting;

    [DataField]
    public float RebootTime;

    [DataField]
    public float RebootDuration = 15f;

    [DataField]
    public bool HasPowerSource;
}
