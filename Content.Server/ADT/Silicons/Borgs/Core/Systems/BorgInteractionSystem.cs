using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.Core.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.PDA;
using Content.Shared.Tools.Systems;
using Content.Shared.Verbs;
using Content.Shared.Wires;

namespace Content.Server.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgInteractionSystem : EntitySystem
{
    [Dependency] private readonly SharedToolSystem _tool = default!;
    [Dependency] private readonly BorgSystem _borg = default!;
    [Dependency] private readonly BorgSlotSystem _slots = default!;
    [Dependency] private readonly BorgModuleSystem _modules = default!;
    [Dependency] private readonly SharedBorgSlotSystem _sharedSlots = default!;
    [Dependency] private readonly SharedBorgWiresSystem _wires = default!;
    [Dependency] private readonly BorgComponentPartSystem _parts = default!;
    [Dependency] private readonly AccessReaderSystem _access = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    /// <summary>ADT-Tweak: access tag required to toggle the panel cover lock.</summary>
    private const string PanelLockAccessTag = "Research";

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
        SubscribeLocalEvent<BorgComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<BorgComponent, ActivateInWorldEvent>(OnActivateInWorld);
    }

    private void OnGetVerbs(EntityUid uid, BorgComponent component, GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        // ADT-Tweak: borg can toggle its own cover lock (SS13 toggle_own_cover verb)
        if (args.User == uid)
        {
            args.Verbs.Add(new Verb
            {
                Text = Loc.GetString("borg-verb-toggle-own-lock"),
                Act = () => _borg.SetPanelLocked(uid, !component.PanelLocked, component),
                Priority = 0
            });
        }

        args.Verbs.Add(new Verb
        {
            Text = Loc.GetString("borg-verb-toggle-panel"),
            Act = () => _borg.SetMaintenancePanel(uid, !component.MaintenancePanelOpen),
            Priority = 1
        });

        args.Verbs.Add(new Verb
        {
            Text = Loc.GetString("borg-verb-cycle-slot"),
            Act = () => _sharedSlots.CycleForward(uid),
            Priority = 2
        });

        if (TryComp<BorgLawComponent>(uid, out var law))
        {
            args.Verbs.Add(new Verb
            {
                Text = Loc.GetString("borg-verb-state-laws"),
                Act = () =>
                {
                    var lawSystem = EntityManager.System<BorgAbilitiesSystem>();
                    lawSystem.DisplayLaws(uid);
                },
                Priority = 3
            });
        }
    }

    private void OnInteractUsing(EntityUid uid, BorgComponent component, InteractUsingEvent args)
    {
        // ADT-Tweak-Start: ID card / PDA swipe to toggle panel cover lock (SS13 parity)
        if (HasComp<IdCardComponent>(args.Used) || HasComp<PdaComponent>(args.Used))
        {
            HandleIdCardSwipe(uid, component, args);
            return;
        }
        // ADT-Tweak-End

        if (_tool.HasQuality(args.Used, "Screwing"))
        {
            // ADT-Tweak: cannot open the panel while the cover is locked
            if (component.PanelLocked && !component.MaintenancePanelOpen)
            {
                _popup.PopupEntity(Loc.GetString("borg-panel-cover-locked"), uid, args.User);
                args.Handled = true;
                return;
            }

            _borg.SetMaintenancePanel(uid, !component.MaintenancePanelOpen);
            args.Handled = true;
            return;
        }

        if (_tool.HasQuality(args.Used, "Pulsing") || _tool.HasQuality(args.Used, "Cutting"))
        {
            if (!component.MaintenancePanelOpen)
                args.Handled = true;
            return;
        }

        // ADT-Tweak: crowbar on closed + locked cover is blocked
        if (!component.MaintenancePanelOpen && _tool.HasQuality(args.Used, "Prying"))
        {
            if (component.PanelLocked)
            {
                _popup.PopupEntity(Loc.GetString("borg-panel-cover-locked"), uid, args.User);
                args.Handled = true;
            }
            return;
        }

        if (component.MaintenancePanelOpen && _tool.HasQuality(args.Used, "Prying"))
        {
            _parts.RemoveRandomPart(uid);
            args.Handled = true;
            return;
        }

        if (component.MaintenancePanelOpen && _tool.HasQuality(args.Used, "Welding"))
        {
            _parts.RepairAllParts(uid);
            args.Handled = true;
            return;
        }

        if (HasComp<ADTBorgModuleComponent>(args.Used))
        {
            if (_modules.TryInstallModule(uid, args.Used, null, component))
            {
                QueueDel(args.Used);
                args.Handled = true;
            }
            return;
        }
    }

    /// <summary>
    /// ADT-Tweak: SS13 parity — swiping an ID card or PDA toggles the
    /// maintenance cover lock, provided the user has Research (robotics) access.
    /// </summary>
    private void HandleIdCardSwipe(EntityUid uid, BorgComponent component, InteractUsingEvent args)
    {
        if (component.MaintenancePanelOpen)
        {
            _popup.PopupEntity(Loc.GetString("borg-panel-id-close-first"), uid, args.User);
            args.Handled = true;
            return;
        }

        var tags = _access.FindAccessTags(args.User);
        if (!tags.Contains(PanelLockAccessTag))
        {
            _popup.PopupEntity(Loc.GetString("borg-panel-access-denied"), uid, args.User);
            args.Handled = true;
            return;
        }

        var newLocked = !component.PanelLocked;
        _borg.SetPanelLocked(uid, newLocked, component);
        _popup.PopupEntity(
            Loc.GetString(newLocked ? "borg-panel-locked" : "borg-panel-unlocked"),
            uid, args.User);
        args.Handled = true;
    }

    private void OnActivateInWorld(EntityUid uid, BorgComponent component, ActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        _sharedSlots.CycleForward(uid);
        args.Handled = true;
    }
}
