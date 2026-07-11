using Robust.Shared.GameStates;

namespace Content.Shared.ADT.Medical.BodyBags;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StasisBodyBagComponent : Component
{
    [DataField, AutoNetworkedField]
    public float SecondsActive = 0f;

    [DataField]
    public float StasisDelay = 5f;

    [DataField]
    public float FireExtinguishDelay = 3f;

    [DataField]
    public float DamagePerSecond = 0.5f;

    [DataField]
    public float MaxIntegrity = 300f;

    [DataField, AutoNetworkedField]
    public bool FireExtinguished = false;

    [DataField, AutoNetworkedField]
    public bool StasisApplied = false;

    [DataField, AutoNetworkedField]
    public float DamageTaken = 0f;
}
