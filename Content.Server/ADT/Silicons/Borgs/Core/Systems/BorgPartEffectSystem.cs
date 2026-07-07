using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.Core.Events;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Radio.Components;

namespace Content.Server.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgPartEffectSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly BlindableSystem _blindable = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgComponent, BorgComponentPartBrokenEvent>(OnPartBroken);
        SubscribeLocalEvent<BorgComponent, BorgComponentPartRepairedEvent>(OnPartRepaired);
    }

    private void OnPartBroken(EntityUid uid, BorgComponent component, ref BorgComponentPartBrokenEvent args)
    {
        switch (args.PartType)
        {
            case BorgPartType.Actuator:
                HandleActuatorBroken(uid);
                break;
            case BorgPartType.Camera:
                HandleCameraBroken(uid);
                break;
            case BorgPartType.Radio:
                HandleRadioBroken(uid);
                break;
            case BorgPartType.BinaryCommunication:
                HandleBinaryBroken(uid);
                break;
        }
    }

    private void OnPartRepaired(EntityUid uid, BorgComponent component, ref BorgComponentPartRepairedEvent args)
    {
        switch (args.PartType)
        {
            case BorgPartType.Actuator:
                HandleActuatorRepaired(uid);
                break;
            case BorgPartType.Camera:
                HandleCameraRepaired(uid);
                break;
            case BorgPartType.Radio:
                HandleRadioRepaired(uid);
                break;
            case BorgPartType.BinaryCommunication:
                HandleBinaryRepaired(uid);
                break;
        }
    }

    private void HandleActuatorBroken(EntityUid uid)
    {
        if (TryComp<MovementSpeedModifierComponent>(uid, out var move))
            _movement.ChangeBaseSpeed(uid, move.BaseWalkSpeed * 0.5f, move.BaseSprintSpeed * 0.5f, move.BaseAcceleration, move);
    }

    private void HandleActuatorRepaired(EntityUid uid)
    {
        if (TryComp<MovementSpeedModifierComponent>(uid, out var move))
            _movement.ChangeBaseSpeed(uid, MovementSpeedModifierComponent.DefaultBaseWalkSpeed, MovementSpeedModifierComponent.DefaultBaseSprintSpeed, MovementSpeedModifierComponent.DefaultAcceleration, move);
    }

    private void HandleCameraBroken(EntityUid uid)
    {
        var blindable = EnsureComp<BlindableComponent>(uid);
        _blindable.AdjustEyeDamage((uid, blindable), blindable.MaxDamage);
    }

    private void HandleCameraRepaired(EntityUid uid)
    {
        if (TryComp<BlindableComponent>(uid, out var blindable))
        {
            _blindable.AdjustEyeDamage((uid, blindable), -blindable.EyeDamage);
            RemComp<BlindableComponent>(uid);
        }
    }

    private void HandleRadioBroken(EntityUid uid)
    {
        RemComp<ActiveRadioComponent>(uid);
        RemComp<IntrinsicRadioTransmitterComponent>(uid);
    }

    private void HandleRadioRepaired(EntityUid uid)
    {
        var activeRadio = AddComp<ActiveRadioComponent>(uid);
        activeRadio.Channels.Add("Binary");
        activeRadio.Channels.Add("Common");

        var transmitter = AddComp<IntrinsicRadioTransmitterComponent>(uid);
        transmitter.Channels.Add("Binary");
        transmitter.Channels.Add("Common");
    }

    private void HandleBinaryBroken(EntityUid uid)
    {
        if (TryComp<ActiveRadioComponent>(uid, out var activeRadio))
            activeRadio.Channels.Remove("Binary");

        if (TryComp<IntrinsicRadioTransmitterComponent>(uid, out var transmitter))
            transmitter.Channels.Remove("Binary");
    }

    private void HandleBinaryRepaired(EntityUid uid)
    {
        if (TryComp<ActiveRadioComponent>(uid, out var activeRadio) && !activeRadio.Channels.Contains("Binary"))
            activeRadio.Channels.Add("Binary");

        if (TryComp<IntrinsicRadioTransmitterComponent>(uid, out var transmitter) && !transmitter.Channels.Contains("Binary"))
            transmitter.Channels.Add("Binary");
    }
}
