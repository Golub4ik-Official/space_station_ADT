using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.ADT.Medical.BodyBags;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BodyBagCinchComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Cinched = false;

    [DataField]
    public float BreakoutTime = 240f;

    [DataField]
    public float CinchTime = 5f;
}

[Serializable, NetSerializable]
public enum BodyBagCinchVisuals : byte
{
    Cinched,
}
