// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Client.Gameplay;
using Content.Client.Viewport;
using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.DoorControl;
using Content.Shared.Input;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Input.Binding;

namespace Content.Client.ADT.Silicons.Borgs.DoorControl;

/// <summary>
/// Client side of the borg door-control keybinds. Binds four hotkeys
/// (bolt / electrify / emergency access / toggle door) that act on whatever
/// door the player is currently hovering with the mouse cursor.
/// </summary>
public sealed class BorgDoorControlSystem : EntitySystem
{
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

    public override void Initialize()
    {
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.ADTBorgBoltDoor,
                InputCmdHandler.FromDelegate(session => HandleAction(BorgDoorAction.ToggleBolt)))
            .Bind(ContentKeyFunctions.ADTBorgElectrifyDoor,
                InputCmdHandler.FromDelegate(session => HandleAction(BorgDoorAction.ToggleElectrified)))
            .Bind(ContentKeyFunctions.ADTBorgEmergencyDoor,
                InputCmdHandler.FromDelegate(session => HandleAction(BorgDoorAction.ToggleEmergencyAccess)))
            .Bind(ContentKeyFunctions.ADTBorgToggleDoor,
                InputCmdHandler.FromDelegate(session => HandleAction(BorgDoorAction.ToggleDoor)))
            .Register<BorgDoorControlSystem>();
    }

    public override void Shutdown()
    {
        CommandBinds.Unregister<BorgDoorControlSystem>();
        base.Shutdown();
    }

    private void HandleAction(BorgDoorAction action)
    {
        var user = _player.LocalEntity;
        if (user == null)
            return;

        // Only borgs (ADT-rework or vanilla chassis) get these hotkeys.
        if (!HasComp<BorgComponent>(user.Value) && !HasComp<BorgChassisComponent>(user.Value))
            return;

        var target = GetEntityUnderMouse();
        if (target == null || !target.Value.IsValid())
            return;

        RaiseNetworkEvent(new BorgDoorControlEvent
        {
            Target = GetNetEntity(target.Value),
            Action = action,
        });
    }

    /// <summary>
    /// Resolves the entity currently under the mouse cursor by reusing the
    /// same viewport hit-test the game uses for the interaction outline.
    /// </summary>
    private EntityUid? GetEntityUnderMouse()
    {
        if (_stateManager.CurrentState is not GameplayStateBase screen)
            return null;

        if (!_inputManager.MouseScreenPosition.IsValid)
            return null;

        if (_uiManager.CurrentlyHovered is not IViewportControl vp)
            return null;

        var mousePosWorld = vp.PixelToMap(_inputManager.MouseScreenPosition.Position);
        return screen.GetClickedEntity(mousePosWorld);
    }
}
