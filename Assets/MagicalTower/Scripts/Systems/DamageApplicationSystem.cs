using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(FireballProjectileSystem))]
[UpdateAfter(typeof(BarrageProjectileSystem))]
[UpdateBefore(typeof(BurningSystem))]
[UpdateBefore(typeof(EnemyDeathSystem))]
public partial struct DamageApplicationSystem : ISystem
{
    private const float DamageNumberHeightOffset = 1.4f;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MagicalTowerGameState>();
    }

    public void OnUpdate(ref SystemState state)
    {
        bool isGameOver = SystemAPI.GetSingleton<MagicalTowerGameState>().IsGameOver;
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        var burningLookup = SystemAPI.GetComponentLookup<Burning>(true);
        foreach (var (impact, damage, effects, view, entity) in SystemAPI.Query<RefRO<ProjectileImpact>, RefRO<DamageOnImpact>, DynamicBuffer<ProjectileHitEffectElement>, RefRO<ProjectileView>>().WithEntityAccess())
        {
            if (!isGameOver)
            {
                foreach (var (enemy, transform, enemyEntity) in SystemAPI.Query<RefRW<Enemy>, RefRO<LocalTransform>>().WithEntityAccess())
                {
                    if (enemy.ValueRO.Health <= 0f || math.distance(transform.ValueRO.Position, impact.ValueRO.Position) > damage.ValueRO.Radius + enemy.ValueRO.Radius)
                    {
                        continue;
                    }

                    float health = math.max(0f, enemy.ValueRO.Health - damage.ValueRO.Amount);
                    enemy.ValueRW.Health = health;
                    Entity damageRequest = ecb.CreateEntity();
                    ecb.AddComponent(damageRequest, new DamageNumberRequest { Position = transform.ValueRO.Position + new float3(0f, DamageNumberHeightOffset, 0f), Amount = damage.ValueRO.Amount });
                    if (health <= 0f)
                    {
                        continue;
                    }

                    for (int i = 0; i < effects.Length; i++)
                    {
                        HitEffectData effect = effects[i].Effect;
                        switch (effect.Kind)
                        {
                            case HitEffectKind.Burning:
                                var burn = new Burning { DamagePerSecond = effect.Value, Remaining = effect.Duration, TickInterval = effect.TickInterval, TickTimer = effect.TickInterval };
                                if (burningLookup.HasComponent(enemyEntity))
                                {
                                    ecb.SetComponent(enemyEntity, burn);
                                }
                                else
                                {
                                    ecb.AddComponent(enemyEntity, burn);
                                }
                                break;
                        }
                    }
                }
            }

            if (view.ValueRO.Id != 0)
            {
                Entity release = ecb.CreateEntity();
                ecb.AddComponent(release, new ReleaseViewRequest { ViewId = view.ValueRO.Id, Kind = view.ValueRO.Kind });
            }
            ecb.DestroyEntity(entity);
        }
    }
}
