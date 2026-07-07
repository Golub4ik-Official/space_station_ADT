using Robust.Shared.GameStates;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class BorgConstructionComponent : Component
{
    [DataField, AutoNetworkedField]
    public ConstructionStage Stage = ConstructionStage.None;

    [DataField, AutoNetworkedField]
    public string? ConfigName;

    [DataField, AutoNetworkedField]
    public bool LawSyncEnabled = true;

    [DataField, AutoNetworkedField]
    public bool AiSyncEnabled = true;

    [DataField, AutoNetworkedField]
    public bool LocomotionEnabled = true;

    [DataField, AutoNetworkedField]
    public bool PanelLocked;

    [DataField, AutoNetworkedField]
    public int SteelDeposited;

    [DataField, AutoNetworkedField]
    public bool CableInserted;

    [DataField, AutoNetworkedField]
    public int FlashesInserted;
}

public enum ConstructionStage : byte
{
    None,
    Frame,
    LimbsInstalled,
    ChestInstalled,
    HeadInstalled,
    Configuration,
    MMIInserted,
    Complete
}
