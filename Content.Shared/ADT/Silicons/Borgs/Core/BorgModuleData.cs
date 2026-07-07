// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝



namespace Content.Shared.ADT.Silicons.Borgs.Core;

[DataDefinition]
[Serializable]
public partial struct BorgEnergyStorageEntry
{
    [DataField]
    public string Name = string.Empty;

    [DataField]
    public float MaxAmount;

    [DataField]
    public float RechargeRate;

    [DataField]
    public float CurrentAmount;

    public BorgEnergyStorageEntry()
    {
    }
}

[DataDefinition]
[Serializable]
public partial struct BorgArmorEntry
{
    [DataField]
    public int Brute;

    [DataField]
    public int Burn;

    public BorgArmorEntry()
    {
    }

    public readonly BorgArmorEntry Add(BorgArmorEntry other) => new()
    {
        Brute = Brute + other.Brute,
        Burn = Burn + other.Burn
    };
}
