using Content.Shared.EntityTable;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.ADT.Medical.BodyBags;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LostCrewBodyBagComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool BodySpawned = false;

    [DataField]
    public bool SpawnBodyOnMapInit = false;

    [DataField]
    public EntProtoId CorpsePrototype = "SalvageHumanCorpse";

    [DataField]
    public ProtoId<EntityTablePrototype> LootTable = "LostCrewLootTable";
}
