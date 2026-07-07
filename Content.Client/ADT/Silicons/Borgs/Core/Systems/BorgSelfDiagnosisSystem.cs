// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝

using Content.Client.ADT.Silicons.Borgs.Core.UI;
using Content.Shared.ADT.Silicons.Borgs.Core.Components;
using Content.Shared.ADT.Silicons.Borgs.Core.Systems;

namespace Content.Client.ADT.Silicons.Borgs.Core.Systems;

public sealed class BorgSelfDiagnosisSystem : EntitySystem
{
    private BorgSelfDiagnosisWindow? _window;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgComponent, BorgSelfDiagnosisEvent>(OnSelfDiagnosisAction);
    }

    private void OnSelfDiagnosisAction(EntityUid uid, BorgComponent component, BorgSelfDiagnosisEvent args)
    {
        if (_window != null)
        {
            _window.Close();
            _window = null;
            return;
        }

        _window = new BorgSelfDiagnosisWindow();
        _window.UpdateDiagnosis(uid, EntityManager);
        _window.Open();

        _window.OnClose += () => _window = null;
    }
}
