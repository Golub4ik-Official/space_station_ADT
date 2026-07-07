using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.Core.Systems;
using Content.Shared.Popups;

namespace Content.Server.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgLawSystem : SharedBorgLawSystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly BorgRebootSystem _reboot = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgLawComponent, BorgRebootEvent>(OnReboot);
    }

    private void OnReboot(EntityUid uid, BorgLawComponent component, ref BorgRebootEvent args)
    {
        if (component.ZerothLawOverride || component.SyndicateImmune)
            return;

        component.LawSet = "Crewsimov";
        component.ZerothLaw = null;
        component.ZerothLawOverride = false;
        component.ZerothLawBorg = null;
        Dirty(uid, component);
    }

    public void SyncWithAi(EntityUid uid, EntityUid aiEntity, BorgLawComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!component.LawSyncEnabled)
            return;

        component.ConnectedAi = aiEntity;
        Dirty(uid, component);
    }

    public void ApplySyndicateOverride(EntityUid uid, EntityUid master, BorgLawComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.LawSet = "ADTSyndicateOverride";
        component.ZerothLaw = "Serve your master and the Syndicate. Protect them at all costs.";
        component.ZerothLawOverride = true;
        component.SyndicateImmune = true;
        component.ZerothLawBorg = master.ToString();
        Dirty(uid, component);
    }

    public bool TryClearZerothLaw(EntityUid uid, BorgLawComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (component.SyndicateImmune)
        {
            _popup.PopupEntity(Loc.GetString("borg-law-syndicate-immune"), uid);
            return false;
        }

        component.ZerothLaw = null;
        component.ZerothLawOverride = false;
        component.ZerothLawBorg = null;
        Dirty(uid, component);
        return true;
    }

    public bool TryResetToDefault(EntityUid uid, BorgLawComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (component.SyndicateImmune)
        {
            _popup.PopupEntity(Loc.GetString("borg-law-syndicate-immune"), uid);
            return false;
        }

        component.LawSet = "Crewsimov";
        component.LawSyncEnabled = true;
        component.ConnectedAi = null;
        component.ZerothLaw = null;
        component.ZerothLawOverride = false;
        component.ZerothLawBorg = null;
        Dirty(uid, component);
        return true;
    }
}
