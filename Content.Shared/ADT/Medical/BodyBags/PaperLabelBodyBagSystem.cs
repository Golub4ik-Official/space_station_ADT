using Content.Shared.Labels.Components;
using Content.Shared.Labels.EntitySystems;
using Content.Shared.NameModifier.EntitySystems;
using Content.Shared.Paper;
using Content.Shared.Storage.Components;
using Robust.Shared.Containers;

namespace Content.Shared.ADT.Medical.BodyBags;

public sealed class PaperLabelBodyBagSystem : EntitySystem
{
    [Dependency] private readonly LabelSystem _label = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PaperLabelComponent, StorageAfterOpenEvent>(OnStorageOpened);
        SubscribeLocalEvent<PaperLabelComponent, RefreshNameModifiersEvent>(OnPaperLabelRefreshNameModifiers);
    }

    private void OnStorageOpened(Entity<PaperLabelComponent> ent, ref StorageAfterOpenEvent args)
    {
        var slot = ent.Comp.LabelSlot;
        if (slot.Item is not { Valid: true } paper)
            return;

        _transform.DropNextTo(paper, ent.Owner);
        _label.RemoveLabel((ent.Owner, (LabelComponent?)null));
    }

    private void OnPaperLabelRefreshNameModifiers(Entity<PaperLabelComponent> ent, ref RefreshNameModifiersEvent args)
    {
        if (ent.Comp.LabelSlot.Item is not { Valid: true } item)
            return;

        if (!TryComp<PaperComponent>(item, out var paper))
            return;

        var content = paper.Content?.Trim();
        if (string.IsNullOrEmpty(content))
            return;

        args.AddModifier("comp-label-format", extraArgs: ("label", content));
    }
}
