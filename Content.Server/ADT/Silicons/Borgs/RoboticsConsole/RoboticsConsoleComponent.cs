using Robust.Shared.GameStates;

namespace Content.Server.ADT.Silicons.Borgs.RoboticsConsole;

[RegisterComponent]
public sealed partial class RoboticsConsoleComponent : Component
{
    [DataField]
    public float RefreshRate = 2f;

    [DataField]
    public double LastKillBotTime;
}
