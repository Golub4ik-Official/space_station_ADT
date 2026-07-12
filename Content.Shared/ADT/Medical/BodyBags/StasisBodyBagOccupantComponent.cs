using Robust.Shared.GameStates;

namespace Content.Shared.ADT.Medical.BodyBags;

/// <summary>
/// Маркер, добавляемый occupant'у стазис-мешка.
/// Позволяет <see cref="StasisBodyBagSystem"/> применять
/// влияние на метаболизм и терморегуляцию.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StasisBodyBagOccupantComponent : Component
{
}
