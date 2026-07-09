using UnityEngine;

[CreateAssetMenu(menuName = "Magical Tower/Fireball Spell")]
public sealed class FireballSpellConfigSO : ProjectileSpellConfigSO
{
    public float projectileSpeed = 9f;
    public float gravity = 4f;
    [Min(0f)] public float projectileLifetime = 4f;
    [Min(0f)] public float groundImpactHeight = 0.2f;
    [Min(0f)] public float explosionHeight = 0.45f;
    public float explosionRadius = 2.2f;
}
