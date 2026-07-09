using UnityEngine;

[CreateAssetMenu(menuName = "Magical Tower/Barrage Spell")]
public sealed class BarrageSpellConfigSO : ProjectileSpellConfigSO
{
    public float flightTime = 0.75f;
    public float arcHeight = 3.5f;
    [Min(0f)] public float targetHeightOffset = 0.45f;
    public float hitRadius = 0.55f;
}
