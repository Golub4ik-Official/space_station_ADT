using Robust.Shared.GameStates;

namespace Content.Shared.ADT.Medical.BodyBags;

/// <summary>
/// Позволяет body bag заполняться газом при закрытии.
/// Режим заполнения задаётся через <see cref="BodyBagGasFillMode"/>.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BodyBagGasComponent : Component
{
    [DataField, AutoNetworkedField]
    public BodyBagGasFillMode FillMode = BodyBagGasFillMode.None;

    public TimeSpan? N2OFillTime;

    public float TimeUntilNextRegen;
}

/// <summary>
/// Режимы заполнения body bag газом.
/// </summary>
public enum BodyBagGasFillMode : byte
{
    /// <summary>Нет заполнения.</summary>
    None = 0,
    /// <summary>Усыпляющий газ (N₂O + O₂).</summary>
    N2O,
    /// <summary>Дыхательная смесь (O₂ + N₂).</summary>
    Breathable,
    /// <summary>Лечебный газ (лечение + дыхательная смесь).</summary>
    MedicalHeal,
    /// <summary>Усыпляющий газ + снотворное.</summary>
    SedativeSleep,
}
