using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Popups;

namespace Content.Server.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgEmagSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly BorgLawSystem _law = default!;
    [Dependency] private readonly BorgModuleSystem _module = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnEmagged(EntityUid uid, BorgComponent component, GotEmaggedEvent args)
    {
        // ADT-Tweak-Start: SS13 2-step emag.
        // Step 1: cover closed + locked  →  unlock the cover, return (no emag yet).
        // Step 2: cover open             →  apply the full emag effect.
        if (!component.MaintenancePanelOpen)
        {
            if (!component.PanelLocked)
            {
                // Cover already unlocked — nothing to do at this step.
                args.Handled = false;
                return;
            }

            component.PanelLocked = false;
            Dirty(uid, component);
            _popup.PopupEntity(Loc.GetString("borg-emag-cover-unlocked"), uid);
            args.Handled = true;
            return;
        }
        // ADT-Tweak-End

        var emag = EnsureComp<BorgEmagComponent>(uid);

        if (emag.Emagged)
        {
            args.Handled = false;
            return;
        }

        emag.Emagged = true;
        emag.MindslaveMaster = args.UserUid;

        var masterName = MetaData(args.UserUid).EntityName;
        _law.ApplySyndicateOverride(uid, args.UserUid);

        ApplyModuleEmagEffect(uid);

        _popup.PopupEntity(Loc.GetString("borg-emag-success", ("master", masterName)), uid);

        Dirty(uid, emag);
        args.Handled = true;
    }

    private void ApplyModuleEmagEffect(EntityUid uid)
    {
        if (!TryComp<ADTBorgModuleComponent>(uid, out var module))
            return;

        switch (module.ModuleType)
        {
            case "Security":
                _popup.PopupEntity(Loc.GetString("borg-emag-security"), uid);
                break;
            case "Medical":
                _popup.PopupEntity(Loc.GetString("borg-emag-medical"), uid);
                break;
            case "Engineering":
                _popup.PopupEntity(Loc.GetString("borg-emag-engineering"), uid);
                break;
            case "Janitor":
                _popup.PopupEntity(Loc.GetString("borg-emag-janitor"), uid);
                break;
            case "Service":
                _popup.PopupEntity(Loc.GetString("borg-emag-service"), uid);
                break;
            case "Miner":
                _popup.PopupEntity(Loc.GetString("borg-emag-miner"), uid);
                break;
            case "Combat":
                _popup.PopupEntity(Loc.GetString("borg-emag-combat"), uid);
                break;
            case "Destroyer":
                _popup.PopupEntity(Loc.GetString("borg-emag-destroyer"), uid);
                break;
            case "Syndicate":
                _popup.PopupEntity(Loc.GetString("borg-emag-syndicate"), uid);
                break;
        }
    }

    public bool IsEmagged(EntityUid uid)
    {
        return TryComp<BorgEmagComponent>(uid, out var emag) && emag.Emagged;
    }

    public void UnEmag(EntityUid uid)
    {
        if (!TryComp<BorgEmagComponent>(uid, out var emag))
            return;

        if (TryComp<BorgLawComponent>(uid, out var law) && law.SyndicateImmune)
        {
            emag.Emagged = false;
            emag.MindslaveMaster = null;
            emag.WeaponsUnlocked = false;
            Dirty(uid, emag);
            _popup.PopupEntity(Loc.GetString("borg-law-syndicate-immune"), uid);
            return;
        }

        emag.Emagged = false;
        emag.MindslaveMaster = null;
        emag.WeaponsUnlocked = false;

        _law.TryClearZerothLaw(uid);
        _law.TryResetToDefault(uid);

        Dirty(uid, emag);
    }
}
