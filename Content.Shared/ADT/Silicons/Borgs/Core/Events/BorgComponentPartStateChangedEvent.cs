using Content.Shared.ADT.Silicons.Borgs.Core.Components;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Events;

[ByRefEvent]
public readonly record struct BorgComponentPartBrokenEvent(
    EntityUid Borg,
    EntityUid PartEntity,
    BorgPartType PartType);
