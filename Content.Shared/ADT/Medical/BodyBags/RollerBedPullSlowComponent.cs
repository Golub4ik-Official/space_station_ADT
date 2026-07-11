using Robust.Shared.GameStates;

namespace Content.Shared.ADT.Medical.BodyBags;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RollerBedPullSlowComponent : Component
{
    [DataField, AutoNetworkedField]
    public float WalkSpeedModifier = 0.7f;

    [DataField, AutoNetworkedField]
    public float SprintSpeedModifier = 0.8f;
}
