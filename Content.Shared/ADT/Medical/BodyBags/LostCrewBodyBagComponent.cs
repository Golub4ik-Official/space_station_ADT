using Robust.Shared.GameStates;

namespace Content.Shared.ADT.Medical.BodyBags;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LostCrewBodyBagComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool BodySpawned = false;

    [DataField]
    public bool SpawnBodyOnMapInit = false;
}
