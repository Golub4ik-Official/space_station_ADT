using Robust.Shared.GameStates;

namespace Content.Shared.ADT.Medical.BodyBags;

/// <summary>
/// Стазис-мешок: временно хранит тело, замедляя метаболизм,
/// тушит огонь и разрушается через некоторое время.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StasisBodyBagComponent : Component
{
    /// <summary>Сколько секунд мешок активен (закрыт с occupant'ом).</summary>
    [DataField, AutoNetworkedField]
    public float SecondsActive = 0f;

    /// <summary>Задержка (сек) перед применением стазиса.</summary>
    [DataField]
    public float StasisDelay = 5f;

    /// <summary>Задержка (сек) перед тушением огня у occupant'а.</summary>
    [DataField]
    public float FireExtinguishDelay = 3f;

    /// <summary>Урон (единиц/сек), получаемый мешком после активации стазиса.</summary>
    [DataField]
    public float DamagePerSecond = 0.5f;

    /// <summary>Максимальная «прочность» мешка до разрушения.</summary>
    [DataField]
    public float MaxIntegrity = 300f;

    /// <summary>Был ли потушен огонь у occupant'а.</summary>
    [DataField, AutoNetworkedField]
    public bool FireExtinguished = false;

    /// <summary>Активирован ли стазис.</summary>
    [DataField, AutoNetworkedField]
    public bool StasisApplied = false;

    /// <summary>Накопленный урон мешка.</summary>
    [DataField, AutoNetworkedField]
    public float DamageTaken = 0f;
}
