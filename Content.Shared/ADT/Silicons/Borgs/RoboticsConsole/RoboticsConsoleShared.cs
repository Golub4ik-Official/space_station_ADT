using Robust.Shared.Serialization;

namespace Content.Shared.ADT.Silicons.Borgs.RoboticsConsole;

[Serializable, NetSerializable]
public enum RoboticsConsoleUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class RoboticsConsoleState : BoundUserInterfaceState
{
    public List<RoboticsConsoleBorgEntry> Borgs;

    public RoboticsConsoleState(List<RoboticsConsoleBorgEntry> borgs)
    {
        Borgs = borgs;
    }
}

[Serializable, NetSerializable]
public sealed class RoboticsConsoleBorgEntry
{
    public NetEntity NetEntity;
    public string Name;
    public string Status;
    public float BatteryPercent;
    public float HealthPercent;
    public string ModuleName;
    public string LawSet;
    public bool RadioEnabled;

    public RoboticsConsoleBorgEntry(
        NetEntity netEntity,
        string name,
        string status,
        float batteryPercent,
        float healthPercent,
        string moduleName,
        string lawSet,
        bool radioEnabled)
    {
        NetEntity = netEntity;
        Name = name;
        Status = status;
        BatteryPercent = batteryPercent;
        HealthPercent = healthPercent;
        ModuleName = moduleName;
        LawSet = lawSet;
        RadioEnabled = radioEnabled;
    }
}

[Serializable, NetSerializable]
public sealed class RoboticsConsoleLockToggleMessage : BoundUserInterfaceMessage
{
    public NetEntity BorgEntity;

    public RoboticsConsoleLockToggleMessage(NetEntity borgEntity)
    {
        BorgEntity = borgEntity;
    }
}

[Serializable, NetSerializable]
public sealed class RoboticsConsoleKillBotMessage : BoundUserInterfaceMessage
{
    public NetEntity BorgEntity;

    public RoboticsConsoleKillBotMessage(NetEntity borgEntity)
    {
        BorgEntity = borgEntity;
    }
}

[Serializable, NetSerializable]
public sealed class RoboticsConsoleHackBotMessage : BoundUserInterfaceMessage
{
    public NetEntity BorgEntity;

    public RoboticsConsoleHackBotMessage(NetEntity borgEntity)
    {
        BorgEntity = borgEntity;
    }
}

[Serializable, NetSerializable]
public sealed class RoboticsConsoleMassLockMessage : BoundUserInterfaceMessage
{
}
