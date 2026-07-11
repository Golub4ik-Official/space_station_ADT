using Robust.Shared.GameStates;

namespace Content.Shared.ADT.Medical.BodyBags;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BodyBagGasComponent : Component
{
    [DataField, AutoNetworkedField]
    public BodyBagGasFillMode FillMode = BodyBagGasFillMode.None;

    public TimeSpan? N2OFillTime;

    public float TimeUntilNextRegen;
}

public enum BodyBagGasFillMode : byte
{
    None = 0,
    N2O,
    Breathable,
    MedicalHeal,
    SedativeSleep,
}
