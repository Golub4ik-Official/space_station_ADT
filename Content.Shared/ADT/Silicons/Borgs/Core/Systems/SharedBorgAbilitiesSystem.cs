using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.Core.Events;
using Content.Shared.Actions;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Systems;

public abstract class SharedBorgAbilitiesSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgComponent, ComponentStartup>(OnBorgStartup);
        SubscribeLocalEvent<BorgComponent, BorgHeadlampToggleEvent>(OnHeadlampToggle);
        SubscribeLocalEvent<BorgComponent, BorgStateLawsEvent>(OnStateLaws);
        SubscribeLocalEvent<BorgComponent, BorgSelfDiagnosisEvent>(OnSelfDiagnosis);
        SubscribeLocalEvent<BorgComponent, BorgSensorModeEvent>(OnSensorMode);
        SubscribeLocalEvent<BorgComponent, BorgMagpulseToggleEvent>(OnMagpulseToggle);
        SubscribeLocalEvent<BorgComponent, BorgThrustersToggleEvent>(OnThrustersToggle);
        SubscribeLocalEvent<BorgComponent, BorgRadioToggleEvent>(OnRadioToggle);
    }

    private void OnBorgStartup(EntityUid uid, BorgComponent component, ComponentStartup args)
    {
        _actions.AddAction(uid, "ActionBorgHeadlamp");
        _actions.AddAction(uid, "ActionBorgStateLaws");
        _actions.AddAction(uid, "ActionBorgSelfDiagnosis");
        _actions.AddAction(uid, "ActionBorgSensorMode");
        _actions.AddAction(uid, "ActionBorgMagpulse");
        _actions.AddAction(uid, "ActionBorgThrusters");
        _actions.AddAction(uid, "ActionBorgPda");
        _actions.AddAction(uid, "ActionBorgRadio");
    }

    private void OnHeadlampToggle(EntityUid uid, BorgComponent component, BorgHeadlampToggleEvent args)
    {
        ToggleHeadlamp(uid);
    }

    private void OnStateLaws(EntityUid uid, BorgComponent component, BorgStateLawsEvent args)
    {
        ShowLaws(uid);
    }

    private void OnSelfDiagnosis(EntityUid uid, BorgComponent component, BorgSelfDiagnosisEvent args)
    {
        ShowDiagnosis(uid);
    }

    private void OnSensorMode(EntityUid uid, BorgComponent component, BorgSensorModeEvent args)
    {
        CycleSensorMode(uid);
    }

    private void OnMagpulseToggle(EntityUid uid, BorgComponent component, BorgMagpulseToggleEvent args)
    {
        ToggleMagpulse(uid);
    }

    private void OnThrustersToggle(EntityUid uid, BorgComponent component, BorgThrustersToggleEvent args)
    {
        ToggleThrusters(uid);
    }

    private void OnRadioToggle(EntityUid uid, BorgComponent component, BorgRadioToggleEvent args)
    {
        ToggleRadio(uid);
    }

    protected abstract void ToggleHeadlamp(EntityUid uid);
    protected abstract void ShowLaws(EntityUid uid);
    protected abstract void ShowDiagnosis(EntityUid uid);
    protected abstract void CycleSensorMode(EntityUid uid);
    protected abstract void ToggleMagpulse(EntityUid uid);
    protected abstract void ToggleThrusters(EntityUid uid);
    protected abstract void ToggleRadio(EntityUid uid);
}

public sealed partial class BorgHeadlampToggleEvent : InstantActionEvent;
public sealed partial class BorgStateLawsEvent : InstantActionEvent;
public sealed partial class BorgSelfDiagnosisEvent : InstantActionEvent;
public sealed partial class BorgSensorModeEvent : InstantActionEvent;
public sealed partial class BorgMagpulseToggleEvent : InstantActionEvent;
public sealed partial class BorgRadioToggleEvent : InstantActionEvent;
