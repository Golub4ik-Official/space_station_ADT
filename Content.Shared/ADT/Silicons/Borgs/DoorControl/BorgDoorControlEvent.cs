// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Robust.Shared.Serialization;

namespace Content.Shared.ADT.Silicons.Borgs.DoorControl;

/// <summary>
/// Action that a borg (or AI) wants to perform on a door via keybind,
/// bypassing the radial menu. Sent from client to server as a network event.
/// </summary>
[Serializable, NetSerializable]
public sealed class BorgDoorControlEvent : EntityEventArgs
{
    /// <summary>
    /// Networked identifier of the targeted door / shutter / blast door.
    /// </summary>
    public NetEntity Target;

    /// <summary>
    /// Which door action the player requested.
    /// </summary>
    public BorgDoorAction Action;
}

/// <summary>
/// Discrete door actions available to borgs through keybinds.
/// "Toggle" semantics keep the interface simple: the server reads the
/// current door state and flips it, so the client does not need to know
/// the exact state under the cursor.
/// </summary>
public enum BorgDoorAction : byte
{
    /// <summary>Flip the bolt state of an airlock/windoor.</summary>
    ToggleBolt,

    /// <summary>Flip the electrified state of an airlock/windoor.</summary>
    ToggleElectrified,

    /// <summary>Flip emergency-access on an airlock.</summary>
    ToggleEmergencyAccess,

    /// <summary>Open or close any door (airlock, windoor, shutter, blast door).</summary>
    ToggleDoor,
}
