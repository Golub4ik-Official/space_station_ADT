using System.Linq;
using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.Silicons.Laws;
using Robust.Shared.Prototypes;

namespace Content.Shared.ADT.Silicons.Borgs.Core.Systems;

public abstract class SharedBorgLawSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgLawComponent, ComponentInit>(OnLawInit);
    }

    private void OnLawInit(EntityUid uid, BorgLawComponent component, ComponentInit args)
    {
        Dirty(uid, component);
    }

    public void SetLawSet(EntityUid uid, ProtoId<SiliconLawsetPrototype> lawSet, BorgLawComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.LawSet = lawSet;
        Dirty(uid, component);
    }

    public void SetLawSync(EntityUid uid, bool enabled, BorgLawComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.LawSyncEnabled = enabled;
        Dirty(uid, component);
    }

    public void SetConnectedAi(EntityUid uid, EntityUid? ai, BorgLawComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.ConnectedAi = ai;
        Dirty(uid, component);
    }

    public void SetZerothLaw(EntityUid uid, string? law, bool isOverride = false, EntityUid? lawBorg = null, BorgLawComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.ZerothLaw = law;
        component.ZerothLawOverride = isOverride;
        component.ZerothLawBorg = lawBorg?.ToString();
        Dirty(uid, component);
    }

    public IReadOnlyList<string> GetLaws(EntityUid uid, BorgLawComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return Array.Empty<string>();

        var laws = new List<string>();

        if (_proto.TryIndex(component.LawSet, out var lawSet))
        {
            if (component.ZerothLaw != null)
                laws.Add(component.ZerothLaw);

            foreach (var lawId in lawSet.Laws)
            {
                if (_proto.TryIndex(lawId, out var lawProto))
                    laws.Add(Loc.GetString(lawProto.LawString));
            }
        }

        return laws;
    }

    public string GetLawDisplay(EntityUid uid, BorgLawComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return string.Empty;

        var laws = GetLaws(uid, component);
        return string.Join("\n", laws.Select((l, i) => $"{i + 1}. {l}"));
    }
}
