using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyMoveSystem))]
public partial struct BarrageSpellSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MagicalTowerGameState>();
        state.RequireForUpdate<BarrageSpellState>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var game = SystemAPI.GetSingleton<MagicalTowerGameState>();
        if (game.IsGameOver)
        {
            return;
        }

        var spell = SystemAPI.GetSingletonRW<BarrageSpellState>();
        spell.ValueRW.Timer -= SystemAPI.Time.DeltaTime;
        if (spell.ValueRO.Timer > 0f)
        {
            return;
        }

        spell.ValueRW.Timer = spell.ValueRO.Cooldown;
        float3 towerPosition = SystemAPI.GetSingleton<TowerState>().Position + new float3(0f, spell.ValueRO.LaunchHeight, 0f);
        float2 halfExtents = game.SpawnHalfExtents;
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        DynamicBuffer<SpellHitEffectElement> spellEffects = SystemAPI.GetSingletonBuffer<SpellHitEffectElement>();
        foreach (var (enemyTransform, enemyEntity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<Enemy>().WithEntityAccess())
        {
            float3 target = enemyTransform.ValueRO.Position;
            if (math.abs(target.x) > halfExtents.x || math.abs(target.z) > halfExtents.y)
            {
                continue;
            }

            Entity projectile = ecb.CreateEntity();
            ecb.AddComponent(projectile, LocalTransform.FromPositionRotationScale(towerPosition, quaternion.identity, spell.ValueRO.ProjectileRadius));
            ecb.AddComponent(projectile, new BarrageProjectile
            {
                Target = enemyEntity,
                Start = towerPosition,
                TargetPosition = target + new float3(0f, spell.ValueRO.TargetHeightOffset, 0f),
                TargetHeightOffset = spell.ValueRO.TargetHeightOffset,
                Duration = spell.ValueRO.FlightTime,
                ArcHeight = spell.ValueRO.ArcHeight
            });
            ecb.AddComponent(projectile, new DamageOnImpact { Amount = spell.ValueRO.Damage, Radius = spell.ValueRO.HitRadius });
            DynamicBuffer<ProjectileHitEffectElement> projectileEffects = ecb.AddBuffer<ProjectileHitEffectElement>(projectile);
            for (int i = 0; i < spellEffects.Length; i++)
            {
                if (spellEffects[i].AttackKind == AttackKind.Barrage)
                {
                    projectileEffects.Add(new ProjectileHitEffectElement { Effect = spellEffects[i].Effect });
                }
            }
            ecb.AddComponent(projectile, new ProjectileView { Kind = MagicalTowerViewKind.Barrage });
            Entity request = ecb.CreateEntity();
            ecb.AddComponent(request, new SpawnProjectileViewRequest { Target = projectile, Kind = MagicalTowerViewKind.Barrage, Position = towerPosition, Scale = spell.ValueRO.ProjectileRadius });
        }
    }
}
