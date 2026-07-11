using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.HealthExaminable;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Storage.Components;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Shared.ADT.Medical.BodyBags;

public sealed class BodyBagOccupantHealthSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examineSystem = default!;
    [Dependency] private readonly HealthExaminableSystem _healthExaminable = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BodyBagOccupantHealthComponent, GetVerbsEvent<ExamineVerb>>(OnGetExamineVerbs);
        SubscribeLocalEvent<BodyBagOccupantHealthComponent, ExaminedEvent>(OnExamined);
    }

    private void OnGetExamineVerbs(EntityUid uid, BodyBagOccupantHealthComponent comp, GetVerbsEvent<ExamineVerb> args)
    {
        var occupant = GetOccupant(uid);
        if (occupant == null)
            return;

        if (!TryComp<HealthExaminableComponent>(occupant, out var healthComp) || !TryComp<DamageableComponent>(occupant, out var damageComp))
            return;

        var detailsRange = _examineSystem.IsInDetailsRange(args.User, uid);
        var user = args.User;
        var ent = uid;

        var verb = new ExamineVerb()
        {
            Act = () =>
            {
                var markup = _healthExaminable.CreateMarkup(occupant.Value, healthComp, damageComp);
                _examineSystem.SendExamineTooltip(user, ent, markup, false, false);
            },
            Text = Loc.GetString("health-examinable-verb-text"),
            Category = VerbCategory.Examine,
            Disabled = !detailsRange,
            Message = detailsRange ? null : Loc.GetString("health-examinable-verb-disabled"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/rejuvenate.svg.192dpi.png"))
        };

        args.Verbs.Add(verb);
    }

    private void OnExamined(EntityUid uid, BodyBagOccupantHealthComponent comp, ExaminedEvent args)
    {
        var occupant = GetOccupant(uid);
        if (occupant == null)
            return;

        if (_mobState.IsDead(occupant.Value))
        {
            args.PushMarkup(Loc.GetString("body-bag-occupant-health-dead"));
        }
        else
        {
            var totalDamage = _damageable.GetTotalDamage((occupant.Value, null));
            if (totalDamage == FixedPoint2.Zero)
            {
                args.PushMarkup(Loc.GetString("body-bag-occupant-health-fine"));
            }
            else
            {
                string locKey;
                if (totalDamage > FixedPoint2.New(180))
                    locKey = "body-bag-occupant-health-critical";
                else if (totalDamage > FixedPoint2.New(100))
                    locKey = "body-bag-occupant-health-severe";
                else if (totalDamage > FixedPoint2.New(50))
                    locKey = "body-bag-occupant-health-injured";
                else
                    locKey = "body-bag-occupant-health-light";

                args.PushMarkup(Loc.GetString(locKey));
            }
        }

        if (args.IsInDetailsRange)
        {
            var occupantMessage = _examineSystem.GetExamineText(occupant.Value, args.Examiner);
            if (!occupantMessage.IsEmpty)
                args.PushMessage(occupantMessage);
        }
    }

    private EntityUid? GetOccupant(EntityUid uid)
    {
        if (!TryComp<EntityStorageComponent>(uid, out var storage) || storage.Open)
            return null;

        foreach (var contained in storage.Contents.ContainedEntities)
        {
            if (HasComp<MobStateComponent>(contained))
                return contained;
        }

        return null;
    }
}
