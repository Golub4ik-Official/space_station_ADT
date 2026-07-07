// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Systems;

public abstract class SharedBorgAiLinkSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgAiLinkComponent, ComponentInit>(OnAiLinkInit);
        SubscribeLocalEvent<BorgAiLinkComponent, ComponentShutdown>(OnAiLinkShutdown);
    }

    private void OnAiLinkInit(EntityUid uid, BorgAiLinkComponent component, ComponentInit args)
    {
        _actions.AddAction(uid, ref component.BackToAiActionEntity, component.BackToAiAction);
    }

    private void OnAiLinkShutdown(EntityUid uid, BorgAiLinkComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, component.BackToAiActionEntity);
    }

    public void LinkToAi(EntityUid uid, EntityUid aiEntity, BorgAiLinkComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.LinkedAi = aiEntity;
        component.LawSync = true;
        Dirty(uid, component);
    }

    public void UnlinkFromAi(EntityUid uid, BorgAiLinkComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.LinkedAi = null;
        component.LawSync = false;
        Dirty(uid, component);
    }

    public bool IsLinkedToAi(EntityUid uid, BorgAiLinkComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        return component.LinkedAi != null;
    }
}
