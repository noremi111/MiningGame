using System.Collections.Generic;

public static class ArtifactDatabase
{
    private static readonly Dictionary<ArtifactType, ArtifactData> artifacts =
        new Dictionary<ArtifactType, ArtifactData>
        {
            { ArtifactType.SeismicPick, new ArtifactData(ArtifactType.SeismicPick, "Seismic Pick",
                "Beim Zerstören eines Blocks werden die 4 direkt benachbarten normalen Steine ebenfalls zerstört.") },
            { ArtifactType.LuckyCoin, new ArtifactData(ArtifactType.LuckyCoin, "Lucky Coin",
                "Jeder zerstörte Block hat eine Chance von 5 %, zusätzlich 50 Gold zu geben.") },
            { ArtifactType.CrimsonPickaxe, new ArtifactData(ArtifactType.CrimsonPickaxe, "Crimson Pickaxe",
                "Mining-Schläge haben eine Chance von 20 %, kritisch zu treffen und doppelten Schaden zu verursachen.") },
            { ArtifactType.StoneTax, new ArtifactData(ArtifactType.StoneTax, "Stone Tax",
                "Normale Steine geben beim Abbau zusätzlich 1 Gold.") },
            { ArtifactType.CartographersEye, new ArtifactData(ArtifactType.CartographersEye, "Cartographer's Eye",
                "Alle 60 Sekunden werden die Positionen versteckter Treppen für 5 Sekunden sichtbar.") }
        };

    public static ArtifactData Get(ArtifactType type)
    {
        if (type == ArtifactType.None) return null;
        return artifacts.TryGetValue(type, out ArtifactData data) ? data : null;
    }

    public static IEnumerable<ArtifactData> GetAll() => artifacts.Values;
}
