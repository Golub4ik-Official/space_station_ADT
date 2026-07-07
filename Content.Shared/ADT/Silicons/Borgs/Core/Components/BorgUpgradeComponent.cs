// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core;
using Robust.Shared.GameStates;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BorgUpgradeComponent : Component
{
    [DataField, AutoNetworkedField]
    public string UpgradeType = string.Empty;

    [DataField]
    public string? ModuleRequired;

    [DataField]
    public bool Installed;
}
