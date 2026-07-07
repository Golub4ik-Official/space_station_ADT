// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core.Components;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Events;

[ByRefEvent]
public readonly record struct BorgLawChangedEvent(
    EntityUid Borg,
    string LawSet,
    bool SyncEnabled,
    EntityUid? ConnectedAi,
    bool ZerothLawOverride);
