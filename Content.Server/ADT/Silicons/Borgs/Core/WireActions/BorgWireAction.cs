using Content.Server.ADT.Silicons.Borgs.Core.Systems;
using Content.Server.Wires;
using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.Core.Events;
using Content.Shared.Wires;
using WireAction = Content.Shared.ADT.Silicons.Borgs.Core.Components.BorgWireAction;

namespace Content.Server.ADT.Silicons.Borgs.Core.WireActions;

[DataDefinition]
public sealed partial class BorgWireAction : IWireAction
{
    [DataField]
    public BorgWireType WireType = BorgWireType.AiControl;

    [DataField]
    public string Name = string.Empty;

    [DataField]
    public Color Color = Color.White;

    public object? StatusKey => null;

    private WiresSystem _wires = default!;

    public void Initialize()
    {
        _wires = IoCManager.Resolve<IEntityManager>().EntitySysManager.GetEntitySystem<WiresSystem>();
    }

    public bool AddWire(Wire wire, int count)
    {
        return true;
    }

    public bool Cut(EntityUid user, Wire wire)
    {
        var wiresSystem = IoCManager.Resolve<IEntityManager>().EntitySysManager.GetEntitySystem<BorgWiresSystem>();
        wiresSystem.ProcessWireAction(wire.Owner, WireType, WireAction.Cut);
        return true;
    }

    public bool Mend(EntityUid user, Wire wire)
    {
        var wiresSystem = IoCManager.Resolve<IEntityManager>().EntitySysManager.GetEntitySystem<BorgWiresSystem>();
        wiresSystem.ProcessWireAction(wire.Owner, WireType, WireAction.Mend);
        return true;
    }

    public void Pulse(EntityUid user, Wire wire)
    {
        var wiresSystem = IoCManager.Resolve<IEntityManager>().EntitySysManager.GetEntitySystem<BorgWiresSystem>();
        wiresSystem.ProcessWireAction(wire.Owner, WireType, WireAction.Pulse);
    }

    public void Update(Wire wire)
    {
    }

    public StatusLightData? GetStatusLightData(Wire wire)
    {
        return new StatusLightData(Color, StatusLightState.On, Loc.GetString(Name));
    }
}
