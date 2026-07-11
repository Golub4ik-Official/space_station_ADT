using Robust.Shared.GameStates;

namespace Content.Shared.ADT.Medical.BodyBags;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BodyBagRotSlowComponent : Component
{
    [DataField, AutoNetworkedField]
    public float DecayMultiplier = 0.333f;
}
