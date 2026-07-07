// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class BorgSlotComponent : Component
{
    public const int MaxSlots = 4;

    [DataField, AutoNetworkedField]
    public EntityUid Slot0 = EntityUid.Invalid;

    [DataField, AutoNetworkedField]
    public EntityUid Slot1 = EntityUid.Invalid;

    [DataField, AutoNetworkedField]
    public EntityUid Slot2 = EntityUid.Invalid;

    [DataField, AutoNetworkedField]
    public EntityUid Slot3 = EntityUid.Invalid;

    [DataField, AutoNetworkedField]
    public int CurrentSlot;

    [DataField, AutoNetworkedField]
    public bool CycleLocked;

    [DataField]
    public HashSet<EntProtoId> ModuleWhitelist = new();

    [DataField]
    public int MaxModules = 1;

    public EntityUid this[int index]
    {
        get => index switch
        {
            0 => Slot0,
            1 => Slot1,
            2 => Slot2,
            3 => Slot3,
            _ => EntityUid.Invalid
        };
        set
        {
            switch (index)
            {
                case 0: Slot0 = value; break;
                case 1: Slot1 = value; break;
                case 2: Slot2 = value; break;
                case 3: Slot3 = value; break;
            }
        }
    }

    public int Count => MaxSlots;
}
