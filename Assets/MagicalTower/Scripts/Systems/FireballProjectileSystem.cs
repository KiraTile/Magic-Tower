using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(FireballSpellSystem))]
public partial struct FireballProjectileSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MagicalTowerGameState>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.GetSingleton<MagicalTowerGameState>().IsGameOver)
        {
            return;
        }

        float dt = SystemAPI.Time.DeltaTime;
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (projectile, impactDamage, transform, entity) in SystemAPI.Query<RefRW<FireballProjectile>, RefRO<DamageOnImpact>, RefRW<LocalTransform>>().WithNone<ProjectileImpact>().WithEntityAccess())
        {
            projectile.ValueRW.Lifetime -= dt;
            projectile.ValueRW.Velocity += new float3(0f, -projectile.ValueRO.Gravity * dt, 0f);
            transform.ValueRW.Position += projectile.ValueRO.Velocity * dt;

            bool hit = transform.ValueRO.Position.y <= projectile.ValueRO.GroundImpactHeight || projectile.ValueRO.Lifetime <= 0f;
            foreach (var (enemy, enemyTransform) in SystemAPI.Query<RefRO<Enemy>, RefRO<LocalTransform>>())
            {
                if (math.distance(enemyTransform.ValueRO.Position, transform.ValueRO.Position) <= enemy.ValueRO.Radius + projectile.ValueRO.Radius)
                {
                    hit = true;
                    break;
                }
            }

            if (!hit)
            {
                continue;
            }

            float3 center = new float3(transform.ValueRO.Position.x, projectile.ValueRO.ExplosionHeight, transform.ValueRO.Position.z);
            Entity explosion = ecb.CreateEntity();
            ecb.AddComponent(explosion, new ExplosionRequest { Position = center, Radius = impactDamage.ValueRO.Radius });
            ecb.AddComponent(entity, new ProjectileImpact { Position = center });
        }
    }
}
