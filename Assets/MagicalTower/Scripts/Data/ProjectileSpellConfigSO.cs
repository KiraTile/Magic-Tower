using UnityEngine;

public abstract class ProjectileSpellConfigSO : ScriptableObject
{
    public float cooldown = 1f;
    [Min(0f)] public float initialDelay;
    [Min(0f)] public float launchHeight;
    public float damage = 10f;
    public float projectileRadius = 0.2f;
    public HitEffectSO[] hitEffects;
}
