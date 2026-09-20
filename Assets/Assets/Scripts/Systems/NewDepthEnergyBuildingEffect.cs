using UnityEngine;

[CreateAssetMenu(menuName = "MiningGame/Building Effects/Energy On New Depth")]
public class NewDepthEnergyBuildingEffect : BuildingEffect
{
    [SerializeField, Min(0f)] private float amount = 10f;
    public override void OnNewDepth(PlayerStats stats)
    {
        if (stats != null) stats.RestoreEnergy(Mathf.Max(0f, amount));
    }
}
