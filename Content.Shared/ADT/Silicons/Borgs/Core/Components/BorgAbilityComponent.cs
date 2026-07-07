// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Robust.Shared.GameStates;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BorgAbilityComponent : Component
{
    [DataField, AutoNetworkedField]
    public string AbilityType = "Toggle";

    [DataField]
    public string Icon = string.Empty;

    [DataField]
    public float PowerCost;

    [DataField, AutoNetworkedField]
    public bool IsActive;
}
