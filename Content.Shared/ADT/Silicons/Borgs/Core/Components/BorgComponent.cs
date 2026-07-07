// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║                                                      ║
// ║   __        ___ _  __ _   _   _    ___ ___ ___       ║
// ║   \ \      / / | |/ /| | | |  / \ | _ \_ _| _ \     ║
// ║    \ \ /\ / /| | ' < | |_| | / _ \|  _/| ||  _/     ║
// ║     \ V  V / |_|_|\_\ \___/ /_/ \_\_| |___|_|       ║
// ║                                                      ║
// ║  Author: WikiHampter                                 ║
// ║  Licensed under MIT                                  ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class BorgComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId? ActiveModule;

    [DataField, AutoNetworkedField]
    public int ActiveSlotIndex;

    [DataField, AutoNetworkedField]
    public bool IsActive;

    [DataField, AutoNetworkedField]
    public bool Locked;

    /// <summary>
    /// ADT-Tweak: whether the maintenance panel cover is locked.
    /// Matches SS13 <c>locked</c> — requires an ID card with Research access
    /// (or emag) to toggle. When locked the cover cannot be pried open.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool PanelLocked = true;

    [DataField, AutoNetworkedField]
    public bool MaintenancePanelOpen;

    [DataField]
    public EntityUid? BrainEntity;

    [DataField, AutoNetworkedField]
    public List<BorgEnergyStorageEntry> EnergyStorages = new();
}
