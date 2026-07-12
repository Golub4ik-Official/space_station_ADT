using Robust.Shared.GameStates;

namespace Content.Shared.ADT.Medical.BodyBags;

/// <summary>
/// Маркер элитного body bag: даёт содержимому иммунитет к огню
/// через <see cref="BodyBagFireProtectionSystem"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BodyBagEliteComponent : Component
{
}
