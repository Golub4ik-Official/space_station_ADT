// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.ADT.Silicons.Borgs.Core;

[Serializable, NetSerializable]
[Prototype("borgModuleType")]
public sealed partial class BorgModuleTypePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name = string.Empty;

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
