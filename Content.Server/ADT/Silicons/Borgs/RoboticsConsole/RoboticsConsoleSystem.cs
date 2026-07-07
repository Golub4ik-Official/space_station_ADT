using Content.Server.ADT.Silicons.Borgs.Core.Systems;
using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.RoboticsConsole;
using Content.Shared.Radio.Components;
using Robust.Server.GameObjects;

namespace Content.Server.ADT.Silicons.Borgs.RoboticsConsole;

public sealed class RoboticsConsoleSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly BorgDeathSystem _death = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RoboticsConsoleComponent, BoundUIOpenedEvent>(OnUIOpened);
        SubscribeLocalEvent<RoboticsConsoleComponent, RoboticsConsoleLockToggleMessage>(OnLockToggleMessage);
        SubscribeLocalEvent<RoboticsConsoleComponent, RoboticsConsoleKillBotMessage>(OnKillBotMessage);
        SubscribeLocalEvent<RoboticsConsoleComponent, RoboticsConsoleHackBotMessage>(OnHackBotMessage);
        SubscribeLocalEvent<RoboticsConsoleComponent, RoboticsConsoleMassLockMessage>(OnMassLockMessage);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<RoboticsConsoleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_uiSystem.IsUiOpen(uid, RoboticsConsoleUiKey.Key))
                UpdateUserInterface(uid);
        }
    }

    private void OnUIOpened(EntityUid uid, RoboticsConsoleComponent component, BoundUIOpenedEvent args)
    {
        UpdateUserInterface(uid);
    }

    private void OnLockToggleMessage(EntityUid uid, RoboticsConsoleComponent component, RoboticsConsoleLockToggleMessage message)
    {
        var borgEntity = GetEntity(message.BorgEntity);
        if (!TryComp<BorgComponent>(borgEntity, out var borg))
            return;

        borg.Locked = !borg.Locked;
        Dirty(borgEntity, borg);
        UpdateUserInterface(uid);
    }

    private void OnKillBotMessage(EntityUid uid, RoboticsConsoleComponent component, RoboticsConsoleKillBotMessage message)
    {
        var borgEntity = GetEntity(message.BorgEntity);
        if (!TryComp<BorgComponent>(borgEntity, out var borg))
            return;

        _death.KillBorg(borgEntity, borg);
        UpdateUserInterface(uid);
    }

    private void OnHackBotMessage(EntityUid uid, RoboticsConsoleComponent component, RoboticsConsoleHackBotMessage message)
    {
        // SS13: Traitor AI remote emag
        var borgEntity = GetEntity(message.BorgEntity);
        if (!TryComp<BorgComponent>(borgEntity, out var borg))
            return;

        var emag = EnsureComp<BorgEmagComponent>(borgEntity);
        emag.Emagged = true;
        emag.MindslaveMaster = uid;
        Dirty(borgEntity, emag);
        UpdateUserInterface(uid);
    }

    private void OnMassLockMessage(EntityUid uid, RoboticsConsoleComponent component, RoboticsConsoleMassLockMessage message)
    {
        // SS13: Mass Lock - lockdown all visible borgs
        var query = EntityQueryEnumerator<BorgComponent>();
        while (query.MoveNext(out var borgUid, out var borg))
        {
            if (TryComp<BorgAiLinkComponent>(borgUid, out var aiLink) && aiLink.ScrambledCodes)
                continue;

            borg.Locked = true;
            Dirty(borgUid, borg);
        }
        UpdateUserInterface(uid);
    }

    private void UpdateUserInterface(EntityUid uid)
    {
        var borgs = new List<RoboticsConsoleBorgEntry>();
        var query = EntityQueryEnumerator<BorgComponent, MetaDataComponent>();
        while (query.MoveNext(out var borgUid, out var borg, out var meta))
        {
            // SS13: Filter out scrambledcodes (syndicate borgs hidden from console)
            if (TryComp<BorgAiLinkComponent>(borgUid, out var aiLink) && aiLink.ScrambledCodes)
                continue;

            var status = GetBorgStatus(borgUid, borg);
            var battery = GetBatteryPercent(borgUid);
            var health = GetHealthPercent(borgUid);
            var module = GetModuleName(borgUid);
            var law = GetLawSet(borgUid);
            var radio = HasComp<ActiveRadioComponent>(borgUid);

            borgs.Add(new RoboticsConsoleBorgEntry(
                GetNetEntity(borgUid),
                meta.EntityName,
                status,
                battery,
                health,
                module,
                law,
                radio
            ));
        }

        _uiSystem.SetUiState(uid, RoboticsConsoleUiKey.Key, new RoboticsConsoleState(borgs));
    }

    private string GetBorgStatus(EntityUid uid, BorgComponent borg)
    {
        if (TryComp<BorgRebootComponent>(uid, out var reboot) && reboot.IsRebooting)
            return "Rebooting";
        if (!borg.IsActive)
            return "Disabled";
        if (borg.Locked)
            return "Locked";
        return "Active";
    }

    private float GetBatteryPercent(EntityUid uid)
    {
        if (!TryComp<BorgBatteryComponent>(uid, out var battery) || battery.MaxCharge <= 0)
            return 0f;
        return battery.Charge / battery.MaxCharge;
    }

    private float GetHealthPercent(EntityUid uid)
    {
        var totalDamage = 0f;
        var maxDamage = 0f;
        var partQuery = EntityQueryEnumerator<BorgComponentPartComponent>();
        while (partQuery.MoveNext(out var partUid, out var part))
        {
            if (!EntityManager.EntityExists(partUid))
                continue;
            totalDamage += part.BruteDamage + part.BurnDamage;
            maxDamage += part.MaxDamage;
        }
        if (maxDamage <= 0)
            return 1f;
        return 1f - totalDamage / maxDamage;
    }

    private string GetModuleName(EntityUid uid)
    {
        if (!TryComp<BorgSlotComponent>(uid, out var slots))
            return "None";
        var activeSlot = slots.CurrentSlot;
        if (activeSlot < 0 || activeSlot >= slots.Count)
            return "None";
        var slotItem = slots[activeSlot];
        if (slotItem == EntityUid.Invalid || !EntityManager.EntityExists(slotItem))
            return "Empty";
        return EntityManager.GetComponent<MetaDataComponent>(slotItem).EntityName;
    }

    private string GetLawSet(EntityUid uid)
    {
        if (!TryComp<BorgLawComponent>(uid, out var law))
            return "None";
        return law.LawSet;
    }
}
