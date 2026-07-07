// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.Core.Events;
using Robust.Shared.Containers;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Systems;

public abstract class SharedBorgSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgComponent, ComponentInit>(OnBorgInit);
    }

    private void OnBorgInit(EntityUid uid, BorgComponent component, ComponentInit args)
    {
        component.ActiveSlotIndex = 0;
        component.IsActive = true;
    }

    public void SetLocked(EntityUid uid, bool locked, BorgComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.Locked = locked;
        Dirty(uid, component);
    }

    public void SetMaintenancePanel(EntityUid uid, bool open, BorgComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.MaintenancePanelOpen = open;
        Dirty(uid, component);
    }

    /// <summary>ADT-Tweak: toggles the maintenance cover lock (SS13 <c>locked</c>).</summary>
    public void SetPanelLocked(EntityUid uid, bool locked, BorgComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.PanelLocked = locked;
        Dirty(uid, component);
    }

    public bool TryGetBrain(EntityUid uid, out EntityUid brain, BorgComponent? component = null)
    {
        brain = default;
        if (!Resolve(uid, ref component))
            return false;

        if (component.BrainEntity == null)
            return false;

        brain = component.BrainEntity.Value;
        return true;
    }

    public void SetBrain(EntityUid uid, EntityUid brain, BorgComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.BrainEntity = brain;
        Dirty(uid, component);
    }
}
