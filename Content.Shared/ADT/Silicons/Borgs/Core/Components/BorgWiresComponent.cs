// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BorgWiresComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<BorgWireState> Wires = new(4);

    [DataField, AutoNetworkedField]
    public bool PanelOpen;
}

[Serializable, NetSerializable]
public enum BorgWireType : byte
{
    AiControl,
    Camera,
    LawCheck,
    Locked
}

[Serializable, NetSerializable]
public enum BorgWireAction : byte
{
    Cut,
    Mend,
    Pulse
}

[DataDefinition]
[Serializable, NetSerializable]
public partial struct BorgWireState
{
    [DataField]
    public BorgWireType Type;

    [DataField]
    public bool IsCut;

    [DataField]
    public bool IsMended = true;

    [DataField]
    public bool IsPulsed;

    [DataField]
    public string Color = string.Empty;

    public BorgWireState()
    {
    }
}
