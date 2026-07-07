// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.Silicons.Laws;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class BorgLawComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<SiliconLawsetPrototype> LawSet = "Crewsimov";

    [DataField, AutoNetworkedField]
    public bool LawSyncEnabled = true;

    [DataField, AutoNetworkedField]
    public EntityUid? ConnectedAi;

    [DataField]
    public string? ZerothLaw;

    [DataField, AutoNetworkedField]
    public bool ZerothLawOverride;

    [DataField]
    public string? ZerothLawBorg;

    [DataField, AutoNetworkedField]
    public bool SyndicateImmune;
}
