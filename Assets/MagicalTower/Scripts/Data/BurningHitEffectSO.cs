using UnityEngine;

[CreateAssetMenu(menuName = "Magical Tower/Hit Effects/Burning")]
public sealed class BurningHitEffectSO : HitEffectSO
{
    public float damagePerSecond = 4f;
    public float duration = 3f;
    [Min(0.01f)] public float tickInterval = 0.5f;

    public override HitEffectData ToRuntimeEffect()
    {
        return new HitEffectData
        {
            Kind = HitEffectKind.Burning,
            Value = damagePerSecond,
            Duration = duration,
            TickInterval = tickInterval
        };
    }
}
