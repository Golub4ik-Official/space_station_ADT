using Robust.Client.GameObjects;
using Robust.Shared.Timing;
using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.Silicons.Borgs.Components;

namespace Content.Client.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgVisualsSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private static readonly Color EmagTint = Color.Red;
    private static readonly Color NormalTint = Color.White;
    private const float LowPowerThreshold = 0.15f;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgEmagComponent, AfterAutoHandleStateEvent>(OnEmagStateChanged);
        SubscribeLocalEvent<BorgBatteryComponent, AfterAutoHandleStateEvent>(OnBatteryStateChanged);
        SubscribeLocalEvent<BorgRebootComponent, AfterAutoHandleStateEvent>(OnRebootStateChanged);
        SubscribeLocalEvent<BorgSlotComponent, AfterAutoHandleStateEvent>(OnSlotStateChanged);
    }

    private void OnEmagStateChanged(EntityUid uid, BorgEmagComponent component, AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        _sprite.LayerSetColor((uid, sprite), BorgVisualLayers.Body, component.Emagged ? EmagTint : NormalTint);
    }

    private void OnBatteryStateChanged(EntityUid uid, BorgBatteryComponent component, AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        var isLowPower = component.MaxCharge > 0
            && component.Charge / component.MaxCharge < LowPowerThreshold;

        if (isLowPower)
        {
            var blink = (int)(_timing.CurTime.TotalSeconds * 4) % 2 == 0;
            _sprite.LayerSetVisible((uid, sprite), BorgVisualLayers.Light, blink);
            _sprite.LayerSetVisible((uid, sprite), BorgVisualLayers.LightStatus, blink);
            _sprite.LayerSetColor((uid, sprite), BorgVisualLayers.Light, Color.Orange);
        }
    }

    private void OnRebootStateChanged(EntityUid uid, BorgRebootComponent component, AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (component.IsRebooting)
        {
            _sprite.LayerSetColor((uid, sprite), BorgVisualLayers.Body, Color.Gray);
            var blink = (int)(_timing.CurTime.TotalSeconds * 6) % 2 == 0;
            _sprite.LayerSetVisible((uid, sprite), BorgVisualLayers.Light, blink);
        }
    }

    private void OnSlotStateChanged(EntityUid uid, BorgSlotComponent component, AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        var activeSlot = component.CurrentSlot;
        var totalSlots = BorgSlotComponent.MaxSlots;

        var slotHighlight = (float)(activeSlot + 1) / totalSlots;
        var highlightColor = Color.FromSrgb(new Color(slotHighlight, 1f - slotHighlight * 0.5f, 0.2f));

        _sprite.LayerSetColor((uid, sprite), BorgVisualLayers.LightStatus, highlightColor);
    }
}
