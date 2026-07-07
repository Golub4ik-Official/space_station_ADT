// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core;
using Robust.Shared.GameStates;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class BorgEmagComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Emagged;

    [DataField, AutoNetworkedField]
    public EntityUid? MindslaveMaster;

    [DataField, AutoNetworkedField]
    public bool WeaponsUnlocked;
}
