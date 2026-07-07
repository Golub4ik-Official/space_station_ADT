// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BorgAiLinkComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? LinkedAi;

    [DataField, AutoNetworkedField]
    public bool LawSync;

    [DataField]
    public EntProtoId BackToAiAction = "ActionBorgReturnToAI";

    [DataField]
    public EntityUid? BackToAiActionEntity;

    [DataField, AutoNetworkedField]
    public bool ScrambledCodes;

    [DataField, AutoNetworkedField]
    public bool MalfHacked;

    [DataField, AutoNetworkedField]
    public bool HasCamera = true;
}
