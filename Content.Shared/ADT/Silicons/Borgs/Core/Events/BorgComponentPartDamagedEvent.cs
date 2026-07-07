// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core.Components;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Events;

[ByRefEvent]
public readonly record struct BorgComponentPartDamagedEvent(
    EntityUid Borg,
    EntityUid PartEntity,
    float BruteDamage,
    float BurnDamage);

[ByRefEvent]
public readonly record struct BorgComponentPartRepairedEvent(
    EntityUid Borg,
    EntityUid PartEntity,
    BorgPartType PartType,
    float BruteRepaired,
    float BurnRepaired);
