using UnityEngine;

// Immutable configuration only. Per-run state belongs to BuildingEffectManager.
public abstract class BuildingEffect : ScriptableObject
{
    public virtual float MaxEnergyBonus => 0f;
    public virtual void OnNewDepth(PlayerStats stats) { }
}
