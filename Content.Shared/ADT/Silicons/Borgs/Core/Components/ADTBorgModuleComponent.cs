// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Components;

[RegisterComponent]
public sealed partial class ADTBorgModuleComponent : Component
{
    [DataField]
    public string ModuleType = string.Empty;

    [DataField]
    public List<EntProtoId> Items = new();

    [DataField]
    public List<EntProtoId> EmagItems = new();

    [DataField]
    public List<EntProtoId> OverrideItems = new();

    [DataField]
    public List<BorgEnergyStorageEntry> EnergyStorages = new();

    [DataField]
    public List<string> RadioChannels = new();

    [DataField]
    public List<EntProtoId> InnateActions = new();

    [DataField]
    public BorgArmorEntry Armor;

    [DataField]
    public List<string> Languages = new();
}
