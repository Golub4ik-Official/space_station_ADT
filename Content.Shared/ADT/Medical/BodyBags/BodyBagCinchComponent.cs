using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.ADT.Medical.BodyBags;

/// <summary>
/// Позволяет затягивать мешок (cinch), блокируя открытие изнутри
/// до выламывания через <see cref="BreakoutTime"/>.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BodyBagCinchComponent : Component
{
    /// <summary>Затянут ли мешок в данный момент.</summary>
    [DataField, AutoNetworkedField]
    public bool Cinched = false;

    /// <summary>Время (сек) на выламывание изнутри.</summary>
    [DataField]
    public float BreakoutTime = 240f;

    /// <summary>Время (сек) на затягивание/расстёгивание.</summary>
    [DataField]
    public float CinchTime = 5f;
}

[Serializable, NetSerializable]
public enum BodyBagCinchVisuals : byte
{
    Cinched,
}
