using Robust.Shared.GameStates;

namespace Content.Shared.ADT.Medical.BodyBags;

/// <summary>
/// Замедляет разложение (гниение) содержимого контейнера.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BodyBagRotSlowComponent : Component
{
    /// <summary>
    /// Множитель скорости гниения. Значение &lt;1 замедляет процесс.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DecayMultiplier = 0.333f;
}
