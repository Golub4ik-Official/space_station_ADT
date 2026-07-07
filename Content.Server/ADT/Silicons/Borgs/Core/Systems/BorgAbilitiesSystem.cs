using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.Core.Systems;
using Content.Shared.ADT.MesonVision;
using Content.Shared.ADT.ThermalVision;
using Content.Shared.Light;
using Content.Shared.Light.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Popups;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgAbilitiesSystem : SharedBorgAbilitiesSystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BorgBatterySystem _battery = default!;
    [Dependency] private readonly BorgLawSystem _lawSystem = default!;
    [Dependency] private readonly SharedHandheldLightSystem _light = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedMesonVisionSystem _meson = default!;
    [Dependency] private readonly SharedThermalVisionSystem _thermal = default!;

    public void DisplayLaws(EntityUid uid)
    {
        ShowLaws(uid);
    }

    protected override void ToggleHeadlamp(EntityUid uid)
    {
        if (!_battery.TryDrainPower(uid, 5))
        {
            _popup.PopupEntity(Loc.GetString("borg-low-power"), uid, uid);
            return;
        }

        if (TryComp<HandheldLightComponent>(uid, out var light))
        {
            _light.SetActivated(uid, !light.Activated, light);
        }
        else
        {
            var newLight = AddComp<HandheldLightComponent>(uid);
            _light.SetActivated(uid, true, newLight);
        }
    }

    protected override void CycleSensorMode(EntityUid uid)
    {
        var sensor = EnsureComp<BorgSensorComponent>(uid);
        sensor.CurrentMode = (byte)((sensor.CurrentMode + 1) % sensor.MaxMode);

        ClearVisionModes(uid);

        switch ((BorgSensorMode)sensor.CurrentMode)
        {
            case BorgSensorMode.Meson:
                EnsureComp<MesonVisionComponent>(uid);
                _popup.PopupEntity(Loc.GetString("borg-sensor-meson"), uid, uid);
                break;
            case BorgSensorMode.Thermal:
                EnsureComp<ThermalVisionComponent>(uid);
                _popup.PopupEntity(Loc.GetString("borg-sensor-thermal"), uid, uid);
                break;
            default:
                _popup.PopupEntity(Loc.GetString("borg-sensor-off"), uid, uid);
                break;
        }

        Dirty(uid, sensor);
    }

    protected override void ToggleMagpulse(EntityUid uid)
    {
        if (TryComp<MovementIgnoreGravityComponent>(uid, out var mag))
        {
            RemComp<MovementIgnoreGravityComponent>(uid);
            _popup.PopupEntity(Loc.GetString("borg-magpulse-off"), uid, uid);
        }
        else
        {
            if (!_battery.TryDrainPower(uid, 10))
            {
                _popup.PopupEntity(Loc.GetString("borg-low-power"), uid, uid);
                return;
            }

            var magpulse = AddComp<MovementIgnoreGravityComponent>(uid);
            magpulse.Weightless = true;
            Dirty(uid, magpulse);
            _popup.PopupEntity(Loc.GetString("borg-magpulse-on"), uid, uid);
        }
    }

    private void ClearVisionModes(EntityUid uid)
    {
        RemComp<MesonVisionComponent>(uid);
        RemComp<ThermalVisionComponent>(uid);
    }

    protected override void ToggleThrusters(EntityUid uid)
    {
        if (TryComp<MovementIgnoreGravityComponent>(uid, out var thrust))
        {
            RemComp<MovementIgnoreGravityComponent>(uid);
            _popup.PopupEntity(Loc.GetString("borg-thrusters-off"), uid, uid);
        }
        else
        {
            if (!_battery.TryDrainPower(uid, 25))
            {
                _popup.PopupEntity(Loc.GetString("borg-low-power"), uid, uid);
                return;
            }

            var thrusters = AddComp<MovementIgnoreGravityComponent>(uid);
            thrusters.Weightless = true;
            Dirty(uid, thrusters);
            _popup.PopupEntity(Loc.GetString("borg-thrusters-on"), uid, uid);
        }
    }

    protected override void ShowLaws(EntityUid uid)
    {
        var lawDisplay = _lawSystem.GetLawDisplay(uid);
        var message = Loc.GetString("borg-laws-header") + "\n" + lawDisplay;
        _popup.PopupEntity(message, uid, uid);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/ADT/Borgs/biamthelaw.ogg"), uid);
    }

    protected override void ShowDiagnosis(EntityUid uid)
    {
        var chargePercent = 0f;
        if (TryComp<BorgBatteryComponent>(uid, out var bat) && bat.MaxCharge > 0)
            chargePercent = (bat.Charge / bat.MaxCharge) * 100;

        var parts = new List<string>();
        var partQuery = EntityQueryEnumerator<BorgComponentPartComponent>();
        while (partQuery.MoveNext(out var partUid, out var part))
        {
            if (!part.Installed) continue;
            var health = part.MaxDamage > 0
                ? (int)((1 - (part.BruteDamage + part.BurnDamage) / part.MaxDamage) * 100)
                : 100;
            var status = part.Broken ? Loc.GetString("borg-diagnosis-broken") : $"{health}%";
            parts.Add($"- {part.PartType}: {status}");
        }

        var diagnosis = Loc.GetString("borg-diagnosis-header",
            ("charge", $"{chargePercent:F0}%"),
            ("parts", string.Join("\n", parts)));
        _popup.PopupEntity(diagnosis, uid, uid);
    }

    protected override void ToggleRadio(EntityUid uid)
    {
        if (HasComp<ActiveRadioComponent>(uid))
        {
            RemComp<ActiveRadioComponent>(uid);
            RemComp<IntrinsicRadioTransmitterComponent>(uid);
            _popup.PopupEntity(Loc.GetString("borg-radio-off"), uid, uid);
        }
        else
        {
            var activeRadio = AddComp<ActiveRadioComponent>(uid);
            activeRadio.Channels.Add("Binary");
            activeRadio.Channels.Add("Common");

            var transmitter = AddComp<IntrinsicRadioTransmitterComponent>(uid);
            transmitter.Channels.Add("Binary");
            transmitter.Channels.Add("Common");

            _popup.PopupEntity(Loc.GetString("borg-radio-on"), uid, uid);
        }
    }
}
