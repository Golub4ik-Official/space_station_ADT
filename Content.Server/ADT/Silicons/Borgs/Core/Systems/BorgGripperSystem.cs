using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Tag;
using Content.Shared.Throwing;
using Content.Shared.Verbs;

namespace Content.Server.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgGripperSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly ThrownItemSystem _thrown = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgGripperComponent, UserActivateInWorldEvent>(OnBorgActivate);
        SubscribeLocalEvent<BorgGripperComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
    }

    private void OnBorgActivate(EntityUid uid, BorgGripperComponent component, UserActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;

        if (!Exists(target))
            return;

        var held = component.HeldEntity;
        if (held != null && Exists(held.Value))
        {
            _transform.DropNextTo(held.Value, uid);
            component.HeldEntity = null;
            component.HeldPrototype = null;
            Dirty(uid, component);
        }

        if (!CanGrip(uid, target, component))
            return;

        if (_hands.TryPickup(uid, target))
        {
            component.HeldEntity = target;
            component.HeldPrototype = MetaData(target).EntityPrototype?.ID;
            Dirty(uid, component);
            args.Handled = true;
        }
    }

    private void OnGetVerbs(EntityUid uid, BorgGripperComponent component, GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (component.HeldEntity == null || !Exists(component.HeldEntity.Value))
            return;

        args.Verbs.Add(new Verb
        {
            Text = Loc.GetString("borg-verb-drop-gripper-item"),
            Act = () =>
            {
                var held = component.HeldEntity;
                if (held != null && Exists(held.Value))
                {
                    _transform.DropNextTo(held.Value, uid);
                    component.HeldEntity = null;
                    component.HeldPrototype = null;
                    Dirty(uid, component);
                }
            },
            Priority = 4
        });
    }

    private bool CanGrip(EntityUid uid, EntityUid target, BorgGripperComponent component)
    {
        if (component.WhitelistTag == null)
            return component.GripperType == "Universal";

        return _tags.HasTag(target, component.WhitelistTag.Value);
    }
}
