namespace Content.Shared.ADT.PDA.Events;

/// <summary>
/// Raised directed on an entity when a player Ctrl+Left clicks it.
/// If Handled is set to true, the default pull behavior is skipped.
/// </summary>
[ByRefEvent]
public record struct TryPullObjectEvent(EntityUid User, EntityUid Target, bool Handled = false);
