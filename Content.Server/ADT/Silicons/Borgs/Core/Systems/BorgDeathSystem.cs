// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.StatusEffect;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Server.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgDeathSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly BorgRebootSystem _reboot = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgComponent, ComponentShutdown>(OnBorgShutdown);
        SubscribeLocalEvent<BorgComponent, BorgRebootEvent>(OnReboot);
    }

    private void OnBorgShutdown(EntityUid uid, BorgComponent component, ComponentShutdown args)
    {
        component.IsActive = false;
        EjectBrain(uid, component);
    }

    public void KillBorg(EntityUid uid, BorgComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.IsActive = false;
        Dirty(uid, component);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/ADT/Borgs/borg_deathsound.ogg"), uid);
        _status.TryRemoveAllStatusEffects(uid);
        EjectBrain(uid, component);
    }

    public void GibBorg(EntityUid uid, BorgComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        EjectBrain(uid, component);
        QueueDel(uid);
    }

    private void EjectBrain(EntityUid uid, BorgComponent component)
    {
        var brain = component.BrainEntity;
        if (brain == null || !Exists(brain.Value))
            return;

        if (_container.TryGetContainer(uid, "_brain", out var container))
        {
            _container.Remove(brain.Value, container);
            _transform.DropNextTo(brain.Value, uid);
        }

        component.BrainEntity = null;
        Dirty(uid, component);
    }

    private void OnReboot(EntityUid uid, BorgComponent component, ref BorgRebootEvent args)
    {
        component.IsActive = true;
        Dirty(uid, component);
    }
}
