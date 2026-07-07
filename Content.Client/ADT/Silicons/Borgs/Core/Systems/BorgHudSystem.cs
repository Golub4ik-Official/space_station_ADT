using Content.Client.ADT.Silicons.Borgs.Core.UI;
using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.Actions;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgHudSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    private BorgHudWindow? _window;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<BorgComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        SubscribeLocalEvent<BorgComponent, AfterAutoHandleStateEvent>(OnBorgStateChanged);
        SubscribeLocalEvent<BorgSlotComponent, AfterAutoHandleStateEvent>(OnBorgStateChanged);
        SubscribeLocalEvent<BorgBatteryComponent, AfterAutoHandleStateEvent>(OnBorgStateChanged);
        SubscribeLocalEvent<BorgLawComponent, AfterAutoHandleStateEvent>(OnBorgStateChanged);
        SubscribeLocalEvent<BorgRebootComponent, AfterAutoHandleStateEvent>(OnBorgStateChanged);
    }

    private void OnPlayerAttached(EntityUid uid, BorgComponent component, LocalPlayerAttachedEvent args)
    {
        if (_window != null)
            _window.Close();

        _window = new BorgHudWindow();
        _window.SetBorgEntity(uid);

        _window.OnPdaPressed += OnPdaPressed;
        _window.OnHeadlampPressed += OnHeadlampPressed;
        _window.OnThrustersPressed += OnThrustersPressed;
        _window.OnSensorModePressed += OnSensorModePressed;
        _window.OnRadioPressed += OnRadioPressed;

        _window.Open();
    }

    private void OnPlayerDetached(EntityUid uid, BorgComponent component, LocalPlayerDetachedEvent args)
    {
        if (_window == null)
            return;

        _window.OnPdaPressed -= OnPdaPressed;
        _window.OnHeadlampPressed -= OnHeadlampPressed;
        _window.OnThrustersPressed -= OnThrustersPressed;
        _window.OnSensorModePressed -= OnSensorModePressed;
        _window.OnRadioPressed -= OnRadioPressed;

        _window.Close();
        _window = null;
    }

    private void OnPdaPressed(EntityUid uid) => TryPerformAction(uid, "ActionBorgPda");
    private void OnHeadlampPressed(EntityUid uid) => TryPerformAction(uid, "ActionBorgHeadlamp");
    private void OnThrustersPressed(EntityUid uid) => TryPerformAction(uid, "ActionBorgThrusters");
    private void OnSensorModePressed(EntityUid uid) => TryPerformAction(uid, "ActionBorgSensorMode");
    private void OnRadioPressed(EntityUid uid) => TryPerformAction(uid, "ActionBorgRadio");

    private void TryPerformAction(EntityUid uid, string actionPrototypeId)
    {
        foreach (var action in _actions.GetActions(uid))
        {
            if (MetaData(action.Owner).EntityPrototype?.ID == actionPrototypeId)
            {
                _actions.PerformAction(uid, action, predicted: false);
                return;
            }
        }
    }

    private void OnBorgStateChanged<T>(EntityUid uid, T component, ref AfterAutoHandleStateEvent args) where T : Component
    {
        if (_window == null)
            return;

        var borgEntity = _window.GetBorgEntity();
        if (borgEntity == null || borgEntity.Value != uid)
            return;

        _window.UpdateAll();
    }
}
