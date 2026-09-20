using UnityEngine;

[CreateAssetMenu(menuName = "MiningGame/Building Effects/Maximum Energy")]
public class MaxEnergyBuildingEffect : BuildingEffect
{
    [SerializeField, Min(0f)] private float amount = 50f;
    public override float MaxEnergyBonus => Mathf.Max(0f, amount);
}
