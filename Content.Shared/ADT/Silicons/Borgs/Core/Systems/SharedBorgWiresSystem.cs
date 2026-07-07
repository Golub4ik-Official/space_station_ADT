// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using System.Linq;
using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.Core.Events;
using Robust.Shared.Random;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Systems;

public abstract class SharedBorgWiresSystem : EntitySystem
{
    private static readonly string[] WireColors = { "red", "blue", "green", "yellow" };

    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgWiresComponent, ComponentInit>(OnWiresInit);
        SubscribeLocalEvent<BorgWiresComponent, BorgWireActionEvent>(OnWireAction);
    }

    private void OnWiresInit(EntityUid uid, BorgWiresComponent component, ComponentInit args)
    {
        if (component.Wires.Count == 0)
        {
            var types = Enum.GetValues<BorgWireType>();
            var colors = WireColors.OrderBy(_ => _random.Next()).ToList();

            for (int i = 0; i < types.Length && i < colors.Count; i++)
            {
                component.Wires.Add(new BorgWireState
                {
                    Type = types[i],
                    IsCut = false,
                    IsMended = true,
                    IsPulsed = false,
                    Color = colors[i]
                });
            }
        }
    }

    private void OnWireAction(EntityUid uid, BorgWiresComponent component, BorgWireActionEvent args)
    {
        var wireIndex = component.Wires.FindIndex(w => w.Type == args.WireType);
        if (wireIndex == -1)
            return;

        var wire = component.Wires[wireIndex];

        switch (args.Action)
        {
            case BorgWireAction.Cut:
                wire.IsCut = true;
                wire.IsMended = false;
                break;
            case BorgWireAction.Mend:
                wire.IsCut = false;
                wire.IsMended = true;
                break;
            case BorgWireAction.Pulse:
                wire.IsPulsed = true;
                break;
        }

        component.Wires[wireIndex] = wire;
        Dirty(uid, component);
    }

    public bool IsWireCut(EntityUid uid, BorgWireType type, BorgWiresComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        var wire = component.Wires.Find(w => w.Type == type);
        return wire.IsCut;
    }

    public void SetPanelOpen(EntityUid uid, bool open, BorgWiresComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.PanelOpen = open;
        Dirty(uid, component);
    }
}
