// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class BorgComponentPartComponent : Component
{
    [DataField, AutoNetworkedField]
    public BorgPartType PartType = BorgPartType.Armour;

    [DataField, AutoNetworkedField]
    public float BruteDamage;

    [DataField, AutoNetworkedField]
    public float BurnDamage;

    [DataField, AutoNetworkedField]
    public float MaxDamage;

    [DataField, AutoNetworkedField]
    public bool Broken;

    [DataField, AutoNetworkedField]
    public bool Installed = true;

    [DataField, AutoNetworkedField]
    public EntityUid? OwnerBorg;
}

[Serializable, NetSerializable]
public enum BorgPartType : byte
{
    Armour,
    Actuator,
    Cell,
    Radio,
    BinaryCommunication,
    Camera,
    DiagnosisUnit
}
