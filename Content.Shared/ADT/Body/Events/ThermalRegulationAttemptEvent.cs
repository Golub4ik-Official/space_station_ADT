namespace Content.Shared.ADT.Body.Events;

[ByRefEvent]
public record struct ThermalRegulationAttemptEvent(EntityUid Uid)
{
    public bool Cancelled;
}
