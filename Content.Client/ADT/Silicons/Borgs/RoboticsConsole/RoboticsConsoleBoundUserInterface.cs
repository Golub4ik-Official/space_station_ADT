using Content.Shared.ADT.Silicons.Borgs.RoboticsConsole;

namespace Content.Client.ADT.Silicons.Borgs.RoboticsConsole;

public sealed class RoboticsConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private ADTRoboticsConsoleWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = new ADTRoboticsConsoleWindow();
        _window.OnLockToggle += OnLockToggle;
        _window.OnClose += Close;
        _window.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        _window?.Close();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is RoboticsConsoleState castState)
            _window?.UpdateState(castState);
    }

    private void OnLockToggle(NetEntity borgEntity)
    {
        SendMessage(new RoboticsConsoleLockToggleMessage(borgEntity));
    }
}
