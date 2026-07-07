// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using System.Linq;
using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Robust.Shared.Random;

namespace Content.Server.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgDamageSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly BorgComponentPartSystem _parts = default!;
    [Dependency] private readonly BorgDeathSystem _death = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgComponent, DamageDealtEvent>(OnDamageDealt);
        SubscribeLocalEvent<BorgComponent, ExaminedEvent>(OnExamine);
    }

    private void OnDamageDealt(EntityUid uid, BorgComponent component, ref DamageDealtEvent args)
    {
        var bruteDamage = (float)args.Damage.DamageDict.GetValueOrDefault("Blunt", FixedPoint2.Zero)
                        + (float)args.Damage.DamageDict.GetValueOrDefault("Slash", FixedPoint2.Zero)
                        + (float)args.Damage.DamageDict.GetValueOrDefault("Piercing", FixedPoint2.Zero);

        var burnDamage = (float)args.Damage.DamageDict.GetValueOrDefault("Heat", FixedPoint2.Zero)
                       + (float)args.Damage.DamageDict.GetValueOrDefault("Shock", FixedPoint2.Zero);

        if (bruteDamage <= 0 && burnDamage <= 0)
            return;

        DistributeDamage(uid, bruteDamage, burnDamage);

        var totalDamage = 0f;
        var maxDamage = 0f;
        var partQuery = EntityQueryEnumerator<BorgComponentPartComponent>();
        while (partQuery.MoveNext(out var _, out var part))
        {
            if (part.Installed)
            {
                totalDamage += part.BruteDamage + part.BurnDamage;
                maxDamage += part.MaxDamage;
            }
        }

        if (maxDamage > 0 && totalDamage >= maxDamage)
            _death.KillBorg(uid);
    }

    private void DistributeDamage(EntityUid uid, float brute, float burn)
    {
        var parts = new List<EntityUid>();
        var partQuery = EntityQueryEnumerator<BorgComponentPartComponent>();
        while (partQuery.MoveNext(out var partUid, out var partComp))
        {
            if (partComp.Installed && !partComp.Broken)
                parts.Add(partUid);
        }

        if (parts.Count == 0)
            return;

        foreach (var partUid in parts)
        {
            var partComp = Comp<BorgComponentPartComponent>(partUid);
            if (partComp.PartType != BorgPartType.Armour)
                continue;

            var armourBrute = Math.Min(brute, partComp.MaxDamage - partComp.BruteDamage);
            var armourBurn = Math.Min(burn, partComp.MaxDamage - partComp.BurnDamage);

            _parts.ApplyDamage(partUid, armourBrute, armourBurn);
            brute -= armourBrute;
            burn -= armourBurn;
            break;
        }

        var nonArmourParts = parts
            .Where(p => Comp<BorgComponentPartComponent>(p).PartType != BorgPartType.Armour)
            .OrderBy(_ => _random.Next())
            .ToList();

        foreach (var partUid in nonArmourParts)
        {
            if (brute <= 0 && burn <= 0)
                break;

            var partComp = Comp<BorgComponentPartComponent>(partUid);
            var remaining = partComp.MaxDamage - partComp.BruteDamage - partComp.BurnDamage;
            if (remaining <= 0)
                continue;

            var damageToApply = Math.Min(brute + burn, remaining);
            var bruteShare = Math.Min(brute, damageToApply * (brute / (brute + burn)));
            var burnShare = damageToApply - bruteShare;

            _parts.ApplyDamage(partUid, bruteShare, burnShare);
            brute -= bruteShare;
            burn -= burnShare;
        }
    }

    private void OnExamine(EntityUid uid, BorgComponent component, ExaminedEvent args)
    {
        args.PushText(Loc.GetString("borg-construction-examine",
            ("panel", component.MaintenancePanelOpen ? "open" : "closed")));

        var totalBrute = 0f;
        var totalBurn = 0f;
        var partQuery = EntityQueryEnumerator<BorgComponentPartComponent>();
        while (partQuery.MoveNext(out var _, out var partComp))
        {
            if (!partComp.Installed)
                continue;

            totalBrute += partComp.BruteDamage;
            totalBurn += partComp.BurnDamage;
        }

        var totalDamage = totalBrute + totalBurn;
        if (totalDamage <= 0)
        {
            args.PushText(Loc.GetString("borg-examine-no-damage"));
            return;
        }

        if (totalDamage < 30)
            args.PushText(Loc.GetString("borg-examine-light-damage"));
        else if (totalDamage < 60)
            args.PushText(Loc.GetString("borg-examine-moderate-damage"));
        else
            args.PushText(Loc.GetString("borg-examine-heavy-damage"));
    }
}
