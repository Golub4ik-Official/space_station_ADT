// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BorgGripperComponent : Component
{
    [DataField, AutoNetworkedField]
    public string GripperType = "Universal";

    [DataField]
    public ProtoId<TagPrototype>? WhitelistTag;

    [DataField, AutoNetworkedField]
    public EntityUid? HeldEntity;

    [DataField]
    public EntProtoId? HeldPrototype;
}
