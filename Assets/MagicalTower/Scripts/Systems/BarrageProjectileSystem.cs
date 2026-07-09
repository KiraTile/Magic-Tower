using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(BarrageSpellSystem))]
public partial struct BarrageProjectileSystem : ISystem
{
    private const float MinimumFlightDuration = 0.05f;

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
        foreach (var (projectile, transform, entity) in SystemAPI.Query<RefRW<BarrageProjectile>, RefRW<LocalTransform>>().WithNone<ProjectileImpact>().WithEntityAccess())
        {
            projectile.ValueRW.Elapsed += dt;
            if (SystemAPI.Exists(projectile.ValueRO.Target) && SystemAPI.HasComponent<LocalTransform>(projectile.ValueRO.Target))
            {
                projectile.ValueRW.TargetPosition = SystemAPI.GetComponent<LocalTransform>(projectile.ValueRO.Target).Position + new float3(0f, projectile.ValueRO.TargetHeightOffset, 0f);
            }

            float t = math.saturate(projectile.ValueRO.Elapsed / math.max(MinimumFlightDuration, projectile.ValueRO.Duration));
            float3 position = math.lerp(projectile.ValueRO.Start, projectile.ValueRO.TargetPosition, t);
            position.y += math.sin(t * math.PI) * projectile.ValueRO.ArcHeight;
            transform.ValueRW.Position = position;

            if (t < 1f)
            {
                continue;
            }

            ecb.AddComponent(entity, new ProjectileImpact { Position = projectile.ValueRO.TargetPosition });
        }
    }
}
