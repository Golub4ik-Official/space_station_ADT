// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Robust.Shared.Serialization;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Events;

[ByRefEvent]
public readonly record struct BorgWireActionEvent(
    EntityUid Borg,
    BorgWireType WireType,
    BorgWireAction Action);

[Serializable, NetSerializable]
public enum BorgWiresUiKey : byte
{
    Wires
}
