using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.Core.Events;
using Content.Shared.Damage;
using Content.Shared.Tools;
using Robust.Shared.Containers;
using Robust.Shared.Random;

namespace Content.Server.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgComponentPartSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgComponentPartComponent, BorgComponentPartDamagedEvent>(OnPartDamaged);
    }

    private void OnPartDamaged(EntityUid uid, BorgComponentPartComponent component, BorgComponentPartDamagedEvent args)
    {
        var wasBroken = component.Broken;
        component.BruteDamage += args.BruteDamage;
        component.BurnDamage += args.BurnDamage;

        var totalDamage = component.BruteDamage + component.BurnDamage;
        if (totalDamage >= component.MaxDamage)
            component.Broken = true;

        Dirty(uid, component);

        if (component.Broken && !wasBroken && component.OwnerBorg != null)
        {
            var ev = new BorgComponentPartBrokenEvent(component.OwnerBorg.Value, uid, component.PartType);
            RaiseLocalEvent(component.OwnerBorg.Value, ref ev);
        }
    }

    public void ApplyDamage(EntityUid uid, float brute, float burn, BorgComponentPartComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var ev = new BorgComponentPartDamagedEvent(uid, uid, brute, burn);
        RaiseLocalEvent(uid, ref ev);
    }

    public void RepairBrute(EntityUid uid, float amount, BorgComponentPartComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var wasBroken = component.Broken;
        component.BruteDamage = Math.Max(0, component.BruteDamage - amount);
        if (component.BruteDamage + component.BurnDamage < component.MaxDamage)
            component.Broken = false;

        Dirty(uid, component);

        if (wasBroken && !component.Broken && component.OwnerBorg != null)
        {
            var ev = new BorgComponentPartRepairedEvent(component.OwnerBorg.Value, uid, component.PartType, amount, 0);
            RaiseLocalEvent(component.OwnerBorg.Value, ref ev);
        }
    }

    public void RepairBurn(EntityUid uid, float amount, BorgComponentPartComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var wasBroken = component.Broken;
        component.BurnDamage = Math.Max(0, component.BurnDamage - amount);
        if (component.BruteDamage + component.BurnDamage < component.MaxDamage)
            component.Broken = false;

        Dirty(uid, component);

        if (wasBroken && !component.Broken && component.OwnerBorg != null)
        {
            var ev = new BorgComponentPartRepairedEvent(component.OwnerBorg.Value, uid, component.PartType, 0, amount);
            RaiseLocalEvent(component.OwnerBorg.Value, ref ev);
        }
    }

    public void SetInstalled(EntityUid uid, bool installed, BorgComponentPartComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.Installed = installed;
        Dirty(uid, component);
    }

    public float GetTotalDamage(EntityUid uid)
    {
        if (!TryComp<BorgComponentPartComponent>(uid, out var component))
            return 0;

        return component.BruteDamage + component.BurnDamage;
    }

    public float GetHealthPercent(EntityUid uid)
    {
        if (!TryComp<BorgComponentPartComponent>(uid, out var component))
            return 1f;

        if (component.MaxDamage <= 0)
            return 1f;

        return 1f - (component.BruteDamage + component.BurnDamage) / component.MaxDamage;
    }

    public void RemovePart(EntityUid uid, BorgPartType partType, BorgComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var query = EntityQueryEnumerator<BorgComponentPartComponent>();
        while (query.MoveNext(out var partUid, out var partComp))
        {
            if (partComp.PartType != partType || !partComp.Installed)
                continue;

            partComp.Installed = false;
            Dirty(partUid, partComp);

            if (_container.TryGetContainer(uid, "borg_parts", out var container))
            {
                _container.Remove(partUid, container);
            }

            return;
        }
    }

    public void RemoveRandomPart(EntityUid uid)
    {
        var query = EntityQueryEnumerator<BorgComponentPartComponent>();
        EntityUid? toRemove = null;
        while (query.MoveNext(out var partUid, out var partComp))
        {
            if (!partComp.Installed || partComp.PartType == BorgPartType.Cell)
                continue;

            if (partComp.Broken)
            {
                toRemove = partUid;
                break;
            }

            toRemove ??= partUid;
        }

        if (toRemove != null)
        {
            RemovePart(uid, Comp<BorgComponentPartComponent>(toRemove.Value).PartType);
        }
    }

    public void RepairAllParts(EntityUid borgUid)
    {
        var query = EntityQueryEnumerator<BorgComponentPartComponent>();
        while (query.MoveNext(out var partUid, out var partComp))
        {
            if (!partComp.Installed)
                continue;

            var wasBroken = partComp.Broken;
            partComp.BruteDamage = 0;
            partComp.BurnDamage = 0;
            partComp.Broken = false;
            Dirty(partUid, partComp);

            if (wasBroken && partComp.OwnerBorg != null)
            {
                var ev = new BorgComponentPartRepairedEvent(partComp.OwnerBorg.Value, partUid, partComp.PartType, 0, 0);
                RaiseLocalEvent(partComp.OwnerBorg.Value, ref ev);
            }
        }
    }
}
