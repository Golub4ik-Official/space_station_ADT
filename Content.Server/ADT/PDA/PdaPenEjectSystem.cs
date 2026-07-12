using Content.Shared.ADT.PDA.Events;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.PDA;
using Content.Shared.Popups;

namespace Content.Server.ADT.PDA;

public sealed class PdaPenEjectSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PdaComponent, TryPullObjectEvent>(OnTryPullObject);
    }

    private void OnTryPullObject(Entity<PdaComponent> ent, ref TryPullObjectEvent args)
    {
        if (args.Handled)
            return;

        if (!IsPdaOnPlayer(ent, args.User))
            return;

        if (ent.Comp.PenSlot.HasItem)
        {
            _itemSlots.TryEjectToHands(ent, ent.Comp.PenSlot, args.User);
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("pda-pen-eject-no-pen"), ent, args.User);
        }

        args.Handled = true;
    }

    private bool IsPdaOnPlayer(Entity<PdaComponent> ent, EntityUid user)
    {
        var parent = Transform(ent.Owner).ParentUid;
        var depth = 0;

        while (parent.IsValid() && depth < 10)
        {
            if (parent == user)
                return true;

            if (!TryComp<TransformComponent>(parent, out var parentXform))
                break;

            parent = parentXform.ParentUid;
            depth++;
        }

        return false;
    }
}
