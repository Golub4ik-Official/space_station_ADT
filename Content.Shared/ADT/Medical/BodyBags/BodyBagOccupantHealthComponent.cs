using Robust.Shared.GameStates;

namespace Content.Shared.ADT.Medical.BodyBags;

/// <summary>
/// Маркер для body bag, позволяющий осматривать здоровье occupant'а
/// через <see cref="BodyBagOccupantHealthSystem"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BodyBagOccupantHealthComponent : Component
{
}
