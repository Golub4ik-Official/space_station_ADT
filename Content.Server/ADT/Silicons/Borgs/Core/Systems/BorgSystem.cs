// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.Core.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgSystem : SharedBorgSystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    /// <summary>
    /// ADT-Tweak: prototype spawned as the default brain when a borg is initialised
    /// without one (matches the SS13 fallback in robot_mob.dm:
    ///   if(mmi == null) mmi = new /obj/item/mmi/robotic_brain(src) ).
    /// </summary>
    private const string DefaultBrainPrototype = "RoboticBrain";

    /// <summary>ADT construction-system brain container id.</summary>
    private const string BrainContainerId = "_brain";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BorgComponent, EntInsertedIntoContainerMessage>(OnContainerInserted);
        SubscribeLocalEvent<BorgComponent, EntRemovedFromContainerMessage>(OnContainerRemoved);
    }

    private void OnMapInit(EntityUid uid, BorgComponent component, MapInitEvent args)
    {
        EnsureComp<BorgSlotComponent>(uid);
        EnsureComp<BorgBatteryComponent>(uid);
        EnsureComp<BorgWiresComponent>(uid);
        EnsureComp<BorgLawComponent>(uid);
        EnsureComp<BorgRebootComponent>(uid);

        SetupComponentParts(uid);
        EnsureDefaultBrain(uid, component);
    }

    /// <summary>
    /// ADT-Tweak: SS13-parity fallback. If the borg has no brain in its
    /// <c>_brain</c> container after MapInit (e.g. admin-spawned bare chassis,
    /// or a prototype without an explicit <c>ContainerFill</c>), spawn a
    /// <c>RoboticBrain</c> and insert it. Player-spawned borgs that already
    /// fill the container via <c>ContainerFill</c> are left untouched.
    /// </summary>
    private void EnsureDefaultBrain(EntityUid uid, BorgComponent component)
    {
        if (component.BrainEntity != null)
            return;

        var container = _container.EnsureContainer<ContainerSlot>(uid, BrainContainerId);
        if (container.ContainedEntity != null)
        {
            component.BrainEntity = container.ContainedEntity;
            Dirty(uid, component);
            return;
        }

        var brain = Spawn(DefaultBrainPrototype, _transform.GetMapCoordinates(uid));
        if (!_container.Insert(brain, container))
            return;

        component.BrainEntity = brain;
        Dirty(uid, component);
    }

    private void SetupComponentParts(EntityUid uid)
    {
        var parts = new (BorgPartType type, float maxDamage)[]
        {
            (BorgPartType.Armour, 100f),
            (BorgPartType.Actuator, 50f),
            (BorgPartType.Radio, 40f),
            (BorgPartType.BinaryCommunication, 30f),
            (BorgPartType.Camera, 40f),
            (BorgPartType.DiagnosisUnit, 30f),
        };

        foreach (var (type, maxDamage) in parts)
        {
            var part = Spawn(null, _transform.GetMapCoordinates(uid));
            var partComp = AddComp<BorgComponentPartComponent>(part);
            partComp.PartType = type;
            partComp.MaxDamage = maxDamage;
            partComp.BruteDamage = 0;
            partComp.BurnDamage = 0;
            partComp.Broken = false;
            partComp.Installed = true;
            partComp.OwnerBorg = uid;
            Dirty(part, partComp);
        }
    }

    private void OnContainerInserted(EntityUid uid, BorgComponent component, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID == "_brain")
        {
            component.BrainEntity = args.Entity;
            Dirty(uid, component);
        }
    }

    private void OnContainerRemoved(EntityUid uid, BorgComponent component, EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID == "_brain")
        {
            component.BrainEntity = null;
            Dirty(uid, component);
        }
    }

    public void SetActive(EntityUid uid, bool active, BorgComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.IsActive = active;
        Dirty(uid, component);
    }
}
