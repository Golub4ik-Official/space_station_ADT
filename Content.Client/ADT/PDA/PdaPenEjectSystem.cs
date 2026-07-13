using Content.Shared.ADT.PDA.Events;
using Content.Shared.PDA;

namespace Content.Client.ADT.PDA;

/// <summary>
/// Prevents predictive default Ctrl-interaction with PDAs carried by the user.
/// </summary>
public sealed class PdaPenEjectSystem : EntitySystem
{
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
