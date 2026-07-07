// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Robust.Shared.Serialization;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Events;

[ByRefEvent]
public readonly record struct BorgSlotChangedEvent(EntityUid Borg, int OldSlot, int NewSlot);

[Serializable, NetSerializable]
public enum BorgUiKey : byte
{
    SlotSelector,
    ModuleConfig,
    SelfDiagnosis,
    LawDisplay
}
