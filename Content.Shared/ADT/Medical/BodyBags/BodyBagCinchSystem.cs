using Content.Shared.Containers;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Popups;
using Content.Shared.Storage.Components;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared.ADT.Medical.BodyBags;

[Serializable, NetSerializable]
public sealed partial class BodyBagCinchDoAfterEvent : SimpleDoAfterEvent
{
    public bool TargetCinched;
}

public sealed class BodyBagCinchSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyBagCinchComponent, StorageOpenAttemptEvent>(OnOpenAttempt);
        SubscribeLocalEvent<BodyBagCinchComponent, GetVerbsEvent<ActivationVerb>>(OnGetVerb);
        SubscribeLocalEvent<BodyBagCinchComponent, BodyBagCinchDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<BodyBagCinchComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<BodyBagCinchComponent, ContainerRelayMovementEntityEvent>(OnRelayMovement);
        SubscribeLocalEvent<BodyBagCinchComponent, StorageAfterOpenEvent>(OnStorageAfterOpen);
    }

    private void OnOpenAttempt(Entity<BodyBagCinchComponent> ent, ref StorageOpenAttemptEvent args)
    {
        if (ent.Comp.Cinched)
        {
            _popup.PopupClient(Loc.GetString("body-bag-cinch-open-prevent"), ent, args.User);
            args.Cancelled = true;
        }
    }

    private void OnGetVerb(Entity<BodyBagCinchComponent> ent, ref GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (TryComp<EntityStorageComponent>(ent, out var storage) && storage.Open)
            return;

        var isInside = HasComp<InsideEntityStorageComponent>(args.User);

        if (isInside && ent.Comp.Cinched)
        {
            var breakoutUser = args.User;
            args.Verbs.Add(new ActivationVerb
            {
                Text = Loc.GetString("body-bag-cinch-verb-breakout"),
                Act = () => TryStartBreakout(ent, breakoutUser),
            });
            return;
        }

        if (isInside)
            return;

        var verbName = ent.Comp.Cinched ? "body-bag-cinch-verb-uncinch" : "body-bag-cinch-verb-cinch";
        var user = args.User;

        args.Verbs.Add(new ActivationVerb
        {
            Text = Loc.GetString(verbName),
            Act = () => TryStartCinch(ent, user),
        });
    }

    private void TryStartBreakout(Entity<BodyBagCinchComponent> ent, EntityUid user)
    {
        _popup.PopupClient(Loc.GetString("body-bag-cinch-breakout-start"), ent, user);

        var doAfterArgs = new DoAfterArgs(EntityManager, user, ent.Comp.BreakoutTime, new BodyBagCinchDoAfterEvent(), ent, user)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            RequireCanInteract = false,
        };

        var ev = doAfterArgs.Event as BodyBagCinchDoAfterEvent;
        if (ev != null)
            ev.TargetCinched = false;

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void TryStartCinch(Entity<BodyBagCinchComponent> ent, EntityUid user)
    {
        var doAfterArgs = new DoAfterArgs(EntityManager, user, ent.Comp.CinchTime, new BodyBagCinchDoAfterEvent(), ent, user)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true,
        };

        var ev = doAfterArgs.Event as BodyBagCinchDoAfterEvent;
        if (ev != null)
            ev.TargetCinched = true;

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnDoAfter(Entity<BodyBagCinchComponent> ent, ref BodyBagCinchDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (TryComp<EntityStorageComponent>(ent, out var storage) && storage.Open)
            return;

        SetCinch(ent, args.TargetCinched, args.User);
        args.Handled = true;
    }

    private void OnExamined(Entity<BodyBagCinchComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.Cinched)
            args.PushMarkup(Loc.GetString("body-bag-cinch-examine-cinched"));
    }

    private void OnStorageAfterOpen(Entity<BodyBagCinchComponent> ent, ref StorageAfterOpenEvent args)
    {
        if (!TryComp<EntityStorageComponent>(ent, out var storage))
            return;

        foreach (var contained in storage.Contents.ContainedEntities)
        {
            if (TryComp<DoAfterComponent>(contained, out var doAfterComp))
            {
                foreach (var (id, doAfter) in doAfterComp.DoAfters)
                {
                    if (doAfter.Args.Event is BodyBagCinchDoAfterEvent)
                        _doAfter.Cancel(contained, id, doAfterComp);
                }
            }
        }
    }

    public void SetCinch(Entity<BodyBagCinchComponent> ent, bool cinched, EntityUid? user = null)
    {
        if (ent.Comp.Cinched == cinched)
            return;

        ent.Comp.Cinched = cinched;
        Dirty(ent);

        _appearance.SetData(ent, BodyBagCinchVisuals.Cinched, ent.Comp.Cinched);

        if (user != null)
        {
            var msg = cinched
                ? Loc.GetString("body-bag-cinch-success")
                : Loc.GetString("body-bag-cinch-uncinch-success");
            _popup.PopupClient(msg, ent, user.Value);
        }

        if (cinched)
        {
            _audio.PlayPredicted(new SoundPathSpecifier("/Audio/Items/belt_equip.ogg"), ent, user);
        }
    }

    private void OnRelayMovement(Entity<BodyBagCinchComponent> ent, ref ContainerRelayMovementEntityEvent args)
    {
        if (!ent.Comp.Cinched)
            return;

        if (!TryComp<EntityStorageComponent>(ent, out var storage) || storage.Open)
            return;

        var occupant = args.Entity;
        if (!HasComp<MobStateComponent>(occupant))
            return;

        if (HasComp<ActiveDoAfterComponent>(occupant))
            return;

        _popup.PopupClient(Loc.GetString("body-bag-cinch-breakout-start"), ent, occupant);

        var doAfterArgs = new DoAfterArgs(EntityManager, occupant, ent.Comp.BreakoutTime, new BodyBagCinchDoAfterEvent(), ent, occupant)
        {
            BreakOnDamage = true,
            RequireCanInteract = false,
        };

        var ev = doAfterArgs.Event as BodyBagCinchDoAfterEvent;
        if (ev != null)
            ev.TargetCinched = false;

        _doAfter.TryStartDoAfter(doAfterArgs);
    }
}
