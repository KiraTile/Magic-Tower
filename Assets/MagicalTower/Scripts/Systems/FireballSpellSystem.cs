using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyMoveSystem))]
public partial struct FireballSpellSystem : ISystem
{
    private const uint FallbackRandomState = 1u;

    private EntityQuery _enemyQuery;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MagicalTowerGameState>();
        state.RequireForUpdate<FireballSpellState>();
        _enemyQuery = SystemAPI.QueryBuilder().WithAll<Enemy, LocalTransform>().Build();
    }

    public void OnUpdate(ref SystemState state)
    {
        var game = SystemAPI.GetSingletonRW<MagicalTowerGameState>();
        if (game.ValueRO.IsGameOver)
        {
            return;
        }

        var spell = SystemAPI.GetSingletonRW<FireballSpellState>();
        spell.ValueRW.Timer -= SystemAPI.Time.DeltaTime;
        if (spell.ValueRO.Timer > 0f)
        {
            return;
        }

        int enemyCount = _enemyQuery.CalculateEntityCount();
        if (enemyCount == 0)
        {
            return;
        }

        spell.ValueRW.Timer = spell.ValueRO.Cooldown;
        var transforms = _enemyQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        var rng = new Random(game.ValueRO.RandomState == 0 ? FallbackRandomState : game.ValueRO.RandomState);
        float3 towerPosition = SystemAPI.GetSingleton<TowerState>().Position + new float3(0f, spell.ValueRO.LaunchHeight, 0f);
        float3 target = transforms[rng.NextInt(0, transforms.Length)].Position;
        game.ValueRW.RandomState = rng.state;

        float3 direction = math.normalizesafe(target - towerPosition, new float3(0f, 0f, 1f));
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        DynamicBuffer<SpellHitEffectElement> spellEffects = SystemAPI.GetSingletonBuffer<SpellHitEffectElement>();
        Entity projectile = ecb.CreateEntity();
        ecb.AddComponent(projectile, LocalTransform.FromPositionRotationScale(towerPosition, quaternion.identity, spell.ValueRO.ProjectileRadius));
        ecb.AddComponent(projectile, new FireballProjectile
        {
            Velocity = direction * spell.ValueRO.ProjectileSpeed,
            Radius = spell.ValueRO.ProjectileRadius,
            Gravity = spell.ValueRO.Gravity,
            Lifetime = spell.ValueRO.ProjectileLifetime,
            GroundImpactHeight = spell.ValueRO.GroundImpactHeight,
            ExplosionHeight = spell.ValueRO.ExplosionHeight
        });
        ecb.AddComponent(projectile, new DamageOnImpact { Amount = spell.ValueRO.Damage, Radius = spell.ValueRO.ExplosionRadius });
        DynamicBuffer<ProjectileHitEffectElement> projectileEffects = ecb.AddBuffer<ProjectileHitEffectElement>(projectile);
        for (int i = 0; i < spellEffects.Length; i++)
        {
            if (spellEffects[i].AttackKind == AttackKind.Fireball)
            {
                projectileEffects.Add(new ProjectileHitEffectElement { Effect = spellEffects[i].Effect });
            }
        }
        ecb.AddComponent(projectile, new ProjectileView { Kind = MagicalTowerViewKind.Fireball });
        Entity request = ecb.CreateEntity();
        ecb.AddComponent(request, new SpawnProjectileViewRequest { Target = projectile, Kind = MagicalTowerViewKind.Fireball, Position = towerPosition, Scale = spell.ValueRO.ProjectileRadius });
    }
}
