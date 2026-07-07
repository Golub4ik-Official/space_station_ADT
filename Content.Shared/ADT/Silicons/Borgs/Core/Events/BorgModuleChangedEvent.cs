// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core.Components;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Events;

[ByRefEvent]
public readonly record struct BorgModuleChangedEvent(
    EntityUid Borg,
    EntityUid Module,
    ADTBorgModuleComponent ModuleComponent,
    int SlotIndex,
    bool Installed);
