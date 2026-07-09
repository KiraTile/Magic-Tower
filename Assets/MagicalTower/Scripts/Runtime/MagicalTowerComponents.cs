using Unity.Entities;
using Unity.Mathematics;

public struct MagicalTowerGameState : IComponentData
{
    public float ElapsedTime;
    public bool IsGameOver;
    public uint RandomState;
    public float2 SpawnHalfExtents;
    public float SpawnPadding;
    public float EnemySpawnHeight;
    public int MaxEnemies;
}

public struct TowerState : IComponentData
{
    public float3 Position;
    public float Health;
    public float MaxHealth;
    public float Radius;
}

public struct Enemy : IComponentData
{
    public int DefinitionId;
    public float Health;
    public float MaxHealth;
    public float MoveSpeed;
    public float ContactDamage;
    public float Radius;
}

public struct Burning : IComponentData
{
    public float DamagePerSecond;
    public float Remaining;
    public float TickInterval;
    public float TickTimer;
}

public struct EnemyView : IComponentData
{
    public int Id;
}

public struct ProjectileView : IComponentData
{
    public int Id;
    public int Kind;
}

public struct FireballSpellState : IComponentData
{
    public float Cooldown;
    public float Timer;
    public float LaunchHeight;
    public float ProjectileSpeed;
    public float ProjectileRadius;
    public float Gravity;
    public float ProjectileLifetime;
    public float GroundImpactHeight;
    public float ExplosionHeight;
    public float Damage;
    public float ExplosionRadius;
}

public struct BarrageSpellState : IComponentData
{
    public float Cooldown;
    public float Timer;
    public float LaunchHeight;
    public float Damage;
    public float FlightTime;
    public float ArcHeight;
    public float TargetHeightOffset;
    public float ProjectileRadius;
    public float HitRadius;
}

public struct FireballProjectile : IComponentData
{
    public float3 Velocity;
    public float Radius;
    public float Gravity;
    public float Lifetime;
    public float GroundImpactHeight;
    public float ExplosionHeight;
}

public struct BarrageProjectile : IComponentData
{
    public Entity Target;
    public float3 Start;
    public float3 TargetPosition;
    public float TargetHeightOffset;
    public float Elapsed;
    public float Duration;
    public float ArcHeight;
}

public struct DamageOnImpact : IComponentData
{
    public float Amount;
    public float Radius;
}

public struct ProjectileImpact : IComponentData
{
    public float3 Position;
}

public struct SpawnState : IComponentData
{
    public float Timer;
}

public struct EnemyDefinitionElement : IBufferElementData
{
    public int Id;
    public float Health;
    public float MoveSpeed;
    public float ContactDamage;
    public float Radius;
    public float Scale;
    public float SpawnWeight;
}

public struct SpawnPhaseElement : IBufferElementData
{
    public float StartTime;
    public float SpawnInterval;
    public float HealthMultiplier;
    public float SpeedMultiplier;
    public float WeightMultiplier;
}

public enum AttackKind
{
    Fireball,
    Barrage
}

public enum HitEffectKind
{
    Burning
}

public struct HitEffectData
{
    public HitEffectKind Kind;
    public float Value;
    public float Duration;
    public float TickInterval;
}

public struct SpellHitEffectElement : IBufferElementData
{
    public AttackKind AttackKind;
    public HitEffectData Effect;
}

public struct ProjectileHitEffectElement : IBufferElementData
{
    public HitEffectData Effect;
}

public struct SpawnEnemyViewRequest : IComponentData
{
    public Entity Target;
    public int DefinitionId;
    public float3 Position;
    public float Scale;
}

public struct SpawnProjectileViewRequest : IComponentData
{
    public Entity Target;
    public int Kind;
    public float3 Position;
    public float Scale;
}

public struct ReleaseViewRequest : IComponentData
{
    public int ViewId;
    public int Kind;
}

public struct DamageNumberRequest : IComponentData
{
    public float3 Position;
    public float Amount;
}

public struct ExplosionRequest : IComponentData
{
    public float3 Position;
    public float Radius;
}

public static class MagicalTowerViewKind
{
    public const int Enemy = 0;
    public const int Fireball = 1;
    public const int Barrage = 2;
    public const int Explosion = 3;
}
