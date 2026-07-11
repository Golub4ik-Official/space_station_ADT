using Content.Shared.Interaction;
using Content.Shared.Labels.Components;
using Content.Shared.Labels.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Tools.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Shared.ADT.Labels;

public sealed class PaperLabelCutSystem : EntitySystem
{
    [Dependency] private readonly SharedToolSystem _tool = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly LabelSystem _label = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PaperLabelComponent, InteractUsingEvent>(OnInteractUsing, before: new[] { typeof(LabelSystem) });
    }

    private void OnInteractUsing(Entity<PaperLabelComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!_tool.HasQuality(args.Used, SharedToolSystem.CutQuality))
            return;

        var slot = ent.Comp.LabelSlot;
        if (slot.Item is not { Valid: true } paper)
            return;

        args.Handled = true;

        _popup.PopupClient(Loc.GetString("paper-label-cut", ("entity", ent)), args.User, ent);
        _audio.PlayPredicted(new SoundPathSpecifier("/Audio/Items/wirecutter.ogg"), ent, args.User);

        _transform.DropNextTo(paper, ent.Owner);
        _label.RemoveLabel((ent.Owner, (LabelComponent?)null));
    }
}
